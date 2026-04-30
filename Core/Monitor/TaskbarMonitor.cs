using System.Windows;
using System.Windows.Threading;
using static SplusXBTMeter.Core.Win32Api;

namespace SplusXBTMeter.Core.Monitor
{
    public class TaskbarMonitor : IDisposable
    {
        private IntPtr _regKeyHandle = IntPtr.Zero;
        private IntPtr _trayNotifyWndHookHandle = IntPtr.Zero;
        private WinEventDelegate? _trayNotifyWndEventDelegate;
        private readonly Dispatcher? _dispatcher;
        private RECT _lastTrayNotifyWndRect;

        // 任务栏对齐方式变化事件（0=左，1=中）
        public event Action<int>? TaskbarAlignmentChanged;

        // 🔥 新增：TrayNotifyWnd 区域变化事件
        public event Action<RECT>? TrayNotifyWndChanged;

        public TaskbarMonitor()
        {
            _dispatcher = Application.Current?.Dispatcher;
        }

        public void Start()
        {
            try
            {
                // 打开注册表项
                int result = RegOpenKeyExW(
                    HKEY_CURRENT_USER,
                    @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                    0,
                    KEY_NOTIFY,
                    out _regKeyHandle);

                if (result != 0)
                {
                    Console.WriteLine($"无法打开注册表项: {result}");
                }
                else
                {
                    // 开始监听注册表变化
                    Task.Run(() => MonitorTaskbarAl());

                    // 立即读取一次当前值
                    ReadAndNotifyTaskbarAl();
                }

                // 🔥 新增：开始监听 TrayNotifyWnd 变化
                StartTrayNotifyWndMonitoring();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动监听失败: {ex.Message}");
            }
        }

        private void StartTrayNotifyWndMonitoring()
        {
            try
            {
                _trayNotifyWndEventDelegate = new WinEventDelegate(TrayNotifyWndEventCallback);

                // 监听对象位置变化事件
                _trayNotifyWndHookHandle = SetWinEventHook(
                    EVENT_OBJECT_LOCATIONCHANGE,
                    EVENT_OBJECT_LOCATIONCHANGE,
                    IntPtr.Zero,
                    _trayNotifyWndEventDelegate,
                    0,  // 所有进程
                    0,  // 所有线程
                    WINEVENT_OUTOFCONTEXT | WINEVENT_SKIPOWNPROCESS);

                if (_trayNotifyWndHookHandle == IntPtr.Zero)
                {
                    Console.WriteLine("无法设置 TrayNotifyWnd 监听钩子");
                }
                else
                {
                    // 获取初始位置
                    UpdateTrayNotifyWndRect();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启动 TrayNotifyWnd 监听失败: {ex.Message}");
            }
        }

        private void MonitorTaskbarAl()
        {
            try
            {
                while (_regKeyHandle != IntPtr.Zero)
                {
                    // 监听注册表变化
                    int result = RegNotifyChangeKeyValue(
                        _regKeyHandle,
                        false,
                        REG_NOTIFY_CHANGE_LAST_SET,
                        IntPtr.Zero,
                        false);

                    if (result != 0)
                    {
                        Console.WriteLine($"监听注册表失败: {result}");
                        break;
                    }

                    // 读取并通知变化
                    ReadAndNotifyTaskbarAl();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"监听异常: {ex.Message}");
            }
        }

        private void ReadAndNotifyTaskbarAl()
        {
            try
            {
                object value = Utils.GetRegValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", "");

                _dispatcher?.Invoke(() =>
                {
                    TaskbarAlignmentChanged?.Invoke(Convert.ToInt32(value));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"读取 TaskbarAl 失败: {ex.Message}");
            }
        }

        private void TrayNotifyWndEventCallback(
            IntPtr hWinEventHook,
            uint eventType,
            IntPtr hwnd,
            int idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime)
        {
            // 检查是否是 TrayNotifyWnd 窗口
            IntPtr trayNotifyWndHwnd = FindWindowEx(
                FindWindowW("Shell_TrayWnd", ""),
                IntPtr.Zero,
                "TrayNotifyWnd",
                 "");

            if (hwnd == trayNotifyWndHwnd)
            {
                UpdateTrayNotifyWndRect();
            }
        }

        private void UpdateTrayNotifyWndRect()
        {
            try
            {
                IntPtr trayNotifyWndHwnd = FindWindowEx(
                    FindWindowW("Shell_TrayWnd", ""),
                    IntPtr.Zero,
                    "TrayNotifyWnd",
                     "");

                if (trayNotifyWndHwnd != IntPtr.Zero &&
                    GetWindowRect(trayNotifyWndHwnd, out RECT rect))
                {
                    // 检查是否发生变化
                    if (rect.Left != _lastTrayNotifyWndRect.Left ||
                        rect.Top != _lastTrayNotifyWndRect.Top ||
                        rect.Right != _lastTrayNotifyWndRect.Right ||
                        rect.Bottom != _lastTrayNotifyWndRect.Bottom)
                    {
                        _lastTrayNotifyWndRect = rect;

                        // 在UI线程上触发事件
                        _dispatcher?.Invoke(() =>
                        {
                            TrayNotifyWndChanged?.Invoke(rect);
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新 TrayNotifyWnd 矩形失败: {ex.Message}");
            }
        }


        public void Stop()
        {
            // 停止注册表监听
            if (_regKeyHandle != IntPtr.Zero)
            {
                _ = RegCloseKey(_regKeyHandle);
                _regKeyHandle = IntPtr.Zero;
            }

            // 🔥 停止 TrayNotifyWnd 监听
            if (_trayNotifyWndHookHandle != IntPtr.Zero)
            {
                UnhookWinEvent(_trayNotifyWndHookHandle);
                _trayNotifyWndHookHandle = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}
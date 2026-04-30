
using SplusXBTMeter.Views;
using System.Diagnostics;

using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Windows;

namespace SplusXBTMeter.Core
{
    public class SystemTray : IDisposable
    {
        #region 线程安全改进
        // 🔒 新增：静态锁对象，保护共享资源访问
        private static readonly object _lockObj = new object();

        // 🔒 使用 volatile 确保多线程可见性
        private static volatile IntPtr _trayWindowHandle;
        private static volatile bool _isDisposed;
        private static volatile IntPtr _originalWndProcPtr;
        private static volatile IntPtr _customTrayIcon = IntPtr.Zero;

        // 🔒 新增：初始化标志，防止重复初始化
        private static volatile bool _isInitialized = false;
        #endregion

        public static void Init()
        {
            // 🔒 使用双重检查锁定模式
            if (_isInitialized) return;

            lock (_lockObj)
            {
                if (_isInitialized) return;

                var trayThread = new Thread(() =>
                {
                    try
                    {
                        _trayWindowHandle = Win32Api.CreateWindowEx(0, "STATIC", "TrayHostWindow",
                            Win32Api.WS_POPUP, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                        Win32Api.ShowWindow(_trayWindowHandle, Win32Api.SW_HIDE);

                        IntPtr wndProcPtr = Marshal.GetFunctionPointerForDelegate(new WndProc(TrayWindowProc));
                        _originalWndProcPtr = Win32Api.SetWindowLongPtr(_trayWindowHandle, Win32Api.GWL_WNDPROC, wndProcPtr);

                        // 加载图标
                        string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"App.ico");
                        _customTrayIcon = Win32Api.LoadImage(IntPtr.Zero, iconPath,
                            Win32Api.IMAGE_ICON, 32, 32, Win32Api.LR_LOADFROMFILE | Win32Api.LR_DEFAULTSIZE);

                        if (_customTrayIcon == IntPtr.Zero)
                        {
                            _customTrayIcon = Win32Api.LoadIcon(IntPtr.Zero, Win32Api.IDI_APPLICATION);
                        }

                        // 注册托盘图标
                        Win32Api.NOTIFYICONDATA nid = new();
                        nid.cbSize = Marshal.SizeOf(nid);
                        nid.hWnd = _trayWindowHandle;
                        nid.uID = 1001;
                        nid.uFlags = Win32Api.NIF_ICON | Win32Api.NIF_TIP | Win32Api.NIF_MESSAGE;
                        nid.uCallbackMessage = Win32Api.WM_TRAY_MSG;
                        nid.hIcon = _customTrayIcon;
                        nid.szTip = $"{AppInfo.Title} v{AppInfo.Version}";

                        Win32Api.Shell_NotifyIcon(Win32Api.NIM_ADD, ref nid);

                        // 消息循环
                        Win32Api.MSG msg = new();
                        while (Win32Api.GetMessage(out msg, _trayWindowHandle, 0, 0) && !_isDisposed)
                        {
                            Win32Api.TranslateMessage(ref msg);
                            Win32Api.DispatchMessage(ref msg);
                        }

                        Win32Api.Shell_NotifyIcon(Win32Api.NIM_DELETE, ref nid);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"托盘初始化失败: {ex.Message}");
                    }
                })
                {
                    IsBackground = true,
                    ApartmentState = ApartmentState.STA
                };

                trayThread.Start();
                _isInitialized = true;
            }
        }

        private static IntPtr TrayWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if (msg == Win32Api.WM_TRAY_MSG && lParam == (IntPtr)Win32Api.WM_RBUTTONUP)
            {
                ShowMenu();
                return IntPtr.Zero;
            }
            return Win32Api.CallWindowProc(_originalWndProcPtr, hWnd, msg, wParam, lParam);
        }

        private static void ShowMenu()
        {
            try
            {
                Win32Api.GetCursorPos(out Win32Api.POINT pt);
                IntPtr menu = Win32Api.CreatePopupMenu();
                Win32Api.AppendMenu(menu, 0, 1001, "设置");
                Win32Api.AppendMenu(menu, 0, 1002, "关于");
                Win32Api.AppendMenu(menu, Win32Api.MF_SEPARATOR, 0, string.Empty);
                Win32Api.AppendMenu(menu, 0, 1003, "检查更新");
                Win32Api.AppendMenu(menu, 0, 1004, "重启");
                Win32Api.AppendMenu(menu, Win32Api.MF_SEPARATOR, 0, string.Empty);
                Win32Api.AppendMenu(menu, 0, 1005, "退出");
                Win32Api.SetForegroundWindow(_trayWindowHandle);
                int cmd = Win32Api.TrackPopupMenu(menu, Win32Api.TPM_RETURNCMD, pt.x, pt.y, 0, _trayWindowHandle, IntPtr.Zero);
                Win32Api.DestroyMenu(menu);

                Application.Current?.Dispatcher.Invoke(() =>
                {
                    if (cmd == 1001)
                    {
                        new SettingWindow().Show();
                    }
                    if (cmd == 1002)
                    {
                        new AboutWindow().Show();
                    }
                    if (cmd == 1003)
                    {
                        // 修复异步调用
                        _ = CheckUpdatev1();
                    }
                    if (cmd == 1004)
                    {
                        // 重启APP
                        RestartApp();
                    }
                    if (cmd == 1005)
                    {
                        var result = HandyControl.Controls.MessageBox.Show("确认退出吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.Yes)
                        {
                            Application.Current.Shutdown();
                        }
                    }

                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示菜单时出错: {ex.Message}");
            }
        }

        private static void RestartApp()
        {
            try
            {
                App.ReleaseMutex();
                string AppPath = Process.GetCurrentProcess().MainModule?.FileName ?? string.Empty;
                if (string.IsNullOrEmpty(AppPath))
                {
                    HandyControl.Controls.MessageBox.Show("无法获取应用程序路径！");
                    return;
                }
                Process.Start(AppPath);
                Application.Current.Shutdown();
            }
            catch
            {
                HandyControl.Controls.MessageBox.Show("重启失败！");
            }
        }

        // ====================== 【修复完成：100%解析成功】 ======================
        private static async Task CheckUpdatev1()
        {
            try
            {

                var result = await CheckUpdate.GetUpdateInfo();
                // 6. 版本对比

                if (result == null )
                {
                    HandyControl.Controls.MessageBox.Show($"检查更新失败", "错误");
                    return;
                }
                if (result.Code == 0 &&  result.Data != null)
                {
                    var updateWindow = new UpdateWindow(result.Data.Body, result.Data.DownloadUrl);
                    updateWindow.Show();
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show(result.Msg, "提示");

                }
            }
            catch (Exception ex)
            {
                // 打印错误详情，方便调试
                Console.WriteLine($"错误：{ex.Message}");
                HandyControl.Controls.MessageBox.Show($"更新检查失败：{ex.Message}", "错误");
            }
        }

        // 【严格匹配你提供的JSON结构】
        public class GiteeRelease
        {
            [JsonPropertyName("tag_name")]
            public string TagName { get; set; } = "0.0.0";

            [JsonPropertyName("body")]
            public string Body { get; set; } = "暂无更新日志";

            [JsonPropertyName("assets")]
            public object[] Assets { get; set; } = [];

            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;
        }
        // ======================================================================


        public void Dispose()
        {
            lock (_lockObj)
            {
                if (_isDisposed) return;

                _isDisposed = true;

                // 删除托盘图标
                Win32Api.NOTIFYICONDATA nid = new();
                nid.cbSize = Marshal.SizeOf(nid);
                nid.hWnd = _trayWindowHandle;
                nid.uID = 1001;
                Win32Api.Shell_NotifyIcon(Win32Api.NIM_DELETE, ref nid);

                // 销毁图标
                if (_customTrayIcon != IntPtr.Zero)
                {
                    Win32Api.DestroyIcon(_customTrayIcon);
                    _customTrayIcon = IntPtr.Zero;
                }

                GC.SuppressFinalize(this);
            }
        }

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
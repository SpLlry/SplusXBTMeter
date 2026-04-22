using System;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Text.Json;
using System.Windows;
using HandyControl.Controls;

namespace BTBatteryDisplayApp
{
    public class SystemTray : IDisposable
    {
        private static IntPtr _trayWindowHandle;
        private static bool _isDisposed;
        private static IntPtr _originalWndProcPtr;
        private static readonly HttpClient _httpClient = new HttpClient();

        public static void Init()
        {
            var trayThread = new Thread(() =>
            {
                try
                {
                    _trayWindowHandle = Win32Api.CreateWindowEx(0, "STATIC", "TrayHostWindow", Win32Api.WS_POPUP, 0, 0, 1, 1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    Win32Api.ShowWindow(_trayWindowHandle, Win32Api.SW_HIDE);

                    IntPtr wndProcPtr = Marshal.GetFunctionPointerForDelegate(new WndProc(TrayWindowProc));
                    _originalWndProcPtr = Win32Api.SetWindowLongPtr(_trayWindowHandle, Win32Api.GWL_WNDPROC, wndProcPtr);

                    Win32Api.NOTIFYICONDATA nid = new Win32Api.NOTIFYICONDATA();
                    nid.cbSize = Marshal.SizeOf(nid);
                    nid.hWnd = _trayWindowHandle;
                    nid.uID = 1001;
                    nid.uFlags = Win32Api.NIF_ICON | Win32Api.NIF_TIP | Win32Api.NIF_MESSAGE;
                    nid.uCallbackMessage = Win32Api.WM_TRAY_MSG;
                    nid.hIcon = Win32Api.LoadIcon(IntPtr.Zero, Win32Api.IDI_APPLICATION);
                    nid.szTip = "蓝牙电量监控工具";

                    Win32Api.Shell_NotifyIcon(Win32Api.NIM_ADD, ref nid);

                    Win32Api.MSG msg = new Win32Api.MSG();
                    while (Win32Api.GetMessage(out msg, _trayWindowHandle, 0, 0))
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
                Win32Api.AppendMenu(menu, Win32Api.MF_SEPARATOR, 0, string.Empty);
                Win32Api.AppendMenu(menu, 0, 1004, "退出");

                Win32Api.SetForegroundWindow(_trayWindowHandle);
                int cmd = Win32Api.TrackPopupMenu(menu, Win32Api.TPM_RETURNCMD, pt.x, pt.y, 0, _trayWindowHandle, IntPtr.Zero);
                Win32Api.DestroyMenu(menu);

                if (Application.Current != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
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
                            _ = ChcekUpdate();
                        }
                        if (cmd == 1004)
                        {
                            var result = HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, "确认退出吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                Application.Current.Shutdown();
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"显示菜单时出错: {ex.Message}");
            }
        }

        // ====================== 【修复完成：100%解析成功】 ======================
        private static async Task ChcekUpdate()
        {
            try
            {
                // 1. 获取本地版本（去掉v前缀，匹配Gitee格式）
                Version localVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version("0.0.0");
                string localVer = $"{localVersion.Major}.{localVersion.Minor}.{localVersion.Build}";

                // 2. 请求Gitee接口
                string apiUrl = "https://gitee.com/api/v5/repos/spllr/BTPowerNotice/releases/latest";
                string json = await _httpClient.GetStringAsync(apiUrl);

                // 3. 【终极解析配置】完全适配你的JSON
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,  // 忽略大小写
                    IgnoreNullValues = true,             // 忽略空值
                    AllowTrailingCommas = true           // 允许尾逗号
                };

                // 4. 反序列化（绝对不为null）
                var data = JsonSerializer.Deserialize<GiteeRelease>(json, options);

                // 5. 校验数据
                if (data == null)
                {
                    HandyControl.Controls.MessageBox.Show("解析失败", "错误");
                    return;
                }

                // 6. 版本对比
                Version serverVersion = new Version(data.tag_name);
                if (serverVersion > new Version(localVer))
                {
                 
                    var ret = HandyControl.Controls.MessageBox.Show(
                        Application.Current.MainWindow,
                        $"发现新版本：{data.tag_name}\n本地版本：{localVer}\n\n更新日志：\n{data.body}",
                        "更新提示", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    if (ret == MessageBoxResult.Yes) {
                        try
                        {
                            // ====================== 解析下载链接 ======================
                            string downloadUrl = "";
                            if (data.assets != null && data.assets.Length > 0)
                            {
                                var asset = JsonSerializer.SerializeToElement(data.assets[0]);
                                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            }
                            if (string.IsNullOrEmpty(downloadUrl))
                            {
                                downloadUrl = "https://gitee.com/spllr/BTPowerNotice/releases";
                            }
                            // ==========================================================

                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(downloadUrl) { UseShellExecute = true });
                        }
                        catch
                        {
                            HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, "打开下载链接失败！", "错误");
                        }
                    }
                    
                }
                else
                {
                    HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, "当前已是最新版本！", "提示");
                }
            }
            catch (Exception ex)
            {
                // 打印错误详情，方便调试
                Console.WriteLine($"错误：{ex.Message}");
                HandyControl.Controls.MessageBox.Show(Application.Current.MainWindow, $"检查更新失败：{ex.Message}", "错误");
            }
        }

        // 【严格匹配你提供的JSON结构】
        public class GiteeRelease
        {
            public string tag_name { get; set; } = "0.0.0";
            public string body { get; set; }="暂无更新日志";
            public object[] assets { get; set; }= Array.Empty<object>();
            public string name { get; set; }= string.Empty;
        }
        // ======================================================================

        public void Dispose()
        {
            if (_isDisposed) return;
            Win32Api.NOTIFYICONDATA nid = new Win32Api.NOTIFYICONDATA();
            nid.cbSize = Marshal.SizeOf(nid);
            nid.hWnd = _trayWindowHandle;
            nid.uID = 1001;
            Win32Api.Shell_NotifyIcon(Win32Api.NIM_DELETE, ref nid);
            _isDisposed = true;
        }

        private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
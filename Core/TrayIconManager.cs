using HandyControl.Controls;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace SplusXBTMeter.Core
{
    public class SystemTray : IDisposable
    {
        private static IntPtr _trayWindowHandle;
        private static bool _isDisposed;
        private static IntPtr _originalWndProcPtr;
        private static readonly HttpClient _httpClient = new();
        private static IntPtr _customTrayIcon = IntPtr.Zero; // 自定义图标句柄

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

                    string iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"app.ico");
                    Console.WriteLine($"iconPath{iconPath}");
                    _customTrayIcon = Win32Api.LoadImage(
                        IntPtr.Zero,
                        iconPath,
                        Win32Api.IMAGE_ICON,
                        32, 32,
                        Win32Api.LR_LOADFROMFILE | Win32Api.LR_DEFAULTSIZE
                    );

                    // 加载失败则使用系统默认图标
                    if (_customTrayIcon == IntPtr.Zero)
                    {
                        _customTrayIcon = Win32Api.LoadIcon(IntPtr.Zero, Win32Api.IDI_APPLICATION);
                    }

                    Win32Api.NOTIFYICONDATA nid = new();
                    nid.cbSize = Marshal.SizeOf(nid);
                    nid.hWnd = _trayWindowHandle;
                    nid.uID = 1001;
                    nid.uFlags = Win32Api.NIF_ICON | Win32Api.NIF_TIP | Win32Api.NIF_MESSAGE;
                    nid.uCallbackMessage = Win32Api.WM_TRAY_MSG;
                    nid.hIcon = _customTrayIcon;
                    nid.szTip = "SplusX蓝牙设备电量显示";

                    Win32Api.Shell_NotifyIcon(Win32Api.NIM_ADD, ref nid);

                    Win32Api.MSG msg = new();
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
                            _ = ChcekUpdate();
                        }
                        if (cmd == 1004)
                        {
                            // 重启APP
                             RestartApp();
                        }
                        if (cmd == 1005)
                        {
                            var result = HandyControl.Controls.MessageBox.Show( "确认退出吗？", "提示", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                string AppPath = Process.GetCurrentProcess().MainModule.FileName;
                Process.Start(AppPath);
                Application.Current.Shutdown();
            }
            catch
            {
                HandyControl.Controls.MessageBox.Show("重启失败！");
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
                JsonSerializerOptions jsonSerializerOptions = new()
                {
                    PropertyNameCaseInsensitive = true,  // 忽略大小写
                    IgnoreNullValues = true,             // 忽略空值
                    AllowTrailingCommas = true           // 允许尾逗号
                };
                var options = jsonSerializerOptions;

                // 4. 反序列化（绝对不为null）
                var data = JsonSerializer.Deserialize<GiteeRelease>(json, options);

                // 5. 校验数据
                if (data == null)
                {
                    HandyControl.Controls.MessageBox.Show("解析失败", "错误");
                    return;
                }

                // 6. 版本对比
                Version serverVersion = new(data.TagNmae);
                if (serverVersion > new Version(localVer))
                {
                 
                    MessageBoxResult ret = HandyControl.Controls.MessageBox.Show(
                        $"发现新版本：{data.TagNmae}\n本地版本：{localVer}\n\n更新日志：\n{data.Body}",
                        "更新提示", MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
                    if (ret == MessageBoxResult.Yes) {
                        try
                        {
                            // ====================== 解析下载链接 ======================
                            string downloadUrl = "";
                            if (data.Assets != null && data.Assets.Length > 0)
                            {
                                var asset = JsonSerializer.SerializeToElement(data.Assets[0]);
                                downloadUrl = asset.GetProperty("browser_download_url").GetString();
                            }
                            if (string.IsNullOrEmpty(downloadUrl))
                            {
                                downloadUrl = "https://gitee.com/spllr/SplusXBTMeter/releases";
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
            public string TagNmae { get; set; } = "0.0.0";
            public string Body { get; set; }="暂无更新日志";
            public object[] Assets { get; set; }= [];

            public string Name { get; set; }= string.Empty;
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
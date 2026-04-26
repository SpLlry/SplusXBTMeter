using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;
using System.IO;
namespace SplusXBTMeter.Core
{
    internal class Utils
    {
        public static bool IsWindows11()
             => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);

        public static bool IsWindows10()
            => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240)
            && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
        /// <summary>
        /// 获取WPF窗口的DPI缩放比例
        /// </summary>
        public static double GetDpiScale(Window window)
        {
            if (window == null) return 1.0;
            var hwnd = new WindowInteropHelper(window).Handle;
            var dpi = VisualTreeHelper.GetDpi(window);
            return dpi.DpiScaleX; // 水平缩放比例（1.0 / 1.25 / 1.5 / 2.0）
        }
        public static TaskBarInfo GetTaskBarInfo() {
            IntPtr hTaskbar = Win32Api.FindWindow("Shell_TrayWnd", "");
          
            Console.WriteLine($"找到任务栏主窗口句柄：{hTaskbar}");
            Console.WriteLine(hTaskbar == IntPtr.Zero);
            if (hTaskbar == IntPtr.Zero) return default;
            int TaskBarAlignment = GetTaskbarAlignment();
            Console.WriteLine($"{TaskBarAlignment}");
            IntPtr h1 = hTaskbar;
            if (TaskBarAlignment ==0)
            {
                //靠左对齐  要取TrayNotifyWnd的区域位置 让任务栏显示区域在靠近托盘的地方
                 h1 = Win32Api.FindWindowEx(hTaskbar, IntPtr.Zero, "TrayNotifyWnd", "");
                Console.WriteLine($"找到TrayNotifyWnd：{h1}");
            }  
            
            Win32Api.GetWindowRect(h1, out Win32Api.RECT containerRect);
            TaskBarInfo taskBarInfo= new TaskBarInfo();
            taskBarInfo.hwnd = hTaskbar;
            taskBarInfo.Left = containerRect.Left;
            taskBarInfo.Top = containerRect.Top;
            taskBarInfo.Right = containerRect.Right;
            taskBarInfo.Bottom = containerRect.Bottom;
            return taskBarInfo;
        }

        public struct TaskBarInfo
        {
            public IntPtr hwnd;
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }
        /// <summary>
        /// 获取任务栏对齐方式（0=左对齐，1=居中）
        /// </summary>
        /// <returns>读取到的值</returns>
        public static int GetTaskbarAlignment() {

            string reg_path = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            string reg_key = "TaskbarAl";
            if (IsWindows10()) { 
              return 0; // Windows 10默认左对齐
            }
            object value = GetRegValue(reg_path, reg_key, "0");
            return Convert.ToInt32(value);
        }
        /// <summary>
        /// 通用：Win32API读取注册表值（HKCU分支）
        /// </summary>
        /// <param name="reg_path">注册表子路径</param>
        /// <param name="reg_key">要读取的键名</param>
        /// <param name="default">默认值</param>
        /// <returns>读取到的值</returns>
        public static object GetRegValue(string reg_path, string reg_key, string @default = "")
        {
            IntPtr hKey = IntPtr.Zero;

            try
            {
                // 1. 打开注册表项
                int result = Win32Api.RegOpenKeyExW(Win32Api.HKEY_CURRENT_USER, reg_path, 0, Win32Api.KEY_READ, out hKey);
                if (result != 0)
                {
                    // 打开失败 = 路径不存在
                    return @default;
                }

                // 2. 先获取数据长度
                int dataSize = 0;
                int type;
                result = Win32Api.RegQueryValueExW(hKey, reg_key, 0, out type, null, ref dataSize);
                if (result != 0)
                {
                    // 键名不存在
                    return @default;
                }

                // 3. 读取数据
                byte[] data = new byte[dataSize];
                result = Win32Api.RegQueryValueExW(hKey, reg_key, 0, out type, data, ref dataSize);
                if (result != 0)
                {
                    return @default;
                }

                // 4. 根据类型转换值
                return type switch
                {
                    Win32Api.REG_SZ => Encoding.Unicode.GetString(data).TrimEnd('\0'), // 字符串
                    Win32Api.REG_DWORD => BitConverter.ToInt32(data, 0),              // DWORD数字
                    _ => data,                                               // 其他类型返回字节数组
                };
            }
            catch (Exception e)
            {
                Console.WriteLine($"[读取注册表失败] {reg_key}：{e.Message}");
                return @default;
            }
            finally
            {
                // 无论成败，关闭注册表句柄
                if (hKey != IntPtr.Zero)
                    Win32Api.RegCloseKey(hKey);
            }
        }
        public static IntPtr GetWpfWindowHwnd(Window window)
        {
            return new WindowInteropHelper(window).EnsureHandle();
        }
        // 获取当前系统主题（1: 浅色，0: 深色）
        public static int CheckSystemDarkTheme()
        {
            try
            {
                string reg_path = "Software\\Microsoft\\Windows\\CurrentVersion\\Themes\\Personalize";
                string reg_key = "SystemUsesLightTheme";
                return (int)Utils.GetRegValue(reg_path, reg_key);
            }
            catch
            {
                return 1;
            }
        }
        /// <summary>
        /// 添加到开机启动文件夹
        /// </summary>
        /// <param name="appPath">应用程序完整路径</param>
        /// <param name="appName">应用程序名称（用作快捷方式文件名）</param>
        /// <param name="description">快捷方式描述</param>
        public static void AddStartup(string appPath, string appName, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(appPath) || !System.IO.File.Exists(appPath))
                {
                    throw new ArgumentException("应用程序路径无效或文件不存在");
                }

                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, $"{appName}.lnk");

                CreateShortcut(shortcutPath, appPath, description);
                Console.WriteLine($"已添加到启动文件夹: {shortcutPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"添加到启动文件夹失败: {ex.Message}");
                HandyControl.Controls.MessageBox.Show($"添加到启动文件夹失败: {ex.Message}", "错误");
            }
        }

        /// <summary>
        /// 从开机启动文件夹移除
        /// </summary>
        /// <param name="appName">应用程序名称（用作快捷方式文件名）</param>
        public static void RemoveStartup(string appName)
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, $"{appName}.lnk");
                Console.WriteLine($"从启动文件夹删除: {shortcutPath}");
                if (System.IO.File.Exists(shortcutPath))
                {
                    System.IO.File.Delete(shortcutPath);
                    Console.WriteLine($"已从启动文件夹删除: {shortcutPath}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从启动文件夹删除失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 创建快捷方式
        /// </summary>
        /// <param name="shortcutPath">快捷方式文件路径</param>
        /// <param name="targetPath">目标程序路径</param>
        /// <param name="description">快捷方式描述</param>
        public static void CreateShortcut(string shortcutPath, string targetPath, string description)
        {
            try
            {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    throw new Exception("无法创建 WScript.Shell 对象");
                }

                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);

                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.WindowStyle = 1;
                shortcut.Description = description;
                shortcut.IconLocation = targetPath + ",0";

                shortcut.Save();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"创建快捷方式失败: {ex.Message}");
                throw;
            }
        }
        public static bool IsSelfStart(string appName)
        {
            try
            {
                string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
                string shortcutPath = Path.Combine(startupFolder, $"{appName}.lnk");

                bool exists = System.IO.File.Exists(shortcutPath);

                Console.WriteLine($"开机自启状态: {(exists ? "已启用" : "未启用")}");
                Console.WriteLine($"快捷方式路径: {shortcutPath}");

                return exists;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 检查开机自启状态失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 将WPF窗口嵌入到任务栏
        /// </summary>
        /// <param name="window">要嵌入的WPF窗口</param>
        /// <returns>任务栏容器句柄，如果嵌入失败则返回IntPtr.Zero</returns>
        public static IntPtr EmbedWindowToTaskbar(Window window)
        {
            try
            {
                IntPtr taskbarContainerHwnd = Win32Api.FindWindow("Shell_TrayWnd", "");
                if (taskbarContainerHwnd == IntPtr.Zero)
                {
                    Console.WriteLine("未找到任务栏容器！");
                    return IntPtr.Zero;
                }

                IntPtr wpfHwnd = GetWpfWindowHwnd(window);

                uint style = (uint)Win32Api.GetWindowLongPtr(wpfHwnd, Win32Api.GWL_STYLE);
                style |= Win32Api.WS_CHILD | Win32Api.WS_VISIBLE;
                Win32Api.SetWindowLongPtr(wpfHwnd, Win32Api.GWL_STYLE, (IntPtr)style);

                uint exStyle = (uint)Win32Api.GetWindowLongPtr(wpfHwnd, Win32Api.GWL_EXSTYLE);
                exStyle |= 0x00000020; // WS_EX_TRANSPARENT：透明不拦截鼠标
                exStyle |= 0x00000004; // WS_EX_NOPARENTNOTIFY：不拦截父窗口/系统消息
                Win32Api.SetWindowLongPtr(wpfHwnd, Win32Api.GWL_EXSTYLE, (IntPtr)exStyle);

                Win32Api.SetParent(wpfHwnd, taskbarContainerHwnd);
                AdjustWindowToTaskbar(window, taskbarContainerHwnd);

                return taskbarContainerHwnd;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"嵌入任务栏失败：{ex.Message}");
                return IntPtr.Zero;
            }
        }

        /// <summary>
        /// 调整窗口到任务栏位置
        /// </summary>
        /// <param name="window">WPF窗口</param>
        /// <param name="taskbarContainerHwnd">任务栏容器句柄</param>
        public static void AdjustWindowToTaskbar(Window window, IntPtr taskbarContainerHwnd)
        {
            if (taskbarContainerHwnd == IntPtr.Zero || window == null)
                return;

            try
            {
                Win32Api.GetWindowRect(taskbarContainerHwnd, out Win32Api.RECT containerRect);
                IntPtr wpfHwnd = GetWpfWindowHwnd(window);
                TaskBarInfo t = GetTaskBarInfo();
                int alignment = GetTaskbarAlignment();

                Console.WriteLine($"任务栏位置：Left={t.Left}, Top={t.Top}, Right={t.Right}, Bottom={t.Bottom}");
                Console.WriteLine($"容器位置：Left={window.Width * GetDpiScale(window)}");

                int pos = (int)(window.Width * GetDpiScale(window));
                if (alignment == 1)
                {
                    pos = 0;
                }

                Win32Api.SetWindowPos(
                    wpfHwnd,
                    IntPtr.Zero,
                    t.Left - pos,
                    0,
                    containerRect.Right - containerRect.Left,
                    containerRect.Bottom - containerRect.Top,
                    Win32Api.SWP_NOZORDER | Win32Api.SWP_NOACTIVATE
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"调整窗口位置失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 从任务栏中移除窗口
        /// </summary>
        /// <param name="window">WPF窗口</param>
        public static void RemoveWindowFromTaskbar(Window window)
        {
            if (window == null)
                return;

            try
            {
                IntPtr wpfHwnd = GetWpfWindowHwnd(window);
                Win32Api.SetParent(wpfHwnd, IntPtr.Zero);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"从任务栏移除窗口失败：{ex.Message}");
            }
        }
    }
    
}
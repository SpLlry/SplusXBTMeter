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

namespace SplusXBTMeter.core
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
    }
    
}

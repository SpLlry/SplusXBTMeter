using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace SplusXBTMeter
{
    public partial class App : Application
    {
        // ✅ 关键：在静态构造函数中订阅（最早时机）
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private static Mutex? _appMutex;
        public static MainWindow? MainWindowInstance { get; private set; }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        public static readonly Config Config = new Config(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "config.ini")
        );

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 单实例检测
            bool isFirstInstance;
            var tempMutex = new Mutex(true, "SplusXBTMeterMutex_2026", out isFirstInstance);

            if (!isFirstInstance)
            {
                tempMutex.Close();
                ActivateFirstInstance();
                Shutdown();
                return;
            }

            _appMutex = tempMutex;
            MainWindowInstance = new MainWindow();
            MainWindowInstance.Hide();
        }

        // ✅ 改进的 AssemblyResolve 处理器
        private static Assembly? ResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                // 获取程序集名称（去掉版本等信息）
                var assemblyName = new AssemblyName(args.Name).Name + ".dll";

                // 尝试从 libs 目录加载
                var libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", assemblyName);

                if (File.Exists(libsPath))
                {
                    return Assembly.LoadFrom(libsPath);
                }

                // 如果 libs 目录没有，尝试根目录（备用）
                var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
                if (File.Exists(rootPath))
                {
                    return Assembly.LoadFrom(rootPath);
                }
            }
            catch
            {
                // 忽略异常
            }

            return null;
        }

        private static void ActivateFirstInstance()
        {
            var currentProcess = Process.GetCurrentProcess();
            foreach (var process in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (process.Id != currentProcess.Id && process.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(process.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(process.MainWindowHandle);
                    return;
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _appMutex?.ReleaseMutex();
            _appMutex?.Close();
            base.OnExit(e);
        }

        public static void SetTheme(int theme_mode)
        {
            try
            {
                Current.Resources.MergedDictionaries.Clear();

                string skinUri = theme_mode == 1
                    ? "pack://application:,,,/HandyControl;component/Themes/SkinDefault.xaml"
                    : "pack://application:,,,/HandyControl;component/Themes/SkinDark.xaml";

                Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(skinUri) });
                Current.Resources.MergedDictionaries.Add(
                    new ResourceDictionary
                    {
                        Source = new Uri("pack://application:,,,/HandyControl;component/Themes/Theme.xaml")
                    });
            }
            catch
            {
                // ignored
            }
        }
        public static void ReleaseMutex()
        {
            try
            {
                _appMutex?.ReleaseMutex();
                _appMutex?.Close();
                _appMutex = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"释放互斥锁失败: {ex.Message}");
            }
        }
    }
}
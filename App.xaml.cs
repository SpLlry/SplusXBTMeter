using SplusXBTMeter.Core;
using SplusXBTMeter.DI;
using SplusXBTMeter.Views;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SplusXBTMeter
{
    public partial class App : Application
    {
        static App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        private static Mutex? _appMutex;
        public static MainWindow? MainWindowInstance { get; private set; }

        public static readonly Config Config = new Config(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "config.ini")
        );

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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

            ServiceLocator.Initialize();

            MainWindowInstance = new MainWindow();
            MainWindowInstance.Hide();
            CheckUpdateInfo();
        }
        private static async void CheckUpdateInfo()
        {
            var result = await CheckUpdate.GetUpdateInfo();
            if (result.Code == 0 && result.Data != null)
            {
                var updateWindow = new UpdateWindow(result.Data.Body, result.Data.DownloadUrl);
                updateWindow.Show();
            }
        }
        private static Assembly? ResolveAssembly(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyName = new AssemblyName(args.Name).Name + ".dll";
                var libsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs", assemblyName);

                if (File.Exists(libsPath))
                {
                    return Assembly.LoadFrom(libsPath);
                }

                var rootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName);
                if (File.Exists(rootPath))
                {
                    return Assembly.LoadFrom(rootPath);
                }
            }
            catch
            {
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
                    Win32Api.ShowWindow(process.MainWindowHandle, Win32Api.SW_RESTORE);
                    Win32Api.SetForegroundWindow(process.MainWindowHandle);
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
            }
        }

        public static void ReleaseMutex()
        {
            try
            {
                if (_appMutex != null)
                {
                    bool ownsMutex = false;
                    try
                    {
                        ownsMutex = _appMutex.WaitOne(0);
                        if (ownsMutex)
                        {
                            _appMutex.ReleaseMutex();
                        }
                    }
                    finally
                    {
                        if (!ownsMutex)
                        {
                            // 当前线程不拥有互斥锁，尝试直接关闭
                        }
                        _appMutex.Close();
                        _appMutex = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"释放互斥锁失败: {ex.Message}");
            }
        }
    }
}
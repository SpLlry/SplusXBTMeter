using SplusXBTMeter.Core;
using SplusXBTMeter.DI;
using SplusXBTMeter.Services.Interfaces;
using SplusXBTMeter.ViewModels.Base;
using System.Windows;

namespace SplusXBTMeter.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IBluetoothService _bluetoothService;
        private List<Core.DeviceBatteryInfo>? _bluetoothDevices = new();
        private SettingWindow? _settingWindow;
        private Core.Monitor.TaskbarMonitor? _taskbarMonitor;

        public List<Core.DeviceBatteryInfo>? BluetoothDevices
        {
            get => _bluetoothDevices;
            set => SetProperty(ref _bluetoothDevices, value);
        }

        public RelayCommand ShowSettingsCommand { get; }

        public MainViewModel()
        {
            _bluetoothService = ServiceLocator.Get<IBluetoothService>();
            _bluetoothService.BluetoothDevicesUpdated += OnBluetoothDevicesUpdated;
            ShowSettingsCommand = new RelayCommand(_ => ShowSettings());
        }

        public void InitializeSync()
        {
            _bluetoothService.StartMonitoring();
            SystemTray.Init();
            _taskbarMonitor = new Core.Monitor.TaskbarMonitor();
            _taskbarMonitor.TaskbarAlignmentChanged += OnTaskbarAlignmentChanged;
            _taskbarMonitor.TrayNotifyWndChanged += OnTrayNotifyWndChanged;
            _taskbarMonitor.Start();
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            SyncSystemTheme();
        }

        private void ShowSettings()
        {
            if (_settingWindow == null || !_settingWindow.IsVisible)
            {
                _settingWindow = new SettingWindow();

                // 🔒 新增：窗口关闭时自动清理引用
                _settingWindow.Closed += (sender, e) =>
                {
                    _settingWindow = null;
                    Console.WriteLine("设置窗口已关闭，引用已清理");
                };

                _settingWindow.Show();
            }
            else
            {
                _settingWindow.Activate();
            }
        }

        private void OnBluetoothDevicesUpdated(List<Core.DeviceBatteryInfo>? devices)
        {
            BluetoothDevices = devices;
        }

        private void OnTaskbarAlignmentChanged(int alignment)
        {
            string alignmentText = alignment switch
            {
                0 => "左对齐",
                1 => "居中对齐",
                _ => "未知"
            };

            Console.WriteLine($"✅ 任务栏对齐方式已更改为: {alignmentText} ({alignment})");
            EventBus.Publish(new TaskbarAlignmentChangedEvent(alignment));
        }

        private void OnTrayNotifyWndChanged(Win32Api.RECT rect)
        {
            Console.WriteLine($"TrayNotifyWnd 区域变化: ({rect.Left},{rect.Top})-({rect.Right},{rect.Bottom})");
            EventBus.Publish(new TaskbarAlignmentChangedEvent(Utils.GetTaskbarAlignment()));
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.WindowResizeBorderThickness))
            {
                Application.Current.Dispatcher.Invoke(SyncSystemTheme);
            }
        }

        private void SyncSystemTheme()
        {
            int isSystemDarkTheme = Utils.CheckSystemDarkTheme();
            App.SetTheme(isSystemDarkTheme);
            Console.WriteLine(isSystemDarkTheme == 1 ? "✅ 浅色模式" : "✅ 深色模式");
        }

        public void Cleanup()
        {
            _bluetoothService.StopMonitoring();
            _bluetoothService.BluetoothDevicesUpdated -= OnBluetoothDevicesUpdated;
            _taskbarMonitor?.Dispose();
            // 🔒 确保窗口被正确关闭
            if (_settingWindow != null)
            {
                _settingWindow.Closed -= (sender, e) => _settingWindow = null;
                _settingWindow.Close();
                _settingWindow = null;
            }
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
        }
    }
}
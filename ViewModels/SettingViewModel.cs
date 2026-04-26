using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using SplusXBTMeter.ViewModels.Base;
using SplusXBTMeter.core;

namespace SplusXBTMeter.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private ObservableCollection<DeviceBatteryInfo>? _bluetoothDevices = new ObservableCollection<DeviceBatteryInfo>();
        private string _selectedSkin = "Default";
        private bool _isStartup;
        private bool _isShowTaskBar;
        private bool _isShowMain;

        public ObservableCollection<DeviceBatteryInfo>? BluetoothDevices
        {
            get => _bluetoothDevices;
            set => SetProperty(ref _bluetoothDevices, value ?? new ObservableCollection<DeviceBatteryInfo>());
        }

        public List<string> SkinList { get; } = ["Default", "Wave"];

        public string SelectedSkin
        {
            get => _selectedSkin;
            set
            {
                if (SetProperty(ref _selectedSkin, value))
                {
                    App.Config.setVal("settings", "skin", value);
                    Console.WriteLine($"皮肤已更改：{_selectedSkin} -> {value}");
                    EventBus.Publish(new SkinChangedEvent(_selectedSkin, value));
                }
            }
        }

        public bool IsStartup
        {
            get => _isStartup;
            set
            {
                if (SetProperty(ref _isStartup, value))
                {
                    App.Config.setVal("Settings", "Startup", value ? "1" : "0");
                    if (value)
                    {
                        string AppPath = Process.GetCurrentProcess().MainModule.FileName;
                        Console.WriteLine($"{AppPath}");
                        Utils.AddStartup(AppPath, "SplusXBTMeter", "SplusX蓝牙设备电量显示");
                    }
                    else
                    {
                        Console.WriteLine($"删除开机");
                        Utils.RemoveStartup("SplusXBTMeter");
                    }
                }
            }
        }

        public bool IsShowTaskBar
        {
            get => _isShowTaskBar;
            set
            {
                if (SetProperty(ref _isShowTaskBar, value))
                {
                    App.Config.setVal("Settings", "TaskBarWindow", value ? "1" : "0");
                }
            }
        }

        public bool IsShowMain
        {
            get => _isShowMain;
            set
            {
                if (SetProperty(ref _isShowMain, value))
                {
                    App.Config.setVal("Settings", "MainWindow", value ? "1" : "0");
                }
            }
        }

        public SettingViewModel()
        {
            EventBus.Subscribe<DeviceListUpdatedEvent>(OnDeviceListUpdated);
        }

        public void LoadSettings()
        {
            IsStartup = Utils.IsSelfStart("SplusXBTMeter");
            IsShowTaskBar = App.Config.getVal("Settings", "TaskBarWindow", "0") == "1";
            IsShowMain = App.Config.getVal("Settings", "MainWindow", "0") == "1";
            SelectedSkin = App.Config.getVal("settings", "skin", "Default");

            var latestEvent = EventBus.GetLatest<DeviceListUpdatedEvent>();
            if (latestEvent != null)
            {
                OnDeviceListUpdated(latestEvent);
            }
        }

        private void OnDeviceListUpdated(DeviceListUpdatedEvent e)
        {
            if (e == null) return;

            Application.Current?.Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"设置窗口收到设备更新，共 {e.Devices.Count} 个设备");
                List<DeviceBatteryInfo> devices = e.Devices;
                BluetoothDevices?.Clear();
                if (devices != null)
                {
                    foreach (var dev in devices)
                    {
                        BluetoothDevices?.Add(dev);
                    }
                }

                var firstDev = BluetoothDevices?.FirstOrDefault();
                Console.WriteLine(
                    $"【任务栏】更新：{(firstDev != null ? $"{firstDev.Name} - {firstDev.Battery}%" : "无设备")}"
                );
            });
        }

        public void EditDevice(DeviceBatteryInfo device)
        {
            var cloneDevice = device.Clone();
            var editWindow = new DeviceDiyWindow(cloneDevice);
            if (editWindow.ShowDialog() == true)
            {
                App.Config.setVal("CustomDeviceName", cloneDevice.Mac, cloneDevice.Name);
                App.Config.setVal(
                    "CustomDeviceShow",
                    cloneDevice.Mac,
                    cloneDevice.IsShow ? "1" : "0"
                );
            }
        }
    }
}
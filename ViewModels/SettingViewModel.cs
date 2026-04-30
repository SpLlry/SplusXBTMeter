using HandyControl.Controls;
using SplusXBTMeter.Core;
using SplusXBTMeter.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace SplusXBTMeter.ViewModels
{
    public class SettingViewModel : ViewModelBase
    {
        private ObservableCollection<Core.DeviceBatteryInfo>? _bluetoothDevices = [];
        private string _selectedSkin = "Default";
        private bool _isStartup;
        private bool _isShowTaskBar;
        private bool _isShowMain;
        private bool _isShowNoConn;

        public ObservableCollection<Core.DeviceBatteryInfo>? BluetoothDevices
        {
            get => _bluetoothDevices;
            set => SetProperty(ref _bluetoothDevices, value: value ?? []);
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
                    Growl.Clear();
                    Growl.Success("修改任务栏样式成功");
                    Console.WriteLine($"皮肤已更改：{_selectedSkin} -> {value}");
                    EventBus.Publish(new SkinChangedEvent(_selectedSkin, value));
                }
            }
        }
        public bool IsShowNoConn
        {
            get
            {
                _isShowNoConn = App.Config.getVal("settings", "ShowNoConn", "1") == "1";
                return _isShowNoConn;
            }
            set
            {
                if (SetProperty(ref _isShowNoConn, value))
                {
                    App.Config.setVal("settings", "ShowNoConn", value ? "1" : "0");
                }
            }
        }
        public bool IsStartup
        {
            get
            {
                _isStartup = Utils.IsSelfStart("SplusXBTMeter"); ;
                return _isStartup;
            }
            set
            {
                if (SetProperty(ref _isStartup, value))
                {
                    App.Config.setVal("Settings", "Startup", value ? "1" : "0");
                    Growl.Clear();
                    if (value)
                    {

                        Growl.Success("添加开机启动成功");
                        string AppPath = Process.GetCurrentProcess().MainModule.FileName;
                        Console.WriteLine($"{AppPath}");
                        Utils.AddStartup(AppPath, "SplusXBTMeter", "SplusX蓝牙设备电量显示");
                    }
                    else
                    {
                        Growl.Success("删除开机启动成功");
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
            SelectedSkin = App.Config.getVal("settings", "skin", "Default") ?? "";

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
                List<Core.DeviceBatteryInfo> devices = e.Devices;
                BluetoothDevices?.Clear();
                if (devices != null)
                {
                    foreach (var dev in devices)
                    {
                        //dev.ConnText = "⛓‍💥";
                        BluetoothDevices?.Add(dev);
                    }
                }

                var firstDev = BluetoothDevices?.FirstOrDefault();
                Console.WriteLine(
                    $"【任务栏】更新：{(firstDev != null ? $"{firstDev.Name} - {firstDev.Battery}%" : "无设备")}"
                );
            });
        }

        public void EditDevice(Core.DeviceBatteryInfo device)
        {
            var cloneDevice = device.Clone();
            var editWindow = new DeviceDiyWindow(cloneDevice);
            if (editWindow.ShowDialog() == true)
            {
                Growl.Clear();
                Growl.Success("修改设备信息成功");
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
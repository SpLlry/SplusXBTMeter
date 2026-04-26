#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using SplusXBTMeter;
using SplusXBTMeter.core;

namespace SplusXBTMeter
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        // 🔥 ObservableCollection（WPF自动刷新必备）
        private ObservableCollection<DeviceBatteryInfo>? _bluetoothDevices =
            new ObservableCollection<DeviceBatteryInfo>();

        public List<string> SkinList { get; set; } = ["Default", "Wave"];

        // 🔥 新增：SelectedSkin 属性
        private string _selectedSkin = "Default";
        public string SelectedSkin
        {
            get => _selectedSkin;
            set
            {
                if (_selectedSkin != value)
                {
                    string oldSkin = _selectedSkin;
                    _selectedSkin = value;
                    OnPropertyChanged();

                    // 当属性变化时，更新配置并触发事件
                    if (oldSkin != value)
                    {
                        App.Config.setVal("settings", "skin", value);
                        Console.WriteLine($"皮肤已更改：{oldSkin} -> {value}");
                        // 触发全局事件
                        EventBus.Publish(new SkinChangedEvent(oldSkin, value));
                    }
                }
            }
        }

        public ObservableCollection<DeviceBatteryInfo>? BluetoothDevices
        {
            get => _bluetoothDevices;
            set
            {
                _bluetoothDevices = value ?? new ObservableCollection<DeviceBatteryInfo>();
                OnPropertyChanged();
            }
        }

        public SettingWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Loaded_;
            Closed += Closed_;
        }

        private void Loaded_(object? sender, RoutedEventArgs e)
        {
            try
            {
                IsStartup.IsChecked = Utils.IsSelfStart("SplusXBTMeter");
                IsShowTaskBar.IsChecked =
                    App.Config.getVal("Settings", "TaskBarWindow", "0") == "1";
                IsShowMain.IsChecked = App.Config.getVal("Settings", "MainWindow", "0") == "1";

                // 🔥 设置 SelectedSkin 的初始值
                string skin = App.Config.getVal("settings", "skin", "Default");
                SelectedSkin = skin;

                // 🔥 核心修复：主动拉取最新的缓存数据（补发历史数据）
               // var latestData = EventBus.GetLatest<DeviceListUpdatedEvent>();
                OnDeviceListUpdated(EventBus.GetLatest<DeviceListUpdatedEvent>());
                //订阅设备更新事件
                EventBus.Subscribe<DeviceListUpdatedEvent>(OnDeviceListUpdated);
            }
            catch (Exception ex)
            {
                // 完全还原你原版的 HandyControl MessageBox
                HandyControl.Controls.MessageBox.Show($"失败：{ex.Message}");
                Close();
            }
        }
        private void OnDeviceListUpdated(DeviceListUpdatedEvent e)
        {
            Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"设置窗口收到设备更新，共 {e.Devices.Count} 个设备");
                List<DeviceBatteryInfo> devices = e.Devices;
                // 更新任务栏显示
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
        private void Closed_(object? sender, EventArgs e)
        {
           
        }


        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 🔥 现在可以通过 SelectedSkin 属性获取选中的值
            Console.WriteLine($"选中的Skin: {SelectedSkin}");
        }

        private void IsStartup_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Console.WriteLine("启用开机自启");
            App.Config.setVal("Settings", "Startup", IsStartup.IsChecked == true ? "1" : "0");
            if (IsStartup.IsChecked == true)
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

        private void IsShowTaskBar_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Config.setVal(
                "Settings",
                "TaskBarWindow",
                IsShowTaskBar.IsChecked == true ? "1" : "0"
            );
        }

        private void IsShowMain_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Config.setVal("Settings", "MainWindow", IsShowMain.IsChecked == true ? "1" : "0");
        }

        // 🔥 双击列表项弹出编辑窗口
        private void ListBox_MouseDoubleClick(
            object sender,
            System.Windows.Input.MouseButtonEventArgs e
        )
        {
            var listBox = sender as ListBox;
            // 获取双击的设备对象
            var device = listBox?.SelectedItem as DeviceBatteryInfo;
            if (device == null)
                return;
            var cloneDevice = device.Clone();
            // 弹出编辑窗口
            var editWindow = new DeviceDiyWindow(cloneDevice);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {
                // 保存成功
                App.Config.setVal("CustomDeviceName", cloneDevice.Mac, cloneDevice.Name);
                App.Config.setVal(
                    "CustomDeviceShow",
                    cloneDevice.Mac,
                    cloneDevice.IsShow ? "1" : "0"
                );
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
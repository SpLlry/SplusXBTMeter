#nullable enable
using HandyControl.Controls;
using SplusXBTMeter;
using SplusXBTMeter.core;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace SplusXBTMeter
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : HandyControl.Controls.Window
    {
        // 🔥 ObservableCollection（WPF自动刷新必备）
        private ObservableCollection<DeviceBatteryInfo>? _bluetoothDevices = new ObservableCollection<DeviceBatteryInfo>();
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
                IsShowTaskBar.IsChecked = App.Config.getVal("Settings", "TaskBarWindow", "0") == "1";
                IsShowMain.IsChecked = App.Config.getVal("Settings", "MainWindow", "0") == "1";
                // 🔥 核心修复：主动拉取最新的缓存数据（补发历史数据）
                // 无论窗口何时打开，都能立即拿到数据
                OnBluetoothDevicesUpdated(MainWindow.LatestBluetoothDevices);
                MainWindow.BluetoothDevicesUpdated += OnBluetoothDevicesUpdated;
                string skin = App.Config.getVal("settings", "skin", "Default");

                foreach (ComboBoxItem item in SkinComboBox.Items)
                {
                    if (item.Tag.ToString() == skin)
                    {
                        SkinComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                // 完全还原你原版的 HandyControl MessageBox
                HandyControl.Controls.MessageBox.Show($"失败：{ex.Message}");
                Close();
            }
        }
        private void Closed_(object? sender, EventArgs e)
        {
            MainWindow.BluetoothDevicesUpdated -= OnBluetoothDevicesUpdated;
        }
        private void TabControl_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HandyControl.Controls.ComboBox comboBox = sender as HandyControl.Controls.ComboBox;
            if (comboBox == null) return;

            // 获取选中的 ComboBoxItem
            ComboBoxItem selectedItem = comboBox.SelectedItem as ComboBoxItem;
            if (selectedItem != null)
            {
                // 获取 Tag 属性（ID）
                string Skin = Convert.ToString(selectedItem.Tag); ;
                Console.WriteLine($"选中的Skin: {Skin}");
                if (Skin != App.Config.getVal("settings", "skin", "Default")) {
                    App.Config.setVal("settings", "skin", Skin);
                    HandyControl.Controls.MessageBox.Show("已切换任务栏样式,重启后生效","提示");
                }
               
                // 根据 ID 执行不同操作
                switch (Skin)
                {
                    case "Default":
                        Console.WriteLine("选择了圆环模式");
                        break;
                    case "Wave":
                        Console.WriteLine("选择了波纹模式");
                        break;
                }
            }
        }


        private void IsStartup_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Console.WriteLine("启用开机自启");
            App.Config.setVal("Settings", "Startup", IsStartup.IsChecked ==true ? "1" : "0");
            if (IsStartup.IsChecked == true)
            {
                string AppPath = Process.GetCurrentProcess().MainModule.FileName;
                Console.WriteLine($"{AppPath}");
                Utils.AddStartup(AppPath, "SplusXBTMeter", "SplusX蓝牙设备电量显示");
            }
            else {
                Console.WriteLine($"删除开机");
                Utils.RemoveStartup("SplusXBTMeter");
            }
        }

        private void IsShowTaskBar_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Config.setVal("Settings", "TaskBarWindow", IsShowTaskBar.IsChecked == true ? "1" : "0");
        }

        private void IsShowMain_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            App.Config.setVal("Settings", "MainWindow", IsShowMain.IsChecked == true ? "1" : "0");
        }
        // 🔥 双击列表项弹出编辑窗口
        private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            // 获取双击的设备对象
            var device = listBox?.SelectedItem as DeviceBatteryInfo;
            if (device == null) return;
            var cloneDevice = device.Clone();
            // 弹出编辑窗口
            var editWindow = new DeviceDiyWindow(cloneDevice);
            editWindow.Owner = this;
            if (editWindow.ShowDialog() == true)
            {

                // 保存成功
                App.Config.setVal("CustomDeviceName", cloneDevice.Mac, cloneDevice.Name);
                App.Config.setVal("CustomDeviceShow", cloneDevice.Mac, cloneDevice.IsShow ? "1" : "0");
            }
        }
        private void OnBluetoothDevicesUpdated(List<DeviceBatteryInfo>? devices)
        {
            Dispatcher.Invoke(() =>
            {
                BluetoothDevices?.Clear();
                if (devices != null)
                {
                    foreach (var dev in devices)
                    {
                        BluetoothDevices?.Add(dev);
                    }
                }

                // 完全保留你原版的调试日志
                var firstDev = BluetoothDevices?.FirstOrDefault();
                Console.WriteLine(
                    $"【任务栏】更新：{(firstDev != null ? $"{firstDev.Name} - {firstDev.Battery}%" : "无设备")}");
            });
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

      
    }
}

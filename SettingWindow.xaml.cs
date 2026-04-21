#nullable enable
using BtBatteryDisplayApp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace BTBatteryDisplayApp
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
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
                IsStartup.IsChecked = App.Config.getVal("Settings", "Startup", "0") == "1";
                IsShowTaskBar.IsChecked = App.Config.getVal("Settings", "TaskBarWindow", "0") == "1";
                IsShowMain.IsChecked = App.Config.getVal("Settings", "MainWindow", "0") == "1";
                // 🔥 核心修复：主动拉取最新的缓存数据（补发历史数据）
                // 无论窗口何时打开，都能立即拿到数据
                OnBluetoothDevicesUpdated(MainWindow.LatestBluetoothDevices);
                MainWindow.BluetoothDevicesUpdated += OnBluetoothDevicesUpdated;

            }
            catch (Exception ex)
            {
                // 完全还原你原版的 HandyControl MessageBox
                MessageBox.Show($"失败：{ex.Message}");
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



        private void IsStartup_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Console.WriteLine("启用开机自启");
            App.Config.setVal("Settings", "Startup", IsStartup.IsChecked == true ? "1" : "0");
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

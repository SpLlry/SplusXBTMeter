#nullable enable
using SplusXBTMeter;
using SplusXBTMeter.core;
using HandyControl; // 🔥 修复：添加根命名空间（解决 ApplicationTheme 找不到）
using HandyControl.Controls;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace SplusXBTMeter
{
    public partial class MainWindow : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        public static List<DeviceBatteryInfo>? LatestBluetoothDevices { get; private set; }
        private TaskBarWindow? taskBarWindow;
        private SettingWindow? settingWindow;
        private readonly System.Timers.Timer _btScanTimer;
        private readonly BtScan _btScan;
        private int _isSystemDarkTheme;

        private List<DeviceBatteryInfo>? _bluetoothDevices = new();
        public List<DeviceBatteryInfo>? BluetoothDevices => _bluetoothDevices;

        public static event Action<List<DeviceBatteryInfo>?>? BluetoothDevicesUpdated;

        public MainWindow()
        {
            
            InitializeComponent();

         
            Closed += MainWindow_Closed;

            _btScan = new BtScan
            {
                UseMockData = false
            };
            _btScanTimer = new System.Timers.Timer(3000);
            _btScanTimer.Elapsed += async (s, e) => await UpdateBluetoothDataAsync();
            StartInit();
            SystemTray.Init();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingWindow = new SettingWindow();
            settingWindow.Show();
        }

        private async void StartInit()
        {
            Console.WriteLine("窗口子啊u人");
            taskBarWindow = new TaskBarWindow();
            taskBarWindow.Show();

            // 监听系统主题切换
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
            SyncSystemTheme();

            _btScanTimer.Start();
            await UpdateBluetoothDataAsync();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _btScanTimer.Stop();
            _btScanTimer.Dispose();
            taskBarWindow?.Close();
            settingWindow?.Close();

            // 释放事件
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
        }

        // 系统主题变更监听
        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SystemParameters.WindowResizeBorderThickness))
            {
                Dispatcher.Invoke(SyncSystemTheme);
            }
        }

        // 同步系统深浅色（HC 3.5.1 专用）
        private void SyncSystemTheme()
        {
            _isSystemDarkTheme = Utils.CheckSystemDarkTheme();

            // 动态切换 HandyControl 主题
            App.SetTheme(_isSystemDarkTheme);
            Console.WriteLine(_isSystemDarkTheme==1 ? "✅ 浅色模式" : "✅ 深色模式");
        }

      

        private async Task UpdateBluetoothDataAsync()
        {
            try
            {
                var newDevices = await _btScan.GetAllBluetoothDevicesBatteryAsync();
                for (int i = 0; i < newDevices.Count; i++)
                {
                    var Mac = newDevices[i].Mac;
                    newDevices[i].IsShow = !(App.Config.getVal("CustomDeviceShow", Mac, "1") == "0");
                    newDevices[i].Name = App.Config.getVal("CustomDeviceName", Mac, newDevices[i].Name);
                }
                Dispatcher.Invoke(() =>
                {
                    bool isDataChanged = !IsDeviceListEqual(_bluetoothDevices, newDevices);
                    if (!isDataChanged) return;

                    _bluetoothDevices = newDevices;
                    LatestBluetoothDevices = newDevices;
                    OnPropertyChanged(nameof(BluetoothDevices));
                    BluetoothDevicesUpdated?.Invoke(_bluetoothDevices);
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    _bluetoothDevices = new List<DeviceBatteryInfo>();
                    BluetoothDevicesUpdated?.Invoke(_bluetoothDevices);
                    Console.WriteLine($"❌ 扫描异常：{ex.Message}");
                });
            }
        }

        private bool IsDeviceListEqual(List<DeviceBatteryInfo>? oldList, List<DeviceBatteryInfo>? newList)
        {
            if (oldList == null && newList == null) return true;
            if (oldList == null || newList == null) return false;
            if (oldList.Count != newList.Count) return false;

            for (int i = 0; i < oldList.Count; i++)
            {
                if (oldList[i].Name != newList[i].Name || oldList[i].Battery != newList[i].Battery || oldList[i].IsShow != newList[i].IsShow)
                    return false;
            }
            return true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Growl.Clear();
            Growl.Error("操作失败，网络连接异常");
        }
    }
}
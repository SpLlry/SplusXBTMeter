#nullable enable
using BtBatteryDisplayApp;
using HandyControl.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

namespace BTBatteryDisplayApp
{
    public partial class MainWindow : HandyControl.Controls.Window, INotifyPropertyChanged
    {
       
        public static List<DeviceBatteryInfo>? LatestBluetoothDevices { get; private set; } // 🔥 新增：静态缓存最新设备数据（全局可访问，解决后打开窗口拿不到数据）
        private TaskBarWindow? taskBarWindow;
        private SettingWindow? settingWindow;
        private TrayWindow? trayWindow;
        private readonly System.Timers.Timer _btScanTimer;
        private readonly BtScan _btScan;

        // 🔥 修复1：改用私有字段存储，手动控制事件发布（解决List不触发set的问题）
        private List<DeviceBatteryInfo>? _bluetoothDevices = new();
        // 🔥 移除public set，禁止外部赋值，完全由内部控制发布
        public List<DeviceBatteryInfo>? BluetoothDevices => _bluetoothDevices;

        // 静态发布事件（保留你的订阅方式，不变）
        public static event Action<List<DeviceBatteryInfo>?>? BluetoothDevicesUpdated;

        public MainWindow()
        {

            SystemTray.Init();
            //trayWindow = new TrayWindow();
            //trayWindow.Show();
            InitializeComponent();
           
            Loaded += MainWindow_Loaded;
            Closed += MainWindow_Closed;

            _btScan = new BtScan();
            _btScan.UseMockData = true;
            _btScanTimer = new System.Timers.Timer(3000);
            _btScanTimer.Elapsed += async (s, e) => await UpdateBluetoothDataAsync();
            //

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingWindow = new SettingWindow();
            settingWindow.Show();
        }
        private async void MainWindow_Loaded(object? sender, RoutedEventArgs e)
        {
            // 先让 TrayWindow 托盘完全初始化，UI线程空闲后再打开窗口
            //await Task.Delay(500);
            await Task.Delay(500);

            taskBarWindow = new TaskBarWindow();

            taskBarWindow.Show();


            _btScanTimer.Start();
          await UpdateBluetoothDataAsync();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _btScanTimer.Stop();
            _btScanTimer.Dispose();
            taskBarWindow?.Close();
            settingWindow?.Close();
            trayWindow?.Close();
        }

        // 🔥 核心修复2：异步更新 + 智能发布（仅数据变化时发布）
        private async Task UpdateBluetoothDataAsync()
        {
            try
            {
                var newDevices = await _btScan.GetAllBluetoothDevicesBatteryAsync();
                for (int i = 0; i < newDevices.Count; i++)
                {
                    var Mac= newDevices[i].Mac;
                    Console.WriteLine($"fff{newDevices[i].Name}");
                    newDevices[i].IsShow = App.Config.getVal("CustomDeviceShow", Mac, "1") == "1";
                    newDevices[i].Name = App.Config.getVal("CustomDeviceName", Mac, newDevices[i].Name);
                    Console.WriteLine($"newDevicesL{JsonSerializer.Serialize(newDevices[i])},{newDevices[i].Name}");
                }
                Dispatcher.Invoke(() =>
                {
                    // 🔥 关键：判断数据是否变化，无变化则不发布（解决无效发布）
                    bool isDataChanged = !IsDeviceListEqual(_bluetoothDevices, newDevices);
                    if (!isDataChanged) return;

                    // 更新数据
                    _bluetoothDevices = newDevices;
                    // 🔥 新增：更新静态缓存
                    LatestBluetoothDevices = newDevices;
                    OnPropertyChanged(nameof(BluetoothDevices));
                    // 🔥 手动发布事件（彻底解决不触发事件的问题）
                    BluetoothDevicesUpdated?.Invoke(_bluetoothDevices);

                    Console.WriteLine($"✅ 设备更新发布：{JsonSerializer.Serialize(_bluetoothDevices)}");
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

        // 🔥 工具方法：判断两个设备列表是否相同（避免无效发布）
        private bool IsDeviceListEqual(List<DeviceBatteryInfo>? oldList, List<DeviceBatteryInfo>? newList)
        {
            if (oldList == null && newList == null) return true;
            if (oldList == null || newList == null) return false;
            if (oldList.Count != newList.Count) return false;

            // 根据设备名称/电量判断是否相同（可根据你的需求调整）
            for (int i = 0; i < oldList.Count; i++)
            {
                if (oldList[i].Name != newList[i].Name || oldList[i].Battery != newList[i].Battery)
                    return false;
            }
            return true;
        }

        // 属性通知（保留）
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
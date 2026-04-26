#nullable enable
using HandyControl; // 🔥 修复：添加根命名空间（解决 ApplicationTheme 找不到）
using HandyControl.Controls;
using Microsoft.Win32;
using SplusXBTMeter;
using SplusXBTMeter.core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Threading;
using static Win32Api;

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
        private TaskbarMonitor? _taskbarMonitor;


        private List<DeviceBatteryInfo>? _bluetoothDevices = new();
        public List<DeviceBatteryInfo>? BluetoothDevices => _bluetoothDevices;

        public static event Action<List<DeviceBatteryInfo>?>? BluetoothDevicesUpdated;

        public MainWindow()
        {
            
            InitializeComponent();

         
            Closed += MainWindow_Closed;

            _btScan = new BtScan
            {
                UseMockData = true
            };
            _btScanTimer = new System.Timers.Timer(3000);
            _btScanTimer.Elapsed += async (s, e) => await UpdateBluetoothDataAsync();
            StartInit();
            SystemTray.Init();
            // 启动任务栏监听
            _taskbarMonitor = new TaskbarMonitor();
            _taskbarMonitor.TaskbarAlignmentChanged += OnTaskbarAlignmentChanged;
            _taskbarMonitor.TrayNotifyWndChanged += OnTrayNotifyWndChanged;  // 🔥 新增
            _taskbarMonitor.Start();
        }
        private void OnTrayNotifyWndChanged(RECT rect)  // 🔥 新增
        {
            Console.WriteLine($"TrayNotifyWnd 区域变化: ({rect.Left},{rect.Top})-({rect.Right},{rect.Bottom})");
            if (taskBarWindow != null) { 
             EventBus.Publish(new TaskbarAlignmentChangedEvent(Utils.GetTaskbarAlignment()));
            }
            // 根据 TrayNotifyWnd 的新位置调整窗口
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

            // 在这里处理任务栏对齐方式变化
            // 例如：调整你的任务栏窗口位置
            if (taskBarWindow != null)
            {
                // 根据对齐方式调整窗口位置
                // AdjustTaskbarWindowPosition(alignment);
                EventBus.Publish(new TaskbarAlignmentChangedEvent(alignment));
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingWindow = new SettingWindow();
            settingWindow.Show();
        }

        private async void StartInit()
        {
            Console.WriteLine("窗口载入");
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

                    // 发布设备更新事件
                    EventBus.Publish(new DeviceListUpdatedEvent(newDevices));
                });
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() =>
                {
                    _bluetoothDevices = new List<DeviceBatteryInfo>();
                    BluetoothDevicesUpdated?.Invoke(_bluetoothDevices);
                    Console.WriteLine($"❌ 扫描异常：{ex.Message}");

                    // 也可以发布一个空列表事件，表示设备更新失败
                    EventBus.Publish(new DeviceListUpdatedEvent(new List<DeviceBatteryInfo>()));
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
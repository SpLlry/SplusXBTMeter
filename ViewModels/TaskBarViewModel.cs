using HandyControl.Controls;
using SplusXBTMeter.Core;
using SplusXBTMeter.ViewModels.Base;
using System.Collections.ObjectModel;
using System.Windows;
using static System.Windows.Forms.AxHost;

namespace SplusXBTMeter.ViewModels
{
    public class TaskBarViewModel : ViewModelBase
    {
        private ObservableCollection<Core.DeviceBatteryInfo>? _bluetoothDevices = new ObservableCollection<Core.DeviceBatteryInfo>();
        private HorizontalAlignment _taskBarAlignment = HorizontalAlignment.Right;
        private bool _isWaveEffect;

        public event Action? TaskbarAlignmentChanged;

        public ObservableCollection<Core.DeviceBatteryInfo>? BluetoothDevices
        {
            get => _bluetoothDevices;
            set => SetProperty(ref _bluetoothDevices, value ?? new ObservableCollection<Core.DeviceBatteryInfo>());
        }

        public HorizontalAlignment TaskBarAlignment
        {
            get => _taskBarAlignment;
            set => SetProperty(ref _taskBarAlignment, value);
        }

        public bool IsWaveEffect
        {
            get => _isWaveEffect;
            set => SetProperty(ref _isWaveEffect, value);
        }

        public TaskBarViewModel()
        {
            LoadWaveConfig();
            EventBus.Subscribe<SkinChangedEvent>(OnSkinChanged);
            EventBus.Subscribe<TaskbarAlignmentChangedEvent>(OnTaskbarAlignmentChanged);
            EventBus.Subscribe<DeviceListUpdatedEvent>(OnDeviceListUpdated);
        }

        public void LoadWaveConfig()
        {
            try
            {
                string? skin = App.Config.getVal("settings", "skin", "Default");
                Console.WriteLine($"skin:{skin}");
                IsWaveEffect = skin == "Wave";

            }
            catch
            {
                IsWaveEffect = false;
            }
        }

        private void OnSkinChanged(SkinChangedEvent e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"收到皮肤更改：{e.OldSkin} -> {e.NewSkin}");
                LoadWaveConfig();
            });
        }

        private void OnTaskbarAlignmentChanged(TaskbarAlignmentChangedEvent e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TaskBarAlignment = e.Alignment == 0 ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                Console.WriteLine($"任务栏窗口收到调整任务栏位置，对齐方式: {e.Alignment}{TaskBarAlignment}");
                TaskbarAlignmentChanged?.Invoke();
            });
        }

        private void OnDeviceListUpdated(DeviceListUpdatedEvent e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"任务栏窗口收到设备更新，共 {e.Devices.Count} 个设备");
                List<Core.DeviceBatteryInfo> devices = e.Devices;

                // 当前界面的旧设备数据（对比基准）
                List<Core.DeviceBatteryInfo> oldDevices = BluetoothDevices?.ToList() ?? new List<Core.DeviceBatteryInfo>();
                if (BluetoothDevices != null && App.Config.getVal("Settings", "ShowDevChangeNotice", "1")=="1") { 
                
                

                // ====================== 核心对比逻辑 ======================
                foreach (var newDevice in devices)
                {
                    // 通过蓝牙MAC地址唯一匹配设备（最精准）
                    var oldDevice = oldDevices.FirstOrDefault(d => d.Mac == newDevice.Mac);

                    // 情况1：旧列表没有该设备 → 新增设备
                    if (oldDevice == null)
                    {
                        Console.WriteLine($"📱 【新增设备】{newDevice.Name} | 状态：{(newDevice.IsConnected ? "已连接" : "未连接")} | 电量：{newDevice.Battery}%");
                        continue;
                    }

                    // 情况2：连接状态发生变化
                    if (oldDevice.IsConnected != newDevice.IsConnected)
                    {
                        string state = newDevice.IsConnected ? "🔗 已连接" : "⛓‍💥 已断开";
                        Console.WriteLine($"🔌 【状态变更】{newDevice.Name} | {state}");
                            if (newDevice.IsConnected) {
                                Growl.SuccessGlobal($"{newDevice.Name}-{state}");
                            } else {
                                Growl.ErrorGlobal($"{newDevice.Name}-{state}");

                            }
                      
                    }

                    // 情况3：电量发生变化
                    if (oldDevice.Battery != newDevice.Battery)
                    {
                        if (newDevice.Battery <= 20) {
                                Growl.InfoGlobal($"{newDevice.Name}-电量低于20%");
                            }
                        Console.WriteLine($"🔋 【电量变更】{newDevice.Name} | {oldDevice.Battery}% → {newDevice.Battery}%");
                    }
                }
                }
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
    }
}
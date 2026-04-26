using System.Collections.ObjectModel;
using System.Windows;
using SplusXBTMeter.ViewModels.Base;
using SplusXBTMeter.core;

namespace SplusXBTMeter.ViewModels
{
    public class TaskBarViewModel : ViewModelBase
    {
        private ObservableCollection<DeviceBatteryInfo>? _bluetoothDevices = new ObservableCollection<DeviceBatteryInfo>();
        private HorizontalAlignment _taskBarAlignment = HorizontalAlignment.Right;
        private bool _isWaveEffect;

        public event Action? TaskbarAlignmentChanged;

        public ObservableCollection<DeviceBatteryInfo>? BluetoothDevices
        {
            get => _bluetoothDevices;
            set => SetProperty(ref _bluetoothDevices, value ?? new ObservableCollection<DeviceBatteryInfo>());
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
    }
}
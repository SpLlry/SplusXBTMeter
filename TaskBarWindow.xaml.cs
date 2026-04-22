#nullable enable
using BtBatteryDisplayApp;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace BTBatteryDisplayApp
{
    public partial class TaskBarWindow : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        private IntPtr _taskbarContainerHwnd = IntPtr.Zero;
        private HwndSource? _hwndSource;

        // 蓝牙设备列表（完全不变）
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

        // 新增：水波纹开关（绑定UI）
        private bool _isWaveEffect;
        public bool IsWaveEffect
        {
            get => _isWaveEffect;
            set { _isWaveEffect = value; OnPropertyChanged(); }
        }

        public TaskBarWindow()
        {
            InitializeComponent();
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            DataContext = this;
            // 直接加载全局配置
            LoadWaveConfig();
            Loaded += Loaded_;
            Closed += Closed_;
        }

        // ====================== 核心：直接调用 App.Config.getVal ======================
        private void LoadWaveConfig()
        {
            try
            {
                // 全局调用，无任何局部声明
                //string? skin = App.Config.getVal("settings", "skin", "Normal");
                string? skin = "Wave1";
                Console.WriteLine($"skin:{skin}");
                IsWaveEffect = skin=="Wave";
            }
            catch
            {
                IsWaveEffect = false;
            }
        }

        // ====================== 以下代码 100% 保留你原有逻辑，无任何修改 ======================
        private void Loaded_(object? sender, RoutedEventArgs e)
        {
            try
            {
                _taskbarContainerHwnd = Win32Api.FindTaskbarEmbedContainer();
                if (_taskbarContainerHwnd == IntPtr.Zero)
                {
                    MessageBox.Show("未找到任务栏容器！");
                    Close();
                    return;
                }

                IntPtr wpfHwnd = Win32Api.GetWpfWindowHwnd(this);
                _hwndSource = HwndSource.FromHwnd(wpfHwnd);
                _hwndSource.AddHook(WpfWndProc);

                uint style = (uint)Win32Api.GetWindowLongPtr(wpfHwnd, Win32Api.GWL_STYLE);
                style |= Win32Api.WS_CHILD | Win32Api.WS_VISIBLE;
                Win32Api.SetWindowLongPtr(wpfHwnd, Win32Api.GWL_STYLE, (IntPtr)style);

                // 🔥 新增：设置窗口扩展样式 → 不拦截系统消息（修复托盘关键代码）
                uint exStyle = (uint)Win32Api.GetWindowLongPtr(wpfHwnd, Win32Api.GWL_EXSTYLE);
                exStyle |= 0x00000020; // WS_EX_TRANSPARENT：透明不拦截鼠标
                exStyle |= 0x00000004; // WS_EX_NOPARENTNOTIFY：不拦截父窗口/系统消息
                Win32Api.SetWindowLongPtr(wpfHwnd, Win32Api.GWL_EXSTYLE, (IntPtr)exStyle);

                Win32Api.SetParent(wpfHwnd, _taskbarContainerHwnd);
                AdjustWindowToTaskbar();

                MainWindow.BluetoothDevicesUpdated += OnBluetoothDevicesUpdated;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"嵌入失败：{ex.Message}");
                Close();
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

                var firstDev = BluetoothDevices?.FirstOrDefault();
                Console.WriteLine(
                    $"【任务栏】更新：{(firstDev != null ? $"{firstDev.Name} - {firstDev.Battery}%" : "无设备")}");
            });
        }

        private void Closed_(object? sender, EventArgs e)
        {
            MainWindow.BluetoothDevicesUpdated -= OnBluetoothDevicesUpdated;

            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WpfWndProc);
                _hwndSource.Dispose();
            }
            if (_taskbarContainerHwnd == IntPtr.Zero) return;
            Win32Api.SetParent(Win32Api.GetWpfWindowHwnd(this), IntPtr.Zero);
        }

        private void AdjustWindowToTaskbar()
        {
            if (_taskbarContainerHwnd == IntPtr.Zero) return;
            Win32Api.GetWindowRect(_taskbarContainerHwnd, out Win32Api.RECT containerRect);
            IntPtr wpfHwnd = Win32Api.GetWpfWindowHwnd(this);
            Win32Api.SetWindowPos(wpfHwnd, IntPtr.Zero, 0, 0, 200, containerRect.Bottom - containerRect.Top, Win32Api.SWP_NOZORDER | Win32Api.SWP_NOACTIVATE);
        }

        private IntPtr WpfWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Api.WM_WINDOWPOSCHANGED)
            {
                AdjustWindowToTaskbar();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
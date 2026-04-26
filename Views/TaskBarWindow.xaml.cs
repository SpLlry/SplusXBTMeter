#nullable enable
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using SplusXBTMeter;
using SplusXBTMeter.core;
using SplusXBTMeter.ViewModels;

namespace SplusXBTMeter
{
    public partial class TaskBarWindow : HandyControl.Controls.Window
    {
        private IntPtr _taskbarContainerHwnd = IntPtr.Zero;
        private HwndSource? _hwndSource;
        private TaskBarViewModel? _viewModel;

        public TaskBarWindow()
        {
            InitializeComponent();
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            _viewModel = new TaskBarViewModel();
            DataContext = _viewModel;
            _viewModel.TaskbarAlignmentChanged += OnTaskbarAlignmentChanged;
            Loaded += Loaded_;
            Closed += Closed_;
        }

        private void Loaded_(object? sender, RoutedEventArgs e)
        {
            try
            {
                _taskbarContainerHwnd = Utils.EmbedWindowToTaskbar(this);
                if (_taskbarContainerHwnd == IntPtr.Zero)
                {
                    MessageBox.Show("未找到任务栏容器！");
                    Close();
                    return;
                }

                IntPtr wpfHwnd = Utils.GetWpfWindowHwnd(this);
                _hwndSource = HwndSource.FromHwnd(wpfHwnd);
                _hwndSource.AddHook(WpfWndProc);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"嵌入失败：{ex.Message}");
                Close();
            }
        }

        private void Closed_(object? sender, EventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.TaskbarAlignmentChanged -= OnTaskbarAlignmentChanged;
            }
            if (_hwndSource != null)
            {
                _hwndSource.RemoveHook(WpfWndProc);
                _hwndSource.Dispose();
            }
            if (_taskbarContainerHwnd != IntPtr.Zero)
            {
                Utils.RemoveWindowFromTaskbar(this);
            }
        }

        private void OnTaskbarAlignmentChanged()
        {
            Dispatcher.Invoke(() =>
            {
                Utils.AdjustWindowToTaskbar(this, _taskbarContainerHwnd);
            });
        }

        private IntPtr WpfWndProc(
            IntPtr hwnd,
            int msg,
            IntPtr wParam,
            IntPtr lParam,
            ref bool handled
        )
        {
            if (msg == Win32Api.WM_WINDOWPOSCHANGED)
            {
                Utils.AdjustWindowToTaskbar(this, _taskbarContainerHwnd);
                handled = true;
            }
            return IntPtr.Zero;
        }
    }
}
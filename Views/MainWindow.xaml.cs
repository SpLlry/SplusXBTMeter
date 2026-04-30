#nullable enable
using HandyControl.Controls;
using SplusXBTMeter.DI;
using SplusXBTMeter.ViewModels;
using System.Windows;

namespace SplusXBTMeter
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        private TaskBarWindow? taskBarWindow;
        private readonly MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = ServiceLocator.Create<MainViewModel>();
            DataContext = _viewModel;

            _viewModel.InitializeSync();
            TaskBarWindowInit();

            Closed += MainWindow_Closed;
        }

        private void TaskBarWindowInit()
        {
            taskBarWindow = new TaskBarWindow();
            taskBarWindow.Show();
        }

        private void MainWindow_Closed(object? sender, EventArgs e)
        {
            _viewModel?.Cleanup();
            taskBarWindow?.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var settingWindow = new SettingWindow();
            settingWindow.Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Growl.Clear();
            Growl.Error("操作失败，网络连接异常");
        }
    }
}
using System.Diagnostics;
using System.Windows;


namespace SplusXBTMeter.Views
{
    /// <summary>
    /// UpdateWindow.xaml 的交互逻辑
    /// </summary>

    public partial class UpdateWindow : HandyControl.Controls.Window
    {
        private readonly UpdateViewModel? _viewModel;
        public UpdateWindow(string updateText, string updateUrl)
        {
            InitializeComponent();
            _viewModel = new UpdateViewModel
            {
                UpdateText = updateText,
                UpdateUrl = updateUrl
            };
            this.DataContext = _viewModel;
        }

        private void Button_Click_Jump(object sender, RoutedEventArgs e)
        {
            string url = _viewModel?.UpdateUrl ?? "https://example.com"; // 从 ViewModel 获取或直接指定
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true // 允许系统选择默认浏览器
            });
        }

        private void Button_Click_Close(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
    }
}

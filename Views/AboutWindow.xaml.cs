using SplusXBTMeter.ViewModels;
using System.Windows;
using System.Windows.Navigation;

namespace SplusXBTMeter
{
    public partial class AboutWindow : HandyControl.Controls.Window
    {
        public AboutWindow()
        {

            InitializeComponent();
            DataContext = new AboutViewModel();
        }

        // 关闭按钮
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }
    }
}
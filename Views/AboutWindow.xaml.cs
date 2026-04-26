using System.Windows;
using SplusXBTMeter.ViewModels;

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
    }
}
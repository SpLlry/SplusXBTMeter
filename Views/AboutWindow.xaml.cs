using SplusXBTMeter.ViewModels;
using System.Reflection;
using System.Windows;

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
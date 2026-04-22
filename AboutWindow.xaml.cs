using System.Windows;


namespace BTBatteryDisplayApp
{
    public partial class AboutWindow : HandyControl.Controls.Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        // 关闭按钮
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            //this.Close();
        }
    }
}
#nullable enable
using SplusXBTMeter;
using System.Windows;

namespace SplusXBTMeter
{
    public partial class DeviceDiyWindow : HandyControl.Controls.Window
    {
        // 接收要编辑的设备对象
        private readonly DeviceBatteryInfo _device;

        public DeviceDiyWindow(DeviceBatteryInfo device)
        {
            InitializeComponent();
            _device = device;

            // 初始化数据
            Name.Text = device.Name;
            Address.Text = device.Mac;
            IsShowDevice.IsChecked = device.IsShow;
        }

        // 保存
        private void Btn_Save(object sender, RoutedEventArgs e)
        {
            _device.Name = Name.Text.Trim();
            _device.Mac = Address.Text.Trim();
            _device.IsShow = IsShowDevice.IsChecked == true;
            
            Console.WriteLine($"【编辑设备】保存：{_device.Name} - {_device.Mac} - {_device.IsShow}");
            DialogResult = true;
            Close();
        }

        // 取消
        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

      
    }
}
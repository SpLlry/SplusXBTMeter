#nullable enable
using SplusXBTMeter;
using SplusXBTMeter.ViewModels;
using System.Windows;

namespace SplusXBTMeter
{
    public partial class DeviceDiyWindow : HandyControl.Controls.Window
    {
        private readonly DeviceDiyViewModel _viewModel;

        public DeviceDiyWindow(DeviceBatteryInfo device)
        {
            InitializeComponent();
            _viewModel = new DeviceDiyViewModel(device);
            DataContext = _viewModel;
        }

        private void Btn_Save(object sender, RoutedEventArgs e)
        {
            _viewModel.Save();
            DialogResult = true;
            Close();
        }

        private void Btn_Cancel(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
using SplusXBTMeter.ViewModels.Base;

namespace SplusXBTMeter.ViewModels
{
    public class DeviceDiyViewModel(Core.DeviceBatteryInfo device) : ViewModelBase
    {
        private readonly Core.DeviceBatteryInfo _device = device;
        private string _name = device.Name ?? string.Empty;
        private string _address = device.Mac ?? string.Empty;
        private bool _isShow = device.IsShow;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public bool IsShow
        {
            get => _isShow;
            set => SetProperty(ref _isShow, value);
        }

        public void Save()
        {
            _device.Name = Name.Trim();
            _device.Mac = Address.Trim();
            _device.IsShow = IsShow;

            Console.WriteLine($"【编辑设备】保存：{_device.Name} - {_device.Mac} - {_device.IsShow}");
        }
    }
}
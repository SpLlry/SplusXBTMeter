using SplusXBTMeter.ViewModels.Base;

namespace SplusXBTMeter.ViewModels
{
    public class DeviceDiyViewModel : ViewModelBase
    {
        private readonly Core.DeviceBatteryInfo _device;
        private string _name;
        private string _address;
        private bool _isShow;

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

        public DeviceDiyViewModel(Core.DeviceBatteryInfo device)
        {
            _device = device;
            _name = device.Name ?? string.Empty;
            _address = device.Mac ?? string.Empty;
            _isShow = device.IsShow;
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
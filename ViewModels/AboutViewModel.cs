using System.Reflection;
using SplusXBTMeter.ViewModels.Base;

namespace SplusXBTMeter.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        private string _verson = "";
        public string AppName { get; } = "SplusX蓝牙设备电量显示";
        public string Version
        {
            get => _verson;
            set => SetProperty(ref _verson, value);
        }
        public string Author { get; } = "SplusX";
        public string Description { get; } = "一款用于显示蓝牙设备电量的工具";

        public AboutViewModel()
        {
            Version localVersion =
                Assembly.GetExecutingAssembly().GetName().Version ?? new Version("0.0.0");
            Console.WriteLine($"ASD{Assembly.GetExecutingAssembly().GetName().FullName}");
            Version = $"{localVersion.Major}.{localVersion.Minor}.{localVersion.Build}";
        }
    }
}

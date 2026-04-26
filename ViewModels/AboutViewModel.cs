using SplusXBTMeter.ViewModels.Base;

namespace SplusXBTMeter.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string AppName { get; } = "SplusX蓝牙设备电量显示";
        public string Version { get; } = "1.0.0";
        public string Author { get; } = "SplusX";
        public string Description { get; } = "一款用于显示蓝牙设备电量的工具";
    }
}
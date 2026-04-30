using SplusXBTMeter.Core;
using SplusXBTMeter.ViewModels.Base;
using System.Windows.Navigation;

namespace SplusXBTMeter.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        public string AppName { get; } = AppInfo.Title;
        public string Version { get; } = AppInfo.Version;
        public string Author { get; } = AppInfo.Author;
        public string Description { get; } = AppInfo.Description;

        public string GiteeUrl { get; } = AppInfo.GiteeUrl;

        public string GitHubUrl { get; } = AppInfo.GitHubUrl;


    }

}

using System.IO;

namespace BTBatteryDisplayApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public static readonly Config Config = new Config(
          Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "config.ini")
      );
    }


}

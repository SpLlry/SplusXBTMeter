using SplusXBTMeter.Core;
using SplusXBTMeter.Services.Interfaces;

namespace SplusXBTMeter.Services
{
    public class BluetoothService : IBluetoothService, IDisposable
    {
        private readonly Core.Bluetooth.BtScan _btScan;
        private readonly System.Timers.Timer _scanTimer;

        public event Action<List<Core.DeviceBatteryInfo>?>? BluetoothDevicesUpdated;

        public BluetoothService()
        {
            _btScan = new Core.Bluetooth.BtScan
            {
                UseMockData = false
            };
            _scanTimer = new System.Timers.Timer(3000);
            _scanTimer.Elapsed += async (s, e) => await UpdateBluetoothDataAsync();

        }

        public async Task<List<Core.DeviceBatteryInfo>> GetAllBluetoothDevicesBatteryAsync()
        {
            return await _btScan.GetAllBluetoothDevicesBatteryAsync();
        }

        public void StartMonitoring()
        {
            // 👇 加上这一行：立即执行一次
            _ = UpdateBluetoothDataAsync();
            _scanTimer.Start();
        }

        public void StopMonitoring()
        {
            _scanTimer.Stop();
        }

        private async Task UpdateBluetoothDataAsync()
        {
            try
            {
                var newDevices = await _btScan.GetAllBluetoothDevicesBatteryAsync();
                bool IsShowNoconn = App.Config.getVal("settings", "ShowNoConn", "1") == "1";
                for (int i = 0; i < newDevices.Count; i++)
                {

                    string Mac = newDevices[i].Mac;
                    string Name = newDevices[i].Name;
                    bool IsConnected = newDevices[i].IsConnected;
                    //div设备显示状态
                    bool CustomDeviceShow = !(App.Config.getVal("CustomDeviceShow", Mac, "1") == "0");


                    newDevices[i].IsShow = CustomDeviceShow && (IsShowNoconn && !IsConnected) || CustomDeviceShow && (IsConnected);
                    newDevices[i].Name = App.Config.getVal("CustomDeviceName", Mac, Name);
                    Console.WriteLine($"{newDevices[i].Name}-{Name}");
                }

                EventBus.Publish(new DeviceListUpdatedEvent(newDevices));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 扫描异常：{ex.Message}");
                EventBus.Publish(new DeviceListUpdatedEvent(new List<Core.DeviceBatteryInfo>()));
            }
        }

        public void Dispose()
        {
            _scanTimer.Stop();
            _scanTimer.Dispose();
        }
    }
}
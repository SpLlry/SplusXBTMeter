using System.Collections.Generic;
using System.Threading.Tasks;

namespace SplusXBTMeter.Services.Interfaces
{
    public interface IBluetoothService
    {
        Task<List<DeviceBatteryInfo>> GetAllBluetoothDevicesBatteryAsync();
        void StartMonitoring();
        void StopMonitoring();
        event Action<List<DeviceBatteryInfo>?>? BluetoothDevicesUpdated;
    }
}
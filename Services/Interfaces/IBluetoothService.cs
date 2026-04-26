using System.Collections.Generic;
using System.Threading.Tasks;

namespace SplusXBTMeter.Services.Interfaces
{
    public interface IBluetoothService
    {
        Task<List<Core.DeviceBatteryInfo>> GetAllBluetoothDevicesBatteryAsync();
        void StartMonitoring();
        void StopMonitoring();
        event Action<List<Core.DeviceBatteryInfo>?>? BluetoothDevicesUpdated;
    }
}
using InTheHand.Bluetooth;
using SplusXBTMeter.Core;
using SplusXBTMeter.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                UseMockData = true
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
                for (int i = 0; i < newDevices.Count; i++)
                {
                    var Mac = newDevices[i].Mac;
                    newDevices[i].IsShow = !(App.Config.getVal("CustomDeviceShow", Mac, "1") == "0");
                    newDevices[i].Name = App.Config.getVal("CustomDeviceName", Mac, newDevices[i].Name);
                }

                BluetoothDevicesUpdated?.Invoke(newDevices);
                EventBus.Publish(new DeviceListUpdatedEvent(newDevices));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ 扫描异常：{ex.Message}");
                BluetoothDevicesUpdated?.Invoke(new List<Core.DeviceBatteryInfo>());
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
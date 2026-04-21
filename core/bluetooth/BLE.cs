using System.Collections.Specialized;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace BtBatteryDisplayApp
{
    /// <summary>
    /// .NET 6 Windows 原生蓝牙服务（官方API，无平台异常，真读电量）
    /// </summary>
    public class BleBatteryService
    {
        // 标准GATT电池UUID（和你的Python代码完全一致）
        private readonly Guid BATTERY_SERVICE_UUID = new Guid("0000180F-0000-1000-8000-00805F9B34FB");
        private readonly Guid BATTERY_LEVEL_UUID = new Guid("00002A19-0000-1000-8000-00805F9B34FB");

        // LRU缓存
        private readonly OrderedDictionary _cache = new OrderedDictionary();
        private const int MAX_CACHE = 20;

        public async Task<List<DeviceBatteryInfo>> GetConnectedDevicesBatteryAsync()
        {
            List<DeviceBatteryInfo> result = new List<DeviceBatteryInfo>();
            try
            {
                Debug.WriteLine("=====================================");
                Debug.WriteLine("【蓝牙服务】开始扫描BLE设备...");

                // 【官方原生API】无平台异常
                string selector = BluetoothLEDevice.GetDeviceSelector();
                DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

                Debug.WriteLine($"【蓝牙服务】扫描到 {devices.Count} 个BLE设备");

                foreach (DeviceInformation dev in devices)
                {
                    BluetoothLEDevice bleDevice = null;
                    try
                    {
                        // 连接设备
                        bleDevice = await BluetoothLEDevice.FromIdAsync(dev.Id);
                        if (bleDevice == null) continue;

                        bool isConnected = bleDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                        Debug.WriteLine($"\n设备：{dev.Name} | 状态：{(isConnected ? "已连接" : "未连接")}");

                        var info = new DeviceBatteryInfo
                        {
                            Name = dev.Name,
                            Mac = bleDevice.BluetoothAddress.ToString("X12"),
                            IsConnected = isConnected
                        };

                        // 已连接设备读取真实电量
                        if (isConnected)
                        {
                            info.Battery = await ReadBatteryLevel(bleDevice);
                            Debug.WriteLine($"设备电量：{info.Battery}%");
                            UpdateCache(dev.Id, info.Battery);
                        }
                        else
                        {
                            info.Battery = _cache.Contains(dev.Id) ? (int)_cache[dev.Id] : 0;
                        }

                        result.Add(info);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"设备异常：{ex.Message}");
                    }
                    finally
                    {
                        // 释放设备资源
                        bleDevice?.Dispose();
                    }
                }

                Debug.WriteLine("=====================================\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"【致命错误】{ex.Message}");
            }
            Debug.WriteLine($"Bleresult{result}");
            return result;
        }

        /// <summary>
        /// 官方API读取真实电量
        /// </summary>
        private async Task<int> ReadBatteryLevel(BluetoothLEDevice device)
        {
            try
            {
                // 获取电池服务
                GattDeviceServicesResult services = await device.GetGattServicesForUuidAsync(BATTERY_SERVICE_UUID);
                if (services.Status != GattCommunicationStatus.Success || services.Services.Count == 0)
                {
                    Debug.WriteLine("未找到电池服务");
                    return 0;
                }

                GattDeviceService service = services.Services[0];

                // 获取电量特征
                GattCharacteristicsResult chars = await service.GetCharacteristicsForUuidAsync(BATTERY_LEVEL_UUID);
                if (chars.Status != GattCommunicationStatus.Success || chars.Characteristics.Count == 0)
                {
                    Debug.WriteLine("未找到电量特征");
                    service.Dispose();
                    return 0;
                }

                GattCharacteristic characteristic = chars.Characteristics[0];

                // 读取电量值
                GattReadResult result = await characteristic.ReadValueAsync();
                if (result.Status != GattCommunicationStatus.Success)
                {
                    service.Dispose();
                    return 0;
                }

                // 解析电量
                DataReader reader = DataReader.FromBuffer(result.Value);
                byte battery = reader.ReadByte();

                // 释放资源
                service.Dispose();
                reader.Dispose();

                return Math.Clamp((int)battery, 0, 100);
            }
            catch
            {
                return 0;
            }
        }

        // 缓存管理
        private void UpdateCache(string deviceId, int battery)
        {
            lock (_cache)
            {
                if (_cache.Contains(deviceId)) _cache.Remove(deviceId);
                _cache.Add(deviceId, battery);
                while (_cache.Count > MAX_CACHE) _cache.RemoveAt(0);
                Debug.WriteLine($"【缓存】更新电量：{battery}%");
            }
        }
    }

    // 🔥 这里原来的 DeviceBatteryInfo 类 已经彻底删除！！！
}
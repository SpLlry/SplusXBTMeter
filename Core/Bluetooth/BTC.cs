using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace SplusXBTMeter.Core.Bluetooth
{
    /// <summary>
    /// 经典蓝牙(BTC)电量读取服务
    /// </summary>
    public class BtcBatteryService
    {
        #region Windows API 结构体
        [StructLayout(LayoutKind.Sequential)]
        public struct GUID
        {
            public uint Data1;
            public ushort Data2;
            public ushort Data3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data4;

            public GUID(uint d1, ushort d2, ushort d3, byte[] d4)
            {
                Data1 = d1;
                Data2 = d2;
                Data3 = d3;
                Data4 = new byte[8];
                Array.Copy(d4, Data4, 8);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public GUID fmtid;
            public uint pid;
        }
        #endregion

        #region Windows API 导入
        private const uint CM_LOCATE_DEVNODE_NORMAL = 0x00000000;
        private const uint CR_SUCCESS = 0x00000000;

        // 🔥 修复：去掉 readonly，解决 ref 参数报错
        private static DEVPROPKEY DEVPKEY_BLUETOOTH_BATTERY = new()
        {
            fmtid = new GUID(0x104EA319, 0x6EE2, 0x4701, [0xBD, 0x47, 0x8D, 0xDB, 0xF4, 0x25, 0xBB, 0xE5]),
            pid = 2
        };

        [DllImport("CfgMgr32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern uint CM_Locate_DevNodeW(out uint devNode, string deviceId, uint flags);

        [DllImport("CfgMgr32.dll", SetLastError = true)]
        private static extern uint CM_Get_DevNode_PropertyW(
            uint devNode, ref DEVPROPKEY propKey, out uint propType,
            out byte propValue, ref uint propSize, uint flags);
        #endregion

        private readonly OrderedDictionary _cache = [];
        private const int MAX_CACHE = 20;

        public async Task<List<DeviceBatteryInfo>> GetConnectedBtcDevicesAsync()
        {
            List<DeviceBatteryInfo> result = [];
            try
            {
                Debug.WriteLine("=====================================");
                Debug.WriteLine("【经典蓝牙BTC】扫描中...");

                var selector = BluetoothDevice.GetDeviceSelector();
                var devices = await DeviceInformation.FindAllAsync(selector);
                Debug.WriteLine($"找到 {devices.Count} 个BTC设备");

                foreach (var dev in devices)
                {
                    try
                    {
                        using var btDevice = await BluetoothDevice.FromIdAsync(dev.Id);
                        if (btDevice == null) continue;

                        bool connected = btDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                        string mac = btDevice.BluetoothAddress.ToString("X12");
                        int battery = 0;

                        Debug.WriteLine($"\n设备：{dev.Name} | 状态：{(connected ? "已连接" : "未连接")}");

                        if (connected)
                        {
                            battery = GetBatteryLevel(dev.Id);
                            Debug.WriteLine($"电量：{battery}%");
                            UpdateCache(dev.Id, battery);
                        }
                        else
                        {
                            battery = _cache.Contains(dev.Id) ? (int)_cache[dev.Id] : 0;
                        }

                        result.Add(new DeviceBatteryInfo
                        {
                            Name = dev.Name,
                            Mac = mac,
                            IsConnected = connected,
                            Battery = battery
                        });
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"设备异常：{ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BTC错误：{ex.Message}");
            }
            Debug.WriteLine($"BTCresult{result}");
            return result;
        }

        private static int GetBatteryLevel(string deviceId)
        {
            try
            {
                if (CM_Locate_DevNodeW(out uint devNode, deviceId, CM_LOCATE_DEVNODE_NORMAL) != CR_SUCCESS)
                    return 0;

                byte battery = 0;
                uint propSize = (uint)Marshal.SizeOf(battery);

                // 🔥 修复：使用静态字段传递 ref，无报错
                if (CM_Get_DevNode_PropertyW(devNode, ref DEVPKEY_BLUETOOTH_BATTERY,
                    out _, out battery, ref propSize, 0) == CR_SUCCESS)
                {
                    return Math.Clamp((int)battery, 0, 100);
                }
            }
            catch { }
            return 0;
        }

        private void UpdateCache(string id, int battery)
        {
            lock (_cache)
            {
                if (_cache.Contains(id)) _cache.Remove(id);
                _cache.Add(id, battery);
                while (_cache.Count > MAX_CACHE) _cache.RemoveAt(0);
            }
        }
    }
}
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
        [StructLayout(LayoutKind.Sequential)]
        private struct SP_DEVINFO_DATA
        {
            public int cbSize;
            public GUID ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }
        #endregion

        #region Windows API 导入
        private const uint CM_LOCATE_DEVNODE_NORMAL = 0x00000000;
        private const uint CR_SUCCESS = 0x00000000;
            private const int DIGCF_PRESENT = 0x00000002;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new(-1);
                private static readonly GUID GUID_DEVCLASS_SYSTEM = new(0x4D36E97D, 0xE325, 0x11CE, new byte[] { 0xBF, 0xC1, 0x08, 0x00, 0x2B, 0xE1, 0x03, 0x18 });
        [DllImport("Setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr SetupDiGetClassDevsW(ref GUID ClassGuid, string? Enumerator, IntPtr hwndParent, int Flags);

        [DllImport("Setupapi.dll", SetLastError = true)]
        private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, int MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

        [DllImport("Setupapi.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool SetupDiGetDeviceInstanceIdW(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, char[] DeviceInstanceId, int InstanceIdSize, out int RequiredSize);

        [DllImport("Setupapi.dll")]
        private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);
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
                            battery = GetBatteryLevel(mac);
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
        // ====================== 【仅新增】获取蓝牙设备实例ID 方法 ======================
        private static string GetBluetoothInstanceId(string bluetoothMac)
        {
            IntPtr hDevInfo = INVALID_HANDLE_VALUE;
            try
            {
                GUID classGuid = GUID_DEVCLASS_SYSTEM;
                hDevInfo = SetupDiGetClassDevsW(ref classGuid, null, IntPtr.Zero, DIGCF_PRESENT);
                if (hDevInfo == INVALID_HANDLE_VALUE) return string.Empty;

                SP_DEVINFO_DATA devInfo = new();
                devInfo.cbSize = Marshal.SizeOf(devInfo);
                int index = 0;

                while (SetupDiEnumDeviceInfo(hDevInfo, index, ref devInfo))
                {
                    index++;
                    char[] buffer = new char[256];
                    if (SetupDiGetDeviceInstanceIdW(hDevInfo, ref devInfo, buffer, buffer.Length, out _))
                    {
                        string instanceId = new string(buffer).TrimEnd('\0');
                        //Debug.WriteLine($"instanceId{instanceId}---{bluetoothMac}");
                        if (instanceId.Contains("BTHENUM\\") && instanceId.Contains(bluetoothMac))
                        {
                            return instanceId;
                        }
                    }
                }
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
            finally
            {
                if (hDevInfo != INVALID_HANDLE_VALUE)
                    SetupDiDestroyDeviceInfoList(hDevInfo);
            }
        }

        private static int GetBatteryLevel(string deviceId)
        {
            try
            {
                string instance_id = GetBluetoothInstanceId(deviceId);
                uint DevNodeRet = CM_Locate_DevNodeW(out uint devNode, instance_id, CM_LOCATE_DEVNODE_NORMAL);
                if (DevNodeRet != CR_SUCCESS)
                {
                    Debug.WriteLine($"CM_Locate_DevNodeW：{deviceId}-{instance_id}");
                    return 0;
                }
            

                byte battery = 0;
                uint propSize = (uint)Marshal.SizeOf(battery);

                // 🔥 修复：使用静态字段传递 ref，无报错
                if (CM_Get_DevNode_PropertyW(devNode, ref DEVPKEY_BLUETOOTH_BATTERY,
                    out _, out battery, ref propSize, 0) == CR_SUCCESS)
                {
                    return Math.Clamp((int)battery, 0, 100);
                }
            }
            catch {
              Debug.WriteLine($"获取电量异常：{deviceId}");
            }
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
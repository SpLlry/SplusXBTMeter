using BtBatteryDisplayApp;

namespace BTBatteryDisplayApp
{
    /// <summary>
    /// 蓝牙扫描总入口：整合经典蓝牙(BTC) + 低功耗蓝牙(BLE)
    /// </summary>
    public class BtScan
    {
        // 分别初始化BTC和BLE扫描服务
        private readonly BtcBatteryService _btcService = new BtcBatteryService();
        private readonly BleBatteryService _bleService = new BleBatteryService();

        /// <summary>
        /// 【调试开关】是否使用模拟数据（默认关闭，上线前需保持false）
        /// </summary>
        public bool UseMockData { get; set; } = false;

        /// <summary>
        /// 获取所有蓝牙设备（BTC+BLE）的电量信息（自动去重）
        /// </summary>
        public async Task<List<DeviceBatteryInfo>> GetAllBluetoothDevicesBatteryAsync()
        {
            // 优先返回模拟数据（调试模式）
            Console.WriteLine($"UseMockData:{UseMockData}");
            if (UseMockData)
            {
                return await Task.FromResult(GenerateMockBluetoothDevices());
            }

            // 真实扫描逻辑（原有代码保留）
            var btcTask = _btcService.GetConnectedBtcDevicesAsync();
            var bleTask = _bleService.GetConnectedDevicesBatteryAsync();
            await Task.WhenAll(btcTask, bleTask);

            // 合并结果（按MAC去重，优先保留已连接的设备）
            var allDevices = new List<DeviceBatteryInfo>();

            // 添加BTC设备
            if (btcTask.Result != null)
                allDevices.AddRange(btcTask.Result);

            // 添加BLE设备（排除MAC重复的）
            if (bleTask.Result != null)
            {
                var existingMacs = allDevices.Select(d => d.Mac).ToHashSet();
                var newBleDevices = bleTask.Result.Where(d => !existingMacs.Contains(d.Mac)).ToList();
                allDevices.AddRange(newBleDevices);
            }

            return allDevices;
        }

        /// <summary>
        /// 生成模拟蓝牙设备数据（覆盖BTC/BLE、连接/未连接、不同电量）
        /// </summary>
        private List<DeviceBatteryInfo> GenerateMockBluetoothDevices()
        {
            return new List<DeviceBatteryInfo>
            {
                // 模拟经典蓝牙(BTC)设备（已连接，高电量）
                new DeviceBatteryInfo
                {
                    Name = "BTC-蓝牙耳机",
                    Mac = "001A2B3C4D5E",
                    IsConnected = true,
                    Battery = 85
                },
                // 模拟经典蓝牙(BTC)设备（未连接，缓存电量）
                new DeviceBatteryInfo
                {
                    Name = "BTC-蓝牙音箱",
                    Mac = "001A2B3C4D5F",
                    IsConnected = false,
                    Battery = 45
                },
                // 模拟低功耗蓝牙(BLE)设备（已连接，低电量提醒）
                new DeviceBatteryInfo
                {
                    Name = "BLE-智能手表",
                    Mac = "112B3C4D5E6F",
                    IsConnected = true,
                    Battery = 18
                },
                // 模拟低功耗蓝牙(BLE)设备（未连接，无缓存）
                new DeviceBatteryInfo
                {
                    Name = "BLE-无线鼠标",
                    Mac = "112B3C4D5E70",
                    IsConnected = false,
                    Battery = 0
                }
            };
        }
    }
}
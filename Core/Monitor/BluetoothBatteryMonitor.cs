using Windows.Devices.Enumeration;

public class BluetoothBatteryMonitor
{
    private DeviceWatcher? _deviceWatcher;

    public async Task StartMonitoring()
    {
        // 1. 构造筛选器，寻找蓝牙设备（包括已配对和已连接的）
        // 这里的 AQS 字符串用于匹配所有的蓝牙设备接口
        string aqsFilter = "System.Devices.DevObjectType:=5";

        // 我们需要获取的设备属性，空数组代表获取所有基础属性（包含电量）
        string[] requestedProperties = new string[]
        {
            "System.Devices.Aep.ContainerId",
            "System.Devices.Battery" // 显式请求电池属性，如果系统暴露了该字段
        };

        // 2. 创建 DeviceWatcher 实例
        _deviceWatcher = DeviceInformation.CreateWatcher(aqsFilter, requestedProperties, DeviceInformationKind.AssociationEndpoint);

        // 3. Hook（订阅）系统事件
        _deviceWatcher.Added += OnDeviceAdded;
        _deviceWatcher.Updated += OnDeviceUpdated;
        _deviceWatcher.Removed += OnDeviceRemoved;
        _deviceWatcher.EnumerationCompleted += OnEnumerationCompleted;

        // 4. 启动监控
        _deviceWatcher.Start();
        Console.WriteLine("开始监控蓝牙设备电量...");
    }

    private void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
    {
        // 当设备信息更新时触发（例如电量下降）
        if (deviceUpdate.Properties.TryGetValue("System.Devices.Battery", out object batteryObj))
        {
            // 电量通常以 0.0 到 1.0 之间的 double 类型存在，或者有时是 int 百分比
            if (batteryObj != null)
            {
                double batteryLevel = Convert.ToDouble(batteryObj);
                int percentage = (int)(batteryLevel * 100);

                Console.WriteLine($"[电量更新] 设备ID: {deviceUpdate.Id}");
                Console.WriteLine($"           当前电量: {percentage}%");
            }
        }
    }

    private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation deviceInfo)
    {
        Console.WriteLine($"[设备接入] 名称: {deviceInfo.Name}, ID: {deviceInfo.Id}");
    }

    private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate deviceUpdate)
    {
        Console.WriteLine($"[设备断开] ID: {deviceUpdate.Id}");
    }

    private void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        Console.WriteLine("初始设备枚举完成。");
    }

    public void StopMonitoring()
    {
        _deviceWatcher?.Stop();
        _deviceWatcher = null;
    }
}
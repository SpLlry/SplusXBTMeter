namespace SplusXBTMeter.Core
{
    // 设置变更事件
    public record SettingsChangedEvent(string SettingName, object? OldValue, object? NewValue);

    // 设备列表更新事件
    public record DeviceListUpdatedEvent(List<DeviceBatteryInfo> Devices);

    // 主题变更事件
    public record ThemeChangedEvent(int ThemeMode);

    // 任务栏样式变更事件
    public record TaskBarStyleChangedEvent(string Style);

    // 重启应用事件
    public record RestartRequestedEvent();

    // 皮肤变更事件
    public record SkinChangedEvent(string OldSkin, string NewSkin);

    // 窗口显示/隐藏事件
    public record WindowVisibilityChangedEvent(string WindowName, bool IsVisible);

    // 蓝牙设备连接事件
    public record BluetoothDeviceConnectedEvent(string DeviceName, string MacAddress);

    // 蓝牙设备断开事件
    public record BluetoothDeviceDisconnectedEvent(string DeviceName, string MacAddress);

    // 电池电量更新事件
    public record BatteryLevelUpdatedEvent(string DeviceName, int BatteryLevel);

    //任务栏对齐方式变更事件
    public record TaskbarAlignmentChangedEvent(int Alignment);

    public record TaskbarTrayNotifyChangedEvent(Win32Api.RECT rect);

}
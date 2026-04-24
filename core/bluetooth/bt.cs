namespace SplusXBTMeter
{
    /// <summary>
    /// 蓝牙设备公共实体类（BLE + BTC 共用）
    /// </summary>
    public class DeviceBatteryInfo
    {
        public required string Name { get; set; } = "";
        public required string Mac { get; set; }
        public bool IsConnected { get; set; }
        public int Battery { get; set; }
        public bool IsShow { get; set; } = true;
        public DeviceBatteryInfo Clone()
        {
            return new DeviceBatteryInfo
            {
                Name = this.Name,
                Mac = this.Mac,
                IsConnected = this.IsConnected,
                Battery = this.Battery,
                IsShow = this.IsShow
            };
        }
    }

}
namespace SplusXBTMeter
{
    public class DeviceListUpdatedEvent
    {
        public List<DeviceBatteryInfo> Devices { get; }

        public DeviceListUpdatedEvent(List<DeviceBatteryInfo> devices)
        {
            Devices = devices;
        }
    }

    public class TaskbarAlignmentChangedEvent
    {
        public int Alignment { get; }

        public TaskbarAlignmentChangedEvent(int alignment)
        {
            Alignment = alignment;
        }
    }

    public class SkinChangedEvent
    {
        public string OldSkin { get; }
        public string NewSkin { get; }

        public SkinChangedEvent(string oldSkin, string newSkin)
        {
            OldSkin = oldSkin;
            NewSkin = newSkin;
        }
    }
}
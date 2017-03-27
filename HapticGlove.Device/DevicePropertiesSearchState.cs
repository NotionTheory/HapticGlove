using System;

namespace NotionTheory.HapticGlove
{
    [Flags]
    public enum DevicePropertiesSearchState
    {
        NotReady = 0x0000,
        DeviceFound = 0x0001,
        DeviceInformationServiceFound = 0x0002,
        BatteryServiceFound = 0x0004,
        Sensor0Found = 0x0008,
        Sensor1Found = 0x0010,
        Sensor2Found = 0x0020,
        Sensor3Found = 0x0040,
        Sensor4Found = 0x0080,
        Motor0Found = 0x0100,
        Motor1Found = 0x0200,
        Motor2Found = 0x0400,
        Motor3Found = 0x0800,
        Motor4Found = 0x1000,
        Ready = 0x1FFF
    }
}

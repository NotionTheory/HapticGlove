using System;

namespace HapticGlove
{
    [Flags]
    public enum GloveState
    {
        NotReady = 0,
        Watching = 1,
        Searching = 2,
        DeviceFound = 4,
        DeviceInformationServiceFound = 8,
        BatteryServiceFound = 16,
        MotorsFound = 32,
        Sensor0Found = 64,
        Sensor1Found = 128,
        Sensor2Found = 256,
        Sensor3Found = 512,
        Sensor4Found = 1024,
        Ready = 0x7FC
    }
}

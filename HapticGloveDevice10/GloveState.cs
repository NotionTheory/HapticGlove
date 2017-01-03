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
        BatteryFound = 32,
        MotorsFound = 64,
        Finger1Found = 128,
        Finger2Found = 256,
        Finger3Found = 512,
        Finger4Found = 1024,
        Finger5Found = 2048,
        Ready = 0xFFC
    }
}

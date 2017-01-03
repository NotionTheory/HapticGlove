using System;

namespace HapticGlove
{
    [Flags]
    public enum GloveState
    {
        NotReady = 0,
        Searching = 1,
        DeviceInformationFound = 2,
        DeviceFound = 4,
        ServiceFound = 8,
        CharacteristicsFound = 16,
        BatteryFound = 32,
        FingersFound = 64,
        MotorsFound = 128,
        Ready = 0xFE
    }
}

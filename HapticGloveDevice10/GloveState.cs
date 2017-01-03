using System;

namespace HapticGlove
{
    [Flags]
    public enum GloveState
    {
        NotReady = 0,
        Searching = 1,
        DeviceFound = 2,
        ServiceFound = 4,
        CharacteristicsFound = 8,
        BatteryFound = 16,
        FingersFound = 32,
        MotorsFound = 64,
        Ready = 0xffffffe
    }
}

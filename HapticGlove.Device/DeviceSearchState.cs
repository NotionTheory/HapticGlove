using System;

namespace NotionTheory.HapticGlove
{
    [Flags]
    public enum DeviceSearchState
    {
        NotReady = 0,
        Watching = 1,
        Searching = 2,
        DevicesFound = 4,
        DevicePropertiesFound = 8,
        Ready = 0xD
    }
}

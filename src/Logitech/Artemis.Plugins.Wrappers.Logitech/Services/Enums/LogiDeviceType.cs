using System;

namespace Artemis.Plugins.Wrappers.Logitech.Services
{
    [Flags]
    public enum LogiDeviceType
    {
        Keyboard = 0x0,//0
        Mouse = 0x3,//3
        Mousemat = 0x4,//4
        Headset = 0x8,//8
        Speaker = 0xe//14
    }
}

using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct KeyboardCustomEffectExtended
    {
        public const int Size = 8 * 24;
        public const int SizeKeys = 6 * 22;

        private fixed uint _colors[Size];
        private fixed uint _keys[SizeKeys];//what even is this?? investigate.

        public SKColor this[int idx]
        {
            get
            {
                if (idx >= Size)
                    throw new ArgumentOutOfRangeException();

                return SKColorExtensions.FromRazerUint(_colors[idx]);
            }
        }
    }
}

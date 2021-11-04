using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct KeyboardCustomEffect
    {
        public const int Size = 6 * 22;

        private fixed uint _colors[Size];

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

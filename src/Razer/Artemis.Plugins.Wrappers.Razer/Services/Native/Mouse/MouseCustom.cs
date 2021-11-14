using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct MouseCustom
    {
        public const int Size = 30;

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

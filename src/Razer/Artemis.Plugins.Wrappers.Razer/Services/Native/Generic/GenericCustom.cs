using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct GenericCustom
    {
        public const int Size = 30 * 30;
        public readonly ulong Count;
        public readonly uint Param;
        private fixed uint _colors[Size];

        public SKColor this[int idx]
        {
            get
            {
                if (idx >= Size)
                    throw new ArgumentOutOfRangeException();

                var clr = _colors[idx];
                return SKColorExtensions.FromRazerUint(clr);
            }
        }
    }
}

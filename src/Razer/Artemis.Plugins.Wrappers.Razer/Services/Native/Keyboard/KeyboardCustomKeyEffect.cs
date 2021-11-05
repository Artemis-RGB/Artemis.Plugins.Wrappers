using SkiaSharp;
using System;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct KeyboardCustomKeyEffect
    {
        public const int Size = 6 * 22;
        public const int SizeKeys = 6 * 22;

        private fixed uint _colors[Size];
        private fixed uint _keys[SizeKeys];

        public SKColor this[int idx]
        {
            get
            {
                if (idx >= Size)
                    throw new ArgumentOutOfRangeException();

                return SKColorExtensions.FromRazerUint(_colors[idx]);
            }
        }

        public SKColor[] GetKeys()
        {
            var arr = new SKColor[SizeKeys];

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = SKColorExtensions.FromRazerUint(_keys[i]);
            }

            return arr;
        }
    }
}

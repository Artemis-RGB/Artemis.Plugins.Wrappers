using SkiaSharp;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services.Native
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal unsafe struct KeypadStatic
    {
        private readonly uint _color;

        public SKColor Color => SKColorExtensions.FromRazerUint(_color);
    }
}

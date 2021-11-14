using SkiaSharp;

namespace Artemis.Plugins.Wrappers.Razer.Services.Native
{
    public static class SKColorExtensions
    {
        public static SKColor FromRazerUint(uint value) => new SKColor(
                    (byte)((value >> 00) & 0xFF),
                    (byte)((value >> 08) & 0xFF),
                    (byte)((value >> 16) & 0xFF)
                //(byte)((value >> 24) & 0xFF)//ignore alpha
                );
    }
}

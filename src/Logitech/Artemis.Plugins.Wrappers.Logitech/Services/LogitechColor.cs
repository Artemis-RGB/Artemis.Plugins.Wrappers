using System.Runtime.InteropServices;
using SkiaSharp;

namespace Artemis.Plugins.Wrappers.Logitech.Services;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct LogitechColor
{
    //bgra
    public readonly byte B;
    public readonly byte G;
    public readonly byte R;
    public readonly byte A;
    
    public SKColor ToSkColor() => new(R, G, B, A);
}
using RGB.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Artemis.Plugins.Wrappers.Razer.Services.Effects
{
    internal interface IRazerChromaEffect
    {
        void Apply(IDictionary<LedId, SKColor> dict);
    }

    internal abstract class AbstractRazerChromaEffect<T> : IRazerChromaEffect where T : unmanaged
    {
        protected readonly T _effect;
        protected AbstractRazerChromaEffect(ReadOnlySpan<byte> bytes)
        {
            _effect = MemoryMarshal.Read<T>(bytes);
        }

        public abstract void Apply(IDictionary<LedId, SKColor> dict);
    }
}

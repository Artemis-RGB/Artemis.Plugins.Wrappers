using Artemis.Plugins.Wrappers.Razer.Services.Native;
using RGB.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Razer.Services.Effects
{
    internal class MouseStaticEffect : AbstractRazerChromaEffect<MouseStatic>
    {
        public MouseStaticEffect(ReadOnlySpan<byte> bytes) : base(bytes) { }

        public override void Apply(IDictionary<LedId, SKColor> dict)
        {
            //TODO
        }
    }
}

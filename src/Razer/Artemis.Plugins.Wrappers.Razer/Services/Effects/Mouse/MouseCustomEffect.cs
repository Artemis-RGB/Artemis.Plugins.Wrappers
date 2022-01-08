using Artemis.Plugins.Wrappers.Razer.Services.Native;
using RGB.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Razer.Services.Effects
{
    internal class MouseCustomEffect : AbstractRazerChromaEffect<MouseCustom>
    {
        public MouseCustomEffect(ReadOnlySpan<byte> bytes) : base(bytes) { }

        public override void Apply(IDictionary<LedId, SKColor> dict)
        {
            for (int i = 0; i < MouseCustom.Size; i++)
            {
                var ledId = LedMapping.Mouse[i];
                var newColor = _effect[i];

                if (ledId != LedId.Invalid)
                    dict[ledId] = newColor;
            }
        }
    }
}

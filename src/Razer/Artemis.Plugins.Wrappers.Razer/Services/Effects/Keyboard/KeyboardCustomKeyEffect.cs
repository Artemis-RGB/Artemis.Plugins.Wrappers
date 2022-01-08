using Artemis.Plugins.Wrappers.Razer.Services.Native;
using RGB.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Razer.Services.Effects
{
    internal class KeyboardCustomKeyEffect : AbstractRazerChromaEffect<KeyboardCustomKey>
    {
        public KeyboardCustomKeyEffect(ReadOnlySpan<byte> bytes) : base(bytes) { }

        public override void Apply(IDictionary<LedId, SKColor> dict)
        {
            //TODO: handle keys
            var keys = _effect.GetKeys();
            for (int i = 0; i < KeyboardCustomKey.Size; i++)
            {
                var ledId = LedMapping.Keyboard[i];
                var newColor = _effect[i];

                if (ledId != LedId.Invalid)
                    dict[ledId] = newColor;
            }
        }
    }
}

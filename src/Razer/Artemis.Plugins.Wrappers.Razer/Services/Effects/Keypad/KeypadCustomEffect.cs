﻿using Artemis.Plugins.Wrappers.Razer.Services.Native;
using RGB.NET.Core;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Razer.Services.Effects
{
    internal class KeypadCustomEffect : AbstractRazerChromaEffect<KeypadCustom>
    {
        public KeypadCustomEffect(ReadOnlySpan<byte> effectData) : base(effectData) { }

        public override void Apply(IDictionary<LedId, SKColor> dict)
        {
            for (int i = 0; i < KeypadCustom.Size; i++)
            {
                var ledId = LedMapping.Keypad[i];
                var newColor = _effect[i];

                if (ledId != LedId.Invalid)
                    dict[ledId] = newColor;
            }
        }
    }
}
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.Wrappers.Logitech.Services;
using RGB.NET.Core;
using SkiaSharp;
using System.Collections.Generic;
using System;

namespace Artemis.Plugins.Wrappers.Logitech.LayerBrushes
{
    public class LogitechWrapperLayerBrush : PerLedLayerBrush<LogitechWrapperLayerPropertyGroup>
    {
        private readonly LogitechWrapperListenerService _wrapperService;
        private readonly Dictionary<LedId, SKColor> _colors = new();

        public LogitechWrapperLayerBrush(LogitechWrapperListenerService wrapperService)
        {
            _wrapperService = wrapperService;
        }

        public override void EnableLayerBrush()
        {
            _wrapperService.ColorsUpdated += OnWrapperServiceColorsUpdated;
            _wrapperService.ClientDisconnected += OnWrapperServiceClientDisconnected;
        }

        public override void DisableLayerBrush()
        {
            _wrapperService.ColorsUpdated -= OnWrapperServiceColorsUpdated;
            _wrapperService.ClientDisconnected -= OnWrapperServiceClientDisconnected;
        }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (_colors.TryGetValue(led.RgbLed.Id, out SKColor color))
            {
                return color;
            }

            return _wrapperService.BackgroundColor;
        }

        public override void Update(double deltaTime) { }

        private void OnWrapperServiceColorsUpdated(object sender, EventArgs e)
        {
            foreach (KeyValuePair<LedId, SKColor> kvp in _wrapperService.Colors)
            {
                _colors[kvp.Key] = kvp.Value;
            }
        }

        private void OnWrapperServiceClientDisconnected(object sender, EventArgs e)
        {
            _colors.Clear();
        }
    }
}

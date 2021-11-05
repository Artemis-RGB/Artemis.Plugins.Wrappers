using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Plugins.Wrappers.Razer.Services;
using SkiaSharp;

namespace Artemis.Plugins.Wrappers.Razer.LayerBrushes
{
    public class RazerWrapperLayerBrush : PerLedLayerBrush<RazerWrapperLayerPropertyGroup>
    {
        private readonly RazerWrapperListenerService _wrapperService;

        public RazerWrapperLayerBrush(RazerWrapperListenerService wrapperService)
        {
            _wrapperService = wrapperService;
        }

        public override void EnableLayerBrush()
        {
        }

        public override void DisableLayerBrush()
        {
        }

        public override SKColor GetColor(ArtemisLed led, SKPoint renderPoint)
        {
            if (_wrapperService.Colors.TryGetValue(led.RgbLed.Id, out SKColor color))
            {
                return color;
            }

            return SKColor.Empty;
        }

        public override void Update(double deltaTime) { }
    }
}

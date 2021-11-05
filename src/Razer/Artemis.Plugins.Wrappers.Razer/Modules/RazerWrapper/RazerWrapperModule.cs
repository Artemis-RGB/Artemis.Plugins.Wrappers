using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Wrappers.Razer.DataModels;
using Artemis.Plugins.Wrappers.Razer.Services;
using RGB.NET.Core;
using SkiaSharp;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Razer
{
    [PluginFeature(Name = "Razer Wrapper", Icon = "Snake")]
    public class RazerWrapperModule : Module<RazerWrapperDataModel>
    {
        private readonly RazerWrapperListenerService _razerWrapperListenerService;
        private readonly Dictionary<LedId, DynamicChild<SKColor>> _colorsCache = new();

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public RazerWrapperModule(RazerWrapperListenerService razerWrapperListenerService)
        {
            _razerWrapperListenerService = razerWrapperListenerService;
        }

        public override void Enable()
        {
            _razerWrapperListenerService.ColorsUpdated += OnColorsUpdated;
        }

        public override void Disable()
        {
        }

        public override void ModuleActivated(bool isOverride)
        {
        }

        public override void ModuleDeactivated(bool isOverride)
        {
        }

        public override void Update(double deltaTime)
        {
        }

        private void OnColorsUpdated(object sender, System.EventArgs e)
        {
            foreach ((var ledId, var clr) in _razerWrapperListenerService.Colors)
            {
                if (!_colorsCache.TryGetValue(ledId, out var colorDataModel))
                {
                    colorDataModel = DataModel.Leds.AddDynamicChild<SKColor>(ledId.ToString(), default);
                    _colorsCache.Add(ledId, colorDataModel);
                }

                colorDataModel.Value = clr;
            }
        }
    }
}
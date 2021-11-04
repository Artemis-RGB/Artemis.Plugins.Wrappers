using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Wrappers.Logitech.Modules.DataModels;
using Artemis.Plugins.Wrappers.Logitech.Services;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Logitech.Modules
{
    [PluginFeature(Name = "Logitech Wrapper Module")]
    public class LogitechWrapperModule : Module<LogitechWrapperDataModel>
    {
        private readonly LogitechWrapperListenerService _wrapperService;
        private readonly Dictionary<LedId, DynamicChild<SKColor>> _colorsCache = new();

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public LogitechWrapperModule(LogitechWrapperListenerService service)
        {
            _wrapperService = service;
        }

        public override void Enable()
        {
            _wrapperService.ColorsUpdated += WrapperServiceOnColorsUpdated;
            _wrapperService.ClientDisconnected += WrapperServiceOnClientDisconnected;
        }

        public override void Disable()
        {
            _wrapperService.ColorsUpdated -= WrapperServiceOnColorsUpdated;
            _wrapperService.ClientDisconnected -= WrapperServiceOnClientDisconnected;
        }

        public override void Update(double deltaTime) { }

        private void WrapperServiceOnColorsUpdated(object sender, EventArgs e)
        {
            DataModel.BackgroundColor = _wrapperService.BackgroundColor;
            DataModel.Flag = _wrapperService.UnknownParameter;
            DataModel.FlagHex = _wrapperService.UnknownParameter.ToString("x8");

            foreach (KeyValuePair<LedId, SKColor> kvp in _wrapperService.Colors)
            {
                if (!_colorsCache.TryGetValue(kvp.Key, out DynamicChild<SKColor> colorDataModel))
                {
                    colorDataModel = DataModel.Keys.AddDynamicChild<SKColor>(kvp.Key.ToString(), default);
                    _colorsCache.Add(kvp.Key, colorDataModel);
                }

                colorDataModel.Value = kvp.Value;
            }
        }

        private void WrapperServiceOnClientDisconnected(object sender, EventArgs e)
        {
            DataModel.BackgroundColor = SKColor.Empty;
            DataModel.Keys.ClearDynamicChildren();
            _colorsCache.Clear();
        }
    }
}
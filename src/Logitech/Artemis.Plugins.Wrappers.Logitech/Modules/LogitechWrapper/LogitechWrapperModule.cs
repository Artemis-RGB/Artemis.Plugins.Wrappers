﻿using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Wrappers.Logitech.Modules.DataModels;
using Artemis.Plugins.Wrappers.Logitech.Services;
using RGB.NET.Core;
using Serilog;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace Artemis.Plugins.Wrappers.Logitech.Modules
{
    [PluginFeature(Name = "Logitech Wrapper Module")]
    public class LogitechWrapperModule : Module<LogitechWrapperDataModel>
    {
        private readonly LogitechPipeListenerService _wrapperService;
        private readonly Dictionary<LedId, DynamicChild<SKColor>> _colorsCache = new();

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public LogitechWrapperModule(LogitechPipeListenerService service)
        {
            _wrapperService = service;
        }

        public override void Enable()
        {
            _wrapperService.ColorsUpdated += WrapperServiceOnColorsUpdated;
            _wrapperService.ClientConnected += WrapperServiceOnClientConnected;
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

        private void WrapperServiceOnClientConnected(object sender, string e)
        {
            DataModel.CurrentApplication = Path.GetFileName(e);
        }

        private void WrapperServiceOnClientDisconnected(object sender, EventArgs e)
        {
            DataModel.CurrentApplication = string.Empty;
            DataModel.BackgroundColor = SKColor.Empty;
            DataModel.Keys.ClearDynamicChildren();
            _colorsCache.Clear();
        }
    }
}
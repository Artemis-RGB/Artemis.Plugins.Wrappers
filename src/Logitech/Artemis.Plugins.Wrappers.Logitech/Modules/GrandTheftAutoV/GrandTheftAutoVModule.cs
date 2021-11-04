using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Wrappers.Logitech.Services;
using SkiaSharp;
using System;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Logitech.Modules
{
    [PluginFeature(Name = "Grand Theft Auto V")]
    public class GrandTheftAutoVModule : Module<GrandTheftAutoVDataModel>
    {
        public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new()
        { new ProcessActivationRequirement("GTA5") };

        private readonly LogitechWrapperListenerService _wrapperService;

        private static readonly Dictionary<SKColor, GtaGameState> _colorDictionary = new()
        {
            [SKColor.Parse("#FF30FF00")] = GtaGameState.SingleplayerFranklin,
            [SKColor.Parse("#FF30FFFF")] = GtaGameState.SingleplayerMichael,
            [SKColor.Parse("#FFAF4F00")] = GtaGameState.SingleplayerTrevor,
            [SKColor.Parse("#FFDB2121")] = GtaGameState.OnlineDefault,//CREW COLOR
            [SKColor.Parse("#FFC14F4F")] = GtaGameState.OnlineHeistPrep,
            [SKColor.Parse("#FF9B6DAF")] = GtaGameState.OnlineHeist,
            [SKColor.Parse("#00000000")] = GtaGameState.None,
            //Note: when dancing in the nighclub the color flashes blue
            //rapidly. Maybe handle this separately?
        };

        public static readonly HashSet<uint> _knownFlags = new()
        {
            0x13B9_C421,
            0x13B9_C4FF,
            0xFFFF_FF21,//spawn in online, pause when wanted??
            0xFFFF_FFFF,
            0x0000_0021,
            0xFFFF_FF50,
            0x13B9_C450,
            0x00FF_0121,//wanted 1. one of these red and the other blue maybe???
            0x00FF_0B21,//wanted 2
            0x3C81_0021,
            0x00FF_01FF,
            0x00FF_FBFF,
            0xFFFF_FF00,
            0x00FF_0150,
            0x00FF_FB50,
            0xFFFF_FFAF,
            0x00FF_01AF,
            0x00FF_0BAF,
            0x0000_2A00,//deth sp
            0x00FF_0100,//wanted 1 sp
            0X00ff_FB00//wanted 2 sp
        };

        public GrandTheftAutoVModule(LogitechWrapperListenerService service)
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

        public override void Update(double deltaTime)
        {
        }

        private void WrapperServiceOnColorsUpdated(object sender, EventArgs e)
        {
            if (!DataModel.KnownValues.Contains(_wrapperService.UnknownParameter))
            {
                DataModel.KnownValues.Add(_wrapperService.UnknownParameter);
                DataModel.KnownValueStrings.Add(_wrapperService.UnknownParameter.ToString("x8"));
            }

            if (!DataModel.KnownColors.Contains(_wrapperService.BackgroundColor))
            {
                DataModel.KnownColors.Add(_wrapperService.BackgroundColor);
            }

            if (_colorDictionary.TryGetValue(_wrapperService.BackgroundColor, out var s))
            {
                DataModel.State = s;
            }
            else
            {
                DataModel.State = GtaGameState.Unknown;
            }
        }

        private void WrapperServiceOnClientDisconnected(object sender, EventArgs e)
        {

        }
    }

    public class GrandTheftAutoVDataModel : DataModel
    {
        public GtaGameState State { get; set; }
        public List<int> KnownValues { get; } = new();
        public List<string> KnownValueStrings { get; } = new();
        public List<SKColor> KnownColors { get; } = new();

        //which singleplayer character
        //is in heist??
        //todo
    }
}

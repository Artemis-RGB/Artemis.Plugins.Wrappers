using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Wrappers.Razer.DataModels;
using Artemis.Plugins.Wrappers.Razer.Services;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.Razer
{
    [PluginFeature(Name = "Razer Wrapper", Icon = "Snake")]
    public class RazerWrapperModule : Module<RazerWrapperDataModel>
    {
        private readonly RazerWrapperListenerService _razerWrapperListenerService;
        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public RazerWrapperModule(RazerWrapperListenerService razerWrapperListenerService)
        {
            _razerWrapperListenerService = razerWrapperListenerService;
        }

        public override void Enable()
        {
            DataModel.Keyboard = _razerWrapperListenerService._keyboardCustom;
            DataModel.KeyboardExtended = _razerWrapperListenerService._keyboardCustomExtended;

            DataModel.Mouse = _razerWrapperListenerService._mouseCustom;
            DataModel.MouseExtended = _razerWrapperListenerService._mouseCustomExtended;

            DataModel.Mousepad = _razerWrapperListenerService._mousepadCustom;
            DataModel.MousepadExtended = _razerWrapperListenerService._mousepadCustomExtended;

            DataModel.Headset = _razerWrapperListenerService._headsetCustom;

            DataModel.Keypad = _razerWrapperListenerService._keypadCustom;

            DataModel.ChromaLink = _razerWrapperListenerService._chromaLinkCustom;
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
    }
}
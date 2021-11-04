using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Wrappers.LightFx.Modules.DataModels;
using Artemis.Plugins.Wrappers.LightFx.Services;
using System.Collections.Generic;

namespace Artemis.Plugins.Wrappers.LightFx.Modules
{
    [PluginFeature(AlwaysEnabled = true)]
    public class LightFxWrapperModule : Module<LightFxWrapperDataModel>
    {
        private readonly LightFxWrapperListenerService _lightFx;

        public override List<IModuleActivationRequirement> ActivationRequirements => null;

        public LightFxWrapperModule(LightFxWrapperListenerService lightFx)
        {
            _lightFx = lightFx;
        }

        public override void Enable()
        {
        }

        public override void Disable()
        {
        }

        public override void Update(double deltaTime)
        {
            DataModel.Color = _lightFx.Color;
        }
    }
}
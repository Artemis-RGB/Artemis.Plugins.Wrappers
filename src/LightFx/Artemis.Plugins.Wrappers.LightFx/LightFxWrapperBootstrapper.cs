using Artemis.Core;
using Artemis.Plugins.Wrappers.LightFx.Prerequisites;

namespace Artemis.Plugins.Wrappers.LightFx
{
    public class LightFxWrapperBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            AddPluginPrerequisite(new LightFxWrapperPrerequisite(plugin));
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}

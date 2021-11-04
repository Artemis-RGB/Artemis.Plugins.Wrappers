using Artemis.Core;
using Artemis.Plugins.Wrappers.Logitech.Prerequisites;

namespace Artemis.Plugins.Wrappers.Logitech
{
    public class LogitechWrapperBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            AddPluginPrerequisite(new LogitechWrapperPrerequisite(plugin));
        }
    }
}

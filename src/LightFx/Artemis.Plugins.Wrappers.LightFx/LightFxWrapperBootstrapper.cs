using Artemis.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Artemis.Plugins.Wrappers.LightFx
{
    public class LightFxWrapperBootstrapper : PluginBootstrapper
    {
        public override void OnPluginLoaded(Plugin plugin)
        {
            AddPluginPrerequisite(new WrapperDllsPrerequisite(plugin));
        }

        public override void OnPluginEnabled(Plugin plugin)
        {
        }

        public override void OnPluginDisabled(Plugin plugin)
        {
        }
    }
}

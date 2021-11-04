using Artemis.Core;
using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.Wrappers.Logitech.LayerBrushes
{
    [PluginFeature(Name = "Logitech Wrapper Layer")]
    public class LogitechWrapperLayerBrushProvider : LayerBrushProvider
    {
        public override void Enable()
        {
            RegisterLayerBrushDescriptor<LogitechWrapperLayerBrush>("Logitech Wrapper Layer", "Allows you to have Logitech Lightsync lighting on all devices.", "Robber");
        }

        public override void Disable()
        {
        }
    }
}

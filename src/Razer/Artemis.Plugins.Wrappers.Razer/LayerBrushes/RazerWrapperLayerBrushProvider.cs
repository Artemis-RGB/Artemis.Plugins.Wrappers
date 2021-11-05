using Artemis.Core;
using Artemis.Core.LayerBrushes;

namespace Artemis.Plugins.Wrappers.Razer.LayerBrushes
{
    [PluginFeature(Name = "Razer Wrapper Layer")]
    public class RazerWrapperLayerBrushProvider : LayerBrushProvider
    {
        public override void Enable()
        {
            RegisterLayerBrushDescriptor<RazerWrapperLayerBrush>("Razer Wrapper Layer", "Allows you to have Razer Chroma lighting on all devices.", "Snake");
        }

        public override void Disable()
        {
        }
    }
}

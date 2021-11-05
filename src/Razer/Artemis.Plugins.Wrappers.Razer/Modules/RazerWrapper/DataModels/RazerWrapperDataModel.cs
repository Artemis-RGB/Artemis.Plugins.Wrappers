using Artemis.Core.Modules;
using SkiaSharp;

namespace Artemis.Plugins.Wrappers.Razer.DataModels
{
    public class RazerWrapperDataModel : DataModel
    {
        public RazerWrapperLedsDataModel Leds { get; set; } = new();
    }

    public class RazerWrapperLedsDataModel : DataModel { }
}
using Artemis.Core.Modules;
using SkiaSharp;

namespace Artemis.Plugins.Wrappers.Razer.DataModels
{
    public class RazerWrapperDataModel : DataModel
    {
        public SKColor[] Keyboard { get; set; }
        public SKColor[] KeyboardExtended { get; set; }

        public SKColor[] Mouse { get; set; }
        public SKColor[] MouseExtended { get; set; }

        public SKColor[] Mousepad { get; set; }
        public SKColor[] MousepadExtended { get; set; }

        public SKColor[] Headset { get; set; }
        public SKColor[] Keypad { get; set; }
        public SKColor[] ChromaLink { get; set; }
    }
}
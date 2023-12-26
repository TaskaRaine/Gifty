namespace Gifty.Config
{
    class GiftyConfig
    {
        public bool UseThickRibbonGiftBoxModel = false;
        public GiftyConfig()
        {

        }
        public GiftyConfig(GiftyConfig previousConfig)
        {
            UseThickRibbonGiftBoxModel = previousConfig.UseThickRibbonGiftBoxModel;
        }
    }
}

using Gifty.Config;
using Vintagestory.API.Common;

namespace Gifty.Utility
{
    class ModConfig
    {
        private GiftyConfig config;

        public void ReadConfig(ICoreAPI api)
        {
            try
            {
                config = LoadConfig(api);

                if (config == null)
                {
                    GenerateConfig(api);
                    config = LoadConfig(api);
                }
                else
                {
                    GenerateConfig(api, config);
                }
            }
            catch
            {
                GenerateConfig(api);
                config = LoadConfig(api);
            }

            api.World.Config.SetBool("UseThickRibbonGiftBoxModel", config.UseThickRibbonGiftBoxModel);
        }
        private GiftyConfig LoadConfig(ICoreAPI api)
        {
            return api.LoadModConfig<GiftyConfig>("GiftyConfig.json");
        }
        private void GenerateConfig(ICoreAPI api)
        {
            api.StoreModConfig<GiftyConfig>(new GiftyConfig(), "GiftyConfig.json");
        }
        private void GenerateConfig(ICoreAPI api, GiftyConfig previousConfig)
        {
            api.StoreModConfig<GiftyConfig>(new GiftyConfig(previousConfig), "GiftyConfig.json");
        }
    }
}

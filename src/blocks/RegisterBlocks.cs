using Vintagestory.API.Common;

namespace Gifty.Blocks
{
    public class RegisterBlocks : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockClass("GBlockGiftBoxNoRibbon", typeof(BlockGiftBoxNoRibbon));
            api.RegisterBlockClass("GBlockGiftBoxRibbon", typeof(BlockGiftBoxRibbon));
        }
    }
}

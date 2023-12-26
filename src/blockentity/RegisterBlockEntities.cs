using Vintagestory.API.Common;

namespace Gifty.BlockEntities
{
    class RegisterBlockEntities : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockEntityClass("GBlockEntityGiftBox", typeof(BlockEntityGiftBox));
        }
    }
}

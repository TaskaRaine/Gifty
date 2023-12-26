using Gifty.BlockBehaviors;
using Vintagestory.API.Common;

namespace Gifty.BlockEntities
{
    class RegisterBlockBehaviors : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterBlockBehaviorClass("GBlockBehaviorContainerAllowRibbonInteract", typeof(BlockBehaviorContainerAllowRibbonInteract));
        }
    }
}

using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Gifty.BlockBehaviors
{
    class BlockBehaviorContainerAllowRibbonInteract : BlockBehaviorContainer
    {
        public BlockBehaviorContainerAllowRibbonInteract(Block block) : base(block)
        {
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if(byPlayer.Entity.Controls.Sneak)
                return false;

            if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);

            AssetLocation assetLocation = byPlayer.InventoryManager.ActiveHotbarSlot.Itemstack?.Collectible?.Code;

            if (assetLocation.Domain == "gifty" && assetLocation.FirstCodePart() == "ribbon")
            {
                return false;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }
    }
}

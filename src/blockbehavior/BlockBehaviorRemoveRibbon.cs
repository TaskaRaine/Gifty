using Gifty.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace Gifty.BlockBehaviors
{
    class BlockBehaviorRemoveRibbon : BlockBehaviorRightClickPickup
    {
        public BlockBehaviorRemoveRibbon(Block block) : base(block)
        {

        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            if (byPlayer is not EntityPlayer || !byPlayer.Entity.Controls.CtrlKey || !byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);

            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGiftBox giftBoxEntity)
            {
                Block giftBox = world.BlockAccessor.GetBlock(new AssetLocation("gifty", "giftbox-noribbon"));
                ItemStack giftBoxItemstack = new ItemStack(giftBox, 1);

                giftBoxItemstack.Attributes.SetString("boxbase", giftBoxEntity.GiftTextures["boxbase"].ToString());
                giftBoxItemstack.Attributes.SetString("boxlid", giftBoxEntity.GiftTextures["boxlid"].ToString());
                giftBoxItemstack.Attributes.SetItemstack("contents", giftBoxEntity.Inventory.FirstNonEmptySlot?.Itemstack);

                world.BlockAccessor.SetBlock(giftBox.Id, blockSel.Position, giftBoxItemstack);
                world.BlockAccessor.MarkBlockDirty(blockSel.Position);

                handling = EnumHandling.PreventDefault;

                return false;
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }
    }
}

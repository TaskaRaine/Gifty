using Gifty.BlockEntities;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Gifty.Blocks
{
    class BlockGiftBoxRibbon: BlockGiftBox
    {
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGiftBox giftboxEntity)
            {
                if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                {
                    ItemStack giftBoxItemstack = new ItemStack(this, 1);

                    if (!byPlayer.Entity.Controls.Sneak)
                    {
                        giftBoxItemstack.Attributes.SetString("boxbase", giftboxEntity.GiftTextures["boxbase"].ToString());
                        giftBoxItemstack.Attributes.SetString("boxlid", giftboxEntity.GiftTextures["boxlid"].ToString());
                        giftBoxItemstack.Attributes.SetString("ribbon", giftboxEntity.GiftTextures["ribbon"].ToString());
                        giftBoxItemstack.Attributes.SetItemstack("contents", giftboxEntity.Inventory.FirstNonEmptySlot?.Itemstack);

                        byPlayer.InventoryManager.TryGiveItemstack(giftBoxItemstack, true);

                        world.BlockAccessor.RemoveBlockEntity(blockSel.Position);
                        world.BlockAccessor.SetBlock(0, blockSel.Position);

                        return true;
                    }
                    else
                    {
                        Block giftBox = world.BlockAccessor.GetBlock(new AssetLocation("gifty", "giftbox-noribbon"));

                        giftBoxItemstack.Attributes.SetString("boxbase", giftboxEntity.GiftTextures["boxbase"].ToString());
                        giftBoxItemstack.Attributes.SetString("boxlid", giftboxEntity.GiftTextures["boxlid"].ToString());
                        giftBoxItemstack.Attributes.SetItemstack("contents", giftboxEntity.Inventory.FirstNonEmptySlot?.Itemstack);

                        world.BlockAccessor.SetBlock(giftBox.Id, blockSel.Position, giftBoxItemstack);
                        world.BlockAccessor.MarkBlockDirty(blockSel.Position);

                        return true;
                    }

                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
    }
}

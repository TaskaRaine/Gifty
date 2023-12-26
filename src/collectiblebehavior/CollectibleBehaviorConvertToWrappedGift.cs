using Gifty.BlockEntities;
using Gifty.Blocks;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Gifty.CollectibleBehaviors
{
    class CollectibleBehaviorConvertToWrappedGift : CollectibleBehavior
    {
        public CollectibleBehaviorConvertToWrappedGift(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null)
                return;

            ICoreAPI api = byEntity.Api;

            if (byEntity is EntityPlayer player)
            {
                if(api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGiftBox giftBoxEntity)
                {
                    Block giftBoxRibbon = api.World.BlockAccessor.GetBlock(new AssetLocation("gifty", "giftbox-ribbon"));
                    ItemStack giftBoxItemstack = new ItemStack(giftBoxRibbon, 1);

                    giftBoxItemstack.Attributes.SetString("boxbase", giftBoxEntity.GiftTextures["boxbase"].ToString());
                    giftBoxItemstack.Attributes.SetString("boxlid", giftBoxEntity.GiftTextures["boxlid"].ToString());
                    giftBoxItemstack.Attributes.SetString("ribbon", slot.Itemstack.Item.Textures["ribbon"].Base.ToString());
                    giftBoxItemstack.Attributes.SetItemstack("contents", giftBoxEntity.Inventory.FirstNonEmptySlot?.Itemstack);

                    api.World.BlockAccessor.SetBlock(giftBoxRibbon.Id, blockSel.Position, giftBoxItemstack);
                    api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                    handling = EnumHandling.PreventDefault;
                    handHandling = EnumHandHandling.PreventDefault;
                }
            }
        }
    }
}

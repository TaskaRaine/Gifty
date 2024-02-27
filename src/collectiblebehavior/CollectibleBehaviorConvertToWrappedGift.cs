using Gifty.BlockEntities;
using Gifty.Blocks;
using Gifty.Utility;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Gifty.CollectibleBehaviors
{
    class GCollectibleBehaviorConvertToWrappedGift : CollectibleBehavior
    {
        WorldInteraction[] wrapGiftInteraction = null;

        public GCollectibleBehaviorConvertToWrappedGift(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            wrapGiftInteraction = ObjectCacheUtil.GetOrCreate(api, "wrapGiftInteraction", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "gifty:blockhelp-giftbox-wrap",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            return wrapGiftInteraction.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
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

                    if (!giftBoxEntity.GiftCard.Equals(default(BlockEntityGiftBox.GiftCardProperties)))
                    {
                        giftBoxItemstack.Attributes.SetString("recipient", giftBoxEntity.GiftCard.Recipient);
                        giftBoxItemstack.Attributes.SetString("message", giftBoxEntity.GiftCard.Message);
                        giftBoxItemstack.Attributes.SetString("gifter", giftBoxEntity.GiftCard.Gifter);
                    }

                    api.World.BlockAccessor.SetBlock(giftBoxRibbon.Id, blockSel.Position, giftBoxItemstack);
                    api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                    handling = EnumHandling.PreventDefault;
                    handHandling = EnumHandHandling.PreventDefault;
                }
            }
        }
    }
}

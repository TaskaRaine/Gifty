﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Gifty.CollectibleBehaviors
{
    class GCollectibleBehaviorConvertToLiddedGift: CollectibleBehavior
    {
        WorldInteraction[] placeLidInteraction = null;

        public GCollectibleBehaviorConvertToLiddedGift(CollectibleObject collObj) : base(collObj)
        {

        }
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            placeLidInteraction = ObjectCacheUtil.GetOrCreate(api, "placeLidInteraction", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "gifty:blockhelp-giftboxlid-placelid",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot, ref EnumHandling handling)
        {
            return placeLidInteraction.Append(base.GetHeldInteractionHelp(inSlot, ref handling));
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handHandling, ref EnumHandling handling)
        {
            if (blockSel == null)
                return;

            ICoreAPI api = byEntity.Api;

            if (byEntity is EntityPlayer player)
            {
                Block interactedBlock = api.World.BlockAccessor.GetBlock(blockSel.Position, BlockLayersAccess.SolidBlocks);

                if (interactedBlock is BlockGroundStorage)
                {
                    if (api.World.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGroundStorage groundStorageEntity)
                    {
                        if (groundStorageEntity.TotalStackSize == 1 && groundStorageEntity.Inventory.FirstNonEmptySlot.Itemstack.Collectible.Code.Domain == "gifty" && groundStorageEntity.Inventory.FirstNonEmptySlot.Itemstack.Collectible.Code.FirstCodePart() == "giftboxbase")
                        {
                            api.World.BlockAccessor.RemoveBlockEntity(blockSel.Position);

                            Block giftbox = api.World.BlockAccessor.GetBlock(new AssetLocation("gifty", "giftbox-noribbon"));
                            ItemStack giftBoxItemstack = new ItemStack(giftbox, 1);

                            giftBoxItemstack.Attributes.SetString("boxbase", groundStorageEntity.Inventory.FirstNonEmptySlot.Itemstack.Block.Textures["boxbase"].Base.ToString());
                            giftBoxItemstack.Attributes.SetString("boxlid", slot.Itemstack.Block.Textures["boxlid"].Base.ToString());

                            api.World.BlockAccessor.SetBlock(giftbox.Id, blockSel.Position, giftBoxItemstack);
                            //api.World.BlockAccessor.SpawnBlockEntity("GBlockEntityGiftBox", blockSel.Position, giftBoxItemstack);
                            api.World.BlockAccessor.MarkBlockDirty(blockSel.Position);

                            player.Player.InventoryManager.ActiveHotbarSlot.TakeOut(1);

                            handling = EnumHandling.PreventDefault;
                            handHandling = EnumHandHandling.PreventDefault;
                        }
                    }
                    return;
                }
            }

            handling = EnumHandling.PassThrough;
        }
    }
}

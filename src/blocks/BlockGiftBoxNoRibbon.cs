using Gifty.BlockEntities;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Util;

namespace Gifty.Blocks
{
    class BlockGiftBoxNoRibbon: BlockGiftBox
    {
        WorldInteraction[] wrapGiftNoCardInteraction = null;
        WorldInteraction[] wrapGiftWithCardInteraction = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            List<ItemStack> ribbons = new List<ItemStack>();

            foreach (Item item in api.World.Items)
            {
                if (item.Code == null) continue;

                if (item.Code.BeginsWith("gifty", "ribbon"))
                    ribbons.Add(new ItemStack(item));
            }

            wrapGiftNoCardInteraction = ObjectCacheUtil.GetOrCreate(api, "blockWrapNoCardGiftInteraction", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-openbox",
                        MouseButton = EnumMouseButton.Right
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-wrap",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = ribbons.ToArray()
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-attachcard",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = new ItemStack[] { new ItemStack(api.World.GetItem(new AssetLocation("gifty", "giftcard-signed"))) }
                    }
                };
            });
            wrapGiftWithCardInteraction = ObjectCacheUtil.GetOrCreate(api, "blockWrapWithCardGiftInteraction", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-openbox",
                        MouseButton = EnumMouseButton.Right
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-wrap",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = ribbons.ToArray()
                    }
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if(world.BlockAccessor.GetBlockEntity(selection.Position) is BlockEntityGiftBox giftBox)
            {
                if (giftBox.GiftCard.Equals(default(BlockEntityGiftBox.GiftCardProperties)))
                    return wrapGiftNoCardInteraction.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }

            return wrapGiftWithCardInteraction.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
        }
    }
}

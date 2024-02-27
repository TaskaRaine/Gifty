using Gifty.BlockEntities;
using Vintagestory.API.Common;
using Gifty.Utility;
using Vintagestory.API.MathTools;
using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Util;
using Vintagestory.API.Client;

namespace Gifty.Blocks
{
    class BlockGiftBoxRibbon: BlockGiftBox
    {
        WorldInteraction[] recipientInteractions = null;
        WorldInteraction[] recipientNoTakeInteractions = null;
        WorldInteraction[] gifterInteractions = null;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            recipientInteractions = ObjectCacheUtil.GetOrCreate(api, "giftRecipientInteractions", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-opengift",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true,
                        HotKeyCode = "sneak"
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-take",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true
                    }
                };
            });
            recipientNoTakeInteractions = ObjectCacheUtil.GetOrCreate(api, "giftRecipientNoTakeInteractions", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-opengift",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true,
                        HotKeyCode = "sneak"
                    }
                };
            });
            gifterInteractions = ObjectCacheUtil.GetOrCreate(api, "giftGifterInteractions", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-removeribbon",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true,
                        HotKeyCodes = new string[] { "sprint", "sneak" }
                    },
                    new WorldInteraction()
                    {
                        ActionLangCode = "gifty:blockhelp-giftbox-take",
                        MouseButton = EnumMouseButton.Right,
                        RequireFreeHand = true
                    }
                };
            });
        }
        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer)
        {
            if (world.BlockAccessor.GetBlockEntity(selection.Position) is BlockEntityGiftBox giftBox)
            {
                if (giftBox.GiftCard.Equals(default(BlockEntityGiftBox.GiftCardProperties)))
                    return recipientInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                else if (giftBox.GiftCard.Gifter == forPlayer.PlayerName && giftBox.GiftCard.Recipient == forPlayer.PlayerName)
                    return recipientNoTakeInteractions.Append(gifterInteractions).Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                else if (giftBox.GiftCard.Recipient == forPlayer.PlayerName)
                    return recipientInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
                else if (giftBox.GiftCard.Gifter == forPlayer.PlayerName)
                    return gifterInteractions.Append(base.GetPlacedBlockInteractionHelp(world, selection, forPlayer));
            }
            
            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer);
        }
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

                        if (!giftboxEntity.GiftCard.Equals(default(BlockEntityGiftBox.GiftCardProperties)))
                        {
                            giftBoxItemstack.Attributes.SetString("recipient", giftboxEntity.GiftCard.Recipient);
                            giftBoxItemstack.Attributes.SetString("message", giftboxEntity.GiftCard.Message);
                            giftBoxItemstack.Attributes.SetString("gifter", giftboxEntity.GiftCard.Gifter);
                        }

                        byPlayer.InventoryManager.TryGiveItemstack(giftBoxItemstack, true);

                        world.BlockAccessor.RemoveBlockEntity(blockSel.Position);
                        world.BlockAccessor.SetBlock(0, blockSel.Position);

                        return true;
                    }
                    else
                    {
                        if (!byPlayer.Entity.Controls.Sprint && GiftyUtilityMethods.PlayerCanInteractAs(byPlayer.PlayerName, giftboxEntity.GiftCard.Recipient))
                        {
                            if(api.Side == EnumAppSide.Client)
                                giftboxEntity.PlayOpenSound();

                            return true;
                        }
                        else if (byPlayer.Entity.Controls.Sprint && GiftyUtilityMethods.PlayerCanInteractAs(byPlayer.PlayerName, giftboxEntity.GiftCard.Gifter))
                        {
                            Block giftBox = world.BlockAccessor.GetBlock(new AssetLocation("gifty", "giftbox-noribbon"));

                            giftBoxItemstack.Attributes.SetString("boxbase", giftboxEntity.GiftTextures["boxbase"].ToString());
                            giftBoxItemstack.Attributes.SetString("boxlid", giftboxEntity.GiftTextures["boxlid"].ToString());
                            giftBoxItemstack.Attributes.SetItemstack("contents", giftboxEntity.Inventory.FirstNonEmptySlot?.Itemstack);

                            if (!giftboxEntity.GiftCard.Equals(default(BlockEntityGiftBox.GiftCardProperties)))
                            {
                                giftBoxItemstack.Attributes.SetString("recipient", giftboxEntity.GiftCard.Recipient);
                                giftBoxItemstack.Attributes.SetString("message", giftboxEntity.GiftCard.Message);
                                giftBoxItemstack.Attributes.SetString("gifter", giftboxEntity.GiftCard.Gifter);
                            }

                            world.BlockAccessor.SetBlock(giftBox.Id, blockSel.Position, giftBoxItemstack);
                            world.BlockAccessor.MarkBlockDirty(blockSel.Position);

                            return true;
                        }

                        return false;
                    }
                }
            }
            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGiftBox giftboxEntity)
            {
                if (GiftyUtilityMethods.PlayerCanInteractAs(byPlayer.PlayerName, giftboxEntity.GiftCard.Recipient))
                {
                    return giftboxEntity.OpenGift(secondsUsed, byPlayer, blockSel.Position);
                }
            }
                return base.OnBlockInteractStep(secondsUsed, world, byPlayer, blockSel);
        }
        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (world.BlockAccessor.GetBlockEntity(blockSel.Position) is BlockEntityGiftBox giftboxEntity)
            {
                if (GiftyUtilityMethods.PlayerCanInteractAs(byPlayer.PlayerName, giftboxEntity.GiftCard.Recipient))
                {
                    giftboxEntity.BreakGift(secondsUsed, byPlayer, blockSel.Position);
                }
            }
        }
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityGiftBox giftboxEntity)
            {
                if (!giftboxEntity.Inventory[0].Empty && dropQuantityMultiplier == 0)
                {
                    Random rand = new Random();

                    int xSign = rand.Next(0, 2);
                    int zSign = rand.Next(0, 2);

                    double xVelocity = rand.NextDouble() / 12;
                    double zVelocity = rand.NextDouble() / 12;
                    double yVelociity = (rand.NextDouble() + 0.25) / 6;

                    if (xSign == 0)
                        xVelocity = -xVelocity;
                    if (zSign == 0)
                        zVelocity = -zVelocity;

                    Vec3d spawnPos = new Vec3d(pos.X + 0.5, pos.Y, pos.Z + 0.5);

                    Entity entity = world.SpawnItemEntity(giftboxEntity.Inventory[0].Itemstack, spawnPos, new Vec3d(xVelocity, yVelociity, zVelocity));
                    giftboxEntity.Inventory.Clear();

                    return null;
                }
            }
            return base.GetDrops(world, pos, byPlayer, dropQuantityMultiplier);
        }
    }
}

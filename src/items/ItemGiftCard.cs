using Gifty.Blocks;
using Gifty.GUI;
using Gifty.ModSystems;
using System.Reflection.Metadata;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Gifty.Items
{
    
    class ItemGiftCard : Item
    {
        const string NETWORK_CHANNEL_ID = "GiftyGiftCardChannel";

        WorldInteraction[] signInteraction = null;
        WorldInteraction[] readInteraction = null;

        private GUIDialogGiftCard CardGUI { get; set; }
        private GiftCardModSystem GCModSystem { get; set; }
        
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            signInteraction = ObjectCacheUtil.GetOrCreate(api, "signCardInteraction", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "gifty:itemhelp-giftcard-signcard",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });
            readInteraction = ObjectCacheUtil.GetOrCreate(api, "readCardInteraction", () =>
            {
                return new WorldInteraction[] {
                    new WorldInteraction()
                        {
                            ActionLangCode = "gifty:itemhelp-giftcard-readcard",
                            MouseButton = EnumMouseButton.Right
                        },
                };
            });

            GCModSystem = api.ModLoader.GetModSystem<GiftCardModSystem>();
        }
        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            if (CodeWithVariant("state", "signed") == Code)
                return readInteraction.Append(base.GetHeldInteractionHelp(inSlot));
            else
                return signInteraction.Append(base.GetHeldInteractionHelp(inSlot));
        }
        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (byEntity is not EntityPlayer player)
                return;

            if (api.Side == EnumAppSide.Client)
            {
                if (CodeWithVariant("state", "signed") == Code)
                {
                    CardGUI = new GUIDialogGiftCard(api as ICoreClientAPI, slot.Itemstack.Attributes["recipient"].ToString(), slot.Itemstack.Attributes["message"].ToString());

                    CardGUI.TryOpen();
                    handling = EnumHandHandling.PreventDefault;
                }
                else if (IsWritingTool(byEntity.LeftHandItemSlot))
                {
                    CardGUI = new GUIDialogGiftCard(api as ICoreClientAPI);

                    CardGUI.TryOpen();

                    CardGUI.OnClosed += () =>
                    {
                        if (CardGUI.IsSigned)
                        {
                            GCModSystem.SignCardClient(CardGUI.Recipient, CardGUI.Message, player.Player.PlayerName);
                            slot.TakeOut(1);
                        }
                    };

                    handling = EnumHandHandling.PreventDefault;
                }
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }
        private bool IsWritingTool(ItemSlot slot)
        {
            return slot.Itemstack?.Collectible.Attributes?.IsTrue("writingTool") == true;
        }
    }
}

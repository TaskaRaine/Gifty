using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using ProtoBuf;
using System.Reflection.Metadata.Ecma335;

namespace Gifty.ModSystems
{
    class GiftCardModSystem: ModSystem
    {
        const string NETWORK_CHANNEL_ID = "GiftyGiftCardChannel";

        [ProtoContract]
        public class GiftCardPacket
        {
            [ProtoMember(1)]
            public string Recipient;
            [ProtoMember(2)]
            public string Message;
            [ProtoMember(3)]
            public string Gifter;
        }
        private ICoreAPI Api { get; set; }

        public override void Start(ICoreAPI api)
        {
            Api = api;

            Api.Network.RegisterChannel(NETWORK_CHANNEL_ID)
                .RegisterMessageType<GiftCardPacket>();

            if (api is ICoreServerAPI sapi)
                sapi.Network.GetChannel(NETWORK_CHANNEL_ID)
                    .SetMessageHandler<GiftCardPacket>(SignCardServer);
        }
        public void SignCardClient(string recipient, string message, string gifter)
        {
            if (Api is ICoreClientAPI capi)
                capi.Network.GetChannel(NETWORK_CHANNEL_ID)
                    .SendPacket<GiftCardPacket>(new GiftCardPacket() { Recipient = recipient, Message = message, Gifter = gifter });
        }
        private void SignCardServer(IServerPlayer fromPlayer, GiftCardPacket packet)
        {
            ItemStack signedCard = new ItemStack(Api.World.GetItem(new AssetLocation("gifty", "giftcard-signed")));

            signedCard.Attributes.SetString("recipient", packet.Recipient);
            signedCard.Attributes.SetString("message", packet.Message);
            signedCard.Attributes.SetString("gifter", packet.Gifter);

            fromPlayer.Entity.WalkInventory((slot) =>
            {
                if (slot.Empty)
                    return true;

                if (slot.Itemstack?.Collectible.Attributes?["signableGiftCard"].AsBool() == true)
                {
                    if(slot.TakeOut(1).StackSize > 0)
                    {
                        if (!fromPlayer.InventoryManager.TryGiveItemstack(signedCard, true))
                        {
                            fromPlayer.Entity.World.SpawnItemEntity(signedCard, fromPlayer.Entity.Pos.XYZ);
                        }

                        return false;
                    }
                }

                return true;
            });
        }
    }
}

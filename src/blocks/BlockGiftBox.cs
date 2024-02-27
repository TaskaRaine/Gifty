using Gifty.BlockEntities;
using Gifty.Items;
using Gifty.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using static Gifty.BlockEntities.BlockEntityGiftBox;

namespace Gifty.Blocks
{
    class BlockGiftBox : Block, ITexPositionSource
    {
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                TextureAtlasPosition texpos = null;
                AssetLocation texturePath = null;

                if (GiftTextures.ContainsKey(textureCode))
                    texturePath = GiftTextures.Get(textureCode);

                if (texturePath == null)
                    texturePath = new AssetLocation("game", "unknown");

                Capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos);

                return texpos;
            }
        }

        public ICoreClientAPI Capi { get; set; }
        //public ItemStack giftItemstack { get; set; }

        public Dictionary<string, AssetLocation> GiftTextures = new Dictionary<string, AssetLocation>
        {
            { "boxbase", new AssetLocation("game", "block/christmas/present/dogwoodrose") },
            { "boxlid", new AssetLocation("game", "block/christmas/present/dogwoodrose") },
            { "ribbon", new AssetLocation("game", "block/cloth/linen/green") }
        };

        public Size2i AtlasSize => Capi.BlockTextureAtlas.Size;
        public BlockEntityGiftBox GiftBoxEntity { get; set; }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            if (api.Side == EnumAppSide.Client)
                Capi = (ICoreClientAPI)api;
        }
        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            StringBuilder stringBuilder = new StringBuilder();

            string lidType = GiftTextures["boxlid"].ToString().Split('/').Last();
            string basetype = GiftTextures["boxbase"].ToString().Split('/').Last();

            stringBuilder.Append(base.GetPlacedBlockInfo(world, pos, forPlayer));

            if (GiftBoxEntity != null)
            {
                if (!string.IsNullOrEmpty(GiftBoxEntity.GiftCard.Recipient))
                {
                    stringBuilder.Append(GiftBoxEntity.GiftCard.Recipient);
                    stringBuilder.Append("\n");
                }
                if (!string.IsNullOrEmpty(GiftBoxEntity.GiftCard.Message))
                {
                    stringBuilder.Append(GiftBoxEntity.GiftCard.Message);
                    stringBuilder.Append("\n");
                }
                if (!string.IsNullOrEmpty(GiftBoxEntity.GiftCard.Gifter))
                {
                    stringBuilder.Append(Lang.Get("gifty:blockinfo-giftcardfrom") + GiftBoxEntity.GiftCard.Gifter);
                    stringBuilder.Append("\n");
                }
            }

            AppendArtistCredits(ref stringBuilder, lidType, basetype);

            return stringBuilder.ToString();
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            string lidType = inSlot.Itemstack.Attributes["boxlid"].ToString().Split('/').Last();
            string basetype = inSlot.Itemstack.Attributes["boxbase"].ToString().Split('/').Last();

            if (inSlot.Itemstack.Attributes.HasAttribute("recipient") && inSlot.Itemstack.Attributes["recipient"].ToString() != string.Empty)
            {
                dsc.Append(inSlot.Itemstack.Attributes["recipient"].ToString());
                dsc.Append("\n");
            }
            if (inSlot.Itemstack.Attributes.HasAttribute("message") && inSlot.Itemstack.Attributes["message"].ToString() != string.Empty)
            {
                dsc.Append(inSlot.Itemstack.Attributes["message"].ToString());
                dsc.Append("\n");
            }
            if (inSlot.Itemstack.Attributes.HasAttribute("gifter") && inSlot.Itemstack.Attributes["gifter"].ToString() != string.Empty)
            {
                dsc.Append(inSlot.Itemstack.Attributes["gifter"].ToString());
                dsc.Append("\n");
            }

            AppendArtistCredits(ref dsc, lidType, basetype);
        }
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            if (itemstack.Attributes == null || itemstack.Attributes.Count == 0)
                return;

            string cacheKey = "giftboxMeshRefs" + Code.Domain + FirstCodePart();
            Dictionary<string, MeshRef> giftboxMeshRefs = ObjectCacheUtil.GetOrCreate(capi, cacheKey, () => new Dictionary<string, MeshRef>());

            GiftTextures["boxbase"] = new AssetLocation(itemstack.Attributes["boxbase"].ToString());
            GiftTextures["boxlid"] = new AssetLocation(itemstack.Attributes["boxlid"].ToString());
            GiftTextures["ribbon"] = new AssetLocation(itemstack.Attributes["ribbon"].ToString());

            if (!giftboxMeshRefs.TryGetValue(cacheKey, out renderinfo.ModelRef.meshrefs[0]))
            {
                MeshData mesh = GenMesh();
                giftboxMeshRefs[cacheKey] = renderinfo.ModelRef.meshrefs[0] = capi.Render.UploadMesh(mesh);
            }
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        }
        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
        {
            if (GiftBoxEntity != null)
            {
                return capi.BlockTextureAtlas.GetRandomColor(GiftBoxEntity["boxbase"], -1);
            }

            return base.GetRandomColor(capi, pos, facing, rndIndex);
        }
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (dropQuantityMultiplier == 0)
                return null;

            ItemStack giftBoxDrop = new ItemStack(this, 1);

            giftBoxDrop.Attributes.SetString("boxbase", GiftTextures["boxbase"].ToString());
            giftBoxDrop.Attributes.SetString("boxlid", GiftTextures["boxlid"].ToString());
            giftBoxDrop.Attributes.SetString("ribbon", GiftTextures["ribbon"].ToString());

            if (GiftBoxEntity != null)
            {
                giftBoxDrop.Attributes.SetItemstack("contents", GiftBoxEntity.Inventory.FirstNonEmptySlot?.Itemstack);

                if (!GiftBoxEntity.GiftCard.Equals(default(BlockEntityGiftBox.GiftCardProperties)))
                {
                    giftBoxDrop.Attributes.SetString("recipient", GiftBoxEntity.GiftCard.Recipient);
                    giftBoxDrop.Attributes.SetString("message", GiftBoxEntity.GiftCard.Message);
                    giftBoxDrop.Attributes.SetString("gifter", GiftBoxEntity.GiftCard.Gifter);
                }
            }

            return new ItemStack[] { giftBoxDrop };
        }
        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel)
        {
            if (byPlayer.InventoryManager.ActiveHotbarSlot.Empty)
                return base.OnBlockInteractStart(world, byPlayer, blockSel);

            ItemSlot giftCardSlot = byPlayer.InventoryManager.ActiveHotbarSlot;

            if(giftCardSlot.Itemstack.Collectible is ItemGiftCard)
                if (giftCardSlot.Itemstack.Collectible.Code == giftCardSlot.Itemstack.Collectible.CodeWithVariant("state", "signed"))
                {
                    if (GiftBoxEntity != null)
                    {
                        if(GiftBoxEntity.GiftCard.Equals(default(GiftCardProperties)) || GiftyUtilityMethods.PlayerCanInteractAs(byPlayer.PlayerName, GiftBoxEntity.GiftCard.Recipient) || GiftyUtilityMethods.PlayerCanInteractAs(byPlayer.PlayerName, GiftBoxEntity.GiftCard.Gifter))
                        {
                            GiftBoxEntity.GiftCard = new BlockEntityGiftBox.GiftCardProperties(giftCardSlot.Itemstack.Attributes["recipient"].ToString(), giftCardSlot.Itemstack.Attributes["message"].ToString(), giftCardSlot.Itemstack.Attributes["gifter"].ToString());

                            giftCardSlot.TakeOut(1);
                            return true;
                        }
                    }
                }

            return base.OnBlockInteractStart(world, byPlayer, blockSel);
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            UnloadMeshReferences();

            base.OnBlockBroken(world, pos, byPlayer, dropQuantityMultiplier);
        }
        public MeshData GenMesh()
        {
            Capi.Tesselator.TesselateShape("giftbox", Capi.TesselatorManager.GetCachedShape(Shape.Base), out MeshData mesh, this);

            return mesh;
        }
        private void UnloadMeshReferences()
        {
            if (api.Side == EnumAppSide.Client)
            {
                string cacheKey = "giftboxMeshRefs" + Code.Domain + FirstCodePart();
                Dictionary<string, MeshRef> giftboxMeshRefs = ObjectCacheUtil.GetOrCreate(Capi, cacheKey, () => new Dictionary<string, MeshRef>());

                if (giftboxMeshRefs != null)
                {
                    foreach (KeyValuePair<string, MeshRef> reference in giftboxMeshRefs)
                    {
                        reference.Value.Dispose();
                    }

                    api.ObjectCache.Remove(cacheKey);
                }
            }
        }
        private void AppendArtistCredits(ref StringBuilder stringBuilder, string lidType, string baseType)
        {
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("<font color=\"#99c9f9\"><i>" + Lang.Get("gifty:block-giftboxlid-" + lidType) + Lang.Get("gifty:blockinfo-designedby", GGiftBoxArtists.GetGiftBoxArtist(lidType)) + "</i></font>");
            stringBuilder.AppendLine("<font color=\"#99c9f9\"><i>" + Lang.Get("gifty:block-giftboxbase-" + baseType) + Lang.Get("gifty:blockinfo-designedby", GGiftBoxArtists.GetGiftBoxArtist(baseType)) + "</i></font>");
        }
    }
}

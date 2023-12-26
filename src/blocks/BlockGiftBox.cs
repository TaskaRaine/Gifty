using Gifty.BlockEntities;
using Gifty.Utility;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Gifty.Blocks
{
    class BlockGiftBox: Block, ITexPositionSource
    {
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                TextureAtlasPosition texpos = null;
                AssetLocation texturePath = null;

                if (GiftTextures.ContainsKey(textureCode))
                    texturePath = GiftTextures.Get(textureCode);

                if(texturePath == null)
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
            stringBuilder.Append("\n");
            stringBuilder.AppendLine("<font color=\"#99c9f9\"><i>" + Lang.Get("gifty:block-giftboxlid-" + lidType) + Lang.Get("gifty:designedby", GGiftBoxArtists.GetGiftBoxArtist(lidType)) + "</i></font>");
            stringBuilder.AppendLine("<font color=\"#99c9f9\"><i>" + Lang.Get("gifty:block-giftboxbase-" + basetype) + Lang.Get("gifty:designedby", GGiftBoxArtists.GetGiftBoxArtist(basetype)) + "</i></font>");

            return stringBuilder.ToString();
        }
        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            string lidType = inSlot.Itemstack.Attributes["boxlid"].ToString().Split('/').Last();
            string basetype = inSlot.Itemstack.Attributes["boxlid"].ToString().Split('/').Last();

            dsc.Append("\n");
            dsc.AppendLine("<font color=\"#99c9f9\"><i>" + Lang.Get("gifty:block-giftboxlid-" + lidType) + Lang.Get("gifty:designedby", GGiftBoxArtists.GetGiftBoxArtist(lidType)) + "</i></font>");
            dsc.AppendLine("<font color=\"#99c9f9\"><i>" + Lang.Get("gifty:block-giftboxbase-" + basetype) + Lang.Get("gifty:designedby", GGiftBoxArtists.GetGiftBoxArtist(basetype)) + "</i></font>");
        }
        public override void OnBeforeRender(ICoreClientAPI capi, ItemStack itemstack, EnumItemRenderTarget target, ref ItemRenderInfo renderinfo)
        {
            string cacheKey = "giftboxMeshRefs" + Code.Domain + FirstCodePart();
            Dictionary<string, MeshRef> giftboxMeshRefs = ObjectCacheUtil.GetOrCreate(capi, cacheKey, () => new Dictionary<string, MeshRef>());

            GiftTextures["boxbase"] = new AssetLocation(itemstack.Attributes["boxbase"].ToString());
            GiftTextures["boxlid"] = new AssetLocation(itemstack.Attributes["boxlid"].ToString());
            GiftTextures["ribbon"] = new AssetLocation(itemstack.Attributes["ribbon"].ToString());

            if (!giftboxMeshRefs.TryGetValue(cacheKey, out renderinfo.ModelRef))
            {
                MeshData mesh = GenMesh();
                giftboxMeshRefs[cacheKey] = renderinfo.ModelRef = capi.Render.UploadMesh(mesh);
            }
            base.OnBeforeRender(capi, itemstack, target, ref renderinfo);
        }
        public override int GetRandomColor(ICoreClientAPI capi, BlockPos pos, BlockFacing facing, int rndIndex = -1)
        {
            if (capi.World.BlockAccessor.GetBlockEntity(pos) is BlockEntityGiftBox giftboxEntity)
            {
                return capi.BlockTextureAtlas.GetRandomColor(giftboxEntity["boxbase"], -1);
            }

            return base.GetRandomColor(capi, pos, facing, rndIndex);
        }
        public override ItemStack[] GetDrops(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            ItemStack giftBoxDrop = new ItemStack(this, 1);

            giftBoxDrop.Attributes.SetString("boxbase", GiftTextures["boxbase"].ToString());
            giftBoxDrop.Attributes.SetString("boxlid", GiftTextures["boxlid"].ToString());
            giftBoxDrop.Attributes.SetString("ribbon", GiftTextures["ribbon"].ToString());

            return new ItemStack[] { giftBoxDrop };
        }
        public override void OnBlockBroken(IWorldAccessor world, BlockPos pos, IPlayer byPlayer, float dropQuantityMultiplier = 1)
        {
            if (world.BlockAccessor.GetBlockEntity(pos) is BlockEntityGiftBox giftboxEntity)
            {
                giftboxEntity.OnBlockBroken(byPlayer);
            }

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
    }
}

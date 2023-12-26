using Gifty.Blocks;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace Gifty.BlockEntities
{
    class BlockEntityGiftBox: BlockEntityGenericContainer, ITexPositionSource
    {
        public TextureAtlasPosition this[string textureCode]
        {
            get
            {
                AssetLocation texturePath = null;
                TextureAtlasPosition texpos = null;

                if (GiftTextures.ContainsKey(textureCode))
                    texturePath = GiftTextures.Get(textureCode);

                if (texturePath == null)
                    texturePath = new AssetLocation("game", "unknown");

                Capi.BlockTextureAtlas.GetOrInsertTexture(texturePath, out _, out texpos);

                return texpos;
            }
        }
        public ICoreClientAPI Capi { get; set; }

        public Size2i AtlasSize => Capi.BlockTextureAtlas.Size;
        public Dictionary<string, AssetLocation> GiftTextures = new Dictionary<string, AssetLocation>
        {
            { "boxbase", new AssetLocation("game", "block/christmas/present/dogwoodrose") },
            { "boxlid", new AssetLocation("game", "block/christmas/present/dogwoodrose") },
            { "ribbon", new AssetLocation("game", "block/cloth/linen/green") }
        };
        public MeshData mesh;

        public override void Initialize(ICoreAPI api)
        {
            Capi = api as ICoreClientAPI;

            base.Initialize(api);

            UpdateShape();
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetString("boxbase", GiftTextures["boxbase"].ToString());
            tree.SetString("boxlid", GiftTextures["boxlid"].ToString());
            tree.SetString("ribbon", GiftTextures["ribbon"].ToString());
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            GiftTextures["boxbase"] = new AssetLocation(tree.GetString("boxbase", GiftTextures["boxbase"].ToString()));
            GiftTextures["boxlid"] = new AssetLocation(tree.GetString("boxlid", GiftTextures["boxlid"].ToString()));

            if(tree.HasAttribute("ribbon"))
                GiftTextures["ribbon"] = new AssetLocation(tree.GetString("ribbon", GiftTextures["ribbon"].ToString()));

            UpdateBlockGiftTextures();
        }
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            GiftTextures["boxbase"] = new AssetLocation(byItemStack.Attributes["boxbase"].ToString());
            GiftTextures["boxlid"] = new AssetLocation(byItemStack.Attributes["boxlid"].ToString());

            if (byItemStack.Attributes.HasAttribute("ribbon"))
                GiftTextures["ribbon"] = new AssetLocation(byItemStack.Attributes["ribbon"].ToString());

            if (byItemStack.Attributes.HasAttribute("contents") && byItemStack.Attributes["contents"].GetValue() != null)
            {
                Inventory[0].Itemstack = byItemStack.Attributes.GetItemstack("contents");
                Inventory[0].Itemstack.ResolveBlockOrItem(this.Api.World);
                Inventory.MarkSlotDirty(0);
            }

            UpdateShape();
            UpdateBlockGiftTextures();

            MarkDirty(true);

            base.OnBlockPlaced(byItemStack);
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            if (mesh != null)
                mesher.AddMeshData(mesh);

            return true;
        }
        private void UpdateShape()
        {
            if (Api.Side == EnumAppSide.Client)
            {
                IAsset asset = Capi.Assets.TryGet("gifty:shapes/" + Block.Shape.Base.Path + ".json");

                if (asset != null)
                {
                    Shape currentShape = asset.ToObject<Shape>();
                    Capi.Tesselator.TesselateShape("container", currentShape, out MeshData wholeMesh, this);

                    mesh = wholeMesh;
                }
            }
        }
        private void UpdateBlockGiftTextures()
        {
            if(Block is BlockGiftBox block)
            {
                block.GiftTextures = GiftTextures;
            }
        }
    }
}

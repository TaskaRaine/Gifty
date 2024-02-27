using Gifty.Blocks;
using System;
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
    class BlockEntityGiftBox : BlockEntityGenericContainer, ITexPositionSource
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
        public struct GiftCardProperties
        {
            public string Recipient;
            public string Message;
            public string Gifter;

            public GiftCardProperties(string recipient, string message, string gifter)
            {
                Recipient = recipient;
                Message = message;
                Gifter = gifter;
            }
        }
        public ICoreClientAPI Capi { get; set; }
        public GiftCardProperties GiftCard;
        public Size2i AtlasSize => Capi.BlockTextureAtlas.Size;
        public Dictionary<string, AssetLocation> GiftTextures = new Dictionary<string, AssetLocation>
        {
            { "boxbase", new AssetLocation("game", "block/christmas/present/dogwoodrose") },
            { "boxlid", new AssetLocation("game", "block/christmas/present/dogwoodrose") },
            { "ribbon", new AssetLocation("game", "block/cloth/linen/green") }
        };
        public MeshData mesh;

        public BlockEntityAnimationUtil animUtil
        {
            get { return GetBehavior<BEBehaviorAnimatable>()?.animUtil; }
        }

        private int GiftOpenDuration { get; set; } = 2;
        private SimpleParticleProperties GiftOpenParticles { get; set; }
        private ILoadedSound GiftOpenSound { get; set; }

        public override void Initialize(ICoreAPI api)
        {
            Capi = api as ICoreClientAPI;
            
            if(Block is BlockGiftBox box)
            {
                box.GiftBoxEntity = this;
            }

            base.Initialize(api);

            if(api is  ICoreClientAPI clientAPI) {
                InitializetGiftParticles();
                InitializeGiftOpenSound();
                UpdateBlockGiftTextures();
                UpdateParticleColor();

                Inventory.OnInventoryOpened += AnimateOpen;
                Inventory.OnInventoryClosed += AnimateClose;
            }
        }
        public override void OnBlockRemoved()
        {
            base.OnBlockRemoved();

            if (GiftOpenSound != null)
            {
                GiftOpenSound.Stop();
                GiftOpenSound.Dispose();
            }
        }
        public override void OnBlockUnloaded()
        {
            base.OnBlockUnloaded();

            if (GiftOpenSound != null)
            {
                GiftOpenSound.Stop();
                GiftOpenSound.Dispose();
            }
        }
        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);

            tree.SetString("boxbase", GiftTextures["boxbase"].ToString());
            tree.SetString("boxlid", GiftTextures["boxlid"].ToString());
            tree.SetString("ribbon", GiftTextures["ribbon"].ToString());

            if(!GiftCard.Equals(default(GiftCardProperties)))
            {
                tree.SetString("recipient", GiftCard.Recipient);
                tree.SetString("message", GiftCard.Message);
                tree.SetString("gifter", GiftCard.Gifter);
            }
        }
        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);

            GiftTextures["boxbase"] = new AssetLocation(tree.GetString("boxbase", GiftTextures["boxbase"].ToString()));
            GiftTextures["boxlid"] = new AssetLocation(tree.GetString("boxlid", GiftTextures["boxlid"].ToString()));

            if(tree.HasAttribute("ribbon"))
                GiftTextures["ribbon"] = new AssetLocation(tree.GetString("ribbon", GiftTextures["ribbon"].ToString()));

            if (tree.HasAttribute("recipient"))
                GiftCard.Recipient = tree.GetString("recipient");

            if(tree.HasAttribute("message"))
                GiftCard.Message = tree.GetString("message");

            if (tree.HasAttribute("gifter"))
                GiftCard.Gifter = tree.GetString("gifter");

            UpdateBlockGiftTextures();
        }
        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            GiftTextures["boxbase"] = new AssetLocation(byItemStack.Attributes["boxbase"].ToString());
            GiftTextures["boxlid"] = new AssetLocation(byItemStack.Attributes["boxlid"].ToString());

            if (byItemStack.Attributes.HasAttribute("ribbon"))
                GiftTextures["ribbon"] = new AssetLocation(byItemStack.Attributes["ribbon"].ToString());

            if (byItemStack.Attributes.HasAttribute("recipient"))
                GiftCard.Recipient = byItemStack.Attributes["recipient"].ToString();

            if(byItemStack.Attributes.HasAttribute("message"))
                GiftCard.Message = byItemStack.Attributes["message"].ToString();

            if (byItemStack.Attributes.HasAttribute("gifter"))
                GiftCard.Gifter = byItemStack.Attributes["gifter"].ToString();

            if (byItemStack.Attributes.HasAttribute("contents") && byItemStack.Attributes["contents"].GetValue() != null)
            {
                Inventory[0].Itemstack = byItemStack.Attributes.GetItemstack("contents");
                Inventory[0].Itemstack.ResolveBlockOrItem(this.Api.World);
                Inventory.MarkSlotDirty(0);
            }

            UpdateBlockGiftTextures();
            UpdateShape();

            base.OnBlockPlaced(byItemStack);
        }
        public override void OnBlockBroken(IPlayer byPlayer = null)
        {
            Inventory.DropAll(this.Pos.ToVec3d());

            base.OnBlockBroken(byPlayer);
        }
        public override bool OnTesselation(ITerrainMeshPool mesher, ITesselatorAPI tessThreadTesselator)
        {
            bool skipmesh = base.OnTesselation(mesher, Capi.Tesselator);

            if(!skipmesh)
            {
                if (mesh == null)
                    UpdateShape();

                mesher.AddMeshData(mesh);
            }

            return true;
        }
        public bool OpenGift(float secondsUsed, IPlayer byPlayer, BlockPos position)
        {
            if (!byPlayer.Entity.Controls.Sneak)
                return false;

            if (secondsUsed <= GiftOpenDuration)
            {
                if (Api.Side == EnumAppSide.Client)
                {
                    UpdateParticleColor();
                    Capi.World.SpawnParticles(GiftOpenParticles);
                }
                return true;
            }
            else return false;
        }
        public void BreakGift(float secondsUsed, IPlayer byPlayer, BlockPos position)
        {
            if(secondsUsed >= GiftOpenDuration)
            {
                Api.World.BlockAccessor.BreakBlock(position, byPlayer, 0);
            }
        }
        public void PlayOpenSound()
        {
            GiftOpenSound.Start();
        }
        private void UpdateShape()
        {
            if (Api == null)
                return;

            if (Api.Side == EnumAppSide.Client)
            {
                IAsset asset = Capi.Assets.TryGet("gifty:shapes/" + Block.Shape.Base.Path + ".json");

                if (asset != null)
                {
                    Shape currentShape = asset.ToObject<Shape>();

                    if (animUtil != null && animUtil.renderer == null)
                    {
                        mesh = animUtil.InitializeAnimator(Block.FirstCodePart(), currentShape, this, null);
                    }    
                    else
                    {
                        Capi.Tesselator.TesselateShape("container", currentShape, out MeshData wholeMesh, this);

                        mesh = wholeMesh;
                    }
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
        private void InitializetGiftParticles()
        {
            GiftOpenParticles = new SimpleParticleProperties
            {
                MinPos = new Vec3d(Pos.X + 0.25, Pos.Y, Pos.Z + 0.25),
                AddPos = new Vec3d(0.5, 0.5, 0.5),

                MinSize = 0.5f,
                MaxSize = 0.7f,
                SizeEvolve = new EvolvingNatFloat(EnumTransformFunction.LINEARREDUCE, 0.25f),

                MinQuantity = 5.0f,

                LifeLength = 0.1f,
                addLifeLength = 0.3f,

                ShouldDieInLiquid = false,

                WithTerrainCollision = true,

                MinVelocity = new Vec3f(-0.5f, 1f, -0.5f),
                AddVelocity = new Vec3f(1f, 1f, 1f),

                GravityEffect = 0.3f,

                ParticleModel = EnumParticleModel.Cube,
                Async = true
            };
        }
        private void InitializeGiftOpenSound()
        {
            GiftOpenSound = (Capi.World).LoadSound(new SoundParams()
            {
                Location = new AssetLocation("gifty", "sounds/newcrumple.ogg"),
                ShouldLoop = false,
                Position = Pos.ToVec3f().Add(0.5f, 0.25f, 0.5f),
                Range = 12.0f,
                DisposeOnFinish = false,
                Volume = 1.0f
            });
        }
        private void UpdateParticleColor()
        {
            GiftOpenParticles.Color = Block.GetRandomColor(Capi, Pos, BlockFacing.EAST);
        }
        protected virtual void AnimateOpen(IPlayer player)
        {
            if (Api.Side == EnumAppSide.Client)
            {
                animUtil?.StartAnimation(new AnimationMetaData()
                {
                    Animation = "lidopen",
                    Code = "lidopen",
                    AnimationSpeed = 1.8f,
                    EaseOutSpeed = 6,
                    EaseInSpeed = 15
                });

                Capi.World.BlockAccessor.MarkBlockDirty(Pos);
            }
        }
        protected virtual void AnimateClose(IPlayer player)
        {
            animUtil?.StopAnimation("lidopen");

            // This is already handled elsewhere and also causes a stackoverflowexception, but seems needed somehow?
            var inv = invDialog;
            invDialog = null; // Weird handling because to prevent endless recursion
            if (inv?.IsOpened() == true) inv?.TryClose();
            inv?.Dispose();
        }
    }
}

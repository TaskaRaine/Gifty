using Vintagestory.API.Common;

namespace Gifty.CollectibleBehaviors
{
    class RegisterCollectibleBehaviors : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterCollectibleBehaviorClass("GCollectibleBehaviorConvertToLiddedGift", typeof(GCollectibleBehaviorConvertToLiddedGift));
            api.RegisterCollectibleBehaviorClass("GCollectibleBehaviorConvertToWrappedGift", typeof(GCollectibleBehaviorConvertToWrappedGift));
        }
    }
}

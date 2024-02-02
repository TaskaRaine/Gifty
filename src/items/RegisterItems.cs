using Vintagestory.API.Common;

namespace Gifty.Items
{
    class RegisterItems: ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            api.RegisterItemClass("GItemGiftCard", typeof(ItemGiftCard));
        }
    }
}

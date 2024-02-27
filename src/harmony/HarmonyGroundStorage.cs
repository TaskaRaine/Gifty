using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace Gifty
{
    [HarmonyPatch(typeof(BlockEntityGroundStorage), "putOrGetItemSingle", MethodType.Normal)]
    public class HarmonyPutOrGetItemSingle
    {
        //-- This tweak is required to get two ground storable items to interact with each other inside of a ground storage --//
        static bool Prefix(BlockEntityGroundStorage __instance, ItemSlot ourSlot, IPlayer player, BlockSelection bs)
        {
            //-- Perform default action if the slot wanting to access is empty --//
            if(ourSlot.Empty) return true;

            string heldItem = player?.InventoryManager?.ActiveHotbarSlot?.Itemstack?.Collectible?.Code.FirstCodePart();
            string[] skipCodes = { "giftboxbase", "giftboxlid" };

            //-- If the slot and item are the same, then allow us to pick up the already placed item --//
            if (heldItem == ourSlot.Itemstack.Collectible.Code.FirstCodePart())
                return true;

            foreach(string code in skipCodes)
            {
                //-- If two items are different and a skip code exists for the placed item, skip the default ground storage action to allow other actions to run --//
                if (heldItem == code)
                    return false;
            }

            return true;
        }
    }
}

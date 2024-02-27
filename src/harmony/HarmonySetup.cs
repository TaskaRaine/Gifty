using HarmonyLib;
using Vintagestory.API.Common;

namespace Gifty
{
    class HarmonySetup : ModSystem
    {
        Harmony harmony = new Harmony("ca.taska.gifty");

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            harmony.PatchAll();
        }

        public override void Dispose()
        {
            base.Dispose();

            harmony.UnpatchAll("ca.taska.gifty");
        }
    }
}

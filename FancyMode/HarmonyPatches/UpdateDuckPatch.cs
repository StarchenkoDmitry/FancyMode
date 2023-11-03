using System;
using DuckGame;


namespace FancyMode.HarmonyPatches
{
    internal class ManipulateVisibilityPatch
    {
        internal static bool fakeFancyBoots = false;
        public static void MainUpdatePatchInstructions(Duck __instance)  // if fancyShoes are on - > turn them invisible
        {
            if(__instance.HasEquipment(typeof(FancyShoes)) && !fakeFancyBoots)
            {
                __instance.GetEquipment(typeof(FancyShoes)).visible = false;
            }
        }
    }
}

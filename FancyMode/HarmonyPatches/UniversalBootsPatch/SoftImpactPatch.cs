using DuckGame;
using System;

namespace FancyMode.HarmonyPatches
{
    internal class SoftImpactPatch
    {
        public static bool KillPatchInstruction(Duck __instance,MaterialThing with, ImpactedFrom from)       // if your ducks stomps on enemy it wont kill it 
        {
            bool hasRealBootsOn = Updater.wallBoots.Count > 0 || Updater.redBoots.Count > 0;
            if (ImpactedFrom.Bottom == from && with.ToString() == "DuckGame.Spikes" && !hasRealBootsOn)     // simulate spike impact
            {
                Thing s = new Spikes(__instance.position.x, __instance.position.y);
                __instance.Destroy(new DTImpact(s));
                return false;
            }
            if (ImpactedFrom.Bottom == from && with.ToString() == "DuckGame.Duck" && (with as Duck).HasEquipment(typeof(SpikeHelm)) && !hasRealBootsOn)    // simulate spike helmet impact
            {
                Thing s = new SpikeHelm(__instance.position.x, __instance.position.y);
                __instance.Destroy(new DTImpact(s));
                return false;
            }
            if (with is Duck && (with as Duck).HasEquipment(typeof(FancyShoes)) && !ManipulateVisibilityPatch.fakeFancyBoots)
            {
                return false;
            }
            else
            {
                return true;
            }
            return true;
        }
    }
}

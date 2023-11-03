using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckGame;

namespace FancyMode.HarmonyPatches
{
    internal class UnragdollPatch
    {
        public static void UnragdollPatchInstructions(Ragdoll __instance)
        {
            if (!Network.isActive)  // for local multiplayer profiles
            {
                if (__instance._duck.grounded && FancyMode.doUpdate && __instance._duck.profile == Profiles.DefaultPlayer1) 
                {
                    __instance._duck.vSpeed = -2f;
                    __instance._duck.position.y -= 20f;
                }
            }
            else
            {
                if (__instance._duck.grounded && __instance._duck.HasEquipment(typeof(FancyShoes)))
                {
                    __instance._duck.vSpeed = -2f;
                    __instance._duck.position.y -= 20f;
                }
            }
        }
    }
}

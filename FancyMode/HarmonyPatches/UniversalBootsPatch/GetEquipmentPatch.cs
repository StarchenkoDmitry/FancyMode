using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DuckGame;

namespace FancyMode.HarmonyPatches
{
    internal class GetEquipmentPatch
    {
        public static void ReturnWallBoots(ref Thing __result, Type t)  // create an illusion that your fake-fancyshoes can wall jump
        {
            try
            {
                if (t == typeof(WallBoots) && FancyMode.doUpdate) //doUpdate == fancyshoes have to be equipped
                {
                    foreach (Thing thing in Updater.wallBoots)
                    {
                        if (thing.owner != null)
                        {
                            WallBoots wallBoots = new WallBoots(0f, 0f);
                            __result = wallBoots;
                        }
                    }
                }
            }
            catch(NullReferenceException e)     // null wallBoots list
            {

            }
        }
    }
}

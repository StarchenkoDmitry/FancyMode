using System;
using System.Collections.Generic;
using System.Reflection;
using DuckGame;
using HarmonyLib;
using Microsoft.Xna.Framework;

using FancyMode.HarmonyPatches;

namespace FancyMode
{
    public class FancyMode : DisabledMod
    {
        internal readonly string id = "fancymode";
        internal static bool doUpdate;
        internal static bool doNotPatchAgain;
        internal static Profile userProfile
        {
            get
            {
                if (!Network.isActive)
                {
                    return Profiles.DefaultPlayer1;
                }
                return DuckNetwork.localProfile;
            }
        }
        public override DuckGame.Priority priority
        {
            get { return base.priority; }
        }
        protected override void OnPreInitialize()
        {
            base.OnPreInitialize();
            HarmonyDependencyResolver.ResolveDependencies();
        }
        protected override void OnPostInitialize()
        {
            // Inject Updater Class into Xna Framework in DuckGame (MonoMain.Instance)
            FieldInfo getInjection = typeof(Game).GetField("updateableComponents", BindingFlags.Instance | BindingFlags.NonPublic);
            List<IUpdateable> addUpdateable = getInjection.GetValue(MonoMain.instance) as List<IUpdateable>;
            addUpdateable.Add(new Updater());
            getInjection.SetValue(MonoMain.instance, addUpdateable);  // overwrite new content 

            base.OnPostInitialize();

            //manual patching - no Attributes
            Harmony patcher = new Harmony(id);
            
            //for Update
            MethodInfo patchedMethod = typeof(ManipulateVisibilityPatch).GetMethod(nameof(ManipulateVisibilityPatch.MainUpdatePatchInstructions), BindingFlags.Static | BindingFlags.Public);
            HarmonyMethod newMethod = new HarmonyMethod(patchedMethod);
            MethodBase origMethod = typeof(Duck).GetMethod(nameof(Duck.Update));

            //for OnSoftImpact
            MethodInfo patchedMethod2 = typeof(SoftImpactPatch).GetMethod(nameof(SoftImpactPatch.KillPatchInstruction), BindingFlags.Static | BindingFlags.Public);
            HarmonyMethod newMethod2 = new HarmonyMethod(patchedMethod2);
            MethodBase origMethod2 = typeof(Duck).GetMethod(nameof(Duck.OnSoftImpact));

            //for Unragdoll
            MethodInfo patchedMethod3 = typeof(UnragdollPatch).GetMethod(nameof(UnragdollPatch.UnragdollPatchInstructions), BindingFlags.Static | BindingFlags.Public);
            HarmonyMethod newMethod3 = new HarmonyMethod(patchedMethod3);
            MethodBase origMethod3 = typeof(Ragdoll).GetMethod(nameof(Ragdoll.Unragdoll));

            //for GetEquipment
            MethodInfo patchedMethod4 = typeof(GetEquipmentPatch).GetMethod(nameof(GetEquipmentPatch.ReturnWallBoots), BindingFlags.Static | BindingFlags.Public);
            HarmonyMethod newMethod4 = new HarmonyMethod(patchedMethod4);
            MethodBase origMethod4 = typeof(Duck).GetMethod(nameof(Duck.GetEquipment));

            DevConsole.commands.Remove("fancymode");             // patch existing command

            Action<CMD> fancyMode = (activation) => 
            {
                string getState = activation.Arg<string>("activator");

                if (getState == "on" || getState == "On")
                {
                    if (doNotPatchAgain)
                        return;

                    doNotPatchAgain = true;

                    DevConsole.Log(DCSection.Mod, "|ORANGE|fancy mode is|GREEN| on");

                    patcher.Patch(origMethod, postfix: newMethod);
                    patcher.Patch(origMethod2, prefix: newMethod2);
                    patcher.Patch(origMethod3, postfix: newMethod3);
                    patcher.Patch(origMethod4, postfix: newMethod4);

                    doUpdate = true;
                }
                if (getState == "off" || getState == "Off")
                {
                    doNotPatchAgain = false;

                    DevConsole.Log(DCSection.Mod, "|ORANGE|fancy mode is|RED| off");

                    patcher.Unpatch(origMethod, HarmonyPatchType.Postfix, id);
                    patcher.Unpatch(origMethod2, HarmonyPatchType.Prefix, id);
                    patcher.Unpatch(origMethod3, HarmonyPatchType.Postfix, id);
                    patcher.Unpatch(origMethod4, HarmonyPatchType.Postfix, id);

                    doUpdate = false;
                    ManipulateVisibilityPatch.fakeFancyBoots = true;
                    ResetBoots();
                }
            };
            Action<CMD> fancyShoes = (activation) =>
            {
                DevConsole.fancyMode = !DevConsole.fancyMode;

                DevConsole.RunCommand("fancymode off");

                DevConsole.Log(DCSection.Mod, "|ORANGE|fancy shoes are" + (DevConsole.fancyMode ? "|GREEN| on" : "|RED| off"));
            };

            CMD.Argument[] args = new CMD.Argument[1];
            args[0] = new CMD.String("activator", false);

            DevConsole.AddCommand(new CMD("fancymode", args, fancyMode ));
            DevConsole.AddCommand(new CMD("fancyshoes",pAction: fancyShoes));
        }
        

        private void ResetBoots()
        {
            try
            {
                foreach(Thing thing in Updater.redBoots)
                {
                    if(thing.owner == userProfile.duck)
                    {
                        Boots boots = new Boots(userProfile.duck.x,userProfile.duck.y);
                        Equipment e = userProfile.duck.GetEquipment(typeof(FancyShoes));
                        userProfile.duck.Equip(boots, false, false);
                        Level.Remove(e);
                        Level.Add(boots);
                    }
                }
                foreach (Thing thing in Updater.wallBoots)
                {
                    if (thing.owner == userProfile.duck)
                    {
                        WallBoots boots = new WallBoots(userProfile.duck.x, userProfile.duck.y);
                        Equipment e = userProfile.duck.GetEquipment(typeof(FancyShoes));
                        userProfile.duck.Unequip(e);
                        Level.Remove(e);
                        Level.Add(boots);
                        userProfile.duck.Equip(boots, false, false);
                    }
                }
                if (userProfile.duck.HasEquipment(typeof(FancyShoes)))
                {
                    userProfile.duck.Unequip(userProfile.duck.GetEquipment(typeof(FancyShoes)));
                }
                Updater.redBoots.Clear();
                Updater.wallBoots.Clear();
            }
            catch(NullReferenceException e)
            {

            }
        }
    }
}

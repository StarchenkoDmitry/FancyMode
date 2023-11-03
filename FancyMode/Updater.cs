using DuckGame;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.Reflection;
using FancyMode.HarmonyPatches;
using System.Collections.Generic;

namespace FancyMode
{
    internal class Updater : IUpdateable
    {
        #region mandatory IUpdateable Attributes
        public bool Enabled
        {
            get
            {
                return true;
            }
        }

        public int UpdateOrder
        {
            get
            {
                return 1;
            }
        }

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        #endregion

        /// <summary>
        /// all fake-fancyshoes in current Level
        /// </summary>
        public static List<Thing> wallBoots = new List<Thing>();
        public static List<Thing> redBoots = new List<Thing>();
        int col = 0;
        public void Update(GameTime gameTime)
        {
            if (FancyMode.doUpdate)
            {
                try
                {
                    Duck duck = FancyMode.userProfile.duck;
                    if (duck.dead && duck.HasEquipment(typeof(Boots)))
                    {
                        duck.Unequip(duck.GetEquipment(typeof(Boots)));

                    }
                    if (!duck.HasEquipment(typeof(FancyShoes)) && duck.HasEquipment(typeof(Boots)) && !duck.HasEquipment(typeof(WallBoots)))  // red boots
                    {
                        #region Reflections
                        FieldInfo pickupSprite = typeof(Boots).GetField("_pickupSprite", BindingFlags.Instance | BindingFlags.NonPublic);
                        FieldInfo sprite = typeof(Boots).GetField("_sprite", BindingFlags.Instance | BindingFlags.NonPublic);
                        #endregion

                        Equipment prevBoots = duck.GetEquipment(typeof(Boots));
                        duck.Unequip(prevBoots);
                        Level.Remove(prevBoots);

                        FancyShoes changeAppearence = new FancyShoes(duck.x, duck.y);
                        ManipulateVisibilityPatch.fakeFancyBoots = true;

                        pickupSprite.SetValue(changeAppearence, new Sprite("bootsPickup", 0f, 0f));
                        sprite.SetValue(changeAppearence, new SpriteMap("boots", 32, 32, false));
                        changeAppearence.graphic = (Sprite)pickupSprite.GetValue(changeAppearence);

                        Level.Add(changeAppearence);
                        redBoots.Add(changeAppearence);
                        duck.Equip(changeAppearence, false, false);
                    }
                    else if (!duck.HasEquipment(typeof(FancyShoes)) && duck.HasEquipment(typeof(WallBoots)))     // wall jump boots
                    {
                        Equipment prevBoots = duck.GetEquipment(typeof(WallBoots));
                        duck.Unequip(prevBoots);
                        Level.Remove(prevBoots);

                        FancyShoes changeAppearence = new FancyShoes(duck.x, duck.y);
                        ManipulateVisibilityPatch.fakeFancyBoots = true;

                        Level.Add(changeAppearence);
                        wallBoots.Add(changeAppearence);
                        duck.Equip(changeAppearence, false, false);
                    }
                    else if (!FancyMode.userProfile.duck.HasEquipment(typeof(FancyShoes)) &&  !duck.dead)     // fancyshoes updater
                    {
                        ManipulateVisibilityPatch.fakeFancyBoots = false;
                        if (duck.HasEquipment(typeof(Boots)))
                        {
                            Equipment prevBoots = duck.GetEquipment(typeof(Boots));
                            duck.Unequip(prevBoots);
                            Level.Remove(prevBoots);
                        }
                        FancyShoes l = new FancyShoes(duck.x, duck.y);
                        Level.Add(l);

                        duck.Equip(l, false, false);
                    }

                    try
                    {
                        foreach (Thing thing in redBoots)
                        {
                            if (thing.owner != null)
                                continue;

                            float xPos = thing.position.x;
                            float yPos = thing.position.y;

                            Level.Remove(thing);
                            Boots newBoots = new Boots(xPos, yPos);
                            Level.Add(newBoots);
                            redBoots.Remove(thing);
                        }
                        foreach (Thing thing in wallBoots)
                        {
                            if (thing.owner != null)
                                continue;

                            float xPos = thing.position.x;
                            float yPos = thing.position.y;

                            Level.Remove(thing);
                            WallBoots newBoots = new WallBoots(xPos, yPos);
                            Level.Add(newBoots);
                            wallBoots.Remove(thing);
                        }
                    }
                    catch (InvalidOperationException e) // wait until boots are deployed
                    {
                    }
                    col++;
                    if (col >= 60)   //2 sec check clear boots manually
                    {
                        #region clear Things in Level 
                        FieldInfo thingsList = typeof(Level).GetField("_things", BindingFlags.Instance | BindingFlags.NonPublic);
                        Level l = Level.current;
                        QuadTreeObjectList list = (QuadTreeObjectList)thingsList.GetValue(l);   // list of all Objects in current Level
                        
                        using (IEnumerator<Thing> enumerator = list.GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                if (enumerator.Current.ToString() == "DuckGame.FancyShoes")
                                {
                                    if (enumerator.Current.visible == false && enumerator.Current.owner == null)  //for all boots that are not equipped and invisible
                                    {
                                        Level.Remove(enumerator.Current);
                                    }
                                }
                            }
                        }
                        DevConsole.Log(wallBoots.Count.ToString());
                        DevConsole.Log(redBoots.Count.ToString());
                        #endregion
                        col = 0;
                    }
                }
                catch (NullReferenceException e)    // cant get user Profile in every frame -> prevent crashing
                {
                }
            }
        }
    }
}

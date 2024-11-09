/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */


using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DOL.GS
{
    class UniqueLootGenerator : LootGeneratorBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [ScriptLoadedEvent]
        public static void OnScriptLoaded(DOLEvent e, object sender, EventArgs args)
        {
            LootMgr.RegisterLootGenerator(new UniqueLootGenerator(), "", "", "", 0);
            InitializeHashtables();
            log.Info("ROG System Loaded!");
        }

        [CmdAttribute(
            "&genuniques",
            ePrivLevel.GM,
            "/genuniques ([TOA] || [L51] || [objecttype]) [itemtype] : generate 8 unique items")]
        public class LootGeneratorUniqueObjectCommandHandler : DOL.GS.Commands.AbstractCommandHandler, DOL.GS.Commands.ICommandHandler
        {
            public void OnCommand(GameClient client, string[] args)
            {
                GameNPC mob = new GameNPC
                {
                    Name = "Unique Object",
                    CurrentRegionID = 1,
                    Level = (byte)(client.Player.Level)
                };
                if (mob.Level > 50)
                    mob.Level = 51;

                try
                {

                    LootList lootlist = new LootList();

                    for (int x = 0; x < 8; x++)
                    {
                        ItemUnique item = null;

                        if (args.Length > 1)
                        {
                            if (Convert.ToString(args[1]).ToUpper() == "TOA")
                            {
                                mob.Level = 60;
                                mob.Name = "TOA Mob";
                                mob.CurrentRegionID = 30;
                                mob.Z = 9999;
                            }
                            else if (Convert.ToString(args[1]).ToUpper() == "L51")
                            {
                                mob.Level = 51;
                            }
                            else
                            {
                                item = new ItemUnique
                                {
                                    Object_Type = Convert.ToInt32(args[1])
                                };

                                if (args.Length > 2)
                                    item.Item_Type = Convert.ToInt32(args[2]);
                            }
                        }

                        lootlist.AddFixed(GenerateUniqueItem(mob, client.Player, item), 1);
                    }

                    foreach (ItemUnique item in lootlist.GetLoot())
                    {
                        GameServer.Database.AddObject(item);
                        InventoryItem invitem = GameInventoryItem.Create<ItemUnique>(item);
                        client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
                        client.Player.Out.SendMessage("Generated: " + item.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
                catch (Exception)
                {
                    DisplaySyntax(client);
                }
            }
        }

        [CmdAttribute(
            "&clearinventory",
            ePrivLevel.GM,
            "/clearinventory YES - clears your entire inventory!")]
        public class ClearInventoryCommandHandler : DOL.GS.Commands.AbstractCommandHandler, DOL.GS.Commands.ICommandHandler
        {
            public void OnCommand(GameClient client, string[] args)
            {
                // must add at least one parameter just to be safe
                if (args.Length > 1 && args[1].ToString() == "YES")
                {
                    foreach (InventoryItem item in client.Player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                        client.Player.Inventory.RemoveItem(item);

                    client.Out.SendMessage("Inventory cleared!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    DisplaySyntax(client);
                }
            }
        }

        /// <summary>
        /// Check target mobs Lootlist
        /// </summary>
        /// <param name="mob"></param>
        /// <returns></returns>
        public virtual bool HasLoot(GameNPC mob)
        {
            IList<LootTemplate> loot = GameServer.Database.SelectObjects<LootTemplate>("`TemplateName` = @TemplateName", new QueryParameter("@TemplateName", mob.Name));

            if (loot != null && loot.Count > 0)
            {
                //log.Error("lootlist nicht leer");
                return true;
            }
            return false;
        }

       

        /// <summary>
        /// Generate loot for given mob
        /// </summary>
        /// <param name="mob"></param>
        /// <returns></returns>
        public override LootList GenerateLoot(GameNPC mob, GameObject killer)
        {
            LootList loot = null;

            //only drops for empty mobs
            if (killer != null && mob != null)
            {

                loot = base.GenerateLoot(mob, killer);
            }

            if (killer != null && killer.ObjectState != GameObject.eObjectState.Active)
                return loot;

            GameLiving living = killer as GameLiving;
                       

            double con = GameObject.GetConLevel(living.Level, mob.Level);
            if (con < -3.0)
                con = -3.0;


            if (con == -3.0)
            {

                //log.ErrorFormat("Mob ist grau also kein loot2!! {0}", con);
                return loot;
            }

            if (living is GameKeepGuard || mob is GuardLord || mob is GameKeepGuard || 
                !living.IsAlive || mob.CurrentRegion.IsDungeon && mob.CurrentRegion.IsRvR || mob.CurrentRegionID == 249)
            {
                // no ROG for underwater mobs
                return loot;
            }
            int minimum_basechance = 20;//Base change 15

            int baseChance = minimum_basechance;//Math.Max(minimum_basechance, 100 - (mob.Level));

            if ((int)killer.GetConLevel(mob) > 1)
            {
                baseChance += (int)(killer.GetConLevel(mob) * 1.5);
            }
            int chance = baseChance;

            ItemUnique item = new ItemUnique();

            // tolakram - drop level 51 items if mob is level 50 or above (confirmed via allakhazam)
            if (mob.Level > 50)
            {
                item.Level = 51;
                item.Quality = Util.Random(95, 99);
            }

            if (IsMobInTOA(mob) && mob.Name.ToLower() != mob.Name && mob.Level > 50)
            {
                // ToA named mobs have good chance to drop unique loot
                chance += 5;
                item.Level = 51;
                item.Quality = Util.Random(95, 99);
            }

            if (killer.Realm != 0 && Util.Chance(chance) && HasLoot(mob) == false)
            {
                ItemUnique newItem = GenerateUniqueItem(mob, killer, item);

                if (newItem != null)
                {

                    newItem.Description = "Unique Object";  //unique object
                    loot.AddFixed(newItem, 1);
                }
            }

            return loot;
        }

        public static bool IsMobInTOA(GameNPC mob)
        {
            if (mob.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
                return true;

            return false;
        }

        public static ItemUnique GenerateUniqueItem(GameNPC mob, GameObject killer, ItemUnique item)
        {


            GamePlayer player = null;
            GameLiving mobKiller = null;
            ControlledNpcBrain brain = null;

            if (killer is GamePlayer == false && killer != null && killer != mob)
            {

                if (killer != null && killer is GameNPC && ((GameNPC)killer).Brain is ControlledNpcBrain)
                {
                    brain = ((GameNPC)killer).Brain as ControlledNpcBrain;
                }

                if (brain != null)
                {
                    mobKiller = brain.GetLivingOwner();

                    if (mobKiller != null && mobKiller is GamePlayer)
                    {
                        player = mobKiller as GamePlayer;
                    }

                    if (killer != null && player != null)
                    {
                        player = killer as GamePlayer;
                    }
                }
            }
            else if (killer != null && killer is GamePlayer)
            {
                player = killer as GamePlayer;
            }

            if (player == null)
            {
                return null;
            }

            BattleGroup battleGroup = player.TempProperties.getProperty<BattleGroup>(BattleGroup.BATTLEGROUP_PROPERTY, null);

            if ((player is GamePlayer && mob.IsWorthReward) && battleGroup != null && GS.ServerProperties.Properties.BATTLEGROUP_LOOT)
            {
                try
                {

                    //BattleGroup
                    foreach (GamePlayer players in battleGroup.Members.Keys)
                    {
                        if (players != null)
                        {
                            killer = players.Group.Leader;


                            if (players.IsWithinRadius(killer, 8000) == false)
                                return null;

                        }
                    }
                }
                catch
                { 
                }
            }
            else if ((player is GamePlayer && mob.IsWorthReward) && (player.Group != null))
            {
                foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                {
                    if (players != null)
                    {

                        killer = players.Group.Leader;

                        if (players.IsWithinRadius(killer, 8000) == false)
                            return null;
                    }
                }
            }
            else
            {
                if (player.IsWithinRadius(killer, 8000) == false)
                    return null;

                killer = player;
            }
            

            if (item == null)
                item = new ItemUnique();

            if (item.Realm == 0)
                item.Realm = (int)killer.Realm;

            int moblevel = mob.Level;

            if (item.Level == 0)
            {
                if (mob.Level > 50)
                    item.Level = 51;
                else if (mob.Level > 2 && mob.Level < killer.Level && killer.Level > 3)
                    item.Level = (mob.Level - 2);
                else if (mob.Level > 2 && mob.Level == killer.Level && killer.Level > 3)
                    item.Level = (killer.Level - 2);


                else if (item.Level > moblevel)
                    item.Level = moblevel; // Item Level = player Level

                else
                {
                    item.Level = mob.Level;
                    if (item.Level > 49)
                        item.Level = 49;
                    if (item.Level < 1)
                        item.Level = 1;
                }
            }

            //object type
            if (item.Object_Type == 0)
            {
                item.Object_Type = (int)GenerateObjectType(item.Realm, item.Level);
               // log.ErrorFormat(" Object_Type {0}", item.Object_Type);
            }
            //item slot
            if (item.Item_Type == 0)
                item.Item_Type = (int)GenerateItemType((eObjectType)item.Object_Type);

            //damage type
            item.Type_Damage = (int)GenerateDamageType((eObjectType)item.Object_Type, item.Level);

            //item stats
            //instrument dps_af needs to be known before name generation
            GenerateItemStats(item);

            //item name and model
            GenerateItemNameModel(item, killer);

            //item quality / maxquality
            if (item.Quality == 1)
                item.Quality = GenerateItemQuality(killer.GetConLevel(mob), item);

            //item magical bonuses
            //if staff and magic..... focus
            GenerateMagicalBonuses(item, mob);

            item.IsDropable = true;
            item.IsPickable = true;
            item.IsTradable = true;

            //item bonus
            int temp = item.Level - 9;
            temp -= temp % 5;
            item.Bonus = temp;
            if (item.Bonus < 0)
                item.Bonus = 0;
            if (item.Level > 4 && item.Level < 10)
                item.Bonus = 5;
            else if (item.Level >= 10 && item.Level <= 20)
                item.Bonus = 10;
            else if (item.Level >= 20 && item.Level <= 30)
                item.Bonus = 15;
            else if (item.Level > 30 && item.Level <= 45)
                item.Bonus = 25;
            else if (item.Level > 45)
                item.Bonus = 35;

            if (item.Bonus > 35)
                item.Bonus = 35;
            //constants
            int condition = item.Level * 2000;
            item.Condition = condition;
            item.MaxCondition = condition;
            item.Durability = condition;
            item.MaxDurability = condition;

            item.Weight = GenerateItemWeight((eObjectType)item.Object_Type, (eInventorySlot)item.Item_Type, item);
            item.Price = Money.SetAutoPrice(item.Level, item.Quality);
            item.Description = mob.Name;

            return item;
        }

        public static InventoryItem GenerateFreeEquipment(eInventorySlot slot, eObjectType type, byte level, GamePlayer player, eDamageType damage)
        {
            ItemUnique item = new ItemUnique
            {

                //item realm
                Realm = (int)player.Realm,
                //item level
                Level = level,

                //object type
                Object_Type = (int)type,

                //item slot
                Item_Type = (int)slot,

                //damage type
                Type_Damage = (int)damage
            };

            //item stats
            //instrument dps_af needs to be known before name generation
            GenerateItemStats(item);

            //item name and model
            GenerateItemNameModel(item, player);

            string prefix = String.Empty;
            //Add the appropriate name to the beginning
            //Armor First
            if (item.Object_Type >= (int)eObjectType._FirstArmor && item.Object_Type <= (int)eObjectType._LastArmor)
            {
                if (player.Level == 20)
                    prefix = "Second-hand";
                else
                    prefix = "Hand-me-down";
            }
            else if (item.Object_Type >= (int)eObjectType._FirstWeapon && item.Object_Type <= (int)eObjectType._LastWeapon)
            {
                switch ((eRealm)player.Realm)
                {
                    case eRealm.Albion:
                        if (player.Level == 20)
                            prefix = "Doddering Goblin Hunter's";
                        else
                            prefix = "Doddering Boogey Hunter's";
                        break;
                    case eRealm.Hibernia:
                        if (player.Level == 20)
                            prefix = "Doddering Weewere Hunter's";
                        else
                            prefix = "Doddering Spraggon Hunter's";
                        break;
                    case eRealm.Midgard:
                        if (player.Level == 20)
                            prefix = "Doddering  Wee Wolf Hunter's";
                        else
                            prefix = "Doddering Tomte Hunter's";
                        break;
                }
            }

            item.Name = string.Format("{0} {1}", prefix, item.Name);

            if (item.Object_Type == (int)eObjectType.Staff)
            {
                item.Bonus1 = Math.Min(50, item.Level);
                item.Bonus1Type = (int)eProperty.AllFocusLevels;
            }

            item.IsDropable = false;
            item.IsPickable = true;
            item.IsTradable = false;

            //item quality / maxquality
            item.Quality = 85;

            //item bonus
            int temp = item.Level - 15;
            temp -= temp % 5;
            item.Bonus = temp;
            if (item.Bonus < 0)
                item.Bonus = 0;

            //constants
            int condition = item.Level * 2000;
            item.Condition = condition;
            item.MaxCondition = condition;
            item.Durability = condition;
            item.MaxDurability = condition;

            item.Weight = GenerateItemWeight((eObjectType)item.Object_Type, (eInventorySlot)item.Item_Type, item);
            item.Price = 1;

            GameServer.Database.AddObject(item);
            return GameInventoryItem.Create<ItemUnique>(item);
        }

        private static eObjectType GenerateObjectType(int realm, int level)
        {
            eGenerateType type = eGenerateType.None;
            if (Util.Chance(55)) type = eGenerateType.Armor;
            else if (Util.Chance(45)) type = eGenerateType.Magical;
            else type = eGenerateType.Weapon;

            switch ((eRealm)realm)
            {
                case eRealm.Albion:
                    {
                        int maxArmor = AlbionArmor.Length - 1;
                        int maxWeapon = AlbionWeapons.Length - 1;

                        if (level < 15)
                            maxArmor--; // remove plate

                        if (level < 5)
                        {
                            maxArmor--; // remove chain
                            maxWeapon = 4; // remove all but base weapons and shield
                        }

                        switch (type)
                        {
                            case eGenerateType.Armor: return AlbionArmor[Util.Random(0, maxArmor)];
                            case eGenerateType.Weapon: return AlbionWeapons[Util.Random(0, maxWeapon)];
                            case eGenerateType.Magical: return eObjectType.Magical;
                        }
                        break;
                    }
                case eRealm.Midgard:
                    {
                        int maxArmor = MidgardArmor.Length - 1;
                        int maxWeapon = MidgardWeapons.Length - 1;

                        if (level < 10)
                            maxArmor--; // remove chain

                        if (level < 5)
                        {
                            maxWeapon = 4; // remove all but base weapons and shield
                        }

                        switch (type)
                        {
                            case eGenerateType.Armor: return MidgardArmor[Util.Random(0, maxArmor)];
                            case eGenerateType.Weapon: return MidgardWeapons[Util.Random(0, maxWeapon)];
                            case eGenerateType.Magical: return eObjectType.Magical;
                        }
                        break;
                    }
                case eRealm.Hibernia:
                    {
                        int maxArmor = HiberniaArmor.Length - 1;
                        int maxWeapon = HiberniaWeapons.Length - 1;

                        if (level < 15)
                            maxArmor--; // remove scale

                        if (level < 5)
                        {
                            maxWeapon = 4; // remove all but base weapons and shield
                        }

                        switch (type)
                        {
                            case eGenerateType.Armor: return HiberniaArmor[Util.Random(0, maxArmor)];
                            case eGenerateType.Weapon: return HiberniaWeapons[Util.Random(0, maxWeapon)];
                            case eGenerateType.Magical: return eObjectType.Magical;
                        }
                        break;
                    }
            }
            return eObjectType.GenericItem;
        }

        private static eInventorySlot GenerateItemType(eObjectType type)
        {
            if ((int)type >= (int)eObjectType._FirstArmor && (int)type <= (int)eObjectType._LastArmor)
                return (eInventorySlot)ArmorSlots[Util.Random(0, ArmorSlots.Length - 1)];
            switch (type)
            {
                //left or right standard
                //tolakram - left hand usable now set based on speed
                case eObjectType.HandToHand:
                case eObjectType.Piercing:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.SlashingWeapon:
                case eObjectType.CrushingWeapon:
                case eObjectType.ThrustWeapon:
                case eObjectType.FistWraps: //Maulers
                    return (eInventorySlot)Slot.RIGHTHAND;
                //left or right or twohand
                case eObjectType.Sword:
                case eObjectType.Hammer:
                    return (eInventorySlot)Util.Random(Slot.RIGHTHAND, Slot.TWOHAND);
                //right
                case eObjectType.Flexible:
                    return (eInventorySlot)Slot.RIGHTHAND;
                //left
                case eObjectType.LeftAxe:
                case eObjectType.Shield:
                    return (eInventorySlot)Slot.LEFTHAND;
                //twohanded
                case eObjectType.LargeWeapons:
                case eObjectType.CelticSpear:
                case eObjectType.PolearmWeapon:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.Scythe:
                case eObjectType.TwoHandedWeapon:
                case eObjectType.MaulerStaff:
                    return (eInventorySlot)Slot.TWOHAND;
                //right or twohand
                case eObjectType.Axe:
                    {
                        if (Util.Chance(50))
                            return eInventorySlot.RightHandWeapon;
                        else return eInventorySlot.TwoHandWeapon;
                    }
                //ranged
                case eObjectType.CompositeBow:
                case eObjectType.Fired:
                case eObjectType.Longbow:
                case eObjectType.RecurvedBow:
                case eObjectType.Crossbow:
                    return (eInventorySlot)Slot.RANGED;
                case eObjectType.Magical:
                    return (eInventorySlot)MagicalSlots[Util.Random(0, MagicalSlots.Length - 1)];
                case eObjectType.Instrument:
                    return (eInventorySlot)Util.Random(Slot.TWOHAND, Slot.RANGED);
            }
            return eInventorySlot.FirstEmptyBackpack;
        }

        private static eDamageType GenerateDamageType(eObjectType type, int level)
        {
            switch (type)
            {
                //all
                case eObjectType.TwoHandedWeapon:
                case eObjectType.PolearmWeapon:
                    return (eDamageType)Util.Random(1, 3);
                //slash
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.SlashingWeapon:
                case eObjectType.LeftAxe:
                case eObjectType.Sword:
                case eObjectType.Scythe:
                    return eDamageType.Slash;
                //thrust
                case eObjectType.ThrustWeapon:
                case eObjectType.Piercing:
                case eObjectType.CelticSpear:
                    return eDamageType.Thrust;
                //crush
                case eObjectType.Hammer:
                case eObjectType.CrushingWeapon:
                case eObjectType.Blunt:
                case eObjectType.MaulerStaff: //Maulers
                case eObjectType.FistWraps: //Maulers
                    return eDamageType.Crush;
                //specifics
                case eObjectType.Longbow:
                case eObjectType.RecurvedBow:
                case eObjectType.CompositeBow:
                case eObjectType.Fired:
                case eObjectType.Crossbow:
                    return eDamageType.Thrust;
                case eObjectType.HandToHand:
                    return (eDamageType)Util.Random(2, 3);
                case eObjectType.Spear:
                    return (eDamageType)Util.Random(2, 3);
                case eObjectType.Flexible:
                    return (eDamageType)Util.Random(1, 2);
                case eObjectType.Staff:
                    return eDamageType.Crush;
                case eObjectType.LargeWeapons:
                    return (eDamageType)Util.Random(1, 2);
                //do shields return the shield size?
                case eObjectType.Shield:
                    return (eDamageType)Util.Random(1, (level < 10 ? 2 : 3));
                //instruments too?
                case eObjectType.Instrument:
                    return (eDamageType)Util.Random(1, 3);

            }
            return eDamageType.Natural;
        }

        private static void GenerateItemNameModel(ItemUnique item, GameObject killer)
        {
            eInventorySlot slot = (eInventorySlot)item.Item_Type;
            eDamageType damage = (eDamageType)item.Type_Damage;
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            string name = "No Name";
            int model = 488;
            int proc = 0;
            int spell = 0;
            int charges = 10;
            int maxcharges = 10;

            bool canAddExtension = false;


            switch (type)
            {
                //armor
                case eObjectType.Cloth:
                    {
                        name = "Cloth " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }

                        switch (realm)
                        {
                            case eRealm.Albion:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 995; break;
                                    case eInventorySlot.LegsArmor: model = 984; break;
                                    case eInventorySlot.FeetArmor: model = 269; break;
                                    case eInventorySlot.HeadArmor: model = 1294; break;
                                    case eInventorySlot.TorsoArmor:
                                        if (Util.Chance(35))
                                        {
                                            model = 683;
                                        }
                                        else
                                        {
                                            if (Util.Chance(35))
                                            {

                                                name = "Cloth Robe";

                                                switch (Util.Random(0, 3))
                                                {
                                                    case 0: model = 58; break;
                                                    case 1: model = 65; break;
                                                    case 2: model = 66; break;
                                                    case 3: model = 2729; break;
                                                }
                                            }
                                            else
                                            {
                                                name = "Cloth Vest";

                                                switch (Util.Random(0, 3))
                                                {
                                                    case 0: model = 2162; break;
                                                    case 1: model = 2728; break;
                                                    case 2: model = 2790; break;
                                                    case 3: model = 3695; break;
                                                }
                                            }
                                        }


                                        break;
                                    case eInventorySlot.HandsArmor: model = 268; break;
                                }
                                break;

                            case eRealm.Midgard:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 247; break;
                                    case eInventorySlot.LegsArmor: model = 246; break;
                                    case eInventorySlot.FeetArmor: model = 987; break;
                                    case eInventorySlot.HeadArmor: model = 1296; break;
                                    case eInventorySlot.TorsoArmor:
                                        if (Util.Chance(35))
                                        {
                                            model = 983;
                                        }
                                        else
                                        {
                                            if (Util.Chance(35))
                                            {

                                                name = "Cloth Robe";

                                                switch (Util.Random(0, 3))
                                                {
                                                    case 0: model = 58; break;
                                                    case 1: model = 65; break;
                                                    case 2: model = 66; break;
                                                    case 3: model = 3018; break;
                                                }
                                            }

                                            else
                                            {
                                                name = "Cloth Vest";

                                                switch (Util.Random(0, 3))
                                                {
                                                    case 0: model = 993; break;
                                                    case 1: model = 2852; break;
                                                    case 2: model = 3058; break;
                                                    case 3: model = 3694; break;
                                                }
                                            }
                                        }


                                        break;
                                    case eInventorySlot.HandsArmor: model = 996; break;
                                }
                                break;

                            case eRealm.Hibernia:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 995; break;
                                    case eInventorySlot.LegsArmor: model = 984; break;
                                    case eInventorySlot.FeetArmor: model = 987; break;
                                    case eInventorySlot.HeadArmor: model = 1298; break;
                                    case eInventorySlot.TorsoArmor:
                                        if (Util.Chance(35))
                                        {
                                            model = 993;
                                        }
                                        else
                                        {
                                            if (Util.Chance(35))
                                            {

                                                name = "Cloth Robe";

                                                switch (Util.Random(0, 3))
                                                {
                                                    case 0: model = 58; break;
                                                    case 1: model = 65; break;
                                                    case 2: model = 66; break;
                                                    case 3: model = 2729; break;
                                                }
                                            }

                                            else
                                            {
                                                name = "Cloth Vest";

                                                switch (Util.Random(0, 3))
                                                {
                                                    case 0: model = 983; break;
                                                    case 1: model = 2759; break;
                                                    case 2: model = 3058; break;
                                                    case 3: model = 2821; break;
                                                }
                                            }
                                        }
                                        break;
                                    case eInventorySlot.HandsArmor: model = 996; break;
                                }
                                break;

                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }
                case eObjectType.Leather:
                    {
                        name = "Leather " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }
                        switch (realm)
                        {
                            case eRealm.Albion:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 3066; break;
                                    case eInventorySlot.LegsArmor: model = 2798; break;
                                    case eInventorySlot.FeetArmor: model = 2801; break;
                                    case eInventorySlot.HeadArmor: model = 2800; break;
                                    case eInventorySlot.TorsoArmor: model = 2797; break;
                                    case eInventorySlot.HandsArmor: model = 2802; break;
                                }
                                break;

                            case eRealm.Midgard:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 2861; break;
                                    case eInventorySlot.LegsArmor: model = 2860; break;
                                    case eInventorySlot.FeetArmor: model = 2863; break;
                                    case eInventorySlot.HeadArmor: model = 2862; break;
                                    case eInventorySlot.TorsoArmor: model = 2859; break;
                                    case eInventorySlot.HandsArmor: model = 3068; break;
                                }
                                break;

                            case eRealm.Hibernia:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 2830; break;
                                    case eInventorySlot.LegsArmor: model = 2829; break;
                                    case eInventorySlot.FeetArmor: model = 2832; break;
                                    case eInventorySlot.HeadArmor: model = 1299; break;
                                    case eInventorySlot.TorsoArmor: model = 2828; break;
                                    case eInventorySlot.HandsArmor: model = 2833; break;
                                }
                                break;

                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }
                case eObjectType.Studded:
                    {
                        name = "Studded " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }
                        switch (realm)
                        {
                            case eRealm.Albion:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 2805; break;
                                    case eInventorySlot.LegsArmor: model = 2804; break;
                                    case eInventorySlot.FeetArmor: model = 2807; break;
                                    case eInventorySlot.HeadArmor: model = 2806; break;
                                    case eInventorySlot.TorsoArmor: model = 2803; break;
                                    case eInventorySlot.HandsArmor: model = 2808; break;
                                }
                                break;

                            case eRealm.Midgard:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 2867; break;
                                    case eInventorySlot.LegsArmor: model = 2866; break;
                                    case eInventorySlot.FeetArmor: model = 2869; break;
                                    case eInventorySlot.HeadArmor: model = 2868; break;
                                    case eInventorySlot.TorsoArmor: model = 2865; break;
                                    case eInventorySlot.HandsArmor: model = 2870; break;
                                }
                                break;
                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }
                case eObjectType.Plate:
                    {
                        name = "Plate " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: model = 2817; break;
                            case eInventorySlot.LegsArmor: model = 2816; break;
                            case eInventorySlot.FeetArmor: model = 2819; break;
                            case eInventorySlot.HeadArmor:
                                if (Util.Chance(25))
                                {
                                    model = 93;
                                    name = "Plate Full Helm";
                                }
                                else
                                    model = 64;

                                break;

                            case eInventorySlot.TorsoArmor:
                                name = ArmorSlotToName(slot, type); // Breastplate
                                model = 2815;
                                break;

                            case eInventorySlot.HandsArmor: model = 49; break;
                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }
                case eObjectType.Chain:
                    {
                        name = "Chain " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }
                        switch (realm)
                        {
                            case eRealm.Albion:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 2811; break;
                                    case eInventorySlot.LegsArmor: model = 2810; break;
                                    case eInventorySlot.FeetArmor: model = 2813; break;
                                    case eInventorySlot.HeadArmor: model = 2812; break;
                                    case eInventorySlot.TorsoArmor: model = 2809; break;
                                    case eInventorySlot.HandsArmor: model = 2814; break;
                                }
                                break;

                            case eRealm.Midgard:
                                switch (slot)
                                {
                                    case eInventorySlot.ArmsArmor: model = 2873; break;
                                    case eInventorySlot.LegsArmor: model = 2872; break;
                                    case eInventorySlot.FeetArmor: model = 2875; break;
                                    case eInventorySlot.HeadArmor: model = 2874; break;
                                    case eInventorySlot.TorsoArmor: model = 2871; break;
                                    case eInventorySlot.HandsArmor: model = 2876; break;
                                }
                                break;
                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }
                case eObjectType.Reinforced:
                    {
                        name = "Reinforced " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: model = 2836; break;
                            case eInventorySlot.LegsArmor: model = 2835; break;
                            case eInventorySlot.FeetArmor: model = 2838; break;
                            case eInventorySlot.HeadArmor: model = 2837; break;
                            case eInventorySlot.TorsoArmor: model = 2834; break;
                            case eInventorySlot.HandsArmor: model = 2839; break;
                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }
                case eObjectType.Scale:
                    {
                        name = "Scale " + ArmorSlotToName(slot, type);

                        switch (Util.Random(0, 6))
                        {

                            case 0: proc = 49008; break; //lifedrain
                            case 1: proc = 13015; break; //heal over time
                            case 2: proc = 1711; break; //endureg proc 
                            case 3: proc = 62010; break; //direct damage proc
                            case 4: proc = 9676; break; //damage over time
                            case 5: proc = 1721; break;//combat speed buff
                            case 6: proc = 55012; break;//af proc

                        }
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: model = 2842; break;
                            case eInventorySlot.LegsArmor: model = 2841; break;
                            case eInventorySlot.FeetArmor: model = 2844; break;
                            case eInventorySlot.HeadArmor: model = 2843; break;
                            case eInventorySlot.TorsoArmor: model = 2840; break;
                            case eInventorySlot.HandsArmor: model = 2845; break;
                        }

                        if (slot != eInventorySlot.HeadArmor)
                            canAddExtension = true;

                        break;
                    }

                //weapons
                case eObjectType.Axe:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.Hand == 1)
                        {
                            if (item.SPD_ABS < 51)
                            {
                                name = "Large Axe";
                                model = 577;
                            }
                            else
                            {
                                name = "Great Axe";
                                model = 317;
                            }
                        }
                        else // 1 handed axe; speed 28-45; 578 (hand), 316 (Bearded), 319 (War), 315 (Spiked), 573 (Double)
                        {
                            if (item.SPD_ABS < 25)
                            {
                                name = "Hand Axe";
                                model = 578;
                            }
                            else if (item.SPD_ABS < 30)
                            {
                                name = "Bearded Axe";
                                model = 316;
                            }
                            else if (item.SPD_ABS < 36)
                            {
                                name = "War Axe";
                                model = 319;
                            }
                            else if (item.SPD_ABS < 40)
                            {
                                name = "Spiked Axe";
                                model = 315;
                            }
                            else
                            {
                                name = "Double-bladed Axe";
                                model = 573;
                            }
                        }
                        break;
                    }
                case eObjectType.Blades:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        // Blades; speed 22 - 45; Short Sword (445), Falcata (444), Broadsword (447), Longsword (446), Bastard Sword (473)
                        if (item.SPD_ABS < 27)
                        {
                            name = "Short Sword";
                            model = 445;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 30)
                        {
                            name = "Falcata";
                            model = 444;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 33)
                        {
                            name = "Broadsword";
                            model = 447;
                        }
                        else if (item.SPD_ABS < 40)
                        {
                            name = "Long Sword";
                            model = 446;
                        }
                        else
                        {
                            name = "Bastard Sword";
                            model = 473;
                        }
                        break;
                    }
                case eObjectType.Blunt:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        // Blunt; speed 22 - 45; Club (449), Mace (450), Hammer (461), Spiked Mace (451), Pick Hammer (641)

                        if (item.SPD_ABS < 30)
                        {
                            name = "Club";
                            model = 449;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 35)
                        {
                            name = "Mace";
                            model = 450;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 40)
                        {
                            name = "Hammer";
                            model = 461;
                        }
                        else if (item.SPD_ABS < 43)
                        {
                            name = "Spiked Mace";
                            model = 451;
                        }
                        else
                        {
                            name = "Pick Hammer";
                            model = 641;
                        }
                        break;
                    }
                case eObjectType.CelticSpear:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        // Short Spear (470), Spear (469), Long Spear (476), War Spear (477)
                        if (item.SPD_ABS < 35)
                        {
                            name = "Short Spear";
                            model = 470;
                        }
                        else if (item.SPD_ABS < 45)
                        {
                            name = "Spear";
                            model = 469;
                        }
                        else if (item.SPD_ABS < 50)
                        {
                            name = "Long Spear";
                            model = 476;
                        }
                        else
                        {
                            name = "War Spear";
                            model = 477;
                        }
                        break;
                    }
                case eObjectType.CompositeBow:
                    {
                        if (item.SPD_ABS > 40)
                            name = "Great Composite Bow";
                        else
                            name = "Composite Bow";

                        model = 564;
                        break;
                    }
                case eObjectType.Crossbow:
                    {
                        name = "Crossbow";
                        model = 226;
                        break;
                    }
                case eObjectType.CrushingWeapon:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        // Hammer (12), Mace (13), Flanged Mace (14), War Hammer (15)
                        if (item.SPD_ABS < 33)
                        {
                            name = "Hammer";
                            model = 12;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 35)
                        {
                            name = "Mace";
                            model = 13;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 40)
                        {
                            name = "Flanged Mace";
                            model = 14;
                        }
                        else
                        {
                            name = "War Hammer";
                            model = 15;
                        }
                        break;
                    }
                case eObjectType.Fired:
                    {
                        if (realm == eRealm.Albion)
                        {
                            name = "Short Bow";
                            model = 569;
                        }
                        else // hibernia
                        {
                            name = "Short Bow";
                            model = 922;
                        }
                        break;
                    }
                case eObjectType.Flexible:
                    {
                        switch (damage)
                        {
                            case eDamageType.Crush:
                                {
                                    switch (Util.Random(0, 5))
                                    {
                                        case 0: proc = 32183; break; //endutrain
                                        case 1: proc = 62010; break; //direct damage proc
                                        case 2: proc = 9676; break; //damage over time
                                        case 3: proc = 9651; break;//damage add use
                                        case 4: proc = 1721; break;//combat speed buff
                                        case 5: proc = 13045; break;//cele proc
                                    }
                                    if (item.SPD_ABS < 33)
                                    {
                                        name = "Morning Star";
                                        model = 862;
                                    }
                                    else if (item.SPD_ABS < 40)
                                    {
                                        name = "Flail";
                                        model = 861;
                                    }
                                    else
                                    {
                                        name = "Weighted Flail";
                                        model = 864;
                                    }
                                    break;
                                }
                            case eDamageType.Slash:
                                {
                                    switch (Util.Random(0, 5))
                                    {
                                        case 0: proc = 32183; break; //endutrain
                                        case 1: proc = 62010; break; //direct damage proc
                                        case 2: proc = 9676; break; //damage over time
                                        case 3: proc = 9651; break;//damage add use
                                        case 4: proc = 1721; break;//combat speed buff
                                        case 5: proc = 13045; break;//cele proc
                                    }
                                    if (item.SPD_ABS < 33)
                                    {
                                        name = "Whip";
                                        model = 867;
                                    }
                                    else if (item.SPD_ABS < 40)
                                    {
                                        name = "Chain";
                                        model = 857;
                                    }
                                    else
                                    {
                                        name = "War Chain";
                                        model = 866;
                                    }
                                    break;
                                }
                        }
                        break;

                    }
                case eObjectType.Hammer:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.Hand == 1)
                        {
                            if (item.SPD_ABS < 56)
                            {
                                name = "Two Handed Hammer";
                                model = 574;
                            }
                            else if (item.SPD_ABS < 53)
                            {
                                name = "Two Handed War Hammer";
                                model = 575;
                            }
                            else
                            {
                                name = "Great Hammer";
                                model = 576;
                            }
                        }
                        else
                        {
                            if (item.SPD_ABS < 30)
                            {
                                name = "Small Hammer";
                                model = 320;
                            }
                            else if (item.SPD_ABS < 35)
                            {
                                name = "Hammer";
                                model = 321;
                            }
                            else if (item.SPD_ABS < 40)
                            {
                                name = "Pick Hammer";
                                model = 323;
                            }
                            else
                            {
                                name = "Battle Hammer";
                                model = 324;
                            }
                        }
                        break;
                    }
                case eObjectType.HandToHand:
                    {
                        switch (damage)
                        {
                            case eDamageType.Slash:
                                {
                                    switch (Util.Random(0, 5))
                                    {
                                        case 0: proc = 32183; break; //endutrain
                                        case 1: proc = 62010; break; //direct damage proc
                                        case 2: proc = 9676; break; //damage over time
                                        case 3: proc = 9651; break;//damage add use
                                        case 4: proc = 1721; break;//combat speed buff
                                        case 5: proc = 13045; break;//cele proc
                                    }
                                    if (item.SPD_ABS < 30)
                                    {
                                        name = "Moon Claw";
                                        model = 981;
                                    }
                                    else if (item.SPD_ABS < 35)
                                    {
                                        name = "Bladed Moon Claw";
                                        model = 961;
                                    }
                                    else
                                    {
                                        name = "Heavy Bladed Moon Claw";
                                        model = 975;
                                    }
                                    break;
                                }
                            case eDamageType.Thrust:
                                {
                                    switch (Util.Random(0, 5))
                                    {
                                        case 0: proc = 32183; break; //endutrain
                                        case 1: proc = 62010; break; //direct damage proc
                                        case 2: proc = 9676; break; //damage over time
                                        case 3: proc = 9651; break;//damage add use
                                        case 4: proc = 1721; break;//combat speed buff
                                        case 5: proc = 13045; break;//cele proc
                                    }
                                    if (item.SPD_ABS < 30)
                                    {
                                        name = "Claw Greave";
                                        model = 963;
                                    }
                                    else if (item.SPD_ABS < 35)
                                    {
                                        name = "Bladed Claw Greave";
                                        model = 959;
                                    }
                                    else
                                    {
                                        name = "Heavy Bladed Claw Greave";
                                        model = 973;
                                    }
                                    break;
                                }
                        }
                        // all hand to hand weapons usable in left hand
                        item.Hand = 2; // allow left hand
                        item.Item_Type = Slot.LEFTHAND;
                        break;
                    }
                case eObjectType.Instrument:
                    {
                        switch (item.DPS_AF)
                        {
                            case 1:
                                {
                                    name = "Drum";
                                    model = 228;
                                    break;
                                }
                            case 2:
                                {
                                    name = "Lute";
                                    model = 227;
                                    break;
                                }
                            case 3:
                                {
                                    name = "Flute";
                                    model = 325;
                                    break;
                                }
                        }
                        break;
                    }
                case eObjectType.LargeWeapons:
                    {
                        switch (damage)
                        {
                            case eDamageType.Slash:
                                {
                                    switch (Util.Random(0, 5))
                                    {
                                        case 0: proc = 32183; break; //endutrain
                                        case 1: proc = 62010; break; //direct damage proc
                                        case 2: proc = 9676; break; //damage over time
                                        case 3: proc = 9651; break;//damage add use
                                        case 4: proc = 1721; break;//combat speed buff
                                        case 5: proc = 13045; break;//cele proc
                                    }
                                    if (item.SPD_ABS < 50)
                                    {
                                        name = "Great Falcata";
                                        model = 639;
                                    }
                                    else
                                    {
                                        name = "Great Sword";
                                        model = 459;
                                    }
                                    break;
                                }
                            case eDamageType.Crush:
                                {
                                    switch (Util.Random(0, 5))
                                    {
                                        case 0: proc = 32183; break; //endutrain
                                        case 1: proc = 62010; break; //direct damage proc
                                        case 2: proc = 9676; break; //damage over time
                                        case 3: proc = 9651; break;//damage add use
                                        case 4: proc = 1721; break;//combat speed buff
                                        case 5: proc = 13045; break;//cele proc
                                    }
                                    if (item.SPD_ABS < 50)
                                    {
                                        name = "Big Shillelagh";
                                        model = 474;
                                    }
                                    else
                                    {
                                        name = "Great Hammer";
                                        model = 462;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case eObjectType.LeftAxe:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS < 25)
                        {
                            name = "Hand Axe";
                            model = 578;
                        }
                        else if (item.SPD_ABS < 30)
                        {
                            name = "Bearded Axe";
                            model = 316;
                        }
                        else
                        {
                            name = "War Axe";
                            model = 319;
                        }
                        break;
                    }
                case eObjectType.Longbow:
                    {
                        if (item.SPD_ABS < 44)
                        {
                            name = "Hunting Bow";
                            model = 569;
                        }
                        else if (item.SPD_ABS < 55)
                        {
                            name = "Longbow";
                            model = 132;
                        }
                        else
                        {
                            name = "Heavy Longbow";
                            model = 570;
                        }
                        break;
                    }
                case eObjectType.Magical:
                    {
                        switch (slot)
                        {
                            case eInventorySlot.Cloak:
                                {
                                    switch (Util.Random(0, 7))
                                    {

                                        case 0: spell = 5461; break; //lifeleech
                                        case 1: spell = 5438; break; // root
                                        case 2: spell = 65200; break; //power auffüllen
                                        case 3: spell = 13015; break; //heal over time
                                        case 4: spell = 1711; break; //endureg proc 
                                        case 5: spell = 3542; break; //direct damage use
                                        case 6: spell = 9651; break;//damage add use
                                        case 7: spell = 1721; break;//combat speed buff
                                    }
                                    if (Util.Chance(50))
                                        name = "Mantle";
                                    else
                                        name = "Cloak";

                                    if (Util.Chance(50))
                                        model = 57;
                                    else if (Util.Chance(50))
                                        model = 559;
                                    else
                                        model = 560;

                                    break;
                                }
                            case eInventorySlot.Waist:
                                {
                                    switch (Util.Random(1, 7))
                                    {
                                        case 1: spell = 5438; break; // root
                                        case 2: spell = 65200; break; //power auffüllen
                                        case 3: spell = 13015; break; //heal over time
                                        case 4: spell = 1711; break; //endureg proc 
                                        case 5: spell = 3542; break; //direct damage use
                                        case 6: spell = 9651; break;//damage add use
                                        case 7: spell = 1721; break;//combat speed buff
                                    }
                                    if (Util.Chance(50))
                                        name = "Belt";
                                    else
                                        name = "Girdle";

                                    model = 597;
                                    break;
                                }
                            case eInventorySlot.Neck:
                                {
                                    switch (Util.Random(1, 7))
                                    {
                                        case 1: spell = 5438; break; // root
                                        case 2: spell = 65200; break; //power auffüllen
                                        case 3: spell = 13015; break; //heal over time
                                        case 4: spell = 1711; break; //endureg proc 
                                        case 5: spell = 3542; break; //direct damage use
                                        case 6: spell = 9651; break;//damage add use
                                        case 7: spell = 1721; break;//combat speed buff
                                    }
                                    if (Util.Chance(50))
                                        name = "Choker";
                                    else
                                        name = "Pendant";

                                    model = 101;
                                    break;
                                }
                            case eInventorySlot.Jewellery:
                                {
                                    switch (Util.Random(1, 7))
                                    {
                                        case 1: spell = 5438; break; // root
                                        case 2: spell = 65200; break; //power auffüllen
                                        case 3: spell = 13015; break; //heal over time
                                        case 4: spell = 1711; break; //endureg proc 
                                        case 5: spell = 3542; break; //direct damage use
                                        case 6: spell = 9651; break;//damage add use
                                        case 7: spell = 1721; break;//combat speed buff
                                    }
                                    if (Util.Chance(50))
                                        name = "Gem";
                                    else
                                        name = "Jewel";

                                    model = Util.Random(110, 119);
                                    break;
                                }
                            case eInventorySlot.LeftBracer:
                            case eInventorySlot.RightBracer:
                                {
                                    switch (Util.Random(1, 7))
                                    {
                                        case 1: spell = 5438; break; // root
                                        case 2: spell = 65200; break; //power auffüllen
                                        case 3: spell = 13015; break; //heal over time
                                        case 4: spell = 1711; break; //endureg proc 
                                        case 5: spell = 3542; break; //direct damage use
                                        case 6: spell = 9651; break;//damage add use
                                        case 7: spell = 1721; break;//combat speed buff
                                    }
                                    if (Util.Chance(50))
                                    {
                                        name = "Bracelet";
                                        model = 619;
                                    }
                                    else
                                    {
                                        name = "Bracer";
                                        model = 598;
                                    }

                                    break;
                                }
                            case eInventorySlot.LeftRing:
                            case eInventorySlot.RightRing:
                                {
                                    switch (Util.Random(1, 7))
                                    {
                                        case 1: spell = 5438; break; // root
                                        case 2: spell = 65200; break; //power auffüllen
                                        case 3: spell = 13015; break; //heal over time
                                        case 4: spell = 1711; break; //endureg proc 
                                        case 5: spell = 3542; break; //direct damage use
                                        case 6: spell = 9651; break;//damage add use
                                        case 7: spell = 1721; break;//combat speed buff
                                    }
                                    if (Util.Chance(55))
                                        name = "Ring";
                                    else
                                        name = "Wrap";

                                    model = 103;
                                    break;
                                }
                        }
                        break;
                    }
                case eObjectType.Piercing:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS < 24)
                        {
                            name = "Dirk";
                            model = 454;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 27)
                        {
                            name = "Stiletto";
                            model = 456;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 30)
                        {
                            name = "Curved Dagger";
                            model = 457;
                        }
                        else
                        {
                            name = "Rapier";
                            model = 455;
                        }
                        break;
                    }
                case eObjectType.PolearmWeapon:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        switch (damage)
                        {
                            case eDamageType.Slash:
                                {
                                    name = "Lochaber Axe";
                                    model = 68;
                                    break;
                                }
                            case eDamageType.Thrust:
                                {
                                    name = "Pike";
                                    model = 69;
                                    break;
                                }
                            case eDamageType.Crush:
                                {
                                    name = "Lucerne Hammer";
                                    model = 70;
                                    break;
                                }
                        }
                        break;
                    }
                case eObjectType.RecurvedBow:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS > 49)
                        {
                            name = "Great Recurve Bow";
                            model = 925;
                        }
                        else
                        {
                            name = "Recurve Bow";
                            model = 924;
                        }
                        break;
                    }
                case eObjectType.Scythe:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS < 47)
                        {
                            name = "Scythe";
                            model = 931;
                        }
                        else if (item.SPD_ABS < 51)
                        {
                            name = "Martial Scythe";
                            model = 930;
                        }
                        else
                        {
                            name = "War Scythe";
                            model = 932;
                        }
                        break;
                    }
                case eObjectType.Shield:
                    {

                        switch (Util.Random(1, 7))
                        {
                            case 1: spell = 5438; break; // root
                            case 2: spell = 65200; break; //power auffüllen
                            case 3: spell = 13015; break; //heal over time
                            case 4: spell = 1711; break; //endureg proc 
                            case 5: spell = 3542; break; //direct damage use
                            case 6: spell = 9651; break;//damage add use
                            case 7: spell = 1721; break;//combat speed buff
                        }

                        switch ((int)damage)
                        {
                            case 1:
                                {
                                    name = "Small Shield";
                                    model = 59;
                                    break;
                                }
                            case 2:
                                {
                                    name = "Medium Shield";
                                    model = 61;
                                    break;
                                }
                            case 3:
                                {
                                    name = "Large Shield";
                                    model = 60;
                                    break;
                                }
                        }
                        break;
                    }
                case eObjectType.SlashingWeapon:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS < 26)
                        {
                            name = "Dagger";
                            model = 1;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 30)
                        {
                            if (Util.Chance(25))
                            {
                                name = "Jambiya";
                                model = 651;
                            }
                            else
                            {
                                name = "Short Sword";
                                model = 3;
                            }
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 32)
                        {
                            name = "Broadsword";
                            model = 5;
                        }
                        else if (item.SPD_ABS < 35)
                        {
                            name = "Scimitar";
                            model = 8;
                        }
                        else if (item.SPD_ABS < 40)
                        {
                            name = "Long Sword";
                            model = 4;
                        }
                        else
                        {
                            name = "Bastard Sword";
                            model = 10;
                        }
                        break;
                    }
                case eObjectType.Spear:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS < 43)
                        {
                            name = "Spear";
                            model = 328;
                        }
                        else if (item.SPD_ABS < 50)
                        {
                            name = "Long Spear";
                            model = 329;
                        }
                        else
                        {
                            name = "Great Spear";
                            model = 332;
                        }
                        break;
                    }
                case eObjectType.MaulerStaff:
                    {
                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        name = "Mauler Staff";
                        model = 19;
                        break;
                    }
                case eObjectType.Staff:
                    {
                        switch (Util.Random(1, 7))
                        {
                            case 1: spell = 5438; break; // root
                            case 2: spell = 65200; break; //power auffüllen
                            case 3: spell = 13015; break; //heal over time
                            case 4: spell = 1711; break; //endureg proc 
                            case 5: spell = 3542; break; //direct damage use
                            case 6: spell = 9651; break;//damage add use
                            case 7: spell = 1721; break;//combat speed buff
                        }
                        switch (realm)
                        {
                            case eRealm.Albion:

                                if (Util.Chance(20))
                                {
                                    item.Description = "friar";

                                    if (item.SPD_ABS < 40)
                                    {
                                        name = "Quarterstaff";
                                        model = 442;
                                    }
                                    else if (item.SPD_ABS < 50)
                                    {
                                        name = "Shod Quarterstaff";
                                        model = 567;
                                    }
                                    else
                                    {
                                        name = "Heavy Shod Quarterstaff";
                                        model = 884;
                                    }
                                }
                                else
                                {
                                    if (item.SPD_ABS < 40)
                                    {
                                        name = "Staff";
                                        model = 568;
                                        item.Bonus1Type = (int)eProperty.AllFocusLevels;
                                        item.Bonus1 = item.Level;
                                        if (item.Bonus1 == 0)
                                            item.Bonus1 = 1;
                                    }
                                    else
                                    {
                                        name = "Staff";
                                        model = 19;
                                        item.Bonus1Type = (int)eProperty.AllFocusLevels;
                                        item.Bonus1 = item.Level;
                                        if (item.Bonus1 == 0)
                                            item.Bonus1 = 1;
                                    }
                                }
                                break;

                            case eRealm.Midgard:

                                if (item.SPD_ABS < 40)
                                {
                                    name = "Staff";
                                    model = 327;
                                    item.Bonus1Type = (int)eProperty.AllFocusLevels;
                                    item.Bonus1 = item.Level;
                                    if (item.Bonus1 == 0)
                                        item.Bonus1 = 1;
                                }
                                else
                                {
                                    name = "Staff";
                                    model = 565;
                                    item.Bonus1Type = (int)eProperty.AllFocusLevels;
                                    item.Bonus1 = item.Level;
                                    if (item.Bonus1 == 0)
                                        item.Bonus1 = 1;
                                }
                                break;

                            case eRealm.Hibernia:

                                name = "Staff";
                                model = 468;
                                item.Bonus1Type = (int)eProperty.AllFocusLevels;
                                item.Bonus1 = item.Level;
                                if (item.Bonus1 == 0)
                                    item.Bonus1 = 1;
                                break;
                        }
                        break;
                    }
                case eObjectType.Sword:
                    {

                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.Hand == 1)
                        {
                            if (item.SPD_ABS > 46)
                            {
                                name = "Great Sword";
                                model = 572;
                            }
                            else
                            {
                                name = "Two Handed Sword";
                                model = 314;
                            }
                        }
                        else
                        {
                            if (item.SPD_ABS < 25)
                            {
                                name = "Dagger";
                                model = 571;
                            }
                            else if (item.SPD_ABS < 30)
                            {
                                name = "Short Sword";
                                model = 311;
                            }
                            else if (item.SPD_ABS < 32)
                            {
                                name = "Broadsword";
                                model = 312;
                            }
                            else if (item.SPD_ABS < 35)
                            {
                                name = "Long Sword";
                                model = 310;
                            }
                            else
                            {
                                name = "Bastard Sword";
                                model = 313;
                            }
                        }
                        break;
                    }
                case eObjectType.ThrustWeapon:
                    {

                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        if (item.SPD_ABS < 24)
                        {
                            name = "Dirk";
                            model = 21;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 28)
                        {
                            name = "Stiletto";
                            model = 71;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 30)
                        {
                            name = "Main Gauche";
                            model = 25;
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 36)
                        {
                            name = "Rapier";
                            model = 22;
                        }
                        else
                        {
                            name = "Gladius";
                            model = 30;
                        }
                        break;
                    }
                case eObjectType.TwoHandedWeapon:
                    {

                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        switch (damage)
                        {
                            case eDamageType.Slash:
                                {
                                    if (item.SPD_ABS < 44)
                                    {
                                        name = "Two Handed Sword";
                                        model = 6;
                                    }
                                    else if (item.SPD_ABS < 48)
                                    {
                                        name = "Great Axe";
                                        model = 72;
                                    }
                                    else if (item.SPD_ABS < 51)
                                    {
                                        name = "Great Scimitar";
                                        model = 645;
                                    }
                                    else
                                    {
                                        name = "Great Sword";
                                        model = 7;
                                    }
                                    break;
                                }
                            case eDamageType.Crush:
                                {
                                    name = "Great Hammer";
                                    model = 17;
                                    break;
                                }
                            case eDamageType.Thrust:
                                {
                                    if (item.SPD_ABS < 46)
                                    {
                                        name = "War Mattock";
                                        model = 16;
                                    }
                                    else
                                    {
                                        name = "War Pick";
                                        model = 646;
                                    }
                                    break;
                                }
                        }
                        break;
                    }
                case eObjectType.FistWraps: // Maulers
                    {

                        switch (Util.Random(0, 5))
                        {
                            case 0: proc = 32183; break; //endutrain
                            case 1: proc = 62010; break; //direct damage proc
                            case 2: proc = 9676; break; //damage over time
                            case 3: proc = 9651; break;//damage add use
                            case 4: proc = 1721; break;//combat speed buff
                            case 5: proc = 13045; break;//cele proc
                        }
                        string str = "Fist";

                        if (Util.Chance(50))
                            str = "Hand";

                        if (item.SPD_ABS < 31)
                        {
                            name = str + " Wrap";
                            model = 3476;
                            item.Effect = 102; // smoke
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else if (item.SPD_ABS < 35)
                        {
                            name = "Studded " + str + " Wrap";
                            model = 3477;
                            item.Effect = 48; // fire
                            item.Hand = 2; // allow left hand
                            item.Item_Type = Slot.LEFTHAND;
                        }
                        else
                        {
                            name = "Spiked Fist Wrap";
                            model = 3478;
                            item.Effect = 49; // sparkle fire
                        }

                        break;
                    }
            }


           

            item.Name = name + " of " + killer.CurrentZone.Description;




            if (item.Level > 19)
            {
                item.SpellID = spell;
                item.Charges = charges;
                item.MaxCharges = maxcharges;
                item.ProcSpellID = proc;
                item.ProcChance = 10;
            }

            item.Model = model;

            if (canAddExtension)
            {
                if (item.Level >= 50)
                    item.Extension = 3;
                if (item.Level > 35)
                    item.Extension = 2;
            }
        }


        private static void GenerateItemStats(ItemUnique item)
        {
            eObjectType type = (eObjectType)item.Object_Type;

            //special property for instrument
            if (type == eObjectType.Instrument)
                item.DPS_AF = Util.Random(1, 3);

            //set hand
            switch (type)
            {
                //two handed weapons
                case eObjectType.CelticSpear:
                case eObjectType.CompositeBow:
                case eObjectType.Crossbow:
                case eObjectType.Fired:
                case eObjectType.Instrument:
                case eObjectType.LargeWeapons:
                case eObjectType.Longbow:
                case eObjectType.PolearmWeapon:
                case eObjectType.RecurvedBow:
                case eObjectType.Scythe:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.TwoHandedWeapon:
                case eObjectType.MaulerStaff: //Maulers
                    {
                        item.Hand = 1;
                        break;
                    }
                //right or left handed weapons
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.CrushingWeapon:
                case eObjectType.HandToHand:
                case eObjectType.Piercing:
                case eObjectType.SlashingWeapon:
                case eObjectType.ThrustWeapon:
                case eObjectType.FistWraps: //Maulers
                    {
                        if ((eInventorySlot)item.Item_Type == eInventorySlot.LeftHandWeapon)
                            item.Hand = 2;
                        break;
                    }
                //left handed weapons
                case eObjectType.LeftAxe:
                case eObjectType.Shield:
                    {
                        item.Hand = 2;
                        break;
                    }
                //right or two handed weapons
                case eObjectType.Sword:
                case eObjectType.Hammer:
                case eObjectType.Axe:
                    {
                        if ((eInventorySlot)item.Item_Type == eInventorySlot.TwoHandWeapon)
                            item.Hand = 1;
                        break;
                    }
            }

            //set dps_af and spd_abs
            if ((int)type >= (int)eObjectType._FirstArmor && (int)type <= (int)eObjectType._LastArmor)
            {
                if (type == eObjectType.Cloth)
                    item.DPS_AF = item.Level;
                else item.DPS_AF = item.Level * 2;
                item.SPD_ABS = GetAbsorb(type);
            }
            switch (type)
            {
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.CelticSpear:
                case eObjectType.CompositeBow:
                case eObjectType.Crossbow:
                case eObjectType.CrushingWeapon:
                case eObjectType.Fired:
                case eObjectType.Flexible:
                case eObjectType.Hammer:
                case eObjectType.HandToHand:
                case eObjectType.LargeWeapons:
                case eObjectType.LeftAxe:
                case eObjectType.Longbow:
                case eObjectType.Piercing:
                case eObjectType.PolearmWeapon:
                case eObjectType.RecurvedBow:
                case eObjectType.Scythe:
                case eObjectType.Shield:
                case eObjectType.SlashingWeapon:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.Sword:
                case eObjectType.ThrustWeapon:
                case eObjectType.TwoHandedWeapon:
                case eObjectType.MaulerStaff: //Maulers
                case eObjectType.FistWraps: //Maulers
                    {
                        item.DPS_AF = (int)(((item.Level * 0.3) + 1.2) * 10);
                        item.SPD_ABS = GetWeaponSpeed(item);
                        break;
                    }
            }
        }

        public enum eBonusType
        {
            Stat,
            AdvancedStat,
            Resist,
            Skill,
            Focus,
        }

        private static void GenerateMagicalBonuses(ItemUnique item, GameNPC mob)
        {
            // unique objects have more bonuses as level rises

            int number = 0;

            if (Util.Chance(20 + item.Level * 2)) // 100% magical starting at level 40
            {
                //1
                number++;

                if (Util.Chance(item.Level * 8 - 40)) // level 6 - 17 (100%)
                {
                    //2
                    number++;

                    if (Util.Chance(item.Level * 6 - 60)) // level 11 - 27 (100%)
                    {
                        //3
                        number++;

                        if (Util.Chance(item.Level * 4 - 80)) // level 21 - 45 (100%)
                        {
                            //4
                            number++;

                            if (number == 4 && item.Level > 50 && IsMobInTOA(mob))
                                number++; // 5
                        }
                    }
                }

            }

            // Magical items have at least 1 bonus
            if (item.Object_Type == (int)eObjectType.Magical && number < 1)
                number = 1;


            bool fNamed = false;
            bool fAddedBonus = false;

            double quality = (double)item.Quality * .01;

            double multiplier = (quality * quality * quality) + 0.15;

            if (IsMobInTOA(mob))
            {
                multiplier += 0.15;
            }

            for (int i = 0; i < number; i++)
            {
                eBonusType type = GetPropertyType(item, mob);
                eProperty property = GetProperty(type, item);
                if (!BonusExists(item, property))
                {
                    int amount = (int)Math.Ceiling((double)GetBonusAmount(type, property, item.Level) * multiplier);
                    WriteBonus(item, property, amount);
                    fAddedBonus = true;
                    if (!fNamed && WriteMagicalName(property, item))
                    {
                        fNamed = true;
                        multiplier *= 0.65;
                    }
                }
            }

            // non magical items get lowercase names
            if (number == 0 || !fAddedBonus)
                item.Name = item.Name.ToLower();
        }

        private static int GenerateItemQuality(double conlevel, ItemUnique item)
        {
            int minQuality = 89 + Math.Max(0, item.Level - 46);
            int maxQuality = (int)(1.310 * conlevel + 94.29 + Math.Max(0, item.Level - 50));
            maxQuality = Math.Min(maxQuality, 99);  // unique objects capped at 99 quality
            maxQuality = Math.Max(maxQuality, minQuality);

            int quality = Util.Random(minQuality, maxQuality);

            return quality;
        }

        private static eBonusType GetPropertyType(ItemUnique item, GameNPC mob)
        {
            //allfocus
            if (CanAddFocus(item))
                return eBonusType.Focus;

            // ToA allows stat cap bonuses
            if (IsMobInTOA(mob) && mob.Name.ToLower() != mob.Name && mob.Level > 50 && Util.Chance(20))
            {
                return eBonusType.AdvancedStat;
            }

            //stats
            if (Util.Chance(50))
                return eBonusType.Stat;
            //resists
            if (Util.Chance(40))
                return eBonusType.Resist;
            //skills
            return eBonusType.Skill;
        }

        // Only try to add focus once
        private static bool CanAddFocus(ItemUnique item)
        {
            if (item.Object_Type == (int)eObjectType.Staff)
            {
                if (item.Bonus1Type != 0)
                    return false;

                if (item.Realm == (int)eRealm.Albion && item.Description == "friar")
                    return false;

                return true;
            }

            return false;
        }

        private static eProperty GetProperty(eBonusType type, ItemUnique item)
        {
            switch (type)
            {
                case eBonusType.Focus:
                    {
                        return eProperty.AllFocusLevels;
                    }
                case eBonusType.Resist:
                    {
                        return (eProperty)Util.Random((int)eProperty.Resist_First, (int)eProperty.Resist_Last);
                    }
                case eBonusType.Skill:
                    {
                        // fill valid skills
                        ArrayList validSkills = new ArrayList();

                        bool fIndividualSkill = false;

                        // All Skills is never combined with any other skill
                        if (!BonusExists(item, eProperty.AllSkills))
                        {
                            // All type skills never combined with individual skills
                            if (!BonusExists(item, eProperty.AllMagicSkills) &&
                                !BonusExists(item, eProperty.AllMeleeWeaponSkills) &&
                                !BonusExists(item, eProperty.AllDualWieldingSkills) &&
                                !BonusExists(item, eProperty.AllArcherySkills))
                            {
                                // individual realm specific skills
                                if ((eRealm)item.Realm == eRealm.Albion)
                                {
                                    foreach (eProperty property in AlbSkillBonus)
                                    {
                                        if (!BonusExists(item, property))
                                        {
                                            if (SkillIsValidForObjectType(item, property))
                                                validSkills.Add(property);
                                        }
                                        else
                                            fIndividualSkill = true;
                                    }
                                }
                                else if ((eRealm)item.Realm == eRealm.Hibernia)
                                {
                                    foreach (eProperty property in HibSkillBonus)
                                    {
                                        if (!BonusExists(item, property))
                                        {
                                            if (SkillIsValidForObjectType(item, property))
                                                validSkills.Add(property);
                                        }
                                        else
                                            fIndividualSkill = true;
                                    }
                                }
                                else if ((eRealm)item.Realm == eRealm.Midgard)
                                {
                                    foreach (eProperty property in MidSkillBonus)
                                    {
                                        if (!BonusExists(item, property))
                                        {
                                            if (SkillIsValidForObjectType(item, property))
                                                validSkills.Add(property);
                                        }
                                        else
                                            fIndividualSkill = true;
                                    }
                                }

                                if (!fIndividualSkill)
                                {
                                    // ok to add AllSkills, but reduce the chance
                                    if (SkillIsValidForObjectType(item, eProperty.AllSkills) && Util.Chance(25))
                                        validSkills.Add(eProperty.AllSkills);
                                }
                            }

                            // All type skills never combined with individual skills
                            if (!fIndividualSkill)
                            {
                                if (!BonusExists(item, eProperty.AllMagicSkills) && SkillIsValidForObjectType(item, eProperty.AllMagicSkills))
                                    validSkills.Add(eProperty.AllMagicSkills);

                                if (!BonusExists(item, eProperty.AllMeleeWeaponSkills) && SkillIsValidForObjectType(item, eProperty.AllMeleeWeaponSkills))
                                    validSkills.Add(eProperty.AllMeleeWeaponSkills);

                                if (!BonusExists(item, eProperty.AllDualWieldingSkills) && SkillIsValidForObjectType(item, eProperty.AllDualWieldingSkills))
                                    validSkills.Add(eProperty.AllDualWieldingSkills);

                                if (!BonusExists(item, eProperty.AllArcherySkills) && SkillIsValidForObjectType(item, eProperty.AllArcherySkills))
                                    validSkills.Add(eProperty.AllArcherySkills);
                            }

                        }

                        int index;
                        index = validSkills.Count - 1;

                        if (index < 1)
                        {
                            // return a safe random stat

                            switch (Util.Random(0, 4))
                            {
                                case 0:
                                    return eProperty.MaxHealth;
                                case 1:
                                    return eProperty.Strength;
                                case 2:
                                    return eProperty.Dexterity;
                                case 3:
                                    return eProperty.Quickness;
                                case 4:
                                    return eProperty.Constitution;
                            }
                        }

                        return (eProperty)validSkills[Util.Random(0, index)];
                    }
                case eBonusType.Stat:
                    {
                        // ToDo: this does not check for duplicates like INT and Acuity
                        ArrayList validStats = new ArrayList();
                        foreach (eProperty property in StatBonus)
                        {
                            if (!BonusExists(item, property) && StatIsValidForObjectType(item, property) && StatIsValidForRealm((eRealm)item.Realm, property))
                            {
                                validStats.Add(property);
                            }
                        }
                        return (eProperty)validStats[Util.Random(0, validStats.Count - 1)];
                    }
                case eBonusType.AdvancedStat:
                    {
                        // ToDo: this does not check for duplicates like INT and Acuity
                        ArrayList validStats = new ArrayList();
                        foreach (eProperty property in AdvancedStats)
                        {
                            if (!BonusExists(item, property) && StatIsValidForObjectType(item, property) && StatIsValidForRealm((eRealm)item.Realm, property))
                                validStats.Add(property);
                        }
                        return (eProperty)validStats[Util.Random(0, validStats.Count - 1)];
                    }
            }
            return eProperty.MaxHealth;
        }

        private static bool StatIsValidForObjectType(ItemUnique item, eProperty property)
        {
            switch ((eObjectType)item.Object_Type)
            {
                case eObjectType.Magical: return StatIsValidForRealm((eRealm)item.Realm, property);
                case eObjectType.Cloth:
                case eObjectType.Leather:
                case eObjectType.Studded:
                case eObjectType.Reinforced:
                case eObjectType.Chain:
                case eObjectType.Scale:
                case eObjectType.Plate: return StatIsValidForArmor(item, property);
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.CelticSpear:
                case eObjectType.CompositeBow:
                case eObjectType.Crossbow:
                case eObjectType.CrushingWeapon:
                case eObjectType.Fired:
                case eObjectType.Flexible:
                case eObjectType.Hammer:
                case eObjectType.HandToHand:
                case eObjectType.Instrument:
                case eObjectType.LargeWeapons:
                case eObjectType.LeftAxe:
                case eObjectType.Longbow:
                case eObjectType.Piercing:
                case eObjectType.PolearmWeapon:
                case eObjectType.RecurvedBow:
                case eObjectType.Scythe:
                case eObjectType.Shield:
                case eObjectType.SlashingWeapon:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.Sword:
                case eObjectType.ThrustWeapon:
                case eObjectType.FistWraps: //Maulers
                case eObjectType.MaulerStaff: //Maulers
                case eObjectType.TwoHandedWeapon: return StatIsValidForWeapon(item, property);
            }
            return true;
        }

        private static bool SkillIsValidForObjectType(ItemUnique item, eProperty property)
        {
            switch ((eObjectType)item.Object_Type)
            {
                case eObjectType.Magical: return true;
                case eObjectType.Cloth:
                case eObjectType.Leather:
                case eObjectType.Studded:
                case eObjectType.Reinforced:
                case eObjectType.Chain:
                case eObjectType.Scale:
                case eObjectType.Plate: return SkillIsValidForArmor(item, property);
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.CelticSpear:
                case eObjectType.CompositeBow:
                case eObjectType.Crossbow:
                case eObjectType.CrushingWeapon:
                case eObjectType.Fired:
                case eObjectType.Flexible:
                case eObjectType.Hammer:
                case eObjectType.HandToHand:
                case eObjectType.Instrument:
                case eObjectType.LargeWeapons:
                case eObjectType.LeftAxe:
                case eObjectType.Longbow:
                case eObjectType.Piercing:
                case eObjectType.PolearmWeapon:
                case eObjectType.RecurvedBow:
                case eObjectType.Scythe:
                case eObjectType.Shield:
                case eObjectType.SlashingWeapon:
                case eObjectType.Spear:
                case eObjectType.Staff:
                case eObjectType.Sword:
                case eObjectType.ThrustWeapon:
                case eObjectType.MaulerStaff:
                case eObjectType.FistWraps:
                case eObjectType.TwoHandedWeapon: return SkillIsValidForWeapon(item, property);
            }
            return true;
        }

        private static bool SkillIsValidForArmor(ItemUnique item, eProperty property)
        {
            int level = item.Level;
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (property)
            {
                case eProperty.Skill_Augmentation:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Axe:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;

                        return false;
                    }
                case eProperty.Skill_Battlesongs:
                    {
                        if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Pathfinding:
                case eProperty.Skill_BeastCraft:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Blades:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Reinforced || type == eObjectType.Scale)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Blunt:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Reinforced || type == eObjectType.Scale)
                            return true;
                        return false;
                    }
                //Cloth skills
                case eProperty.Skill_Arboreal:
                case eProperty.Skill_Body:
                case eProperty.Skill_BoneArmy:
                case eProperty.Skill_Cold:
                case eProperty.Skill_Creeping:
                case eProperty.Skill_Cursing:
                case eProperty.Skill_Darkness:
                case eProperty.Skill_Death_Servant:
                case eProperty.Skill_DeathSight:
                case eProperty.Skill_Earth:
                case eProperty.Skill_Enchantments:
                case eProperty.Skill_EtherealShriek:
                case eProperty.Skill_Fire:
                case eProperty.Skill_Hexing:
                case eProperty.Skill_Light:
                case eProperty.Skill_Mana:
                case eProperty.Skill_Matter:
                case eProperty.Skill_Mentalism:
                case eProperty.Skill_Mind:
                case eProperty.Skill_Pain_working:
                case eProperty.Skill_PhantasmalWail:
                case eProperty.Skill_Runecarving:
                case eProperty.Skill_SpectralForce:
                case eProperty.Skill_Spirit:
                case eProperty.Skill_Summoning:
                case eProperty.Skill_Suppression:
                case eProperty.Skill_Verdant:
                case eProperty.Skill_Void:
                case eProperty.Skill_Wind:
                case eProperty.Skill_Witchcraft:
                    {
                        if (type == eObjectType.Cloth)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Celtic_Dual:
                    {
                        if (type == eObjectType.Leather ||
                            type == eObjectType.Reinforced)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Celtic_Spear:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Reinforced)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Scale)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Chants:
                    {
                        return false;
                    }
                case eProperty.Skill_Composite:
                case eProperty.Skill_RecurvedBow:
                case eProperty.Skill_Long_bows:
                case eProperty.Skill_Archery:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;

                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Studded || type == eObjectType.Reinforced)
                                return true;

                            return false;
                        }
                    }
                case eProperty.Skill_Critical_Strike:
                case eProperty.Skill_Envenom:
                case eProperty.Skill_Dementia:
                case eProperty.Skill_Nightshade:
                case eProperty.Skill_ShadowMastery:
                case eProperty.Skill_VampiiricEmbrace:
                    {
                        if (type == eObjectType.Leather)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Cross_Bows:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Plate)
                                return true;
                            return false;
                        }
                    }

                case eProperty.Skill_Crushing:
                    {
                        if (realm == eRealm.Albion && type == eObjectType.Cloth) // heretic
                            return true;

                        if (level < 15)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain || type == eObjectType.Plate)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Dual_Wield:
                    {
                        if (level < 20)
                        {
                            if (type == eObjectType.Leather || type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Leather || type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Enhancement:
                    {
                        // friar
                        if (type == eObjectType.Leather)
                            return true;

                        if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Flexible_Weapon:
                    {
                        if (type == eObjectType.Cloth) // Heretic
                            return true;

                        if (level < 10)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Hammer:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_HandToHand:
                    {
                        if (type == eObjectType.Studded)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Instruments:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Large_Weapon:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Reinforced)
                                return true;

                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Scale)
                                return true;

                            return false;
                        }
                    }
                case eProperty.Skill_Left_Axe:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded)
                            return true;
                        break;
                    }
                case eProperty.Skill_Music:
                    {
                        if (level < 15)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Reinforced)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Nature:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Reinforced)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Scale)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Nurture:
                case eProperty.Skill_Regrowth:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Reinforced || type == eObjectType.Scale)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_OdinsWill:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Pacification:
                    {
                        if (level < 10)
                        {
                            if (type == eObjectType.Leather)
                                return true;
                            return false;
                        }
                        else if (level < 20)
                        {
                            if (type == eObjectType.Studded)
                                return true;
                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Chain)
                                return true;
                            return false;
                        }
                    }
                case eProperty.Skill_Parry:
                    {
                        if (type == eObjectType.Cloth && realm == eRealm.Hibernia && level >= 5)
                            return true;
                        else if (realm == eRealm.Hibernia && level < 2)
                            return false;
                        else if (realm == eRealm.Albion && level < 5)
                            return false;
                        else if (realm == eRealm.Albion && level < 10 && type == eObjectType.Studded)
                            return true;
                        else if (realm == eRealm.Albion && level >= 10 && (type == eObjectType.Leather || type == eObjectType.Chain || type == eObjectType.Plate))
                            return true;
                        else if (realm == eRealm.Hibernia && level < 20 && type == eObjectType.Reinforced)
                            return true;
                        else if (realm == eRealm.Hibernia && level >= 15 && type == eObjectType.Scale)
                            return true;
                        else if (realm == eRealm.Midgard && (type == eObjectType.Studded || type == eObjectType.Chain))
                            return true;

                        break;
                    }
                case eProperty.Skill_Piercing:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Reinforced)
                            return true;
                        return false;
                    }
                case eProperty.Skill_Polearms:
                    {
                        if (level < 5 && type == eObjectType.Studded)
                        {
                            return true;
                        }
                        else if (level < 15)
                        {
                            if (type == eObjectType.Chain)
                                return true;

                            return false;
                        }
                        else
                        {
                            if (type == eObjectType.Plate)
                                return true;

                            return false;
                        }
                    }
                case eProperty.Skill_Rejuvenation:
                    {
                        if (type == eObjectType.Cloth)
                            return true;
                        else if (type == eObjectType.Leather)
                            return true;
                        else if (type == eObjectType.Studded && level >= 10 && level < 20)
                            return true;
                        else if (type == eObjectType.Chain && level >= 20)
                            return true;
                        break;
                    }
                case eProperty.Skill_Savagery:
                    {
                        if (type == eObjectType.Studded)
                            return true;
                        break;
                    }
                case eProperty.Skill_Scythe:
                    {
                        if (type == eObjectType.Cloth)
                            return true;
                        break;
                    }
                case eProperty.Skill_Shields:
                    {
                        if (type == eObjectType.Cloth && realm == eRealm.Albion)
                            return true;
                        else if (type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Reinforced || type == eObjectType.Scale || type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_ShortBow:
                    {
                        return false;
                    }
                case eProperty.Skill_Smiting:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Studded && level < 20)
                            return true;
                        else if (type == eObjectType.Chain && level >= 20)
                            return true;
                        break;
                    }
                case eProperty.Skill_SoulRending:
                    {
                        if (type == eObjectType.Studded && level < 10)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;
                        break;
                    }
                case eProperty.Skill_Spear:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Studded)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;
                        break;
                    }
                case eProperty.Skill_Staff:
                    {
                        if (type == eObjectType.Leather && realm == eRealm.Albion)
                            return true;
                        break;
                    }
                case eProperty.Skill_Stealth:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Reinforced)
                            return true;
                        else if (realm == eRealm.Albion && level >= 20 && type == eObjectType.Chain)
                            return true;
                        break;
                    }
                case eProperty.Skill_Stormcalling:
                    {
                        if (type == eObjectType.Studded && level < 10)
                            return true;
                        else if (type == eObjectType.Chain && level >= 10)
                            return true;
                        break;
                    }
                case eProperty.Skill_Subterranean:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (type == eObjectType.Studded && level < 20)
                            return true;
                        else if (type == eObjectType.Chain && level >= 20)
                            return true;
                        break;
                    }
                case eProperty.Skill_Sword:
                    {
                        if (type == eObjectType.Studded || type == eObjectType.Chain)
                            return true;
                        break;
                    }
                case eProperty.Skill_Slashing:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_Thrusting:
                    {
                        if (type == eObjectType.Leather || type == eObjectType.Studded || type == eObjectType.Chain || type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_Two_Handed:
                    {
                        if (type == eObjectType.Studded && level < 10)
                            return true;
                        else if (type == eObjectType.Chain && level < 20)
                            return true;
                        else if (type == eObjectType.Plate)
                            return true;
                        break;
                    }
                case eProperty.Skill_Valor:
                    {
                        if (type == eObjectType.Reinforced && level < 20)
                            return true;
                        else if (type == eObjectType.Scale)
                            return true;
                        break;
                    }
                case eProperty.AllArcherySkills:
                    {
                        if (type == eObjectType.Leather && level < 10)
                            return true;
                        else if (level >= 10 && (type == eObjectType.Reinforced || type == eObjectType.Studded))
                            return true;

                        break;
                    }
                case eProperty.AllDualWieldingSkills:
                    {
                        //Dualwielders are always above level 4 and can wear better than cloth from the start.
                        if (type == eObjectType.Cloth)
                            return false;
                        //mercs are the only dualwielder who can wear chain
                        else if (realm == eRealm.Albion && type == eObjectType.Studded && level < 10)
                            return true;
                        else if (realm == eRealm.Albion && type == eObjectType.Chain)
                            return true;
                        //all assassins wear leather, blademasters and zerks wear studded.
                        else if (type == eObjectType.Leather || type == eObjectType.Reinforced || (type == eObjectType.Studded && realm == eRealm.Midgard))
                            return true;
                        break;
                    }
                case eProperty.AllMagicSkills:
                    {
                        // not for scouts
                        if (realm == eRealm.Albion && type == eObjectType.Studded && level >= 20)
                            return false;
                        // Paladins can't use + magic skills
                        if (realm == eRealm.Albion && type == eObjectType.Plate)
                            return false;

                        return true;
                    }
                case eProperty.AllMeleeWeaponSkills:
                    {
                        if (realm == eRealm.Midgard && type == eObjectType.Cloth)
                            return false;
                        else if (level >= 5)
                            return true;

                        break;
                    }
                case eProperty.AllSkills:
                    {
                        return true;
                    }
                case eProperty.Skill_Power_Strikes:
                case eProperty.Skill_Magnetism:
                case eProperty.Skill_MaulerStaff:
                case eProperty.Skill_Aura_Manipulation:
                case eProperty.Skill_FistWraps:
                    {
                        //Maulers
                        if (type == eObjectType.Leather) //Maulers can only wear leather.
                            return true;

                        break;
                    }

            }

            return false;
        }

        private static bool SkillIsValidForWeapon(ItemUnique item, eProperty property)
        {
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (property)
            {
                case eProperty.Skill_Arboreal:
                case eProperty.Skill_Body:
                case eProperty.Skill_BoneArmy:
                case eProperty.Skill_Cold:
                case eProperty.Skill_Creeping:
                case eProperty.Skill_Cursing:
                case eProperty.Skill_Darkness:
                case eProperty.Skill_Death_Servant:
                case eProperty.Skill_DeathSight:
                case eProperty.Skill_Earth:
                case eProperty.Skill_Enchantments:
                case eProperty.Skill_EtherealShriek:
                case eProperty.Skill_Fire:
                case eProperty.Skill_Hexing:
                case eProperty.Skill_Light:
                case eProperty.Skill_Mana:
                case eProperty.Skill_Matter:
                case eProperty.Skill_Mentalism:
                case eProperty.Skill_Mind:
                case eProperty.Skill_Pain_working:
                case eProperty.Skill_PhantasmalWail:
                case eProperty.Skill_Runecarving:
                case eProperty.Skill_SpectralForce:
                case eProperty.Skill_Spirit:
                case eProperty.Skill_Summoning:
                case eProperty.Skill_Suppression:
                case eProperty.Skill_Verdant:
                case eProperty.Skill_Void:
                case eProperty.Skill_Wind:
                case eProperty.Skill_Witchcraft:
                    {
                        if (type == eObjectType.Staff && item.Description != "friar")
                            return true;
                        break;
                    }
                //healer things
                case eProperty.Skill_Smiting:
                    {
                        if ((type == eObjectType.Shield && item.Type_Damage < 3) || type == eObjectType.CrushingWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Enhancement:
                case eProperty.Skill_Rejuvenation:
                    {
                        if ((type == eObjectType.Staff && item.Description == "friar") || (type == eObjectType.Shield && item.Type_Damage < 3) || type == eObjectType.CrushingWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Augmentation:
                case eProperty.Skill_Mending:
                case eProperty.Skill_Subterranean:
                case eProperty.Skill_Nurture:
                case eProperty.Skill_Nature:
                case eProperty.Skill_Regrowth:
                    {
                        if (type == eObjectType.Hammer || (type == eObjectType.Shield && item.Type_Damage < 2) || type == eObjectType.Blunt || type == eObjectType.Blades)
                            return true;
                        break;
                    }
                //archery things
                case eProperty.Skill_Archery:
                    if (type == eObjectType.CompositeBow || type == eObjectType.RecurvedBow || type == eObjectType.Longbow)
                        return true;
                    break;
                case eProperty.Skill_Composite:
                    {
                        if (type == eObjectType.CompositeBow)
                            return true;
                        break;
                    }
                case eProperty.Skill_RecurvedBow:
                    {
                        if (type == eObjectType.RecurvedBow)
                            return true;
                        break;
                    }
                case eProperty.Skill_Long_bows:
                    {
                        if (type == eObjectType.Longbow)
                            return true;
                        break;
                    }
                //other specifics
                case eProperty.Skill_Staff:
                    {
                        if (type == eObjectType.Staff && item.Description == "friar")
                            return true;
                        break;
                    }
                case eProperty.Skill_Axe:
                    {
                        if (type == eObjectType.Axe || type == eObjectType.LeftAxe || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Battlesongs:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || (type == eObjectType.Shield && item.Type_Damage < 3))
                            return true;
                        break;
                    }
                case eProperty.Skill_BeastCraft:
                    {
                        if (type == eObjectType.Spear)
                            return true;
                        break;
                    }
                case eProperty.Skill_Blades:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Blunt:
                    {
                        if (type == eObjectType.Blunt || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Celtic_Dual:
                    {
                        if (type == eObjectType.Piercing || type == eObjectType.Blades || type == eObjectType.Blunt)
                            return true;
                        break;
                    }
                case eProperty.Skill_Celtic_Spear:
                    {
                        if (type == eObjectType.CelticSpear)
                            return true;
                        break;
                    }
                case eProperty.Skill_Chants:
                    {
                        return false;
                    }
                case eProperty.Skill_Critical_Strike:
                    {
                        if (type == eObjectType.Piercing || type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Blades || type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.LeftAxe)
                            return true;
                        break;
                    }
                case eProperty.Skill_Cross_Bows:
                    {
                        if (type == eObjectType.Crossbow)
                            return true;
                        break;
                    }
                case eProperty.Skill_Crushing:
                    {
                        if (type == eObjectType.CrushingWeapon ||
                            ((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && item.Type_Damage == (int)eDamageType.Crush) ||
                            type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Dual_Wield:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.CrushingWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Envenom:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Flexible_Weapon:
                    {
                        if (type == eObjectType.Flexible || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Hammer:
                    {
                        if (type == eObjectType.Hammer || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_HandToHand:
                    {
                        if (type == eObjectType.HandToHand)
                            return true;
                        break;
                    }
                case eProperty.Skill_Instruments:
                    {
                        if (type == eObjectType.Instrument || (type == eObjectType.Shield && item.Type_Damage == 1))
                            return true;
                        break;
                    }
                case eProperty.Skill_Large_Weapon:
                    {
                        if (type == eObjectType.LargeWeapons)
                            return true;
                        break;
                    }
                case eProperty.Skill_Left_Axe:
                    {
                        if (type == eObjectType.Axe || type == eObjectType.LeftAxe)
                            return true;
                        break;
                    }
                case eProperty.Skill_Music:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Blunt || (type == eObjectType.Shield && item.Type_Damage == 1) || type == eObjectType.Instrument)
                            return true;
                        break;
                    }
                case eProperty.Skill_Nightshade:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_OdinsWill:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Spear || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Pathfinding:
                    {
                        if (type == eObjectType.RecurvedBow || type == eObjectType.Piercing || type == eObjectType.Blades)
                            return true;
                        break;
                    }
                case eProperty.Skill_Piercing:
                    {
                        if (type == eObjectType.Piercing || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Polearms:
                    {
                        if (type == eObjectType.PolearmWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Savagery:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || type == eObjectType.HandToHand)
                            return true;
                        break;
                    }
                case eProperty.Skill_Scythe:
                    {
                        if (type == eObjectType.Scythe)
                            return true;
                        break;
                    }

                case eProperty.Skill_VampiiricEmbrace:
                case eProperty.Skill_ShadowMastery:
                    {
                        if (type == eObjectType.Piercing)
                            return true;
                        break;
                    }
                case eProperty.Skill_Shields:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Blunt || type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Shield || type == eObjectType.Axe || type == eObjectType.Sword || type == eObjectType.Hammer)
                            return true;
                        break;
                    }
                case eProperty.Skill_ShortBow:
                    {
                        return false;
                    }
                case eProperty.Skill_Slashing:
                    {
                        if (type == eObjectType.SlashingWeapon ||
                            ((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && item.Type_Damage == (int)eDamageType.Slash) ||
                            type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_SoulRending:
                    {
                        if (type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Flexible || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Spear:
                    {
                        if (type == eObjectType.Spear)
                            return true;
                        break;
                    }
                case eProperty.Skill_Stealth:
                    {
                        if (type == eObjectType.Longbow || type == eObjectType.RecurvedBow || type == eObjectType.CompositeBow || (realm == eRealm.Albion && type == eObjectType.Shield && item.Type_Damage == 1) || type == eObjectType.Spear || type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.LeftAxe || type == eObjectType.SlashingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Piercing || type == eObjectType.Blades || (realm == eRealm.Albion && type == eObjectType.Instrument))
                            return true;
                        break;
                    }
                case eProperty.Skill_Stormcalling:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Axe || type == eObjectType.Hammer || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Sword:
                    {
                        if (type == eObjectType.Sword || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Thrusting:
                    {
                        if (type == eObjectType.ThrustWeapon ||
                            ((type == eObjectType.TwoHandedWeapon || type == eObjectType.PolearmWeapon) && item.Type_Damage == (int)eDamageType.Thrust) ||
                            type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Two_Handed:
                    {
                        if (type == eObjectType.TwoHandedWeapon)
                            return true;
                        break;
                    }
                case eProperty.Skill_Valor:
                    {
                        if (type == eObjectType.Blades || type == eObjectType.Piercing || type == eObjectType.Blunt || type == eObjectType.LargeWeapons || type == eObjectType.Shield)
                            return true;
                        break;
                    }
                case eProperty.Skill_Thrown_Weapons:
                    {
                        return false;
                    }
                case eProperty.Skill_Pacification:
                    {
                        if (type == eObjectType.Hammer)
                            return true;
                        break;
                    }
                case eProperty.Skill_Dementia:
                    {
                        if (type == eObjectType.Piercing)
                            return true;
                        break;
                    }
                case eProperty.AllArcherySkills:
                    {
                        if (type == eObjectType.CompositeBow || type == eObjectType.Longbow || type == eObjectType.RecurvedBow)
                            return true;
                        break;
                    }
                case eProperty.AllDualWieldingSkills:
                    {
                        if (type == eObjectType.Axe || type == eObjectType.Sword || type == eObjectType.Hammer || type == eObjectType.LeftAxe || type == eObjectType.SlashingWeapon || type == eObjectType.CrushingWeapon || type == eObjectType.ThrustWeapon || type == eObjectType.Piercing || type == eObjectType.Blades || type == eObjectType.Blunt)
                            return true;
                        break;
                    }
                case eProperty.AllMagicSkills:
                    {
                        //scouts, armsmen, paladins, mercs, blademasters, heroes, zerks, warriors do not need this.
                        if (type == eObjectType.Longbow || type == eObjectType.CelticSpear || type == eObjectType.PolearmWeapon || type == eObjectType.TwoHandedWeapon || type == eObjectType.Crossbow || (type == eObjectType.Shield && item.Type_Damage > 2))
                            return false;
                        else
                            return true;
                    }
                case eProperty.AllMeleeWeaponSkills:
                    {

                        if (type == eObjectType.Staff && realm != eRealm.Albion)
                            return false;
                        else if (type == eObjectType.Staff && item.Description != "friar") // do not add if caster staff
                            return false;
                        else if (type == eObjectType.Longbow || type == eObjectType.CompositeBow || type == eObjectType.RecurvedBow || type == eObjectType.Crossbow || type == eObjectType.Fired || type == eObjectType.Instrument)
                            return false;
                        else
                            return true;
                    }
                case eProperty.Skill_Aura_Manipulation: //Maulers
                    {
                        if (type == eObjectType.MaulerStaff || type == eObjectType.FistWraps)
                            return true;
                        break;
                    }
                case eProperty.Skill_Magnetism: //Maulers
                    {
                        if (type == eObjectType.FistWraps || type == eObjectType.MaulerStaff)
                            return true;
                        break;
                    }
                case eProperty.Skill_MaulerStaff: //Maulers
                    {
                        if (type == eObjectType.MaulerStaff)
                            return true;
                        break;
                    }
                case eProperty.Skill_Power_Strikes: //Maulers
                    {
                        if (type == eObjectType.MaulerStaff || type == eObjectType.FistWraps)
                            return true;
                        break;
                    }
                case eProperty.Skill_FistWraps: //Maulers
                    {
                        if (type == eObjectType.FistWraps)
                            return true;
                        break;
                    }
            }
            return false;
        }

        private static bool StatIsValidForRealm(eRealm realm, eProperty property)
        {
            switch (property)
            {
                case eProperty.Piety:
                case eProperty.PieCapBonus:
                    {
                        if (realm == eRealm.Hibernia)
                            return false;
                        break;
                    }
                case eProperty.Empathy:
                case eProperty.EmpCapBonus:
                    {
                        if (realm == eRealm.Midgard || realm == eRealm.Albion)
                            return false;
                        break;
                    }
                case eProperty.Intelligence:
                case eProperty.IntCapBonus:
                    {
                        if (realm == eRealm.Midgard)
                            return false;
                        break;
                    }
            }
            return true;
        }

        private static bool StatIsValidForArmor(ItemUnique item, eProperty property)
        {
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (property)
            {
                case eProperty.Intelligence:
                case eProperty.IntCapBonus:
                    {
                        if (realm == eRealm.Midgard)
                            return false;

                        if (realm == eRealm.Hibernia && item.Level < 20 && type != eObjectType.Reinforced && type != eObjectType.Cloth)
                            return false;

                        if (realm == eRealm.Hibernia && item.Level >= 20 && type != eObjectType.Scale && type != eObjectType.Cloth)
                            return false;

                        if (type != eObjectType.Cloth)
                            return false;

                        break;
                    }
                case eProperty.Acuity:
                case eProperty.AcuCapBonus:
                case eProperty.PowerPool:
                case eProperty.PowerPoolCapBonus:
                    {
                        if (realm == eRealm.Albion && item.Level >= 20 && type == eObjectType.Studded)
                            return false;

                        if (realm == eRealm.Midgard && item.Level >= 10 && type == eObjectType.Leather)
                            return false;

                        if (realm == eRealm.Midgard && item.Level >= 20 && type == eObjectType.Studded)
                            return false;

                        break;
                    }
                case eProperty.Piety:
                case eProperty.PieCapBonus:
                    {
                        if (realm == eRealm.Albion)
                        {
                            if (type == eObjectType.Leather && item.Level >= 10)
                                return false;

                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 10)
                                return false;
                        }
                        else if (realm == eRealm.Midgard)
                        {
                            if (type == eObjectType.Leather && item.Level >= 10)
                                return false;

                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 10)
                                return false;
                        }
                        else if (realm == eRealm.Hibernia)
                        {
                            return false;
                        }
                        break;
                    }
                case eProperty.Charisma:
                case eProperty.ChaCapBonus:
                    {
                        if (realm == eRealm.Albion)
                        {
                            if (type == eObjectType.Leather && item.Level >= 10)
                                return false;

                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 20)
                                return false;
                        }
                        if (realm == eRealm.Midgard)
                        {
                            if (type == eObjectType.Studded && item.Level >= 20)
                                return false;

                            if (type == eObjectType.Chain && item.Level < 20)
                                return false;
                        }
                        else if (realm == eRealm.Hibernia)
                        {
                            if (type == eObjectType.Leather && item.Level >= 15)
                                return false;

                            if (type == eObjectType.Reinforced && item.Level < 15)
                                return false;
                        }
                        break;
                    }
                case eProperty.Empathy:
                case eProperty.EmpCapBonus:
                    {
                        if (realm != eRealm.Hibernia)
                            return false;

                        if (type == eObjectType.Leather && item.Level >= 10)
                            return false;

                        if (type == eObjectType.Reinforced && item.Level >= 20)
                            return false;

                        if (type == eObjectType.Scale && item.Level < 20)
                            return false;

                        break;
                    }
            }
            return true;
        }

        private static bool StatIsValidForWeapon(ItemUnique item, eProperty property)
        {
            eRealm realm = (eRealm)item.Realm;
            eObjectType type = (eObjectType)item.Object_Type;

            switch (type)
            {
                case eObjectType.Staff:
                    {
                        if ((property == eProperty.Piety || property == eProperty.PieCapBonus) && realm == eRealm.Hibernia)
                            return false;
                        else if ((property == eProperty.Piety || property == eProperty.PieCapBonus) && realm == eRealm.Albion && item.Description != "friar")
                            return false; // caster staff
                        else if (property == eProperty.Charisma || property == eProperty.Empathy || property == eProperty.ChaCapBonus || property == eProperty.EmpCapBonus)
                            return false;
                        else if ((property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.AcuCapBonus) && item.Description == "friar")
                            return false;
                        break;
                    }

                case eObjectType.Shield:
                    {
                        if ((realm == eRealm.Albion || realm == eRealm.Midgard) && (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus))
                            return false;
                        else if (realm == eRealm.Hibernia && (property == eProperty.Piety || property == eProperty.PieCapBonus))
                            return false;
                        else if ((realm == eRealm.Albion || realm == eRealm.Hibernia) && item.Type_Damage > 1 && (property == eProperty.Charisma || property == eProperty.ChaCapBonus))
                            return false;
                        else if (realm == eRealm.Midgard && item.Type_Damage > 2 && (property == eProperty.Charisma || property == eProperty.ChaCapBonus))
                            return false;
                        else if (item.Type_Damage > 2 && property == eProperty.MaxMana)
                            return false;

                        break;
                    }
                case eObjectType.Blades:
                case eObjectType.Blunt:
                    {
                        if (property == eProperty.Piety || property == eProperty.PieCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.LargeWeapons:
                case eObjectType.Piercing:
                case eObjectType.Scythe:
                    {
                        if (property == eProperty.Piety || property == eProperty.Empathy || property == eProperty.Charisma)
                            return false;
                        break;
                    }
                case eObjectType.CrushingWeapon:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.SlashingWeapon:
                case eObjectType.ThrustWeapon:
                case eObjectType.Hammer:
                case eObjectType.Sword:
                case eObjectType.Axe:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity)
                            return false;
                        break;
                    }
                case eObjectType.TwoHandedWeapon:
                case eObjectType.Flexible:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus)
                            return false;
                        break;
                    }
                case eObjectType.RecurvedBow:
                case eObjectType.CompositeBow:
                case eObjectType.Longbow:
                case eObjectType.Crossbow:
                case eObjectType.Fired:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus ||
                            property == eProperty.MaxMana || property == eProperty.PowerPool || property == eProperty.SpellDuration || property == eProperty.PowerPoolCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity || property == eProperty.Piety || property == eProperty.PieCapBonus
                            || property == eProperty.SpellDuration || property == eProperty.HealingEffectiveness || property == eProperty.BuffEffectiveness)
                            return false;
                        break;
                    }
                case eObjectType.Spear:
                case eObjectType.CelticSpear:
                case eObjectType.LeftAxe:
                case eObjectType.PolearmWeapon:
                case eObjectType.HandToHand:
                case eObjectType.FistWraps: //Maulers
                case eObjectType.MaulerStaff: //Maulers
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Charisma || property == eProperty.ChaCapBonus ||
                            property == eProperty.MaxMana || property == eProperty.PowerPool || property == eProperty.PowerPoolCapBonus || property == eProperty.AcuCapBonus || property == eProperty.Acuity || property == eProperty.Piety || property == eProperty.PieCapBonus
                             || property == eProperty.SpellDuration || property == eProperty.HealingEffectiveness || property == eProperty.BuffEffectiveness)
                            return false;
                        break;
                    }
                case eObjectType.Instrument:
                    {
                        if (property == eProperty.Intelligence || property == eProperty.IntCapBonus || property == eProperty.Empathy || property == eProperty.EmpCapBonus || property == eProperty.Piety || property == eProperty.PieCapBonus)
                            return false;
                        break;
                    }
            }
            return true;
        }

        private static void WriteBonus(ItemUnique item, eProperty property, int amount)
        {
            if (property == eProperty.AllFocusLevels)
            {
                amount = Math.Min(50, amount);
            }

            if (item.Bonus1 == 0)
            {
                item.Bonus1 = amount;
                item.Bonus1Type = (int)property;

                if (property == eProperty.AllFocusLevels)
                    item.Name = "Focus " + item.Name;
            }
            else if (item.Bonus2 == 0)
            {
                item.Bonus2 = amount;
                item.Bonus2Type = (int)property;
            }
            else if (item.Bonus3 == 0)
            {
                item.Bonus3 = amount;
                item.Bonus3Type = (int)property;
            }
            else if (item.Bonus4 == 0)
            {
                item.Bonus4 = amount;
                item.Bonus4Type = (int)property;
            }
            else if (item.Bonus5 == 0)
            {
                item.Bonus5 = amount;
                item.Bonus5Type = (int)property;
            }
        }

        private static bool BonusExists(ItemUnique item, eProperty property)
        {
            if (item.Bonus1Type == (int)property ||
                item.Bonus2Type == (int)property ||
                item.Bonus3Type == (int)property ||
                item.Bonus4Type == (int)property ||
                item.Bonus5Type == (int)property)
                return true;

            return false;
        }

        private static int GetBonusAmount(eBonusType type, eProperty property, int level)
        {
            switch (type)
            {
                case eBonusType.Focus:
                    {
                        return level;
                    }
                case eBonusType.Resist:
                    {
                        int max = (int)Math.Ceiling((((level / 2.0) + 1) / 4));
                        return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                    }
                case eBonusType.Skill:
                    {
                        int max = (int)Math.Ceiling(((level / 5.0) + 1) / 3);
                        if (property == eProperty.AllSkills)
                            max = (int)Math.Ceiling((double)max / 2.0);
                        return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                    }
                case eBonusType.Stat:
                    {
                        if (property == eProperty.MaxHealth)
                        {
                            int max = (int)Math.Ceiling(((double)level * 4.0) / 4);
                            return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                        }
                        else if (property == eProperty.MaxMana)
                        {
                            int max = (int)Math.Ceiling(((double)level / 2.0 + 1) / 4);
                            return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                        }
                        else
                        {
                            int max = (int)Math.Ceiling(((double)level * 1.5) / 3);
                            return Util.Random((int)Math.Ceiling((double)max / 2.0), max);
                        }
                    }
                case eBonusType.AdvancedStat:
                    {
                        if (property == eProperty.MaxHealthCapBonus)
                            return Util.Random(5, 25); // cap is 400
                        else if (property == eProperty.PowerPoolCapBonus)
                            return Util.Random(1, 10); // cap is 50
                        else
                            return Util.Random(1, 6); // cap is 26
                    }
            }
            return 1;
        }

        private static string ArmorSlotToName(eInventorySlot slot, eObjectType type)
        {
            switch (slot)
            {
                case eInventorySlot.ArmsArmor:
                    if (type == eObjectType.Plate)
                        return "Arms";
                    else
                        return "Sleeves";

                case eInventorySlot.FeetArmor:
                    return "Boots";

                case eInventorySlot.HandsArmor:
                    if (type == eObjectType.Plate)
                        return "Gauntlets";
                    else
                        return "Gloves";

                case eInventorySlot.HeadArmor:
                    if (type == eObjectType.Cloth)
                        return "Cap";
                    else if (type == eObjectType.Scale)
                        return "Coif";
                    else
                        return "Helm";

                case eInventorySlot.LegsArmor:
                    if (type == eObjectType.Cloth)
                        return "Pants";
                    else if (type == eObjectType.Plate)
                        return "Legs";
                    else
                        return "Leggings";

                case eInventorySlot.TorsoArmor:
                    if (type == eObjectType.Chain || type == eObjectType.Scale)
                        return "Hauberk";
                    else if (type == eObjectType.Plate)
                        return "Breastplate";
                    else if ((type == eObjectType.Leather || type == eObjectType.Studded) && Util.Chance(50))
                        return "Jerkin";
                    else
                        return "Vest";

                default: return "No armor slot to name found for " + GlobalConstants.SlotToName((int)slot);
            }
        }

        private static int GetAbsorb(eObjectType type)
        {
            switch (type)
            {
                case eObjectType.Cloth: return 0;
                case eObjectType.Leather: return 10;
                case eObjectType.Studded: return 19;
                case eObjectType.Reinforced: return 19;
                case eObjectType.Chain: return 27;
                case eObjectType.Scale: return 27;
                case eObjectType.Plate: return 34;
                default: return 0;
            }
        }

        private static int GetWeaponSpeed(ItemUnique item)
        {
            // tolakram - reset speeds based on data from allakhazam 1-26-2008
            // removed specific left hand speed - left hand usable set based on speed in GenerateItemNameModel

            switch ((eObjectType)item.Object_Type)
            {
                case eObjectType.SlashingWeapon:
                case eObjectType.CrushingWeapon:
                    {
                        return Util.Random(24, 42);
                    }
                case eObjectType.ThrustWeapon:
                    {
                        return Util.Random(22, 38);
                    }
                case eObjectType.Fired:
                    {
                        return Util.Random(40, 46);
                    }
                case eObjectType.TwoHandedWeapon:
                    {
                        return Util.Random(38, 55);
                    }
                case eObjectType.PolearmWeapon:
                    {
                        return Util.Random(43, 59);
                    }
                case eObjectType.Staff:
                    {
                        return Util.Random(31, 54);
                    }
                case eObjectType.MaulerStaff: //Maulers
                    {
                        return Util.Random(34, 54);
                    }
                case eObjectType.Longbow:
                    {
                        return Util.Random(41, 56);
                    }
                case eObjectType.Crossbow:
                    {
                        return Util.Random(41, 56);
                    }
                case eObjectType.Flexible:
                    {
                        return Util.Random(33, 46);
                    }
                case eObjectType.Sword:
                case eObjectType.Hammer:
                case eObjectType.Axe:
                    {
                        //28 to 58
                        if (item.Hand == 1)
                            return Util.Random(46, 55);  // two handed
                        else return Util.Random(23, 45); // one handed
                    }
                case eObjectType.Spear:
                    {
                        return Util.Random(39, 55);
                    }
                case eObjectType.CompositeBow:
                    {
                        return Util.Random(37, 53);
                    }
                case eObjectType.LeftAxe:
                    {
                        return Util.Random(24, 36);
                    }
                case eObjectType.HandToHand:
                    {
                        return Util.Random(26, 37);
                    }
                case eObjectType.FistWraps:
                    {
                        return Util.Random(28, 41);
                    }
                case eObjectType.RecurvedBow:
                    {
                        return Util.Random(40, 55);
                    }
                case eObjectType.Blades:
                case eObjectType.Blunt:
                case eObjectType.Piercing:
                    {
                        return Util.Random(23, 45);
                    }
                case eObjectType.LargeWeapons:
                    {
                        return Util.Random(37, 57);
                    }
                case eObjectType.CelticSpear:
                    {
                        return Util.Random(33, 58);
                    }
                case eObjectType.Scythe:
                    {
                        return Util.Random(38, 55);
                    }
                case eObjectType.Shield:
                    {
                        switch (item.Type_Damage)
                        {
                            case 1:
                                return 30;
                            case 2:
                                return 40;
                            case 3:
                                return 45;
                        }
                        return 50;
                    }
            }
            // for unhandled types
            if (item.Hand == 1)
                return 50;  // two handed
            else if (item.Hand == 2)
                return 33;  // left hand
            else return 41; // right hand
        }

        public static int GenerateItemWeight(eObjectType type, eInventorySlot slot, ItemUnique item)
        {
            switch (type)
            {
                case eObjectType.LeftAxe:
                case eObjectType.Flexible:
                case eObjectType.Axe:
                case eObjectType.Blades:
                case eObjectType.HandToHand:
                case eObjectType.FistWraps: //Maulers
                    return 20;
                case eObjectType.CompositeBow:
                case eObjectType.RecurvedBow:
                case eObjectType.Longbow:
                case eObjectType.Blunt:
                case eObjectType.CrushingWeapon:
                case eObjectType.Fired:
                case eObjectType.Hammer:
                case eObjectType.Piercing:
                case eObjectType.SlashingWeapon:
                case eObjectType.Sword:
                case eObjectType.ThrustWeapon:
                    return 30;
                case eObjectType.Crossbow:
                case eObjectType.Spear:
                case eObjectType.CelticSpear:
                case eObjectType.Staff:
                case eObjectType.TwoHandedWeapon:
                case eObjectType.MaulerStaff: //Maulers
                    return 40;
                case eObjectType.Scale:
                case eObjectType.Chain:
                    {
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: return 38;
                            case eInventorySlot.FeetArmor: return 24;
                            case eInventorySlot.HandsArmor: return 22;
                            case eInventorySlot.HeadArmor: return 32;
                            case eInventorySlot.LegsArmor: return 40;
                            case eInventorySlot.TorsoArmor: return 50;
                        }
                        return 0;
                    }
                case eObjectType.Cloth:
                    {
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: return 8;
                            case eInventorySlot.FeetArmor: return 8;
                            case eInventorySlot.HandsArmor: return 8;
                            case eInventorySlot.HeadArmor: return 32;
                            case eInventorySlot.LegsArmor: return 14;
                            case eInventorySlot.TorsoArmor: return 20;
                        }
                        return 0;
                    }
                case eObjectType.Instrument:
                    return 15;
                case eObjectType.LargeWeapons:
                    return 50;
                case eObjectType.Leather:
                    {
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: return 24;
                            case eInventorySlot.FeetArmor: return 16;
                            case eInventorySlot.HandsArmor: return 16;
                            case eInventorySlot.HeadArmor: return 16;
                            case eInventorySlot.LegsArmor: return 28;
                            case eInventorySlot.TorsoArmor: return 40;
                        }
                        return 0;
                    }
                case eObjectType.Magical:
                    return 5;
                case eObjectType.Plate:
                    {
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: return 44;
                            case eInventorySlot.FeetArmor: return 37;
                            case eInventorySlot.HandsArmor: return 36;
                            case eInventorySlot.HeadArmor: return 41;
                            case eInventorySlot.LegsArmor: return 53;
                            case eInventorySlot.TorsoArmor: return 65;
                        }
                        return 0;
                    }
                case eObjectType.PolearmWeapon:
                    return 60;
                case eObjectType.Reinforced:
                case eObjectType.Studded:
                    {
                        switch (slot)
                        {
                            case eInventorySlot.ArmsArmor: return 36;
                            case eInventorySlot.FeetArmor: return 24;
                            case eInventorySlot.HandsArmor: return 24;
                            case eInventorySlot.HeadArmor: return 24;
                            case eInventorySlot.LegsArmor: return 38;
                            case eInventorySlot.TorsoArmor: return 55;
                        }
                        return 0;
                    }
                case eObjectType.Scythe:
                    return 40;
                case eObjectType.Shield:
                    switch (item.Type_Damage)
                    {
                        case 1:
                            return 31;
                        case 2:
                            return 35;
                        case 3:
                            return 38;
                    }
                    return 31;
            }
            return 10;
        }


        #region definitions

        public enum eGenerateType
        {
            Weapon,
            Armor,
            Magical,
            None,
        }

        private static readonly eProperty[] StatBonus = new eProperty[]
        {
            eProperty.Strength,
            eProperty.Dexterity,
            eProperty.Constitution,
            eProperty.Quickness,
            eProperty.Intelligence,
            eProperty.Piety,
            eProperty.Empathy,
            eProperty.Charisma,
            eProperty.MaxMana,
            eProperty.MaxHealth,
        };

        
        private static readonly eProperty[] AdvancedStats = new eProperty[]
        {
            //neu
            eProperty.SpellDuration,
            eProperty.HealingEffectiveness,
            eProperty.BuffEffectiveness,

            eProperty.PowerPool,
            eProperty.PowerPoolCapBonus,
            eProperty.StrCapBonus,
            eProperty.DexCapBonus,
            eProperty.ConCapBonus,
            eProperty.QuiCapBonus,
            eProperty.IntCapBonus,
            eProperty.PieCapBonus,
            eProperty.EmpCapBonus,
            eProperty.ChaCapBonus,
            ///neu
           
              

			//eProperty.Acuity,  // need duplicate check between base stat and acuity before enabling
			//eProperty.AcuCapBonus,
		};
        

        private static readonly eProperty[] ResistBonus = new eProperty[]
        {
            eProperty.Resist_Body,
            eProperty.Resist_Cold,
            eProperty.Resist_Crush,
            eProperty.Resist_Energy,
            eProperty.Resist_Heat,
            eProperty.Resist_Matter,
            eProperty.Resist_Slash,
            eProperty.Resist_Spirit,
            eProperty.Resist_Thrust,
        };


        private static readonly eProperty[] AlbSkillBonus = new eProperty[]
        {
            eProperty.Skill_Two_Handed,
            eProperty.Skill_Body,
	        //eProperty.Skill_Chants, // bonus not used
	        eProperty.Skill_Critical_Strike,
            eProperty.Skill_Cross_Bows,
            eProperty.Skill_Crushing,
            eProperty.Skill_Death_Servant,
            eProperty.Skill_DeathSight,
            eProperty.Skill_Dual_Wield,
            eProperty.Skill_Earth,
            eProperty.Skill_Enhancement,
            eProperty.Skill_Envenom,
            eProperty.Skill_Fire,
            eProperty.Skill_Flexible_Weapon,
            eProperty.Skill_Cold,
            eProperty.Skill_Instruments,
            eProperty.Skill_Archery,
            eProperty.Skill_Matter,
            eProperty.Skill_Mind,
            eProperty.Skill_Pain_working,
            eProperty.Skill_Parry,
            eProperty.Skill_Polearms,
            eProperty.Skill_Rejuvenation,
            eProperty.Skill_Shields,
            eProperty.Skill_Slashing,
            eProperty.Skill_Smiting,
            eProperty.Skill_SoulRending,
            eProperty.Skill_Spirit,
            eProperty.Skill_Staff,
            eProperty.Skill_Stealth,
            eProperty.Skill_Thrusting,
            eProperty.Skill_Wind,
            eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
        };


        private static readonly eProperty[] HibSkillBonus = new eProperty[]
        {
            eProperty.Skill_Critical_Strike,
            eProperty.Skill_Envenom,
            eProperty.Skill_Parry,
            eProperty.Skill_Shields,
            eProperty.Skill_Stealth,
            eProperty.Skill_Light,
            eProperty.Skill_Void,
            eProperty.Skill_Mana,
            eProperty.Skill_Blades,
            eProperty.Skill_Blunt,
            eProperty.Skill_Piercing,
            eProperty.Skill_Large_Weapon,
            eProperty.Skill_Mentalism,
            eProperty.Skill_Regrowth,
            eProperty.Skill_Nurture,
            eProperty.Skill_Nature,
            eProperty.Skill_Music,
            eProperty.Skill_Celtic_Dual,
            eProperty.Skill_Celtic_Spear,
            eProperty.Skill_Archery,
            eProperty.Skill_Valor,
            eProperty.Skill_Verdant,
            eProperty.Skill_Creeping,
            eProperty.Skill_Arboreal,
            eProperty.Skill_Scythe,
	        //eProperty.Skill_Nightshade, // bonus not used
	        //eProperty.Skill_Pathfinding, // bonus not used
	        eProperty.Skill_Dementia,
            eProperty.Skill_ShadowMastery,
            eProperty.Skill_VampiiricEmbrace,
            eProperty.Skill_EtherealShriek,
            eProperty.Skill_PhantasmalWail,
            eProperty.Skill_SpectralForce,
            eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
        };

        private static readonly eProperty[] MidSkillBonus = new eProperty[]
        {
            eProperty.Skill_Critical_Strike,
            eProperty.Skill_Envenom,
            eProperty.Skill_Parry,
            eProperty.Skill_Shields,
            eProperty.Skill_Stealth,
            eProperty.Skill_Sword,
            eProperty.Skill_Hammer,
            eProperty.Skill_Axe,
            eProperty.Skill_Left_Axe,
            eProperty.Skill_Spear,
            eProperty.Skill_Mending,
            eProperty.Skill_Augmentation,
	        //Skill_Cave_Magic = 59,
	        eProperty.Skill_Darkness,
            eProperty.Skill_Suppression,
            eProperty.Skill_Runecarving,
            eProperty.Skill_Stormcalling,
	        //eProperty.Skill_BeastCraft, // bonus not used
			eProperty.Skill_Archery,
            eProperty.Skill_Battlesongs,
            eProperty.Skill_Subterranean,
            eProperty.Skill_BoneArmy,
            eProperty.Skill_Thrown_Weapons,
            eProperty.Skill_HandToHand,
    		//eProperty.Skill_Pacification,
	        //eProperty.Skill_Savagery,
	        eProperty.Skill_OdinsWill,
            eProperty.Skill_Cursing,
            eProperty.Skill_Hexing,
            eProperty.Skill_Witchcraft,
            eProperty.Skill_Summoning,
            eProperty.Skill_Aura_Manipulation, //Maulers
			eProperty.Skill_FistWraps, //Maulers
			eProperty.Skill_MaulerStaff, //Maulers
			eProperty.Skill_Magnetism, //Maulers
			eProperty.Skill_Power_Strikes, //Maulers
		};



        private static readonly int[] ArmorSlots = new int[] { 21, 22, 23, 25, 27, 28, };
        private static readonly int[] MagicalSlots = new int[] { 24, 26, 29, 32, 33, 34, 35, 36 };

        // the following are doubled up to work around an apparent mid-number bias to the random number generator

        // note that weapon array has been adjusted to add weight to more commonly used items
        private static readonly eObjectType[] AlbionWeapons = new eObjectType[]
        {
            eObjectType.ThrustWeapon,
            eObjectType.CrushingWeapon,
            eObjectType.SlashingWeapon,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.TwoHandedWeapon,
            eObjectType.Longbow,
            eObjectType.Flexible,
            eObjectType.PolearmWeapon,
            eObjectType.FistWraps, //Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Instrument,
            eObjectType.Crossbow,
            eObjectType.ThrustWeapon,
            eObjectType.CrushingWeapon,
            eObjectType.SlashingWeapon,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.TwoHandedWeapon,
            eObjectType.Longbow,
            eObjectType.Flexible,
            eObjectType.PolearmWeapon,
            eObjectType.FistWraps, //Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Instrument,
            eObjectType.Crossbow,
        };

        private static readonly eObjectType[] AlbionArmor = new eObjectType[]
        {
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
            eObjectType.Plate,
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
            eObjectType.Plate,
        };
        private static readonly eObjectType[] MidgardWeapons = new eObjectType[]
        {
            eObjectType.Sword,
            eObjectType.Hammer,
            eObjectType.Axe,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.Spear,
            eObjectType.CompositeBow ,
            eObjectType.LeftAxe,
            eObjectType.HandToHand,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Sword,
            eObjectType.Hammer,
            eObjectType.Axe,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.Spear,
            eObjectType.CompositeBow ,
            eObjectType.LeftAxe,
            eObjectType.HandToHand,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
		};

        private static readonly eObjectType[] MidgardArmor = new eObjectType[]
        {
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Studded,
            eObjectType.Chain,
        };

        private static readonly eObjectType[] HiberniaWeapons = new eObjectType[]
        {
            eObjectType.Blades,
            eObjectType.Blunt,
            eObjectType.Piercing,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.LargeWeapons,
            eObjectType.CelticSpear,
            eObjectType.Scythe,
            eObjectType.RecurvedBow,
            eObjectType.Instrument,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
			eObjectType.Blades,
            eObjectType.Blunt,
            eObjectType.Piercing,
            eObjectType.Shield,
            eObjectType.Staff,
            eObjectType.LargeWeapons,
            eObjectType.CelticSpear,
            eObjectType.Scythe,
            eObjectType.RecurvedBow,
            eObjectType.Instrument,
            eObjectType.FistWraps,//Maulers
			eObjectType.MaulerStaff,//Maulers
		};

        private static readonly eObjectType[] HiberniaArmor = new eObjectType[]
        {
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Reinforced,
            eObjectType.Scale,
            eObjectType.Cloth,
            eObjectType.Leather,
            eObjectType.Reinforced,
            eObjectType.Scale,
        };

        #endregion definitions


        #region Magic Naming

        protected static Dictionary<eProperty, string> hPropertyToMagicPrefix = new Dictionary<eProperty, string>();

        public static bool WriteMagicalName(eProperty property, ItemUnique item)
        {

            if (hPropertyToMagicPrefix.ContainsKey(property))
            {
                string str = hPropertyToMagicPrefix[property];
                if (str != string.Empty)
                    item.Name = str + " " + item.Name;
                return true;
            }

            return false;
        }

        public static void InitializeHashtables()
        {
            // Magic Prefix

            hPropertyToMagicPrefix.Add(eProperty.Strength, "Mighty");
            hPropertyToMagicPrefix.Add(eProperty.Dexterity, "Adroit");
            hPropertyToMagicPrefix.Add(eProperty.Constitution, "Fortifying");
            hPropertyToMagicPrefix.Add(eProperty.Quickness, "Speedy");
            hPropertyToMagicPrefix.Add(eProperty.Intelligence, "Insightful");
            hPropertyToMagicPrefix.Add(eProperty.Piety, "Willful");
            hPropertyToMagicPrefix.Add(eProperty.Empathy, "Attuned");
            hPropertyToMagicPrefix.Add(eProperty.Charisma, "Glib");
            hPropertyToMagicPrefix.Add(eProperty.MaxMana, "Arcane");
            hPropertyToMagicPrefix.Add(eProperty.MaxHealth, "Sturdy");
            hPropertyToMagicPrefix.Add(eProperty.PowerPool, "Arcane");
            //neu
            hPropertyToMagicPrefix.Add(eProperty.SpellDuration, "Arcane");
            hPropertyToMagicPrefix.Add(eProperty.HealingEffectiveness, "Sturdy");
            hPropertyToMagicPrefix.Add(eProperty.BuffEffectiveness, "Mighty");
            

            hPropertyToMagicPrefix.Add(eProperty.Resist_Body, "Bodybender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Cold, "Icebender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Crush, "Bluntbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Energy, "Energybender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Heat, "Heatbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Matter, "Matterbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Slash, "Edgebender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Spirit, "Spiritbender");
            hPropertyToMagicPrefix.Add(eProperty.Resist_Thrust, "Thrustbender");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Two_Handed, "Sundering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Body, "Soul Crusher");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Critical_Strike, "Lifetaker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Cross_Bows, "Truefire");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Crushing, "Battering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Death_Servant, "Death Binder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_DeathSight, "Minionbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Dual_Wield, "Whirling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Earth, "Earthborn");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Enhancement, "Fervent");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Envenom, "Venomous");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Fire, "Flameborn");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Flexible_Weapon, "Tensile");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Cold, "Iceborn");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Instruments, "Melodic");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Long_bows, "Winged");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Matter, "Earthsplitter");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mind, "Dominating");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Pain_working, "Painbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Parry, "Bladeblocker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Polearms, "Decimator");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Rejuvenation, "Rejuvenating");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Shields, "Protector's");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Slashing, "Honed");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Smiting, "Earthshaker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_SoulRending, "Soul Taker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Spirit, "Spiritbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Staff, "Thunderer");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Stealth, "Shadowwalker");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Thrusting, "Perforator");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Wind, "Airy");


            hPropertyToMagicPrefix.Add(eProperty.AllMagicSkills, "Mystical");
            hPropertyToMagicPrefix.Add(eProperty.AllMeleeWeaponSkills, "Gladiator");
            hPropertyToMagicPrefix.Add(eProperty.AllSkills, "Skillful");
            hPropertyToMagicPrefix.Add(eProperty.AllDualWieldingSkills, "Duelist");
            hPropertyToMagicPrefix.Add(eProperty.AllArcherySkills, "Bowmaster");


            hPropertyToMagicPrefix.Add(eProperty.Skill_Sword, "Serrated");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Hammer, "Demolishing");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Axe, "Swathe Cutter's");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Left_Axe, "Cleaving");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Spear, "Impaling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mending, "Bodymender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Augmentation, "Empowering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Darkness, "Shadowbender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Suppression, "Spiritbinder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Runecarving, "Runebender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Stormcalling, "Stormcaller");
            hPropertyToMagicPrefix.Add(eProperty.Skill_BeastCraft, "Lifebender");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Light, "Lightbender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Void, "Voidbender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mana, "Starbinder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Enchantments, "Chanter");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Blades, "Razored");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Blunt, "Crushing");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Piercing, "Lancenator");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Large_Weapon, "Sundering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Mentalism, "Mindbinder");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Regrowth, "Forestbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Nurture, "Plantbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Nature, "Animalbound");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Music, "Resonant");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Celtic_Dual, "Whirling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Celtic_Spear, "Impaling");
            hPropertyToMagicPrefix.Add(eProperty.Skill_RecurvedBow, "Hawk");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Valor, "Courageous");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Subterranean, "Ancestral");
            hPropertyToMagicPrefix.Add(eProperty.Skill_BoneArmy, "Blighted");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Verdant, "Vale Defender");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Battlesongs, "Motivating");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Composite, "Dragon");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Creeping, "Withering");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Arboreal, "Arbor Defender");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Scythe, "Reaper's");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Thrown_Weapons, "Catapult");
            hPropertyToMagicPrefix.Add(eProperty.Skill_HandToHand, "Martial");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Pacification, "Pacifying");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Savagery, "Savage");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Nightshade, "Nightshade");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Pathfinding, "Trail");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Summoning, "Soulbinder");

            hPropertyToMagicPrefix.Add(eProperty.Skill_Dementia, "Feverish");
            hPropertyToMagicPrefix.Add(eProperty.Skill_ShadowMastery, "Ominous");
            hPropertyToMagicPrefix.Add(eProperty.Skill_VampiiricEmbrace, "Deathly");
            hPropertyToMagicPrefix.Add(eProperty.Skill_EtherealShriek, "Shrill");
            hPropertyToMagicPrefix.Add(eProperty.Skill_PhantasmalWail, "Keening");
            hPropertyToMagicPrefix.Add(eProperty.Skill_SpectralForce, "Uncanny");
            hPropertyToMagicPrefix.Add(eProperty.Skill_OdinsWill, "Ardent");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Cursing, "Infernal");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Hexing, "Bedeviled");
            hPropertyToMagicPrefix.Add(eProperty.Skill_Witchcraft, "Diabolic");

            // Mauler - live mauler prefixes do not exist, as lame as that sounds.
            hPropertyToMagicPrefix.Add(eProperty.Skill_Aura_Manipulation, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_FistWraps, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_MaulerStaff, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_Magnetism, string.Empty);
            hPropertyToMagicPrefix.Add(eProperty.Skill_Power_Strikes, string.Empty);
        }

        #endregion
    }

}

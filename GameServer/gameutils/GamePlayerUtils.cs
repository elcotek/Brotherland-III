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

using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.Language;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// GamePlayer Utils Extension Class
    /// </summary>
    public static class GamePlayerUtils
    {

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Popup Window Clear
        /// </summary>
        /// <param name="player"></param>
		public static void SendClearPopupWindow(GamePlayer player)
        {
            GameObject obj = player.TargetObject;
            player.Out.SendChangeTarget(player);
            player.Out.SendMessage("", eChatType.CT_System, eChatLoc.CL_PopupWindow);
            player.Out.SendChangeTarget(obj);
        }

        #region Spot and Area Description / Translation
        /// <summary>
        /// Get Spot Description Checking Any Area with Description or Zone Description
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="spot"></param>
        /// <returns></returns>
        public static string GetSpotDescription(this Region reg, IPoint3D spot)
        {
            return reg.GetSpotDescription(spot.X, spot.Y, spot.Z);
        }

        /// <summary>
        /// Get Spot Description Checking Any Area with Description or Zone Description
        /// </summary>
        /// <param name="reg"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public static string GetSpotDescription(this Region reg, int x, int y, int z)
        {
            if (reg != null)
            {
                var area = reg.GetAreasOfSpot(x, y, z).OfType<AbstractArea>().FirstOrDefault(a => a.DisplayMessage && !string.IsNullOrEmpty(a.Description));

                if (area != null)
                    return area.Description;

                var zone = reg.GetZone(x, y);

                if (zone != null)
                    return zone.Description;

                return reg.Description;
            }

            return string.Empty;
        }

        #endregion

        #region player skills / bonuses
     

        /// <summary>
        /// Grey out some skills on client for specified duration
        /// </summary>
        /// <param name="skill">the skill to disable</param>
        /// <param name="duration">duration of disable in milliseconds</param>
        public static void DisableSkill(this GamePlayer player, Skill skill, int duration)
        {
            if (player.Client.Account.PrivLevel > 1)
                return;

            player.DisableSkill(skill, duration);

            player.Out.SendDisableSkill(skill, duration / 1000 + 1);
        }

        /// <summary>
        /// Updates all disabled skills to player
        /// </summary>
        public static void UpdateDisabledSkills(this GamePlayer player)
        {
            if (player != null)
            {
                foreach (Skill skl in player.GetAllDisabledSkills())
                {
                    if (skl != null)
                        player.Out.SendDisableSkill(skl, player.GetSkillDisabledDuration(skl) / 1000 + 1);
                }
            }
        }

        /// <summary>
        /// Reset all disabled skills to player
        /// </summary>
        public static void ResetDisabledSkills(this GamePlayer player)
        {
            foreach (Skill skl in player.GetAllDisabledSkills())
            {
                player.Out.SendDisableSkill(skl, 1);
                player.RemoveDisabledSkill(skl);
            }
            player.UpdateDisabledSkills();
        }

       
        public static int GetResistValueFromBuffs(GamePlayer player, eProperty resist)
        {
         
            if (player != null)
            {
                int buffValue = 0;
                for (int i = 0; i < (int)eProperty.ResCapBonus_Last; i++)
                {
                    if (i == (int)resist)
                    {

                        buffValue = player.GetModifiedFromBuffs(resist);

                       
                        return buffValue;
                    }

                }
            }
            return 0;
        }
      

        /// <summary>
        /// Delve Player Bonuses for Info Window
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static ICollection<string> GetBonusesInfo(this GamePlayer player)
        {
            var info = new List<string>
            {

                /*
                <Begin Info: Bonuses (snapshot)>
                Resistances
                 Crush: +25%/+0%
                 Slash: +28%/+0%
                 Thrust: +28%/+0%
                 Heat: +25%/+0%
                 Cold: +25%/+0%
                 Matter: +26%/+0%
                 Body: +31%/+0%
                 Spirit: +21%/+0%
                 Energy: +26%/+0%

                Special Item Bonuses
                 +20% to Power Pool
                 +3% to all Melee Damage
                 +6% to all Stat Buff Spells
                 +23% to all Heal Spells
                 +3% to Melee Combat Speed
                 +10% to Casting Speed

                Realm Rank Bonuses
                 +7 to ALL Specs

                Relic Bonuses
                 +20% to all Melee Damage

                Outpost Bonuses
                 none

                <End Info>
                 */

                //AbilityBonus[(int)((eProperty)updateResists[i])]
                LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Resist"),
                string.Format(" {2}:   {0:+0;-0}%/\t{1:+0;-0}%", player.GetModified(eProperty.Resist_Crush) -  GetResistValueFromBuffs(player, eProperty.Resist_Crush), GetResistValueFromBuffs(player, eProperty.Resist_Crush), SkillBase.GetPropertyName(eProperty.Resist_Crush, player.Client.Account.Language)),
                string.Format(" {2}:    {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Slash) - GetResistValueFromBuffs(player, eProperty.Resist_Slash),GetResistValueFromBuffs(player, eProperty.Resist_Slash), SkillBase.GetPropertyName(eProperty.Resist_Slash, player.Client.Account.Language)),
                string.Format(" {2}:  {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Thrust) - GetResistValueFromBuffs(player, eProperty.Resist_Slash), GetResistValueFromBuffs(player, eProperty.Resist_Thrust),SkillBase.GetPropertyName(eProperty.Resist_Thrust, player.Client.Account.Language)),
                string.Format(" {2}:     {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Heat) - GetResistValueFromBuffs(player, eProperty.Resist_Heat), GetResistValueFromBuffs(player, eProperty.Resist_Heat), SkillBase.GetPropertyName(eProperty.Resist_Heat, player.Client.Account.Language)),
                string.Format(" {2}:      {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Cold) - GetResistValueFromBuffs(player, eProperty.Resist_Cold),  GetResistValueFromBuffs(player, eProperty.Resist_Cold), SkillBase.GetPropertyName(eProperty.Resist_Cold, player.Client.Account.Language)),
                string.Format(" {2}:  {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Matter) - GetResistValueFromBuffs(player, eProperty.Resist_Matter),  GetResistValueFromBuffs(player, eProperty.Resist_Matter), SkillBase.GetPropertyName(eProperty.Resist_Matter, player.Client.Account.Language)),
                string.Format(" {2}:     {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Body) -  GetResistValueFromBuffs(player, eProperty.Resist_Body),  GetResistValueFromBuffs(player, eProperty.Resist_Body), SkillBase.GetPropertyName(eProperty.Resist_Body, player.Client.Account.Language)),
                string.Format(" {2}:     {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Spirit) -  GetResistValueFromBuffs(player, eProperty.Resist_Spirit),  GetResistValueFromBuffs(player, eProperty.Resist_Spirit), SkillBase.GetPropertyName(eProperty.Resist_Spirit, player.Client.Account.Language)),
                string.Format(" {2}:  {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Energy) -  GetResistValueFromBuffs(player, eProperty.Resist_Energy),  GetResistValueFromBuffs(player, eProperty.Resist_Energy), SkillBase.GetPropertyName(eProperty.Resist_Energy, player.Client.Account.Language)),
                string.Format(" {2}: {0:+0;-0}%/{1:+0;-0}%", player.GetModified(eProperty.Resist_Natural) -  GetResistValueFromBuffs(player, eProperty.Resist_Natural),  GetResistValueFromBuffs(player, eProperty.Resist_Natural), SkillBase.GetPropertyName(eProperty.Resist_Natural, player.Client.Account.Language)),

                " ",
                LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Special")
            };

            //This is an Array of the bonuses that show up in the Bonus Snapshot on Live, the only ones that really need to be there.
            int[] bonusToBeDisplayed = new int[36] { 10, 150, 151, 153, 154, 155, 173, 174, 179, 180, 182, 183, 184, 185, 186, 187, 188, 189, 190, 191, 192, 193, 194, 195, 196, 197, 198, 199, 200, 247, 248, 251, 252, 253, 254, 210 };
            for (int i = 0; i < (int)eProperty.MaxProperty; i++)
            {
                if ((player.ItemBonus[i] > 0) && ((Array.BinarySearch(bonusToBeDisplayed, i)) >= 0)) //Tiny edit here to add the binary serach to weed out the non essential bonuses
                {
                    if (player.ItemBonus[i] != 0)
                    {
                        //LIFEFLIGHT Add, to correct power pool from showing too much
                        //This is where we need to correct the display, make it cut off at the cap if
                        //Same with hits and hits cap
                        if (i == (int)eProperty.PowerPool)
                        {
                            int powercap = player.ItemBonus[(int)eProperty.PowerPoolCapBonus];
                            if (powercap > 50)
                            {
                                powercap = 50;
                            }
                            int powerpool = player.ItemBonus[(int)eProperty.PowerPool];
                            if (powerpool > powercap + 25)
                            {
                                int tempbonus = powercap + 25;
                                info.Add(ItemBonusDescription(tempbonus, i, player));
                            }
                            else
                            {
                                int tempbonus = powerpool;
                                info.Add(ItemBonusDescription(tempbonus, i, player));
                            }


                        }
                        else if (i == (int)eProperty.MaxHealth)
                        {
                            int hitscap = player.ItemBonus[(int)eProperty.MaxHealthCapBonus];
                            if (hitscap > 200)
                            {
                                hitscap = 200;
                            }
                            int hits = player.ItemBonus[(int)eProperty.MaxHealth];
                            if (hits > hitscap + 200)
                            {
                                int tempbonus = hitscap + 200;
                                info.Add(ItemBonusDescription(tempbonus, i, player));
                            }
                            else
                            {
                                int tempbonus = hits;
                                info.Add(ItemBonusDescription(tempbonus, i, player));
                            }
                        }
                        else
                        {
                            info.Add(ItemBonusDescription(player.ItemBonus[i], i, player));
                        }
                    }
                }
            }

            info.Add(" ");
            info.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Realm"));
            if (player.RealmLevel > 10)
                info.Add(string.Format(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Specs", player.RealmLevel / 10)));
            else
                info.Add(" none");
            info.Add(" ");
            info.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Relic"));

            double meleeRelicBonus = RelicMgr.GetRelicBonusModifier(player.Realm, eRelicType.Strength);
            double magicRelicBonus = RelicMgr.GetRelicBonusModifier(player.Realm, eRelicType.Magic);

            info.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Melee", (meleeRelicBonus * 100)));
            info.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Magic", (magicRelicBonus * 100)));

            info.Add(" ");
            info.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "PlayerBonusesListRequestHandler.HandlePacket.Outpost"));
            info.Add("TODO, this needs to be written");

            return info;
        }

        /// <summary>
        /// Helper For Bonus Description
        /// </summary>
        /// <param name="iBonus"></param>
        /// <param name="iBonusType"></param>
        /// <returns></returns>
        private static string ItemBonusDescription(int iBonus, int iBonusType, GamePlayer player)
        {
            //This displays the bonuses just like the Live servers. there is a check against the pts/% differences
            string str = ((iBonusType == 150) | (iBonusType == 210) | (iBonusType == 10) | (iBonusType == 151) | (iBonusType == 186)) ? "pts of " : "% to ";  //150(Health Regen) 151(PowerRegen) and 186(Style reductions) need the prefix of "pts of " to be correct

            //we need to only display the effective bonus, cut off caps

            //Lifeflight add

            //iBonusTypes that cap at 10
            //SpellRange, ArcheryRange, MeleeSpeed, MeleeDamage, RangedDamage, ArcherySpeed,
            //CastingSpeed, ResistPierce, SpellDamage, StyleDamage
            if (iBonusType == (int)eProperty.SpellRange || iBonusType == (int)eProperty.ArcheryRange || iBonusType == (int)eProperty.MeleeSpeed
                || iBonusType == (int)eProperty.MeleeDamage || iBonusType == (int)eProperty.RangedDamage || iBonusType == (int)eProperty.ArcherySpeed
                || iBonusType == (int)eProperty.CastingSpeed || iBonusType == (int)eProperty.ResistPierce || iBonusType == (int)eProperty.SpellDamage || iBonusType == (int)eProperty.StyleDamage)
            {
                if (iBonus > 10)
                    iBonus = 10;
            }

            //cap at 25 with no chance of going over
            //DebuffEffectivness, BuffEffectiveness, HealingEffectiveness
            //SpellDuration
            if (iBonusType == (int)eProperty.DebuffEffectivness || iBonusType == (int)eProperty.BuffEffectiveness || iBonusType == (int)eProperty.HealingEffectiveness || iBonusType == (int)eProperty.SpellDuration || iBonusType == (int)eProperty.ArcaneSyphon)
            {
                if (iBonus > 25)
                    iBonus = 25;
            }
            //hitscap, caps at 200
            if (iBonusType == (int)eProperty.MaxHealthCapBonus)
            {
                if (iBonus > 200)
                    iBonus = 200;
            }
            //cap at 25, but can get overcaps
            //PowerPool
            //This will need to be done in the method that calls this method.
            //if (iBonusType == (int)eProperty.PowerPool)
            //{
            //    if (iBonus > 25)
            //        iBonus = 50;
            //}
            //caps at 50
            //PowerPoolCapBonus
            if (iBonusType == (int)eProperty.PowerPoolCapBonus)
            {
                if (iBonus > 50)
                    iBonus = 50;
            }

            return string.Format("+{0}{1}{2}", iBonus, str, SkillBase.GetPropertyName(((eProperty)iBonusType), player.Client.Account.Language));
        }
        #endregion
    }
}

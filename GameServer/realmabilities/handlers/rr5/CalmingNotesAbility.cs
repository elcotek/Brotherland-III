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
using System;
using System.Collections.Generic;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.RealmAbilities
{
    public class CalmingNotesAbility : RR5RealmAbility
    {
        public CalmingNotesAbility(DBAbility dba, int level) : base(dba, level) { }

        public override void Execute(GameLiving living)
        {
            if (CheckPreconditions(living, DEAD | SITTING | MEZZED | STUNNED)) return;

            SpellLine spline = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
            Spell abSpell = SkillBase.GetSpellByID(7045);
            bool casted = true;
            if (spline != null && abSpell != null)
            {
                if (living != null && living.IsObjectAlive)
                {
                    foreach (GameLiving enemy in living.GetNPCsInRadius((ushort)abSpell.Radius))
                    {
                        if (casted == true && enemy != null && enemy is GameNPC && enemy is GameSiegeWeapon == false && enemy.IsAttackable && enemy.IsAlive && enemy.Realm != living.Realm)
                        {

                            living.CastSpell(abSpell, spline);
                            DisableSkill(living);
                            casted = false;
                        }
                        foreach (GameLiving player in living.GetPlayersInRadius((ushort)abSpell.Radius))
                        {
                            if (casted == true && player != null && player is GamePlayer && player.IsAlive && player.Realm != living.Realm)
                            {

                                living.CastSpell(abSpell, spline);
                                DisableSkill(living);
                                casted = false;
                            }


                        }
                    }

                    if (casted == true)
                    {
                        (living as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((living as GamePlayer).Client.Account.Language, "A valid target must be around you!"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                }
            }
        }
        public override int GetReUseDelay(int level)
        {
            return 300;
        }
        public override void AddEffectsInfo(IList<string> list, GamePlayer player)
        {
            list.Add("This ability does not require an active target to start, but enemys in casters radius.");
            list.Add("Insta-cast spell that mesmerizes all enemys around the caster within 750 radius for 20 seconds.");
            list.Add("");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Radius", " 750"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Target", "Enemy"));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.Duration") + " 20 seconds");
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", "instant"));
        }
    }
}
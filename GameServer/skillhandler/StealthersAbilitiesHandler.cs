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
using DOL.Database;
using DOL.Events;
using log4net;
using System;
using System.Reflection;

namespace DOL.GS
{
    public class StealtherAbilities
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		[ScriptLoadedEvent]
		public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.AddHandler(GamePlayerEvent.KillsTotalDeathBlowsChanged, new DOLEventHandler(AssassinsAbilities));
		}
		
		private static void AssassinsAbilities(DOLEvent e, object sender, EventArgs arguments)
		{
			GamePlayer player = sender as GamePlayer;
			
			//Shadowblade-Blood Rage
			if (player.HasAbility(Abilities.BloodRage))
            {
                player.CastSpell(GetBR(), (SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells)));
            }

            //Infiltrator-Heightened Awareness
            if (player.HasAbility(Abilities.HeightenedAwareness))
            {
                player.CastSpell(GetHA(), (SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells)));
            }

            //Nightshade-Subtle Kills
            if (player.HasAbility(Abilities.SubtleKills))
            {
                player.CastSpell(GetSK(), (SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells)));
            }
        }


		#region Blood Rage Spell
		protected static Spell Blood_Rage;

        public static Spell GetBR()
        {
            if (Blood_Rage == null)
            {
                DBSpell spell = new DBSpell
                {
                    AllowAdd = true,
                    CastTime = 0,
                    Uninterruptible = true,
                    Icon = 10541,
                    ClientEffect = 10541,
                    Description = "Movement speed of the player in stealth is increased by 25% for 1 minute after they get a killing blow on a realm enemy.",
                    Name = "Blood Rage",
                    Range = 0,
                    Value = 25,
                    Duration = 60,
                    SpellID = 900090,
                    Target = "Self",
                    Type = "BloodRage"
                };
                Blood_Rage = new Spell(spell, 50);
                SkillBase.GetSpellList(GlobalSpellsLines.Reserved_Spells).Add(Blood_Rage);
            }
            return Blood_Rage;
        }
        #endregion

        #region Heightened Awareness Spell
        protected static Spell Heightened_Awareness;

        public static Spell GetHA()
        {
            if (Heightened_Awareness == null)
            {
                DBSpell spell = new DBSpell
                {
                    AllowAdd = true,
                    CastTime = 0,
                    Uninterruptible = true,
                    Icon = 10541,
                    ClientEffect = 10541,
                    Description = "Greater Chance to Detect Stealthed Enemies for 1 minute after executing a klling blow on a realm opponent.",
                    Name = "Heightened Awareness",
                    Range = 0,
                    Value = 25,
                    Duration = 60,
                    SpellID = 900091,
                    Target = "Self",
                    Type = "HeightenedAwareness"
                };
                Heightened_Awareness = new Spell(spell, 50);
                SkillBase.GetSpellList(GlobalSpellsLines.Reserved_Spells).Add(Heightened_Awareness);
            }
            return Heightened_Awareness;
        }
        #endregion

        #region Subtle Kills Spell
        protected static Spell Subtle_Kills;

        public static Spell GetSK()
        {
            if (Subtle_Kills == null)
            {
                DBSpell spell = new DBSpell
                {
                    AllowAdd = true,
                    CastTime = 0,
                    Uninterruptible = true,
                    Icon = 10541,
                    ClientEffect = 10541,
                    Description = "Greater chance of remaining hidden while stealthed for 1 minute after executing a killing blow on a realm opponent.",
                    Name = "Subtle Kills",
                    Range = 0,
                    Value = 25,
                    Duration = 60,
                    SpellID = 900092,
                    Target = "Self",
                    Type = "SubtleKills"
                };
                Subtle_Kills = new Spell(spell, 50);
                SkillBase.GetSpellList(GlobalSpellsLines.Reserved_Spells).Add(Subtle_Kills);
            }
            return Subtle_Kills;
        }
        #endregion
    }
}

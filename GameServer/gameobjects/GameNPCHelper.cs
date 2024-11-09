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

namespace DOL.GS
{
    /// <summary>
    /// GameNPC Helper Class is a collection of (static) GameNPC methods to avoid clutter in the GameNPC class itself.
    /// </summary>

    public static class GameNPCHelper
    {
       
        #region GameNPC cast methods
        /// <summary>
        /// Cast a spell on player and its pets/subpets if available.
        /// </summary>
        /// <param name="sourceNPC">NPC that is casting the spell</param>
        /// <param name="player">Player is the owner and first target of the spell</param>
        /// <param name="spell">Casted spell</param>
        /// <param name="line">SpellLine the casted spell is derived from</param>
        /// <param name="checkLOS">Determines if line of sight is checked</param>
        public static void CastSpellOnOwnerAndPets(this GameLiving sourceNPC, GameLiving target, Spell spell, SpellLine line, bool checkLOS)
        {
            sourceNPC.TargetObject = target;

            if (sourceNPC is GameNPC)
                (sourceNPC as GameNPC).CastSpell(spell, line, checkLOS);
            else
                sourceNPC.CastSpell(spell, line);

            if (target.ControlledBrain != null)
            {
                sourceNPC.TargetObject = target.ControlledBrain.Body;

                if (sourceNPC is GameNPC)
                    (sourceNPC as GameNPC).CastSpell(spell, line, checkLOS);
                else
                    sourceNPC.CastSpell(spell, line);

                if (target.ControlledBrain.Body.ControlledNpcList != null)
                    foreach (AI.Brain.IControlledBrain subpet in target.ControlledBrain.Body.ControlledNpcList)
                        if (subpet != null)
                        {
                            sourceNPC.TargetObject = subpet.Body;
                            if (sourceNPC is GameNPC)
                                (sourceNPC as GameNPC).CastSpell(spell, line, checkLOS);
                            else
                            sourceNPC.CastSpell(spell, line);
                        }
            }
        }

        /// <summary>
        /// Cast a spell on player and its pets/subpets if available (LOS checked).
        /// </summary>
        /// <param name="sourceNPC">NPC that is casting the spell</param>
        /// <param name="player">Player is the owner and first target of the spell</param>
        /// <param name="spell">Casted spell</param>
        /// <param name="line">SpellLine the casted spell is derived from</param>
        public static void CastSpellOnOwnerAndPets(this GameNPC sourceNPC, GameLiving target, Spell spell, SpellLine line)
        {
            CastSpellOnOwnerAndPets(sourceNPC, target, spell, line, true);
        }
        #endregion GameNPC cast methods
    }
}


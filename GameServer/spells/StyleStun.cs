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
using DOL.GS.Effects;
using System;





namespace DOL.GS.Spells
{
    /// <summary>
    /// Style stun effect spell handler
    /// </summary>
    [SpellHandler("StyleStun")]
	public class StyleStun : StunSpellHandler
	{
		public override int CalculateSpellResistChance(GameLiving target)
		{
			return 0;
		}

		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			return Spell.Duration;
		}

		/// <summary>
		/// When an applied effect expires.
		/// Duration spells only.
		/// </summary>
		/// <param name="effect">The expired effect</param>
		/// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
		/// <returns>immunity duration in milliseconds</returns>
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{

            {
                if (effect.Owner is GameNPC)
                {
                    AttackData ad = Caster.TempProperties.getProperty<object>(GameLiving.LAST_ATTACK_DATA, null) as AttackData;
                    if (ad != null)
                    {
                        IOldAggressiveBrain aggroBrain = ((GameNPC)effect.Owner).Brain as IOldAggressiveBrain;
                        if (aggroBrain != null)
                        {
                            int aggro = Convert.ToInt32(ad.Damage * 4);
                            aggroBrain.AddToAggroList(Caster, aggro);

                            //log.DebugFormat("Damage: {0}, Taunt Value: {1}, (de)Taunt Amount {2}", ad.Damage, Spell.Value, aggro.ToString());
                        }
                    }
                }
            }
          
			// http://www.camelotherald.com/more/1749.shtml
			// immunity timer will now be exactly five times the length of the stun
			base.OnEffectExpires(effect, noMessages);
			return Spell.Duration * 5;
		}

		/// <summary>
		/// Determines wether this spell is compatible with given spell
		/// and therefore overwritable by better versions
		/// spells that are overwritable cannot stack
		/// </summary>
		/// <param name="compare"></param>
		/// <returns></returns>
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			if (Spell.EffectGroup != 0)
				return Spell.EffectGroup == compare.Spell.EffectGroup;
			if (compare.Spell.SpellType == "Stun") return true;
			return base.IsOverwritable(compare);
		}

		// constructor
		public StyleStun(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
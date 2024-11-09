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

// Original code by Dinberg

using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using log4net;
using System.Collections.Generic;
using System.Reflection;

namespace DOL.GS.Spells
{
    /// <summary>
    /// This pet is purely aesthetic and can't be cast in RvR zones
    /// </summary>
    [SpellHandler("SummonNoveltyPet")]
    public class SummonNoveltyPet : SummonSpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Constructs the spell handler
        /// </summary>
		public SummonNoveltyPet(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);

			if (pet != null)
			{
				pet.Flags |= GameNPC.eFlags.PEACE; //must be peace!

				//No brain for now, so just follow owner.
				pet.Follow(Caster, 100, WorldMgr.VISIBILITY_DISTANCE);

				Caster.TempProperties.setProperty(NoveltyPetBrain.HAS_PET, true);
			}
                        
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
            base.OnEffectExpires(effect, noMessages);

            //log.Error("time has expired");
            //GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
            //Caster.TempProperties.setProperty(NoveltyPetBrain.HAS_PET, true);
            Caster.TempProperties.setProperty(NoveltyPetBrain.HAS_PET, false);
            if (!noMessages && Spell.Pulse == 0)
            {
                MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
                Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, false)), eChatType.CT_SpellExpires, effect.Owner);
            }
            return 0;
        }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster.CurrentRegion.IsRvR || Caster.CurrentRegion.IsHousing || Caster.CurrentRegion.IsCapitalCity)
            {
                MessageToCaster("You cannot cast this spell here!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted);
                return false;
            }

			if (Caster.TempProperties.getProperty<bool>(NoveltyPetBrain.HAS_PET, false))
			{
                MessageToCaster("You already has a Follower!", DOL.GS.PacketHandler.eChatType.CT_SpellResisted);
                // no message
                return false;
			}

            return base.CheckBeginCast(selectedTarget);
        }

        /// <summary>
        /// These pets aren't controllable!
        /// </summary>
        /// <param name="brain"></param>
        protected override void SetBrainToOwner(IControlledBrain brain)
        {
        }

        protected override IControlledBrain GetPetBrain(GameLiving owner)
        {
            return new NoveltyPetBrain(owner as GamePlayer);
        }

		public override IList<string> DelveInfo(GamePlayer player)
        {
			
			{
				var list = new List<string>();
				list.Add(string.Format("  {0}", Spell.Description));

				return list;
			}
		}
    }
}

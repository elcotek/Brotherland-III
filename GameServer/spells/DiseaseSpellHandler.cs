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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;

namespace DOL.GS.Spells
{
	[SpellHandler("Disease")]
	public class DiseaseSpellHandler : SpellHandler
	{
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);

			if (effect.Owner.Realm == 0 || Caster.Realm == 0)
			{
				effect.Owner.LastAttackedByEnemyTickPvE = effect.Owner.CurrentRegion.Time;
				Caster.LastAttackTickPvE = Caster.CurrentRegion.Time;
			}
			else
			{
				effect.Owner.LastAttackedByEnemyTickPvP = effect.Owner.CurrentRegion.Time;
				Caster.LastAttackTickPvP = Caster.CurrentRegion.Time;
			}

			GameSpellEffect mezz = FindEffectOnTarget(effect.Owner, "Mesmerize");
			if (mezz != null) mezz.Cancel(false);
			effect.Owner.Disease(true);
			effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 0.15);
			effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.Strength, this, 1.0 - 0.075);

			SendUpdates(effect);

			MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
			Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_System, effect.Owner);

			effect.Owner.StartInterruptTimer(effect.Owner.SpellInterruptDuration, AttackData.eAttackType.Spell, Caster);
			if (effect.Owner is GameNPC)
			{
				IOldAggressiveBrain aggroBrain = ((GameNPC)effect.Owner).Brain as IOldAggressiveBrain;
				if (aggroBrain != null)
					aggroBrain.AddToAggroList(Caster, 1);
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			base.OnEffectExpires(effect, noMessages);
			effect.Owner.Disease(false);
			effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
			effect.Owner.BuffBonusMultCategory1.Remove((int)eProperty.Strength, this);

			if (!noMessages)
			{
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_SpellExpires);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_SpellExpires, effect.Owner);
			}

			SendUpdates(effect);

			return 0;
		}

		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			double duration = base.CalculateEffectDuration(target, effectiveness);
			duration -= duration * target.GetResist(Spell.DamageType) * 0.01;

			if (duration < 1)
				duration = 1;
			else if (duration > (Spell.Duration * 4))
				duration = (Spell.Duration * 4);
			return (int)duration;
		}

		protected virtual void SendUpdates(GameSpellEffect effect)
		{
			effect.Owner.UpdateMaxSpeed();
			if (effect.Owner is GamePlayer player)
			{
				player.Out.SendCharStatsUpdate();
				player.Out.SendUpdateWeaponAndArmorStats();
			}
		}

		public override PlayerXEffect GetSavedEffect(GameSpellEffect e)
		{
			if ( //VaNaTiC-> this cannot work, cause PulsingSpellEffect is derived from object and only implements IConcEffect
				 //e is PulsingSpellEffect ||
				 //VaNaTiC<-
				Spell.Pulse != 0 || Spell.Concentration != 0 || e.RemainingTime < 1)
				return null;
			PlayerXEffect eff = new PlayerXEffect();
			eff.Var1 = Spell.ID;
			eff.Duration = e.RemainingTime;
			eff.IsHandler = true;
			eff.SpellLine = SpellLine.KeyName;
			return eff;
		}

		public override void OnEffectRestored(GameSpellEffect effect, int[] vars)
		{
			effect.Owner.Disease(true);
			effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, 1.0 - 0.15);
			effect.Owner.BuffBonusMultCategory1.Set((int)eProperty.Strength, this, 1.0 - 0.075);
		}

		public override int OnRestoredEffectExpires(GameSpellEffect effect, int[] vars, bool noMessages)
		{
			return this.OnEffectExpires(effect, noMessages);
		}

		public DiseaseSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }

		//TODO: Adjust strength reduction
		///public override string ShortDescription
		//	=> $"Inflicts a wasting disease on the target that slows target by 15%, reduces its strength by 7.5% and inhibits healing by 50%";
	}
}

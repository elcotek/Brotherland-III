using System;
using System.Collections;
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
	[SpellHandlerAttribute("VampiirEffectivenessDeBuff")]
	public class VampiirEffectivenessDeBuff : SpellHandler
	{
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }


        /// <summary>
        /// When an applied effect starts
        /// duration spells only
        /// </summary>
        /// <param name="effect"></param>
        public override void OnEffectStart(GameSpellEffect effect)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Effectiveness -= Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
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
            GamePlayer player = effect.Owner as GamePlayer;
            if (player != null)
            {
                player.Effectiveness += Spell.Value * 0.01;
                player.Out.SendUpdateWeaponAndArmorStats();
                player.Out.SendStatusUpdate();
            }
            return 0;
        }

        //Effectiveness zwischenspeichern ist der falsche weg!!
        /*
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}

		
		public override void OnEffectStart(GameSpellEffect effect)
		{

			if (effect.Owner is GamePlayer && effect.Owner != null)
			{
				base.OnEffectStart(effect);
				GamePlayer player = effect.Owner as GamePlayer;
                // Virtual was here :3
                //player.BaseBuffBonusCategory[0] = (int)player.Effectiveness;
                player.TempProperties.setProperty("PreEffectivenessDebuff", player.Effectiveness);

				double effectiveness =  player.Effectiveness;
				double valueToAdd = (Spell.Value * effectiveness)/100;
				valueToAdd = effectiveness - valueToAdd;

				if ((valueToAdd) > 0)
				{
					player.Effectiveness = valueToAdd;
				}
				else
				{
					player.Effectiveness = 0;
				}
			

				MessageToLiving(effect.Owner, Spell.Message1, eChatType.CT_Spell);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message2, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
                // Added to fix?
               player.Out.SendUpdateWeaponAndArmorStats();
               player.Out.SendStatusUpdate();
			}
			
		}

		
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			if (effect.Owner is GamePlayer && effect.Owner != null)
			{
				GamePlayer player = effect.Owner as GamePlayer;
                //player.Effectiveness = player.BaseBuffBonusCategory[0];
                player.Effectiveness = player.TempProperties.getProperty<double>("PreEffectivenessDebuff");
                player.TempProperties.removeProperty("PreEffectivenessDebuff");
                //player.BaseBuffBonusCategory[0] = 0;
				MessageToLiving(effect.Owner, Spell.Message3, eChatType.CT_Spell);
				Message.SystemToArea(effect.Owner, Util.MakeSentence(Spell.Message4, effect.Owner.GetName(0, true)), eChatType.CT_Spell, effect.Owner);
                // Added to fix?
               player.Out.SendUpdateWeaponAndArmorStats();
               player.Out.SendStatusUpdate();
            }
			return 0;
		}
        */

        public override IList<string> DelveInfo(GamePlayer player)
        {
			
			{
				var list = new List<string>(16);
				//Name
				list.Add("Name: " + Spell.Name);
				//Description
				list.Add("Description: " + Spell.Description);
				//Target
				list.Add("Target: " + Spell.Target);
				//Cast
				list.Add("Casting time: " + (Spell.CastTime*0.001).ToString("0.0## sec;-0.0## sec;'instant'"));
				//Duration
				if (Spell.Duration >= ushort.MaxValue*1000)
					list.Add("Duration: Permanent.");
				else if (Spell.Duration > 60000)
					list.Add(string.Format("Duration: {0}:{1} min", Spell.Duration/60000, (Spell.Duration%60000/1000).ToString("00")));
				else if (Spell.Duration != 0)
					list.Add("Duration: " + (Spell.Duration/1000).ToString("0' sec';'Permanent.';'Permanent.'"));
				//Recast
				if (Spell.RecastDelay > 60000)
					list.Add("Recast time: " + (Spell.RecastDelay/60000).ToString() + ":" + (Spell.RecastDelay%60000/1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add("Recast time: " + (Spell.RecastDelay/1000).ToString() + " sec");
				//Range
				if(Spell.Range != 0) list.Add("Range: " + Spell.Range);
				//Radius
				if(Spell.Radius != 0) list.Add("Radius: " + Spell.Radius);
				//Cost
				if (Spell.Power != 0)
					list.Add("Power cost: " + Spell.Power.ToString("0;0'%'"));
				//Effect
				list.Add("Debuff Effectiveness Of target By: " + Spell.Value + "%");

				if (Spell.Frequency != 0)
					list.Add("Frequency: " + (Spell.Frequency*0.001).ToString("0.0"));
								
				return list;
			}
		}

		
		public VampiirEffectivenessDeBuff(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}
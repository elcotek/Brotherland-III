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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Chamber")]
	public class ChamberSpellHandler : SpellHandler
	{
		private Spell m_primaryspell = null;
		private SpellLine m_primaryspellline = null;
		private Spell m_secondaryspell = null;
		private SpellLine m_secondaryspelline = null;
		private int m_effectslot = 0;

		public Spell PrimarySpell
		{
			get
			{
				return m_primaryspell;
			}
			set
			{
				m_primaryspell = value;
			}
		}

		public SpellLine PrimarySpellLine
		{
			get
			{
				return m_primaryspellline;
			}
			set
			{
				m_primaryspellline = value;
			}
		}

		public Spell SecondarySpell
		{
			get
			{
				return m_secondaryspell;
			}
			set
			{
				m_secondaryspell = value;
			}
		}

		public SpellLine SecondarySpellLine
		{
			get
			{
				return m_secondaryspelline;
			}
			set
			{
				m_secondaryspelline = value;
			}
		}

		public int EffectSlot
		{
			get
			{
				return m_effectslot;
			}
			set
			{
				m_effectslot = value;
			}
		}
		
		public override void InterruptCasting()
		{
			base.InterruptCasting();
			Caster.CurrentSpellHandler = null;
		}
		public override bool CastSpell()
		{
			GamePlayer caster = (GamePlayer)m_caster;
			m_spellTarget = caster.TargetObject as GameLiving;
			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(caster, "Chamber", m_spell.Name);
			if(effect != null && m_spell.Name == effect.Spell.Name)
			{
				ISpellHandler spellhandler = null;
				ISpellHandler spellhandler2 = null;
				ChamberSpellHandler chamber = (ChamberSpellHandler)effect.SpellHandler;
				GameSpellEffect PhaseShift = SpellHandler.FindEffectOnTarget(m_spellTarget, "Phaseshift");
				SelectiveBlindnessEffect SelectiveBlindness = Caster.EffectList.GetOfType<SelectiveBlindnessEffect>();
				spellhandler = ScriptMgr.CreateSpellHandler(caster, chamber.PrimarySpell, chamber.PrimarySpellLine);

				#region Pre-checks
				int duration = caster.GetSkillDisabledDuration(m_spell);
				if (duration > 0)
				{
                    if (Caster is GamePlayer)
                    {
                        //MessageToCaster("You must wait " + (duration / 1000 + 1) + " seconds to use this spell!", eChatType.CT_System);
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourMustWaitToUseSpell", (duration / 1000 + 1)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (caster.IsMoving || caster.IsStrafing)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("You must be standing still to cast this spell!", eChatType.CT_System);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client, "SpellHandler.YouMustBeStandingStillToCast"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (caster.IsSitting)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("You can't cast this spell while sitting!", eChatType.CT_System);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client, "SpellHandler.YouCantCastThisSpellWhileSitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (m_spellTarget == null)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("You must have a target!", eChatType.CT_SpellResisted);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client, "SpellHandler.YouMustHaveATarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
                    
				}
				if (!caster.IsAlive)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("You cannot cast you are dead!", eChatType.CT_SpellResisted);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client, "SpellHandler.YouAreDeatCantCast"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (!m_spellTarget.IsAlive)
				{

                    ((GamePlayer)caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)caster).Client.Account.Language, "HealSpellHandler.Execute.TargetIsDead", m_spellTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("You cannot cast this on the dead!", eChatType.CT_SpellResisted);
					return false;
				}
				if (caster.IsMezzed || caster.IsStunned || caster.IsSilenced)
				{
					MessageToCaster("You can't use that in your state.", eChatType.CT_System);
                    ((GamePlayer)caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)caster).Client.Account.Language, "SpellHandler.Execute.YouCantUseThatInYourState", m_spellTarget.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
				}
				if (!caster.TargetInView)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("Your target is not visible!", eChatType.CT_SpellResisted);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNotVisible"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (caster.IsObjectInFront(m_spellTarget, 180) == false)
				{
                    //MessageToCaster("Your target is not in view!", eChatType.CT_SpellResisted);
                    if (caster is GamePlayer)
                    {
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (caster.IsInvulnerableToAttack)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("Your invunerable at the momment and cannot use that spell!", eChatType.CT_System);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "SpellHandler.YourInvunerableAtTheMomment"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (m_spellTarget is GamePlayer)
				{
					if ((m_spellTarget as GamePlayer).IsInvulnerableToAttack)
					{
                        if (caster is GamePlayer)
                        {
                            //MessageToCaster("Your target is invunerable at the momment and cannot be attacked!", eChatType.CT_System);
                            (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetsInvunerableAtTheMomment"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        }
                            return false;
					}
				}
				if (!caster.IsWithinRadius(m_spellTarget, ((SpellHandler)spellhandler).CalculateSpellRange()))
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster("That target is too far away!", eChatType.CT_SpellResisted);
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (PhaseShift != null)
				{
                    if (caster is GamePlayer)
                    {
                        //MessageToCaster(m_spellTarget.Name + " is Phaseshifted and can't be attacked!", eChatType.CT_System); return false;
                        (caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((caster as GamePlayer).Client.Account.Language, "SpellHandler.YouArePhaseshiftedCantBeAttacked", m_spellTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                    }
				if (SelectiveBlindness != null)
				{
					GameLiving EffectOwner = SelectiveBlindness.EffectSource;
					if (EffectOwner == m_spellTarget)
					{
                        if (m_caster is GamePlayer)
                        {
                            //((GamePlayer)m_caster).Out.SendMessage(string.Format("{0} is invisible to you!", m_spellTarget.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                            (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.IsNotVisibleForYou", m_spellTarget.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                        }
                            return false;
					}
				}
				if (m_spellTarget.HasAbility(Abilities.DamageImmunity))
				{
                    //MessageToCaster(m_spellTarget.Name + " is immune to this effect!", eChatType.CT_SpellResisted);
                    if (m_caster is GamePlayer)
                    {
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "CCSpellHandler.TargetIsImmuneToThisEffect", m_spellTarget.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (GameServer.ServerRules.IsAllowedToAttack(Caster, m_spellTarget, true) && chamber.PrimarySpell.Target.ToLower() == "realm")
				{
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("This spell only works on friendly targets!", eChatType.CT_System);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.SpellOnlyWorksOnFriendlyTargets"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				if (!GameServer.ServerRules.IsAllowedToAttack(Caster, m_spellTarget, true) && chamber.PrimarySpell.Target.ToLower() != "realm")
				{
                    if (m_caster is GamePlayer)
                    {
                        //MessageToCaster("That target isn't attackable at this time!", eChatType.CT_System);
                        (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SpellHandler.ThatTargetIsntAttackableAtThisTime"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                        return false;
				}
				spellhandler.CastSpell();
				#endregion

				if (chamber.SecondarySpell != null)
				{
					spellhandler2 = ScriptMgr.CreateSpellHandler(caster, chamber.SecondarySpell, chamber.SecondarySpellLine);
					spellhandler2.CastSpell();
				}
				effect.Cancel(false);

				if (m_caster is GamePlayer)
				{
					GamePlayer player_Caster = Caster as GamePlayer;
					foreach (SpellLine spellline in player_Caster.GetSpellLines())
						foreach (Spell sp in SkillBase.GetSpellList(spellline.KeyName))
							if (sp.SpellType == m_spell.SpellType)
								m_caster.DisableSkill(sp, sp.RecastDelay);
				}
				else if (m_caster is GameNPC)
					m_caster.DisableSkill(m_spell, m_spell.RecastDelay);
			}
			else
			{
				base.CastSpell ();
				int duration = caster.GetSkillDisabledDuration(m_spell);
                if (Caster is GamePlayer && duration == 0)

                    if (caster is GamePlayer)
                    {
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectTheFirstSpellName", Spell.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        //((GamePlayer)Caster).Out.SendMessage("Select the first spell for your " + Spell.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
			}
			return true;
		}

		/// <summary>
		/// Fire bolt
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			
			// endurance
			m_caster.Endurance -= 5;

			// messages
			GamePlayer caster = (GamePlayer)m_caster;
			if (Spell.InstrumentRequirement == 0)
			{
				if(SecondarySpell == null && PrimarySpell == null)
				{
					MessageToCaster("No spells were loaded into " + m_spell.Name + ".", eChatType.CT_Spell);
				}
				else
				{
					//MessageToCaster("Your " + m_spell.Name + " is ready for use.", eChatType.CT_Spell);
                    ((GamePlayer)caster).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)caster).Client.Account.Language, "SpellHandler.YourSpellIsReadyForUse", m_spell.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    //StartSpell(target); // and action
                    GameSpellEffect neweffect = CreateSpellEffect(target, 1);
					neweffect.Start(m_caster);
					SendEffectAnimation(m_caster, 0, false, 1);
					((GamePlayer)m_caster).Out.SendWarlockChamberEffect((GamePlayer)m_caster);
				}
				
				foreach (GamePlayer player in m_caster.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{
					if (player != m_caster)
                        ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellHandler.SendCastMessage.OtherCast", m_caster.Name), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                    //player.Out.SendMessage(m_caster.GetName(0, true) + " casts a spell!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
				}
			}

			//the quick cast is unallowed whenever you miss the spell
			//set the time when casting to can not quickcast during a minimum time
			if (m_caster is GamePlayer)
			{
				QuickCastEffect quickcast = m_caster.EffectList.GetOfType<QuickCastEffect>();
				if (quickcast != null && Spell.CastTime > 0)
				{
					m_caster.TempProperties.setProperty(GamePlayer.QUICK_CAST_CHANGE_TICK, m_caster.CurrentRegion.Time);
					m_caster.DisableSkill(SkillBase.GetAbility(Abilities.Quickcast), QuickCastAbilityHandler.DISABLE_DURATION);
					quickcast.Cancel(false);
				}
			}
		}

		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{

			((GamePlayer)m_caster).Out.SendWarlockChamberEffect((GamePlayer)effect.Owner);
			return base.OnEffectExpires (effect, noMessages);
		}

		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, 0, 0, effectiveness);
		}

		public static int GetEffectSlot(string spellName)
		{
			switch(spellName)
			{
                case "Chamber of Lesser Fade":
					return 1;
				case "Chamber of Restraint":
					return 2;
				case "Chamber of Destruction":
					return 3;
                case "Chamber of Creation":
					return 4;
				case "Chamber of Greater Fate":
					return 5;
                case "Chamber of Decimation":
					return 5;
            }

			return 0;
		}
		#region Devle Info
		public override IList<string> DelveInfo(GamePlayer player)
        {
			
			{
				var list = new List<string>();

				//Name
				list.Add("Name: " + Spell.Name);
				list.Add("");

				//Description
				list.Add("Description: " + Spell.Description);
				list.Add("");

				//SpellType
				if (!Spell.AllowBolt)
					list.Add("Type: Any but bolts");
				if (Spell.AllowBolt)
					list.Add("Type: Any");

				//Cast
				list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.CastingTime", (Spell.CastTime * 0.001).ToString("0.0## sec;-0.0## sec;'instant'")));
				//Recast
				if (Spell.RecastDelay > 60000)
					list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + (Spell.RecastDelay / 60000).ToString() + ":" + (Spell.RecastDelay % 60000 / 1000).ToString("00") + " min");
				else if (Spell.RecastDelay > 0)
					list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "DelveInfo.RecastTime") + (Spell.RecastDelay / 1000).ToString() + " sec");
				return list;
			}
		}
		#endregion
		// constructor
		public ChamberSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) {}
	}
}

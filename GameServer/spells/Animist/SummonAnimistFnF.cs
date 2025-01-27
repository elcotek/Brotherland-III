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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using System;



namespace DOL.GS.Spells
{
    /// <summary>
    /// Summon a fnf animist pet.
    /// </summary>
    [SpellHandler("SummonAnimistFnF")]
	public class SummonAnimistFnF : SummonAnimistPet
	{
		public SummonAnimistFnF(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

       


        public override bool CheckBeginCast(GameLiving selectedTarget)
		{
			int nCount = 0;

            if (Caster is GamePlayer && (Caster as GamePlayer).HasGroundTarget() == false)
            {
                if (m_caster is GamePlayer)
                {
                   (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SummonAnimistFnFSpellHandler.CantSummonTurretWithoutGroundtarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                return false;
			}

			foreach (GameNPC npc in Caster.CurrentRegion.GetNPCsInRadius(Caster.GroundTarget.X, Caster.GroundTarget.Y, Caster.GroundTarget.Z, (ushort)Properties.TURRET_AREA_CAP_RADIUS, false))
				if (npc.Brain is TurretFNFBrain)
					nCount++;

			if (nCount >= Properties.TURRET_AREA_CAP_COUNT)
			{
                if (m_caster is GamePlayer)
                {
                    //MessageToCaster("You can't summon anymore Turrets in this Area!", eChatType.CT_SpellResisted);
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SummonAnimistFnFSpellHandler.CantSummonAnymoreTurretInArea"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                    return false;
			}

			if (Caster.PetCount >= Properties.TURRET_PLAYER_CAP_COUNT)
			{
                
                if (m_caster is GamePlayer && (m_caster as GamePlayer).CurrentRegion.IsRvR)
                {
                    //MessageToCaster("You cannot control anymore Turrets!", eChatType.CT_SpellResisted);
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SummonAnimistFnFSpellHandler.CanNotControlAnymoreTurrets"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return false;
                }
                    
                if (m_caster is GamePlayer && Caster.PetCount >= Properties.TURRET_PLAYER_CAP_COUNT_PVE)
                {
                   
                    //MessageToCaster("You cannot control anymore Turrets!", eChatType.CT_SpellResisted);
                    (m_caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((m_caster as GamePlayer).Client.Account.Language, "SummonAnimistFnFSpellHandler.CanNotControlAnymoreTurrets"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return false;

                }
            }


			return base.CheckBeginCast(selectedTarget);
		}

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);

            if (Spell.SubSpellID > 0 && SkillBase.GetSpellByID(Spell.SubSpellID) != null)
            {
                pet.Spells.Add(SkillBase.GetSpellByID(Spell.SubSpellID));
            }

            (pet.Brain as TurretBrain).IsMainPet = false;
            if (!pet.TargetInView)
              return;
            {
                (pet.Brain as IOldAggressiveBrain).AddToAggroList(target, 1);
                (pet.Brain as TurretBrain).Think();
                //[Ganrod] Nidel: Set only one spell.
                (pet as TurretPet).TurretSpell = pet.Spells[0] as Spell;
                Caster.PetCount++;
            }
        }        

		protected override void SetBrainToOwner(IControlledBrain brain)
		{
		}

		/// <summary>
		/// [Ganrod] Nidel: Can remove TurretFNF
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
		{
			pet = sender as GamePet;
			if (pet == null)
				return;

			if ((pet.Brain as TurretFNFBrain) == null)
				return;

			if (Caster.ControlledBrain == null)
			{
				((GamePlayer)Caster).Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
			}

			GameEventMgr.RemoveHandler(pet, GameLivingEvent.PetReleased, OnNpcReleaseCommand);

			GameSpellEffect effect = FindEffectOnTarget(pet, this);
			if (effect != null)
				effect.Cancel(false);
		}

		protected override byte GetPetLevel()
		{
			byte level = base.GetPetLevel();
			if (level > 44)
				level = 44;
			return level;
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
			Caster.PetCount--;

			return base.OnEffectExpires(effect, noMessages);
		}

		protected override IControlledBrain GetPetBrain(GameLiving owner)
		{
			return new TurretFNFBrain(owner);
		}
	}
}

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
/*
 * [Ganrod] Nidel 2008-07-08
 * - Useless using removed
 * - Get Spell
 * - Add GetGamePet override for use TurretPet class
 */
using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using DOL.Language;


namespace DOL.GS.Spells
{
  /// <summary>
  /// Summon an animist pet.
  /// </summary>
  public abstract class SummonAnimistPet : SummonSpellHandler
  {
    public SummonAnimistPet(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
    {
    }

    /// <summary>
    /// Check whether it's possible to summon a pet.
    /// </summary>
    /// <param name="selectedTarget"></param>
    /// <returns></returns>
    public override bool CheckBeginCast(GameLiving selectedTarget)
    {
      if(Caster.GroundTarget == null)
      {
         // MessageToCaster("You have to set a ground target for this Spell.", eChatType.CT_SpellResisted);
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SummonAnimistFnFSpellHandler.CantSummonTurretWithoutGroundtarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
      }

      if(!Caster.GroundTargetInView)
      {
                if (Caster is GamePlayer)
                {
                    // MessageToCaster("Your ground target is not in view.", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourGroundTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                    return false;
      }

      if(!Caster.IsWithinRadius( Caster.GroundTarget, CalculateSpellRange() ))
      {
                if (Caster is GamePlayer)
                {
                    // MessageToCaster("You have to select a closer ground target.", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SummonAnimistMainPetSpellHandler.SelectACloserGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                    return false;
      }

      return base.CheckBeginCast(selectedTarget);
    }
    public override void FinishSpellCast(GameLiving target)
    {
        if (Caster.GroundTarget == null)
        {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouNeedAGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("You have to set a ground target for this Spell.", eChatType.CT_SpellResisted);
                }
                    return;
        }

        if (!Caster.GroundTargetInView)
        {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("Your ground target is not in view.", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourGroundTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                    return;
        }

        if (!Caster.IsWithinRadius(Caster.GroundTarget, CalculateSpellRange()))
        {
                if (Caster is GamePlayer)
                {
                    //MessageToCaster("You have to select a closer ground target.", eChatType.CT_SpellResisted);
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SummonAnimistMainPetSpellHandler.SelectACloserGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                }
                    return;
        }

        base.FinishSpellCast(target);
    }

    public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
    {
      base.ApplyEffectOnTarget(target, effectiveness);

      pet.Name = Spell.Name;
      //[Ganrod] Nidel: Set only one spell.
      if(pet.Spells != null && pet.Spells.Count > 0)
      {
        (pet as TurretPet).TurretSpell = pet.Spells[0] as Spell;
      }
    }

    //[Ganrod] Nidel: use TurretPet
    protected override GamePet GetGamePet(INpcTemplate template)
    {
      return new TurretPet(template);
    }

    protected override IControlledBrain GetPetBrain(GameLiving owner)
    {
      return new TurretBrain(owner);
    }

    protected override void GetPetLocation(out int x, out int y, out int z, out ushort heading, out Region region)
    {
      x = Caster.GroundTarget.X;
      y = Caster.GroundTarget.Y;
      z = Caster.GroundTarget.Z;
      heading = Caster.Heading;
      region = Caster.CurrentRegion;
    }
  }
}
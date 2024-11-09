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
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using System;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Pet summon spell handler
    ///
    /// Spell.LifeDrainReturn is used for pet ID.
    ///
    /// Spell.Value is used for hard pet level cap
    /// Spell.Damage is used to set pet level:
    /// less than zero is considered as a percent (0 .. 100+) of target level;
    /// higher than zero is considered as level value.
    /// Resulting value is limited by the Byte field type.
    /// Spell.DamageType is used to determine which type of pet is being cast:
    /// 0 = melee
    /// 1 = healer
    /// 2 = mage
    /// 3 = debuffer
    /// 4 = Buffer
    /// 5 = Range
    /// </summary>
    [SpellHandler("BossPetSummonMinion")]
    public class BossPetSummonMinionHandler : SummonSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public BossPetSummonMinionHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line) { }

        /// <summary>
        /// All checks before any casting begins
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            
            /*
            if (Caster is GameNPC && ((GameNPC)Caster).ControlledBrain == null)
            {
                log.DebugFormat("BossPetSummonMinionHandler no subpets found!");
                return false;
            }
            */
            if (Caster is GameNPC && (Caster as GameNPC).ControlledNpcList == null || (Caster as GameNPC).PetCount >= (Caster as GameNPC).ControlledNpcList.Length)
            {
               // log.DebugFormat("BossPetSummonMinionHandler to mutch pets castet!");

                return false;
            }
            return base.CheckBeginCast(selectedTarget);
        }


        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            if (Caster == null)// || Caster.ControlledBrain == null)
            {
                log.Error("keinen Caster gefunden gefunden!");
                return;
            }

            GameNPC temppet = Caster as GameNPC;
           
            //Lets let NPC's able to cast minions.  Here we make sure that the Caster is a GameNPC
            //and that m_controlledNpc is initialized (since we aren't thread safe).
            if (temppet == null)
            {
                if (Caster is GameNPC)
                {
                    temppet = (GameNPC)Caster;
                    //We'll give default NPCs 2 minions!
                    if (temppet.ControlledNpcList == null)
                        temppet.InitControlledBrainArray(1);
                }
                else
                    return;
            }

            base.ApplyEffectOnTarget(target, effectiveness);

            if (pet.Brain is BossArcherBrain)
            {
                ItemTemplate temp = GameServer.Database.FindObjectByKey<ItemTemplate>("BD_Archer_Distance_bow") as ItemTemplate;
                if (temp == null)
                    log.Error("Unable to find Bonedancer Archer's Bow");
                else
                {
                    if (pet.Inventory == null)
                        pet.Inventory = new GameNPCInventory(new GameNpcInventoryTemplate());
                    else
                        pet.Inventory.RemoveItem(pet.Inventory.GetItem(eInventorySlot.DistanceWeapon));

                    pet.Inventory.AddItem(eInventorySlot.DistanceWeapon, GameInventoryItem.Create<ItemTemplate>(temp));
                }
                pet.UpdateNPCEquipmentAppearance();
            }
        }

        /// <summary>
        /// Called when owner release NPC
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="arguments"></param>
        protected override void OnNpcReleaseCommand(DOLEvent e, object sender, EventArgs arguments)
        {
            GameNPC pet = sender as GameNPC;
            if (pet == null)
                return;

            GameEventMgr.RemoveHandler(pet, GameLivingEvent.PetReleased, new DOLEventHandler(OnNpcReleaseCommand));

            GameSpellEffect effect = FindEffectOnTarget(pet, this);
            if (effect != null)
                
           // PetsDie(Caster as GameNPC);
            effect.Cancel(false);
        }
        /// <summary>
        /// Direct the subpets to attack too
        /// </summary>
        /// <param name="target">The target to attack</param>
        public virtual void PetsDie(GameNPC owner)
        {
            if (owner != null && owner is GameNPC && (owner as GameNPC).ControlledNpcList != null)
            {
                lock ((owner as GameNPC).ControlledNpcList)
                {
                    foreach (BossPetBrain icb in (owner as GameNPC).ControlledNpcList)
                        if (icb != null && icb.Body.IsAlive)
                            icb.Body.Die(icb.Body);



                }
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            //if ((effect.Owner is BossPet) && ((effect.Owner as BossNPC) is GameNPC))

                if ((effect.Owner is BossPet) && ((effect.Owner as BossPet).Brain is IControlledBrain) && (((effect.Owner as BossPet).Brain as IControlledBrain).Owner is GameNPC))
                {
                BossPet pet = effect.Owner as BossPet;
                GameNPC commander = (pet.Brain as IControlledBrain).Owner as GameNPC;
                commander.RemoveControlledNpc(pet.Brain as IControlledBrain);
            }
            return base.OnEffectExpires(effect, noMessages);
        }

        protected override IControlledBrain GetPetBrain(GameLiving owner)
        {
            IControlledBrain controlledBrain = null;
            BossSubPet.SubPetType type = (BossSubPet.SubPetType)(byte)this.Spell.DamageType;
            owner = Caster as GameNPC;
           
            switch (type)
            {
                //Melee
                case BossSubPet.SubPetType.Melee:
                    controlledBrain = new BossMeleeBrain(owner);
                    break;
                //Healer
                case BossSubPet.SubPetType.Healer:
                    controlledBrain = new BossHealerBrain(owner);
                    break;
                //Mage
                case BossSubPet.SubPetType.Caster:
                    controlledBrain = new BossCasterBrain(owner);
                    break;
                //Debuffer
                case BossSubPet.SubPetType.Debuffer:
                    controlledBrain = new BossDebufferBrain(owner);
                    break;
                //Buffer
                case BossSubPet.SubPetType.Buffer:
                    controlledBrain = new BossBufferBrain(owner);
                    break;
                //Range
                case BossSubPet.SubPetType.Archer:
                    controlledBrain = new BossArcherBrain(owner);
                    break;
                //Other
                default:
                    controlledBrain = new ControlledNpcBrain(owner);
                    break;
            }

            return controlledBrain;
        }

        protected override GamePet GetGamePet(INpcTemplate template)
        {
            return new BossSubPet(template);
        }

        protected override void SetBrainToOwner(IControlledBrain brain)
        {
            (Caster as GameNPC).AddControlledNpc(brain);
        }

        protected override byte GetPetLevel()
        {
            byte level = base.GetPetLevel();

            //edit for BD
            //Patch 1.87: subpets have been increased by one level to make them blue
            //to a level 50
            if (level == 37 && (pet.Brain as IControlledBrain).Owner.Level >= 41)
                level = 41;

            return level;
        }
    }
}
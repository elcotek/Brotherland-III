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
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS
{
    /// <summary>
    /// GameMovingObject is a base class for boats and siege weapons.
    /// </summary>
    public class GameSiegeCauldron : GameSiegeWeapon
    {
        public GameKeepComponent Component = null;
       
        public GameSiegeCauldron()
            : base()
        {
            MeleeDamageType = eDamageType.Heat;
            Name = "cauldron of boiling oil";
            AmmoType = 0x3B;
            EnableToMove = false;
            Effect = 0x8A1;
            Model = 0xA2F;
            Flags ^= eFlags.FLYING;
            CurrentState = eState.Aimed;
            SetGroundTarget(X, Y, Z - 100);
            ActionDelay = new int[]
                {
                    0, //none
                    0, //aiming
                    15000, //arming
                    0, //loading
                    1000 //fireing
                }; //en ms
        }

        public override bool AddToWorld()
        {
            SetGroundTarget(X, Y, Component.Keep.Z);
            return base.AddToWorld();
        }

        public override void DoDamage()
        {
            //todo remove ammo + spell in db and uncomment
            //m_spellHandler.StartSpell(player);
            base.DoDamage(); //anim mut be called after damage


            foreach (GameLiving npc in GetNPCsInRadius((ushort)OilSpell.Range))
            {
                if (npc != null && npc is GamePlayer)
                {
                    CastSpell(OilSpell, SiegeSpellLine, true);
                }
            }

            foreach (GameLiving player in GetPlayersInRadius((ushort)OilSpell.Range))
            {
                if (player != null && player is GamePlayer)
                {
                    CastSpell(OilSpell, SiegeSpellLine, true);
                }
            }
        } 

        private static Spell m_OilSpell;

        public static Spell OilSpell
        {
            get
            {
                if (m_OilSpell == null)
                {
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = false,
                        CastTime = 2,
                        ClientEffect = 2209, //2209? 5909? 7086? 7091?
                        Damage = 1550,
                        DamageType = (int)eDamageType.Heat,
                        Name = "Boiling Oil",
                        Radius = 260,
                        Range = 650,
                        SpellID = 50005,
                        Target = "Area",
                        Type = "SiegeDirectDamage"
                    };
                    m_OilSpell = new Spell(spell, 50);
                }
                return m_OilSpell;
            }
        }
    }
}

namespace DOL.GS.Spells
{

    /// <summary>
    /// 
    /// </summary>
    [SpellHandlerAttribute("SiegeDirectDamage")]
    public class SiegeDirectDamageSpellHandler : DirectDamageSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Calculates chance of spell getting resisted
        /// </summary>
        /// <param name="target">the target of the spell</param>
        /// <returns>chance that spell will be resisted for specific target</returns>
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override int CalculateToHitChance(GameLiving target)
        {
            return 100;
        }

        public override bool CasterIsAttacked(GameLiving attacker)
        {
            return false;
        }

        public override void CalculateDamageVariance(GameLiving target, out double min, out double max)
        {
            min = 600;
            max = 1500;
        }
        double armorAbsorb = 0;
        public override AttackData CalculateDamageToTarget(GameLiving target, double effectiveness)
        {
            AttackData ad = base.CalculateDamageToTarget(target, effectiveness);
            if (target is GamePlayer)
            {
                GamePlayer player = target as GamePlayer;
                

                //Mythical Beleaguered Reduction
                if (player.GetModified(eProperty.MythicalSiegeDamageReduction) > 0)
                {
                    ad.Damage -= (int)((double)(ad.Damage * player.GetModified(eProperty.MythicalSiegeDamageReduction) / 100 * 0.1)); //test
                }



                if (player is GamePlayer)
                {
                    ad.ArmorHitLocation = ((GamePlayer)player).CalculateArmorHitLocation(ad);

                    InventoryItem armor = null;
                    if (player.Inventory != null)


                        armor = player.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

                    armorAbsorb = player.GetArmorAbsorb(ad.ArmorHitLocation) * 10;

                    if (armorAbsorb == 1)
                        armorAbsorb = 1.5;
                    else if (armorAbsorb < 1)
                        armorAbsorb = 1;

                   //log.ErrorFormat("absorb = {0}", armorAbsorb);
                    //reduction by players Absorb
                    ad.Damage /= (int)armorAbsorb;


                  
                    //3000 spec
                    //ram protection
                    //lvl 0 50%
                    //lvl 1 60%
                    //lvl 2 70%
                    //lvl 3 80%
                    if (player.IsRiding && player.Steed is GameSiegeRam)
                    {
                        ad.Damage = (int)((double)ad.Damage * (1.0 - (50.0 + (double)player.Steed.Level * 5.0) / 100.0));
                    }
                }
                
            }
            return ad;
        }

        public override void SendDamageMessages(AttackData ad)
        {
            string modmessage = String.Empty;
            if (ad.Modifier > 0)
                modmessage = " (+" + ad.Modifier + ")";
            if (ad.Modifier < 0)
                modmessage = " (" + ad.Modifier + ")";
            if (ad.Modifier < -1000)
                modmessage = String.Empty;

            if (Caster is GameSiegeWeapon)
            {
                GameSiegeWeapon siege = (Caster as GameSiegeWeapon);
                if (siege.Owner != null)
                {
                    siege.Owner.Out.SendMessage(string.Format("You hit {0} for {1}{2} damage!", ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                }
            }
        }

        public override void DamageTarget(AttackData ad, bool showEffectAnimation, int attackResult, bool wasBlocked)
        {
            if (Caster is GameSiegeWeapon)
            {
                GameSiegeWeapon siege = (Caster as GameSiegeWeapon);
                if (siege.Owner != null)
                {
                    ad.Attacker = siege.Owner;

                    if (ad.Damage > 2500) //damage cap
                    {
                        ad.Damage = 2500;
                        //log.ErrorFormat("Damage Cap: {0}", ad.Damage);
                    }
                }
            }
            base.DamageTarget(ad, showEffectAnimation, attackResult, wasBlocked);
        }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            return true;
        }

        public override bool CheckEndCast(GameLiving target)
        {
            return true;
        }


        // constructor
        public SiegeDirectDamageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}
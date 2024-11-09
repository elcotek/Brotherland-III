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

using DOL.Events;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// GameMovingObject is a base class for boats and siege weapons.
    /// </summary>
    public class GameSiegeRam : GameSiegeWeapon
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GameSiegeRam()
            : base()
        {
            MeleeDamageType = eDamageType.Body;
            Name = "siege ram";
            Constitution = 10;
            Race = 200;
            AmmoType = 0x3B;
            this.Effect = 0x8A1;
            AmmoType = 0x26;
            this.Model = 0xA2A;//0xA28
            //TODO find all value for ram
            ActionDelay = new int[]
            {
                0,//none
				14000,//aiming
				7000,//arming
				0,//loading
				800,//fireing
			};//en ms
        }

        public override ushort Type()
        {
            return 0x9602;
        }

        public override int MAX_PASSENGERS
        {
            get
            {
                switch (Level)
                {
                    case 0:
                        return 2;
                    case 1:
                        return 6;
                    case 2:
                        return 8;
                    case 3:
                        return 12;
                }
                return Level * 3;
            }
        }

        public override int SLOT_OFFSET
        {
            get
            {
                return 1;
            }
        }

        public override void DoDamage()
        {
            if (Owner == null)
                return;
            GameObject target = (Owner.TargetObject as GameObject);

            if ((target is GameDoor == false && target is GameKeepDoor == false && target is GameKeepComponent == false) || target == this || target == null)
            {
                Owner.Out.SendMessage("You have to be first a door or a wall in target to start the siege ram!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            //todo good  distance check
            if (!this.IsWithinRadius(target, AttackRange))
            {
                Owner.Out.SendMessage("You are too far away to attack " + target.Name, eChatType.CT_System,
                                      eChatLoc.CL_SystemWindow);
                return;
            }

            if (Owner.TargetObject is GameKeepComponent && Owner.CurrentRegionID == 163 && (this as GameLiving).AllowedToRaid() == false && (Owner.TargetObject as GameKeepComponent).Keep.InCombat == false)
            {
                ((GamePlayer)Owner).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "AntiRaidMsg", Properties.RAID_MEMBER_COUNT + 1), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return;
            }

            if (Owner.TargetObject is GameKeepDoor && Owner.CurrentRegionID == 163 && (this as GameLiving).AllowedToRaid() == false && (Owner.TargetObject as GameKeepDoor).InCombat == false)
            {
                ((GamePlayer)Owner).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "AntiRaidMsg", Properties.RAID_MEMBER_COUNT + 1), eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return;
            }
            int damageAmount = RamDamage;



            if (Owner.TargetObject is GameKeepComponent)
            {
                damageAmount = ((damageAmount * ServerProperties.Properties.Siege_Ram_Damage) / Owner.TargetObject.Level);
            }
            
            //TODO: dps change by number
            target.TakeDamage(this, eDamageType.Crush, damageAmount, 0);
            Owner.Out.SendMessage("The Ram hits " + target.Name + " for " + damageAmount + " dmg!", eChatType.CT_YouHit,
                                  eChatLoc.CL_SystemWindow);
            Message.SystemToArea(this, GetName(0, false) + " hits " + target.GetName(0, true), eChatType.CT_OthersCombat,
                                 Owner);
            base.DoDamage();
        }

        public override bool RiderMount(GamePlayer rider, bool forced, int slot)
        {
            int exists = RiderArrayLocation(rider);
            if (exists != -1)
                return false;

            if (Riders[slot] != null)
                return false;

            Notify(GameNPCEvent.RiderMount, this, new RiderMountEventArgs(rider, this));
            Riders[slot] = rider;
            rider.Steed = this;
            UpdateRamStatus();
            return true;

        }


        public override bool RiderDismount(bool forced, GamePlayer player)
        {
            if (Riders.Length <= 0)
                return false;

            int slot = RiderArrayLocation(player);

            if (slot < 0)
            {
                return false;
            }
            Riders[slot] = null;

            Notify(GameNPCEvent.RiderDismount, this, new RiderDismountEventArgs(player, this));
            player.Steed = null;

            if (player != Owner)
            {
                // ReleaseControl();
                UpdateRamStatus();
                return true;
            }
            else if (player == Owner)
            {
                if (IsMoving)
                {
                    StopMoving();
                }

                ReleaseControl();
                UpdateRamStatus();

                return true;
            }

            return false;
        }
        public override void ReleaseControl()
        {
            base.ReleaseControl();

            if (Owner == null)
            {
                StopMove();
            }
        }

        public void UpdateRamStatus()
        {
            //speed of reload changed by number
            ActionDelay[1] = GetReloadDelay;
        }

        private int GetReloadDelay
        {
            get
            {
                //custom formula
                return 10000 + ((Level + 1) * 2000) - 10000 * (int)((double)CurrentRiders.Length / (double)MAX_PASSENGERS);
            }
        }

        private int RamDamage
        {
            get
            {
                return BaseRamDamage + (int)(((double)BaseRamDamage / 2.0) * (double)((double)CurrentRiders.Length / (double)MAX_PASSENGERS));
            }
        }

        private int BaseRamDamage
        {
            get
            {
                int damageAmount = 0;
                switch (Level)
                {
                    case 0:
                        damageAmount = 350;
                        break;
                    case 1:
                        damageAmount = 550;
                        break;
                    case 2:
                        damageAmount = 600;
                        break;
                    case 3:
                        damageAmount = 750;
                        break;
                }
                return damageAmount;
            }
        }

        public override int AttackRange
        {
            get
            {
                switch (Level)
                {
                    case 0: return 300;
                    case 1: return 400;
                    case 2:
                    case 3: return 500;
                    default: return 500;
                }
            }
        }
        public override void StartHealthRegeneration()
        {
            //don't regenerate health
        }
        public override short MaxSpeed
        {
            get
            {
                //custom formula
                double speed = (10.0 + (5.0 * Level) + 100.0 * CurrentRiders.Length / MAX_PASSENGERS);
                foreach (GamePlayer player in CurrentRiders)
                {
                    int specLevelBattlemaster = player.GetModifiedSpecLevel(Specs.Battlemaster);
                    if (specLevelBattlemaster > 0)
                    {
                        speed *= 1 + (specLevelBattlemaster / 100);
                    }
                    RealmAbilities.RAPropertyEnhancer ab = player.GetAbility<RealmAbilities.LifterAbility>();
                    if (ab != null)
                        speed *= 1 + (ab.Amount / 100);
                }
                return (short)speed;
            }
        }
    }
}

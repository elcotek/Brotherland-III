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
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using DOL.Language;
using System.Collections;

namespace DOL.GS
{
    /// <summary>
    /// GameMovingObject is a base class for boats and siege weapons.
    /// </summary>
    public class GameSiegeTrebuchet : GameSiegeWeapon
    {

        public GameSiegeTrebuchet()
            : base()
        {
            MeleeDamageType = eDamageType.Crush;
            Name = "trebuchet";
            Constitution = 90;
            AmmoType = 0x3A;
            EnableToMove = false;
            this.Model = 0xA2E;
            this.Effect = 0x89C;
            ActionDelay = new int[]
			{
				0,//none
				14000,//aiming
				12000,//arming
				0,//loading
				4500//fireing
			};//en ms
        }
        private const string REFUND_ITEM_WEAK = "refunded item";


        protected IList SelectTargets()
        {
            ArrayList list = new ArrayList(20);

            foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot(this.CurrentRegionID, GroundTarget.X, GroundTarget.Y, GroundTarget.Z, (ushort)150))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(Owner, player, true))
                {
                    list.Add(player);
                }
            }
            foreach (GameNPC npc in WorldMgr.GetNPCsCloseToSpot(this.CurrentRegionID, GroundTarget.X, GroundTarget.Y, GroundTarget.Z, (ushort)150))
            {
                if (GameServer.ServerRules.IsAllowedToAttack(Owner, npc, true))
                {
                    list.Add(npc);
                }
            }

            if (!list.Contains(this.TargetObject))
            {
                list.Add(this.TargetObject);
            }
            return list;
        }

        public override void DoDamage()
        {
            //			InventoryItem ammo = this.Ammo[AmmoSlot] as InventoryItem;
            //todo remove ammo + spell in db and uncomment
            //m_spellHandler.StartSpell(player);
            base.DoDamage();//anim mut be called after damage
            //if (GroundTarget == null) return;
            IList targets = SelectTargets();

            foreach (GameLiving living in targets)
            {

                if (Owner == null) continue;
                if (this is GameSiegeTimedTrebuchet && living.IsWithinRadius(Owner, DOL.GS.ServerProperties.Properties.Siege_Trebuchet_Maxrange) == false)
                {
                    Owner.Out.SendMessage("your target is to far away.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                    break;
                }

                if (Owner.TargetInView == false || Owner.TargetObject == this)
                {
                    //Owner.Out.SendMessage("your target is not in view.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                    (Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    break;
                }
                if ((Owner.TargetObject as GameLiving).IsStealthed == true)
                {
                    //Owner.Out.SendMessage("your target is not visible.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                    (Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsNotVisible"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    break;
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


                int damageAmount = Util.Random(200, 600);
                //Mythical Beleaguered Reduction
                if (Owner.TargetObject is GamePlayer && living.GetModified(eProperty.MythicalSiegeDamageReduction) > 0)
                {
                    damageAmount -= (damageAmount * living.GetModified(eProperty.MythicalSiegeDamageReduction) / 100);
                }
                if (Owner.TargetObject is GameKeepComponent)
                {
                    damageAmount = ((damageAmount * ServerProperties.Properties.Siege_Trebuchet_Damage) / Owner.TargetObject.Level);
                }
                living.TakeDamage(Owner, eDamageType.Crush, damageAmount, 0);
                Owner.Out.SendMessage("The " + this.Name + " hits " + living.Name + " for " + damageAmount + " damage!", eChatType.CT_YouHit,
                                      eChatLoc.CL_SystemWindow);
                foreach (GamePlayer player in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))


                    player.Out.SendCombatAnimation(this, living, 0x0000, 0x0000, 0x00, 0x00, 0x14, living.HealthPercent);
            }
            return;
        }

       
        public override bool ReceiveItem(GameLiving source, DOL.Database.InventoryItem item)
        {


            return base.ReceiveItem(source, item);
        }
    }
}
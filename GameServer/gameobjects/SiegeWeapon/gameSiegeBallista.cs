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


/*1,Ballista,1,ammo,0.46,1
2,Catapult,2,ammo,0.39,1
3,Trebuchet,2,ammo,1.03,1
4,Scorpion,1,ammo,0.22,1
5,Oil,3,ammo,0,1*/
/*ID,Textual Name,Load start,Load End,Fire Start,Fire End,Release Point
1,Ballista,30,60,0,30,12
2,Ram,30,60,0,30,12
3,Mangonel,30,200,0,30,12
4,trebuchet,97,180,0,96,
5,Ballista Low,11,30,0,10,
6,Ballista High,20,90,0,19,
7,Catapult W,40,111,0,40,
8,Catapult G,40,111,0,40,
9,Ram High,0,12,13,80,
10,Ram Mid,0,12,13,80,
11,Ram Low,0,12,13,80,*/
namespace DOL.GS
{
    /// <summary>
    /// GameMovingObject is a base class for boats and siege weapons.
    /// </summary>
    public class GameSiegeBallista : GameSiegeWeapon
    {
        public GameSiegeBallista()
            : base()
        {
            MeleeDamageType = eDamageType.Thrust;
            Name = "field ballista";
            Constitution = 50;
            AmmoType = 0x18;
            this.Model = 0x0A55;
            this.Effect = 0x089A;
            ActionDelay = new int[]{
				0,//none
				15000,//aiming
				10000,//arming
				0,//loading
				1100//fireing
			};//en ms
            /*SpellLine siegeWeaponSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.SiegeWeapon_Spells);
            IList spells = SkillBase.GetSpellList(siegeWeaponSpellLine.KeyName);
            if (spells != null)
            {
                foreach (Spell spell in spells)
                {
                    if (spell.ID == 2430) //TODO good id for balista
                    {
                        if(spell.Level <= Level)
                        {
                            m_spellHandler = ScriptMgr.CreateSpellHandler(this, spell, siegeWeaponSpellLine);
                        }
                        break;
                    }
                }
            }*/
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
                if (this is GameSiegeBallista && living.IsWithinRadius(Owner, DOL.GS.ServerProperties.Properties.Siege_Balista_Maxrange) == false)
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
                int damageAmount = Util.Random(200, 350);
                //Mythical Beleaguered Reduction
                if (Owner.TargetObject is GamePlayer && living.GetModified(eProperty.MythicalSiegeDamageReduction) > 0)
                {
                    damageAmount -= (damageAmount * living.GetModified(eProperty.MythicalSiegeDamageReduction) / 100);
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
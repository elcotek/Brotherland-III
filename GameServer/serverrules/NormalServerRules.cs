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
using System.Collections;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.Styles;
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.ServerRules
{
    /// <summary>
    /// Set of rules for "normal" server type.
    /// </summary>
    [ServerRules(eGameServerType.GST_Normal)]
    public class NormalServerRules : AbstractServerRules
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public override string RulesDescription()
        {
            return "standard Normal server rules";
        }

        /// <summary>
        /// Invoked on NPC death and deals out
        /// experience/realm points if needed
        /// </summary>
        /// <param name="killedNPC">npc that died</param>
        /// <param name="killer">killer</param>
        public override void OnNPCKilled(GameNPC killedNPC, GameObject killer)
        {
            base.OnNPCKilled(killedNPC, killer);
        }

        public override bool IsAllowedToAttack(GameLiving attacker, GameLiving defender, bool quiet)
        {
            if (!base.IsAllowedToAttack(attacker, defender, quiet))
                return false;

            // if controlled NPC - do checks for owner instead
            if (attacker is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)attacker).Brain as IControlledBrain;
                if (controlled != null)
                {
                    attacker = controlled.GetLivingOwner();
                    quiet = true; // silence all attacks by controlled npc
                }
            }
            if (defender is GameNPC)
            {
                IControlledBrain controlled = ((GameNPC)defender).Brain as IControlledBrain;
                if (controlled != null)
                    defender = controlled.GetLivingOwner();
            }




            //"You can't attack yourself!"
            if (attacker == defender)
            {
                if (quiet == false)

                    //MessageToLiving(attacker, "You can't attack yourself!");
                    (attacker as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((attacker as GamePlayer).Client.Account.Language, "SpellHandler.YouCantAttackYourself"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            //Don't allow attacks on same realm members on Normal Servers
            if (attacker.Realm == defender.Realm && !(attacker is GamePlayer && ((GamePlayer)attacker).DuelTarget == defender))
            {
                // allow confused mobs to attack same realm
                if (attacker is GameNPC && (attacker as GameNPC).IsConfused)
                    return true;

                if (attacker.Realm == 0)
                {
                    return FactionMgr.CanLivingAttack(attacker, defender);
                }

                if (quiet == false) MessageToLiving(attacker, "You can't attack a member of your realm!");
                return false;
            }

            return true;
        }


        public override bool IsSameRealm(GameObject source, GameObject target, bool quiet)
        {
            if (source == null || target == null)
                return false;
            if (source.ObjectState != GameObject.eObjectState.Active || target.ObjectState != GameObject.eObjectState.Active)
                return false;


            // if controlled NPC - do checks for owner instead
            if (source is GameNPC)
            {
                if ((source as GameNPC).Brain is IControlledBrain)
                {
                    IControlledBrain controlled = ((GameNPC)source).Brain as IControlledBrain;
                    if (controlled != null)
                    {
                        source = controlled.GetLivingOwner();
                        quiet = true; // silence all attacks by controlled npc
                    }
                }
            }
            if (target is GameNPC)
            {
                if ((target as GameNPC).Brain is IControlledBrain)
                {
                    IControlledBrain controlled = ((GameNPC)target).Brain as IControlledBrain;
                    if (controlled != null)
                    {
                        target = controlled.GetPlayerOwner();
                    }
                }
            }

            if (source.Realm == target.Realm)
                return true;

            if (source == target)
                return true;

            // clients with priv level > 1 are considered friendly by anyone
            if (target is GamePlayer && ((GamePlayer)target).Client.Account.PrivLevel > 1) return true;
            // checking as a gm, targets are considered friendly
            if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;

          

            //Peace flag NPCs are same realm
            if (target is GameNPC)
                if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (source is GameNPC)
                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;



            if (source.Realm != target.Realm)
            {
                if (quiet == false)
                    MessageToLiving(source, target.GetName(0, true) + " is not a member of your realm!");
                return false;
            }
            return true;
        } 

        public override bool IsAllowedCharsInAllRealms(GameClient client)
        {
            if (client.Account.PrivLevel > 1)
                return true;


            IList<Account> PlayerAccountName = GameServer.Database.SelectObjects<Account>("`Name` = @Name", new QueryParameter("@Name", client.Account.Name));

            foreach (Account db in PlayerAccountName)
            {

                if (db.LastLoginRealm != 0 && db.PrivLevel == 1 && db != null && db.IsOtherRealmAllowed == false && ServerProperties.Properties.LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES > 0)
                {
                    if (db.LoginWaitTime > DateTime.Now)
                    {
                        if (db.Realm == db.LastLoginRealm)
                        {
                            return false;
                        }
                        else
                            return true;
                    }
                    else
                    {
                        if (ServerProperties.Properties.ALLOW_ALL_REALMS)
                        {
                            return true;
                        }
                    }
                }



            }
            return false;
        }

        public override bool IsAllowedToGroup(GamePlayer source, GamePlayer target, bool quiet)
        {
            if (source == null || target == null) return false;

            if (source.Realm != target.Realm)
            {
                if (quiet == false) MessageToLiving(source, "You can't invite a player of another realm.");
                return false;
            }
            return true;
        }


        public override bool IsAllowedToJoinGuild(GamePlayer source, Guild guild)
        {
            if (source == null)
                return false;

            if (ServerProperties.Properties.ALLOW_CROSS_REALM_GUILDS == false && guild.Realm != eRealm.None && source.Realm != guild.Realm)
            {
                return false;
            }

            return true;
        }

        public override bool IsAllowedToTrade(GameLiving source, GameLiving target, bool quiet)
        {
            if (source == null || target == null) return false;

            // clients with priv level > 1 are allowed to trade with anyone
            if (source is GamePlayer && target is GamePlayer)
            {
                if ((source as GamePlayer).Client.Account.PrivLevel > 1 || (target as GamePlayer).Client.Account.PrivLevel > 1)
                    return true;
            }

            //Peace flag NPCs can trade with everyone
            if (target is GameNPC)
                if ((((GameNPC)target).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (source is GameNPC)
                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (source.Realm != target.Realm)
            {
                if (quiet == false) MessageToLiving(source, "You can't trade with enemy realm!");
                return false;
            }
            return true;
        }

        public override bool IsAllowedToUnderstand(GameLiving source, GamePlayer target)
        {
            if (source == null || target == null) return false;

            // clients with priv level > 1 are allowed to talk and hear anyone
            if (source is GamePlayer && ((GamePlayer)source).Client.Account.PrivLevel > 1) return true;
            if (target.Client.Account.PrivLevel > 1) return true;

            //Peace flag NPCs can be understood by everyone

            if (source is GameNPC)
                if ((((GameNPC)source).Flags & GameNPC.eFlags.PEACE) != 0)
                    return true;

            if (source.Realm > 0 && source.Realm != target.Realm) return false;
            return true;
        }

        /// <summary>
        /// Is player allowed to bind
        /// </summary>
        /// <param name="player"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public override bool IsAllowedToBind(GamePlayer player, BindPoint point)
        {
            if (point.Realm == 0) return true;
            return player.Realm == (eRealm)point.Realm;
        }

        /// <summary>
        /// Is player allowed to make the item
        /// </summary>
        /// <param name="player"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool IsAllowedToCraft(GamePlayer player, ItemTemplate item)
        {
            if ((eRealm)item.Realm == 0) // elcotek: items with realm 0 is now craftebel!
                return (eRealm)item.Realm == (eRealm)item.Realm;
            else
                return player.Realm == (eRealm)item.Realm;
        }

        /// <summary>
        /// Translates object type to compatible object types based on server type
        /// </summary>
        /// <param name="objectType">The object type</param>
        /// <returns>An array of compatible object types</returns>
        protected override eObjectType[] GetCompatibleObjectTypes(eObjectType objectType)
        {
            if (m_compatibleObjectTypes == null)
            {
                m_compatibleObjectTypes = new Hashtable();
                m_compatibleObjectTypes[(int)eObjectType.Staff] = new eObjectType[] { eObjectType.Staff };
                m_compatibleObjectTypes[(int)eObjectType.Fired] = new eObjectType[] { eObjectType.Fired };
                m_compatibleObjectTypes[(int)eObjectType.MaulerStaff] = new eObjectType[] { eObjectType.MaulerStaff };
                m_compatibleObjectTypes[(int)eObjectType.FistWraps] = new eObjectType[] { eObjectType.FistWraps };

                //alb
                m_compatibleObjectTypes[(int)eObjectType.CrushingWeapon] = new eObjectType[] { eObjectType.CrushingWeapon };
                m_compatibleObjectTypes[(int)eObjectType.SlashingWeapon] = new eObjectType[] { eObjectType.SlashingWeapon };
                m_compatibleObjectTypes[(int)eObjectType.ThrustWeapon] = new eObjectType[] { eObjectType.ThrustWeapon };
                m_compatibleObjectTypes[(int)eObjectType.TwoHandedWeapon] = new eObjectType[] { eObjectType.TwoHandedWeapon };
                m_compatibleObjectTypes[(int)eObjectType.PolearmWeapon] = new eObjectType[] { eObjectType.PolearmWeapon };
                m_compatibleObjectTypes[(int)eObjectType.Flexible] = new eObjectType[] { eObjectType.Flexible };
                m_compatibleObjectTypes[(int)eObjectType.Longbow] = new eObjectType[] { eObjectType.Longbow };
                m_compatibleObjectTypes[(int)eObjectType.Crossbow] = new eObjectType[] { eObjectType.Crossbow };
                //TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;                                         

                //mid
                m_compatibleObjectTypes[(int)eObjectType.Hammer] = new eObjectType[] { eObjectType.Hammer };
                m_compatibleObjectTypes[(int)eObjectType.Sword] = new eObjectType[] { eObjectType.Sword };
                m_compatibleObjectTypes[(int)eObjectType.LeftAxe] = new eObjectType[] { eObjectType.LeftAxe };
                m_compatibleObjectTypes[(int)eObjectType.Axe] = new eObjectType[] { eObjectType.Axe };
                m_compatibleObjectTypes[(int)eObjectType.HandToHand] = new eObjectType[] { eObjectType.HandToHand };
                m_compatibleObjectTypes[(int)eObjectType.Spear] = new eObjectType[] { eObjectType.Spear };
                m_compatibleObjectTypes[(int)eObjectType.CompositeBow] = new eObjectType[] { eObjectType.CompositeBow };
                m_compatibleObjectTypes[(int)eObjectType.Thrown] = new eObjectType[] { eObjectType.Thrown };

                //hib
                m_compatibleObjectTypes[(int)eObjectType.Blunt] = new eObjectType[] { eObjectType.Blunt };
                m_compatibleObjectTypes[(int)eObjectType.Blades] = new eObjectType[] { eObjectType.Blades };
                m_compatibleObjectTypes[(int)eObjectType.Piercing] = new eObjectType[] { eObjectType.Piercing };
                m_compatibleObjectTypes[(int)eObjectType.LargeWeapons] = new eObjectType[] { eObjectType.LargeWeapons };
                m_compatibleObjectTypes[(int)eObjectType.CelticSpear] = new eObjectType[] { eObjectType.CelticSpear };
                m_compatibleObjectTypes[(int)eObjectType.Scythe] = new eObjectType[] { eObjectType.Scythe };
                m_compatibleObjectTypes[(int)eObjectType.RecurvedBow] = new eObjectType[] { eObjectType.RecurvedBow };

                m_compatibleObjectTypes[(int)eObjectType.Shield] = new eObjectType[] { eObjectType.Shield };
                m_compatibleObjectTypes[(int)eObjectType.Poison] = new eObjectType[] { eObjectType.Poison };
                //TODO: case 45: abilityCheck = Abilities.instruments; break;
            }

            eObjectType[] res = (eObjectType[])m_compatibleObjectTypes[(int)objectType];
            if (res == null)
                return new eObjectType[0];
            return res;
        }

        /// <summary>
        /// Gets the player name based on server type
        /// </summary>
        /// <param name="source">The "looking" player</param>
        /// <param name="target">The considered player</param>
        /// <returns>The name of the target</returns>
        public override string GetPlayerName(GamePlayer source, GamePlayer target)
        {
            if (IsSameRealm(source, target, true))
                return target.Name;
            return target.RaceName;
        }

        /// <summary>
        /// Gets the player last name based on server type
        /// </summary>
        /// <param name="source">The "looking" player</param>
        /// <param name="target">The considered player</param>
        /// <returns>The last name of the target</returns>
        public override string GetPlayerLastName(GamePlayer source, GamePlayer target)
        {
            if (IsSameRealm(source, target, true))
                return target.LastName;
            return target.RealmTitle;
        }

        /// <summary>
        /// Gets the player guild name based on server type
        /// </summary>
        /// <param name="source">The "looking" player</param>
        /// <param name="target">The considered player</param>
        /// <returns>The guild name of the target</returns>
        public override string GetPlayerGuildName(GamePlayer source, GamePlayer target)
        {
            if (IsSameRealm(source, target, true))
                return target.GuildName;
            return string.Empty;
        }

        /// <summary>
        /// Reset the keep with special server rules handling
        /// </summary>
        /// <param name="lord">The lord that was killed</param>
        /// <param name="killer">The lord's killer</param>
        public override void ResetKeep(GuardLord lord, GameObject killer)
        {
            base.ResetKeep(lord, killer);
            lord.Component.Keep.Reset((eRealm)killer.Realm);
        }
    }
}

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

using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.Effects;
using DOL.Language;

namespace DOL.GS.Spells
{
    /// <summary>
    /// The spell used for the Personal Bind Recall Stone.
    /// </summary>
    /// <author>Aredhel</author>
    [SpellHandlerAttribute("GatewayPersonalHouse")]
    public class GatewayPersonalHouse : SpellHandler
    {
        public GatewayPersonalHouse(GameLiving caster, Spell spell, SpellLine spellLine)
            : base(caster, spell, spellLine) { }


        /// <summary>
        /// Can this spell be queued with other spells?
        /// </summary>
        public override bool CanQueue
        {
            get { return false; }
        }

        public virtual bool GetPlayerHouse(GamePlayer player)
        {
            House house = HouseMgr.GetHouseByPlayer(player);

            if (house != null)
            {

                if (house.HouseNumber >= 1 && house.HouseNumber <= 1383 && player.Realm == eRealm.Albion)
                {
                    return true;
                }

                if (house.HouseNumber >= 1601 && house.HouseNumber <= 2574 && player.Realm == eRealm.Midgard)
                {
                    return true;
                }

                if (house.HouseNumber >= 3200 && house.HouseNumber <= 4399 && player.Realm == eRealm.Hibernia)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Whether this spell can be cast on the selected target at all.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
                return false;

            if (player.CurrentRegion.IsRvR)
            {
                // Actual live message is: You can't use that item!
               //player.Out.SendMessage("You can't use that here!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "YouCantUseThatHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.IsMoving)
            {
                player.Out.SendMessage("You must be standing still to use this item!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.InCombat || GameRelic.IsPlayerCarryingRelic(player))
            {
                player.Out.SendMessage("You have been in combat recently and cannot use this item!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
                return false;
            }
            // Another special case is personal house, as there is no location
            // that will work for every player.
            if (!(player.DBCharacter.BindHouseRegion > 0))
            {
                player.Out.SendMessage("Sorry, you haven't set any house bind point yet.", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
                return false;
            }
            

          return true;
        }


        /// <summary>
        /// Always a constant casting time
        /// </summary>
        /// <returns></returns>
        public override int CalculateCastingTime()
        {
            return m_spell.CastTime;
        }


        /// <summary>
        /// Apply the effect.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
                return;

            if (player.InCombat || GameRelic.IsPlayerCarryingRelic(player) || player.IsMoving)
                return;

            SendEffectAnimation(player, 0, false, 1);

            UniPortalEffect effect = new UniPortalEffect(this, 1000);
            effect.Start(player);

                      
          
                House house = HouseMgr.GetHouseByPlayer(player);

            if (house == null || GetPlayerHouse(player) == false)
            {
                player.Out.SendMessage("Sorry, there was no house found!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
            }
            else
            {
                IGameLocation location = house.OutdoorJumpPoint(player);
                Teleport teleport = new Teleport();
                teleport.TeleportID = "personal";
                teleport.Realm = (int)player.Realm;
                teleport.RegionID = location.RegionID;
                teleport.X = location.X;
                teleport.Y = location.Y;
                teleport.Z = location.Z;
                teleport.Heading = location.Heading;
                OnDestinationPicked(player, teleport);
                return;
            }
            
            //DOLCharacters character = player.DBCharacter;
            //player.MoveTo((ushort)character.BindRegion, character.BindXpos, character.BindYpos, character.BindZpos, (ushort)character.BindHeading);
        }


        /// <summary>
        /// Player has picked a destination.
        /// Override if you need the teleporter to say something to the player
        /// before porting him.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected virtual void OnDestinationPicked(GamePlayer player, Teleport destination)
        {
            Region region = WorldMgr.GetRegion((ushort)destination.RegionID);

            if (region == null || region.IsDisabled)
            {
                player.Out.SendMessage("This destination is not available.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            OnTeleport(player, destination);
        }


        public override void CasterMoves()
        {
            InterruptCasting();
            MessageToCaster("You move and interrupt your spellcast!", DOL.GS.PacketHandler.eChatType.CT_Important);
        }


        public override void InterruptCasting()
        {
            m_startReuseTimer = false;
            base.InterruptCasting();
        }

        public override IList<string> DelveInfo(GamePlayer player)
        {

            {
                var list = new List<string>();
                list.Add(string.Format("  {0}", Spell.Description));

                return list;
            }
        }
        /// <summary>
        /// Teleport the player to the designated coordinates. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="destination"></param>
        protected virtual void OnTeleport(GamePlayer player, Teleport destination)
        {
            if (player.InCombat == false && GameRelic.IsPlayerCarryingRelic(player) == false)
            {
                player.LeaveHouse();
                GameLocation currentLocation = new GameLocation("TeleportStart", player.CurrentRegionID, player.X, player.Y, player.Z);
                player.MoveTo((ushort)destination.RegionID, destination.X, destination.Y, destination.Z, (ushort)destination.Heading);
                GameServer.ServerRules.OnPlayerTeleport(player, currentLocation, destination);
            }
        }
    }
}

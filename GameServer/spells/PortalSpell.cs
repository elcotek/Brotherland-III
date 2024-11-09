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

using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System.Reflection;

namespace DOL.GS.Spells
{
    /// <summary>
    /// The spell used by classic teleporters.
    /// </summary>
    /// <author>Aredhel</author>
    [SpellHandlerAttribute("PortalSpell")]
    public class PortalSpell : SpellHandler
    {

        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        ///Albion

        /// <summary>
        /// Port Region Destination
        /// </summary>
        protected static ushort m_aregionID;

        /// <summary>
        /// Port Region Destination
        /// </summary>
        public static ushort ARegionID
        {
            get { return m_aregionID; }
            set { m_aregionID = value; }
        }


        /// <summary>
        /// Port X Destination
        /// </summary>
        protected static int m_axPos;



        /// <summary>
        /// Port X Destination
        /// </summary>
        public static int AXPos
        {
            get { return m_axPos; }
            set { m_axPos = value; }
        }

        /// <summary>
        /// Port Y Destination
        /// </summary>
        protected static int m_ayPOS;

        /// <summary>
        /// Port Y Destination
        /// </summary>
        public static int AYPOS
        {
            get { return m_ayPOS; }
            set { m_ayPOS = value; }
        }

        /// <summary>
        /// Port Z Destination
        /// </summary>
        protected static int m_azPOS;

        /// <summary>
        /// Port Z Destination
        /// </summary>
        public static int AZPOS
        {
            get { return m_azPOS; }
            set { m_azPOS = value; }
        }

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        protected static ushort m_aheadingPos;

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        public static ushort AHeadingPos
        {
            get { return m_aheadingPos; }
            set { m_aheadingPos = value; }
        }



        //Midgard




        /// <summary>
        /// Port Region Destination
        /// </summary>
        protected static ushort m_mregionID;

        /// <summary>
        /// Port Region Destination
        /// </summary>
        public static ushort MRegionID
        {
            get { return m_mregionID; }
            set { m_mregionID = value; }
        }


        /// <summary>
        /// Port X Destination
        /// </summary>
        protected static int m_mxPos;



        /// <summary>
        /// Port X Destination
        /// </summary>
        public static int MXPos
        {
            get { return m_mxPos; }
            set { m_mxPos = value; }
        }

        /// <summary>
        /// Port Y Destination
        /// </summary>
        protected static int m_myPOS;

        /// <summary>
        /// Port Y Destination
        /// </summary>
        public static int MYPOS
        {
            get { return m_myPOS; }
            set { m_myPOS = value; }
        }

        /// <summary>
        /// Port Z Destination
        /// </summary>
        protected static int m_mzPOS;

        /// <summary>
        /// Port Z Destination
        /// </summary>
        public static int MZPOS
        {
            get { return m_mzPOS; }
            set { m_mzPOS = value; }
        }

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        protected static ushort m_mheadingPos;

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        public static ushort MHeadingPos
        {
            get { return m_mheadingPos; }
            set { m_mheadingPos = value; }
        }

        //Hibernia


        /// <summary>
        /// Port Region Destination
        /// </summary>
        protected static eRealm m_realm;

        /// <summary>
        /// Port Region Destination
        /// </summary>
        public static eRealm Realm
        {
            get { return m_realm; }
            set { m_realm = value; }
        }

        /// <summary>
        /// Port Region Destination
        /// </summary>
        protected static ushort m_hregionID;

        /// <summary>
        /// Port Region Destination
        /// </summary>
        public static ushort HRegionID
        {
            get { return m_hregionID; }
            set { m_hregionID = value; }
        }


        /// <summary>
        /// Port X Destination
        /// </summary>
        protected static int m_hxPos;



        /// <summary>
        /// Port X Destination
        /// </summary>
        public static int HXPos
        {
            get { return m_hxPos; }
            set { m_hxPos = value; }
        }

        /// <summary>
        /// Port Y Destination
        /// </summary>
        protected static int m_hyPOS;

        /// <summary>
        /// Port Y Destination
        /// </summary>
        public static int HYPOS
        {
            get { return m_hyPOS; }
            set { m_hyPOS = value; }
        }

        /// <summary>
        /// Port Z Destination
        /// </summary>
        protected static int m_hzPOS;

        /// <summary>
        /// Port Z Destination
        /// </summary>
        public static int HZPOS
        {
            get { return m_hzPOS; }
            set { m_hzPOS = value; }
        }

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        protected static ushort m_hheadingPos;

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        public static ushort HHeadingPos
        {
            get { return m_hheadingPos; }
            set { m_hheadingPos = value; }
        }



        //All Realms



        /// <summary>
        /// Port X Destination
        /// </summary>
        protected static int m_xPos;


        /// <summary>
        /// Port X Destination
        /// </summary>
        public static int XPos
        {
            get { return m_xPos; }
            set { m_xPos = value; }
        }

        /// <summary>
        /// Port Region Destination
        /// </summary>
        protected static ushort m_regionID;

        /// <summary>
        /// Port Region Destination
        /// </summary>
        public static ushort RegionID
        {
            get { return m_regionID; }
            set { m_regionID = value; }
        }

        /// <summary>
        /// Port Y Destination
        /// </summary>
        protected static int m_yPOS;

        /// <summary>
        /// Port Y Destination
        /// </summary>
        public static int YPOS
        {
            get { return m_yPOS; }
            set { m_yPOS = value; }
        }


        /// <summary>
        /// Port Z Destination
        /// </summary>
        protected static int m_zPOS;



        /// <summary>
        /// Port Z Destination
        /// </summary>
        public static int ZPOS
        {
            get { return m_zPOS; }
            set { m_zPOS = value; }
        }

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        protected static ushort m_headingPos;

        /// <summary>
        /// Port Heading Destination
        /// </summary>
        public static ushort HeadingPos
        {
            get { return m_headingPos; }
            set { m_headingPos = value; }
        }

      

        /// <summary>
        /// Destination setup
        /// </summary>
        /// <param name="region"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        public static bool SetDestinationAlb(ushort region = 0, int x = 0, int y = 0, int z = 0, ushort heading = 0, eRealm realm = 0)
        {
           
                ARegionID = region;
                AXPos = x;
                AYPOS = y;
                AZPOS = z;
                AHeadingPos = heading;
                Realm = realm;

            return false;
        }
        /// <summary>
        /// Destination setup
        /// </summary>
        /// <param name="region"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        public static bool SetDestinationMid(ushort region = 0, int x = 0, int y = 0, int z = 0, ushort heading = 0, eRealm realm = 0)
        {
           
                MRegionID = region;
                MXPos = x;
                MYPOS = y;
                MZPOS = z;
                MHeadingPos = heading;
                Realm = realm;

            return false;
        }
        /// <summary>
        /// Destination setup
        /// </summary>
        /// <param name="region"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        public static bool SetDestinationHib(ushort region = 0, int x = 0, int y = 0, int z = 0, ushort heading = 0, eRealm realm = 0)
        {
                HRegionID = region;
                HXPos = x;
                HYPOS = y;
                HZPOS = z;
                HHeadingPos = heading;
                Realm = realm;

            return false;
        }
        /// <summary>
        /// Destination setup
        /// </summary>
        /// <param name="region"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="heading"></param>
        /// <returns></returns>
        public static bool SetDestination(ushort region = 0, int x = 0, int y = 0, int z = 0, ushort heading = 0)
        {
            
                RegionID = region;
                XPos = x;
                YPOS = y;
                ZPOS = z;
                HeadingPos = heading;
              
          

            return false;
        }

        public PortalSpell(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        /// <summary>
        /// Whether this spell can be cast on the selected target at all.
        /// </summary>
        /// <param name="selectedTarget"></param>
        /// <returns></returns>
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!base.CheckBeginCast(selectedTarget))
                return false;
            return (selectedTarget is GameLiving);
        }

        /// <summary>
        /// Apply the effect.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {



            if (target == null)
                return;


            if (target is GamePlayer && ((target as GamePlayer).InCombat || GameRelic.IsPlayerCarryingRelic((target as GamePlayer))))
            {
                (target as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((target as GamePlayer).Client.Account.Language, "GamePlayer.UseSlot.CantUseInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            SendEffectAnimation(target, 0, false, 1);

            UniPortalEffect effect = new UniPortalEffect(this, 1000);
            effect.Start(target);

            if (target is GamePlayer)
            {
                (target as GamePlayer).LeaveHouse();
            }



            switch (Realm)
            {
                case eRealm.Albion:
                    {
                        if (Realm == target.Realm)
                        {
                            target.MoveTo(ARegionID, m_axPos, m_ayPOS, m_azPOS, m_aheadingPos);


                        }
                        else
                        if (target is GamePlayer)
                        {
                            (target as GamePlayer).Out.SendMessage("I'm currently too busy with other ports, so try again.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                case eRealm.Midgard:
                    {
                        if (Realm == target.Realm)
                        {
                            target.MoveTo(MRegionID, m_mxPos, m_myPOS, m_mzPOS, m_mheadingPos);
                          
                        }
                        else
                        if (target is GamePlayer)
                        {
                            (target as GamePlayer).Out.SendMessage("I'm currently too busy with other ports, so try again.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                case eRealm.Hibernia:
                    {
                        if (Realm == target.Realm)
                        {
                            target.MoveTo(HRegionID, m_hxPos, m_hyPOS, m_hzPOS, m_hheadingPos);
                            
                        }
                        else
                        if (target is GamePlayer)
                        {
                            (target as GamePlayer).Out.SendMessage("I'm currently too busy with other ports, so try again.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    }
                case eRealm.None:
                    {
                       
                            target.MoveTo(RegionID, m_xPos, m_yPOS, m_zPOS, m_headingPos);
                           
                     
                        break;
                    }
            }
        }
    }
}

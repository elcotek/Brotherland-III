﻿/*
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

//Instance devised by Dinberg
//     - there will probably be questions, direct them to dinberg_darktouch@hotmail.co.uk ;)
using DOL.Database;
using System;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// The Instance is an implementation of BaseInstance that contains additional functionality to load
    /// a template from InstanceXElement.
    /// </summary>
    public class Instance : BaseInstance
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates an instance object. This shouldn't be used directly - Please use WorldMgr.CreateInstance
        /// to create an instance.
        /// </summary>
        public Instance(ushort ID, GameTimer.TimeManager time, RegionData data) : base(ID, time, data)
        {
        }

        ~Instance()
        {
            log.Debug("Instance destructor called for " + Description);
        }

        #region Entrance

        protected GameLocation m_entranceLocation = null;

        /// <summary>
        /// Returns the entrance location into this instance.
        /// </summary>
        public GameLocation InstanceEntranceLocation
        { get { return m_entranceLocation; } }

        #endregion

        #region LoadFromDatabase

        /// <summary>
        /// Loads elements relating to the given instance keyname from the database and populates the instance.
        /// </summary>
        /// <param name="instanceName"></param>
        public virtual void LoadFromDatabase(string instanceName)
        {
            var objects = GameServer.Database.SelectObjects<DBInstanceXElement>("`InstanceID` = @InstanceID", new QueryParameter("@InstanceID", instanceName));

            if (objects.Count == 0)
                return;

            int count = 0;

            //Now we have a list of DBElements, lets create the various entries
            //associated with them and populate the instance.
            foreach (DBInstanceXElement entry in objects)
            {
                if (entry == null)
                    continue; //an odd error, but experience knows best.

                GameObject obj = null;
                string theType = "DOL.GS.GameNPC";

                //Switch the classtype to see what we are making.
                switch (entry.ClassType)
                {
                    case "entrance":
                        {
                            //create the entrance, then move to the next.
                            m_entranceLocation = new GameLocation(instanceName + "entranceRegion" + ID, ID, entry.X, entry.Y, entry.Z, entry.Heading);
                            //move to the next entry, nothing more to do here...
                            continue;
                        }
                    case "region": continue; //This is used to save the regionID as NPCTemplate.
                    case "DOL.GS.GameNPC": break;
                    default: theType = entry.ClassType; break;
                }

                //Now we have the classtype to create, create it thus!
                //This is required to ensure we check scripts for the space aswell, such as quests!
                foreach (Assembly asm in ScriptMgr.GameServerScripts)
                {
                    obj = (GameObject)(asm.CreateInstance(theType, false));
                    if (obj != null)
                        break;
                }


                if (obj == null)
                    continue;


                //We now have an object that isnt null. Lets place it at the location, in this region.

                obj.X = entry.X;
                obj.Y = entry.Y;
                obj.Z = entry.Z;
                obj.Heading = entry.Heading;
                obj.CurrentRegionID = ID;
              
                //If its an npc, load from the npc template about now.
                //By default, we ignore npctemplate if its set to 0.
                if ((GameNPC)obj != null && !Util.IsEmpty(entry.NPCTemplate, true))
                {
                    var listTemplate = entry.NPCTemplate.SplitCSV(true);
                   
                    if (int.TryParse(listTemplate[Util.Random(listTemplate.Count - 1)], out int template) && template > 0)
                    {
                        INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(template);
                        //we only want to load the template if one actually exists, or there could be trouble!
                        if (npcTemplate != null)
                        {
                            ((GameNPC)obj).LoadTemplate(npcTemplate);
                            
                        }
                    }
                }
                //Finally, add it to the world!
                obj.AddToWorld();

                //Keep track of numbers.
                count++;
            }

            log.Info("Successfully loaded a db entry to " + Description + " - Region ID " + ID + ". Loaded Entities: " + count);
        }

        #endregion


        /// <summary>
        /// This method returns an int representative of an average level for the instance.
        /// Instances do not scale with level by default, but specific instances like TaskDungeonMission can
        /// use this for an accurate representation of level.
        /// </summary>
        /// <returns></returns>
        public int GetInstanceLevel()
        {
            if (Objects == null)
                return 0;
            double membercount = 0;
            double pLevel = 0; 
            //double level = 0;
           // double count = 0;
            foreach (GameObject obj in Objects)
            {
                if (obj == null)
                    continue;

                if (!(obj is GamePlayer player))
                    continue;

                if (player.Group != null)
                    membercount = (double)player.Group.GetPlayersInTheGroup().Count;

                //The hightest Level deside the Dungeon level
                if (player.Level > pLevel)
                {
                    pLevel = player.Level;
                }
               
              
            }
            double soloLevel = ((pLevel * 10.0 / 100));//10% level increase
            double groupLevel = ((pLevel * 10.0 * membercount / 100)); //level increase * member count

            //log.ErrorFormat("soloLevel: {0} groupLevel: {1} membercount: {2}", soloLevel, groupLevel, membercount);
            pLevel += Math.Max(soloLevel, groupLevel); //double needed needed for lower levels...
            //level = Math.Max(1, (level * membercount / 100)); //double needed needed for lower levels...

            //level += level;
            return (int)pLevel;
        }
    }
}

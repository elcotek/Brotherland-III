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
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS.Keeps
{
    /// <summary>
    /// Class to manage the guards Positions
    /// </summary>
    public class PositionMgr
	{
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Gets the most usable position directly from the database
        /// </summary>
        /// <param name="guard">The guard object</param>
        /// <returns>The position object</returns>
        public static DBKeepPosition GetUsablePosition(GameKeepGuard guard)
		{
            //return GameServer.Database.SelectObject<DBKeepPosition>("ClassType != 'DOL.GS.Keeps.Banner' and TemplateID = '" + GameServer.Database.Escape(guard.TemplateID) + "' and ComponentSkin = '" + guard.Component.Skin + "' and Height <= " + guard.Component.Height + " order by Height desc limit 0,1") as DBKeepPosition;
            return GameServer.Database.SelectObjects<DBKeepPosition>("`ClassType` != @ClassType AND `TemplateID` = @TemplateID AND `ComponentSkin` = @ComponentSkin AND `Height` <= @Height",
                                                                     new[] { new QueryParameter("@ClassType", "DOL.GS.Keeps.Banner"), new QueryParameter("@TemplateID", guard.TemplateID), new QueryParameter("@ComponentSkin", guard.Component.Skin), new QueryParameter("@Height", guard.Component.Height) })
                .OrderByDescending(it => it.Height).FirstOrDefault();
        }
        /// <summary>
        /// Gets the most usuable position for a banner directly from the database
        /// </summary>
        /// <param name="b">The banner object</param>
        /// <returns>The position object</returns>
        public static DBKeepPosition GetUsablePosition(GameKeepBanner b)
        {
            //return GameServer.Database.SelectObject<DBKeepPosition>("ClassType = 'DOL.GS.Keeps.Banner' and TemplateID = '" + GameServer.Database.Escape(b.TemplateID) + "' and ComponentSkin = '" + b.Component.Skin + "' and Height <= " + b.Component.Height + " order by Height desc limit 0,1") as DBKeepPosition;
            return GameServer.Database.SelectObjects<DBKeepPosition>("`ClassType` = @ClassType AND `TemplateID` = @TemplateID AND `ComponentSkin` = @ComponentSkin AND `Height` <= @Height",
                                                                      new[] { new QueryParameter("@ClassType", "DOL.GS.Keeps.Banner"), new QueryParameter("@TemplateID", b.TemplateID), new QueryParameter("@ComponentSkin", b.Component.Skin), new QueryParameter("@Height", b.Component.Height) })
                 .OrderByDescending(it => it.Height).FirstOrDefault();
        }

		/// <summary>
		/// Gets the position at the exact entry from the database
		/// </summary>
		/// <param name="guard">The guard object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition GetPosition(GameKeepGuard guard)
		{
            //return GameServer.Database.SelectObject<DBKeepPosition>("TemplateID = '" + GameServer.Database.Escape(guard.TemplateID) + "' and ComponentSkin = '" + guard.Component.Skin + "' and Height = " + guard.Component.Height) as DBKeepPosition;
            return GameServer.Database.SelectObjects<DBKeepPosition>("`TemplateID` = @TemplateID AND `ComponentSkin` = @ComponentSkin AND `Height` <= @Height",
                                                                        new[] { new QueryParameter("@TemplateID", guard.TemplateID), new QueryParameter("@ComponentSkin", guard.Component.Skin), new QueryParameter("@Height", guard.Component.Height) }).FirstOrDefault();
        }


        public static void LoadGuardPosition(DBKeepPosition pos, GameKeepGuard guard)
		{
			LoadKeepItemPosition(pos, guard);
           
            guard.SpawnPoint.X = guard.X;
            guard.SpawnPoint.Y = guard.Y;
            guard.SpawnPoint.Z = guard.Z;
			guard.SpawnHeading = guard.Heading;
		}

		public static void LoadKeepItemPosition(DBKeepPosition pos, IKeepItem item)
		{
			item.CurrentRegionID = item.Component.CurrentRegionID;
            LoadXY(item.Component, pos.XOff, pos.YOff, out int x, out int y);
            item.X = x;
			item.Y = y;

			item.Z = item.Component.Keep.Z + pos.ZOff;

			item.Heading = (ushort)(item.Component.Heading + pos.HOff);

			item.DBPosition = pos;
		}

		/// <summary>
		/// Calculates X and Y based on component rotation and offset
		/// </summary>
		/// <param name="component">The assigned component object</param>
		/// <param name="inX">The argument X</param>
		/// <param name="inY">The argument Y</param>
		/// <param name="outX">The result X</param>
		/// <param name="outY">The result Y</param>
		public static void LoadXY(GameKeepComponent component, int inX, int inY, out int outX, out int outY)
		{
			double angle = component.Keep.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
			double C = Math.Cos(angle);
			double S = Math.Sin(angle);
			switch (component.ComponentHeading)
			{
				case 0:
					{
						outX = (int)(component.X + C * inX + S * inY);
						outY = (int)(component.Y - C * inY + S * inX);
						break;
					}
				case 1:
					{
						outX = (int)(component.X + C * inY - S * inX);
						outY = (int)(component.Y + C * inX + S * inY);
						break;
					}
				case 2:
					{
						outX = (int)(component.X - C * inX - S * inY);
						outY = (int)(component.Y + C * inY - S * inX);
						break;
					}
				case 3:
					{
						outX = (int)(component.X - C * inY + S * inX);
						outY = (int)(component.Y - C * inX - S * inY);
						break;
					}
				default:
					{
						outX = 0;
						outY = 0;
						break;
					}
			}
		}

		/// <summary>
		/// Saves X and Y offsets
		/// </summary>
		/// <param name="component">The assigned component object</param>
		/// <param name="inX">The argument X</param>
		/// <param name="inY">The argument Y</param>
		/// <param name="outX">The result X</param>
		/// <param name="outY">The result Y</param>
		public static void SaveXY(GameKeepComponent component, int inX, int inY, out int outX, out int outY)
		{
			double angle = component.Keep.Heading * ((Math.PI * 2) / 360); // angle*2pi/360;
			int gx = inX - component.X;
			int gy = inY - component.Y;
			double C = Math.Cos(angle);
			double S = Math.Sin(angle);
			switch (component.ComponentHeading)
			{
				case 0:
					{
						outX = (int)(gx * C + gy * S);
						outY = (int)(gx * S - gy * C);
						break;
					}
				case 1:
					{
						outX = (int)(gy * C - gx * S);
						outY = (int)(gx * C + gy * S);
						break;
					}
				case 2:
					{
						outX = (int)((gx * C + gy * S) / (-C * C - S * S));
						outY = (int)(gy * C - gx * S);
						break;
					}
				case 3:
					{
						outX = (int)(gx * S - gy * C);
						outY = (int)((gx * C + gy * S) / (-C * C - S * S));
						break;
					}
				default:
					{
						outX = 0;
						outY = 0;
						break;
					}
			}
		}

		/// <summary>
        /// Creates a position
		/// </summary>
		/// <param name="type"></param>
		/// <param name="height"></param>
		/// <param name="player"></param>
		/// <param name="guardID"></param>
		/// <param name="component"></param>
		/// <returns></returns>
		public static DBKeepPosition CreatePosition(Type type, int height, GamePlayer player, string guardID, GameKeepComponent component)
		{
			DBKeepPosition pos = CreatePosition(guardID, component, player);
			pos.Height = height;
			pos.ClassType = type.ToString();
			GameServer.Database.AddObject(pos);
			return pos;
		}

		/// <summary>
		/// Creates a guard patrol position
		/// </summary>
		/// <param name="guardID">The guard ID</param>
		/// <param name="component">The component object</param>
		/// <param name="player">The player object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition CreatePatrolPosition(string guardID, GameKeepComponent component, GamePlayer player, AbstractGameKeep.eKeepType keepType)
		{
			DBKeepPosition pos = CreatePosition(guardID, component, player);
			pos.Height = 0;
			pos.ClassType = "DOL.GS.Keeps.Patrol";
			pos.KeepType = (int)keepType;
			GameServer.Database.AddObject(pos);
			return pos;
		}

		/// <summary>
		/// Creates a position
		/// </summary>
		/// <param name="templateID">The template ID</param>
		/// <param name="component">The component object</param>
		/// <param name="player">The creating player object</param>
		/// <returns>The position object</returns>
		public static DBKeepPosition CreatePosition(string templateID, GameKeepComponent component, GamePlayer player)
		{
            DBKeepPosition pos = new DBKeepPosition
            {
                ComponentSkin = component.Skin,
                ComponentRotation = component.ComponentHeading,
                TemplateID = templateID
            };

            SaveXY(component, player.X, player.Y, out int x, out int y);
            pos.XOff = x;
			pos.YOff = y;

			pos.ZOff = player.Z - component.Z;

			pos.HOff = player.Heading - component.Heading;
			return pos;
		}

		public static void AddPosition(DBKeepPosition position)
		{
			foreach (AbstractGameKeep keep in KeepMgr.GetAllKeeps())
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					DBKeepPosition[] list = component.Positions[position.TemplateID] as DBKeepPosition[];
					if (list == null)
					{
						list = new DBKeepPosition[4];
						component.Positions[position.TemplateID] = list;
					}
					//list.SetValue(position, position.Height);
					list[position.Height] = position;
				}
			}
		}

		public static void RemovePosition(DBKeepPosition position)
		{
			foreach (AbstractGameKeep keep in KeepMgr.GetAllKeeps())
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					DBKeepPosition[] list = component.Positions[position.TemplateID] as DBKeepPosition[];
					if (list == null)
					{
						list = new DBKeepPosition[4];
						component.Positions[position.TemplateID] = list;
					}
					//list.SetValue(position, position.Height);
					list[position.Height] = null;
				}
			}
			GameServer.Database.DeleteObject(position);
		}

		public static void FillPositions()
		{
			foreach (AbstractGameKeep keep in KeepMgr.GetAllKeeps())
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					component.LoadPositions();
					component.FillPositions();
				}
			}
		}

		/// <summary>
		/// Method to retrieve the Patrol Path from the Patrol ID and Component
		/// 
		/// We need this because we store this all using our offset system
		/// </summary>
		/// <param name="pathID">The path ID, which is the Patrol ID</param>
		/// <param name="component">The Component object</param>
		/// <returns>The Patrol path</returns>
		public static PathPoint LoadPatrolPath(string pathID, GameKeepComponent component)
		{
			SortedList sorted = new SortedList();
			pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
                                       //DBPath dbpath = GameServer.Database.SelectObject<DBPath>("PathID='" + GameServer.Database.Escape(pathID) + "'");
            DBPath dbpath = GameServer.Database.SelectObjects<DBPath>("`PathID` = @PathID", new QueryParameter("@PathID", pathID)).FirstOrDefault();
           
            IList<DBPathPoint> pathpoints = null;
			ePathType pathType = ePathType.Once;

			if (dbpath != null)
			{
				pathType = (ePathType)dbpath.PathType;
			}
            if (pathpoints == null)
            {
                //pathpoints = GameServer.Database.SelectObjects<DBPathPoint>("PathID='" + GameServer.Database.Escape(pathID) + "'");
                pathpoints = GameServer.Database.SelectObjects<DBPathPoint>("`PathID` = @PathID", new QueryParameter("@PathID", pathID));
            }

			foreach (DBPathPoint point in pathpoints)
			{
				sorted.Add(point.Step, point);
			}
			PathPoint prev = null;
			PathPoint first = null;
			for (int i = 0; i < sorted.Count; i++)
			{
				DBPathPoint pp = (DBPathPoint)sorted.GetByIndex(i);
				PathPoint p = new PathPoint(pp.X, pp.Y, pp.Z, pp.MaxSpeed, pathType);

                LoadXY(component, pp.X, pp.Y, out int x, out int y);
                p.X = x;
				p.Y = y;
				p.Z = component.Keep.Z + p.Z;

				p.WaitTime = pp.WaitTime;

				if (first == null)
				{
					first = p;
				}
				p.Prev = prev;
				if (prev != null)
				{
					prev.Next = p;
				}
				prev = p;
			}
			return first;
		}

		/// <summary>
        /// Method to save the Patrol Path using the Patrol ID and the Component
		/// </summary>
		/// <param name="pathID"></param>
		/// <param name="path"></param>
		/// <param name="component"></param>
		public static void SavePatrolPath(string pathID, PathPoint path, GameKeepComponent component)
		{
			if (path == null)
				return;

            pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
            foreach (DBPath pp in GameServer.Database.SelectObjects<DBPath>("`PathID` = @PathID", new QueryParameter("@PathID", pathID)))
            {
                GameServer.Database.DeleteObject(pp);
            }

            PathPoint root = MovementMgr.FindFirstPathPoint(path);

			//Set the current pathpoint to the rootpoint!
			path = root;
			DBPath dbp = new DBPath(pathID, ePathType.Loop);
			GameServer.Database.AddObject(dbp);

			int i = 1;
			do
			{
				DBPathPoint dbpp = new DBPathPoint(path.X, path.Y, path.Z, path.MaxSpeed);
                SaveXY(component, dbpp.X, dbpp.Y, out int x, out int y);
                dbpp.X = x;
				dbpp.Y = y;
				dbpp.Z -= component.Z;

				dbpp.Step = i++;
				dbpp.PathID = pathID;
				dbpp.WaitTime = path.WaitTime;
				GameServer.Database.AddObject(dbpp);
				path = path.Next;
			} while (path != null && path != root);
		}

		public static void CreateDoor(int doorID, GamePlayer player)
		{
			int ownerKeepId = (doorID / 100000) % 1000;
			int towerNum = (doorID / 10000) % 10;
			int keepID = ownerKeepId + towerNum * 256;
			int componentID = (doorID / 100) % 100;
			int doorIndex = doorID % 10;

			AbstractGameKeep keep = KeepMgr.GetKeepByID(keepID);
			if (keep == null)
			{
				player.Out.SendMessage("Cannot create door as keep is null!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			GameKeepComponent component = null;
			foreach (GameKeepComponent c in keep.KeepComponents)
			{
				if (c.ID == componentID)
				{
					component = c;
					break;
				}
			}
			if (component == null)
			{
				player.Out.SendMessage("Cannot create door as component is null!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
            DBKeepPosition pos = new DBKeepPosition
            {
                ClassType = "DOL.GS.Keeps.GameKeepDoor",
                TemplateType = doorIndex,
                ComponentSkin = component.Skin,
                ComponentRotation = component.ComponentHeading,
                TemplateID = Guid.NewGuid().ToString()
            };

            SaveXY(component, player.X, player.Y, out int x, out int y);
            pos.XOff = x;
			pos.YOff = y;

			pos.ZOff = player.Z - component.Z;

			pos.HOff = player.Heading - component.Heading;

			GameServer.Database.AddObject(pos);

			player.Out.SendMessage("Added door successfully, restart the server", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}

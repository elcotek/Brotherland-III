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
using System.Collections.Generic;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	   "&faction",
	   ePrivLevel.GM,
	   "GMCommands.Faction.Description",
	   "GMCommands.Faction.Usage.Create",
	   "GMCommands.Faction.Usage.Assign",
	   "GMCommands.Faction.Usage.AddFriend",
	   "GMCommands.Faction.Usage.AddEnemy",
	   "GMCommands.Faction.Usage.List",
	   "GMCommands.Faction.Usage.Select",
	   "GMCommands.Faction.Info",
	   "GMCommands.Faction.Add",
		"GMCommands.Faction.Link")]
	public class FactionCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		protected string TEMP_FACTION_LAST = "TEMP_FACTION_LAST";

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);



		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}
			Faction myfaction = (Faction)client.Player.TempProperties.getProperty<object>(TEMP_FACTION_LAST, null);
			switch (args[1])
			{
				case "info":
					{

						GameNPC targetMob = client.Player.TargetObject as GameNPC;
						if (targetMob == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MustSelectMob"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						info(client, targetMob, args); break;

					}
				case "link":
					{
						GameNPC targetMob = client.Player.TargetObject as GameNPC;
						if (targetMob == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MustSelectMob"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;

						}

						//log.ErrorFormat("args lenght = {0}", args.Length);
						link(client, targetMob, args);

						break;
					}
				case "add":
					{
						GameNPC targetMob = client.Player.TargetObject as GameNPC;
						if (targetMob == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MustSelectMob"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;

						}

						//log.ErrorFormat("args lenght = {0}", args.Length);
						add(client, targetMob, args);

						break;
					}
				#region Create
				case "create":
					{
						GameNPC targetMob = client.Player.TargetObject as GameNPC;

						string name = "";

						if (targetMob == null)
						{
							name = args[2];
							//client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MustSelectMob"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							//return;
						}

						if (targetMob == null && args.Length < 4)
						{
							DisplaySyntax(client);
							return;
						}
						if (targetMob != null && args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}

						if (targetMob != null)
						{
							name = targetMob.Name;//args[2];
						}
						int baseAggro = 0;
						try
						{
							if (targetMob != null)
							{
								baseAggro = Convert.ToInt32(args[2]);
							}
							else
								baseAggro = Convert.ToInt32(args[3]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Create.BAMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						int max = 0;
						//Log.Info("count:" + FactionMgr.Factions.Count.ToString());
						if (FactionMgr.Factions.Count != 0)
						{
							//Log.Info("count >0");
							IEnumerator enumerator = FactionMgr.Factions.Keys.GetEnumerator();
							while (enumerator.MoveNext())
							{
								//Log.Info("max :" + max + " et current :" + (int)enumerator.Current);
								max = System.Math.Max(max, (int)enumerator.Current);
							}
						}
						//Log.Info("max :" + max);
						DBFaction dbfaction = new DBFaction();
						dbfaction.BaseAggroLevel = baseAggro;
						dbfaction.Name = name;
						dbfaction.ID = (max + 1);
						//Log.Info("add obj to db with id :" + dbfaction.ID);
						GameServer.Database.AddObject(dbfaction);
						//Log.Info("add obj to db");
						myfaction = new Faction();
						myfaction.LoadFromDatabase(dbfaction);
						FactionMgr.Factions.Add(dbfaction.ID, myfaction);



						if (targetMob != null)
						{
							CrateFactionMobID(client, targetMob, dbfaction.ID.ToString());
						}



						client.Player.TempProperties.setProperty(TEMP_FACTION_LAST, myfaction);
						client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Create.NewCreated"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					}
					break;
				#endregion Create
				#region Assign
				case "assign":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.MustSelectFaction"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}

						GameNPC npc = client.Player.TargetObject as GameNPC;
						if (npc == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MustSelectMob"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						npc.Faction = myfaction;
						client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.Assign.MobHasJoinedFact", npc.Name, myfaction.Name), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
					}
					break;
				#endregion Assign
				#region AddFriend
				case "addfriend":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.MustSelectFaction"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.IndexMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction linkedfaction = FactionMgr.GetFactionByID(id);
						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = myfaction.ID;
						dblinkedfaction.LinkedFactionID = linkedfaction.ID;
						dblinkedfaction.IsFriend = true;
						GameServer.Database.AddObject(dblinkedfaction);
						myfaction.AddFriendFaction(linkedfaction);
					}
					break;
				#endregion AddFriend
				#region AddEnemy
				case "addenemy":
					{
						if (myfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.MustSelectFaction"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.IndexMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction linkedfaction = FactionMgr.GetFactionByID(id);
						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = myfaction.ID;
						dblinkedfaction.LinkedFactionID = linkedfaction.ID;
						dblinkedfaction.IsFriend = false;
						GameServer.Database.AddObject(dblinkedfaction);
						myfaction.AddEnemyFaction(linkedfaction);
					}
					break;
				#endregion AddEnemy
				#region List
				case "list":
					{
						foreach (Faction faction in FactionMgr.Factions.Values)
							client.Player.Out.SendMessage("#" + faction.ID.ToString() + ": " + faction.Name + " (" + faction.BaseAggroLevel + ")", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
						return;
					}
				#endregion List
				#region Select
				case "select":
					{
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						int id = 0;
						try
						{
							id = Convert.ToInt32(args[2]);
						}
						catch
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.IndexMustBeNumber"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						Faction tempfaction = FactionMgr.GetFactionByID(id);
						if (tempfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						client.Player.TempProperties.setProperty(TEMP_FACTION_LAST, tempfaction);
					}
					break;

				#endregion Select
				#region Default
				default:
					{
						DisplaySyntax(client);
						return;
					}
					#endregion Default
			}

		}

		private void info(GameClient client, GameNPC targetMob, string[] args)
		{

			var info = new List<string>();

			Mob mob = null;

			if (targetMob.InternalID != null)
			{
				mob = GameServer.Database.FindObjectByKey<Mob>(targetMob.InternalID);
			}

			Faction tempfaction = null;
			string name = "";
			int factionID = 0;

			if (mob != null)
			{
				//GameServer.Database.UpdateInCache<DBFaction>(mob);
				tempfaction = FactionMgr.GetFactionByID(mob.FactionID);
			}
			if (tempfaction != null)
			{
				name = tempfaction.Name;
				factionID = tempfaction.ID;
			}




			if (mob != null)
			{


				GameServer.Database.UpdateInCache<Mob>(mob);

				if (targetMob.LoadedFromScript)
					info.Add(" + Loaded: from Script");
				else
					info.Add(" + Loaded: from Database");

				info.Add(" + Class: " + targetMob.GetType().ToString());
				info.Add(" + Realm: " + GlobalConstants.RealmToName(targetMob.Realm));



				if (tempfaction != null)
					info.Add($" + Faction: {name} [{factionID}]");
				else if (mob.FactionID >= 0)
				{
					info.Add($" + Faction: {"Mob faction ID are not in fation db:"} [{mob.FactionID}]");
				}

				info.Add(" + Level: " + targetMob.Level);


				IOldAggressiveBrain aggroBrain = targetMob.Brain as IOldAggressiveBrain;

				if (aggroBrain != null)
				{
					info.Add(" + Aggro level: " + aggroBrain.AggroLevel);
					info.Add(" + Aggro range: " + aggroBrain.AggroRange);

					if (targetMob.Faction != null)
					{
						info.Add(" + Faction aggro to player: " + targetMob.Faction.GetAggroToFaction(client.Player).ToString());
					}

					if (targetMob.MaxDistance < 0)
						info.Add(" + MaxDistance: " + -targetMob.MaxDistance * aggroBrain.AggroRange / 100);
					else
						info.Add(" + MaxDistance: " + targetMob.MaxDistance);

				}
				else
				{
					info.Add(" + Not aggressive brain");
				}


				info.Add(" + Brain: " + (targetMob.Brain == null ? "(null)" : targetMob.Brain.GetType().ToString()));


				info.Add("+ DB Status:" + CheckFactionDB());

				client.Out.SendCustomTextWindow("[ " + targetMob.Name + " ]", info);
			}
		}
		/// <summary>
		/// Whether this NPC is a friend or not.
		/// </summary>
		/// <param name="npc">The NPC that is checked against.</param>
		/// <returns></returns>
		public virtual string CheckFactionDB()
		{
			var dblinkedfactions = GameServer.Database.SelectAllObjects<DBLinkedFaction>();
			foreach (DBLinkedFaction dblinkedfaction in dblinkedfactions)
			{
				Faction faction = FactionMgr.GetFactionByID(dblinkedfaction.LinkedFactionID);
				Faction linkedFaction = FactionMgr.GetFactionByID(dblinkedfaction.FactionID);
				if (faction == null || linkedFaction == null)
				{
					return " Missing Faction or friend faction with Id :" + dblinkedfaction.LinkedFactionID + "/" + dblinkedfaction.FactionID;


				}
				else
				{
					return " Database checked ok!";
				}
			}
			return " Database not checked!";
		}





		private void link(GameClient client, GameNPC targetMob, string[] args)
		{
			int _factionID;
			bool isFriend = false;
			//IsFriend
			try
			{
				_factionID = int.Parse(args[2]);

				if (int.Parse(args[3]) >= 1)
				{
					isFriend = true;
				}
				else
					isFriend = false;
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}


			Mob mob = null;

			if (targetMob.InternalID != null)
			{
				mob = GameServer.Database.FindObjectByKey<Mob>(targetMob.InternalID);
			}

			if (mob != null)
			{

				if (targetMob.Faction != null && targetMob.LoadedFromScript == false)
				{
					try
					{
						IList<Mob> mobs = GameServer.Database.SelectObjects<Mob>("`Name` LIKE @Name", new QueryParameter("@Name", mob.Name));

						foreach (Mob _mob in mobs)
						{
							if (_mob.Name == targetMob.Name)
							{
								log.ErrorFormat("link id Friend", isFriend);
								/*
								IOldAggressiveBrain aggroBrain = targetMob.Brain as IOldAggressiveBrain;

								if (aggroBrain != null)
								{
									
									if (_mob.AggroLevel > 0 && aggroBrain.AggroLevel > 0)
									{
										isFriend = true;

									}
									if (_mob.AggroLevel == 0 && aggroBrain.AggroLevel > 0)
									{
										isFriend = false;
									}
									if (_mob.AggroLevel == 0 && aggroBrain.AggroLevel == 0)
									{
										isFriend = true;
									}
								}
								*/
							}
						}

						Faction linkedfaction = FactionMgr.GetFactionByID(_factionID);

						if (linkedfaction == null)
						{
							client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "GMCommands.Faction.FactionNotLoaded"), eChatType.CT_Say, eChatLoc.CL_SystemWindow);
							return;
						}
						DBLinkedFaction dblinkedfaction = new DBLinkedFaction();
						dblinkedfaction.FactionID = targetMob.Faction.ID;
						dblinkedfaction.LinkedFactionID = linkedfaction.ID;
						dblinkedfaction.IsFriend = isFriend;
						GameServer.Database.AddObject(dblinkedfaction);
						client.Out.SendMessage("Target mob Faction id: " + targetMob.Faction.ID + " linked to Faction id: " + _factionID + " if Friend: " + dblinkedfaction.IsFriend + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					catch
					{

					}
				}
				else
				{
					client.Out.SendMessage("The "+ targetMob.Name +" mob has no Faction!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}
		
		
		
		private void add(GameClient client, GameNPC targetMob, string[] args)
		{
			int _factionID;
			int aggressionLevel = 0;
			bool containsID = false;
			try
			{
				_factionID = int.Parse(args[2]);
			}
			catch (Exception)
			{
				DisplaySyntax(client, args[1]);
				return;
			}


			Mob mob = null;

			if (targetMob.InternalID != null)
			{
				mob = GameServer.Database.FindObjectByKey<Mob>(targetMob.InternalID);
			}

			if (mob != null)
			{
				if (targetMob.LoadedFromScript == false)
				{
					try
					{
						client.Out.SendMessage("please wait... Write Faction id: " + _factionID + " to all mobs with the same Name", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						IList<Mob> mobs = GameServer.Database.SelectObjects<Mob>("`Name` LIKE @Name", new QueryParameter("@Name", mob.Name));

						foreach (Mob _mob in mobs)
						{
							if (_mob.Name == targetMob.Name)
							{
								_mob.FactionID = _factionID;

								if (aggressionLevel == 0)
									aggressionLevel = _mob.AggroLevel;

							  	     
							}
							//mob.FactionID = _factionID;
							GameServer.Database.SaveObject(_mob);
							GameServer.Database.UpdateInCache<Mob>(_mob);
						}


						int max = 0;
						//Log.Info("count:" + FactionMgr.Factions.Count.ToString());
						if (FactionMgr.Factions.Count != 0)
						{
							//Log.Info("count >0");
							IEnumerator enumerator = FactionMgr.Factions.Keys.GetEnumerator();
							while (enumerator.MoveNext())
							{
								//Log.Info("max :" + max + " et current :" + (int)enumerator.Current);
								max = System.Math.Max(max, (int)enumerator.Current);
							}


						}

						if (max + 1 == _factionID)
						{
							//log.ErrorFormat("max2: {0}", max);
							//add only if not exist
						}
						else
						{
							IList<DBFaction> dbfact = GameServer.Database.SelectObjects<DBFaction>("`ID` LIKE @ID", new QueryParameter("@ID", _factionID));
							foreach (DBFaction ft in dbfact)
							{
								if (ft.ID == _factionID)
								{
									containsID = true;
									client.Out.SendMessage("Faction ID added to all Mobs with name: (" + targetMob.Name + ")  - but Faction id: " + _factionID + " was already in the bd. skip new ID faction create..", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
							}
						}

						if (containsID == false)
						{
							//Log.Info("max :" + max);
							DBFaction dbfaction = new DBFaction();
							if (dbfaction.BaseAggroLevel != aggressionLevel)
							{
								dbfaction.BaseAggroLevel = aggressionLevel;
							}

							dbfaction.Name = targetMob.Name;
							dbfaction.ID = _factionID;

							GameServer.Database.AddObject(dbfaction);
							//FactionMgr.Init();
							//GameServer.Database.UpdateInCache<DBFaction>(dbfaction);


							client.Out.SendMessage("Faction Save and added to all mobs with the name: (" + targetMob.Name + ")  with Faction id: " + _factionID + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						MobReload(mob.Name, client.Player);
					}
					catch
                    {
						log.Error("Fehler Faction add!");
                    }
				}
				else
				{
					client.Out.SendMessage("Error: This mob is loaded from script", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
			
				
				
			}


			//targetMob.SaveIntoDatabase();
			//client.Out.SendMessage("Mob faction changed to: " + mob.FactionID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		static void MobReload(string monbame, GamePlayer player)
		{
			int count = 0;

			foreach (GameNPC mob in WorldMgr.GetNPCsFromRegion(player.CurrentRegionID))
			{
				if (!mob.LoadedFromScript)
				{
					if (player.TargetObject != null && player.TargetObject is GameNPC)
					{
						if (mob.Name == monbame)
						{
							FactionMgr.Init();
							mob.RemoveFromWorld();
							Mob mobs = GameServer.Database.FindObjectByKey<Mob>(mob.InternalID);

							if (mobs != null)
							{
								mob.LoadFromDatabase(mobs);
								mob.AddToWorld();
								count++;
							}
						}
					}
				}
			}
			ChatUtil.SendSystemMessage(player, count + " Facion Mobs reloaded!");
		}
		private void CrateFactionMobID(GameClient client, GameNPC targetMob, string id)
		{
			int _factionID;

			try
			{
				_factionID = int.Parse(id);
			}
			catch (Exception)
			{
				return;
			}


			Mob mob = null;

			if (targetMob.InternalID != null)
			{
				mob = GameServer.Database.FindObjectByKey<Mob>(targetMob.InternalID);
			}

			if (mob != null)
			{
				if (targetMob.LoadedFromScript == false)
				{
					mob.FactionID = _factionID;
				}
				else
				{
					client.Out.SendMessage("Error: This mob is loaded from script", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				GameServer.Database.SaveObject(mob);
			}


			//targetMob.SaveIntoDatabase();
			client.Out.SendMessage("Mob faction changed to: " + mob.FactionID, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}
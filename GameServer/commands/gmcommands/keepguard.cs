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
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections;

namespace DOL.GS.Commands
{
    /// <summary>
    /// Various keep guard commands
    /// </summary>
    [CmdAttribute(
		"&keepguard",
         ePrivLevel.Admin,
		"GMCommands.KeepGuard.Description",
		"GMCommands.KeepGuard.Information",
		"GMCommands.KeepGuard.Usage.Create",
		"GMCommands.KeepGuard.Usage.Position.Add",
		"GMCommands.KeepGuard.Usage.Position.Remove",
		"GMCommands.KeepGuard.Usage.Path.Create",
		"GMCommands.KeepGuard.Usage.Path.Add",
		"GMCommands.KeepGuard.Usage.Path.Save")]
	public class KeepGuardCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		/// <summary>
		/// The command handler itself
		/// </summary>
		/// <param name="client">The client using the command</param>
		/// <param name="args">The command arguments</param>
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
				#region Create
				case "create":
					{
						GameKeepGuard guard = null;
						if (args.Length < 3)
						{
							DisplaySyntax(client);
							return;
						}
						
						switch (args[2].ToLower())
						{
							#region Lord
							case "lord":
								{
									guard = new GuardLord();
									break;
								}
							#endregion Lord
							#region Fighter
							case "fighter":
								{
									guard = new GuardFighter();
									break;
								}
							#endregion Fighter
							#region Archer
							case "archer":
								{
									if (args.Length > 3)
										guard = new GuardStaticArcher();
									else
										guard = new GuardArcher();
									break;
								}
							#endregion Archer
							#region Healer
							case "healer":
								{
									guard = new GuardHealer();
									break;
								}
							#endregion Healer
							#region Stealther
							case "stealther":
								{
									guard = new GuardStealther();
									break;
								}
							#endregion Stealther
							#region Caster
							case "caster":
								{
									if (args.Length > 3)
										guard = new GuardStaticCaster();
									else
										guard = new GuardCaster();
									break;
								}
							#endregion Caster
							#region Hastener
							case "hastener":
								{
									guard = new FrontierHastener();
									break;
								}
							#endregion Hastener
							#region Mission
							case "mission":
								{
									guard = new MissionMaster();
									break;
								}
							#endregion Mission
							#region Patrol
							case "patrol":
								{
									if (args.Length < 4)
									{
										DisplayMessage(client, "You need to provide a name for this patrol.");
										return;
									}

									AbstractGameKeep.eKeepType keepType = AbstractGameKeep.eKeepType.Any;

									if (args.Length < 5)
									{
										DisplayMessage(client, "You need to provide the type of keep this patrol works with.");
										int i = 0;
										foreach (string str in Enum.GetNames(typeof(Keeps.AbstractGameKeep.eKeepType)))
										{
											DisplayMessage(client, "#" + i + ": " + str);
											i++;
										}
										return;
									}

									try
									{
										keepType = (AbstractGameKeep.eKeepType)Convert.ToInt32(args[4]);
									}
									catch
									{
										DisplayMessage(client, "Type of keep specified was not valid.");
										return;
									}


									if (client.Player.TargetObject is GameKeepComponent == false)
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Create.NoKCompTarget"));
										return;
									}
									GameKeepComponent c = client.Player.TargetObject as GameKeepComponent;
                                    Patrol p = new Patrol(c)
                                    {
                                        PatrolID = args[3],
                                        KeepType = keepType
                                    };
                                    p.SpawnPosition = PositionMgr.CreatePatrolPosition(p.PatrolID, c, client.Player, keepType);
									p.PatrolID = p.SpawnPosition.TemplateID;
									p.InitialiseGuards();
									DisplayMessage(client, "Patrol created for Keep Type " + Enum.GetName(typeof(AbstractGameKeep.eKeepType), keepType));
									return;
								}
							#endregion Patrol
						}

						if (guard == null)
						{
							DisplaySyntax(client);
							return;
						}

						GameKeepComponent component = client.Player.TargetObject as GameKeepComponent;
						if (component != null)
						{
							int height = component.Height;
							if (args.Length > 4)
								int.TryParse(args[4], out height);

							DBKeepPosition pos = PositionMgr.CreatePosition(guard.GetType(), height, client.Player, Guid.NewGuid().ToString(), component);
							//PositionMgr.AddPosition(pos);
							//PositionMgr.FillPositions();
							DBKeepPosition[] list = component.Positions[pos.TemplateID] as DBKeepPosition[];
							if (list == null)
							{
								list = new DBKeepPosition[4];
								component.Positions[pos.TemplateID] = list;
							}
								
							list[pos.Height] = pos;
							component.LoadPositions();
							component.FillPositions();
						}
						else
						{
							guard.CurrentRegion = client.Player.CurrentRegion;
							guard.X = client.Player.X;
							guard.Y = client.Player.Y;
							guard.Z = client.Player.Z;
							guard.Heading = client.Player.Heading;
							guard.Realm = guard.CurrentZone.GetRealm();
                            guard.LoadedFromScript = false;
                            guard.SaveIntoDatabase();
							
							foreach (AbstractArea area in guard.CurrentAreas)
							{
								if (area is GameKeepArea)
								{
									AbstractGameKeep keep = (area as GameKeepArea).Keep;
                                    guard.Component = new GameKeepComponent
                                    {
                                        Keep = keep
                                    };
                                    break;
								}
							}
                            TemplateMgr.RefreshTemplate(guard);
							guard.AddToWorld();

							if (guard.Component != null && guard.Component.Keep != null)
								guard.Component.Keep.Guards.Add(DOL.Database.UniqueID.IDGenerator.GenerateID(), guard);
						}

						PositionMgr.FillPositions();

						DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Create.GuardAdded"));
						break;
					}
				#endregion Create
				#region Position
				case "position":
					{
						switch (args[2].ToLower())
						{
							#region Add
							case "add":
								{
									if (!(client.Player.TargetObject is GameKeepGuard))
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Position.TargetGuard"));
										return;
									}

									if (args.Length != 4)
									{
										DisplaySyntax(client);
										return;
									}

									byte height = byte.Parse(args[3]);
                                    height = KeepMgr.GetHeightFromLevel(null, height);
									GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;
									
									if (PositionMgr.GetPosition(guard) != null)
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Position.PAlreadyAss", height));
										return;
									}

									DBKeepPosition pos = PositionMgr.CreatePosition(guard.GetType(), height, client.Player, guard.TemplateID, guard.Component);
									PositionMgr.AddPosition(pos);
									PositionMgr.FillPositions();

									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Position.GuardPAdded"));
									break;
								}
							#endregion Add
							#region Remove
							case "remove":
								{
									if (!(client.Player.TargetObject is GameKeepGuard))
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Position.TargetGuard"));
										return;
									}

									GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;
									DBKeepPosition pos = guard.DBPosition;
									if (pos != null)
									{
										PositionMgr.RemovePosition(pos);

										if (guard.LoadedFromScript)
										{
											if (guard.PatrolGroup != null)
											{
												foreach (GameKeepGuard g in guard.PatrolGroup.PatrolGuards)
												{
													g.Delete();
												}
											}
											else
											{
												guard.Delete();
											}
										}
									}

									PositionMgr.FillPositions();

									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Position.GuardRemoved"));
									break;
								}
							#endregion Remove
							#region Default
							default:
								{
									DisplaySyntax(client);
									return;
								}
							#endregion Default
						}
						break;
					}
				#endregion Position
				#region Path
				case "path":
					{
						switch (args[2].ToLower())
						{
							#region Create
							case "create":
								{
									RemoveAllTempPathObjects(client);

									PathPoint startpoint = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, 100000, ePathType.Once);
									client.Player.TempProperties.setProperty(TEMP_PATH_FIRST, startpoint);
									client.Player.TempProperties.setProperty(TEMP_PATH_LAST, startpoint);
									client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.CreationStarted"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									CreateTempPathObject(client, startpoint, "TMP PP 1");
									break;
								}
							#endregion Create
							#region Add
							case "add":
								{
									PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
									if (path == null)
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.NoPathCreatedYet"));
										return;
									}

									int speedlimit = 1000;
									if (args.Length == 4)
									{
										try
										{
											speedlimit = int.Parse(args[3]);
										}
										catch
										{
											DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.NoValidSpLimit", args[2]));
											return;
										}
									}

									PathPoint newpp = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, speedlimit, path.Type);
									path.Next = newpp;
									newpp.Prev = path;
									client.Player.TempProperties.setProperty(TEMP_PATH_LAST, newpp);

									int len = 0;
									while (path.Prev != null)
									{
										len++;
										path = path.Prev;
									}
									len += 2;

									CreateTempPathObject(client, newpp, "TMP PP " + len);
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.PPAdded", len));
									break;
								}
							#endregion Add
							#region Save
							case "save":
								{
									PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
									if (path == null)
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.NoPathCreatedYet"));
										return;
									}

									GameKeepGuard guard = client.Player.TargetObject as GameKeepGuard;
									if (guard == null || guard.PatrolGroup == null)
									{
										DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.TargPatrolGuard"));
										return;
									}

									path.Type = ePathType.Loop;
									PositionMgr.SavePatrolPath(guard.TemplateID, path, guard.Component);
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.KeepGuard.Path.Saved"));
									RemoveAllTempPathObjects(client);
									guard.PatrolGroup.InitialiseGuards();

									PositionMgr.FillPositions();

									DisplayMessage(client, "Patrol groups initialized!");

									break;
								}
							#endregion Save
							#region Default
							default:
								{
									DisplaySyntax(client);
									return;
								}
							#endregion Default
						}
						break;
					}
				#endregion Path
				#region Default
				default:
					{
						DisplaySyntax(client);
						return;
					}
				#endregion Default
			}
		}

		protected string TEMP_PATH_FIRST = "TEMP_PATH_FIRST";
		protected string TEMP_PATH_LAST = "TEMP_PATH_LAST";
		protected string TEMP_PATH_OBJS = "TEMP_PATH_OBJS";

		private void CreateTempPathObject(GameClient client, PathPoint pp, string name)
		{
            GameStaticItem obj = new GameStaticItem
            {
                X = pp.X,
                Y = pp.Y,
                Z = pp.Z,
                CurrentRegion = client.Player.CurrentRegion,
                Heading = client.Player.Heading,
                Name = name,
                Model = 488,
                Emblem = 0
            };
            obj.AddToWorld();
            ArrayList objs = (ArrayList)client.Player.TempProperties.getProperty<object>(TEMP_PATH_OBJS, null);
			if (objs == null)
                objs = new ArrayList();
			objs.Add(obj);
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, objs);
		}

		private void RemoveAllTempPathObjects(GameClient client)
		{
            ArrayList objs = (ArrayList)client.Player.TempProperties.getProperty<object>(TEMP_PATH_OBJS, null);
			if (objs == null)
				return;
			foreach (GameStaticItem obj in objs)
				obj.Delete();
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, null);
		}
	}
}
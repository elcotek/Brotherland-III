using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DOL.Database;
using DOL.GS.Keeps;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Path calculator component that can be added to any NPC to calculate paths
	/// @author mlinder
	/// </summary>
	public sealed class PathCalculator
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Minimum distance that a target has to be away before we start plotting paths instead of directly
		/// walking towards the target. Includes Z.
		/// </summary>
		public const int MIN_PATHING_DISTANCE = 80;

		/// <summary>
		/// Minimum distance difference required before a path is being replotted
		/// </summary>
		public const int MIN_TARGET_DIFF_REPLOT_DISTANCE = 80;

		/// <summary>
		/// Distance at which we consider a pathing node reached
		/// </summary>
		public const int NODE_REACHED_DISTANCE = 24;

		/// <summary>
		/// Distance to search for doors when computing NextDoor.
		/// </summary>
		private const int DOOR_SEARCH_DISTANCE = 512;

		/// <summary>
		/// True if this calculator can be used for the specified NPC.
		/// </summary>
		/// <param name="o"></param>
		/// <returns></returns>
		public static bool IsSupported(GameNPC o)
		{
			return o?.CurrentZone != null && o.CurrentZone.IsPathingEnabled;
		}

		/// <summary>
		/// Owner to which this calculator belongs to. Used for calculating position offsets
		/// </summary>
		public GameNPC Owner { get; private set; }

		/// <summary>
		/// If set, contains the next door on the NPCs path
		/// </summary>
		public IDoor NextDoor { get; private set; }

		//private readonly Queue<WrappedPathPoint> _pathNodes = new Queue<WrappedPathPoint>();
		private readonly Queue<WrappedPathPoint> _pathNodes = new Queue<WrappedPathPoint>();
		private Vector3 _lastTarget = Vector3.Zero;
		private GamePath _visualizationPath;

		/// <summary>
		/// Forces the path to be replot on the next CalculateNextTarget(...)
		/// </summary>
		public bool ForceReplot { get; set; }

		/// <summary>
		/// True if a path to the target was plotted. False if resorting back to air/direct route
		/// </summary>
		public bool DidFindPath { get; private set; }

		/// <summary>
		/// True if this path should be visualized
		/// </summary>
		public bool VisualizePath { get; set; }

		/// <summary>
		/// Creates a path calculator for the given NPC
		/// </summary>
		/// <param name="owner"></param>
		public PathCalculator(GameNPC owner)
		{
			ForceReplot = true;
			Owner = owner;
		}

		/// <summary>
		/// True if we should path towards the target point
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		private bool ShouldPath(Vector3 target)
		{
			return ShouldPath(Owner, target);
		}

		/// <summary>
		/// Clears all data stored in this calculator
		/// </summary>
		public void Clear()
		{
			_pathNodes.Clear();
			_lastTarget = Vector3.Zero;
			DidFindPath = false;
			ForceReplot = true;
		}

		/// <summary>
		/// True if we should path towards the target point
		/// </summary>
		/// <param name="owner"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public static bool ShouldPath(GameNPC owner, Vector3 target)
		{
			Vector3 sourcePosition = new Vector3(owner.X, owner.Y, owner.Z);
			sourcePosition.ToVector2();

			if (Vector3.Distance(sourcePosition, target) < MIN_PATHING_DISTANCE)
				return false; // too close to path
			if ((owner.Flags & GameNPC.eFlags.FLYING) != 0)
				return false;
			if (owner.Z <= float.Epsilon)
				return false; // this will probably result in some really awkward paths otherwise
			var zone = owner.CurrentZone;
			if (zone == null || !zone.IsPathingEnabled)
				return false; // we're in nirvana
			if (owner.CurrentRegion.GetZone(target) != zone)
				return false; // target is in a different zone (TODO: implement this maybe? not sure if really required)
			return true;
		}

		//private readonly Meter noPathFoundMetric = Metric.Meter("NoPathFound", Unit.Calls);

		/// <summary>
		/// Semaphore to prevent multiple replots
		/// </summary>
		/// <summary>
		/// Semaphore to prevent multiple replots
		/// </summary>
		private int calculatorState = (int)CalcState.IDLE;

		enum CalcState : int
		{
			IDLE = 0,
			REPLOTTING = 1,
		}

		private async Task ReplotPathAsync(Vector3 target)
		{
			lock (_pathNodes)
				_lastTarget = target;
			// Try acquiring a pathing lock
			if (Interlocked.CompareExchange(ref calculatorState, (int)CalcState.REPLOTTING, (int)CalcState.IDLE) != (int)CalcState.IDLE)
			{
				// Computation is already in progress. ReplotPathAsync will be called again automatically by .PathTo every few ms
				return;
			}

			try
			{
				while (!await CalculatePathAsync(target))
					target = _lastTarget;
			}
			finally
			{
				if (Interlocked.Exchange(ref calculatorState, (int)CalcState.IDLE) != (int)CalcState.REPLOTTING)
					log.Warn("PathCalc semaphore was in IDLE state even though we were replotting. This should never happen");
			}
		}

		private async Task<bool> CalculatePathAsync(Vector3 target)
		{
			
			var currentZone = Owner.CurrentZone;
			var currentPos = new Vector3(Owner.X, Owner.Y, Owner.Z);
			//var currentPos = currentPos;
			// we make a blocking call here because we are already in a worker thread and inside a lock
			var pathingResult = await PathingMgr.Instance.GetPathStraightAsync(currentZone, currentPos, target).ConfigureAwait(false);

			lock (_pathNodes)
			{
				if (_lastTarget != target)
				{
					Owner.DebugSend("target changed from {0} to {1}", target, _lastTarget);
					return false;
				}

				_pathNodes.Clear();
				if (pathingResult.Error != PathingError.NoPathFound && pathingResult.Error != PathingError.NavmeshUnavailable &&
					pathingResult.Points != null)
				{
					DidFindPath = true;
					var to = pathingResult.Points.Length;
					if (pathingResult.Error == PathingError.PartialPathFound)
					{
						to = pathingResult.Points.Length;
					}
					for (int i = 1; i < to; i++) /* remove first node */
					{
						var pt = pathingResult.Points[i];
						if (pt.Position.X < -500000)
						{
							log.Error("PathCalculator.ReplotPath returned weird node: " + pt + " (result=" + pathingResult.Error +
									  "); this=" + this);
						}
						_pathNodes.Enqueue(pt);
					}

					Owner.DebugSend("Found path to target with {0} nodes for {1} (VisualizePath={2})", pathingResult.Points.Length, Owner.Name, VisualizePath);

					// Visualize the path?
					if (VisualizePath)
					{
						DoVisualizePath(pathingResult);
					}
					else if (_visualizationPath != null)
					{
						_visualizationPath.Hide();
						_visualizationPath = null;
					}
				}
				else
				{
					//noPathFoundMetric.Mark();
					DidFindPath = false;
					Owner.DebugSend("No path to destination found for {0}", Owner.Name);
				}
				ForceReplot = false;
			}

			return true;
		}

		
		private void DoVisualizePath(WrappedPathingResult pathingResult)
		{
			_visualizationPath?.Hide();
			_visualizationPath = new GamePath($"path({Owner.Name})", Owner.CurrentRegion) { HasLavaEffect = false };
			foreach (var node in pathingResult.Points)
			{
				var model = GamePath.MarkerModel.Green;
				if ((node.Flags & dtPolyFlags.DOOR) != 0)
					model = GamePath.MarkerModel.Red;
				else if ((node.Flags & dtPolyFlags.SWIM) != 0)
					model = GamePath.MarkerModel.Blue;
				else if ((node.Flags & dtPolyFlags.JUMP) != 0)
					model = GamePath.MarkerModel.Yellow;

				_visualizationPath.Append(new GameLocation("", Owner.CurrentRegionID, (int)node.Position.X, (int)node.Position.Y, (int)node.Position.Z, 0), Owner.MaxSpeed, model);
				//(String name, ushort regionId, int x, int y, int z, ushort heading) : base(x, y, z)
			}
			//Owner.DebugSend("Visualizing Path");
			_visualizationPath.Show();
		}
		//protected static List<int> DoorList = new List<int>();

		/*
		/// <summary>
		/// Loads Doors into List
		/// </summary>
		public static void LoadDoorsfromList()
		{
			int doorID = 0;
			//var monsters = GameServer.Database.SelectAllObjects<DayMonsters>();
			var dbdoors = GameServer.Database.SelectAllObjects<DBDoor>();
			if (dbdoors != null)
			{
				for (int i = 0; i < dbdoors.Count; i++)
				{
					if (string.IsNullOrEmpty(dbdoors[i].InternalID.ToString()) == false)
					{
						doorID = dbdoors[i].InternalID;

					}

					if (doorID > 0 && DoorList.Contains(doorID) == false)
					{
						DoorList.Add(doorID);
					}
				}
			}
		}
		*/
		/*
		public static void RemoveDoorsfromList(int DoorID)
		{
			int doorID = 0;
			//var monsters = GameServer.Database.SelectAllObjects<DayMonsters>();
			if (DoorList.Count > 0)
			{
				if (DoorList != null)
				{
					for (int i = 0; i < DoorList.Count; i++)
					{
						if (string.IsNullOrEmpty(DoorList[i].ToString()) == false)
						{
							doorID = DoorList[i];
							log.ErrorFormat("Entferne Door aus Liste! ID: {0}", doorID);
						}

						if (doorID > 0 && DoorList.Contains(doorID))
						{
							DoorList.RemoveAt(i);
						}
					}
				}
			}
		}
		*/

		public IDoor FindDoorsInRadius(GameObject owner, WrappedPathPoint node, int DOOR_SEARCH_DISTANCE, Vector3 currentNode)
		{
			IDoor door = null;
			
			//LoadDoorsfromList();
			
			int distance = 0;
			if (owner != null && (owner.Name == "rat") == false && (node.Position != null || currentNode != null))
			{

				var dbdoors = GameServer.Database.SelectAllObjects<DBDoor>();
				if (dbdoors != null)
				{
					foreach (var doors in dbdoors)
					{
						
						List<IDoor> GameDoorListTest = DoorMgr.getDoorByID(doors.InternalID);
						if (GameDoorListTest.Count > 0)
						{
							
							//Console.Out.WriteLine("IDoorFound !");
							foreach (IDoor mydoor in GameDoorListTest)
							{
								IDoor mydoors = mydoor as IDoor;



								if (mydoor != null && owner.IsWithinRadius(mydoor, 500))
								{

									door = mydoor; // AtTheDoor.GetDoorsInRadius(1000) as GameDoor;
												   //log.ErrorFormat("Door!! ID: {0}", mydoor.DoorID);
									Point3D Cuurentpoint = new Point3D((int)currentNode.X, (int)currentNode.Y, (int)currentNode.Z);
									//Point3D OwnerPosition = new Point3D((int)owner.X, (int)owner.Y, (int)owner.Z);
									Point3D DoorPosition = new Point3D((int)mydoor.X, (int)mydoor.Y, (int)mydoor.Z);
									//log.ErrorFormat("Cuurentpoint = {0}", Cuurentpoint); // ist kleiner
																						 //log.ErrorFormat("OwnerPosition = {0}", OwnerPosition);
									//log.ErrorFormat("DoorPosition = {0}", DoorPosition); // ist größer
																						 //AtTheDoor.RemoveFromWorld();
									if (mydoor is GameDoor)
										distance = (mydoor as GameDoor).GetDistanceTo(Cuurentpoint);

									//log.ErrorFormat("Dor gefunden !! Distance zum punkt = {0}", distance);

									if (Cuurentpoint.X != DoorPosition.X)
									{
										//log.ErrorFormat("Door!! ID: {0}", mydoor.DoorID);
										//log.ErrorFormat("Door state: {0}, Realm: {1}", mydoor.State, mydoor.Realm);
										if (mydoor.State == eDoorState.Closed)
										{
											mydoor.Open();
											(mydoor as GameObject).BroadcastUpdate();

										}
										//log.Error("Door gefunden ??");
										//return mydoor as GameDoor;
										return mydoor;
									}

								}
							}

						}

					}
				}
				
			}
			return null;
		}


		/// <summary>
		/// Calculates the next point this NPC should walk to to reach the target
		/// </summary>
		/// <param name="destination"></param>
		/// <returns>Next path node, or null if target reached. Throws a NoPathToTargetException if path is blocked/returns>
		/// <summary>
		/// Calculates the next point this NPC should walk to to reach the target
		/// </summary>
		/// <param name="destination"></param>
		/// <returns>Next path node, or null if target reached. Throws a NoPathToTargetException if path is blocked/returns>
		public async Task<Tuple<Vector3?, NoPathReason>> CalculateNextTargetAsync(Vector3? destination = null)
		{
			var target = destination ?? _lastTarget;

			if (!ShouldPath(target))
			{

				//log.ErrorFormat("Skipping pathing for target {0}", target);
				DidFindPath = true; // not needed
				return new Tuple<Vector3?, NoPathReason>(null, NoPathReason.NOPROBLEM);
			}

			Interlocked.Increment(ref Statistics.PathToCalculateNextTargetCalls);

			// Check if we can reuse our path. We assume that we ourselves never "suddenly" warp to a completely
			// different position.
			if (ForceReplot || !_lastTarget.IsInRange(target, MIN_TARGET_DIFF_REPLOT_DISTANCE))
			{
				//Owner.DebugSend("Target moved too far from original target or forced replot; replotting path");
				///log.ErrorFormat("Target moved too far from original target or forced replot; replotting path {0}", Owner.Name);
				await ReplotPathAsync(target).ConfigureAwait(false);
			}

			Vector3 sourcePosition = new Vector3(Owner.X, Owner.Y, Owner.Z);

			// Find the next node in the path to the target, but skip points that are too close
			while (_pathNodes.Count > 0 && sourcePosition.IsInRange(_pathNodes.Peek().Position, NODE_REACHED_DISTANCE))
			{
				//Owner.DebugSend("Pathing node reached; removing");
				//----------------------------log.Error("Pathing node reached; removing");
				_pathNodes.Dequeue();
			}

			// Scan the next few nodes for a potential door
			// TODO(mlinder): Implement support for doors
			NextDoor = null;
			foreach (var node in _pathNodes.Take(1))
			{

				if ((node.Flags & dtPolyFlags.DOOR) != 0)
				{
					var currentNode = node;
					try
					{

						NextDoor = FindDoorsInRadius(Owner, node, DOOR_SEARCH_DISTANCE, currentNode.Position);

						//public GameDoor FindDoorsInRadius(GameObject owner, WrappedPathPoint node, int DOOR_SEARCH_DISTANCE, Vector3 currentNode)
						// TODO(mlinder): Confirm whether this actually makes sure that the path goes through a door, and not just next to it?
					}
					catch (InvalidOperationException)
					{
						// TODO(mlinder): this is really inefficient b/c of exception handling, duh
						Owner.DebugSend("Did not find door in radius");
					}
					break;
				}
			}

			// Open doors automagically (maybe not the best place to do this?)
			if (NextDoor != null)
			{
				//log.ErrorFormat("There is a door on the next segment: {0}", NextDoor);
				if (!DealWithDoorOnPath(NextDoor))
				{
					return new Tuple<Vector3?, NoPathReason>(null, NoPathReason.DOOR_EN_ROUTE);
				}
			}

			if (_pathNodes.Count == 0)
			{
				// Path end reached, or no path found
				//Owner.DebugSend("No nodes remaining for {0}; choosing direct path", Owner.Name);
				////-------------------log.ErrorFormat("No nodes remaining for {0}; choosing direct path", Owner.Name);
				if (!DidFindPath)
				{
					return new Tuple<Vector3?, NoPathReason>(null, NoPathReason.RECAST_FOUND_NO_PATH);
				}
				return new Tuple<Vector3?, NoPathReason>(null, NoPathReason.UNKNOWN); // no more nodes (or no path)
			}

			// Just walk to the next pathing node
			var next = _pathNodes.Peek();

			///----------------------------log.ErrorFormat("Sending NPC {0} to {1} to reach {2} (remaining nodes={3}; dist={4})", Owner.Name, next, target, _pathNodes.Count, Vector3.Distance(next.Position, Owner.Position));
			return new Tuple<Vector3?, NoPathReason>(next.Position, NoPathReason.NOPROBLEM);
		}

		/** Periodically called when a closeby door is on our path to the target. */
		private bool DealWithDoorOnPath(IDoor nextDoor)
		{
			return true;

			/*
			Vector3 ownerPosition = new Vector3(Owner.X, Owner.Y, Owner.Z);
			Vector3 doorPosition = new Vector3(nextDoor.X, nextDoor.Y, nextDoor.Z);


			if (nextDoor is GameDoor)
				if (!ownerPosition.IsInRange(doorPosition, (nextDoor as GameDoor).InteractDistance))
				{
					return true; // Next door is still too far away
				}


			// Door is blocked/other realm
			log.Error("Interact with door failed; stopping movement");

			Owner.StopFollowing();
			Owner.StopMoving();
			return false;
			*/
		}

		public override string ToString()
		{
			return $"PathCalc[Target={_lastTarget}, Nodes={_pathNodes.Count}, NextNode={(_pathNodes.Count > 0 ? _pathNodes.Peek().ToString() : null)}, NextDoor={NextDoor}]";
		}
	}
}

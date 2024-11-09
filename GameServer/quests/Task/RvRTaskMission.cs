using System;
using System.Collections;
using System.Collections.Generic;

using DOL.Events;
using DOL.GS.Keeps;

namespace DOL.GS.Quests
{
	public class RvRTaskMission : AbstractMission
	{
	

		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public RvRTaskMission(object owner)
			: base(owner)
		{

		}
					

		public override void FinishMission()
		{
			base.FinishMission();
			
		
		}

		public override void ExpireMission()
		{
			base.ExpireMission();
			
		}
		public static string MissionInfo = "";

		public override string Description
		{
			get
			{
				return "[RvR Mission] " + MissionInfo + "";
			}
		}
		//Findet magische Camps auf der "+ MissionInfo + " seite in New Frontiers und erobert sie. Bewacht sie solange, bis die Task beendet wurde.
		//Find magical camps on the "+ MissionInfo +" side in New Frontiers and capture them. Guard it until the task has ended. 
		public override long RewardMoney
		{
			get
			{
				return 25 * 100 * 100;
			}
		}

		public override long RewardRealmPoints
		{
			get
			{
				return 50;
			}
		}
		
		
	}
}
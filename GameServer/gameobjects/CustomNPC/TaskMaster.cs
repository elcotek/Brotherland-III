using System.Reflection;
using DOL.GS;
using DOL.GS.Quests;
using log4net;
using DOL.GS.ServerProperties;
namespace DOL.GS
{
    public class TaskMaster : GameNPC
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            //we need to disable them for players for now
            if (Properties.DISABLE_TASKDUNGEONS == true && player.Client.Account.PrivLevel == 1)
            {
                SayTo(player, "I'm sorry, Task Dungeons are currently disabled!");
                return true;
            }

            if (player.Mission == null)
                SayTo(player, "If you don't want to spend a long time looking for suitable [leveling areas], be it alone or in a group ?");
            else SayTo(player, "You already have a task that requires competion.");

            return true;
        }

        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str))
                return false;

            GamePlayer player = source as GamePlayer;
            if (player == null)
                return false;

            if (player.Mission != null)
                return false;

            switch (str.ToLower())
            {
                case "leveling areas":
                    {
                        SayTo(player, "It's a good idea to level up in my [task dungeons] that I created especially for You or your Group, or do you wish more infos about [tasks] ?");
                        break;
                    }
                case "tasks":
                    {
                        SayTo(player, "Unlike the tasks which you can receive from guards by using /whisper task when speaking to one, the taskmaster program is available to adventurers across a wide range of experience. You'll find taskmasters in many of our towns, ready to offer you the chance to aid the realm by confronting some of the monsters which inhabit a nearby dungeon. With the recent emergance of the new threat from beneath the Earth, as well as the ongoing war with the enemy realms, we need all the help that we can get.");
                        break;
                    }

                case "task dungeons":
                    {

                        SayTo(player, "Do you prefer [long corridors] or [labyrinth] like Dungeons?");
                        break;
                    }
                case "long corridors":
                    {

                        SayTo(player, "long corridors as you desire.");

                        if (player.Mission != null)
                            break;
                        //Can't if we already have one!

                        //Can't if group has one!
                        if (player.Group != null && player.Group.Mission != null)
                            break;
                        log.Info("INFO: TaskMaster Dungeons activated");
                        TaskDungeonMission mission;
                        if (player.Group != null)
                            mission = new TaskDungeonMission(player.Group, TaskDungeonMission.eDungeonType.Ranged);
                        else
                        {
                            mission = new TaskDungeonMission(player, TaskDungeonMission.eDungeonType.Ranged);
                            player.Mission = mission;
                        }
                        /*
                         * Very well Gwirenn, it's good to see adventurers willing to help out the realm in such times.  Dralkden the Thirster has taken over the caves to the south and needs to be disposed of.  Good luck!
                         * Very well Gwirenn, it's good to see adventurers willing to help out the realm in such times. Clear the caves to the south of creatures. Good luck!
                         */
                        string msg = "Very well " + player.Name + ", it's good to see adventurers willing to help out the realm in such times.";
                        switch (mission.TDMissionType)
                        {
                            case TaskDungeonMission.eTDMissionType.Clear:
                                msg += " Clear the nearest instance of creatures. Good luck!";//" Clear " + mission.TaskRegion.Description + " of creatures. Good luck!";
                                break;
                            case TaskDungeonMission.eTDMissionType.Boss:
                                msg += " " + mission.BossName + " has taken over " + mission.TaskRegion.Description + " and needs to be disposed of. Good luck!";
                                break;
                            case TaskDungeonMission.eTDMissionType.Specific:
                                msg += " Please remove " + mission.Total + " " + mission.TargetName + " from " + mission.TaskRegion.Description + "! The entrance is nearby.";
                                break;
                        }
                        SayTo(player, msg);
                        break;
                    }

                case "labyrinth":
                    {

                        SayTo(player, "labyrinth like as you desire.");

                        if (player.Mission != null)
                            break;
                        //Can't if we already have one!

                        //Can't if group has one!
                        if (player.Group != null && player.Group.Mission != null)
                            break;
                        log.Info("INFO: TaskMaster Dungeons activated");
                        TaskDungeonMission mission;
                        if (player.Group != null)
                            mission = new TaskDungeonMission(player.Group, TaskDungeonMission.eDungeonType.Melee);
                        else
                        {
                            mission = new TaskDungeonMission(player, TaskDungeonMission.eDungeonType.Melee);
                            player.Mission = mission;
                        }
                        /*
                         * Very well Gwirenn, it's good to see adventurers willing to help out the realm in such times.  Dralkden the Thirster has taken over the caves to the south and needs to be disposed of.  Good luck!
                         * Very well Gwirenn, it's good to see adventurers willing to help out the realm in such times. Clear the caves to the south of creatures. Good luck!
                         */
                        string msg = "Very well " + player.Name + ", it's good to see adventurers willing to help out the realm in such times.";
                        switch (mission.TDMissionType)
                        {
                            case TaskDungeonMission.eTDMissionType.Clear:
                                msg += " Clear the nearest instance of creatures. Good luck!";//" Clear " + mission.TaskRegion.Description + " of creatures. Good luck!";
                                break;
                            case TaskDungeonMission.eTDMissionType.Boss:
                                msg += " " + mission.BossName + " has taken over " + mission.TaskRegion.Description + " and needs to be disposed of. Good luck!";
                                break;
                            case TaskDungeonMission.eTDMissionType.Specific:
                                msg += " Please remove " + mission.Total + " " + mission.TargetName + " from " + mission.TaskRegion.Description + "! The entrance is nearby.";
                                break;
                        }
                        SayTo(player, msg);
                        break;
                    }

            }
            return true;
        }
    }
}

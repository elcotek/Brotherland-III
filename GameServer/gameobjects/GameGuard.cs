using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    /// <summary>
    /// Represents a guard NPC in the game that interacts with players and helps them.
    /// </summary>
    public class GameGuard : GameNPC
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initializes a new instance of the <see cref="GameGuard"/> class.
        /// </summary>
        public GameGuard()
            : base()
        {
            m_ownBrain = new GuardBrain
            {
                Body = this
            };
        }

        /// <summary>
        /// Prevents the guard from dropping loot when it dies.
        /// </summary>
        /// <param name="killer">The object that killed the guard.</param>
        public override void DropLoot(GameObject killer)
        {
            // Guards don't drop loot when they die
        }

        /// <summary>
        /// Gets a list of examination messages for the specified player.
        /// </summary>
        /// <param name="player">The player requesting the examination messages.</param>
        /// <returns>A list of messages for the player.</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            var translations = LanguageMgr.GetTranslation(player.Client.Account.Language,
                "GameGuard.GetExamineMessages.Examine", GetName(0, true), GetPronoun(0, true), GetAggroLevelString(player, false));

            return new ArrayList { translations };
        }

        protected static int tx1, ty1, tz1;
        protected static int tx0, ty0, tz0;
        static GamePlayer player = null;
        static short speed = 191;
        static GameGuard targetGuard = null;
        static string Searchname = "";
        string some = "";
        string more = "";
        protected static List<string> mobobjList = new List<string>(40);

        /// <summary>
        /// Allows a player to interact with the guard.
        /// </summary>
        /// <param name="source">The player interacting with the guard.</param>
        /// <returns>True if the interaction was successful; otherwise, false.</returns>
        public override bool Interact(GamePlayer source)
        {
            if (!base.Interact(source)) return false;

            if (!(source is GamePlayer t)) return false;

            if (InCombat) return false;

            if (IsMoving)
            {
                StopMoving();
            }

            TargetObject = t;
            TurnTo(t, 20000);
            Emote(eEmote.Induct);
            t.Out.SendMessage($"{Name} says hello {t.Name} /whisper {{target name}}, tell me an npc name of this Region and I'll take you to him.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            return true;
        }

        /// <summary>
        /// Receives whispers from players and processes them.
        /// </summary>
        /// <param name="source">The source of the whisper.</param>
        /// <param name="str">The message received from the player.</param>
        /// <returns>True if the whisper was processed successfully; otherwise, false.</returns>
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;

            if (!(source is GamePlayer t)) return false;

            targetGuard = this;
            int count = 0;
            Point3D point = null;
            Point3D point0 = null;
            Point3D point1 = null;
            Point3D point2 = null;

            int distance0toplayer = 0;
            int distance1toplayer = 0;
            int distancetoMob2 = 0;
            bool calc = false;

            player = t;

            if (string.IsNullOrWhiteSpace(str))
                return false;

            int maxreturn = 10; // Default
            if (int.TryParse(str, out var tempReturn))
            {
                maxreturn = Math.Min(60, tempReturn); // Limit to avoid overflow
            }

            mobobjList.Clear(); // Clear the object list

            if (string.Equals(str, Name, StringComparison.OrdinalIgnoreCase))
            {
                player.Out.SendMessage($"{Name} says, here I am.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Emote(eEmote.Wave);
                return false;
            }
            if (InCombat && TargetObject != null)
            {
                player.Out.SendMessage($"{Name} says, ask me again when I'm not fighting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (IsMoving || InCombat || (TargetObject != null && TargetObject != player))
            {
                player.Out.SendMessage($"{Name} says, Sorry, a guard is already on the way!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            // Searches for mobs based on the whispered name.
            var mobsFirst = GameServer.Database.SelectObjects<Mob>("`Name` LIKE @Name", new QueryParameter("@Name", str)).FirstOrDefault();
            var mobs = GameServer.Database.SelectObjects<Mob>("`Name` LIKE @Name", new QueryParameter("@Name", str)).ToList();

            foreach (var mob in mobs)
            {
                if (mobsFirst != null && mob.Region == CurrentRegionID && mobsFirst.Region == mob.Region && mob.Name == mobsFirst.Name)
                {
                    Searchname = mobsFirst.Name;

                    if (!mobobjList.Contains(mob.ObjectId))
                    {
                        mobobjList.Add(mob.ObjectId);
                    }

                    count = mobobjList.Count(obj => obj != null);

                    if (count == mobobjList.Count)
                    {
                        point0 = new Point3D(mobsFirst.X, mobsFirst.Y, mobsFirst.Z);
                        point1 = new Point3D(mob.X, mob.Y, mob.Z);

                        distance0toplayer = GetDistanceTo(point0);
                        distance1toplayer = GetDistanceTo(point1);

                        var distancetoPlayer = Math.Min(distance0toplayer, distance1toplayer);

                        if (distancetoPlayer == distance0toplayer)
                        {
                            tx0 = mobsFirst.X;
                            ty0 = mobsFirst.Y;
                            tz0 = mobsFirst.Z;
                        }
                        if (distancetoPlayer == distance1toplayer)
                        {
                            tx0 = mob.X;
                            ty0 = mob.Y;
                            tz0 = mob.Z;
                        }
                    }
                }
                else
                {
                    calc = false;
                }

                var mobs3 = GameServer.Database.SelectObjects<Mob>("`Name` LIKE @Name", new QueryParameter("@Name", str));

                foreach (var mob3 in mobs3)
                {
                    if (mob3.Region == CurrentRegionID)
                    {
                        point2 = new Point3D(mob3.X, mob3.Y, mob3.Z);

                        distancetoMob2 = GetDistanceTo(point2);
                        var distancetoMob = Math.Min(distance0toplayer, distancetoMob2);

                        if (distancetoMob == distancetoMob2)
                        {
                            tx0 = mob3.X;
                            ty0 = mob3.Y;
                            tz0 = mob3.Z;
                            calc = true;
                        }
                        else
                        {
                            calc = true;
                        }
                    }
                }
            }

            if (calc)
            {
                some = mobobjList.Count > 1 ? "some" : "";
                more = mobobjList.Count > 1 ? "'s" : "";

                player.Out.SendMessage($"{Name} says, you can find {some} ({Searchname}) {more} in this direction, I can show you the place, just follow me.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage($"{Name} says, you can find {some} ({Searchname}) {more} in this direction, I can show you the place, just follow me.", eChatType.CT_Send, eChatLoc.CL_ChatWindow);

                point = new Point3D(tx0, ty0, tz0);
                int distance = GetDistanceTo(point);

                if (MaxDistance <= distance)
                {
                    MaxDistance = distance;
                    Flags |= GameNPC.eFlags.PEACE;
                }

                if (speed < 191)
                    speed = 191;

                if (IsMoving)
                {
                    StopMoving();
                }

                if (!IsMoving)
                {
                    tx1 = tx0;
                    ty1 = ty0;
                    tz1 = tz0;

                    PathTo(tx0, ty0, tz0, speed);
                }
                else
                {
                    tx1 = tx0;
                    ty1 = ty0;
                    tz1 = tz0;
                    StopMoving();
                    if (!IsMoving)
                    {
                        PathTo(tx0, ty0, tz0, speed);
                    }
                }
                NPCWalkToTarget(this);
            }
            else
            {
                Emote(eEmote.No);
                t.Out.SendMessage($"{Name} says, No matches found in this region.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

            return true;
        }

        public const string foundMessage = "FOUNDMESSAGE";

        /// <summary>
        /// Makes the NPC walk to the target location and display a message when reached.
        /// </summary>
        /// <param name="npc">The NPC that is walking.</param>
        public static void NPCWalkToTarget(GameNPC npc)
        {
            bool targetReached = false;
            Point3D point = null;

            if (player != null && targetGuard != null && targetGuard == npc)
            {
                point = new Point3D(tx1, ty1, tz1);
                int distance = npc.GetDistanceTo(point);

                if (distance < 100)
                {
                    if (!npc.TempProperties.getProperty<bool>(foundMessage))
                    {
                        player.Out.SendMessage($"{npc.Name} says, look here is ({Searchname})!.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        player.Out.SendMessage($"{npc.Name} says, look here is ({Searchname})!.", eChatType.CT_Send, eChatLoc.CL_SystemWindow);
                        npc.TempProperties.setProperty(foundMessage, true);
                    }

                    npc.CurrentSpeed = 20;

                    if (distance < 90)
                    {
                        targetReached = true;
                        speed--;

                        if (speed <= 10)
                        {
                            speed = 5;
                            npc.WalkToSpawn();
                        }
                        if (distance < 50)
                        {
                            if (distance < 20)
                            {
                                speed = 5;

                                if (speed == 5)
                                {
                                    npc.Emote(eEmote.Wave);
                                    npc.TempProperties.removeProperty(foundMessage);
                                    speed++;
                                }
                            }

                            if (distance > 20)
                            {
                                npc.Emote(eEmote.Wave);
                                if (speed < 150)
                                    speed = 150;
                                npc.MaxDistance = 2800;
                                npc.WalkToSpawn();
                                targetGuard = null;

                                if (distance > 1500)
                                {
                                    npc.BroadcastUpdate();
                                }
                            }
                        }
                    }
                }

                if (targetReached)
                {
                    npc.MaxDistance = 2800;
                    npc.BroadcastUpdate();
                    targetReached = false;
                }
            }
        }

        /// <summary>
        /// Starts an attack against a specified target, notifying players nearby.
        /// </summary>
        /// <param name="attackTarget">The target to attack.</param>
        public override void StartAttack(GameObject attackTarget)
        {
            base.StartAttack(attackTarget);

            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
            {
                if (player != null)
                {
                    switch (Realm)
                    {
                        case eRealm.Albion:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Albion.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        case eRealm.Midgard:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Midgard.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                        case eRealm.Hibernia:
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameGuard.Hibernia.StartAttackSay"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            break;
                    }
                }
            }
        }
    }
}

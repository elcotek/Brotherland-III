using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.Spells;
using log4net;
using System.Reflection;

namespace DOL.AI.Brain
{
    /// <summary>
    /// Brain Class for Area Capture Guards
    /// </summary>
    public class KeepGuardBrain : StandardMobBrain
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public const string ReturnToSpawn = "RETURNTOSPAWN";

        public GameKeepGuard guard;
        /// <summary>
        /// Constructor for the Brain setting default values
        /// </summary>
        public KeepGuardBrain()
            : base()
        {
            base.AggroLevel = 60;
            base.AggroRange = 1200;
        }

        public void SetAggression(int aggroLevel, int aggroRange)
        {
            AggroLevel = aggroLevel;
            AggroRange = aggroRange;
        }

        public override int ThinkInterval
        {
            get
            {
                return 1500;
            }
        }


        /// <summary>
        /// Calculate the aggro range for ranged attacks
        /// </summary>
        /// <param name="guard"></param>
        /// <returns></returns>
         public override int CalculateAggroRange(GameNPC guard)
        {
            int newAggroRange = AggroRange;
            GameSpellEffect nearsightSepell = SpellHandler.FindEffectOnTarget(guard, "Nearsight");

            if ((guard is GuardLord || guard is GuardArcher || guard is GuardHealer || guard is GuardStaticArcher || guard is GuardStaticCaster) && nearsightSepell != null)
            {
                newAggroRange -= (int)(((int)nearsightSepell.Spell.Value * AggroRange) / 100);

                if (newAggroRange > 0)
                    //log.ErrorFormat("Aggro range: {0}", newAggroRange);
                    return newAggroRange;
            }
           
            return AggroRange;
        }


        /// <summary>
        /// Extra update for BGs and Caledonia
        /// </summary>
        /// <param name="guard"></param>
        public virtual void GuardEarlyUpdates(GameKeepGuard guard)
        {
            const string SpamTime = "NOSPAMFORGUARDUPDATES";
            //Extra update for BGs
            if ((guard.CurrentRegion.IsBG || guard.CurrentRegion.IsRvR) && guard.CurrentRegionID != 163)// && guard.CurrentRegionID != 163)//&& !guard.CurrentRegion.IsDungeon && !guard.CurrentRegion.IsFrontier)
            {
                
                foreach (GameClient clients in WorldMgr.GetClientsOfRegion(guard.CurrentRegionID))
                {
                    if (clients != null && clients.Player.CurrentRegionID == guard.CurrentRegionID)
                    {
                        long UPDATETICK = guard.TempProperties.getProperty<long>(SpamTime);
                        long changeTime = guard.CurrentRegion.Time - UPDATETICK;

                        if (changeTime > 20 * 1000 || UPDATETICK == 0)
                        {
                            guard.TempProperties.setProperty(SpamTime, guard.CurrentRegion.Time);

                            if (clients.Player.IsWithinRadius(guard, 2096) == false)
                            {
                               
                                clients.Player.Client.Out.SendObjectUpdate(guard);
                                //log.ErrorFormat("Guard update: {0} ", guard.Name);
                            }
                        }
                    }
                }
            }

        }
        /// <summary>
        /// Actions to be taken on each Think pulse
        /// </summary>
        public override void Think()
        {
            GameLiving npc = null;

            if (guard == null)
                guard = Body as GameKeepGuard;
            if (guard == null)
            {
                Stop();
                return;
            }


            GuardEarlyUpdates(guard);

            if (guard.BodyType == 1576 && guard.TargetObject != null && guard != null && (guard.InCombat || HasAggro || guard.HealthPercent < 100) && guard is GameKeepGuard && guard.PathID != "")
            {
                
                StandardMobBrain brain = Body.Brain as StandardMobBrain;

               
                if (((GameKeepGuard)guard).Attackers.Contains(guard.TargetObject)) 
                {

                    //log.ErrorFormat("targetobject vorhanden =  {0}", guard.TargetObject.Name);

                    guard.StopFollowing();
#pragma warning disable CS0618 // Typ oder Element ist veraltet
                    guard.WalkTo(guard.TargetObject.X, guard.TargetObject.Y, guard.TargetObject.Z, guard.MaxSpeed);
#pragma warning restore CS0618 // Typ oder Element ist veraltet
                    guard.StartAttack(guard.TargetObject);
                }
                else
                    brain.ClearAggroList();
                   Body.TargetObject = null;
                   Body.MoveOnPath(225);
            }

            //schleichende Spieler oder kein Target soll zurück zum spawn
            if (guard != null && HasAggro == false && guard.InCombat == false && ((GameKeepGuard)guard).Attackers.Count == 0 && guard.PathID == "" && (guard is GuardArcher || guard is GuardCaster || guard is GuardStaticCaster || guard is GameKeepGuard))
            {
                


                int distance = guard.GetDistanceTo(guard.SpawnPoint);

                if (guard.InCombat == false && distance > 50)
                {
                    guard.TempProperties.setProperty(ReturnToSpawn, guard);
                }

                if (guard.TempProperties.getProperty<GameNPC>(ReturnToSpawn) != null)
                { 
                    guard.WalkToSpawn();
                   
                    guard.Heading = guard.SpawnHeading;
                    //log.ErrorFormat("SpawnPoint distance2: {0}", distance);

                    if (guard.Heading == guard.SpawnHeading)
                    {
                        guard.TempProperties.removeProperty(ReturnToSpawn);
                        //log.ErrorFormat("ok heading: {0}, guard.SpawnHeading: {1}", guard.Heading, guard.SpawnHeading);
                    }
                }
            }

            if (guard != null && HasAggro && (guard is GuardArcher || guard is GuardCaster || guard is GuardStaticCaster || guard is GameKeepGuard && guard.PathID != "" || guard is GuardLord && guard.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance))
            {
                if (guard.TargetObject != null && (guard.IsWithinRadius(Body.TargetObject, CalculateAggroRange(guard) + 100) == false))
                {
                   //log.DebugFormat("targetobject vorhanden =  {0}", guard.TargetObject);


                    npc = Body;
                    if (npc != null)
                    {
                        {
                            //Keep patrols
                            if (((GameKeepGuard)npc).LoadedFromScript && ((GameKeepGuard)npc).BodyType == 1576)
                            {
                                if (Body.TargetObject != null && Body.InCombat == false && HasAggro == false)
                                {
                                    Body.StopAttack();

                                    StandardMobBrain brain = Body.Brain as StandardMobBrain;


                                    if (brain != null && Body.TargetObject != null && HasAggro == false && Body.InCombat == false)
                                    {
                                        brain.ClearAggroList();
                                        Body.TargetObject = null;
                                        Body.MoveOnPath(225);
                                        //log.Error("patrol stop attack");
                                        return;
                                    }
                                }
                            }
                            else if (Body.TargetObject != null && HasAggro == false)
                            {
                                // log.ErrorFormat("try  removed attacker {0} , Class = {1}", Body.Brain, Body);
                                if (((GameKeepGuard)guard).Attackers.Contains(guard.TargetObject))
                                    //log.Error("attacker found and removed");
                                    ((GameKeepGuard)npc).RemoveAttacker(npc.TargetObject);

                                // Body.StopAttack();
                            }
                        }
                    }
                }
            }

           
            if (guard is GuardArcher || guard is GuardLord)
            {
                if (guard.AttackState && guard.CanUseRanged && guard.ActiveWeaponSlot != GameLiving.eActiveWeaponSlot.Distance)
                {
                    guard.SwitchToRanged(guard.TargetObject);
                }
            }
            

            //if we are not doing an action, let us see if we should move somewhere
            if (guard != null && guard.CurrentSpellHandler == null && !guard.IsMoving && !guard.AttackState && !guard.InCombat)
            {
                // Tolakram - always clear the aggro list so if this is done by mistake the list will correctly re-fill on next think
                ClearAggroList();

                if (guard.BodyType != 1576 && guard.GetDistanceTo(guard.SpawnPoint, 0) > 50)
                {
                    guard.WalkToSpawn();
                }
                if (guard.BodyType == 1576)
                    guard.MoveOnPath(guard.MaxSpeed);
            }
            //Eden - Portal Keeps Guards max distance
            if (guard != null && guard.Level > 200 && !guard.IsWithinRadius(guard.SpawnPoint, 2000))
            {
                ClearAggroList();
                guard.WalkToSpawn();
            }
            else if (guard != null && guard.InCombat == false && guard.IsWithinRadius(guard.SpawnPoint, 6000) == false)
            {
                ClearAggroList();
                if (guard.BodyType != 1576)
                {
                    guard.WalkToSpawn();
                }
                if (guard.BodyType == 1576)
                    guard.MoveOnPath(guard.MaxSpeed);
            }
            if (guard.InCombat && guard.IsMovingOnPath)
            {
                CheckPlayerAggro();
                CheckNPCAggro();

                Body.StopMovingOnPath(); //OnPath();
                AttackMostWanted();
            }
            // We want guards to check aggro even when they are returning home, which StandardMobBrain does not, so add checks here
            if (guard != null && guard.CurrentSpellHandler == null && !guard.AttackState && !guard.InCombat && guard.ObjectState == GameObject.eObjectState.Active)
            {

                CheckPlayerAggro();
                CheckNPCAggro();

                if (HasAggro && guard.IsMovingOnPath && guard.BodyType == 1576)
                {
                    guard.StopMovingOnPath(); //OnPath();
                    AttackMostWanted();
                }

                if (guard != null && HasAggro && guard.IsReturningHome && (guard is GuardArcher || guard is GuardCaster))
                {

                    if (guard.TargetObject != null && guard.TargetObject.IsWithinRadius(guard, CalculateAggroRange(guard)))
                    {
                        guard.StopMoving();
                        Body.CancelWalkToSpawn();
                    }
                    if (guard.TargetObject != null && guard.TargetObject.IsWithinRadius(guard, CalculateAggroRange(guard)) == false)
                    {
                        Body.TurnTo(Body.SpawnPoint.X, Body.SpawnPoint.Y);
                        return;
                    }
                    AttackMostWanted();
                }
            }
            base.Think();
        }
        public const int StealthLore = 30204;
        bool detected = false;
        /// <summary>
        /// Check Area for Players to attack
        /// </summary>
        protected override void CheckPlayerAggro()
        {
           // if (Body.AttackState || Body.CurrentSpellHandler != null) return;
           
            if (HasAggro || Body.CurrentSpellHandler != null) return;

            foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {


                if (player == null) continue;
                if (GameServer.ServerRules.IsAllowedToAttack(Body, player, true))
                {

                    //Guard patrol
                    if (player.Z - 50 > Body.Z && Body.PathID != "")
                        continue;

                    /*
                    //Los Check
                    if (Body.AllowedToAttackTarget() == false)
                    {
                        continue;
                    }
                    */

                    if (!Body.IsWithinRadius(player, CalculateAggroRange(guard)))
                        continue;
                    if ((Body as GameKeepGuard).Component != null && !KeepMgr.IsEnemy(Body as GameKeepGuard, player, true))
                        continue;
                    if (Body is GuardStealther == false && player.IsStealthed)
                        continue;


                    if (Body is GuardStealther && player is GamePlayer && (player as GamePlayer).IsStealthed && Body.IsWithinRadius(player, 300) == false)
                    {
                        detected = false;
                        continue;
                    }


                    if (detected == false && Body is GuardStealther && player is GamePlayer && (player as GamePlayer).IsStealthed && Body is GuardStealther && player.EffectList.GetOfType<CamouflageEffect>() == null)
                    {
                        detected = true;
                        Body.CastSpell(SkillBase.GetSpellByID(StealthLore), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        (player as GamePlayer).Stealth(false);
                    }
                    if (detected == false && Body is GuardStealther && player is GamePlayer && (player as GamePlayer).IsStealthed && Body is GuardStealther && Body.IsWithinRadius(player, 150) && player.EffectList.GetOfType<CamouflageEffect>() != null)
                    {
                        detected = true;
                        Body.CastSpell(SkillBase.GetSpellByID(StealthLore), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                        (player as GamePlayer).Stealth(false);
                    }

                    if (Body is GuardStealther && Body.IsWithinRadius(player, 150) == false && player.EffectList.GetOfType<CamouflageEffect>() != null)
                        continue;

                    //By Elcotek if player in front ? 
                    if (Body.Brain is ControlledNpcBrain == false && !Body.IsObjectInFront(player, 225) && Body.HealthPercent == 100 && !Body.IsWithinRadius(player, 50))
                        continue;

                    WarMapMgr.AddGroup((byte)player.CurrentZone.ID, player.X, player.Y, player.Name, (byte)player.Realm);

                    if (DOL.GS.ServerProperties.Properties.ENABLE_DEBUG)
                    {
                        Body.Say("Want to attack player " + player.Name);
                    }

                    //AddToAggroList(player, player.EffectiveLevel << 1, true);
                    AddToAggroList(player, 1, true);
                    return;
                }
            }
        }




        /// <summary>
        /// Check area for NPCs to attack
        /// </summary>
        protected override void CheckNPCAggro()
        {
            //if (Body.AttackState || Body.CurrentSpellHandler != null) return;

            if (HasAggro || Body.CurrentSpellHandler != null) return;

            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)CalculateAggroRange(guard)))
            {
                if (npc == null || npc.Brain == null || npc is GameKeepGuard || npc.Brain is IControlledBrain == false || npc.ObjectState != GameObject.eObjectState.Active)
                    continue;

                GamePlayer player = (npc.Brain as IControlledBrain).GetPlayerOwner();

                if (player == null || player.ObjectState != GameObject.eObjectState.Active)
                    continue;

                if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
                {
                    //Guard patrol
                    if (npc.Z - 50 > Body.Z && Body.PathID != "")
                        continue;

                    //Los Check
                   


                    if ((Body as GameKeepGuard).Component != null && !KeepMgr.IsEnemy(Body as GameKeepGuard, player, true))
                    {
                        continue;
                    }

                    WarMapMgr.AddGroup((byte)player.CurrentZone.ID, player.X, player.Y, player.Name, (byte)player.Realm);

                    if (DOL.GS.ServerProperties.Properties.ENABLE_DEBUG)
                    {
                        Body.Say("Want to attack player " + player.Name + " pet " + npc.Name);
                    }

                    //AddToAggroList(npc, (npc.Level + 1) << 1, true);

                    if (npc.Brain is ControlledNpcBrain) // This is a pet or charmed creature, checkLOS
                        AddToAggroList(npc, 1, true);
                    else
                        AddToAggroList(npc, 1);

                    return;
                }
            }
        }


        public override int CalculateAggroLevelToTarget(GameLiving target)
        {
            GamePlayer checkPlayer = null;
            if (target is GameNPC && (target as GameNPC).Brain is IControlledBrain)
                checkPlayer = ((target as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
            if (target is GamePlayer)
                checkPlayer = target as GamePlayer;
            if (checkPlayer == null)
                return 0;
            if (KeepMgr.IsEnemy(Body as GameKeepGuard, checkPlayer, true))
                return AggroLevel;
            return 0;
        }

        public override bool AggroLOS
        {
            get { return true; }
        }
    }
}

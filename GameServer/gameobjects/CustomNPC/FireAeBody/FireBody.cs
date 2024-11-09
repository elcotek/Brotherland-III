/*
 * Elcotek: based on the work of ML10 Porter.
 * 
 * 
 * 
 */

using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.PacketHandler;
using log4net;
using System;


namespace DOL.GS.Scripts
{


    public class FireAEBody : GameNPC
    {
        public override bool AddToWorld()
        {

            this.SetOwnBrain(new FireBodyBrain());
            Name = "hot fire";
            Level = 36;
            Model = 2074;
            MaxSpeedBase = 0;
            Constitution = 5000;
            Race = 200;
            Dexterity = 65;
            Quickness = 60;
            return base.AddToWorld();
        }

        /*

        public AECaster()
            : base()
        {
            SetOwnBrain(new FireBodyBrain());

            Model = 2074;
            Size = 50;
        }
        */
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// Gets/sets the body of this brain
        /// </summary>
        /*
        public GameNPC Body
        {
            get { return m_body; }
            set { m_body = value; }
        }
        */
        /// <summary>
        /// The body of this brain
        /// </summary>
       // protected GameNPC m_body;


    }
}

namespace DOL.AI.Brain
{



    public class FireBodyBrain : StandardMobBrain
    {
        public FireBodyBrain()
            : base()
        {

            AggroLevel = 65;
            AggroRange = 300;
            ThinkInterval = 1500;
        }
        public override void Think()
        {

            foreach (GamePlayer player in Body.GetPlayersInRadius(600))
            {
                GamePlayer t = (GamePlayer)player;

                if (player == null) continue;
                if (player.Client.Account.PrivLevel >= (uint)ePrivLevel.GM) continue;
                if ((!Body.IsWithinRadius(player, (short)150))) continue;
                {
                    //casts the Spell
                    CheckGlare(player);

                }
                base.Think();
            }
        }

        private void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
        #region FireAESpell

        public const int FireSpell = 421;


        private const int m_FireAEChance = 99;

        /// <summary>
        /// Chance to cast Fire when a potential target has been detected.
        /// </summary>
        protected int AEFireChance
        {
            get { return m_FireAEChance; }
        }


        private GameLiving m_FireAETarget;


        /// <summary>
        /// The target for the next Fire attack.
        /// </summary>
        private GameLiving AETarget
        {
            get { return m_FireAETarget; }
            set { m_FireAETarget = value; PrepareToGlare(); }
        }
        /// <summary>
		/// Announce the Fire attack and start the 5 second timer.
		/// </summary>
		private void PrepareToGlare()
        {
            if (AETarget == null) return;
            Body.TurnTo(AETarget);
            //BroadcastMessage(String.Format(AETarget.Name + " Hit by Fire"));
            new RegionTimer(Body, new RegionTimerCallback(CastAE), 5000);
        }
        /// <summary>
		/// Cast glare on the target.
		/// </summary>
		/// <param name="timer">The timer that started this cast.</param>
		/// <returns></returns>
		private int CastAE(RegionTimer timer)
        {
            // Turn around to the target and cast glare, then go back to the original
            // target, if one exists.

            GameObject oldTarget = Body.TargetObject;
            Body.TargetObject = AETarget;
            Body.Z = Body.SpawnPoint.Z; // this is a fix to correct Z errors that sometimes happen.
            Body.TurnTo(AETarget);


            //Body.CastSpell(FireSpell, SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
            Body.CastSpell(SkillBase.GetSpellByID(FireSpell), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
            AETarget = null;
            if (oldTarget != null) Body.TargetObject = oldTarget;
            return 0;
        }
        /// <summary>
        /// Check whether or not to glare at this target.
        /// </summary>
        /// <param name="target">The potential target.</param>
        /// <returns>Whether or not the spell was cast.</returns>
        public bool CheckGlare(GameLiving target)
        {
            if (target == null || AETarget != null) return false;
            bool success = Util.Chance(m_FireAEChance);
            if (success)

                AETarget = target;
            return success;
        }

        #endregion

        /// <summary>
        /// Broadcast relevant messages to the raid.
        /// </summary>
        /// <param name="message">The message to be broadcast.</param>
        public void BroadcastMessage(String message)
        {
            foreach (GamePlayer player in Body.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
            {
                player.Out.SendMessage(message, eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
            }
        }

    }
}



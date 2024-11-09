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
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections;
using System.Numerics;

namespace DOL.GS
{
    /// <summary> 
    /// This class represents a static Item in the gameworld
    /// </summary>
    public class GameStaticItem : GameLiving, ITranslatableObject// war vorher GameObject

    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// The emblem of the Object
        /// </summary>
        protected int m_Emblem;

        /// <summary>
        /// The respawn interval of this world object
        /// </summary>
        protected int m_respawnInterval = 0;

        public int RespawnInterval
        {
            get { return m_respawnInterval; }
            set { m_respawnInterval = value; }
        }

        /// <summary>
        /// returns if this living is alive
        /// </summary>
        public override bool IsAlive
        {
            get { return HealthPercent > 0; }
        }
        /// <summary>
        /// Gets the Health in percent 0..100
        /// </summary>
        public override byte HealthPercent
        {
            get
            {
                return (byte)(MaxHealth <= 0 ? 0 : Health * 100 / MaxHealth);
            }
        }


        /// <summary>
        /// Constructs a new GameStaticItem
        /// </summary>
        public GameStaticItem() : base()
        {
            m_owners = new ArrayList(1);
        }



        #region Name/Model/GetName/GetExamineMessages
        /// <summary>
        /// gets or sets the model of this Item
        /// </summary>
        public override ushort Model
        {
            get { return base.Model; }
            set
            {
                base.Model = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
            }
        }

        /// <summary>
        /// gets or sets the health of this Item
        /// </summary>
        public override int Health
        {
            get { return base.Health; }
            set
            {
                base.Health = value;
               
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
                BroadcastUpdate();
            }
        }
        private int m_healthPercentOld = 100;

        /// <summary>
        /// The amount of health before the most recent attack.
        /// </summary>
        public int HealthPercentOld
        {
            get { return m_healthPercentOld; }
            protected set { m_healthPercentOld = value; }
        }


        #region timer


        /// <summary>
        /// Special Timer 
        /// </summary>
        protected RegionTimer m_SpecialAttackTimer;

        /// <summary>
        /// Starts the Change Level Timer
        /// </summary>
        public void StartSpecialEventTimer()
        {


            int newinterval = 12 * 60 * 1000;//12 Min

            if (m_SpecialAttackTimer.IsAlive)
            {
                int timeelapsed = m_SpecialAttackTimer.Interval - m_SpecialAttackTimer.TimeUntilElapsed;
                //if timer has run for more then we need, run event instantly
                if (timeelapsed > m_SpecialAttackTimer.Interval)
                    newinterval = 1;
                //change timer to the value we need
                else if (timeelapsed < newinterval)
                    newinterval = m_SpecialAttackTimer.Interval - timeelapsed;
                m_SpecialAttackTimer.Interval = newinterval;

            }
            m_SpecialAttackTimer.Stop();
            m_SpecialAttackTimer.Start(newinterval);
        }

        /// <summary>
        /// Stops the Special Timer 
        /// </summary>
        public void StopSpecialEventTimer()
        {
            m_SpecialAttackTimer.Stop();
            m_SpecialAttackTimer.Interval = 1;
        }
        /// <summary>
        /// Destroys the Special Timer 
        /// </summary>
        public void UnloadSpecialEventTimer()
        {
            if (m_SpecialAttackTimer != null)
            {
                m_SpecialAttackTimer.Stop();
                m_SpecialAttackTimer = null;
            }
        }

        protected void InitialiseSpecialEventTimer()
        {
            m_SpecialAttackTimer = new RegionTimer(CurrentRegion.TimeManager)
            {
                Callback = new RegionTimerCallback(SpecialEventTimerCallback)
            };

        }


        public int SpecialEventTimerCallback(RegionTimer timer)
        {

            if (IsObjectAlive == false)
            {
                RemoveFromWorld(RespawnInterval);
            }
            //Braunes Ei
            if (this != null && Emblem == 2251 && Model != 3740 && InCombat == false)
            {

                if (Model == 3751)
                {

                    Model = 3740;
                    StopSpecialEventTimer();
                    return 0;
                }


                if (Model == 3750)
                {
                    Model = 3740;
                    if (Model == 3740)
                        RemoveFromWorld(RespawnInterval);
                    StopSpecialEventTimer();

                }
                else
                    //Ist tot
                    Model = 3740;
                if (Model == 3740)
                    RemoveFromWorld(RespawnInterval);
                StopSpecialEventTimer();

            }

            //Blaues Ei
            if (this != null && Emblem == 2252 && Model != 3739 && InCombat == false)
            {


                if (Model == 3749)
                {

                    Model = 3739;
                    StopSpecialEventTimer();
                    return 0;
                }

                if (Model == 3748)
                {
                    Model = 3739;
                    if (Model == 3739)
                        RemoveFromWorld(RespawnInterval);
                    StopSpecialEventTimer();

                }
                else
                    //Ist tot
                    Model = 3739;
                if (Model == 3739)
                    RemoveFromWorld(RespawnInterval);
                StopSpecialEventTimer();

            }



            //schwarzes Ei
            if (this != null && Emblem == 2253 && Model != 3741 && InCombat == false)
            {

                if (Model == 3747)
                {

                    Model = 3741;
                    StopSpecialEventTimer();
                    return 0;
                }


                if (Model == 3746)
                {
                    Model = 3741;
                    if (Model == 3741)
                        RemoveFromWorld(RespawnInterval);
                    StopSpecialEventTimer();


                }
                else
                    //Ist tot
                    Model = 3741;
                if (Model == 3741)
                    RemoveFromWorld(RespawnInterval);
                StopSpecialEventTimer();

            }

            return 0;
        }


        #endregion

        #region Egg Event
        public const string ItemKiller = "ITEMKILLER";



        /// <summary>
        /// Take some amount of damage inflicted by another GameObject.
        /// </summary>
        /// <param name="source">The object inflicting the damage.</param>
        /// <param name="damageType">The type of damage.</param>
        /// <param name="damageAmount">The amount of damage inflicted.</param>
        /// <param name="criticalAmount">The critical amount of damage inflicted</param>
        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            GamePlayer player = null;
            SetModelFromHealth();
            base.TakeDamage(source, damageType, damageAmount, criticalAmount);

            //Get the the Attacker |
            player = GetPlayerAttacker(source as GameLiving);

            if (player != null)
            {
                TempProperties.setProperty(ItemKiller, player);
            }

            Notify(GameObjectEvent.TakeDamage, this,
                new TakeDamageEventArgs(source, damageType, damageAmount, criticalAmount));
        }

        private void GetReward()
        {
            ArrayList allGrpuplayers = new ArrayList();
            GamePlayer player = null;

            player = TempProperties.getProperty<GamePlayer>(ItemKiller);

            if (player != null)
            {
                if (player.Group != null)
                {

                    GamePlayer Players = null;
                    foreach (GamePlayer players in player.Group.GetMembersInTheGroup())
                    {
                        if (allGrpuplayers.Contains(players) == false)
                        {
                            allGrpuplayers.Add(players);

                        }
                        Players = players;
                        players.TempProperties.removeProperty(ItemKiller);

                        if (allGrpuplayers.Count == player.Group.GetMembersInTheGroup().Count)
                            break;
                    }


                    if (player.Group.GetMembersInTheGroup().Count > 1)
                    {
                        try
                        {
                            int selectedMamber = Util.Random(allGrpuplayers.Count - 1);
                            {

                                if (selectedMamber < 0)
                                    selectedMamber = 0;


                                if (allGrpuplayers[selectedMamber] != null)
                                {
                                    GetRewardForCategory(0, (allGrpuplayers[selectedMamber] as GamePlayer));
                                    TempProperties.setProperty(ItemKiller, (allGrpuplayers[selectedMamber] as GamePlayer));
                                    allGrpuplayers.Clear();
                                }
                            }

                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    //Reward
                    GetRewardForCategory(0, player);
                    player.TempProperties.removeProperty(ItemKiller);
                }
            }
        }

        public virtual void SetModelFromHealth()
        {


            if (ServerProperties.Properties.Enable_Caledonia_Event)
            {
                bool addsChance = Util.Chance(60);

                if (Emblem == 2251 || Emblem == 2252 || Emblem == 2253)
                {
                    StartSpecialEventTimer();

                    if (Emblem == 2251)
                    {
                        switch (Model)
                        {
                            case 3740:
                            case 3751:
                            case 3750:
                                {

                                    //Braunes Ei 2251
                                    if (HealthPercent < 90 && HealthPercent >= 20)
                                    {
                                        Model = 3751;
                                        //log.Error("Braunes Ei 1");
                                        BroadcastUpdate();
                                        break;

                                    }
                                    else if (HealthPercent <= 50)
                                    {
                                        //log.Error("Braunes Ei 2");
                                        Model = 3750;
                                        Health = 0;
                                        BroadcastUpdate();

                                        GetReward();

                                        if (addsChance)
                                        {
                                            //log.Error("drache!");
                                            Addspawn(this, 1105);//mini dragon alb
                                        }
                                        break;
                                    }
                                    //log.Error("Braunes Ei 3");
                                    Model = 3740;
                                    BroadcastUpdate();
                                    break;
                                }
                        }
                    }


                    if (Emblem == 2252)
                    {
                        switch (Model)
                        {

                            //Blaues Ei 2252
                            case 3739:
                            case 3749:
                            case 3748:
                                {
                                    //Blaues Ei
                                    if (HealthPercent < 90 && HealthPercent >= 20)
                                    {

                                        //log.Error("Blaues Ei 1");
                                        Model = 3749;
                                        BroadcastUpdate();
                                        break;
                                    }
                                    else if (HealthPercent <= 50)
                                    {
                                        //log.Error("Blaues Ei 2");
                                        Model = 3748;
                                        Health = 0;
                                        BroadcastUpdate();
                                        GetReward();

                                        if (addsChance)
                                        {
                                            //log.Error("drache!");
                                            Addspawn(this, 1106); //mini dragon mid
                                        }

                                        break;
                                    }
                                    //log.Error("Blaues Ei 3");
                                    Model = 3739;
                                    BroadcastUpdate();
                                    break;
                                }
                        }
                    }

                    if (Emblem == 2253)
                    {
                        switch (Model)
                        {
                            case 3741:
                            case 3747:
                            case 3746:
                                {
                                    //Schwarzes Ei 2253
                                    if (HealthPercent < 90 && HealthPercent >= 20)
                                    {
                                        //log.Error("schwarzes Ei 1");
                                        Model = 3747;
                                        BroadcastUpdate();
                                        break;
                                    }
                                    else if (HealthPercent <= 50)
                                    {
                                        //log.Error("schwarzes Ei 2");
                                        Model = 3746;
                                        Health = 0;
                                        BroadcastUpdate();

                                        GetReward();
                                        if (addsChance)
                                        {

                                            Addspawn(this, 1107); //mini dragon hib 
                                        }
                                        break;
                                    }
                                    //log.Error("schwarzes Ei 3");
                                    Model = 3741;
                                    BroadcastUpdate();
                                    break;
                                }
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// gets or sets the MaxHealth of this Item
        /// </summary>
        public override int MaxHealth
        {
            get { return base.MaxHealth; }
            set
            {
                base.MaxHealth = value;
                /*
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
                */
            }
        }

        /// <summary>
		/// Gets or Sets the current Emblem of the Object
		/// </summary>
		public virtual int Emblem
        {
            get { return m_Emblem; }
            set
            {
                m_Emblem = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
            }
        }

        public virtual LanguageDataObject.eTranslationIdentifier TranslationIdentifier
        {
            get { return LanguageDataObject.eTranslationIdentifier.eObject; }
        }

        /// <summary>
        /// The translation id
        /// </summary>
        protected string m_translationId = String.Empty;

        /// <summary>
        /// Gets or sets the translation id
        /// </summary>
        public string TranslationId
        {
            get { return m_translationId; }
            set { m_translationId = (value == null ? "" : value); }
        }

        /// <summary>
        /// Gets or sets the name of this item
        /// </summary>
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
            }
        }

        /// <summary>
        /// Holds the examine article
        /// </summary>
        private string m_examineArticle = String.Empty;
        /// <summary>
        /// Gets or sets the examine article
        /// </summary>
        public string ExamineArticle
        {
            get { return m_examineArticle; }
            set { m_examineArticle = (value == null ? "" : value); }
        }

        private bool m_loadedFromScript = true;
        public bool LoadedFromScript
        {
            get { return m_loadedFromScript; }
            set { m_loadedFromScript = value; }
        }



        /// <summary>
        /// Returns name with article for nouns
        /// </summary>
        /// <param name="article">0=definite, 1=indefinite</param>
        /// <param name="firstLetterUppercase">Forces the first letter of the returned string to be uppercase</param>
        /// <returns>name of this object (includes article if needed)</returns>
        public override string GetName(int article, bool firstLetterUppercase)
        {
            if (Name == String.Empty)
                return String.Empty;

            if (char.IsUpper(Name[0]))
            {
                // proper name

                if (firstLetterUppercase)
                    return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStaticItem.GetName.Article1", Name);
                else
                    return LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "GameStaticItem.GetName.Article2", Name);
            }
            else
            {
                // common noun
                return base.GetName(article, firstLetterUppercase);
            }
        }

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            Level = base.Level;
           
            //Level = base.Level;
            IList list = new ArrayList(4);
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameObject.GetExamineMessages.YouTarget", GetName(0, false)));
            return list;
        }
        /*
        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {

            IList list = base.GetExamineMessages(player);
            list.Insert(0, "You select " + GetName(0, false) + ".");
            base.Level = Level;
           
            return list;
        }
        */

        #endregion

        public override void LoadFromDatabase(DataObject obj)
        {
            WorldObject item = obj as WorldObject;
            base.LoadFromDatabase(obj);

            m_loadedFromScript = false;
            CurrentRegionID = item.Region;
            TranslationId = item.TranslationId;
            Name = item.Name;
            Model = item.Model;
            Level = item.Level;

            Emblem = item.Emblem;
            Realm = (eRealm)item.Realm;
            Heading = item.Heading;
            Health = item.Health;
            MaxHealth = item.MaxHealth;


            if (Emblem == 2251 || Emblem == 2252 || Emblem == 2253)
            {
                if (ServerProperties.Properties.Enable_Caledonia_Event)
                {

                    int loc = Util.Random(1, 3);

                    switch (loc)
                    {
                        case 1:
                            {
                                //Caledonia unten:
                                X = Util.Random(13867, 65804);
                                Y = Util.Random(44511, 49733);
                                Heading = (ushort)Util.Random(1, 4096);
                                break;
                            }
                        case 2:
                            {
                                //Caledonia oben links:
                                X = Util.Random(21120, 37522);
                                Y = Util.Random(12576, 31757);
                                Heading = (ushort)Util.Random(1, 4096);
                                break;
                            }
                        case 3:
                            {
                                //Caledonia oben rechts:
                                X = Util.Random(44413, 65437);
                                Y = Util.Random(27866, 49507);
                                Heading = (ushort)Util.Random(1, 4096);
                                break;
                            }
                    }


                    Z = 4644;
                    RespawnInterval = item.RespawnInterval;
                }
            }
            else

                X = item.X;
            Y = item.Y;
            Z = item.Z;
            RespawnInterval = item.RespawnInterval;
        }

        /// <summary>
        /// Gets or sets the heading of this item
        /// </summary>
        public override ushort Heading
        {
            get { return base.Heading; }
            set
            {
                base.Heading = value;
                if (ObjectState == eObjectState.Active)
                {
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the level of this item
        /// </summary>
        public override byte Level
        {
            get { return base.Level; }
            set
            {
                base.Level = value;

                if (ObjectState == eObjectState.Active)
                {
                   
                    foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                        player.Out.SendObjectCreate(this);
                }
                base.Level = Level;
                BroadcastUpdate();
            }
        }

     
        /// <summary>
        /// Gets or sets the realm of this item
        /// </summary>
        public override eRealm Realm
        {
            get { return base.Realm; }
            set
            {
                base.Realm = value;
            }
        }

        /// <summary>
        /// Saves this Item in the WorldObject DB
        /// </summary>
        public override void SaveIntoDatabase()
        {
            WorldObject obj = null;
            if (InternalID != null)
            {
                obj = (WorldObject)GameServer.Database.FindObjectByKey<WorldObject>(InternalID);
            }
            if (obj == null)
            {
                if (LoadedFromScript == false)
                {
                    obj = new WorldObject();
                }
                else
                {
                    return;
                }
            }
            obj.Name = Name;
            obj.Model = Model;
            obj.Level = Level;
            obj.Emblem = Emblem;
            obj.Realm = (byte)Realm;
            obj.Heading = Heading;
            obj.Health = Health;
            obj.MaxHealth = MaxHealth;
            obj.Region = CurrentRegionID;
            obj.X = X;
            obj.Y = Y;
            obj.Z = Z;
            obj.ClassType = this.GetType().ToString();
            obj.RespawnInterval = RespawnInterval;

            if (InternalID == null)
            {
                GameServer.Database.AddObject(obj);
                InternalID = obj.ObjectId;
            }
            else
            {
                GameServer.Database.SaveObject(obj);
            }
        }

        public override void Delete()
        {
            Notify(GameObjectEvent.Delete, this);
            RemoveFromWorld(0); // will not respawn
            ObjectState = eObjectState.Deleted;
        }

        /// <summary>
        /// Deletes this item from the WorldObject DB
        /// </summary>
        public override void DeleteFromDatabase()
        {
            if (InternalID != null)
            {
                WorldObject obj = (WorldObject)GameServer.Database.FindObjectByKey<WorldObject>(InternalID);
                if (obj != null)
                    GameServer.Database.DeleteObject(obj);
            }
            InternalID = null;
        }

        /// <summary>
        /// Called to create an item in the world
        /// </summary>
        /// <returns>true when created</returns>
        public override bool AddToWorld()
        {
            if (!base.AddToWorld()) return false;


            if (ServerProperties.Properties.Enable_Caledonia_Event)
            {
                if (Emblem == 2251 || Emblem == 2252 || Emblem == 2253)
                {
                    //We need a actual z from the zone to spawn the items in a random position
                    Vector3 zonepoint = new Vector3(X, Y, Z);
                    Zone zone = (this as GameLiving).CurrentZone.ZoneRegion.GetZone(zonepoint);

                    if (zone != null)
                    {
                        //log.ErrorFormat("New Item Z:{0}", zone.GetZ(X, Y));
                        if (zone.GetZ(X, Y) != 0)

                            Z = zone.GetZ(X, Y);
                        if (Z < zone.Waterlevel)
                        {
                            Z = zone.Waterlevel;
                            //log.ErrorFormat("Benutze Wasserlevel: {0} von Z: {1} niediger: ", zone.Waterlevel, Z);
                        }

                    }
                    InitialiseSpecialEventTimer();
                }
            }

            Health = MaxHealth;
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                player.Out.SendObjectCreate(this);
            return true;
        }

        /// <summary>
        /// Called to remove the item from the world
        /// </summary>
        /// <returns>true if removed</returns>
        public override bool RemoveFromWorld()
        {
            return RemoveFromWorld(RespawnInterval);
        }

        public override void Die(GameObject killer)
        {
            if (killer != this)
                base.Die(killer);

            //Nicht für das Event!
            if (Emblem == 2250)
            {
                GamePlayer player = killer as GamePlayer;



                if (player is GamePlayer && player.Group != null)
                {
                    foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                    {

                        players.Out.SendMessage("The " + Name + " dies", eChatType.CT_Chat, eChatLoc.CL_SystemWindow);
                        RemoveFromWorld(RespawnInterval);
                    }
                }
                else
                {
                    player.Out.SendMessage("The " + Name + " dies", eChatType.CT_Chat, eChatLoc.CL_SystemWindow);
                    RemoveFromWorld(RespawnInterval);
                }
            }

        }

        /// <summary>
        /// Temporarily remove this static item from the world.
        /// Used mainly for quest interaction
        /// </summary>
        /// <param name="respawnSeconds"></param>
        /// <returns></returns>
        public virtual bool RemoveFromWorld(int respawnSeconds)
        {
            if (ObjectState == eObjectState.Active)
            {
                foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
                    player.Out.SendObjectRemove(this);
            }

            if (base.RemoveFromWorld())
            {


                if (respawnSeconds > 0)
                {
                    if (Emblem == 2251 || Emblem == 2252 || Emblem == 2253)
                    {
                        Health = MaxHealth;

                        /*
                        X = Util.Random(270578, 271197);
                        Y = Util.Random(539987, 540880);
                        Heading = (ushort)Util.Random(1, 4096);
                       */

                        int loc = Util.Random(1, 3);

                        switch (loc)
                        {
                            case 1:
                                {
                                    //Caledonia unten:
                                    X = Util.Random(13867, 65804);
                                    Y = Util.Random(44511, 49733);
                                    Heading = (ushort)Util.Random(1, 4096);
                                    break;
                                }
                            case 2:
                                {
                                    //Caledonia oben links:
                                    X = Util.Random(21120, 37522);
                                    Y = Util.Random(12576, 31757);
                                    Heading = (ushort)Util.Random(1, 4096);
                                    break;
                                }
                            case 3:
                                {
                                    //Caledonia oben rechts:
                                    X = Util.Random(44413, 65437);
                                    Y = Util.Random(27866, 49507);
                                    Heading = (ushort)Util.Random(1, 4096);
                                    break;
                                }
                        }

                    }


                    StartRespawn(Math.Max(1, respawnSeconds));
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Timer used to respawn this object
        /// </summary>
        protected RegionTimer m_respawnTimer = null;

        /// <summary>
        /// The sync object for respawn timer modifications
        /// </summary>
        protected readonly object m_respawnTimerLock = new object();

        /// <summary>
        /// Starts the Respawn Timer
        /// </summary>
        protected virtual void StartRespawn(int respawnSeconds)
        {
            lock (m_respawnTimerLock)
            {
                if (m_respawnTimer == null)
                {
                    m_respawnTimer = new RegionTimer(this)
                    {
                        Callback = new RegionTimerCallback(RespawnTimerCallback)
                    };
                    m_respawnTimer.Start(respawnSeconds * 5 * 1000);
                }
            }
        }

        /// <summary>
        /// The callback that will respawn this object
        /// </summary>
        /// <param name="respawnTimer">the timer calling this callback</param>
        /// <returns>the new interval</returns>
        protected virtual int RespawnTimerCallback(RegionTimer respawnTimer)
        {
            lock (m_respawnTimerLock)
            {
                if (m_respawnTimer != null)
                {
                    m_respawnTimer.Stop();
                    m_respawnTimer = null;
                    AddToWorld();
                }
            }

            return 0;
        }


        /// <summary>
        /// Holds the owners of this item, can be more than 1 person
        /// </summary>
        private readonly ArrayList m_owners;
        /// <summary>
        /// Adds an owner to this item
        /// </summary>
        /// <param name="player">the object that is an owner</param>
        public void AddOwner(GameObject player)
        {
            lock (m_owners)
            {
                foreach (WeakReference weak in m_owners)
                    if (weak.Target == player) return;
                m_owners.Add(new WeakRef(player));
            }
        }
        /// <summary>
        /// Tests if a specific gameobject owns this item
        /// </summary>
        /// <param name="testOwner">the owner to test for</param>
        /// <returns>true if this object owns this item</returns>
        public bool IsOwner(GameObject testOwner)
        {
            lock (m_owners)
            {
                //No owner ... return true
                if (m_owners.Count == 0) return true;

                foreach (WeakReference weak in m_owners)
                    if (weak.Target == testOwner) return true;
                return false;
            }
        }

        /// <summary>
        /// Returns an array of owners
        /// </summary>
        public GameObject[] Owners
        {
            get
            {
                ArrayList activeOwners = new ArrayList();
                foreach (WeakReference weak in m_owners)
                    if (weak.Target != null)
                        activeOwners.Add(weak.Target);
                return (GameObject[])activeOwners.ToArray(typeof(GameObject));
            }
        }

        #region EGG Event Rewards 

        /// <summary>
        /// Reward and Category
        /// </summary>
        /// <param name="cat">1-3</param>
        /// <param name="player"></param>
        public virtual void GetRewardForCategory(short cat, GamePlayer player)
        {
            int numberCat1 = Util.Random(1, 5);
            int numberCat2 = Util.Random(1, 25);
            int numberCat3 = Util.Random(1, 31);
            int DropChance = ServerProperties.Properties.Caledonia_Event_Drop_Chance;

            if (player.Group != null)
            {
                foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                {
                    if (players != player)
                        //players.Out.SendMessage("The content of the egg goes to " + player.Name + " ", eChatType.CT_Loot, eChatLoc.CL_ChatWindow);
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "TheContentOfTheEggGoesTo", player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                }
            }

            if (DropChance > 0 && Util.Chance(DropChance))
            {
                cat = (short)Util.Random(1, 3);
                if (cat == 1)
                {

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "CongratulationsTopPrize", player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                if (cat == 2)
                {

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "CongratulationsSecondBest", player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }


                if (cat == 3)
                {

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "CongratulationsMainPrizes", player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

            }
            else

            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "NothingDroppedKeepTrying"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                return;
            }

            switch (cat)
            {
                case 1:
                    {
                        if (numberCat1 == 1)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("beautiful_red_rose");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat1 == 2)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("chicken_in_a_bottle");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat1 == 3)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("beer_humpen");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat1 == 4)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Flowers");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat1 == 5)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("wedding_cake");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        break;

                    }
                case 2:
                    {
                        if (numberCat2 == 1)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Trophy_of_a_Thane");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 2)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Talos_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 3)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Mesedsubastet_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 4)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("housing_indoor_shop83");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 5)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("respec_single");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 6)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Golestandt_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 7)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("housing_indoor_shop10");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 8)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Chisisi_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 9)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Jari_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 10)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("housing_indoor_shop26");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 11)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("housing_indoor_shop0");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 12)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Golden_Pickaxe_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 13)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Ant_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 14)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Wolf_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 15)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("housing_indoor_shop10");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 16)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Frog_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 17)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Boar_Trophy");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 18)
                        {

                            player.Client.Player.ReceiveItems(player, "emeraldseal", 100);
                            player.UpdatePlayerStatus();
                        }
                        if (numberCat2 == 19)
                        {

                            player.Client.Player.ReceiveItems(player, "Sapphireseal", 100);
                            player.UpdatePlayerStatus();

                        }

                        if (numberCat2 == 20)
                        {

                            player.Client.Player.ReceiveItems(player, "gold_weapon_luster", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 21)
                        {

                            player.Client.Player.ReceiveItems(player, "blue_weapon_luster", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 22)
                        {

                            player.Client.Player.ReceiveItems(player, "burgundy_weapon_luster", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 23)
                        {

                            player.Client.Player.ReceiveItems(player, "black_weapon_luster", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 24)
                        {

                            player.Client.Player.ReceiveItems(player, "dark_gold_weapon_luster", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat2 == 25)
                        {

                            player.Client.Player.ReceiveItems(player, "dark_violet_weapon_luster", 1);
                            player.UpdatePlayerStatus();

                        }
                        break;
                    }
                case 3:
                    {
                        if (numberCat3 == 1)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("zahurs_bracer");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 2)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("zahurs_ring");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 3)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("greater_mythirian_of_power");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 4)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("greater_mythirian_of_health");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 5)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("greater_protection_mythirian");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 6)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("greater_mythirian_of_endurance");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 7)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Greater_Alightment_Mythirian");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 8)
                        {

                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("Bolstering_Mighty_Mythirian");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 9)
                        {


                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("EventMount1");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 10)
                        {

                            player.Client.Player.ReceiveItems(player, "aurulite", 100);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 11)
                        {

                            player.Client.Player.ReceiveItems(player, "diamondseal", 100);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 12)
                        {

                            player.Client.Player.ReceiveItems(player, "atlantean_glas", 100);
                            player.UpdatePlayerStatus();

                        }

                        if (numberCat3 == 13)
                        {

                            player.Client.Player.ReceiveItems(player, "dragon_scale", 100);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 14)
                        {

                            player.Client.Player.ReceiveItems(player, "respec_full_dragon_merchant", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 15)
                        {

                            player.Client.Player.ReceiveItems(player, "respec_single_dragon_merchant", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 16)
                        {

                            player.Client.Player.ReceiveItems(player, "respec_realm_dragon_merchant", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 17)
                        {

                            player.Client.Player.ReceiveItems(player, "Armsman_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 18)
                        {

                            player.Client.Player.ReceiveItems(player, "Cleric_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 19)
                        {

                            player.Client.Player.ReceiveItems(player, "Druid_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 20)
                        {

                            player.Client.Player.ReceiveItems(player, "Enchanter_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 21)
                        {

                            player.Client.Player.ReceiveItems(player, "Firbolg_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 22)
                        {

                            player.Client.Player.ReceiveItems(player, "Healer_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 23)
                        {

                            player.Client.Player.ReceiveItems(player, "Hunter_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 24)
                        {

                            player.Client.Player.ReceiveItems(player, "Ranger_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 25)
                        {

                            player.Client.Player.ReceiveItems(player, "Runenmaster_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 26)
                        {

                            player.Client.Player.ReceiveItems(player, "Scout_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 27)
                        {

                            player.Client.Player.ReceiveItems(player, "warrior_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 28)
                        {

                            player.Client.Player.ReceiveItems(player, "Wizzard_Call_Stone", 1);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 29)
                        {


                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("EventMount2");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 30)
                        {


                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("EventMount3");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        if (numberCat3 == 31)
                        {


                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("EventMount4");
                            InventoryItem generic0 = new GameInventoryItem(tgeneric0);
                            player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, generic0);
                            player.UpdatePlayerStatus();

                        }
                        break;
                    }
            }
        }


        private void Addspawn(GameStaticItem eck, int tmaplateID)
        {

            EckSpawnAdd(tmaplateID, 55, eck.X, eck.Y + 50, eck.Z, eck.Heading, eck.CurrentRegionID, 0, -1, eck);

            /*
            switch (Util.Random(14))
            {
                case 1:
                    {
                      
                        EckSpawnAdd(1062, 55, eck.X, eck.Y + 50, eck.Z, eck.Heading, eck.CurrentRegionID, 0, -1, chest);
                        break;
                    }

               
                default:

                    //nothing
                    break;



            }
            */
        }


        private INpcTemplate m_addMobTemplate;

        /// <summary>
        /// Create an add from the specified template.
        /// </summary>
        /// <param name="templateID"></param>
        /// <param name="level"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="uptime"></param>
        /// <returns></returns>
        ///  SpawnAdd(dba.NPCTemplateID, Util.Random(65, 68), dba.X, dba.Y, dba.Z, dba.Heading, dba.Region);
        protected GameNPC EckSpawnAdd(int templateID, int level, int x, int y, int Z, ushort heading, ushort region, int roamingrange, int respawnInterval, GameStaticItem chest)
        {
            GameNPC add = null;

            try
            {
                if (m_addMobTemplate == null || m_addMobTemplate.TemplateId != templateID)
                {
                    m_addMobTemplate = NpcTemplateMgr.GetTemplate(templateID);
                }

                // Create add from template.
                if (m_addMobTemplate != null)
                {
                    add = new GameNPC(m_addMobTemplate)
                    {
                        CurrentRegionID = region,
                        Heading = Convert.ToUInt16(heading),
                        Realm = 0,
                        X = x,
                        Y = y,
                        Z = Z,
                        CurrentSpeed = 0,
                        Level = (byte)level,
                        RespawnInterval = respawnInterval,
                        RoamingRange = roamingrange

                    };

                    add.AddToWorld();
                    if (chest != null)
                    {
                        chest.Model = 1769;
                    }
                   new DespawnTimers2(add, add, 60 * 5 * 1000, chest);
                }
            }

            catch
            {
                log.Warn(String.Format("Unable to get template for {0}", Name));
            }
            return add;
        }
        /// <summary>
        /// Provides a timer to remove an NPC from the world after some
        /// time has passed.
        /// </summary>
        protected class DespawnTimers2 : GameTimer
        {
            private GameNPC m_npc;
            private GameStaticItem m_chest;

            /// <summary>
            /// Constructs a new DespawnTimer.
            /// </summary>
            /// <param name="timerOwner">The owner of this timer.</param>
            /// <param name="npc">The GameNPC to despawn when the time is up.</param>
            /// <param name="delay">The time after which the add is supposed to despawn.</param>
            public DespawnTimers2(GameObject timerOwner, GameNPC npc, int delay, GameStaticItem chest)
                : base(timerOwner.CurrentRegion.TimeManager)
            {
                m_npc = npc;
                m_chest = chest;

                Start(delay);
            }
            /// <summary>
            /// Called on every timer tick.
            /// </summary>
            protected override void OnTick()
            {

                if (m_npc != null && m_npc.InCombat == false)
                {

                    m_npc.Delete();
                    m_npc = null;

                }
            }
        }

        #endregion
    }
}

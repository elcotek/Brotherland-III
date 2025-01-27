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
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Reflection;

namespace DOL.GS
{
    #region GameSiegeweapon
    /// <summary>
    /// Description r�sum�e de GameSiegeWeapon.
    /// </summary>
    public class GameSiegeWeapon : GameMovingObject
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GameSiegeWeapon()
        {
            SetOwnBrain(new BlankBrain());
            this.Realm = 0;
            Level = 1;
            CurrentState = eState.Inactive;
            m_ammo = new ArrayList();
            Health = MaxHealth;
            m_ammoSlot = 0x14;
            ActionDelay = new int[]
                {
                    0,//none
					10000,//aiming
					15000,//arming
					5000,//loading
					0//fireing
				};//en ms
            m_enableToMove = true;
            MaxSpeedBase = 100;
        }

        public const int SIEGE_WEAPON_CONTROLE_DISTANCE = 100;
        public const int TIME_TO_DECAY = 60 * 1000 * 10; //All 10 min 10% damge
      
        #region enum
        public enum eState : byte
        {
            Inactive = 0x0,
            Armed = 0x01,
            Aimed = 0x02,
            Ready = 0x03,//armed+aimed
        }

        public enum eCommand : byte
        {
            None = 0x00,
            PutAmmo = 0x01,
            Aim = 0x03,
            Fire = 0x04,
            Move = 0x05,
            Repair = 0x06,
            Salvage = 0x07,
            Release = 0x08,
        }
        #endregion
        #region properties
        private GameKeepHookPoint m_hookPoint;
        public GameKeepHookPoint HookPoint
        {
            get { return m_hookPoint; }
            set { m_hookPoint = value; }
        }

        private GameObject m_turningTarget;
        public GameObject LastTuringTarget
        {
            get { return m_turningTarget; }
            set { m_turningTarget = value; }
        }


        public override Region CurrentRegion
        {
            get
            {
                return base.CurrentRegion;
            }
            set
            {
                base.CurrentRegion = value;
                SiegeWeaponTimer = new SiegeTimer(this);
            }
        }

        private byte m_ammoType;
        public byte AmmoType
        {
            get { return m_ammoType; }
            set { m_ammoType = value; }
        }

        private GamePlayer m_owner;
        public new GamePlayer Owner
        {
            get { return m_owner; }
            set { m_owner = value; }
        }
        private eState m_currentState;
        public eState CurrentState
        {
            get { return m_currentState; }
            set { m_currentState = value; }
        }

        public virtual ushort Effect
        {
            get { return m_effect; }
            set { m_effect = value; }
        }
        protected ArrayList m_ammo;
        public virtual ArrayList Ammo
        {
            get { return m_ammo; }
            set { m_ammo = value; }
        }
        protected SiegeTimer m_siegeTimer;
        public virtual SiegeTimer SiegeWeaponTimer
        {
            get { return m_siegeTimer; }
            set { m_siegeTimer = value; }
        }
        public override int MaxHealth
        {
            get
            {
                return 10000;
            }
        }

        public override int Mana
        {
            get { return 50000; }
        }

        private int m_timesrepaired;
        public int TimesRepaired
        {
            get { return m_timesrepaired; }
            set { m_timesrepaired = value; }
        }

        protected RegionTimer m_decayTimer;
        /// <summary>
        /// The lock object for lazy regen timers initialization
        /// </summary>
        protected readonly object m_decayTimerLock = new object();

        private ushort m_ammoSlot;
        public ushort AmmoSlot
        {
            get { return m_ammoSlot; }
            set { m_ammoSlot = value; }
        }

        public int DecayedHp
        {
            get { return 3 * (this.MaxHealth / 10); }
        }

        public int DeductHp
        {
            get { return this.MaxHealth * 10 / 100; } //-this.MaxHealth / 10; } www
        }

        private string m_itemId;
        public string ItemId
        {
            get { return m_itemId; }
            set { m_itemId = value; }
        }
        #endregion
        #region public methode
        public void TakeControl(GamePlayer player)
        {

            if (Owner != null && Owner != player && Owner.IsWithinRadius(this, 500) == false && (this is GameSiegeTrebuchet || this is GameSiegeBallista || this is GameSiegeCauldron || this is GameSiegeCatapult))
            {
                ReleaseControl();
            }
            if (Owner != null && Owner != player)
            {
                player.Out.SendMessage(GetName(0, true) + " is already under control.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return;
            }
            if (player.SiegeWeapon != null && player.SiegeWeapon == this)
            {
                player.Out.SendMessage("You already have a siege weapon under your control.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return;
            }
            if (IsMoving)
            {
                player.Out.SendMessage("You can't take control of a siege weapon while it is moving.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return;
            }
            if (!this.IsWithinRadius(player, SIEGE_WEAPON_CONTROLE_DISTANCE))
            {//You prepare the cauldron of boiling oil for firing. (15.0s until armed)
                player.Out.SendMessage("You are too far away to control a siegeweapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
            Owner = player;



            player.SiegeWeapon = this;
            Owner.Out.SendSiegeWeaponInterface(this, SiegeWeaponTimer.TimeUntilElapsed / 100);
            player.Out.SendMessage("You take control of " + GetName(0, false) + ".", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
            if (CurrentState != GameSiegeWeapon.eState.Armed)
                Arm();

        }
        public virtual void ReleaseControl()
        {

            if (Owner == null)
            {
                StopMove();
                return;
            }
            Owner.Out.SendMessage("You are no longer controlling " + GetName(0, false) + ".", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
            Owner.Out.SendSiegeWeaponCloseInterface();
            Owner.SiegeWeapon = null;
            Owner = null;
            StopMove();
        }

        public override void Die(GameObject killer)
        {
            StopDecay();
            ReleaseControl();
            Delete();
        }

        public void Aim()
        {
            if (!CanUse()) return;
            //The trebuchet isn't ready to be aimed yet!
            if (Owner.TargetObject is GameNPC || Owner.TargetObject is GameLiving == false) return;
            if (Owner.TargetObject == null) return;
            if (!GameServer.ServerRules.IsAllowedToAttack(Owner, ((GameLiving)Owner.TargetObject), true)) return;
            CurrentState &= ~eState.Aimed;


            SetGroundTarget(Owner.TargetObject.X, Owner.TargetObject.Y, Owner.TargetObject.Z);
            TargetObject = Owner.TargetObject;
            if (this is GameSiegeRam)
            {
                LastTuringTarget = TargetObject;
            }
            SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Aiming;
            Heading = GetHeading(GroundTarget);
            PreAction();

            if (Owner != null)
            {
                this.StopMove();
                Owner.Out.SendMessage(GetName(0, true) + " is turning to your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

            }

            //Automatische Rammen Ausrichtung
            bool turning = false;
            foreach (GameNPC target in this.GetNPCsInRadius(600))
            {

                if (Owner != null && target.X != this.X && target.Y - (this.AttackRange - 370) != this.Y && this is GameSiegeRam && Owner.TargetObject is GameKeepDoor && target.Name == "RamPosition" && target != null && this != null && turning == false)
                {



                    foreach (GameLiving ram in GetNPCsInRadius(1852))
                    {
                        if (ram != null && ram is GameSiegeRam && ram != this)
                        {

                            if (ram.X == this.X && ram.Y == this.Y)
                            {
                                foreach (GameLiving player in GetPlayersInRadius(800))
                                {
                                    if (player != null)
                                    {
                                        (player as GamePlayer).Out.SendMessage("Another ram was already in front of the door, You hear a loud crack and your ram was destroyed!!!.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                    }
                                }
                                turning = false;
                                Die(this);
                            }
                            else
                            {

                                this.MoveTo(target.CurrentRegionID, target.X, target.Y - (this.AttackRange - 370), target.Z, (ushort)target.Heading);
                                turning = true;
                                this.SaveIntoDatabase();
                            }
                        }
                        else
                        {

                            this.MoveTo(target.CurrentRegionID, target.X, target.Y - (this.AttackRange - 370), target.Z, (ushort)target.Heading);
                            turning = true;
                            this.SaveIntoDatabase();
                        }
                    }

                }
            }
        }

        public void Arm()
        {
            if (!CanUse()) return;
            CurrentState &= ~eState.Armed;
            SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Arming;
            //SiegeWeaponTimer.CurrentAction = SiegeTimer.eAction.Aiming;

            PreAction();
            if (Owner != null)
            {//You prepare the cauldron of boiling oil for firing. (15.0s until armed)


                if (Owner.GetSpellLine("ML2 Warlord") != null || Owner.GetModified(eProperty.MythicalSiegeSpeed) > 0)
                {
                    (Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "GamePlayer.WarlordSiege", this.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

                Owner.Out.SendMessage("You prepare " + GetName(0, false) + " for firing. (" + (GetActionDelay(SiegeTimer.eAction.Arming) / 1000).ToString("N") + "s until armed)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }


        public void Move()
        {
            if (!CanUse()) return;
            if (!m_enableToMove) return;
            if (Owner == null || Owner.GroundTarget == null) return;

            foreach (GameObject door in Owner.GetDoorsInRadius(1000))
            {
                if (door.IsObjectAlive && door.IsWithinRadius(Owner.GroundTarget, 130) && door is GameKeepDoor == true && Owner is GamePlayer && door != null)
                {


                    // WalkTo(new Point3D(Owner.GroundTarget.X, Owner.GroundTarget.Y, Owner.GroundTarget.Z), 100);
                    Owner.SiegeWeapon.StopMove();
                    Owner.Out.SendMessage("Your Ground target is too close to a door!!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }
            }
            if (!this.IsWithinRadius(Owner.GroundTarget, 1200))
            {
                Owner.Out.SendMessage("You can't move a ram wile stealthed!", eChatType.CT_System,
                                      eChatLoc.CL_SystemWindow);
                return;
            }
            if (!this.IsWithinRadius(Owner.GroundTarget, 1200))
            {
                Owner.Out.SendMessage("Your Ground target is too far away to move to!", eChatType.CT_System,
                                      eChatLoc.CL_SystemWindow);
                return;
            }
            if (!Owner.GroundTargetInView)
            {
                Owner.Out.SendMessage("Your Ground target is out of sight!", eChatType.CT_System,
                                      eChatLoc.CL_SystemWindow);
                return;
            }

            //let's check if we are trying to move too close to a door, if we are, don't move
            foreach (IDoor door in Owner.CurrentRegion.GetDoorsInRadius(Owner.GroundTarget.X, Owner.GroundTarget.Y, Owner.GroundTarget.Z, (ushort)(AttackRange - 240), false))
            {
                if ((door as GameKeepDoor).IsObjectAlive && door is GameKeepDoor)
                {
                    Owner.Out.SendMessage("You can't move a ram that close to a door!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
            }

            //unarmed siege weapon
            CurrentState &= ~eState.Armed;

            WalkTo(new Point3D(Owner.GroundTarget.X, Owner.GroundTarget.Y, Owner.GroundTarget.Z), 100);
            //WalkTo(Owner.GroundTarget, 100);
        }

        public void StopMove()
        {
            StopMoving();
        }

        public void Load(int ammo)
        {
            AmmoSlot = (ushort)ammo;
        }
        public void Aimed()
        {
            if (!CanUse()) return;
            CurrentState |= eState.Aimed;
            if (Owner != null)
            {

                // Owner.UpdateEncumberance();
                //Owner.UpdatePlayerStatus();
                if (CurrentState != eState.Ready)
                    CurrentState = eState.Ready;

                Owner.Out.SendMessage("Your " + Name + " is now aimed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                //Owner.Out.SendMessage("Your " + Name + " ram is updatet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }
        public void Armed()
        {
            if (!CanUse()) return;
            if (CurrentState != eState.Ready)
                CurrentState = eState.Ready;
            CurrentState |= eState.Armed;
            if (Owner != null)
            {
                Owner.Out.SendMessage("Your " + Name + " is now armed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }
        }
        public void Fire()
        {
            if (!CanUse()) return;
            {
                if (Owner != null)
                {
                    GameObject target = (LastTuringTarget as GameObject);

                    //target fix: Rammen m�ssen das letzte LastTuringTarget benutzen
                    if (this is GameSiegeRam && (target is GameDoor == false && target is GameKeepDoor == false && target is GameKeepComponent == false || Owner.TargetObject != LastTuringTarget))
                    {
                        Owner.Out.SendMessage("You must first target a Door or Wall and this have be the first Turning target to fire!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (this is GameSiegeCauldron == false && this.TargetObject == null)
                    {
                        return;
                    }
                    if (this is GameSiegeTrebuchet && this.IsWithinRadius(this.TargetObject, 1500))
                    {
                        Owner.Out.SendMessage("Your target must be greather range to attack ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (this is GameSiegeCatapult && this.IsWithinRadius(this.TargetObject, 800))
                    {
                        Owner.Out.SendMessage("Your target must be greather range to attack ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (this is GameSiegeRam && TargetObject is GameKeepDoor && !this.IsWithinRadius(TargetObject, 380))
                    {
                        Owner.Out.SendMessage("You Ram is too far away to attack ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (this is GameSiegeRam && TargetObject is GameKeepComponent && !this.IsWithinRadius(TargetObject, 580))
                    {
                        Owner.Out.SendMessage("You Ram is too far away to attack ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (TargetObject is GameKeepDoor && (this is GameSiegeCatapult || this is GameSiegeTrebuchet || this is GameSiegeBallista))
                    {
                        Owner.Out.SendMessage("You can't attack a door with this Weapon", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        SiegeWeaponTimer.Stop();
                        Owner.Out.SendSiegeWeaponCloseInterface();
                        ReleaseControl();
                        Owner = null;
                        StopMove();
                        return;
                    }

                    if (!this.IsWithinRadius(TargetObject, 6000) && (this is GameSiegeCatapult || this is GameSiegeTrebuchet || this is GameSiegeBallista))
                    {
                        //Owner.Out.SendMessage("Your target is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        (Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Owner as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        SiegeWeaponTimer.Stop();
                        Owner.Out.SendSiegeWeaponCloseInterface();
                        ReleaseControl();
                        Owner = null;
                        StopMove();
                        return;
                    }

                    if (!this.IsWithinRadius(this.Owner, SIEGE_WEAPON_CONTROLE_DISTANCE) && (this is GameSiegeCauldron || this is GameSiegeCatapult || this is GameSiegeTrebuchet || this is GameSiegeBallista) || !this.Owner.IsAlive)
                    {//You prepare the cauldron of boiling oil for firing. (15.0s until armed)
                        Owner.Out.SendMessage("You are too far from your siege equipment to control it any longer!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        SiegeWeaponTimer.Stop();
                        Owner.Out.SendSiegeWeaponCloseInterface();
                        ReleaseControl();
                        Owner = null;
                        StopMove();
                        return;
                    }
                }
                if (CurrentState != eState.Ready)
                {
                    if (Owner != null)
                    {
                        Owner.Out.SendMessage("The " + Name + " is not ready to fire yet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                    return;
                }
                if (TargetObject != null)
                    SetGroundTarget(TargetObject.X, TargetObject.Y, TargetObject.Z);
                if (GroundTarget == null)
                    return;

                if (TargetObject != null && TargetObject.Realm == Realm)
                    return;


                new RegionTimer(this, new RegionTimerCallback(MakeDelayedDamage), GetActionDelay(SiegeTimer.eAction.Fire));
                BroadcastFireAnimation(GetActionDelay(SiegeTimer.eAction.Fire));

                if (Owner != null)
                    Owner.Out.SendMessage("You fire " + GetName(0, false) + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Arm();
            }
        }

        private int MakeDelayedDamage(RegionTimer callingTimer)
        {

            DoDamage();
            return 0;
        }

        public virtual void DoDamage()
        {
        }
        /*	slot:48 level:90 value1:0x00 value2:0x00 hand:0x00 damageType:0x00 objectType:0x29 weight:2    con:100 dur:0   qual:0   bonus:0  model:0x0A3C color:0x0000 effect:0x00 unk1_172:1 "2 greek fire"
            slot:49 level:90 value1:0x00 value2:0x00 hand:0x00 damageType:0x00 objectType:0x29 weight:2    con:100 dur:0   qual:0   bonus:0  model:0x0A3D color:0x0000 effect:0x00 unk1_172:1 "2 ice ball"
            index:0  unk1:0x5A00 unk2:0x0000 unk3:0x2900 unk4:0x0264 unk5:0x00 unk6:0x0000 model:0x0A3D unk7:0x0001 unk7:0x0000 name:"2 ice ball")
            index level value1  value2 hand  objecttype damagetype weight conc dur qual bonnus model color effect
        */

        public void Repair()
        {
            if (Owner != null)
            {
                if (TimesRepaired <= 3)
                {
                    if (Owner.GetCraftingSkillValue(eCraftingSkill.WoodWorking) < 301)
                    {
                        Owner.Out.SendMessage("You must have woodworking skill to repair a siege weapon.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    TimesRepaired += 1;
                    Health += (int)(this.MaxHealth * 0.15);
                }
                else
                {
                    this.Owner.Out.SendMessage("The siegeweapon has decayed beyond repairs!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                }
            }
        }

        public void salvage()
        {
            if (Owner.GetCraftingSkillValue(eCraftingSkill.SiegeCrafting) == -1)
            {
                Owner.Out.SendMessage("You must be a Siege weapon crafter to salvage it.", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return;
            }
            Owner.SalvageSiegeWeapon(this);
        }
        #endregion
        #region private methods
        private void BroadcastAnimation()
        {
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSiegeWeaponAnimation(this);
            }
        }
        private void BroadcastFireAnimation(int timer)
        {
            foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
            {
                player.Out.SendSiegeWeaponFireAnimation(this, timer);
                //				player.Out.SendEmoteAnimation(this, (eEmote)201); // on trebuchet fire external ammo ?
            }
        }
        protected int[] ActionDelay;
        private ushort m_effect;
        private bool m_enableToMove;

        /// <summary>
        /// delay to do action in Ms
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private int GetActionDelay(SiegeTimer.eAction action)
        {
            if (action == SiegeTimer.eAction.Fire && TargetObject != null)
                return (int)(ActionDelay[(int)action] * 0.001 * this.GetDistanceTo(TargetObject));

            int delay = ActionDelay[(int)action];

            if (Owner != null)
            {
                int mythicalSpeed = 0;
                if (Owner.GetModified(eProperty.MythicalSiegeSpeed) > 0)
                {
                    mythicalSpeed = Owner.GetModified(eProperty.MythicalSiegeSpeed);
                }
                //log.DebugFormat("siege duration {0}", delay);
                //Warlord ML2 Siege bonus 30%
                if (Owner.GetSpellLine("ML2 Warlord") != null)
                {
                    delay = (int)(delay * (70 - mythicalSpeed) / 100);
                    //  log.DebugFormat("siege duration now {0}", delay);

                }
                else if(Owner.GetModified(eProperty.MythicalSiegeSpeed) > 0)
                {
                   //log.ErrorFormat("mythicalSpeed: {0}", mythicalSpeed);
                   delay = (int)(delay * (100 - mythicalSpeed) / 100);
                }
               
            }

            //TODO: better to use a property here - discuss to implement one? dunnow if siegespeed is used at another place.
            if (Owner != null && Owner.EffectList.CountOfType<BannerOfBesiegingEffect>() > 0)
            {
                BannerOfBesiegingEffect eff = Owner.EffectList.GetOfType<BannerOfBesiegingEffect>();
                if (eff != null)
                    delay = (int)(delay * (1 - 0.06 * eff.Effectiveness));

            }
            return delay;
        }

        private Boolean CanUse()
        {
            if (Owner == null)
                return false;

            if (!Owner.IsAlive || Owner.IsMezzed || Owner.IsCasting || Owner.IsStunned && Owner.IsMoving || Owner.IsSwimming)
            {
                this.Owner.Out.SendMessage("You can't use the siegeweapon now!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                SiegeWeaponTimer.Stop();
                Owner.Out.SendSiegeWeaponCloseInterface();
                ReleaseControl();
                return false;
            }
            if (Health <= DecayedHp)
            {
                this.Owner.Out.SendMessage("The siegeweapon needs to be repaired!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return false;
            }

            return true;
        }

        private void PreAction()
        {
            if (SiegeWeaponTimer.IsAlive)
            {
                SiegeWeaponTimer.Stop();
                if (Owner != null)
                    Owner.Out.SendSiegeWeaponCloseInterface();
            }
            SiegeWeaponTimer.Start(GetActionDelay(SiegeWeaponTimer.CurrentAction));
            if (Owner != null)
            {
                Owner.Out.SendSiegeWeaponInterface(this, GetActionDelay(SiegeWeaponTimer.CurrentAction) / 100);
            }
            BroadcastAnimation();
        }
        #endregion
        #region override function
        public override bool ReceiveItem(GameLiving source, DOL.Database.InventoryItem item)
        {
            //todo check if bullet
            return base.ReceiveItem(source, item);
        }
        public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
        {
            if (source is GamePlayer)
            {
                damageAmount /= 30;
                criticalAmount /= 30;
            }
            base.TakeDamage(source, damageType, damageAmount, criticalAmount);
        }

        public override bool Interact(GamePlayer player)
        {

            if (!base.Interact(player))
                return false;

            if (Owner == player)
                return false;

            TakeControl(player);
            return true;
        }
        public override void LoadFromDatabase(DataObject obj)
        {
            base.LoadFromDatabase(obj);
            if (!(obj is ItemTemplate)) return;
            ItemTemplate item = (ItemTemplate)obj;
            this.Name = item.Name;
            this.Model = (ushort)item.Model;
        }

        public bool EnableToMove
        {
            set { m_enableToMove = value; }
            get { return m_enableToMove; }
        }

        public override bool AddToWorld()
        {
            if (!base.AddToWorld()) return false;
            StartDecay();
            TimesRepaired = 0;
            return true;
        }
        public override bool RemoveFromWorld()
        {
            if (!base.RemoveFromWorld()) return false;
            StopDecay();
            return true;
        }

        #endregion
        #region decay
        public override void StartHealthRegeneration()
        {
            //don't regenerate health
        }

        private void StartDecay()
        {
            if (ObjectState != eObjectState.Active)
                return;
            lock (m_decayTimerLock)
            {
                if (m_decayTimer == null)
                {
                    m_decayTimer = new RegionTimer(this)
                    {
                        Callback = new RegionTimerCallback(DecayTimerCallback)
                    };
                }
                else if (m_decayTimer.IsAlive)
                    return;
                m_decayTimer.Start(TIME_TO_DECAY);
            }
        }

        private void StopDecay()
        {
            lock (m_decayTimerLock)
            {
                if (m_decayTimer == null)
                    return;
                m_decayTimer.Stop();
                m_decayTimer = null;
            }
        }

        private int DecayTimerCallback(RegionTimer callingTimer)
        {
            //DeductHp = 10%
            TakeDamage(this, eDamageType.Natural, DeductHp, 0);
            //log.Error("Ram Take Damage");
            return TIME_TO_DECAY;
        }

        #endregion

        private static SpellLine m_SiegeSpellLine;
        public static SpellLine SiegeSpellLine
        {
            get
            {
                if (m_SiegeSpellLine == null)
                    m_SiegeSpellLine = new SpellLine("SiegeSpellLine", "Siege Weapon Spells", "unknown", false);

                return m_SiegeSpellLine;
            }
        }
    }
    #endregion
    #region siegeTimer
    public class SiegeTimer : RegionAction
    {
        public enum eAction : byte
        {
            None = 0x00,
            Aiming = 0x01,
            Arming = 0x02,
            Loading = 0x03,
            Fire = 0x04,
        }

        /// <summary>
        /// Constructs a new UseSlotAction
        /// </summary>
        /// <param name="siegeWeapon">The siege weapon</param>
        public SiegeTimer(GameSiegeWeapon siegeWeapon)
            : base(siegeWeapon)
        {
            m_siegeWeapon = siegeWeapon;
        }

        private eAction m_currentAction;
        private GameSiegeWeapon m_siegeWeapon;

        public eAction CurrentAction
        {
            get { return m_currentAction; }
            set { m_currentAction = value; }
        }
        public GameSiegeWeapon SiegeWeapon
        {
            get { return m_siegeWeapon; }
            set { m_siegeWeapon = value; }
        }

        protected override void OnTick()
        {

            if (SiegeWeapon.Owner != null)
                SiegeWeapon.Owner.Out.SendMessage("Action = " + CurrentAction, eChatType.CT_Say, eChatLoc.CL_SystemWindow);
            else return;
            switch (CurrentAction)
            {
                case eAction.Arming:
                    {
                        SiegeWeapon.Armed();
                        break;
                    }
                case eAction.Aiming:
                    {


                        // if (CurrentAction != eAction.Arming)
                        SiegeWeapon.Aimed();

                        break;
                    }
                case eAction.Loading:
                    {

                        //todo set ammo
                        break;
                    }
                case eAction.Fire:
                    {

                        SiegeWeapon.DoDamage();
                        break;
                    }
                default: break;
            }

            if (SiegeWeapon.Owner != null)
            {
                SiegeWeapon.Owner.Out.SendSiegeWeaponInterface(this.SiegeWeapon, 0);
            }
            if ((SiegeWeapon.CurrentState & GameSiegeWeapon.eState.Armed) != GameSiegeWeapon.eState.Armed)
                SiegeWeapon.Arm();
        }
    }
    #endregion
}
/* messages:

You are too far from your siege equipment to control it any longer!
You can't salvage the trebuchet!
The trebuchet's target is too far away to reach!
Your target has moved out of range!
You put the ice ball into the field catapult.
The trebuchet is fully repaired!
That object isn't carryable...
*/

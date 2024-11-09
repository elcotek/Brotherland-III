using System.Reflection;
using log4net;
using System;
using System.Collections;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.AI.Brain;
using DOL.GS;
using DOL.GS.Keeps;
using DOL.Events;
using DOL.Language;
using System.Collections.Specialized;

namespace DOL.GS.Spells
{
    //http://www.camelotherald.com/masterlevels/ma.php?ml=Sojourner
    //no shared timer
    #region Sojourner-1
    //Gameplayer - MaxEncumbrance
    #endregion

    //ML2 Unending Breath - already handled in another area

    //ML3 Reveal Crystalseed - already handled in another area

    //no shared timer
    #region Sojourner-4
    [SpellHandlerAttribute("UnmakeCrystalseed")]
    public class UnmakeCrystalseedSpellHandler : SpellHandler
    {
        /// <summary>
        /// Execute unmake crystal seed spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// execute non duration spell effect on target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="effectiveness"></param>
        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            if (target == null || !target.IsAlive)
                return;

            foreach (GameNPC item in target.GetNPCsInRadius((ushort)m_spell.Radius))
            {
                if (item != null && item is GameMine)
                {
                    (item as GameMine).Delete();
                }
            }
        }

        public UnmakeCrystalseedSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //no shared timer
    #region Sojourner-5
    [SpellHandlerAttribute("AncientTransmuter")]
    public class AncientTransmuterSpellHandler : SpellHandler
    {
        private GameMerchant merchant;
        /// <summary>
        /// Execute Acient Transmuter summon spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner == null || !effect.Owner.IsAlive)
                return;

            merchant.AddToWorld();
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (merchant != null) merchant.Delete();
            return base.OnEffectExpires(effect, noMessages);
        }
        public AncientTransmuterSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            if (caster is GamePlayer)
            {
                GamePlayer casterPlayer = caster as GamePlayer;
                merchant = new GameMerchant();
                //Fill the object variables
                merchant.X = casterPlayer.X + Util.Random(20, 40) - Util.Random(20, 40);
                merchant.Y = casterPlayer.Y + Util.Random(20, 40) - Util.Random(20, 40);
                merchant.Z = casterPlayer.Z;
                merchant.CurrentRegion = casterPlayer.CurrentRegion;
                merchant.Heading = (ushort)((casterPlayer.Heading + 2048) % 4096);
                merchant.Level = 1;
                merchant.Realm = casterPlayer.Realm;
                merchant.Name = "Ancient Transmuter";
                merchant.Model = 993;
                merchant.CurrentSpeed = 0;
                merchant.MaxSpeedBase = 0;
                merchant.GuildName = "";
                merchant.Size = 50;
                merchant.Flags |= GameNPC.eFlags.PEACE;
                merchant.TradeItems = new MerchantTradeItems("ML_transmuteritems");
            }
        }
    }
    #endregion


    #region GameVault

    [SpellHandlerAttribute("SummonGameVault")]
    public class GameVaultSpellHandler : SpellHandler
    {
        private GameVaultKeeper merchant;
        /// <summary>
        /// Execute Acient Transmuter summon spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (effect.Owner == null || !effect.Owner.IsAlive)
                return;

            merchant.AddToWorld();
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (merchant != null) merchant.Delete();
            return base.OnEffectExpires(effect, noMessages);
        }
        public GameVaultSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            if (caster is GamePlayer)
            {
                GamePlayer casterPlayer = caster as GamePlayer;
                merchant = new GameVaultKeeper();
                //Fill the object variables
                merchant.X = casterPlayer.X + Util.Random(20, 40) - Util.Random(20, 40);
                merchant.Y = casterPlayer.Y + Util.Random(20, 40) - Util.Random(20, 40);
                merchant.Z = casterPlayer.Z;
                merchant.CurrentRegion = casterPlayer.CurrentRegion;
                merchant.Heading = (ushort)((casterPlayer.Heading + 2048) % 4096);
                merchant.Level = 1;
                merchant.Realm = casterPlayer.Realm;
                merchant.Name = "Vault Keeper";
                merchant.GuildName = caster.GuildName;
                merchant.Model = 633;
                merchant.CurrentSpeed = 0;
                merchant.MaxSpeedBase = 0;
                merchant.Size = 50;
                merchant.Flags |= GameNPC.eFlags.PEACE;

            }

        }
    }
    #endregion

    //no shared timer
    #region Sojourner-6
    [SpellHandlerAttribute("Port")]
    public class Port : MasterlevelHandling
    {
        // constructor
        public Port(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            if (target == null) return;
            if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;
            if (Caster.CurrentRegionID == 51)
            {
                MessageToCaster("You can't use this Ability here or bind here first!", eChatType.CT_SpellResisted);
                return;
            }
            GamePlayer player = Caster as GamePlayer;

           


            if (player != null)
            {
                if (GameRelic.IsPlayerCarryingRelic(player))
                {
                    player.Out.SendMessage("You can't teleport you has a Relic!.", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                    return;
                }
                   
                if (!player.InCombat)
                {
                    //Anti Buffbot for PVM Zones      
                    GameSpellEffect effect1 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "DexterityQuicknessBuff");
                    GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "StrengthConstitutionBuff");
                    GameSpellEffect effect3 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "StrengthBuff");
                    GameSpellEffect effect4 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "ConstitutionBuff");
                    GameSpellEffect effect5 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "DexterityBuff");
                    GameSpellEffect effect6 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "AcuityBuff");
                    GameSpellEffect effect7 = SpellHandler.FindEffectOnTarget((GamePlayer)target, "ArmorFactorBuff");


                    if ((target.CurrentRegionID == 163 || target.CurrentRegionID == 233 || target.CurrentRegionID == 244 || target.CurrentRegionID == 245) && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                    {
                        if (effect1 != null) effect1.Cancel(false);
                        if (effect2 != null) effect2.Cancel(false);
                        if (effect3 != null) effect3.Cancel(false);
                        if (effect4 != null) effect4.Cancel(false);
                        if (effect5 != null) effect5.Cancel(false);
                        if (effect6 != null) effect6.Cancel(false);
                        if (effect7 != null) effect7.Cancel(false);
                    }



                    SendEffectAnimation(player, 0, false, 1);
                    player.MoveToBind();
                }
            }
        }


    #endregion

        //no shared timer
        #region Sojourner-7
        [SpellHandlerAttribute("EssenceResist")]
        public class EssenceResistHandler : AbstractResistBuff
        {
            public override eBuffBonusCategory BonusCategory1 { get { return eBuffBonusCategory.BaseBuff; } }
            public override eProperty Property1 { get { return eProperty.Resist_Natural; } }
            public EssenceResistHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        }
        #endregion Sojourner-7

        //no shared timer
        #region Sojourner-8
        [SpellHandlerAttribute("Zephyr")]
        public class FZSpellHandler : MasterlevelHandling
        {
            protected RegionTimer m_expireTimer;
            protected GameNPC m_npc;
            protected GamePlayer m_target;
            protected IPoint3D m_loc;

            public override void OnDirectEffect(GameLiving target, double effectiveness)
            {
                if (target == null) return;
                GamePlayer player = target as GamePlayer;
                if (player != null && player.IsAlive)
                {
                    Zephyr(player);
                }
            }

            public override bool CheckBeginCast(GameLiving target)
            {
                if (m_caster.CurrentRegion.IsDungeon)
                {
                    MessageToCaster("This Ability work only in open Areas!", eChatType.CT_SpellResisted);
                    return false;
                }

                if (target == null)
                {
                    if (Caster is GamePlayer)
                    {
                        (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.SelectATargetForThisSpell", Spell.Name), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                        // MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                    }
                        return false;
                }

                if (target is GameNPC == true)
                    return false;

                if (!GameServer.ServerRules.IsAllowedToAttack(Caster, target, true))
                    return false;

                return base.CheckBeginCast(target);
            }

            private void Zephyr(GamePlayer target)
            {
                if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;
                GameNPC npc = new GameNPC();

                m_npc = npc;

                npc.Realm = Caster.Realm;
                npc.Heading = Caster.Heading;
                npc.Model = 1269;
                npc.Y = Caster.Y;
                npc.X = Caster.X;
                npc.Z = Caster.Z;
                npc.Name = "Forceful Zephyr";
                npc.MaxSpeedBase = 430;
                npc.Level = 55;
                npc.CurrentRegion = Caster.CurrentRegion;
                npc.Flags |= GameNPC.eFlags.PEACE;
                npc.Flags |= GameNPC.eFlags.DONTSHOWNAME;
                npc.Flags |= GameNPC.eFlags.CANTTARGET;
                BlankBrain brain = new BlankBrain();
                npc.SetOwnBrain(brain);
                npc.AddToWorld();
                npc.TempProperties.setProperty("target", target);
                GameEventMgr.AddHandler(npc, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(ArriveAtTarget));

                npc.WalkTo(new Point3D(target.X, target.Y, target.Z), target.MaxSpeed);
                //npc.Follow(target, 1, 1800);

                m_target = target;

                StartTimer();

            }

            protected virtual void StartTimer()
            {
                StopTimer();
                m_expireTimer = new RegionTimer(m_npc, new RegionTimerCallback(ExpiredCallback), 10000);
            }

            protected virtual int ExpiredCallback(RegionTimer callingTimer)
            {
                m_target.IsStunned = false;
                m_target.DismountSteed(true);
                //m_target.DebuffCategory[(int)eProperty.SpellFumbleChance] -= 100;
                GameEventMgr.RemoveHandler(m_target, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                m_npc.StopMoving();
                m_npc.RemoveFromWorld();
                //sometimes player can't move after zephyr :
                m_target.Out.SendUpdateMaxSpeed();
                return 0;
            }

            protected virtual void StopTimer()
            {

                if (m_expireTimer != null)
                {
                    m_expireTimer.Stop();
                    m_expireTimer = null;
                }

            }

            private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
            {
                GameLiving living = sender as GameLiving;
                if (living == null) return;
                AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
                AttackData ad = null;
                if (attackedByEnemy != null)
                    ad = attackedByEnemy.AttackData;

                if (ad.Attacker is GamePlayer)
                {
                    ad.Damage = 0;
                    ad.CriticalDamage = 0;
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                    //GamePlayer player = ad.Attacker as GamePlayer;

                    MessageToLiving(ad.Target, string.Format("You're in a Zephyr and can't be attacked!"), eChatType.CT_Spell);
                    MessageToLiving(ad.Attacker, string.Format("Your target is in a Zephyr and can't be attacked!"), eChatType.CT_Spell);
                }
                if (ad.Attacker is GameNPC)
                {
                    ad.Damage = 0;
                    ad.CriticalDamage = 0;
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                   // GamePlayer player = ad.Attacker as GamePlayer;

                   MessageToLiving(ad.Target, string.Format("You're in a Zephyr and can't be attacked!"), eChatType.CT_Spell);
                   // MessageToLiving(ad.Attacker, string.Format("Your target is in a Zephyr and can't be attacked!"), eChatType.CT_Spell);
                }
            }
           
            private void ArriveAtTarget(DOLEvent e, object obj, EventArgs args)
            {
                GameNPC npc = obj as GameNPC;

                if (npc == null) return;

                GamePlayer target = npc.TempProperties.getProperty<object>("target", null) as GamePlayer;

                if (target == null || !target.IsAlive) return;
                GameEventMgr.RemoveHandler(npc, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(ArriveAtTarget));

                GamePlayer player = target as GamePlayer;
                if (player == null) return;
                if (!player.IsAlive) return;

                player.IsStunned = true;
                //player.IsSilenced = true;
                //player.DebuffCategory[(int)eProperty.SpellFumbleChance] += 100;
                player.StopAttack();
                player.StopCurrentSpellcast();
                player.MountSteed(npc, true);
                GameEventMgr.AddHandler(player, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));

                player.Out.SendMessage("You are picked up by a forceful zephyr!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                npc.StopFollowing();

                if (Caster is GamePlayer)
                {
                    //Calculate random target
                    m_loc = GetTargetLoc();
                    (Caster as GamePlayer).Out.SendCheckLOS((Caster as GamePlayer), m_npc, new CheckLOSResponse(ZephyrCheckLOS));
                }
            }
            /// <summary>
            /// Is there are KeepComonents in radius 500?
            /// </summary>
            /// <param name="npc"></param>
            /// <returns></returns>
            public bool IsThereKeepComponents(GameLiving npc)
            {

                foreach (AbstractGameKeep keep in KeepMgr.GetAllKeeps())
                {
                    foreach (GameKeepComponent component in keep.KeepComponents)
                    {

                        if (component != null && m_npc != null && m_npc.IsWithinRadius(component, 1500))
                        {
                           // Console.Write("keep in der nähe");
                            return true;
                        }
                    }
                }
                return false;
            }



            public void ZephyrCheckLOS(GameLiving player, ushort response, ushort targetOID)
            {
                if ((response & 0x100) == 0x100)
                    m_npc.WalkTo(new Point3D(m_loc.X, m_loc.Y, m_loc.Z), 120);
                
            }

            public virtual IPoint3D GetTargetLoc()
            {
                double targetX = 0;
                double targetY = 0;

                if (IsThereKeepComponents(m_npc))
                {
                    targetX = m_npc.X + Util.Random(-250, 250);
                    targetY = m_npc.Y + Util.Random(-250, 250);
                }
                else
                {
                    targetX = m_npc.X + Util.Random(-1500, 1500);
                    targetY = m_npc.Y + Util.Random(-1500, 1500);
                }
                return new Point3D((int)targetX, (int)targetY, m_npc.Z);
            }

            public override int CalculateSpellResistChance(GameLiving target)
            {
                return 0;
            }

            public FZSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        }
        #endregion

        //no shared timer
        #region Sojourner-9
        [SpellHandlerAttribute("Phaseshift")]
        public class PhaseshiftHandler : MasterlevelHandling
        {
            private int endurance;

            public override bool CheckBeginCast(GameLiving selectedTarget)
            {
                endurance = 50;

                if (Caster.Endurance < endurance)
                {
                    MessageToCaster("You need 50 endurance for this spell!!", eChatType.CT_System);
                    return false;
                }

                return base.CheckBeginCast(selectedTarget);
            }

            public override void OnEffectStart(GameSpellEffect effect)
            {
                base.OnEffectStart(effect);

                GameEventMgr.AddHandler(Caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                Caster.Endurance -= endurance;


             
                GameSpellEffect speed2 = Spells.SpellHandler.FindEffectOnTarget(Caster, "SpeedOfTheRealm");
                if (speed2 != null)
                    speed2.Cancel(false);
                GameSpellEffect speed3 = Spells.SpellHandler.FindEffectOnTarget(Caster, "ArcherSpeedEnhancement");
                if (speed3 != null)
                    speed3.Cancel(false);
                GameSpellEffect speed4 = Spells.SpellHandler.FindEffectOnTarget(Caster, "SpeedEnhancement");
                if (speed4 != null)
                    speed4.Cancel(false);

               

                GameSpellEffect DecreaseLOP = Spells.SpellHandler.FindEffectOnTarget(Caster, "HereticDamageSpeedDecreaseLOP");

                if (DecreaseLOP != null)
                {
                    DecreaseLOP.Cancel(false);
                }
            }

            private void OnAttack(DOLEvent e, object sender, EventArgs arguments)
            {
                GameLiving living = sender as GameLiving;
                if (living == null) return;
                AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
                AttackData ad = null;
                if (attackedByEnemy != null)
                    ad = attackedByEnemy.AttackData;

                if (ad.Attacker is GamePlayer)
                {
                    GameSpellEffect effect3 = Spells.SpellHandler.FindEffectOnTarget(living, "StyleSpeedDecrease");
                    if (effect3 != null)
                        effect3.Cancel(false);
                    GameSpellEffect speed2 = Spells.SpellHandler.FindEffectOnTarget(living, "SpeedOfTheRealm");
                    if (speed2 != null)
                        speed2.Cancel(false);
                    GameSpellEffect speed3 = Spells.SpellHandler.FindEffectOnTarget(living, "ArcherSpeedEnhancement");
                    if (speed3 != null)
                        speed3.Cancel(false);
                    GameSpellEffect speed4 = Spells.SpellHandler.FindEffectOnTarget(Caster, "SpeedEnhancement");
                    if (speed4 != null)
                        speed4.Cancel(false);
                    GameSpellEffect effect4 = Spells.SpellHandler.FindEffectOnTarget(living, "SpeedDecrease");
                    if (effect4 != null)
                        effect4.Cancel(false);
                    GameSpellEffect effect5 = Spells.SpellHandler.FindEffectOnTarget(living, "HereticPiercingMagic");
                    if (effect5 != null)
                        effect5.Cancel(false);
                   
                    GameSpellEffect DecreaseLOP = Spells.SpellHandler.FindEffectOnTarget(living, "HereticDamageSpeedDecreaseLOP");

                    if (DecreaseLOP != null)
                    {
                        DecreaseLOP.Cancel(false);
                    }

                    ad.Damage = 0;
                    ad.CriticalDamage = 0;
                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                    GamePlayer player = ad.Attacker as GamePlayer;
                    player.Out.SendMessage(living.Name + " is Phaseshifted and can't be attacked!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                }
            }

            public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
            {
                GameEventMgr.RemoveHandler(Caster, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(OnAttack));
                return base.OnEffectExpires(effect, noMessages);
            }

            public override bool HasPositiveEffect
            {
                get
                {
                    return false;
                }
            }

            public override int CalculateSpellResistChance(GameLiving target)
            {
                return 0;
            }

            // constructor
            public PhaseshiftHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
        }
        #endregion

        //no shared timer
        #region Sojourner-10
        [SpellHandlerAttribute("Groupport")]
        public class Groupport : MasterlevelHandling
        {
            public Groupport(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

            public override bool CheckBeginCast(GameLiving selectedTarget)
            {
                if (Caster is GamePlayer && Caster.CurrentRegionID == 51 && ((GamePlayer)Caster).DBCharacter.BindRegion == 51)
                {

                    if (Caster.CurrentRegionID == 51)
                    {
                        MessageToCaster("You can't use this Ability here", eChatType.CT_SpellResisted);
                        return false;
                    }
                    else
                    {
                        MessageToCaster("Bind in another Region to use this Ability", eChatType.CT_SpellResisted);
                        return false;
                    }
                }
                return base.CheckBeginCast(selectedTarget);
            }

            public override void FinishSpellCast(GameLiving target)
            {
                base.FinishSpellCast(target);
            }

            public override void OnDirectEffect(GameLiving target, double effectiveness)
            {
                if (target == null) return;
                if (!target.IsAlive || target.ObjectState != GameLiving.eObjectState.Active) return;

                GamePlayer player = Caster as GamePlayer;
                if (player != null)
                {
                    if (player.Group == null)
                    {
                        player.Out.SendMessage("You are not a part of a group!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    if (player.Group != null)
                    {
                        foreach (GamePlayer players in player.Group.GetPlayersInTheGroup())
                        {
                            if (players.IsWithinRadius(Caster, 1000 , false) == false)
                            {
                                player.Out.SendMessage("One of your group members was not in range, the Spell fail!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;
                            }
                            if (GameRelic.IsPlayerCarryingRelic(players))
                            {
                                player.Out.SendMessage("One of your group members has a Relic, the Spell fail!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                                return;

                            }

                            if (GameRelic.IsPlayerCarryingRelic(player))
                            {
                                player.Out.SendMessage("You can't teleport, you has a Relic, the Spell fail!.", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
                                return;
                            }
                        }
                    }


                    if (player.Group.IsGroupInCombat())
                    {
                        player.Out.SendMessage("You can't teleport a group that is in combat, the Spell fail!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    else
                    {
                        foreach (GamePlayer pl in player.Group.GetPlayersInTheGroup())
                        {
                            if (pl != null) //if (pl != null)
                            {
                                //Anti Buffbot for PVM Zones
                                GameSpellEffect effect1 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "DexterityQuicknessBuff");
                                GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "StrengthConstitutionBuff");
                                GameSpellEffect effect3 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "StrengthBuff");
                                GameSpellEffect effect4 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "ConstitutionBuff");
                                GameSpellEffect effect5 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "DexterityBuff");
                                GameSpellEffect effect6 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "AcuityBuff");
                                GameSpellEffect effect7 = SpellHandler.FindEffectOnTarget((GamePlayer)pl, "ArmorFactorBuff");

                                if ((pl.CurrentRegion.IsRvR || Caster.CurrentRegion.IsRvR))
                                {
                                    if ((pl.CurrentRegionID == 163 || pl.CurrentRegionID == 233 || pl.CurrentRegionID == 244 || pl.CurrentRegionID == 245) && ServerProperties.Properties.BUFFBOTS_ONLY_RVR == true)
                                    {
                                        if (effect1 != null) effect1.Cancel(false);
                                        if (effect2 != null) effect2.Cancel(false);
                                        if (effect3 != null) effect3.Cancel(false);
                                        if (effect4 != null) effect4.Cancel(false);
                                        if (effect5 != null) effect5.Cancel(false);
                                        if (effect6 != null) effect6.Cancel(false);
                                        if (effect7 != null) effect7.Cancel(false);
                                    }



                                    SendEffectAnimation(pl, 0, false, 1);
                                    pl.MoveTo((ushort)player.DBCharacter.BindRegion, player.DBCharacter.BindXpos, player.DBCharacter.BindYpos, player.DBCharacter.BindZpos, (ushort)player.DBCharacter.BindHeading);
                                }

                                if (!pl.CurrentRegion.IsRvR && !Caster.CurrentRegion.IsRvR)
                                {
                                    SendEffectAnimation(pl, 0, false, 1);
                                    pl.MoveTo((ushort)player.DBCharacter.BindRegion, player.DBCharacter.BindXpos, player.DBCharacter.BindYpos, player.DBCharacter.BindZpos, (ushort)player.DBCharacter.BindHeading);
                                }

                            }



                        }
                    }

        #endregion
                }
            }
        }
    }
}
      
   

    



             
       






                       
   

        
 



       
  


using System;
using System.Collections;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;
using DOL.Database;
using DOL.Language;



namespace DOL.GS.Spells
{

    //http://www.camelotherald.com/masterlevels/ma.php?ml=Convoker
    //no shared timer
    #region Convoker-1
    [SpellHandlerAttribute("SummonWood")]
    public class SummonWoodSpellHandler : SummonItemSpellHandler
    {
        public SummonWoodSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>("mysticwood_wooden_boards");
            if (template != null)
            {
                items.Add(GameInventoryItem.Create<ItemTemplate>(template));
                foreach (InventoryItem item in items)
                {
                    if (item.IsStackable)
                    {
                        item.Count = Util.Random(5, 20);
                        item.Weight = item.Count * item.Weight;
                    }
                }
            }
        }
    }
    #endregion

    //no shared timer
    #region Convoker-2
    [SpellHandlerAttribute("PrescienceNode")]
    public class PrescienceNodeSpellHandler : FontSpellHandler
    {
        // constructor
        public PrescienceNodeSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnNPC = false;
            ApplyOnCombat = true;

            //Construct a new font.
            font = new GameFont();
            font.Model = 2584;
            font.Name = spell.Name;
            font.Realm = caster.Realm;
            font.X = caster.X;
            font.Y = caster.Y;
            font.Z = caster.Z;
            font.CurrentRegionID = caster.CurrentRegionID;
            font.Heading = caster.Heading;
            font.Owner = (GamePlayer)caster;

            // Construct the font spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Description = "Ground targeted ward(1000 radius) that can make enemy stealthers visible if they is moving, without removing their stealth.";
            dbs.Icon = 7312;
            dbs.ClientEffect = 7312;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "Prescience";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 1000;
            dbs.SpellGroup = 2245;
            s = new Spell(dbs, 50);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    [SpellHandlerAttribute("Prescience")]
    public class PrescienceSpellHandler : SpellHandler
    {
        const string SpamTime = "CONVOKERSPAM";

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);

            if (!effect.Owner.IsAlive)
                return;



            if (effect.Owner != null && effect.Owner is GamePlayer && effect.Owner.IsStealthed)
            {
                //anti Spam function
                long UPDATETICK = effect.Owner.TempProperties.getProperty<long>(SpamTime);
                long changeTime = effect.Owner.CurrentRegion.Time - UPDATETICK;

                if (changeTime > 15 * 1000 || UPDATETICK == 0)
                {
                    effect.Owner.TempProperties.setProperty(SpamTime, effect.Owner.CurrentRegion.Time);
                    
                    ((GamePlayer)effect.Owner).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)effect.Owner).Client.Account.Language, "SpellHandler.ConvokerPrescienceMessage"), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);

                }
                else
                {
                   //no message
                }
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
         return base.OnEffectExpires(effect, noMessages);
        }
       public PrescienceSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
   
    #endregion

    //no shared timer
    #region Convoker-3
    [SpellHandlerAttribute("PowerTrap")]
    public class PowerTrapSpellHandler : MineSpellHandler
    {
        // constructor
        public PowerTrapSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new mine.
            mine = new GameMine();
            mine.Model = 2590;
            mine.Name = spell.Name;
            mine.Realm = caster.Realm;
            mine.X = caster.X;
            mine.Y = caster.Y;
            mine.Z = caster.Z;
            mine.CurrentRegionID = caster.CurrentRegionID;
            mine.Heading = caster.Heading;
            mine.Owner = (GamePlayer)caster;

            // Construct the mine spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Icon = 7313;
            dbs.ClientEffect = 7313;
            dbs.Damage =  spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Description = "Rune that drains power from enemies when it detonates (works only on enemies that have a power pool)";
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "PowerRend";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    //no shared timer
    #region Convoker-4
    [SpellHandlerAttribute("SpeedWrapWard")]
    public class SpeedWrapWardSpellHandler : FontSpellHandler
    {
        // constructor
        public SpeedWrapWardSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            ApplyOnCombat = true;
            Friendly = false;

            //Construct a new mine.
            font = new GameFont();
            font.Model = 2586;
            font.Name = spell.Name;
            font.Realm = caster.Realm;
            font.X = caster.X;
            font.Y = caster.Y;
            font.Z = caster.Z;
            font.CurrentRegionID = caster.CurrentRegionID;
            font.Heading = caster.Heading;
            font.Owner = (GamePlayer)caster;

            // Construct the mine spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Description = "Ward that negates the effects of speed boosts for enemies who pass through it.";
            dbs.Icon = 7237;
            dbs.ClientEffect = 7237;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "SpeedWrap";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = (ushort)spell.Radius;
            dbs.SpellGroup = 9;
            s = new Spell(dbs, 50);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            heal = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    [SpellHandlerAttribute("SpeedWrap")]
    public class SpeedWrapSpellHandler : SpellHandler
    {
        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            if (!effect.Owner.IsAlive)
                return;
            if (effect.Owner is GamePlayer)
                ((GamePlayer)effect.Owner).Out.SendUpdateMaxSpeed();
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner is GamePlayer)
                ((GamePlayer)effect.Owner).Out.SendUpdateMaxSpeed();
            return base.OnEffectExpires(effect, noMessages);
        }
        public SpeedWrapSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
    #endregion

    //shared timer 1
    #region Convoker-5
    [SpellHandlerAttribute("SummonWarcrystal")]
    public class SummonWarcrystalSpellHandler : SummonItemSpellHandler
    {
        public SummonWarcrystalSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            string ammo = "";
            switch (Util.Random(1, 2))
            {
                case 1:
                    ammo = "mystic_ammo_heat";
                    break;
                case 2:
                    ammo = "mystic_ammo_cold";
                    break;
            }
            ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(ammo);
            if (template != null)
            {
                items.Add(GameInventoryItem.Create<ItemTemplate>(template));
                foreach (InventoryItem item in items)
                {
                    if (item.IsStackable)
                    {
                        item.Count = 1;
                        item.Weight = item.Count * item.Weight;
                    }
                }
            }
        }
    }
    #endregion

    //shared timer 1
    #region Convoker-6
    [SpellHandlerAttribute("Battlewarder")]
    public class BattlewarderSpellHandler : SpellHandler
    {
        private GameNPC warder;
        private GameSpellEffect m_effect;
        /// <summary>
        /// Execute battle warder summon spell
        /// </summary>
        /// <param name="target"></param>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }
        public override bool IsOverwritable(GameSpellEffect compare)
        {
            return false;
        }
        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            m_effect = effect;
            if (effect.Owner == null || !effect.Owner.IsAlive)
                return;

            if ((effect.Owner is GamePlayer))
            {
                GamePlayer casterPlayer = effect.Owner as GamePlayer;
                if (casterPlayer.GroundTarget != null && casterPlayer.GroundTargetInView)
                {
                    GameEventMgr.AddHandler(casterPlayer, GamePlayerEvent.Moving, new DOLEventHandler(PlayerMoves));
                    GameEventMgr.AddHandler(warder, GameLivingEvent.Dying, new DOLEventHandler(BattleWarderDie));
                    GameEventMgr.AddHandler(casterPlayer, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerMoves));
                    GameEventMgr.AddHandler(casterPlayer, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerMoves));
                    warder.X = casterPlayer.GroundTarget.X;
                    warder.Y = casterPlayer.GroundTarget.Y;
                    warder.Z = casterPlayer.GroundTarget.Z;
                    warder.AddBrain(new MLBrain());
                    warder.AddToWorld();
                }
                else
                {
                    MessageToCaster("Your area target is out of range.  Set a closer ground position.", eChatType.CT_SpellResisted);
                    effect.Cancel(false);
                }
            }
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (warder != null)
            {
                GameEventMgr.RemoveHandler(warder, GameLivingEvent.Dying, new DOLEventHandler(BattleWarderDie));
                warder.RemoveBrain(warder.Brain);
                warder.Health = 0;
                warder.Delete();
            }
            if ((effect.Owner is GamePlayer))
            {
                GamePlayer casterPlayer = effect.Owner as GamePlayer;
                GameEventMgr.RemoveHandler(casterPlayer, GamePlayerEvent.Moving, new DOLEventHandler(PlayerMoves));
                GameEventMgr.RemoveHandler(casterPlayer, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerMoves));
                GameEventMgr.RemoveHandler(casterPlayer, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerMoves));
            }
            effect.Owner.EffectList.Remove(effect);
            return base.OnEffectExpires(effect, noMessages);
        }

        // Event : player moves, lose focus
        public void PlayerMoves(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;
            if (player == null) return;
            if (e == GamePlayerEvent.Moving)
            {
                MessageToCaster("Your concentration fades", eChatType.CT_SpellExpires);
                OnEffectExpires(m_effect, true);
                return;
            }
        }

        // Event : Battle warder has died
        private void BattleWarderDie(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC kWarder = sender as GameNPC;
            if (kWarder == null) return;
            if (e == GameLivingEvent.Dying)
            {
                MessageToCaster("Your Battle Warder has fallen!", eChatType.CT_SpellExpires);
                OnEffectExpires(m_effect, true);
                return;
            }
        }
        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!base.CheckBeginCast(selectedTarget)) return false;
            if (!(m_caster.GroundTarget != null && m_caster.GroundTargetInView))
            {
                MessageToCaster("Your area target is out of range.  Set a closer ground position.", eChatType.CT_SpellResisted);
                return false;
            }
            return true;

        }
        public BattlewarderSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            warder = new GameNPC();
            //Fill the object variables
            warder.CurrentRegion = caster.CurrentRegion;
            warder.Heading = (ushort)((caster.Heading + 2048) % 4096);
            warder.Level = 70;
            warder.Realm = caster.Realm;
            warder.Name = "Battle Warder";
            warder.Model = 993;
            warder.CurrentSpeed = 0;
            warder.MaxSpeedBase = 0;
            warder.GuildName = "";
            warder.Size = 50;
        }
    }
    #endregion

    //no shared timer
    #region Convoker-7
    [SpellHandlerAttribute("DissonanceTrap")]
    public class DissonanceTrapSpellHandler : MineSpellHandler
    {
        // constructor
        public DissonanceTrapSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            //Construct a new mine.
            mine = new GameMine();
            mine.Model = 2588;
            mine.Name = spell.Name;
            mine.Realm = caster.Realm;
            mine.X = caster.X;
            mine.Y = caster.Y;
            mine.Z = caster.Z;
            mine.CurrentRegionID = caster.CurrentRegionID;
            mine.Heading = caster.Heading;
            mine.Owner = (GamePlayer)caster;

            // Construct the mine spell
            dbs = new DBSpell();
            dbs.Name = spell.Name;
            dbs.Description = "Rune that does 40% essence damage to the enemy when it detonates.";
            dbs.Icon = 7255;
            dbs.ClientEffect = 7255;
            dbs.Damage = spell.Damage;
            dbs.DamageType = (int)spell.DamageType;
            dbs.Target = "Enemy";
            dbs.Radius = 0;
            dbs.Type = "DirectDamage";
            dbs.Value = spell.Value;
            dbs.Duration = spell.ResurrectHealth;
            dbs.Frequency = spell.ResurrectMana;
            dbs.Pulse = 0;
            dbs.PulsePower = 0;
            dbs.LifeDrainReturn = spell.LifeDrainReturn;
            dbs.Power = 0;
            dbs.CastTime = 0;
            dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
            sRadius = 350;
            s = new Spell(dbs, 1);
            sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
            trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
        }
    }
    #endregion

    //no shared timer
    #region Convoker-8
    [SpellHandler("BrittleGuard")]
    public class BrittleGuardSpellHandler : MasterlevelHandling
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		GameNPC summoned = null;
    	GameSpellEffect beffect = null;
        public BrittleGuardSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {

        }
        public override bool HasPositiveEffect
        {
            get { return true; }
        }
       
       
        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
            {
                return;
            }
            GameSpellEffect effect = SpellHandler.FindEffectOnTarget(player, "BrittleGuard");
            if (effect != null)
            {
                MessageToLiving((GameLiving)player, "You cant have more as one Battle Guard!", eChatType.CT_SpellResisted);
                return;
            }

            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
                MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
                return;
            }

            Point2D summonloc;
            beffect = CreateSpellEffect(target, effectiveness);
            {
                summonloc = target.GetPointFromHeading( target.Heading, 64 );

                BrittleBrain controlledBrain = new BrittleBrain(player);
                controlledBrain.IsMainPet = false;
                summoned = new GameNPC(template);
                summoned.SetOwnBrain(controlledBrain);
                summoned.X = summonloc.X;
                summoned.Y = summonloc.Y;
                summoned.Z = target.Z;
                summoned.CurrentRegion = target.CurrentRegion;
                summoned.Heading = (ushort)((target.Heading + 2048) % 4096);
                summoned.Realm = target.Realm;
                summoned.CurrentSpeed = 0;
                summoned.Size = 10;
                summoned.Strength = 1;
                summoned.Level = 1;
                summoned.AddToWorld();
                controlledBrain.AggressionState = eAggressionState.Passive;
                GameEventMgr.AddHandler(summoned, GameLivingEvent.Dying, new DOLEventHandler(GuardDie));
                beffect.Start(Caster);     
            }
        }
        private void GuardDie(DOLEvent e, object sender, EventArgs args)
        {
        	GameNPC bguard = sender as GameNPC;
        	if(bguard==summoned)
        	{
        		GameEventMgr.RemoveHandler(summoned, GameLivingEvent.Dying, new DOLEventHandler(GuardDie));
				beffect.Cancel(false);
        	}				
        }
        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {       
        	if(summoned!=null)
        	{
           		summoned.Health = 0; // to send proper remove packet
            	summoned.Delete();
        	}
            return base.OnEffectExpires(effect, noMessages);
        }
    }
    #endregion

    //no shared timer
    #region Convoker-9
    [SpellHandlerAttribute("SummonMastery")]
    public class Convoker9Handler : MasterlevelHandling
    //public class Convoker9Handler : MasterlevelBuffHandling
    {
        private GameNPC m_living;
        private GamePlayer m_player;


        public override bool CheckBeginCast(GameLiving target)
        {
            
            if (target == null)
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouNeedATarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    // MessageToCaster("You must select a target for this spell!", eChatType.CT_SpellResisted);
                }
                return false;
            }

            if (target is GameNPC == false)
                return false;

            if (!target.IsWithinRadius(Caster, 1000))
            {
                if (Caster is GamePlayer)
                {
                    (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourTargetIsToFarAway"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    //MessageToCaster("Your target is too far away", eChatType.CT_SpellResisted);
                }
                return false;
            }

            return base.CheckBeginCast(target);
        }


        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            foreach (JuggernautEffect jg in target.EffectList.GetAllOfType(typeof(JuggernautEffect)))
            {
                if (jg != null)
                {
                    MessageToCaster("Your Pet already has an ability of this type active", eChatType.CT_SpellResisted);
                    return;
                }
            }
            if  (m_player != null && m_player.IsCharcterClass(eCharacterClass.Necromancer))
 	  	                         { // Caster is a Necro
 	  	                                 NecromancerPet necroPet = target as NecromancerPet;
 	  	                                 if (necroPet == null || necroPet.Owner == m_player)
	  	                                 { // Caster is a Nekro and his Target is his Own Pet
 	  	                                         MessageToCaster("You cant use this ability on your own Pet", eChatType.CT_SpellResisted);
	  	                                         return;
	  	                                 }
 	  	                         }
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            if (effect.Owner != null && effect.Owner.IsAlive && effect.Owner.ObjectState == GameLiving.eObjectState.Active)
            {
                GameLiving pet = effect.Owner as GameLiving;

                byte oldhealthPercent = pet.HealthPercent;


                if (pet.CurrentRegion.IsRvR)
                {
                    m_living = m_player.ControlledBrain.Body;
                    m_living.Level += 10;
                    m_living.BaseBuffBonusCategory[(int)eProperty.Strength] += ServerProperties.Properties.PET_ML9_STRENGTH;
                    m_living.BaseBuffBonusCategory[(int)eProperty.MeleeDamage] += 15;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorFactor] += 5;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] += 75;
                    m_living.Size += 20;

                    while (m_living.HealthPercent < oldhealthPercent)
                    {
                        //if (m_living.HealthPercent < oldhealthPercent)

                        m_living.Health++;
                        if (m_living.HealthPercent != oldhealthPercent)
                            m_living.BroadcastUpdate();

                    }
                    base.OnEffectStart(effect);

                }
                else
                {
                    m_living = m_player.ControlledBrain.Body;
                    m_living.Level += 20;
                    m_living.BaseBuffBonusCategory[(int)eProperty.MeleeDamage] += 15;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorFactor] += 5;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] += 75;
                    m_living.Size += 40;

                    while (m_living.HealthPercent < oldhealthPercent)
                    {
                        //if (m_living.HealthPercent < oldhealthPercent)

                        m_living.Health++;
                        if (m_living.HealthPercent != oldhealthPercent)
                            m_living.BroadcastUpdate();

                    }

                    base.OnEffectStart(effect);
                }
            }
        }





        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            if (effect.Owner != null && effect.Owner.IsAlive && effect.Owner.ObjectState == GameLiving.eObjectState.Active)
            {
                GameLiving pet = effect.Owner as GameLiving;

                byte oldhealthPercent = pet.HealthPercent;



                //effect.Owner.SilencedTime = effect.Owner.CurrentRegion.Time + CalculateEffectDuration(effect.Owner, Caster.Effectiveness);
                if (pet.CurrentRegion.IsRvR)
                {
                    m_living.Level -= 10;
                    m_living.BaseBuffBonusCategory[(int)eProperty.Strength] -= ServerProperties.Properties.PET_ML9_STRENGTH;
                    m_living.BaseBuffBonusCategory[(int)eProperty.MeleeDamage] -= 15;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorFactor] -= 5;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] -= 75;
                    m_living.Size -= 20;

                    while (m_living.HealthPercent > oldhealthPercent)
                    {
                        //if (m_living.HealthPercent < oldhealthPercent)

                        m_living.Health--;
                        if (m_living.HealthPercent != oldhealthPercent)
                            m_living.BroadcastUpdate();

                    }

                    return base.OnEffectExpires(effect, noMessages);
                }
                else
                {
                    m_living.Level -= 20;
                    m_living.BaseBuffBonusCategory[(int)eProperty.MeleeDamage] -= 15;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorFactor] -= 5;
                    m_living.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] -= 75;
                    m_living.Size -= 40;
                    while (m_living.HealthPercent > oldhealthPercent)
                    {
                        //if (m_living.HealthPercent < oldhealthPercent)

                        m_living.Health--;
                        if (m_living.HealthPercent != oldhealthPercent)
                            m_living.BroadcastUpdate();

                    }
                    return base.OnEffectExpires(effect, noMessages);
                }
               
            }
            return 0;
        }
        
        public Convoker9Handler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line)
 	  	                 {
 	                         m_player = caster as GamePlayer;
	  	                 }
    }
    #endregion


    //no shared timer
    #region Convoker-10
    [SpellHandler("SummonTitan")]
    public class Convoker10SpellHandler : SpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private int x, y, z;
		GameNPC summoned = null;
		RegionTimer m_growTimer;
		private const int C_GROWTIMER = 5000;
		
        public Convoker10SpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (Caster is GamePlayer && (Caster as GamePlayer).HasGroundTarget() == false)
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YouNeedAGroundTarget"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (!CheckCastLocation())
                return false;
            return base.CheckBeginCast(selectedTarget);
        }

        /// <summary>
        /// called after normal spell cast is completed and effect has to be started
        /// </summary>
        public override void FinishSpellCast(GameLiving target)
        {
            m_caster.Mana -= PowerCost(target);
            base.FinishSpellCast(target);
        }

        /// <summary>
        /// Apply effect on target or do spell action if non duration spell
        /// </summary>
        /// <param name="target">target that gets the effect</param>
        /// <param name="effectiveness">factor from 0..1 (0%-100%)</param>
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            GamePlayer player = Caster as GamePlayer;
            if (player == null)
            {
                return;
            }

            INpcTemplate template = NpcTemplateMgr.GetTemplate(Spell.LifeDrainReturn);
            if (template == null)
            {
                if (log.IsWarnEnabled)
                    log.WarnFormat("NPC template {0} not found! Spell: {1}", Spell.LifeDrainReturn, Spell.ToString());
                MessageToCaster("NPC template " + Spell.LifeDrainReturn + " not found!", eChatType.CT_System);
                return;
            }
            GameSpellEffect effect = CreateSpellEffect(target, effectiveness);
            TitanBrain controlledBrain = new TitanBrain(player);
            controlledBrain.IsMainPet = false;
            controlledBrain.WalkState = eWalkState.Stay;
            summoned = new GameNPC(template);
            summoned.SetOwnBrain(controlledBrain);
            //Suncheck:
            //	Is needed, else it can cause error (i.e. /cast-command)
            if (x == 0 || y == 0)
            	CheckCastLocation();
            summoned.X = x; 
            summoned.Y = y;
            summoned.Z = z;
            summoned.CurrentRegion = player.CurrentRegion;
            summoned.Heading = (ushort)((player.Heading + 2048) % 4096);
            summoned.Realm = player.Realm;
            summoned.CurrentSpeed = 0;
            summoned.Size = 10;
            summoned.Level = 100;
            summoned.Flags |= GameNPC.eFlags.PEACE;
            summoned.AddToWorld();
            controlledBrain.AggressionState = eAggressionState.Aggressive;
            effect.Start(summoned);
            m_growTimer = new RegionTimer((GameObject)m_caster, new RegionTimerCallback(TitanGrows), C_GROWTIMER);
        }
        
        // Make titan growing, and activate it on completition
        private int TitanGrows(RegionTimer timer)
        {
        	if(summoned != null && summoned.Size != 70)
        	{
        		summoned.Size +=8;
                if (summoned.Size > 70)
                {
                    summoned.Size = 70;
                }
                return C_GROWTIMER;
        	}
        	else
        	{
        		summoned.Flags = 0;
        		m_growTimer.Stop();
                m_growTimer = null;
        	}
            return 0;
        }

        private bool CheckCastLocation()
        {
            x = Caster.X;
            y = Caster.Y;
            z = Caster.Z;
            if (Caster is GamePlayer && Caster.GroundTargetInView && Caster.IsWithinRadius(Caster.GroundTarget, Spell.Range) && (Caster as GamePlayer).HasGroundTarget())
            {
                x = Caster.GroundTarget.X;
                y = Caster.GroundTarget.Y;
                z = Caster.GroundTarget.Z;
            }
            else if (Caster is GamePlayer && (Caster as GamePlayer).HasGroundTarget() && (Caster.IsWithinRadius(Caster.GroundTarget, Spell.Range) == false || Caster.GroundTargetInView == false))
            {
                (Caster as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((Caster as GamePlayer).Client.Account.Language, "SpellHandler.YourGroundTargetIsNotInView"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                return false;

            }

            return true;
        }
        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            effect.Owner.Health = 0; // to send proper remove packet
            effect.Owner.Delete();
            return 0;
        }

        public override int CalculateSpellResistChance(GameLiving target)
        {
            return 0;
        }
    }
    #endregion


    #region PowerRend
   //ist in Spells
    #endregion
}

#region BrittleBrain
namespace DOL.AI.Brain
{
    public class BrittleBrain : ControlledNpcBrain
    {
        public BrittleBrain(GameLiving owner)
            : base(owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
        }

        public override void FollowOwner()
        {
            Body.StopAttack();
            Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
        }
    }
}
#endregion

#region Titanbrain

namespace DOL.AI.Brain
{
    public class TitanBrain : ControlledNpcBrain, IControlledBrain
    {
        private GameLiving m_target;

        public TitanBrain(GameLiving owner)
            : base(owner)
        {
        }

        public override int ThinkInterval
        {
            get { return 2000; }
        }

        public GameLiving Target
        {
            get { return m_target; }
            set { m_target = value; }
        }

        #region AI

        public override bool Start()
        {
            if (!base.Start()) return false;
            return true;
        }

        public override bool Stop()
        {
            if (!base.Stop()) return false;
            return true;
        }

        private IList FindTarget()
        {
            ArrayList list = new ArrayList();

            foreach (GamePlayer o in Body.GetPlayersInRadius((ushort)Body.AttackRange))
            {
                GamePlayer p = o as GamePlayer;

                if (GameServer.ServerRules.IsAllowedToAttack(Body, p, true))
                    list.Add(p);
            }
            return list;
        }

        public override void Think()
        {
            if (Body.TargetObject is GameNPC)
                Body.TargetObject = null;

            if (Body.AttackState)
                return;

            IList enemies = new ArrayList();
            if (Target == null)
                enemies = FindTarget();
            else if (!Body.IsWithinRadius(Target, Body.AttackRange))
                enemies = FindTarget();
            else if (!Target.IsAlive)
                enemies = FindTarget();
            if (enemies.Count > 0 && Target == null)
            {
                //pick a random target...
                int targetnum = Util.Random(0, enemies.Count - 1);

                //Choose a random target.
                Target = enemies[targetnum] as GameLiving;
            }
            else if (enemies.Count < 1)
            {
                WalkState = eWalkState.Stay;
                enemies = FindTarget();
            }

            if (Target != null)
            {
                if (!Target.IsAlive)
                {
                    Target = null;
                }
                else if (Body.IsWithinRadius(Target, Body.AttackRange))
                {
                    Body.TargetObject = Target;
                    Goto(Target);
                    Body.StartAttack(Target);
                }
                else
                {
                    Target = null;
                }
            }
        }
        #endregion
    }
}
#endregion

#region MLBrain
public class MLBrain : GuardBrain
{
    public MLBrain() : base() { }

    public override int AggroRange
    {
        get { return 400; }
    }
    protected override void CheckNPCAggro()
    {
        //Check if we are already attacking, return if yes
        if (Body.AttackState)
            return;

        foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
        {
            if (m_aggroTable.ContainsKey(npc))
                continue; // add only new npcs
            if ((npc.Flags & GameNPC.eFlags.FLYING) != 0)
                continue; // let's not try to attack flying mobs
            if (!GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
                continue;
            if (!npc.IsWithinRadius(Body, AggroRange))
                continue;

            if (!(npc.Brain is IControlledBrain || npc is GameGuard))
                continue;

            AddToAggroList(npc, npc.Level << 1);
            return;
        }
    }
}
#endregion

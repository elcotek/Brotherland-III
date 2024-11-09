//Andraste BuffBot v1.0 - by Vico

using System.Collections;
using DOL.Database;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using System;
using DOL.GS.Keeps;
using DOL.AI;
using DOL.AI.Brain;



namespace DOL.GS
{

    public class BuffBot : GameNPC
    {

        public BuffBot() : base() { Flags |= GameNPC.eFlags.PEACE; }
        private const string CURED_SPELL_TYPE = GlobalSpells.PvERessurectionIllnessSpellType;
        protected const string MOVEMENT = "movement";
        public override int Concentration { get { return 100000; } }
        public override int Mana { get { return 100000; } }
        private static ArrayList m_baseSpells = null;

        public static ArrayList BaseBuffs
        {
            get
            {
                if (m_baseSpells == null)
                {
                    m_baseSpells = new ArrayList();
                    m_baseSpells.Add(SpeedEnhancement);
                    m_baseSpells.Add(BotStrBuff);
                    m_baseSpells.Add(BotConBuff);
                    m_baseSpells.Add(BotDexBuff);
                }
                return m_baseSpells;
            }
        }

        private static ArrayList m_specSpells = null;
        public static ArrayList SpecBuffs
        {
            get
            {
                if (m_specSpells == null)
                {
                    m_specSpells = new ArrayList();
                    m_specSpells.Add(BotStrConBuff);
                    m_specSpells.Add(BotDexQuiBuff);
                    m_specSpells.Add(BotAcuityBuff);
                    m_specSpells.Add(BotSpecAFBuff);
                }
                return m_specSpells;
            }
        }

        private static ArrayList m_otherSpells = null;
        public static ArrayList OtherBuffs
        {
            get
            {
                if (m_otherSpells == null)
                {
                    m_otherSpells = new ArrayList();
                }
                return m_otherSpells;
            }
        }

        private Queue m_buffs = new Queue();
        private Queue m_buffspet = new Queue();

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;
            TurnTo(player, 3000);
           /*
            if (player.Level < 40)
            {
                SendReply(player, "you need first level 40 for my service!");
                return false;
            }
           */
            if (player.InCombat)
            {
                Say("You are in combat i can't buff you.");
                return false;
            }


            {
                lock (m_buffs.SyncRoot)
                {
                    foreach (Spell s in BaseBuffs) { if (s.SpellType == "AcuityBuff" && player.CharacterClass.ClassType != eClassType.ListCaster) continue; Container con = new Container(s, BotBaseSpellLine, player); m_buffs.Enqueue(con); }
                    foreach (Spell s in SpecBuffs) { Container con = new Container(s, BotSpecSpellLine, player); m_buffs.Enqueue(con); }
                    foreach (Spell s in OtherBuffs) { if (s.SpellType == "PowerRegenBuff" && player.MaxMana == 0) continue; Container con = new Container(s, BotOtherSpellLine, player); m_buffs.Enqueue(con); }
                    if (player.ControlledBrain != null)
                    {
                        foreach (Spell s in BaseBuffs) { Container con = new Container(s, BotBaseSpellLine, player); m_buffspet.Enqueue(con); }
                        foreach (Spell s in SpecBuffs) { Container con = new Container(s, BotSpecSpellLine, player); m_buffspet.Enqueue(con); }
                        foreach (Spell s in OtherBuffs) { Container con = new Container(s, BotOtherSpellLine, player); m_buffspet.Enqueue(con); }
                    }
                }

                if (CurrentSpellHandler == null) CastBuffs();
                if (CurrentSpellHandler == null && player.ControlledBrain != null) CastBuffsPet();
                if (player.CharacterClass.Name != "Vampiir" && player.CharacterClass.Name != "Mauler") player.Mana = player.MaxMana;

            }
           
            return true;
        }

       
        public void CastBuffs()
        {
            Spell BuffSpell = null;
            SpellLine BuffSpellLine = null;
            GameLiving target = null;
            while (m_buffs.Count > 0)
            {
                Container con = (Container)m_buffs.Dequeue();
                BuffSpell = con.Spell;
                target = con.Target;
                BuffSpellLine = con.SpellLine;
                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, BuffSpell, BuffSpellLine);
                if (spellHandler != null) { TargetObject = target; TurnTo(target, 1000); spellHandler.StartSpell(target); }
            }
        }

        public void CastBuffsPet()
        {
            Spell BuffSpell = null;
            SpellLine BuffSpellLine = null;
            GameLiving target = null;
            while (m_buffspet.Count > 0)
            {
                Container con = (Container)m_buffspet.Dequeue();
                BuffSpell = con.Spell;
                target = con.Target;
                BuffSpellLine = con.SpellLine;
                ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, BuffSpell, BuffSpellLine);
                if (spellHandler != null) { TargetObject = target.ControlledBrain.Body; TurnTo(target.ControlledBrain.Body, 1000); spellHandler.StartSpell(target.ControlledBrain.Body); }
            }
        }

        #region SpellCasting
        private static SpellLine m_BotBaseSpellLine;
        private static SpellLine m_BotSpecSpellLine;
        private static SpellLine m_BotOtherSpellLine;

        public static SpellLine BotBaseSpellLine { get { if (m_BotBaseSpellLine == null) m_BotBaseSpellLine = new SpellLine("BotBaseSpellLine", "BuffBot Spells", "unknown", true); return m_BotBaseSpellLine; } }
        public static SpellLine BotSpecSpellLine { get { if (m_BotSpecSpellLine == null) m_BotSpecSpellLine = new SpellLine("BotSpecSpellLine", "BuffBot Spells", "unknown", false); return m_BotSpecSpellLine; } }
        public static SpellLine BotOtherSpellLine { get { if (m_BotOtherSpellLine == null) m_BotOtherSpellLine = new SpellLine("BotOtherSpellLine", "BuffBot Spells", "unknown", true); return m_BotOtherSpellLine; } }

        private static Spell m_speed;
        private static Spell m_basestr;
        private static Spell m_basecon;
        private static Spell m_basedex;
        private static Spell m_strcon;
        private static Spell m_dexqui;
        private static Spell m_acuity;
        private static Spell m_specaf;
        private static Spell m_powereg;

        private static Spell m_haste;
        private static Spell m_hpRegen;
        //private static Spell m_endRegen;
        private static Spell m_heal;
        private static Spell m_resists;
        private static Spell m_resist1;
        private static Spell m_resist2;

        #region Spells
        public static Spell SpeedEnhancement
        {
            get
            {
                if (m_speed == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 10; spell.CastTime = 3; spell.ClientEffect = 2430; spell.Icon = 2430; spell.Duration = 600; spell.Value = 145; spell.Name = "Speed of the Realm";
                    spell.Description = "The movement speed of the target is increased."; spell.SpellID = 2430; spell.Target = "Realm"; spell.Type = "SpeedEnhancement"; spell.EffectGroup = 10; m_speed = new Spell(spell, 50);
                }
                return m_speed;
            }
        }

        public static Spell BotStrBuff
        {
            get
            {
                if (m_basestr == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 4; spell.CastTime = 4; spell.ClientEffect = 1457; spell.Icon = 1457; spell.Duration = 65535; spell.Value = 50; spell.Name = "Strength Buff";
                    spell.Description = "Increases target's Strength."; spell.SpellID = 100002; spell.Target = "Realm"; spell.Type = "StrengthBuff"; spell.EffectGroup = 4; m_basestr = new Spell(spell, 50);
                }
                return m_basestr;
            }
        }

        public static Spell BotConBuff
        {
            get
            {
                if (m_basecon == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 201; spell.CastTime = 5; spell.ClientEffect = 1486; spell.Icon = 1486; spell.Duration = 65535; spell.Value = 44; spell.Name = "Constitution Buff";
                    spell.Description = "Increases target's Constitution."; spell.SpellID = 100003; spell.Target = "Realm"; spell.Type = "ConstitutionBuff"; m_basecon = new Spell(spell, 50);
                }
                return m_basecon;
            }
        }

        public static Spell BotDexBuff
        {
            get
            {
                if (m_basedex == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 202; spell.CastTime = 6; spell.ClientEffect = 1476; spell.Icon = 1476; spell.Duration = 65535; spell.Value = 48; spell.Name = "Dexterity Buff";
                    spell.Description = "Increases target's Dexterity."; spell.SpellID = 100004; spell.Target = "Realm"; spell.Type = "DexterityBuff"; m_basedex = new Spell(spell, 50);
                }
                return m_basedex;
            }
        }

        public static Spell BotStrConBuff
        {
            get
            {
                if (m_strcon == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 204; spell.CastTime = 7; spell.ClientEffect = 1517; spell.Icon = 1517; spell.Duration = 65535; spell.Value = 67; spell.Name = "Strength/Constitution Buff";
                    spell.Description = "Increases Str/Con for a character"; spell.SpellID = 100005; spell.Target = "Realm"; spell.Type = "StrengthConstitutionBuff"; m_strcon = new Spell(spell, 50);
                }
                return m_strcon;
            }
        }

        public static Spell BotDexQuiBuff
        {
            get
            {
                if (m_dexqui == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 203; spell.CastTime = 8; spell.ClientEffect = 1526; spell.Icon = 1526; spell.Duration = 65535; spell.Value = 75; spell.Name = "Dexterity/Quickness Buff";
                    spell.Description = "Decreases Dexterity and Quickness for a character."; spell.SpellID = 100006; spell.Target = "Realm"; spell.Type = "DexterityQuicknessBuff"; m_dexqui = new Spell(spell, 50);
                }
                return m_dexqui;
            }
        }

        public static Spell BotAcuityBuff
        {
            get
            {
                if (m_acuity == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 200; spell.CastTime = 9; spell.ClientEffect = 1538; spell.Icon = 1538; spell.Duration = 65535; spell.Value = 52; spell.Name = "Acuity Buff Buff";
                    spell.Description = "Increases Acuity (casting attribute) for a character."; spell.SpellID = 100007; spell.Target = "Realm"; spell.Type = "AcuityBuff"; m_acuity = new Spell(spell, 50);
                }
                return m_acuity;
            }
        }

        public static Spell BotSpecAFBuff
        {
            get
            {
                if (m_specaf == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 1; spell.CastTime = 4; spell.ClientEffect = 1506; spell.Icon = 5020; spell.Duration = 65535; spell.Value = 52; spell.Name = "Base AF Buff";
                    spell.Description = "Adds to the recipient's Armor Factor (AF),resulting in better protection against some forms of attack. It acts in addition to any armor the target is wearing."; spell.SpellID = 100014; spell.Target = "Realm"; spell.Type = "ArmorFactorBuff"; m_specaf = new Spell(spell, 50);
                }
                return m_specaf;
            }
        }

        public static Spell BotPoweregBuff
        {
            get
            {
                if (m_powereg == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.CastTime = 5; spell.ClientEffect = 979; spell.Icon = 979; spell.Duration = 65535; spell.Value = 4; spell.Name = "Power Regeneration Buff";
                    spell.Description = "Target regenerates power regeneration during the duration of the spell"; spell.SpellID = 100008; spell.Target = "Realm"; spell.Type = "PowerRegenBuff"; m_powereg = new Spell(spell, 50);
                }
                return m_powereg;
            }
        }

        //public static Spell BotDmgaddBuff { get { if(m_dmgadd==null) {
        // DBSpell spell=new DBSpell(); spell.AllowAdd=false; spell.CastTime=6; spell.ClientEffect=16; spell.Icon=16; spell.Duration=65535; spell.Damage=5.0; spell.DamageType=15; spell.Name="Damage Add Buff";
        //spell.Description="Target's melee attacks do additional damage."; spell.SpellID=100009; spell.Target="Realm"; spell.Type="DamageAdd"; m_dmgadd=new Spell(spell,50); }
        //return m_dmgadd; } }

        public static Spell BotHasteBuff
        {
            get
            {
                if (m_haste == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.EffectGroup = 100; spell.CastTime = 7; spell.ClientEffect = 406; spell.Icon = 406; spell.Duration = 65535; spell.Value = 16; spell.Name = "Haste Buff";
                    spell.Description = "Increases the target's combat speed."; spell.SpellID = 100010; spell.Target = "Realm"; spell.Type = "CombatSpeedBuff"; m_haste = new Spell(spell, 50);
                }
                return m_haste;
            }
        }

        public static Spell BotHPRegenBuff
        {
            get
            {
                if (m_hpRegen == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.CastTime = 3; spell.ClientEffect = 1533; spell.Icon = 1533; spell.Duration = 65535; spell.Value = 5; spell.Name = "Health Regeneration Buff";
                    spell.Description = "Target regenerates the given amount of health every tick"; spell.SpellID = 100011; spell.Target = "Realm"; spell.Type = "HealthRegenBuff"; m_hpRegen = new Spell(spell, 50);
                }
                return m_hpRegen;
            }
        }

        //public static Spell BotEndRegenBuff { get { if(m_endRegen==null) {
        // DBSpell spell=new DBSpell(); spell.AllowAdd=false; spell.CastTime=2; spell.ClientEffect=3295; spell.Icon=3295; spell.Duration=65535; spell.Value=1; spell.Name="Endurance Regeneration Buff";
        // spell.Description="Target regenerates endurance during the duration of the spell."; spell.SpellID=100012; spell.Target="Realm"; spell.Type="EnduranceRegenBuff"; m_endRegen=new Spell(spell,50); }
        // return m_endRegen; } }

        public static Spell BotResistsBuff
        {
            get
            {
                if (m_resists == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.CastTime = 1; spell.ClientEffect = 13130; spell.Icon = 13130; spell.Duration = 65535; spell.Value = 8; spell.Name = "Magic Resists";
                    spell.Description = "Target's magical resists are increased."; spell.SpellID = 100014; spell.Target = "Realm"; spell.Type = "VampiirMagicResistance"; m_resists = new Spell(spell, 50);
                }
                return m_resists;
            }
        }
        public static Spell BotResist1Buff
        {
            get
            {
                if (m_resists == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.CastTime = 0; spell.ClientEffect = 14286; spell.Icon = 14286; spell.Duration = 65535; spell.Value = 8; spell.Name = "Heat/Cold/Matter Resists";
                    spell.Description = "Target's magical resists are increased."; spell.SpellID = 100015; spell.Target = "Realm"; spell.Type = "HeatColdMatterBuff"; m_resist1 = new Spell(spell, 50);
                }
                return m_resist1;
            }
        }
        public static Spell BotResist2Buff
        {
            get
            {
                if (m_resists == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.CastTime = 4; spell.ClientEffect = 14285; spell.Icon = 14285; spell.Duration = 65535; spell.Value = 8; spell.Name = "Body/Spirit/Energy Resists";
                    spell.Description = "Target's magical resists are increased."; spell.SpellID = 100016; spell.Target = "Realm"; spell.Type = "BodySpiritEnergyBuff"; m_resist2 = new Spell(spell, 50);
                }
                return m_resist2;
            }
        }

        public static Spell BotHealBuff
        {
            get
            {
                if (m_heal == null)
                {
                    DBSpell spell = new DBSpell(); spell.AllowAdd = false; spell.CastTime = 5; spell.ClientEffect = 1424; spell.Value = 3000; spell.Name = "Heal";
                    spell.Description = "Heals the target."; spell.SpellID = 100013; spell.Target = "Realm"; spell.Type = "Heal"; m_heal = new Spell(spell, 50);
                }
                return m_heal;
            }
        }
        #endregion Spells

        #endregion SpellCasting
        private void SendReply(GamePlayer target, string msg) { target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow); }
        /*
         public class ContainerPet { private Spell m_spell; public Spell Spell { get { return m_spell; } }
         private SpellLine m_spellLine;
         public SpellLine SpellLine { get { return m_spellLine; } }
         private IControlledBrain m_targetpet;
         public GameLiving Target { get { return m_target; } set { m_target=value; } }
         //public IControlledBrain TargetPet { get { return m_targetpet; } set { m_targetpet=value; } }
         public ContainerPet(Spell spell,SpellLine spellLine,IControlledBrain target) { m_spell=spell; m_spellLine=spellLine; m_targetpet=target; } }*/


        public class Container
        {
            private Spell m_spell; public Spell Spell { get { return m_spell; } }
            private SpellLine m_spellLine;
            public SpellLine SpellLine { get { return m_spellLine; } }
            private GameLiving m_target;
            public GameLiving Target { get { return m_target; } set { m_target = value; } }
            public Container(Spell spell, SpellLine spellLine, GameLiving target) { m_spell = spell; m_spellLine = spellLine; m_target = target; }

        }
    }
}


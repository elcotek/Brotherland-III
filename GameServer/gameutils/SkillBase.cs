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
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.Language;
using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DOL.GS
{
    /// <summary>
    /// Skill Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SkillHandlerAttribute : Attribute
    {
        protected string m_keyName;

        public SkillHandlerAttribute(string keyName)
        {
            m_keyName = keyName;
        }

        public string KeyName
        {
            get { return m_keyName; }
        }
    }

    /// <summary>
    /// base class for skills
    /// </summary>
    public abstract class Skill
    {
        protected ushort m_id;
        protected string m_name;
        protected int m_level;
        protected int m_internalID;
        /// <summary>
        /// Internal ID is used for Hardcoded Tooltip.
        /// </summary>
        public virtual int InternalID
        {
            get { return m_internalID; }
            set { m_internalID = value; }
        }
        /// <summary>
        /// Construct a Skill from the name, an id, and a level
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="level"></param>
        public Skill(string name, ushort id, int level)
        {
            m_id = id;
            m_name = name;
            m_level = level;
        }

        /// <summary>
        /// in most cases it is icon id or other specifiing id for client
        /// like spell id or style id in spells
        /// </summary>
        public virtual ushort ID
        {
            get { return m_id; }
        }

        /// <summary>
        /// The Skill Name
        /// </summary>
        public virtual string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        /// <summary>
        /// The Skill Level
        /// </summary>
        public virtual int Level
        {
            get { return m_level; }
            set { m_level = value; }
        }

        /// <summary>
        /// the type of the skill
        /// </summary>
        public virtual eSkillPage SkillType
        {
            get { return eSkillPage.Abilities; }
        }

        /// <summary>
        /// Clone a skill
        /// </summary>
        /// <returns></returns>
        public virtual Skill Clone()
        {
            return (Skill)MemberwiseClone();
        }
    }

    /// <summary>
    /// the named skill is used for identification purposes
    /// the name is strong and must be unique for one type of skill page
    /// so better make the name real unique
    /// </summary>
    public class NamedSkill : Skill
    {
        private readonly string m_keyName;

        /// <summary>
        /// Construct a named skill from the keyname, name, id and level
        /// </summary>
        /// <param name="keyName">The keyname</param>
        /// <param name="name">The name</param>
        /// <param name="id">The ID</param>
        /// <param name="level">The level</param>
        public NamedSkill(string keyName, string name, ushort id, int level)
            : base(name, id, level)
        {
            m_keyName = keyName;
        }

        /// <summary>
        /// Returns the string representation of the Skill
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return new StringBuilder(32)
                .Append("KeyName=").Append(KeyName)
                .Append(", ID=").Append(ID)
                .ToString();
        }

        /// <summary>
        /// strong identification name
        /// </summary>
        public virtual string KeyName
        {
            get { return m_keyName; }
        }
    }

    public class Song : Spell
    {
        public Song(DBSpell spell, int requiredLevel)
            : base(spell, requiredLevel)
        {
        }

        public override eSkillPage SkillType
        {
            get { return eSkillPage.Songs; }
        }
    }

    public class SpellLine : NamedSkill
    {
        protected bool m_isBaseLine;
        protected string m_spec;

        public SpellLine(string keyname, string name, string spec, bool baseline)
            : base(keyname, name, 0, 1)
        {
            m_isBaseLine = baseline;
            m_spec = spec;
        }

        
        public string Spec
        {
            get { return m_spec; }
        }

        public bool IsBaseLine
        {
            get { return m_isBaseLine; }
        }

        public override eSkillPage SkillType
        {
            get { return eSkillPage.Spells; }
        }
    }


    public enum eSkillPage
    {
        Specialization = 0x00,
        Abilities = 0x01,
        Styles = 0x02,
        Spells = 0x03,
        Songs = 0x04,
        AbilitiesSpell = 0x05,
        RealmAbilities = 0x06
    }

    /// <summary>
    ///
    /// </summary>
    public class SkillBase
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static bool m_loaded = false;

        private static readonly ReaderWriterLockSlim m_syncLockUpdates = new ReaderWriterLockSlim();
        private static readonly object m_loadingLock = new object();

        protected static readonly Dictionary<string, Specialization> m_specsByName = new Dictionary<string, Specialization>();
        protected static readonly Dictionary<string, DBAbility> m_abilitiesByName = new Dictionary<string, DBAbility>();
        protected static readonly Dictionary<string, SpellLine> m_spellLinesByName = new Dictionary<string, SpellLine>();

        protected static readonly Dictionary<string, Type> m_abilityActionHandler = new Dictionary<string, Type>();
        protected static readonly Dictionary<string, Type> m_implementationTypeCache = new Dictionary<string, Type>();
        protected static readonly Dictionary<string, Type> m_specActionHandler = new Dictionary<string, Type>();

        // global table for spellLine => List of spells
        protected static Dictionary<string, List<Spell>> m_spellLists = new Dictionary<string, List<Spell>>();

        // global table for spec => List of styles
        protected static readonly Dictionary<string, List<Style>> m_styleLists = new Dictionary<string, List<Style>>();

        // global table for spec => list of spec dependend abilities
        protected static readonly Dictionary<string, List<Ability>> m_specAbilities = new Dictionary<string, List<Ability>>();

        /// <summary>
        /// (procs) global table for style => list of styles dependend spells
        /// [StyleID, [ClassID, DBStyleXSpell]]
        /// ClassID for normal style is 0
        /// </summary>
        protected static readonly Dictionary<int, Dictionary<int, List<DBStyleXSpell>>> m_styleSpells = new Dictionary<int, Dictionary<int, List<DBStyleXSpell>>>();

        // lookup table for styles
        protected static readonly Dictionary<KeyValuePair<int, int>, Style> m_stylesByIDClass = new Dictionary<KeyValuePair<int, int>, Style>();

        // class id => realm ability list
        protected static readonly Dictionary<int, List<RealmAbility>> m_classRealmAbilities = new Dictionary<int, List<RealmAbility>>();

        // all spells by id
        protected static Dictionary<int, Spell> m_spells = new Dictionary<int, Spell>(5000);

        // all DB Spells by id
        protected static Dictionary<int, DBSpell> m_dbSpells = new Dictionary<int, DBSpell>();


        #region Initialization Tables

        /// <summary>
        /// Holds object type to spec convertion table
        /// </summary>
        protected static Dictionary<eObjectType, string> m_objectTypeToSpec = new Dictionary<eObjectType, string>();

        /// <summary>
        /// Holds spec to skill table
        /// </summary>
        protected static Dictionary<string, eProperty> m_specToSkill = new Dictionary<string, eProperty>();

        /// <summary>
        /// Holds spec to focus table
        /// </summary>
        protected static Dictionary<string, eProperty> m_specToFocus = new Dictionary<string, eProperty>();





        protected static Dictionary<string, string> m_languageString = new Dictionary<string, string>();


        /// <summary>
        /// Holds all property types
        /// </summary>
        private static readonly ePropertyType[] m_propertyTypes = new ePropertyType[(int)eProperty.MaxProperty];

        /// <summary>
        /// tables for property names and languages
        /// </summary>
        protected static readonly Dictionary<eProperty, string> m_propertyNamesDE = new Dictionary<eProperty, string>();
        protected static readonly Dictionary<eProperty, string> m_propertyNamesFR = new Dictionary<eProperty, string>();
        protected static readonly Dictionary<eProperty, string> m_propertyNamesEN = new Dictionary<eProperty, string>();
        protected static readonly Dictionary<eProperty, string> m_propertyNamesIT = new Dictionary<eProperty, string>();
        protected static readonly Dictionary<eProperty, string> m_propertyNamesES = new Dictionary<eProperty, string>();
        protected static readonly Dictionary<eProperty, string> m_propertyNamesCZ = new Dictionary<eProperty, string>();
       
        private static readonly ReaderWriterLockSlim m_raceResistLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Table to hold the race resists
        /// </summary>
        protected static readonly Dictionary<int, int[]> m_raceResists = new Dictionary<int, int[]>();

        /// <summary>
        /// Initialize the object type hashtable
        /// </summary>
        private static void InitializeObjectTypeToSpec()
        {
            m_objectTypeToSpec.Add(eObjectType.Staff, Specs.Staff);
            m_objectTypeToSpec.Add(eObjectType.Fired, Specs.ShortBow);

            m_objectTypeToSpec.Add(eObjectType.FistWraps, Specs.Fist_Wraps);
            m_objectTypeToSpec.Add(eObjectType.MaulerStaff, Specs.Mauler_Staff);

            //alb
            m_objectTypeToSpec.Add(eObjectType.CrushingWeapon, Specs.Crush);
            m_objectTypeToSpec.Add(eObjectType.SlashingWeapon, Specs.Slash);
            m_objectTypeToSpec.Add(eObjectType.ThrustWeapon, Specs.Thrust);
            m_objectTypeToSpec.Add(eObjectType.TwoHandedWeapon, Specs.Two_Handed);
            m_objectTypeToSpec.Add(eObjectType.PolearmWeapon, Specs.Polearms);
            m_objectTypeToSpec.Add(eObjectType.Flexible, Specs.Flexible);
            m_objectTypeToSpec.Add(eObjectType.Crossbow, Specs.Crossbow);

            // RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
            if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
            {
                m_objectTypeToSpec.Add(eObjectType.Longbow, Specs.Longbow);
            }
            // RDSandersJR: If we are NOT using old archery it should be SpellDamage
            else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
            {
                m_objectTypeToSpec.Add(eObjectType.Longbow, Specs.Archery);
            }

            //TODO: case 5: abilityCheck = Abilities.Weapon_Thrown); break);

            //mid
            m_objectTypeToSpec.Add(eObjectType.Hammer, Specs.Hammer);
            m_objectTypeToSpec.Add(eObjectType.Sword, Specs.Sword);
            m_objectTypeToSpec.Add(eObjectType.LeftAxe, Specs.Left_Axe);
            m_objectTypeToSpec.Add(eObjectType.Axe, Specs.Axe);
            m_objectTypeToSpec.Add(eObjectType.HandToHand, Specs.HandToHand);
            m_objectTypeToSpec.Add(eObjectType.Spear, Specs.Spear);
            m_objectTypeToSpec.Add(eObjectType.Thrown, Specs.Thrown_Weapons);

            // RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
            if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
            {
                m_objectTypeToSpec.Add(eObjectType.CompositeBow, Specs.CompositeBow);
            }
            // RDSandersJR: If we are NOT using old archery it should be SpellDamage
            else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
            {
                m_objectTypeToSpec.Add(eObjectType.CompositeBow, Specs.Archery);
            }

            //hib
            m_objectTypeToSpec.Add(eObjectType.Blunt, Specs.Blunt);
            m_objectTypeToSpec.Add(eObjectType.Blades, Specs.Blades);
            m_objectTypeToSpec.Add(eObjectType.Piercing, Specs.Piercing);
            m_objectTypeToSpec.Add(eObjectType.LargeWeapons, Specs.Large_Weapons);
            m_objectTypeToSpec.Add(eObjectType.CelticSpear, Specs.Celtic_Spear);
            m_objectTypeToSpec.Add(eObjectType.Scythe, Specs.Scythe);
            m_objectTypeToSpec.Add(eObjectType.Shield, Specs.Shields);
            m_objectTypeToSpec.Add(eObjectType.Poison, Specs.Envenom);

            // RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
            if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
            {
                m_objectTypeToSpec.Add(eObjectType.RecurvedBow, Specs.RecurveBow);
            }
            // RDSandersJR: If we are NOT using old archery it should be SpellDamage
            else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
            {
                m_objectTypeToSpec.Add(eObjectType.RecurvedBow, Specs.Archery);
            }
        }

        /// <summary>
        /// Initialize the spec to skill table
        /// </summary>
        private static void InitializeSpecToSkill()
        {
            #region Weapon Specs

            //Weapon specs
            //Alb
            m_specToSkill.Add(Specs.Thrust, eProperty.Skill_Thrusting);
            m_specToSkill.Add(Specs.Slash, eProperty.Skill_Slashing);
            m_specToSkill.Add(Specs.Crush, eProperty.Skill_Crushing);
            m_specToSkill.Add(Specs.Polearms, eProperty.Skill_Polearms);
            m_specToSkill.Add(Specs.Two_Handed, eProperty.Skill_Two_Handed);
            m_specToSkill.Add(Specs.Staff, eProperty.Skill_Staff);
            m_specToSkill.Add(Specs.Dual_Wield, eProperty.Skill_Dual_Wield);
            m_specToSkill.Add(Specs.Flexible, eProperty.Skill_Flexible_Weapon);
            m_specToSkill.Add(Specs.Longbow, eProperty.Skill_Long_bows);
            m_specToSkill.Add(Specs.Crossbow, eProperty.Skill_Cross_Bows);
            //Mid
            m_specToSkill.Add(Specs.Sword, eProperty.Skill_Sword);
            m_specToSkill.Add(Specs.Axe, eProperty.Skill_Axe);
            m_specToSkill.Add(Specs.Hammer, eProperty.Skill_Hammer);
            m_specToSkill.Add(Specs.Left_Axe, eProperty.Skill_Left_Axe);
            m_specToSkill.Add(Specs.Spear, eProperty.Skill_Spear);
            m_specToSkill.Add(Specs.CompositeBow, eProperty.Skill_Composite);
            m_specToSkill.Add(Specs.Thrown_Weapons, eProperty.Skill_Thrown_Weapons);
            m_specToSkill.Add(Specs.HandToHand, eProperty.Skill_HandToHand);
            //Hib
            m_specToSkill.Add(Specs.Blades, eProperty.Skill_Blades);
            m_specToSkill.Add(Specs.Blunt, eProperty.Skill_Blunt);
            m_specToSkill.Add(Specs.Piercing, eProperty.Skill_Piercing);
            m_specToSkill.Add(Specs.Large_Weapons, eProperty.Skill_Large_Weapon);
            m_specToSkill.Add(Specs.Celtic_Dual, eProperty.Skill_Celtic_Dual);
            m_specToSkill.Add(Specs.Celtic_Spear, eProperty.Skill_Celtic_Spear);
            m_specToSkill.Add(Specs.RecurveBow, eProperty.Skill_RecurvedBow);
            m_specToSkill.Add(Specs.Scythe, eProperty.Skill_Scythe);

            #endregion

            #region Magic Specs

            //Magic specs
            //Alb
            m_specToSkill.Add(Specs.Matter_Magic, eProperty.Skill_Matter);
            m_specToSkill.Add(Specs.Body_Magic, eProperty.Skill_Body);
            m_specToSkill.Add(Specs.Spirit_Magic, eProperty.Skill_Spirit);
            m_specToSkill.Add(Specs.Rejuvenation, eProperty.Skill_Rejuvenation);
            m_specToSkill.Add(Specs.Enhancement, eProperty.Skill_Enhancement);
            m_specToSkill.Add(Specs.Smite, eProperty.Skill_Smiting);
            m_specToSkill.Add(Specs.Instruments, eProperty.Skill_Instruments);
            m_specToSkill.Add(Specs.Deathsight, eProperty.Skill_DeathSight);
            m_specToSkill.Add(Specs.Painworking, eProperty.Skill_Pain_working);
            m_specToSkill.Add(Specs.Death_Servant, eProperty.Skill_Death_Servant);
            m_specToSkill.Add(Specs.Chants, eProperty.Skill_Chants);
            m_specToSkill.Add(Specs.Mind_Magic, eProperty.Skill_Mind);
            m_specToSkill.Add(Specs.Earth_Magic, eProperty.Skill_Earth);
            m_specToSkill.Add(Specs.Cold_Magic, eProperty.Skill_Cold);
            m_specToSkill.Add(Specs.Fire_Magic, eProperty.Skill_Fire);
            m_specToSkill.Add(Specs.Wind_Magic, eProperty.Skill_Wind);
            m_specToSkill.Add(Specs.Soulrending, eProperty.Skill_SoulRending);
            //Mid
            m_specToSkill.Add(Specs.Darkness, eProperty.Skill_Darkness);
            m_specToSkill.Add(Specs.Suppression, eProperty.Skill_Suppression);
            m_specToSkill.Add(Specs.Runecarving, eProperty.Skill_Runecarving);
            m_specToSkill.Add(Specs.Summoning, eProperty.Skill_Summoning);
            m_specToSkill.Add(Specs.BoneArmy, eProperty.Skill_BoneArmy);
            m_specToSkill.Add(Specs.Mending, eProperty.Skill_Mending);
            m_specToSkill.Add(Specs.Augmentation, eProperty.Skill_Augmentation);
            m_specToSkill.Add(Specs.Pacification, eProperty.Skill_Pacification);
            m_specToSkill.Add(Specs.Subterranean, eProperty.Skill_Subterranean);
            m_specToSkill.Add(Specs.Beastcraft, eProperty.Skill_BeastCraft);
            m_specToSkill.Add(Specs.Stormcalling, eProperty.Skill_Stormcalling);
            m_specToSkill.Add(Specs.Battlesongs, eProperty.Skill_Battlesongs);
            m_specToSkill.Add(Specs.Savagery, eProperty.Skill_Savagery);
            m_specToSkill.Add(Specs.OdinsWill, eProperty.Skill_OdinsWill);
            m_specToSkill.Add(Specs.Cursing, eProperty.Skill_Cursing);
            m_specToSkill.Add(Specs.Hexing, eProperty.Skill_Hexing);
            m_specToSkill.Add(Specs.Witchcraft, eProperty.Skill_Witchcraft);

            //Hib
            m_specToSkill.Add(Specs.Arboreal_Path, eProperty.Skill_Arboreal);
            m_specToSkill.Add(Specs.Creeping_Path, eProperty.Skill_Creeping);
            m_specToSkill.Add(Specs.Verdant_Path, eProperty.Skill_Verdant);
            m_specToSkill.Add(Specs.Regrowth, eProperty.Skill_Regrowth);
            m_specToSkill.Add(Specs.Nurture, eProperty.Skill_Nurture);
            m_specToSkill.Add(Specs.Music, eProperty.Skill_Music);
            m_specToSkill.Add(Specs.Valor, eProperty.Skill_Valor);
            m_specToSkill.Add(Specs.Nature, eProperty.Skill_Nature);
            m_specToSkill.Add(Specs.Light, eProperty.Skill_Light);
            m_specToSkill.Add(Specs.Void, eProperty.Skill_Void);
            m_specToSkill.Add(Specs.Mana, eProperty.Skill_Mana);
            m_specToSkill.Add(Specs.Enchantments, eProperty.Skill_Enchantments);
            m_specToSkill.Add(Specs.Mentalism, eProperty.Skill_Mentalism);
            m_specToSkill.Add(Specs.Nightshade_Magic, eProperty.Skill_Nightshade);
            m_specToSkill.Add(Specs.Pathfinding, eProperty.Skill_Pathfinding);
            m_specToSkill.Add(Specs.Dementia, eProperty.Skill_Dementia);
            m_specToSkill.Add(Specs.ShadowMastery, eProperty.Skill_ShadowMastery);
            m_specToSkill.Add(Specs.VampiiricEmbrace, eProperty.Skill_VampiiricEmbrace);
            m_specToSkill.Add(Specs.EtherealShriek, eProperty.Skill_EtherealShriek);
            m_specToSkill.Add(Specs.PhantasmalWail, eProperty.Skill_PhantasmalWail);
            m_specToSkill.Add(Specs.SpectralForce, eProperty.Skill_SpectralForce);
            m_specToSkill.Add(Specs.SpectralGuard, eProperty.Skill_SpectralGuard);

            #endregion

            #region Other

            //Other
            m_specToSkill.Add(Specs.Critical_Strike, eProperty.Skill_Critical_Strike);
            m_specToSkill.Add(Specs.Stealth, eProperty.Skill_Stealth);
            m_specToSkill.Add(Specs.Shields, eProperty.Skill_Shields);
            m_specToSkill.Add(Specs.Envenom, eProperty.Skill_Envenom);
            m_specToSkill.Add(Specs.Parry, eProperty.Skill_Parry);
            m_specToSkill.Add(Specs.ShortBow, eProperty.Skill_ShortBow);
            m_specToSkill.Add(Specs.Mauler_Staff, eProperty.Skill_MaulerStaff);
            m_specToSkill.Add(Specs.Fist_Wraps, eProperty.Skill_FistWraps);
            m_specToSkill.Add(Specs.Aura_Manipulation, eProperty.Skill_Aura_Manipulation);
            m_specToSkill.Add(Specs.Magnetism, eProperty.Skill_Magnetism);
            m_specToSkill.Add(Specs.Power_Strikes, eProperty.Skill_Power_Strikes);

            m_specToSkill.Add(Specs.Archery, eProperty.Skill_Archery);

            #endregion
        }

        /// <summary>
        /// Initialize the spec to focus tables
        /// </summary>
        private static void InitializeSpecToFocus()
        {
            m_specToFocus.Add(Specs.Darkness, eProperty.Focus_Darkness);
            m_specToFocus.Add(Specs.Suppression, eProperty.Focus_Suppression);
            m_specToFocus.Add(Specs.Runecarving, eProperty.Focus_Runecarving);
            m_specToFocus.Add(Specs.Spirit_Magic, eProperty.Focus_Spirit);
            m_specToFocus.Add(Specs.Fire_Magic, eProperty.Focus_Fire);
            m_specToFocus.Add(Specs.Wind_Magic, eProperty.Focus_Air);
            m_specToFocus.Add(Specs.Cold_Magic, eProperty.Focus_Cold);
            m_specToFocus.Add(Specs.Earth_Magic, eProperty.Focus_Earth);
            m_specToFocus.Add(Specs.Light, eProperty.Focus_Light);
            m_specToFocus.Add(Specs.Body_Magic, eProperty.Focus_Body);
            m_specToFocus.Add(Specs.Mind_Magic, eProperty.Focus_Mind);
            m_specToFocus.Add(Specs.Matter_Magic, eProperty.Focus_Matter);
            m_specToFocus.Add(Specs.Void, eProperty.Focus_Void);
            m_specToFocus.Add(Specs.Mana, eProperty.Focus_Mana);
            m_specToFocus.Add(Specs.Enchantments, eProperty.Focus_Enchantments);
            m_specToFocus.Add(Specs.Mentalism, eProperty.Focus_Mentalism);
            m_specToFocus.Add(Specs.Summoning, eProperty.Focus_Summoning);
            // SI
            m_specToFocus.Add(Specs.BoneArmy, eProperty.Focus_BoneArmy);
            m_specToFocus.Add(Specs.Painworking, eProperty.Focus_PainWorking);
            m_specToFocus.Add(Specs.Deathsight, eProperty.Focus_DeathSight);
            m_specToFocus.Add(Specs.Death_Servant, eProperty.Focus_DeathServant);
            m_specToFocus.Add(Specs.Verdant_Path, eProperty.Focus_Verdant);
            m_specToFocus.Add(Specs.Creeping_Path, eProperty.Focus_CreepingPath);
            m_specToFocus.Add(Specs.Arboreal_Path, eProperty.Focus_Arboreal);
            // Catacombs
            m_specToFocus.Add(Specs.EtherealShriek, eProperty.Focus_EtherealShriek);
            m_specToFocus.Add(Specs.PhantasmalWail, eProperty.Focus_PhantasmalWail);
            m_specToFocus.Add(Specs.SpectralForce, eProperty.Focus_SpectralForce);
            m_specToFocus.Add(Specs.Cursing, eProperty.Focus_Cursing);
            m_specToFocus.Add(Specs.Hexing, eProperty.Focus_Hexing);
            m_specToFocus.Add(Specs.Witchcraft, eProperty.Focus_Witchcraft);
        }

        /// <summary>
        /// Init property types table
        /// </summary>
        private static void InitPropertyTypes()
        {
            #region Resist

            // resists
            m_propertyTypes[(int)eProperty.Resist_Natural] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Body] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Cold] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Crush] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Energy] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Heat] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Matter] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Slash] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Spirit] = ePropertyType.Resist;
            m_propertyTypes[(int)eProperty.Resist_Thrust] = ePropertyType.Resist;

            #endregion

            #region Focus

            // focuses
            m_propertyTypes[(int)eProperty.Focus_Darkness] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Suppression] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Runecarving] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Spirit] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Fire] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Air] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Cold] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Earth] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Light] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Body] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Matter] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Mind] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Void] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Mana] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Enchantments] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Mentalism] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Summoning] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_BoneArmy] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_PainWorking] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_DeathSight] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_DeathServant] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Verdant] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_CreepingPath] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Arboreal] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_EtherealShriek] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_PhantasmalWail] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_SpectralForce] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Cursing] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Hexing] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.Focus_Witchcraft] = ePropertyType.Focus;
            m_propertyTypes[(int)eProperty.AllFocusLevels] = ePropertyType.Focus;

            #endregion


            /*
			 * http://www.camelotherald.com/more/1036.shtml
			 * "- ALL melee weapon skills - This bonus will increase your
			 * skill in many weapon types. This bonus does not increase shield,
			 * parry, archery skills, or dual wield skills (hand to hand is the
			 * exception, as this skill is also the main weapon skill associated
			 * with hand to hand weapons, and not just the off-hand skill). If
			 * your item has "All melee weapon skills: +3" and your character
			 * can train in hammer, axe and sword, your item should give you
			 * a +3 increase to all three."
			 */

            #region Melee Skills

            // skills
            m_propertyTypes[(int)eProperty.Skill_Two_Handed] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Critical_Strike] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Crushing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Flexible_Weapon] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Polearms] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Slashing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Staff] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Thrusting] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Sword] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Hammer] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Axe] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Spear] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Blades] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Blunt] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Piercing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Large_Weapon] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Celtic_Spear] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Scythe] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_Thrown_Weapons] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_HandToHand] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_FistWraps] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
            m_propertyTypes[(int)eProperty.Skill_MaulerStaff] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;

            m_propertyTypes[(int)eProperty.Skill_Dual_Wield] = ePropertyType.Skill | ePropertyType.SkillDualWield;
            m_propertyTypes[(int)eProperty.Skill_Left_Axe] = ePropertyType.Skill | ePropertyType.SkillDualWield;
            m_propertyTypes[(int)eProperty.Skill_Celtic_Dual] = ePropertyType.Skill | ePropertyType.SkillDualWield;

            #endregion

            #region Magical Skills

            m_propertyTypes[(int)eProperty.Skill_Power_Strikes] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Magnetism] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Aura_Manipulation] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Body] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Chants] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Death_Servant] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_DeathSight] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Earth] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Enhancement] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Fire] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Cold] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Instruments] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Matter] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Mind] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Pain_working] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Rejuvenation] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Smiting] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_SoulRending] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Spirit] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Wind] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Mending] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Augmentation] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Darkness] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Suppression] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Runecarving] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Stormcalling] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_BeastCraft] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Light] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Void] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Mana] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Battlesongs] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Enchantments] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Mentalism] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Regrowth] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Nurture] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Nature] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Music] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Valor] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Subterranean] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_BoneArmy] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Verdant] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Creeping] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Arboreal] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Pacification] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Savagery] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Nightshade] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Pathfinding] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Summoning] = ePropertyType.Skill | ePropertyType.SkillMagical;

            // no idea about these
            m_propertyTypes[(int)eProperty.Skill_Dementia] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_ShadowMastery] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_VampiiricEmbrace] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_EtherealShriek] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_PhantasmalWail] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_SpectralForce] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_SpectralGuard] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_OdinsWill] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Cursing] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Hexing] = ePropertyType.Skill | ePropertyType.SkillMagical;
            m_propertyTypes[(int)eProperty.Skill_Witchcraft] = ePropertyType.Skill | ePropertyType.SkillMagical;

            #endregion

            #region Other

            m_propertyTypes[(int)eProperty.Skill_Long_bows] = ePropertyType.Skill | ePropertyType.SkillArchery;
            m_propertyTypes[(int)eProperty.Skill_Composite] = ePropertyType.Skill | ePropertyType.SkillArchery;
            m_propertyTypes[(int)eProperty.Skill_RecurvedBow] = ePropertyType.Skill | ePropertyType.SkillArchery;

            m_propertyTypes[(int)eProperty.Skill_Parry] = ePropertyType.Skill;
            m_propertyTypes[(int)eProperty.Skill_Shields] = ePropertyType.Skill;

            m_propertyTypes[(int)eProperty.Skill_Stealth] = ePropertyType.Skill;
            m_propertyTypes[(int)eProperty.Skill_Cross_Bows] = ePropertyType.Skill;
            m_propertyTypes[(int)eProperty.Skill_ShortBow] = ePropertyType.Skill;
            m_propertyTypes[(int)eProperty.Skill_Envenom] = ePropertyType.Skill;
            m_propertyTypes[(int)eProperty.Skill_Archery] = ePropertyType.Skill | ePropertyType.SkillArchery;

            #endregion
        }

        /// <summary>
        /// Initializes the race resist table
        /// </summary>
        public static void InitializeRaceResists()
        {
            m_syncLockUpdates.EnterWriteLock();
            try
            {
                // http://camelot.allakhazam.com/Start_Stats.html
                IList<Race> races;

                try
                {
                    races = GameServer.Database.SelectAllObjects<Race>();
                }
                catch
                {
                    m_raceResists.Clear();
                    return;
                }

                if (races != null)
                {

                    m_raceResists.Clear();

                    for (int i = 0; i < races.Count; i++)
                    {
                        Race race = races[i];
                        m_raceResists.Add(race.ID, new int[10]);
                        m_raceResists[race.ID][0] = race.ResistBody;
                        m_raceResists[race.ID][1] = race.ResistCold;
                        m_raceResists[race.ID][2] = race.ResistCrush;
                        m_raceResists[race.ID][3] = race.ResistEnergy;
                        m_raceResists[race.ID][4] = race.ResistHeat;
                        m_raceResists[race.ID][5] = race.ResistMatter;
                        m_raceResists[race.ID][6] = race.ResistSlash;
                        m_raceResists[race.ID][7] = race.ResistSpirit;
                        m_raceResists[race.ID][8] = race.ResistThrust;
                        m_raceResists[race.ID][9] = race.ResistNatural;
                    }

                    races = null;
                }
            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }

        private static void RegisterPropertyNamesDE()
        {


            m_propertyNamesDE.Add(eProperty.Strength, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Strength"));
            m_propertyNamesDE.Add(eProperty.Dexterity, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Dexterity"));
            m_propertyNamesDE.Add(eProperty.Constitution, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Constitution"));
            m_propertyNamesDE.Add(eProperty.Quickness, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Quickness"));
            m_propertyNamesDE.Add(eProperty.Intelligence, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Intelligence"));
            m_propertyNamesDE.Add(eProperty.Piety, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Piety"));
            m_propertyNamesDE.Add(eProperty.Empathy, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Empathy"));
            m_propertyNamesDE.Add(eProperty.Charisma, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Charisma"));

            m_propertyNamesDE.Add(eProperty.MaxMana, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Power"));
            m_propertyNamesDE.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Hits"));

            // resists (does not say "resist" on live server)
            m_propertyNamesDE.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Body"));
            m_propertyNamesDE.Add(eProperty.Resist_Natural, LanguageMgr.GetTranslation("DE", "Essenz"));
            m_propertyNamesDE.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Cold"));
            m_propertyNamesDE.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Crush"));
            m_propertyNamesDE.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Energy"));
            m_propertyNamesDE.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Heat"));
            m_propertyNamesDE.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Matter"));
            m_propertyNamesDE.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Slash"));
            m_propertyNamesDE.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Spirit"));
            m_propertyNamesDE.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Thrust"));

            // Eden - Mythirian bonus
            m_propertyNamesDE.Add(eProperty.BodyResCapBonus, LanguageMgr.GetTranslation("DE", "Kärper Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.ColdResCapBonus, LanguageMgr.GetTranslation("DE", "Kälte Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.CrushResCapBonus, LanguageMgr.GetTranslation("DE", "Schlag Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.EnergyResCapBonus, LanguageMgr.GetTranslation("DE", "Energie Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.HeatResCapBonus, LanguageMgr.GetTranslation("DE", "Hitze Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.MatterResCapBonus, LanguageMgr.GetTranslation("DE", "Materie Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.SlashResCapBonus, LanguageMgr.GetTranslation("DE", "Schnitt Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.SpiritResCapBonus, LanguageMgr.GetTranslation("DE", "Geist Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.ThrustResCapBonus, LanguageMgr.GetTranslation("DE", "Stich Cap Erhöhung"));
            m_propertyNamesDE.Add(eProperty.MythicalSafeFall, LanguageMgr.GetTranslation("DE", "Mythischer sicherer Fall"));
            m_propertyNamesDE.Add(eProperty.MythicalEvade, LanguageMgr.GetTranslation("DE", "Mythisches Ausweichen"));
            m_propertyNamesDE.Add(eProperty.MythicalDiscumbering, LanguageMgr.GetTranslation("DE", "Mythische Discumbering Erhöhung"));
            m_propertyNamesDE.Add(eProperty.MythicalCoin, LanguageMgr.GetTranslation("DE", "Mythische Münzen Erhöhung"));
            m_propertyNamesDE.Add(eProperty.MythicalDefense, LanguageMgr.GetTranslation("DE", "Mythischer Verteidigungs Erhöhung"));
            m_propertyNamesDE.Add(eProperty.MythicalDPS, LanguageMgr.GetTranslation("DE", "Mythische DPS Erhöhung"));
            m_propertyNamesDE.Add(eProperty.MythicalSiegeDamageReduction, LanguageMgr.GetTranslation("DE", "Mythischer Belagerugs Schaden Ablive"));
            m_propertyNamesDE.Add(eProperty.MythicalSiegeSpeed, LanguageMgr.GetTranslation("DE", "Mythische Belagerungsgeschwindigkeit"));
            m_propertyNamesDE.Add(eProperty.SpellLevel, LanguageMgr.GetTranslation("DE", "Kopfgeld"));
            //Eden - special actifacts bonus
            m_propertyNamesDE.Add(eProperty.Conversion, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Conversion"));
            m_propertyNamesDE.Add(eProperty.ExtraHP, LanguageMgr.GetTranslation("DE", "Extra Lebenspunkte"));
            m_propertyNamesDE.Add(eProperty.StyleAbsorb, LanguageMgr.GetTranslation("DE", "Style Absorb"));
            m_propertyNamesDE.Add(eProperty.ArcaneSyphon, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ArcaneSyphon"));
            m_propertyNamesDE.Add(eProperty.RealmPoints, LanguageMgr.GetTranslation("DE", "Reichspunkte"));
            //[Freya] Nidel
            m_propertyNamesDE.Add(eProperty.BountyPoints, LanguageMgr.GetTranslation("DE", "Kopfgeldpunkte"));
            m_propertyNamesDE.Add(eProperty.XpPoints, LanguageMgr.GetTranslation("DE", "Erfahrungspunkte"));

            // skills
            m_propertyNamesDE.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.TwoHanded"));
            m_propertyNamesDE.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BodyMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Chants"));
            m_propertyNamesDE.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CriticalStrike"));
            m_propertyNamesDE.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Crossbows"));
            m_propertyNamesDE.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Crushing"));
            m_propertyNamesDE.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DeathServant"));
            m_propertyNamesDE.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Deathsight"));
            m_propertyNamesDE.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DualWield"));
            m_propertyNamesDE.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EarthMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Enhancement"));
            m_propertyNamesDE.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Envenom"));
            m_propertyNamesDE.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.FireMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
            m_propertyNamesDE.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ColdMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Instruments"));
            m_propertyNamesDE.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Longbows"));
            m_propertyNamesDE.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MatterMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MindMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Painworking"));
            m_propertyNamesDE.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Parry"));
            m_propertyNamesDE.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Polearms"));
            m_propertyNamesDE.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Rejuvenation"));
            m_propertyNamesDE.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Shields"));
            m_propertyNamesDE.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Slashing"));
            m_propertyNamesDE.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Smiting"));
            m_propertyNamesDE.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Soulrending"));
            m_propertyNamesDE.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpiritMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Staff"));
            m_propertyNamesDE.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Stealth"));
            m_propertyNamesDE.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Thrusting"));
            m_propertyNamesDE.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.WindMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Sword"));
            m_propertyNamesDE.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Hammer"));
            m_propertyNamesDE.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Axe"));
            m_propertyNamesDE.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.LeftAxe"));
            m_propertyNamesDE.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Spear"));
            m_propertyNamesDE.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Mending"));
            m_propertyNamesDE.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Augmentation"));
            m_propertyNamesDE.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Darkness"));
            m_propertyNamesDE.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Suppression"));
            m_propertyNamesDE.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Runecarving"));
            m_propertyNamesDE.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Stormcalling"));
            m_propertyNamesDE.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BeastCraft"));
            m_propertyNamesDE.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.LightMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.VoidMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ManaMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Composite"));
            m_propertyNamesDE.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Battlesongs"));
            m_propertyNamesDE.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Enchantment"));

            m_propertyNamesDE.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Blades"));
            m_propertyNamesDE.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Blunt"));
            m_propertyNamesDE.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Piercing"));
            m_propertyNamesDE.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.LargeWeapon"));
            m_propertyNamesDE.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Mentalism"));
            m_propertyNamesDE.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Regrowth"));
            m_propertyNamesDE.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Nurture"));
            m_propertyNamesDE.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Nature"));
            m_propertyNamesDE.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Music"));
            m_propertyNamesDE.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CelticDual"));
            m_propertyNamesDE.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CelticSpear"));
            m_propertyNamesDE.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.RecurvedBow"));
            m_propertyNamesDE.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Valor"));
            m_propertyNamesDE.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CaveMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BoneArmy"));
            m_propertyNamesDE.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Verdant"));
            m_propertyNamesDE.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Creeping"));
            m_propertyNamesDE.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Arboreal"));
            m_propertyNamesDE.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Scythe"));
            m_propertyNamesDE.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ThrownWeapons"));
            m_propertyNamesDE.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.HandToHand"));
            m_propertyNamesDE.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ShortBow"));
            m_propertyNamesDE.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Pacification"));
            m_propertyNamesDE.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Savagery"));
            m_propertyNamesDE.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.NightshadeMagic"));
            m_propertyNamesDE.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Pathfinding"));
            m_propertyNamesDE.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Summoning"));
            m_propertyNamesDE.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Archery"));

            // Mauler
            m_propertyNamesDE.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.FistWraps"));
            m_propertyNamesDE.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MaulerStaff"));
            m_propertyNamesDE.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PowerStrikes"));
            m_propertyNamesDE.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Magnetism"));
            m_propertyNamesDE.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AuraManipulation"));


            //Catacombs skills
            m_propertyNamesDE.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Dementia"));
            m_propertyNamesDE.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ShadowMastery"));
            m_propertyNamesDE.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
            m_propertyNamesDE.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EtherealShriek"));
            m_propertyNamesDE.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PhantasmalWail"));
            m_propertyNamesDE.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpectralForce"));
            m_propertyNamesDE.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpectralGuard"));
            m_propertyNamesDE.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.OdinsWill"));
            m_propertyNamesDE.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Cursing"));
            m_propertyNamesDE.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Hexing"));
            m_propertyNamesDE.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Witchcraft"));


            // Classic Focii
            m_propertyNamesDE.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DarknessFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SuppressionFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.FireMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.WindMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.LightMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MindMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MentalismFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SummoningFocus"));
            // SI Focii
            // Mid
            m_propertyNamesDE.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
            // Alb
            m_propertyNamesDE.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PainworkingFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DeathsightFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DeathservantFocus"));
            // Hib
            m_propertyNamesDE.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.VerdantFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ArborealFocus"));
            // Catacombs Focii
            m_propertyNamesDE.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CursingFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.HexingFocus"));
            m_propertyNamesDE.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

            m_propertyNamesDE.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MaximumSpeed"));
            m_propertyNamesDE.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Concentration"));

            m_propertyNamesDE.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
            m_propertyNamesDE.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

            m_propertyNamesDE.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.HealthRegeneration"));
            m_propertyNamesDE.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PowerRegeneration"));
            m_propertyNamesDE.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
            m_propertyNamesDE.Add(eProperty.SpellRange, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpellRange"));
            m_propertyNamesDE.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ArcheryRange"));
            m_propertyNamesDE.Add(eProperty.Acuity, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Acuity"));

            m_propertyNamesDE.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AllMagicSkills"));
            m_propertyNamesDE.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
            m_propertyNamesDE.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ALLSpellLines"));
            m_propertyNamesDE.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
            m_propertyNamesDE.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AllArcherySkills"));

            m_propertyNamesDE.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EffectiveLevel"));


            //Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
            //Date : 20-Jan-2005
            //Missing bonusses begin
            m_propertyNamesDE.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EvadeChance"));
            m_propertyNamesDE.Add(eProperty.BlockChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BlockChance"));
            m_propertyNamesDE.Add(eProperty.ParryChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ParryChance"));
            m_propertyNamesDE.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.FumbleChance"));
            m_propertyNamesDE.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MeleeDamage"));
            m_propertyNamesDE.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.RangedDamage"));
            m_propertyNamesDE.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
            m_propertyNamesDE.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.StunDuration"));
            m_propertyNamesDE.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
            m_propertyNamesDE.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
            m_propertyNamesDE.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DefensiveBonus"));
            m_propertyNamesDE.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PieceAblative"));
            m_propertyNamesDE.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.NegativeReduction"));
            m_propertyNamesDE.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
            m_propertyNamesDE.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpellPowerCost"));
            m_propertyNamesDE.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.StyleCostReduction"));
            m_propertyNamesDE.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ToHitBonus"));
            m_propertyNamesDE.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ArcherySpeed"));
            m_propertyNamesDE.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ArrowRecovery"));
            m_propertyNamesDE.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.StatBuffSpells"));
            m_propertyNamesDE.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CastingSpeed"));
            m_propertyNamesDE.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ExperienceLoss"));
            m_propertyNamesDE.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
            m_propertyNamesDE.Add(eProperty.Fatigue, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.Fatigue"));
            m_propertyNamesDE.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
            m_propertyNamesDE.Add(eProperty.PowerPool, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PowerPool"));
            //Magiekraftvorrat
            m_propertyNamesDE.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ResistPierce"));
            m_propertyNamesDE.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
            m_propertyNamesDE.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.SpellDuration"));
            m_propertyNamesDE.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.StyleDamage"));
            m_propertyNamesDE.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.MeleeSpeed"));
            //Missing bonusses end

            m_propertyNamesDE.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
            m_propertyNamesDE.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
            m_propertyNamesDE.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
            m_propertyNamesDE.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
            m_propertyNamesDE.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
            m_propertyNamesDE.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PietyBonusCap"));
            m_propertyNamesDE.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
            m_propertyNamesDE.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
            m_propertyNamesDE.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
            m_propertyNamesDE.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
            m_propertyNamesDE.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.PowerBonusCap"));
            m_propertyNamesDE.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.WeaponSkill"));
            m_propertyNamesDE.Add(eProperty.AllSkills, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.AllSkills"));
            m_propertyNamesDE.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
            m_propertyNamesDE.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
            m_propertyNamesDE.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
            m_propertyNamesDE.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation("DE", "SkillBase.RegisterPropertyNames.CriticalHealHit"));
            //Forsaken Worlds: Mythical Stat Cap
            m_propertyNamesDE.Add(eProperty.MythicalStrCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Strength)"));
            m_propertyNamesDE.Add(eProperty.MythicalDexCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Dexterity)"));
            m_propertyNamesDE.Add(eProperty.MythicalConCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Constitution)"));
            m_propertyNamesDE.Add(eProperty.MythicalQuiCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Quickness)"));
            m_propertyNamesDE.Add(eProperty.MythicalIntCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Intelligence)"));
            m_propertyNamesDE.Add(eProperty.MythicalPieCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Piety)"));
            m_propertyNamesDE.Add(eProperty.MythicalChaCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Charisma)"));
            m_propertyNamesDE.Add(eProperty.MythicalEmpCapBonus, LanguageMgr.GetTranslation("DE", "Mythical Stat Cap (Empathy)"));

            log.Info("DE language: stat names loaded");
        }
        private static void RegisterPropertyNamesFR()
        {


            m_propertyNamesFR.Add(eProperty.Strength, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Strength"));
            m_propertyNamesFR.Add(eProperty.Dexterity, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Dexterity"));
            m_propertyNamesFR.Add(eProperty.Constitution, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Constitution"));
            m_propertyNamesFR.Add(eProperty.Quickness, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Quickness"));
            m_propertyNamesFR.Add(eProperty.Intelligence, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Intelligence"));
            m_propertyNamesFR.Add(eProperty.Piety, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Piety"));
            m_propertyNamesFR.Add(eProperty.Empathy, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Empathy"));
            m_propertyNamesFR.Add(eProperty.Charisma, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Charisma"));

            m_propertyNamesFR.Add(eProperty.MaxMana, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Power"));
            m_propertyNamesFR.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Hits"));

            // resists (does not say "resist" on live server)
            m_propertyNamesFR.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Body"));
            m_propertyNamesFR.Add(eProperty.Resist_Natural, LanguageMgr.GetTranslation("FR", "Essence"));
            m_propertyNamesFR.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Cold"));
            m_propertyNamesFR.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Crush"));
            m_propertyNamesFR.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Energy"));
            m_propertyNamesFR.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Heat"));
            m_propertyNamesFR.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Matter"));
            m_propertyNamesFR.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Slash"));
            m_propertyNamesFR.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Spirit"));
            m_propertyNamesFR.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Thrust"));

            // Eden - Mythirian bonus
            m_propertyNamesFR.Add(eProperty.BodyResCapBonus, LanguageMgr.GetTranslation("FR", "Body cap"));
            m_propertyNamesFR.Add(eProperty.ColdResCapBonus, LanguageMgr.GetTranslation("FR", "Cold cap"));
            m_propertyNamesFR.Add(eProperty.CrushResCapBonus, LanguageMgr.GetTranslation("FR", "Crush cap"));
            m_propertyNamesFR.Add(eProperty.EnergyResCapBonus, LanguageMgr.GetTranslation("FR", "Energy cap"));
            m_propertyNamesFR.Add(eProperty.HeatResCapBonus, LanguageMgr.GetTranslation("FR", "Heat cap"));
            m_propertyNamesFR.Add(eProperty.MatterResCapBonus, LanguageMgr.GetTranslation("FR", "Matter cap"));
            m_propertyNamesFR.Add(eProperty.SlashResCapBonus, LanguageMgr.GetTranslation("FR", "Slash cap"));
            m_propertyNamesFR.Add(eProperty.SpiritResCapBonus, LanguageMgr.GetTranslation("FR", "Spirit cap"));
            m_propertyNamesFR.Add(eProperty.ThrustResCapBonus, LanguageMgr.GetTranslation("FR", "Thrust cap"));
            m_propertyNamesFR.Add(eProperty.MythicalSafeFall, LanguageMgr.GetTranslation("FR", "Mythical Safe Fall"));
            m_propertyNamesFR.Add(eProperty.MythicalEvade, LanguageMgr.GetTranslation("FR", "Mythical Evade"));
            m_propertyNamesFR.Add(eProperty.MythicalDiscumbering, LanguageMgr.GetTranslation("FR", "Mythical Discumbering"));
            m_propertyNamesFR.Add(eProperty.MythicalCoin, LanguageMgr.GetTranslation("FR", "Mythical Coin"));
            m_propertyNamesFR.Add(eProperty.MythicalDefense, LanguageMgr.GetTranslation("FR", "Mythical Defense"));
            m_propertyNamesFR.Add(eProperty.MythicalDPS, LanguageMgr.GetTranslation("FR", "Mythical DPS"));
            m_propertyNamesFR.Add(eProperty.MythicalSiegeDamageReduction, LanguageMgr.GetTranslation("FR", "Mythical Siege Damage Ablive"));
            m_propertyNamesFR.Add(eProperty.MythicalSiegeSpeed, LanguageMgr.GetTranslation("FR", "Mythical Siege Speed"));
            m_propertyNamesFR.Add(eProperty.SpellLevel, LanguageMgr.GetTranslation("FR", "Spell Focus"));
            //Eden - special actifacts bonus
            m_propertyNamesFR.Add(eProperty.Conversion, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Conversion"));
            m_propertyNamesFR.Add(eProperty.ExtraHP, LanguageMgr.GetTranslation("FR", "Extra Health Points"));
            m_propertyNamesFR.Add(eProperty.StyleAbsorb, LanguageMgr.GetTranslation("FR", "Style Absorb"));
            m_propertyNamesFR.Add(eProperty.ArcaneSyphon, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ArcaneSyphon"));
            m_propertyNamesFR.Add(eProperty.RealmPoints, LanguageMgr.GetTranslation("FR", "Realm Points"));
            //[Freya] Nidel
            m_propertyNamesFR.Add(eProperty.BountyPoints, LanguageMgr.GetTranslation("FR", "Bounty Points"));
            m_propertyNamesFR.Add(eProperty.XpPoints, LanguageMgr.GetTranslation("FR", "Experience Points"));

            // skills
            m_propertyNamesFR.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.TwoHanded"));
            m_propertyNamesFR.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BodyMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Chants"));
            m_propertyNamesFR.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CriticalStrike"));
            m_propertyNamesFR.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Crossbows"));
            m_propertyNamesFR.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Crushing"));
            m_propertyNamesFR.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DeathServant"));
            m_propertyNamesFR.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Deathsight"));
            m_propertyNamesFR.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DualWield"));
            m_propertyNamesFR.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EarthMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Enhancement"));
            m_propertyNamesFR.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Envenom"));
            m_propertyNamesFR.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.FireMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
            m_propertyNamesFR.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ColdMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Instruments"));
            m_propertyNamesFR.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Longbows"));
            m_propertyNamesFR.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MatterMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MindMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Painworking"));
            m_propertyNamesFR.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Parry"));
            m_propertyNamesFR.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Polearms"));
            m_propertyNamesFR.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Rejuvenation"));
            m_propertyNamesFR.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Shields"));
            m_propertyNamesFR.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Slashing"));
            m_propertyNamesFR.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Smiting"));
            m_propertyNamesFR.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Soulrending"));
            m_propertyNamesFR.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpiritMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Staff"));
            m_propertyNamesFR.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Stealth"));
            m_propertyNamesFR.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Thrusting"));
            m_propertyNamesFR.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.WindMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Sword"));
            m_propertyNamesFR.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Hammer"));
            m_propertyNamesFR.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Axe"));
            m_propertyNamesFR.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.LeftAxe"));
            m_propertyNamesFR.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Spear"));
            m_propertyNamesFR.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Mending"));
            m_propertyNamesFR.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Augmentation"));
            m_propertyNamesFR.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Darkness"));
            m_propertyNamesFR.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Suppression"));
            m_propertyNamesFR.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Runecarving"));
            m_propertyNamesFR.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Stormcalling"));
            m_propertyNamesFR.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BeastCraft"));
            m_propertyNamesFR.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.LightMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.VoidMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ManaMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Composite"));
            m_propertyNamesFR.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Battlesongs"));
            m_propertyNamesFR.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Enchantment"));

            m_propertyNamesFR.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Blades"));
            m_propertyNamesFR.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Blunt"));
            m_propertyNamesFR.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Piercing"));
            m_propertyNamesFR.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.LargeWeapon"));
            m_propertyNamesFR.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Mentalism"));
            m_propertyNamesFR.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Regrowth"));
            m_propertyNamesFR.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Nurture"));
            m_propertyNamesFR.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Nature"));
            m_propertyNamesFR.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Music"));
            m_propertyNamesFR.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CelticDual"));
            m_propertyNamesFR.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CelticSpear"));
            m_propertyNamesFR.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.RecurvedBow"));
            m_propertyNamesFR.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Valor"));
            m_propertyNamesFR.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CaveMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BoneArmy"));
            m_propertyNamesFR.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Verdant"));
            m_propertyNamesFR.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Creeping"));
            m_propertyNamesFR.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Arboreal"));
            m_propertyNamesFR.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Scythe"));
            m_propertyNamesFR.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ThrownWeapons"));
            m_propertyNamesFR.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.HandToHand"));
            m_propertyNamesFR.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ShortBow"));
            m_propertyNamesFR.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Pacification"));
            m_propertyNamesFR.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Savagery"));
            m_propertyNamesFR.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.NightshadeMagic"));
            m_propertyNamesFR.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Pathfinding"));
            m_propertyNamesFR.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Summoning"));
            m_propertyNamesFR.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Archery"));

            // Mauler
            m_propertyNamesFR.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.FistWraps"));
            m_propertyNamesFR.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MaulerStaff"));
            m_propertyNamesFR.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PowerStrikes"));
            m_propertyNamesFR.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Magnetism"));
            m_propertyNamesFR.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AuraManipulation"));


            //Catacombs skills
            m_propertyNamesFR.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Dementia"));
            m_propertyNamesFR.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ShadowMastery"));
            m_propertyNamesFR.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
            m_propertyNamesFR.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EtherealShriek"));
            m_propertyNamesFR.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PhantasmalWail"));
            m_propertyNamesFR.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpectralForce"));
            m_propertyNamesFR.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpectralGuard"));
            m_propertyNamesFR.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.OdinsWill"));
            m_propertyNamesFR.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Cursing"));
            m_propertyNamesFR.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Hexing"));
            m_propertyNamesFR.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Witchcraft"));


            // Classic Focii
            m_propertyNamesFR.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DarknessFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SuppressionFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.FireMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.WindMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.LightMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MindMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MentalismFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SummoningFocus"));
            // SI Focii
            // Mid
            m_propertyNamesFR.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
            // Alb
            m_propertyNamesFR.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PainworkingFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DeathsightFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DeathservantFocus"));
            // Hib
            m_propertyNamesFR.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.VerdantFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ArborealFocus"));
            // Catacombs Focii
            m_propertyNamesFR.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CursingFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.HexingFocus"));
            m_propertyNamesFR.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

            m_propertyNamesFR.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MaximumSpeed"));
            m_propertyNamesFR.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Concentration"));

            m_propertyNamesFR.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
            m_propertyNamesFR.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

            m_propertyNamesFR.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.HealthRegeneration"));
            m_propertyNamesFR.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PowerRegeneration"));
            m_propertyNamesFR.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
            m_propertyNamesFR.Add(eProperty.SpellRange, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpellRange"));
            m_propertyNamesFR.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ArcheryRange"));
            m_propertyNamesFR.Add(eProperty.Acuity, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Acuity"));

            m_propertyNamesFR.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AllMagicSkills"));
            m_propertyNamesFR.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
            m_propertyNamesFR.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ALLSpellLines"));
            m_propertyNamesFR.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
            m_propertyNamesFR.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AllArcherySkills"));

            m_propertyNamesFR.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EffectiveLevel"));


            //Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
            //Date : 20-Jan-2005
            //Missing bonusses begin
            m_propertyNamesFR.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EvadeChance"));
            m_propertyNamesFR.Add(eProperty.BlockChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BlockChance"));
            m_propertyNamesFR.Add(eProperty.ParryChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ParryChance"));
            m_propertyNamesFR.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.FumbleChance"));
            m_propertyNamesFR.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MeleeDamage"));
            m_propertyNamesFR.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.RangedDamage"));
            m_propertyNamesFR.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
            m_propertyNamesFR.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.StunDuration"));
            m_propertyNamesFR.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
            m_propertyNamesFR.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
            m_propertyNamesFR.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DefensiveBonus"));
            m_propertyNamesFR.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PieceAblative"));
            m_propertyNamesFR.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.NegativeReduction"));
            m_propertyNamesFR.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
            m_propertyNamesFR.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpellPowerCost"));
            m_propertyNamesFR.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.StyleCostReduction"));
            m_propertyNamesFR.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ToHitBonus"));
            m_propertyNamesFR.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ArcherySpeed"));
            m_propertyNamesFR.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ArrowRecovery"));
            m_propertyNamesFR.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.StatBuffSpells"));
            m_propertyNamesFR.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CastingSpeed"));
            m_propertyNamesFR.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ExperienceLoss"));
            m_propertyNamesFR.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
            m_propertyNamesFR.Add(eProperty.Fatigue, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.Fatigue"));
            m_propertyNamesFR.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
            m_propertyNamesFR.Add(eProperty.PowerPool, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PowerPool"));
            //Magiekraftvorrat
            m_propertyNamesFR.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ResistPierce"));
            m_propertyNamesFR.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
            m_propertyNamesFR.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.SpellDuration"));
            m_propertyNamesFR.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.StyleDamage"));
            m_propertyNamesFR.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.MeleeSpeed"));
            //Missing bonusses end

            m_propertyNamesFR.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
            m_propertyNamesFR.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
            m_propertyNamesFR.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
            m_propertyNamesFR.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
            m_propertyNamesFR.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
            m_propertyNamesFR.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PietyBonusCap"));
            m_propertyNamesFR.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
            m_propertyNamesFR.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
            m_propertyNamesFR.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
            m_propertyNamesFR.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
            m_propertyNamesFR.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.PowerBonusCap"));
            m_propertyNamesFR.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.WeaponSkill"));
            m_propertyNamesFR.Add(eProperty.AllSkills, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.AllSkills"));
            m_propertyNamesFR.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
            m_propertyNamesFR.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
            m_propertyNamesFR.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
            m_propertyNamesFR.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation("FR", "SkillBase.RegisterPropertyNames.CriticalHealHit"));
            //Forsaken Worlds: Mythical Stat Cap
            m_propertyNamesFR.Add(eProperty.MythicalStrCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Strength)"));
            m_propertyNamesFR.Add(eProperty.MythicalDexCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Dexterity)"));
            m_propertyNamesFR.Add(eProperty.MythicalConCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Constitution)"));
            m_propertyNamesFR.Add(eProperty.MythicalQuiCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Quickness)"));
            m_propertyNamesFR.Add(eProperty.MythicalIntCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Intelligence)"));
            m_propertyNamesFR.Add(eProperty.MythicalPieCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Piety)"));
            m_propertyNamesFR.Add(eProperty.MythicalChaCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Charisma)"));
            m_propertyNamesFR.Add(eProperty.MythicalEmpCapBonus, LanguageMgr.GetTranslation("FR", "Mythical Stat Cap (Empathy)"));

            log.Info("FR language: stat names loaded");
        }
        private static void RegisterPropertyNamesEN()
        {


            m_propertyNamesEN.Add(eProperty.Strength, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Strength"));
            m_propertyNamesEN.Add(eProperty.Dexterity, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Dexterity"));
            m_propertyNamesEN.Add(eProperty.Constitution, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Constitution"));
            m_propertyNamesEN.Add(eProperty.Quickness, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Quickness"));
            m_propertyNamesEN.Add(eProperty.Intelligence, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Intelligence"));
            m_propertyNamesEN.Add(eProperty.Piety, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Piety"));
            m_propertyNamesEN.Add(eProperty.Empathy, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Empathy"));
            m_propertyNamesEN.Add(eProperty.Charisma, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Charisma"));

            m_propertyNamesEN.Add(eProperty.MaxMana, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Power"));
            m_propertyNamesEN.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Hits"));

            // resists (does not say "resist" on live server)
            m_propertyNamesEN.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Body"));
            m_propertyNamesEN.Add(eProperty.Resist_Natural, LanguageMgr.GetTranslation("EN", "Essence"));
            m_propertyNamesEN.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Cold"));
            m_propertyNamesEN.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Crush"));
            m_propertyNamesEN.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Energy"));
            m_propertyNamesEN.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Heat"));
            m_propertyNamesEN.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Matter"));
            m_propertyNamesEN.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Slash"));
            m_propertyNamesEN.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Spirit"));
            m_propertyNamesEN.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Thrust"));

            // Eden - Mythirian bonus
            m_propertyNamesEN.Add(eProperty.BodyResCapBonus, LanguageMgr.GetTranslation("EN", "Body cap"));
            m_propertyNamesEN.Add(eProperty.ColdResCapBonus, LanguageMgr.GetTranslation("EN", "Cold cap"));
            m_propertyNamesEN.Add(eProperty.CrushResCapBonus, LanguageMgr.GetTranslation("EN", "Crush cap"));
            m_propertyNamesEN.Add(eProperty.EnergyResCapBonus, LanguageMgr.GetTranslation("EN", "Energy cap"));
            m_propertyNamesEN.Add(eProperty.HeatResCapBonus, LanguageMgr.GetTranslation("EN", "Heat cap"));
            m_propertyNamesEN.Add(eProperty.MatterResCapBonus, LanguageMgr.GetTranslation("EN", "Matter cap"));
            m_propertyNamesEN.Add(eProperty.SlashResCapBonus, LanguageMgr.GetTranslation("EN", "Slash cap"));
            m_propertyNamesEN.Add(eProperty.SpiritResCapBonus, LanguageMgr.GetTranslation("EN", "Spirit cap"));
            m_propertyNamesEN.Add(eProperty.ThrustResCapBonus, LanguageMgr.GetTranslation("EN", "Thrust cap"));
            m_propertyNamesEN.Add(eProperty.MythicalSafeFall, LanguageMgr.GetTranslation("EN", "Mythical Safe Fall"));
            m_propertyNamesEN.Add(eProperty.MythicalEvade, LanguageMgr.GetTranslation("EN", "Mythical Evade"));
            m_propertyNamesEN.Add(eProperty.MythicalDiscumbering, LanguageMgr.GetTranslation("EN", "Mythical Discumbering"));
            m_propertyNamesEN.Add(eProperty.MythicalCoin, LanguageMgr.GetTranslation("EN", "Mythical Coin"));
            m_propertyNamesEN.Add(eProperty.MythicalDefense, LanguageMgr.GetTranslation("EN", "Mythical Defense"));
            m_propertyNamesEN.Add(eProperty.MythicalDPS, LanguageMgr.GetTranslation("EN", "Mythical DPS"));
            m_propertyNamesEN.Add(eProperty.MythicalSiegeDamageReduction, LanguageMgr.GetTranslation("EN", "Mythical Siege Damage Ablive"));
            m_propertyNamesEN.Add(eProperty.MythicalSiegeSpeed, LanguageMgr.GetTranslation("EN", "Mythical Siege Speed"));
            m_propertyNamesEN.Add(eProperty.SpellLevel, LanguageMgr.GetTranslation("EN", "Spell Focus"));
            //Eden - special actifacts bonus
            m_propertyNamesEN.Add(eProperty.Conversion, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Conversion"));
            m_propertyNamesEN.Add(eProperty.ExtraHP, LanguageMgr.GetTranslation("EN", "Extra Health Points"));
            m_propertyNamesEN.Add(eProperty.StyleAbsorb, LanguageMgr.GetTranslation("EN", "Style Absorb"));
            m_propertyNamesEN.Add(eProperty.ArcaneSyphon, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ArcaneSyphon"));
            m_propertyNamesEN.Add(eProperty.RealmPoints, LanguageMgr.GetTranslation("EN", "Realm Points"));
            //[Freya] Nidel
            m_propertyNamesEN.Add(eProperty.BountyPoints, LanguageMgr.GetTranslation("EN", "Bounty Points"));
            m_propertyNamesEN.Add(eProperty.XpPoints, LanguageMgr.GetTranslation("EN", "Experience Points"));

            // skills
            m_propertyNamesEN.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.TwoHanded"));
            m_propertyNamesEN.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BodyMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Chants"));
            m_propertyNamesEN.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CriticalStrike"));
            m_propertyNamesEN.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Crossbows"));
            m_propertyNamesEN.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Crushing"));
            m_propertyNamesEN.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DeathServant"));
            m_propertyNamesEN.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Deathsight"));
            m_propertyNamesEN.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DualWield"));
            m_propertyNamesEN.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EarthMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Enhancement"));
            m_propertyNamesEN.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Envenom"));
            m_propertyNamesEN.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.FireMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
            m_propertyNamesEN.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ColdMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Instruments"));
            m_propertyNamesEN.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Longbows"));
            m_propertyNamesEN.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MatterMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MindMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Painworking"));
            m_propertyNamesEN.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Parry"));
            m_propertyNamesEN.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Polearms"));
            m_propertyNamesEN.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Rejuvenation"));
            m_propertyNamesEN.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Shields"));
            m_propertyNamesEN.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Slashing"));
            m_propertyNamesEN.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Smiting"));
            m_propertyNamesEN.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Soulrending"));
            m_propertyNamesEN.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpiritMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Staff"));
            m_propertyNamesEN.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Stealth"));
            m_propertyNamesEN.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Thrusting"));
            m_propertyNamesEN.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.WindMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Sword"));
            m_propertyNamesEN.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Hammer"));
            m_propertyNamesEN.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Axe"));
            m_propertyNamesEN.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.LeftAxe"));
            m_propertyNamesEN.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Spear"));
            m_propertyNamesEN.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Mending"));
            m_propertyNamesEN.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Augmentation"));
            m_propertyNamesEN.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Darkness"));
            m_propertyNamesEN.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Suppression"));
            m_propertyNamesEN.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Runecarving"));
            m_propertyNamesEN.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Stormcalling"));
            m_propertyNamesEN.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BeastCraft"));
            m_propertyNamesEN.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.LightMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.VoidMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ManaMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Composite"));
            m_propertyNamesEN.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Battlesongs"));
            m_propertyNamesEN.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Enchantment"));

            m_propertyNamesEN.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Blades"));
            m_propertyNamesEN.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Blunt"));
            m_propertyNamesEN.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Piercing"));
            m_propertyNamesEN.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.LargeWeapon"));
            m_propertyNamesEN.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Mentalism"));
            m_propertyNamesEN.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Regrowth"));
            m_propertyNamesEN.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Nurture"));
            m_propertyNamesEN.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Nature"));
            m_propertyNamesEN.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Music"));
            m_propertyNamesEN.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CelticDual"));
            m_propertyNamesEN.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CelticSpear"));
            m_propertyNamesEN.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.RecurvedBow"));
            m_propertyNamesEN.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Valor"));
            m_propertyNamesEN.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CaveMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BoneArmy"));
            m_propertyNamesEN.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Verdant"));
            m_propertyNamesEN.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Creeping"));
            m_propertyNamesEN.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Arboreal"));
            m_propertyNamesEN.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Scythe"));
            m_propertyNamesEN.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ThrownWeapons"));
            m_propertyNamesEN.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.HandToHand"));
            m_propertyNamesEN.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ShortBow"));
            m_propertyNamesEN.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Pacification"));
            m_propertyNamesEN.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Savagery"));
            m_propertyNamesEN.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.NightshadeMagic"));
            m_propertyNamesEN.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Pathfinding"));
            m_propertyNamesEN.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Summoning"));
            m_propertyNamesEN.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Archery"));

            // Mauler
            m_propertyNamesEN.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.FistWraps"));
            m_propertyNamesEN.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MaulerStaff"));
            m_propertyNamesEN.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PowerStrikes"));
            m_propertyNamesEN.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Magnetism"));
            m_propertyNamesEN.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AuraManipulation"));


            //Catacombs skills
            m_propertyNamesEN.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Dementia"));
            m_propertyNamesEN.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ShadowMastery"));
            m_propertyNamesEN.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
            m_propertyNamesEN.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EtherealShriek"));
            m_propertyNamesEN.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PhantasmalWail"));
            m_propertyNamesEN.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpectralForce"));
            m_propertyNamesEN.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpectralGuard"));
            m_propertyNamesEN.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.OdinsWill"));
            m_propertyNamesEN.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Cursing"));
            m_propertyNamesEN.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Hexing"));
            m_propertyNamesEN.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Witchcraft"));


            // Classic Focii
            m_propertyNamesEN.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DarknessFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SuppressionFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.FireMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.WindMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.LightMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MindMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MentalismFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SummoningFocus"));
            // SI Focii
            // Mid
            m_propertyNamesEN.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
            // Alb
            m_propertyNamesEN.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PainworkingFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DeathsightFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DeathservantFocus"));
            // Hib
            m_propertyNamesEN.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.VerdantFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ArborealFocus"));
            // Catacombs Focii
            m_propertyNamesEN.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CursingFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.HexingFocus"));
            m_propertyNamesEN.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

            m_propertyNamesEN.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MaximumSpeed"));
            m_propertyNamesEN.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Concentration"));

            m_propertyNamesEN.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
            m_propertyNamesEN.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

            m_propertyNamesEN.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.HealthRegeneration"));
            m_propertyNamesEN.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PowerRegeneration"));
            m_propertyNamesEN.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
            m_propertyNamesEN.Add(eProperty.SpellRange, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpellRange"));
            m_propertyNamesEN.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ArcheryRange"));
            m_propertyNamesEN.Add(eProperty.Acuity, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Acuity"));

            m_propertyNamesEN.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AllMagicSkills"));
            m_propertyNamesEN.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
            m_propertyNamesEN.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ALLSpellLines"));
            m_propertyNamesEN.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
            m_propertyNamesEN.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AllArcherySkills"));

            m_propertyNamesEN.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EffectiveLevel"));


            //Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
            //Date : 20-Jan-2005
            //Missing bonusses begin
            m_propertyNamesEN.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EvadeChance"));
            m_propertyNamesEN.Add(eProperty.BlockChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BlockChance"));
            m_propertyNamesEN.Add(eProperty.ParryChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ParryChance"));
            m_propertyNamesEN.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.FumbleChance"));
            m_propertyNamesEN.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MeleeDamage"));
            m_propertyNamesEN.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.RangedDamage"));
            m_propertyNamesEN.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
            m_propertyNamesEN.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.StunDuration"));
            m_propertyNamesEN.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
            m_propertyNamesEN.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
            m_propertyNamesEN.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DefensiveBonus"));
            m_propertyNamesEN.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PieceAblative"));
            m_propertyNamesEN.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.NegativeReduction"));
            m_propertyNamesEN.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
            m_propertyNamesEN.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpellPowerCost"));
            m_propertyNamesEN.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.StyleCostReduction"));
            m_propertyNamesEN.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ToHitBonus"));
            m_propertyNamesEN.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ArcherySpeed"));
            m_propertyNamesEN.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ArrowRecovery"));
            m_propertyNamesEN.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.StatBuffSpells"));
            m_propertyNamesEN.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CastingSpeed"));
            m_propertyNamesEN.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ExperienceLoss"));
            m_propertyNamesEN.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
            m_propertyNamesEN.Add(eProperty.Fatigue, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.Fatigue"));
            m_propertyNamesEN.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
            m_propertyNamesEN.Add(eProperty.PowerPool, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PowerPool"));
            //Magiekraftvorrat
            m_propertyNamesEN.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ResistPierce"));
            m_propertyNamesEN.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
            m_propertyNamesEN.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.SpellDuration"));
            m_propertyNamesEN.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.StyleDamage"));
            m_propertyNamesEN.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.MeleeSpeed"));
            //Missing bonusses end

            m_propertyNamesEN.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
            m_propertyNamesEN.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
            m_propertyNamesEN.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
            m_propertyNamesEN.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
            m_propertyNamesEN.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
            m_propertyNamesEN.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PietyBonusCap"));
            m_propertyNamesEN.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
            m_propertyNamesEN.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
            m_propertyNamesEN.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
            m_propertyNamesEN.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
            m_propertyNamesEN.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.PowerBonusCap"));
            m_propertyNamesEN.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.WeaponSkill"));
            m_propertyNamesEN.Add(eProperty.AllSkills, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.AllSkills"));
            m_propertyNamesEN.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
            m_propertyNamesEN.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
            m_propertyNamesEN.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
            m_propertyNamesEN.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation("EN", "SkillBase.RegisterPropertyNames.CriticalHealHit"));
            //Forsaken Worlds: Mythical Stat Cap
            m_propertyNamesEN.Add(eProperty.MythicalStrCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Strength)"));
            m_propertyNamesEN.Add(eProperty.MythicalDexCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Dexterity)"));
            m_propertyNamesEN.Add(eProperty.MythicalConCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Constitution)"));
            m_propertyNamesEN.Add(eProperty.MythicalQuiCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Quickness)"));
            m_propertyNamesEN.Add(eProperty.MythicalIntCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Intelligence)"));
            m_propertyNamesEN.Add(eProperty.MythicalPieCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Piety)"));
            m_propertyNamesEN.Add(eProperty.MythicalChaCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Charisma)"));
            m_propertyNamesEN.Add(eProperty.MythicalEmpCapBonus, LanguageMgr.GetTranslation("EN", "Mythical Stat Cap (Empathy)"));

            log.Info("EN language: stat names loaded");
        }

        private static void RegisterPropertyNamesIT()
        {


            m_propertyNamesIT.Add(eProperty.Strength, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Strength"));
            m_propertyNamesIT.Add(eProperty.Dexterity, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Dexterity"));
            m_propertyNamesIT.Add(eProperty.Constitution, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Constitution"));
            m_propertyNamesIT.Add(eProperty.Quickness, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Quickness"));
            m_propertyNamesIT.Add(eProperty.Intelligence, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Intelligence"));
            m_propertyNamesIT.Add(eProperty.Piety, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Piety"));
            m_propertyNamesIT.Add(eProperty.Empathy, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Empathy"));
            m_propertyNamesIT.Add(eProperty.Charisma, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Charisma"));

            m_propertyNamesIT.Add(eProperty.MaxMana, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Power"));
            m_propertyNamesIT.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Hits"));

            // resists (does not say "resist" on live server)
            m_propertyNamesIT.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Body"));
            m_propertyNamesIT.Add(eProperty.Resist_Natural, LanguageMgr.GetTranslation("IT", "Essence"));
            m_propertyNamesIT.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Cold"));
            m_propertyNamesIT.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Crush"));
            m_propertyNamesIT.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Energy"));
            m_propertyNamesIT.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Heat"));
            m_propertyNamesIT.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Matter"));
            m_propertyNamesIT.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Slash"));
            m_propertyNamesIT.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Spirit"));
            m_propertyNamesIT.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Thrust"));

            // Eden - Mythirian bonus
            m_propertyNamesIT.Add(eProperty.BodyResCapBonus, LanguageMgr.GetTranslation("IT", "Body cap"));
            m_propertyNamesIT.Add(eProperty.ColdResCapBonus, LanguageMgr.GetTranslation("IT", "Cold cap"));
            m_propertyNamesIT.Add(eProperty.CrushResCapBonus, LanguageMgr.GetTranslation("IT", "Crush cap"));
            m_propertyNamesIT.Add(eProperty.EnergyResCapBonus, LanguageMgr.GetTranslation("IT", "Energy cap"));
            m_propertyNamesIT.Add(eProperty.HeatResCapBonus, LanguageMgr.GetTranslation("IT", "Heat cap"));
            m_propertyNamesIT.Add(eProperty.MatterResCapBonus, LanguageMgr.GetTranslation("IT", "Matter cap"));
            m_propertyNamesIT.Add(eProperty.SlashResCapBonus, LanguageMgr.GetTranslation("IT", "Slash cap"));
            m_propertyNamesIT.Add(eProperty.SpiritResCapBonus, LanguageMgr.GetTranslation("IT", "Spirit cap"));
            m_propertyNamesIT.Add(eProperty.ThrustResCapBonus, LanguageMgr.GetTranslation("IT", "Thrust cap"));
            m_propertyNamesIT.Add(eProperty.MythicalSafeFall, LanguageMgr.GetTranslation("IT", "Mythical Safe Fall"));
            m_propertyNamesIT.Add(eProperty.MythicalEvade, LanguageMgr.GetTranslation("IT", "Mythical Evade"));
            m_propertyNamesIT.Add(eProperty.MythicalDiscumbering, LanguageMgr.GetTranslation("IT", "Mythical Discumbering"));
            m_propertyNamesIT.Add(eProperty.MythicalCoin, LanguageMgr.GetTranslation("IT", "Mythical Coin"));
            m_propertyNamesIT.Add(eProperty.MythicalDefense, LanguageMgr.GetTranslation("IT", "Mythical Defense"));
            m_propertyNamesIT.Add(eProperty.MythicalDPS, LanguageMgr.GetTranslation("IT", "Mythical DPS"));
            m_propertyNamesIT.Add(eProperty.MythicalSiegeDamageReduction, LanguageMgr.GetTranslation("IT", "Mythical Siege Damage Ablive"));
            m_propertyNamesIT.Add(eProperty.MythicalSiegeSpeed, LanguageMgr.GetTranslation("IT", "Mythical Siege Speed"));
            m_propertyNamesIT.Add(eProperty.SpellLevel, LanguageMgr.GetTranslation("IT", "Spell Focus"));
            //Eden - special actifacts bonus
            m_propertyNamesIT.Add(eProperty.Conversion, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Conversion"));
            m_propertyNamesIT.Add(eProperty.ExtraHP, LanguageMgr.GetTranslation("IT", "Extra Health Points"));
            m_propertyNamesIT.Add(eProperty.StyleAbsorb, LanguageMgr.GetTranslation("IT", "Style Absorb"));
            m_propertyNamesIT.Add(eProperty.ArcaneSyphon, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ArcaneSyphon"));
            m_propertyNamesIT.Add(eProperty.RealmPoints, LanguageMgr.GetTranslation("IT", "Realm Points"));
            //[Freya] Nidel
            m_propertyNamesIT.Add(eProperty.BountyPoints, LanguageMgr.GetTranslation("IT", "Bounty Points"));
            m_propertyNamesIT.Add(eProperty.XpPoints, LanguageMgr.GetTranslation("IT", "Experience Points"));

            // skills
            m_propertyNamesIT.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.TwoHanded"));
            m_propertyNamesIT.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BodyMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Chants"));
            m_propertyNamesIT.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CriticalStrike"));
            m_propertyNamesIT.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Crossbows"));
            m_propertyNamesIT.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Crushing"));
            m_propertyNamesIT.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DeathServant"));
            m_propertyNamesIT.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Deathsight"));
            m_propertyNamesIT.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DualWield"));
            m_propertyNamesIT.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EarthMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Enhancement"));
            m_propertyNamesIT.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Envenom"));
            m_propertyNamesIT.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.FireMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
            m_propertyNamesIT.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ColdMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Instruments"));
            m_propertyNamesIT.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Longbows"));
            m_propertyNamesIT.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MatterMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MindMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Painworking"));
            m_propertyNamesIT.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Parry"));
            m_propertyNamesIT.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Polearms"));
            m_propertyNamesIT.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Rejuvenation"));
            m_propertyNamesIT.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Shields"));
            m_propertyNamesIT.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Slashing"));
            m_propertyNamesIT.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Smiting"));
            m_propertyNamesIT.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Soulrending"));
            m_propertyNamesIT.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpiritMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Staff"));
            m_propertyNamesIT.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Stealth"));
            m_propertyNamesIT.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Thrusting"));
            m_propertyNamesIT.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.WindMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Sword"));
            m_propertyNamesIT.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Hammer"));
            m_propertyNamesIT.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Axe"));
            m_propertyNamesIT.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.LeftAxe"));
            m_propertyNamesIT.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Spear"));
            m_propertyNamesIT.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Mending"));
            m_propertyNamesIT.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Augmentation"));
            m_propertyNamesIT.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Darkness"));
            m_propertyNamesIT.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Suppression"));
            m_propertyNamesIT.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Runecarving"));
            m_propertyNamesIT.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Stormcalling"));
            m_propertyNamesIT.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BeastCraft"));
            m_propertyNamesIT.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.LightMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.VoidMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ManaMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Composite"));
            m_propertyNamesIT.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Battlesongs"));
            m_propertyNamesIT.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Enchantment"));

            m_propertyNamesIT.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Blades"));
            m_propertyNamesIT.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Blunt"));
            m_propertyNamesIT.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Piercing"));
            m_propertyNamesIT.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.LargeWeapon"));
            m_propertyNamesIT.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Mentalism"));
            m_propertyNamesIT.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Regrowth"));
            m_propertyNamesIT.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Nurture"));
            m_propertyNamesIT.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Nature"));
            m_propertyNamesIT.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Music"));
            m_propertyNamesIT.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CelticDual"));
            m_propertyNamesIT.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CelticSpear"));
            m_propertyNamesIT.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.RecurvedBow"));
            m_propertyNamesIT.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Valor"));
            m_propertyNamesIT.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CaveMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BoneArmy"));
            m_propertyNamesIT.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Verdant"));
            m_propertyNamesIT.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Creeping"));
            m_propertyNamesIT.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Arboreal"));
            m_propertyNamesIT.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Scythe"));
            m_propertyNamesIT.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ThrownWeapons"));
            m_propertyNamesIT.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.HandToHand"));
            m_propertyNamesIT.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ShortBow"));
            m_propertyNamesIT.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Pacification"));
            m_propertyNamesIT.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Savagery"));
            m_propertyNamesIT.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.NightshadeMagic"));
            m_propertyNamesIT.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Pathfinding"));
            m_propertyNamesIT.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Summoning"));
            m_propertyNamesIT.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Archery"));

            // Mauler
            m_propertyNamesIT.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.FistWraps"));
            m_propertyNamesIT.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MaulerStaff"));
            m_propertyNamesIT.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PowerStrikes"));
            m_propertyNamesIT.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Magnetism"));
            m_propertyNamesIT.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AuraManipulation"));


            //Catacombs skills
            m_propertyNamesIT.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Dementia"));
            m_propertyNamesIT.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ShadowMastery"));
            m_propertyNamesIT.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
            m_propertyNamesIT.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EtherealShriek"));
            m_propertyNamesIT.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PhantasmalWail"));
            m_propertyNamesIT.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpectralForce"));
            m_propertyNamesIT.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpectralGuard"));
            m_propertyNamesIT.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.OdinsWill"));
            m_propertyNamesIT.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Cursing"));
            m_propertyNamesIT.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Hexing"));
            m_propertyNamesIT.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Witchcraft"));


            // Classic Focii
            m_propertyNamesIT.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DarknessFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SuppressionFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.FireMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.WindMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.LightMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MindMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MentalismFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SummoningFocus"));
            // SI Focii
            // Mid
            m_propertyNamesIT.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
            // Alb
            m_propertyNamesIT.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PainworkingFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DeathsightFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DeathservantFocus"));
            // Hib
            m_propertyNamesIT.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.VerdantFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ArborealFocus"));
            // Catacombs Focii
            m_propertyNamesIT.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CursingFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.HexingFocus"));
            m_propertyNamesIT.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

            m_propertyNamesIT.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MaximumSpeed"));
            m_propertyNamesIT.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Concentration"));

            m_propertyNamesIT.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
            m_propertyNamesIT.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

            m_propertyNamesIT.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.HealthRegeneration"));
            m_propertyNamesIT.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PowerRegeneration"));
            m_propertyNamesIT.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
            m_propertyNamesIT.Add(eProperty.SpellRange, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpellRange"));
            m_propertyNamesIT.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ArcheryRange"));
            m_propertyNamesIT.Add(eProperty.Acuity, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Acuity"));

            m_propertyNamesIT.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AllMagicSkills"));
            m_propertyNamesIT.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
            m_propertyNamesIT.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ALLSpellLines"));
            m_propertyNamesIT.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
            m_propertyNamesIT.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AllArcherySkills"));

            m_propertyNamesIT.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EffectiveLevel"));


            //Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
            //Date : 20-Jan-2005
            //Missing bonusses begin
            m_propertyNamesIT.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EvadeChance"));
            m_propertyNamesIT.Add(eProperty.BlockChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BlockChance"));
            m_propertyNamesIT.Add(eProperty.ParryChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ParryChance"));
            m_propertyNamesIT.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.FumbleChance"));
            m_propertyNamesIT.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MeleeDamage"));
            m_propertyNamesIT.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.RangedDamage"));
            m_propertyNamesIT.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
            m_propertyNamesIT.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.StunDuration"));
            m_propertyNamesIT.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
            m_propertyNamesIT.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
            m_propertyNamesIT.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DefensiveBonus"));
            m_propertyNamesIT.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PieceAblative"));
            m_propertyNamesIT.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.NegativeReduction"));
            m_propertyNamesIT.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
            m_propertyNamesIT.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpellPowerCost"));
            m_propertyNamesIT.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.StyleCostReduction"));
            m_propertyNamesIT.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ToHitBonus"));
            m_propertyNamesIT.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ArcherySpeed"));
            m_propertyNamesIT.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ArrowRecovery"));
            m_propertyNamesIT.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.StatBuffSpells"));
            m_propertyNamesIT.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CastingSpeed"));
            m_propertyNamesIT.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ExperienceLoss"));
            m_propertyNamesIT.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
            m_propertyNamesIT.Add(eProperty.Fatigue, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.Fatigue"));
            m_propertyNamesIT.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
            m_propertyNamesIT.Add(eProperty.PowerPool, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PowerPool"));
            //Magiekraftvorrat
            m_propertyNamesIT.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ResistPierce"));
            m_propertyNamesIT.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
            m_propertyNamesIT.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.SpellDuration"));
            m_propertyNamesIT.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.StyleDamage"));
            m_propertyNamesIT.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.MeleeSpeed"));
            //Missing bonusses end

            m_propertyNamesIT.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
            m_propertyNamesIT.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
            m_propertyNamesIT.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
            m_propertyNamesIT.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
            m_propertyNamesIT.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
            m_propertyNamesIT.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PietyBonusCap"));
            m_propertyNamesIT.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
            m_propertyNamesIT.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
            m_propertyNamesIT.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
            m_propertyNamesIT.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
            m_propertyNamesIT.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.PowerBonusCap"));
            m_propertyNamesIT.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.WeaponSkill"));
            m_propertyNamesIT.Add(eProperty.AllSkills, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.AllSkills"));
            m_propertyNamesIT.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
            m_propertyNamesIT.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
            m_propertyNamesIT.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
            m_propertyNamesIT.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation("IT", "SkillBase.RegisterPropertyNames.CriticalHealHit"));
            //Forsaken Worlds: Mythical Stat Cap
            m_propertyNamesIT.Add(eProperty.MythicalStrCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Strength)"));
            m_propertyNamesIT.Add(eProperty.MythicalDexCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Dexterity)"));
            m_propertyNamesIT.Add(eProperty.MythicalConCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Constitution)"));
            m_propertyNamesIT.Add(eProperty.MythicalQuiCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Quickness)"));
            m_propertyNamesIT.Add(eProperty.MythicalIntCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Intelligence)"));
            m_propertyNamesIT.Add(eProperty.MythicalPieCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Piety)"));
            m_propertyNamesIT.Add(eProperty.MythicalChaCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Charisma)"));
            m_propertyNamesIT.Add(eProperty.MythicalEmpCapBonus, LanguageMgr.GetTranslation("IT", "Mythical Stat Cap (Empathy)"));

            log.Info("IT language: stat names loaded");
        }


       
        private static void RegisterPropertyNamesES()
        {


            m_propertyNamesES.Add(eProperty.Strength, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Strength"));
            m_propertyNamesES.Add(eProperty.Dexterity, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Dexterity"));
            m_propertyNamesES.Add(eProperty.Constitution, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Constitution"));
            m_propertyNamesES.Add(eProperty.Quickness, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Quickness"));
            m_propertyNamesES.Add(eProperty.Intelligence, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Intelligence"));
            m_propertyNamesES.Add(eProperty.Piety, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Piety"));
            m_propertyNamesES.Add(eProperty.Empathy, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Empathy"));
            m_propertyNamesES.Add(eProperty.Charisma, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Charisma"));

            m_propertyNamesES.Add(eProperty.MaxMana, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Power"));
            m_propertyNamesES.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Hits"));

            // resists (does not say "resist" on live server)
            m_propertyNamesES.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Body"));
            m_propertyNamesES.Add(eProperty.Resist_Natural, LanguageMgr.GetTranslation("ES", "Essence"));
            m_propertyNamesES.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Cold"));
            m_propertyNamesES.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Crush"));
            m_propertyNamesES.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Energy"));
            m_propertyNamesES.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Heat"));
            m_propertyNamesES.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Matter"));
            m_propertyNamesES.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Slash"));
            m_propertyNamesES.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Spirit"));
            m_propertyNamesES.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Thrust"));

            // Eden - Mythirian bonus
            m_propertyNamesES.Add(eProperty.BodyResCapBonus, LanguageMgr.GetTranslation("ES", "Body cap"));
            m_propertyNamesES.Add(eProperty.ColdResCapBonus, LanguageMgr.GetTranslation("ES", "Cold cap"));
            m_propertyNamesES.Add(eProperty.CrushResCapBonus, LanguageMgr.GetTranslation("ES", "Crush cap"));
            m_propertyNamesES.Add(eProperty.EnergyResCapBonus, LanguageMgr.GetTranslation("ES", "Energy cap"));
            m_propertyNamesES.Add(eProperty.HeatResCapBonus, LanguageMgr.GetTranslation("ES", "Heat cap"));
            m_propertyNamesES.Add(eProperty.MatterResCapBonus, LanguageMgr.GetTranslation("ES", "Matter cap"));
            m_propertyNamesES.Add(eProperty.SlashResCapBonus, LanguageMgr.GetTranslation("ES", "Slash cap"));
            m_propertyNamesES.Add(eProperty.SpiritResCapBonus, LanguageMgr.GetTranslation("ES", "Spirit cap"));
            m_propertyNamesES.Add(eProperty.ThrustResCapBonus, LanguageMgr.GetTranslation("ES", "Thrust cap"));
            m_propertyNamesES.Add(eProperty.MythicalSafeFall, LanguageMgr.GetTranslation("ES", "Mythical Safe Fall"));
            m_propertyNamesES.Add(eProperty.MythicalEvade, LanguageMgr.GetTranslation("ES", "Mythical Evade"));
            m_propertyNamesES.Add(eProperty.MythicalDiscumbering, LanguageMgr.GetTranslation("ES", "Mythical Discumbering"));
            m_propertyNamesES.Add(eProperty.MythicalCoin, LanguageMgr.GetTranslation("ES", "Mythical Coin"));
            m_propertyNamesES.Add(eProperty.MythicalDefense, LanguageMgr.GetTranslation("ES", "Mythical Defense"));
            m_propertyNamesES.Add(eProperty.MythicalDPS, LanguageMgr.GetTranslation("ES", "Mythical DPS"));
            m_propertyNamesES.Add(eProperty.MythicalSiegeDamageReduction, LanguageMgr.GetTranslation("ES", "Mythical Siege Damage Ablive"));
            m_propertyNamesES.Add(eProperty.MythicalSiegeSpeed, LanguageMgr.GetTranslation("ES", "Mythical Siege Speed"));
            m_propertyNamesES.Add(eProperty.SpellLevel, LanguageMgr.GetTranslation("ES", "Spell Focus"));
            //Eden - special actifacts bonus
            m_propertyNamesES.Add(eProperty.Conversion, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Conversion"));
            m_propertyNamesES.Add(eProperty.ExtraHP, LanguageMgr.GetTranslation("ES", "Extra Health Points"));
            m_propertyNamesES.Add(eProperty.StyleAbsorb, LanguageMgr.GetTranslation("ES", "Style Absorb"));
            m_propertyNamesES.Add(eProperty.ArcaneSyphon, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ArcaneSyphon"));
            m_propertyNamesES.Add(eProperty.RealmPoints, LanguageMgr.GetTranslation("ES", "Realm Points"));
            //[Freya] Nidel
            m_propertyNamesES.Add(eProperty.BountyPoints, LanguageMgr.GetTranslation("ES", "Bounty Points"));
            m_propertyNamesES.Add(eProperty.XpPoints, LanguageMgr.GetTranslation("ES", "Experience Points"));

            // skills
            m_propertyNamesES.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.TwoHanded"));
            m_propertyNamesES.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BodyMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Chants"));
            m_propertyNamesES.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CriticalStrike"));
            m_propertyNamesES.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Crossbows"));
            m_propertyNamesES.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Crushing"));
            m_propertyNamesES.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DeathServant"));
            m_propertyNamesES.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Deathsight"));
            m_propertyNamesES.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DualWield"));
            m_propertyNamesES.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EarthMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Enhancement"));
            m_propertyNamesES.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Envenom"));
            m_propertyNamesES.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.FireMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
            m_propertyNamesES.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ColdMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Instruments"));
            m_propertyNamesES.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Longbows"));
            m_propertyNamesES.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MatterMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MindMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Painworking"));
            m_propertyNamesES.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Parry"));
            m_propertyNamesES.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Polearms"));
            m_propertyNamesES.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Rejuvenation"));
            m_propertyNamesES.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Shields"));
            m_propertyNamesES.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Slashing"));
            m_propertyNamesES.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Smiting"));
            m_propertyNamesES.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Soulrending"));
            m_propertyNamesES.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpiritMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Staff"));
            m_propertyNamesES.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Stealth"));
            m_propertyNamesES.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Thrusting"));
            m_propertyNamesES.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.WindMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Sword"));
            m_propertyNamesES.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Hammer"));
            m_propertyNamesES.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Axe"));
            m_propertyNamesES.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.LeftAxe"));
            m_propertyNamesES.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Spear"));
            m_propertyNamesES.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Mending"));
            m_propertyNamesES.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Augmentation"));
            m_propertyNamesES.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Darkness"));
            m_propertyNamesES.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Suppression"));
            m_propertyNamesES.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Runecarving"));
            m_propertyNamesES.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Stormcalling"));
            m_propertyNamesES.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BeastCraft"));
            m_propertyNamesES.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.LightMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.VoidMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ManaMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Composite"));
            m_propertyNamesES.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Battlesongs"));
            m_propertyNamesES.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Enchantment"));

            m_propertyNamesES.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Blades"));
            m_propertyNamesES.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Blunt"));
            m_propertyNamesES.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Piercing"));
            m_propertyNamesES.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.LargeWeapon"));
            m_propertyNamesES.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Mentalism"));
            m_propertyNamesES.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Regrowth"));
            m_propertyNamesES.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Nurture"));
            m_propertyNamesES.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Nature"));
            m_propertyNamesES.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Music"));
            m_propertyNamesES.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CelticDual"));
            m_propertyNamesES.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CelticSpear"));
            m_propertyNamesES.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.RecurvedBow"));
            m_propertyNamesES.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Valor"));
            m_propertyNamesES.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CaveMagic"));
            m_propertyNamesES.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BoneArmy"));
            m_propertyNamesES.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Verdant"));
            m_propertyNamesES.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Creeping"));
            m_propertyNamesES.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Arboreal"));
            m_propertyNamesES.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Scythe"));
            m_propertyNamesES.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ThrownWeapons"));
            m_propertyNamesES.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.HandToHand"));
            m_propertyNamesES.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ShortBow"));
            m_propertyNamesES.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Pacification"));
            m_propertyNamesES.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Savagery"));
            m_propertyNamesES.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.NightshadeMagic"));
            m_propertyNamesES.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Pathfinding"));
            m_propertyNamesES.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Summoning"));
            m_propertyNamesES.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Archery"));

            // Mauler
            m_propertyNamesES.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.FistWraps"));
            m_propertyNamesES.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MaulerStaff"));
            m_propertyNamesES.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PowerStrikes"));
            m_propertyNamesES.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Magnetism"));
            m_propertyNamesES.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AuraManipulation"));


            //Catacombs skills
            m_propertyNamesES.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Dementia"));
            m_propertyNamesES.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ShadowMastery"));
            m_propertyNamesES.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
            m_propertyNamesES.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EtherealShriek"));
            m_propertyNamesES.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PhantasmalWail"));
            m_propertyNamesES.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpectralForce"));
            m_propertyNamesES.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpectralGuard"));
            m_propertyNamesES.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.OdinsWill"));
            m_propertyNamesES.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Cursing"));
            m_propertyNamesES.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Hexing"));
            m_propertyNamesES.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Witchcraft"));


            // Classic Focii
            m_propertyNamesES.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DarknessFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SuppressionFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.FireMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.WindMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.LightMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MindMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MentalismFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SummoningFocus"));
            // SI Focii
            // Mid
            m_propertyNamesES.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
            // Alb
            m_propertyNamesES.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PainworkingFocus"));
            m_propertyNamesES.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DeathsightFocus"));
            m_propertyNamesES.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DeathservantFocus"));
            // Hib
            m_propertyNamesES.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.VerdantFocus"));
            m_propertyNamesES.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ArborealFocus"));
            // Catacombs Focii
            m_propertyNamesES.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
            m_propertyNamesES.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
            m_propertyNamesES.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CursingFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.HexingFocus"));
            m_propertyNamesES.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

            m_propertyNamesES.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MaximumSpeed"));
            m_propertyNamesES.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Concentration"));

            m_propertyNamesES.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
            m_propertyNamesES.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

            m_propertyNamesES.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.HealthRegeneration"));
            m_propertyNamesES.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PowerRegeneration"));
            m_propertyNamesES.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
            m_propertyNamesES.Add(eProperty.SpellRange, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpellRange"));
            m_propertyNamesES.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ArcheryRange"));
            m_propertyNamesES.Add(eProperty.Acuity, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Acuity"));

            m_propertyNamesES.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AllMagicSkills"));
            m_propertyNamesES.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
            m_propertyNamesES.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ALLSpellLines"));
            m_propertyNamesES.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
            m_propertyNamesES.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AllArcherySkills"));

            m_propertyNamesES.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EffectiveLevel"));


            //Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
            //Date : 20-Jan-2005
            //Missing bonusses begin
            m_propertyNamesES.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EvadeChance"));
            m_propertyNamesES.Add(eProperty.BlockChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BlockChance"));
            m_propertyNamesES.Add(eProperty.ParryChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ParryChance"));
            m_propertyNamesES.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.FumbleChance"));
            m_propertyNamesES.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MeleeDamage"));
            m_propertyNamesES.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.RangedDamage"));
            m_propertyNamesES.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
            m_propertyNamesES.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.StunDuration"));
            m_propertyNamesES.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
            m_propertyNamesES.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
            m_propertyNamesES.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DefensiveBonus"));
            m_propertyNamesES.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PieceAblative"));
            m_propertyNamesES.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.NegativeReduction"));
            m_propertyNamesES.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
            m_propertyNamesES.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpellPowerCost"));
            m_propertyNamesES.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.StyleCostReduction"));
            m_propertyNamesES.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ToHitBonus"));
            m_propertyNamesES.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ArcherySpeed"));
            m_propertyNamesES.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ArrowRecovery"));
            m_propertyNamesES.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.StatBuffSpells"));
            m_propertyNamesES.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CastingSpeed"));
            m_propertyNamesES.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ExperienceLoss"));
            m_propertyNamesES.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
            m_propertyNamesES.Add(eProperty.Fatigue, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.Fatigue"));
            m_propertyNamesES.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
            m_propertyNamesES.Add(eProperty.PowerPool, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PowerPool"));
            //Magiekraftvorrat
            m_propertyNamesES.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ResistPierce"));
            m_propertyNamesES.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
            m_propertyNamesES.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.SpellDuration"));
            m_propertyNamesES.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.StyleDamage"));
            m_propertyNamesES.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.MeleeSpeed"));
            //Missing bonusses end

            m_propertyNamesES.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
            m_propertyNamesES.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
            m_propertyNamesES.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
            m_propertyNamesES.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
            m_propertyNamesES.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
            m_propertyNamesES.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PietyBonusCap"));
            m_propertyNamesES.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
            m_propertyNamesES.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
            m_propertyNamesES.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
            m_propertyNamesES.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
            m_propertyNamesES.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.PowerBonusCap"));
            m_propertyNamesES.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.WeaponSkill"));
            m_propertyNamesES.Add(eProperty.AllSkills, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.AllSkills"));
            m_propertyNamesES.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
            m_propertyNamesES.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
            m_propertyNamesES.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
            m_propertyNamesES.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation("ES", "SkillBase.RegisterPropertyNames.CriticalHealHit"));
            //Forsaken Worlds: Mythical Stat Cap
            m_propertyNamesES.Add(eProperty.MythicalStrCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Strength)"));
            m_propertyNamesES.Add(eProperty.MythicalDexCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Dexterity)"));
            m_propertyNamesES.Add(eProperty.MythicalConCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Constitution)"));
            m_propertyNamesES.Add(eProperty.MythicalQuiCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Quickness)"));
            m_propertyNamesES.Add(eProperty.MythicalIntCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Intelligence)"));
            m_propertyNamesES.Add(eProperty.MythicalPieCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Piety)"));
            m_propertyNamesES.Add(eProperty.MythicalChaCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Charisma)"));
            m_propertyNamesES.Add(eProperty.MythicalEmpCapBonus, LanguageMgr.GetTranslation("ES", "Mythical Stat Cap (Empathy)"));

            log.Info("ES language: stat names loaded");
        }

        private static void RegisterPropertyNamesCZ()
        {


            m_propertyNamesCZ.Add(eProperty.Strength, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Strength"));
            m_propertyNamesCZ.Add(eProperty.Dexterity, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Dexterity"));
            m_propertyNamesCZ.Add(eProperty.Constitution, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Constitution"));
            m_propertyNamesCZ.Add(eProperty.Quickness, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Quickness"));
            m_propertyNamesCZ.Add(eProperty.Intelligence, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Intelligence"));
            m_propertyNamesCZ.Add(eProperty.Piety, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Piety"));
            m_propertyNamesCZ.Add(eProperty.Empathy, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Empathy"));
            m_propertyNamesCZ.Add(eProperty.Charisma, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Charisma"));

            m_propertyNamesCZ.Add(eProperty.MaxMana, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Power"));
            m_propertyNamesCZ.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Hits"));

            // resists (does not say "resist" on live server)
            m_propertyNamesCZ.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Body"));
            m_propertyNamesCZ.Add(eProperty.Resist_Natural, LanguageMgr.GetTranslation("CZ", "Essence"));
            m_propertyNamesCZ.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Cold"));
            m_propertyNamesCZ.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Crush"));
            m_propertyNamesCZ.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Energy"));
            m_propertyNamesCZ.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Heat"));
            m_propertyNamesCZ.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Matter"));
            m_propertyNamesCZ.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Slash"));
            m_propertyNamesCZ.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Spirit"));
            m_propertyNamesCZ.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Thrust"));

            // Eden - Mythirian bonus
            m_propertyNamesCZ.Add(eProperty.BodyResCapBonus, LanguageMgr.GetTranslation("CZ", "Body cap"));
            m_propertyNamesCZ.Add(eProperty.ColdResCapBonus, LanguageMgr.GetTranslation("CZ", "Cold cap"));
            m_propertyNamesCZ.Add(eProperty.CrushResCapBonus, LanguageMgr.GetTranslation("CZ", "Crush cap"));
            m_propertyNamesCZ.Add(eProperty.EnergyResCapBonus, LanguageMgr.GetTranslation("CZ", "Energy cap"));
            m_propertyNamesCZ.Add(eProperty.HeatResCapBonus, LanguageMgr.GetTranslation("CZ", "Heat cap"));
            m_propertyNamesCZ.Add(eProperty.MatterResCapBonus, LanguageMgr.GetTranslation("CZ", "Matter cap"));
            m_propertyNamesCZ.Add(eProperty.SlashResCapBonus, LanguageMgr.GetTranslation("CZ", "Slash cap"));
            m_propertyNamesCZ.Add(eProperty.SpiritResCapBonus, LanguageMgr.GetTranslation("CZ", "Spirit cap"));
            m_propertyNamesCZ.Add(eProperty.ThrustResCapBonus, LanguageMgr.GetTranslation("CZ", "Thrust cap"));
            m_propertyNamesCZ.Add(eProperty.MythicalSafeFall, LanguageMgr.GetTranslation("CZ", "Mythical Safe Fall"));
            m_propertyNamesCZ.Add(eProperty.MythicalEvade, LanguageMgr.GetTranslation("CZ", "Mythical Evade"));
            m_propertyNamesCZ.Add(eProperty.MythicalDiscumbering, LanguageMgr.GetTranslation("CZ", "Mythical Discumbering"));
            m_propertyNamesCZ.Add(eProperty.MythicalCoin, LanguageMgr.GetTranslation("CZ", "Mythical Coin"));
            m_propertyNamesCZ.Add(eProperty.MythicalDefense, LanguageMgr.GetTranslation("CZ", "Mythical Defense"));
            m_propertyNamesCZ.Add(eProperty.MythicalDPS, LanguageMgr.GetTranslation("CZ", "Mythical DPS"));
            m_propertyNamesCZ.Add(eProperty.MythicalSiegeDamageReduction, LanguageMgr.GetTranslation("CZ", "Mythical Siege Damage Ablive"));
            m_propertyNamesCZ.Add(eProperty.MythicalSiegeSpeed, LanguageMgr.GetTranslation("CZ", "Mythical Siege Speed"));
            m_propertyNamesCZ.Add(eProperty.SpellLevel, LanguageMgr.GetTranslation("CZ", "Spell Focus"));
            //Eden - special actifacts bonus
            m_propertyNamesCZ.Add(eProperty.Conversion, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Conversion"));
            m_propertyNamesCZ.Add(eProperty.ExtraHP, LanguageMgr.GetTranslation("CZ", "Extra Health Points"));
            m_propertyNamesCZ.Add(eProperty.StyleAbsorb, LanguageMgr.GetTranslation("CZ", "Style Absorb"));
            m_propertyNamesCZ.Add(eProperty.ArcaneSyphon, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ArcaneSyphon"));
            m_propertyNamesCZ.Add(eProperty.RealmPoints, LanguageMgr.GetTranslation("CZ", "Realm Points"));
            //[Freya] Nidel
            m_propertyNamesCZ.Add(eProperty.BountyPoints, LanguageMgr.GetTranslation("CZ", "Bounty Points"));
            m_propertyNamesCZ.Add(eProperty.XpPoints, LanguageMgr.GetTranslation("CZ", "Experience Points"));

            // skills
            m_propertyNamesCZ.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.TwoHanded"));
            m_propertyNamesCZ.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BodyMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Chants"));
            m_propertyNamesCZ.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CriticalStrike"));
            m_propertyNamesCZ.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Crossbows"));
            m_propertyNamesCZ.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Crushing"));
            m_propertyNamesCZ.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DeathServant"));
            m_propertyNamesCZ.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Deathsight"));
            m_propertyNamesCZ.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DualWield"));
            m_propertyNamesCZ.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EarthMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Enhancement"));
            m_propertyNamesCZ.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Envenom"));
            m_propertyNamesCZ.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.FireMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
            m_propertyNamesCZ.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ColdMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Instruments"));
            m_propertyNamesCZ.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Longbows"));
            m_propertyNamesCZ.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MatterMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MindMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Painworking"));
            m_propertyNamesCZ.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Parry"));
            m_propertyNamesCZ.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Polearms"));
            m_propertyNamesCZ.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Rejuvenation"));
            m_propertyNamesCZ.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Shields"));
            m_propertyNamesCZ.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Slashing"));
            m_propertyNamesCZ.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Smiting"));
            m_propertyNamesCZ.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Soulrending"));
            m_propertyNamesCZ.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpiritMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Staff"));
            m_propertyNamesCZ.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Stealth"));
            m_propertyNamesCZ.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Thrusting"));
            m_propertyNamesCZ.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.WindMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Sword"));
            m_propertyNamesCZ.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Hammer"));
            m_propertyNamesCZ.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Axe"));
            m_propertyNamesCZ.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.LeftAxe"));
            m_propertyNamesCZ.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Spear"));
            m_propertyNamesCZ.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Mending"));
            m_propertyNamesCZ.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Augmentation"));
            m_propertyNamesCZ.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Darkness"));
            m_propertyNamesCZ.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Suppression"));
            m_propertyNamesCZ.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Runecarving"));
            m_propertyNamesCZ.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Stormcalling"));
            m_propertyNamesCZ.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BeastCraft"));
            m_propertyNamesCZ.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.LightMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.VoidMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ManaMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Composite"));
            m_propertyNamesCZ.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Battlesongs"));
            m_propertyNamesCZ.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Enchantment"));

            m_propertyNamesCZ.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Blades"));
            m_propertyNamesCZ.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Blunt"));
            m_propertyNamesCZ.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Piercing"));
            m_propertyNamesCZ.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.LargeWeapon"));
            m_propertyNamesCZ.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Mentalism"));
            m_propertyNamesCZ.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Regrowth"));
            m_propertyNamesCZ.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Nurture"));
            m_propertyNamesCZ.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Nature"));
            m_propertyNamesCZ.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Music"));
            m_propertyNamesCZ.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CelticDual"));
            m_propertyNamesCZ.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CelticSpear"));
            m_propertyNamesCZ.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.RecurvedBow"));
            m_propertyNamesCZ.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Valor"));
            m_propertyNamesCZ.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CaveMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BoneArmy"));
            m_propertyNamesCZ.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Verdant"));
            m_propertyNamesCZ.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Creeping"));
            m_propertyNamesCZ.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Arboreal"));
            m_propertyNamesCZ.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Scythe"));
            m_propertyNamesCZ.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ThrownWeapons"));
            m_propertyNamesCZ.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.HandToHand"));
            m_propertyNamesCZ.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ShortBow"));
            m_propertyNamesCZ.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Pacification"));
            m_propertyNamesCZ.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Savagery"));
            m_propertyNamesCZ.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.NightshadeMagic"));
            m_propertyNamesCZ.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Pathfinding"));
            m_propertyNamesCZ.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Summoning"));
            m_propertyNamesCZ.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Archery"));

            // Mauler
            m_propertyNamesCZ.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.FistWraps"));
            m_propertyNamesCZ.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MaulerStaff"));
            m_propertyNamesCZ.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PowerStrikes"));
            m_propertyNamesCZ.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Magnetism"));
            m_propertyNamesCZ.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AuraManipulation"));


            //Catacombs skills
            m_propertyNamesCZ.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Dementia"));
            m_propertyNamesCZ.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ShadowMastery"));
            m_propertyNamesCZ.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
            m_propertyNamesCZ.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EtherealShriek"));
            m_propertyNamesCZ.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PhantasmalWail"));
            m_propertyNamesCZ.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpectralForce"));
            m_propertyNamesCZ.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpectralGuard"));
            m_propertyNamesCZ.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.OdinsWill"));
            m_propertyNamesCZ.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Cursing"));
            m_propertyNamesCZ.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Hexing"));
            m_propertyNamesCZ.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Witchcraft"));


            // Classic Focii
            m_propertyNamesCZ.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DarknessFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SuppressionFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.FireMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.WindMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.LightMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MindMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MentalismFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SummoningFocus"));
            // SI Focii
            // Mid
            m_propertyNamesCZ.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
            // Alb
            m_propertyNamesCZ.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PainworkingFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DeathsightFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DeathservantFocus"));
            // Hib
            m_propertyNamesCZ.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.VerdantFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ArborealFocus"));
            // Catacombs Focii
            m_propertyNamesCZ.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CursingFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.HexingFocus"));
            m_propertyNamesCZ.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

            m_propertyNamesCZ.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MaximumSpeed"));
            m_propertyNamesCZ.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Concentration"));

            m_propertyNamesCZ.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
            m_propertyNamesCZ.Add(eProperty.ArmorAbsorption, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

            m_propertyNamesCZ.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.HealthRegeneration"));
            m_propertyNamesCZ.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PowerRegeneration"));
            m_propertyNamesCZ.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
            m_propertyNamesCZ.Add(eProperty.SpellRange, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpellRange"));
            m_propertyNamesCZ.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ArcheryRange"));
            m_propertyNamesCZ.Add(eProperty.Acuity, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Acuity"));

            m_propertyNamesCZ.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AllMagicSkills"));
            m_propertyNamesCZ.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
            m_propertyNamesCZ.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ALLSpellLines"));
            m_propertyNamesCZ.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
            m_propertyNamesCZ.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AllArcherySkills"));

            m_propertyNamesCZ.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EffectiveLevel"));


            //Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
            //Date : 20-Jan-2005
            //Missing bonusses begin
            m_propertyNamesCZ.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EvadeChance"));
            m_propertyNamesCZ.Add(eProperty.BlockChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BlockChance"));
            m_propertyNamesCZ.Add(eProperty.ParryChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ParryChance"));
            m_propertyNamesCZ.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.FumbleChance"));
            m_propertyNamesCZ.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MeleeDamage"));
            m_propertyNamesCZ.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.RangedDamage"));
            m_propertyNamesCZ.Add(eProperty.MesmerizeDurationReduction, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
            m_propertyNamesCZ.Add(eProperty.StunDurationReduction, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.StunDuration"));
            m_propertyNamesCZ.Add(eProperty.SpeedDecreaseDurationReduction, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
            m_propertyNamesCZ.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
            m_propertyNamesCZ.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DefensiveBonus"));
            m_propertyNamesCZ.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PieceAblative"));
            m_propertyNamesCZ.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.NegativeReduction"));
            m_propertyNamesCZ.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
            m_propertyNamesCZ.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpellPowerCost"));
            m_propertyNamesCZ.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.StyleCostReduction"));
            m_propertyNamesCZ.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ToHitBonus"));
            m_propertyNamesCZ.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ArcherySpeed"));
            m_propertyNamesCZ.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ArrowRecovery"));
            m_propertyNamesCZ.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.StatBuffSpells"));
            m_propertyNamesCZ.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CastingSpeed"));
            m_propertyNamesCZ.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ExperienceLoss"));
            m_propertyNamesCZ.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
            m_propertyNamesCZ.Add(eProperty.Fatigue, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.Fatigue"));
            m_propertyNamesCZ.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
            m_propertyNamesCZ.Add(eProperty.PowerPool, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PowerPool"));
            //Magiekraftvorrat
            m_propertyNamesCZ.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ResistPierce"));
            m_propertyNamesCZ.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
            m_propertyNamesCZ.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.SpellDuration"));
            m_propertyNamesCZ.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.StyleDamage"));
            m_propertyNamesCZ.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.MeleeSpeed"));
            //Missing bonusses end

            m_propertyNamesCZ.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
            m_propertyNamesCZ.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
            m_propertyNamesCZ.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
            m_propertyNamesCZ.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
            m_propertyNamesCZ.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
            m_propertyNamesCZ.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PietyBonusCap"));
            m_propertyNamesCZ.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
            m_propertyNamesCZ.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
            m_propertyNamesCZ.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
            m_propertyNamesCZ.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
            m_propertyNamesCZ.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.PowerBonusCap"));
            m_propertyNamesCZ.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.WeaponSkill"));
            m_propertyNamesCZ.Add(eProperty.AllSkills, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.AllSkills"));
            m_propertyNamesCZ.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
            m_propertyNamesCZ.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
            m_propertyNamesCZ.Add(eProperty.CriticalSpellHitChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CriticalSpellHit"));
            m_propertyNamesCZ.Add(eProperty.CriticalHealHitChance, LanguageMgr.GetTranslation("CZ", "SkillBase.RegisterPropertyNames.CriticalHealHit"));
            //Forsaken Worlds: Mythical Stat Cap
            m_propertyNamesCZ.Add(eProperty.MythicalStrCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Strength)"));
            m_propertyNamesCZ.Add(eProperty.MythicalDexCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Dexterity)"));
            m_propertyNamesCZ.Add(eProperty.MythicalConCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Constitution)"));
            m_propertyNamesCZ.Add(eProperty.MythicalQuiCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Quickness)"));
            m_propertyNamesCZ.Add(eProperty.MythicalIntCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Intelligence)"));
            m_propertyNamesCZ.Add(eProperty.MythicalPieCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Piety)"));
            m_propertyNamesCZ.Add(eProperty.MythicalChaCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Charisma)"));
            m_propertyNamesCZ.Add(eProperty.MythicalEmpCapBonus, LanguageMgr.GetTranslation("CZ", "Mythical Stat Cap (Empathy)"));

            log.Info("CZ language: stat names loaded");
        }


        #endregion

        static SkillBase()
        {
            RegisterPropertyNamesEN();
            RegisterPropertyNamesFR();
            RegisterPropertyNamesDE();
            RegisterPropertyNamesIT();
            RegisterPropertyNamesES();
            InitArmorResists();
            InitPropertyTypes();
            InitializeObjectTypeToSpec();
            InitializeSpecToSkill();
            InitializeSpecToFocus();
            InitializeRaceResists();
        }


        //liste laden
        public static void LoadPropertyLanguage(string Language, string accountName, bool geladen)
        {
            if (Language != null && geladen == false)
            {
                //  RegisterPropertyNames(Language, accountName);
                geladen = true;
                log.WarnFormat("list {0} loaded!", Language);
            }
        }

        //Remove key






        public static void LoadSkills()
        {
            lock (m_loadingLock)
            {
                if (!m_loaded)
                {
                    LoadSpells();
                    LoadSpellLines();
                    LoadAbilities();
                    LoadClassRealmAbilities();
                    LoadProcs();
                    LoadSpecAbility();
                    LoadSpecializations();
                    LoadAbilityHandlers();
                    LoadSkillHandlers();
                    m_loaded = true;
                }
            }
        }

        public static void LoadSpells()
        {
            m_syncLockUpdates.EnterWriteLock();
            try
            {
                //load all spells
                if (log.IsInfoEnabled)
                    log.Info("Loading spells...");

                IList<DBSpell> spelldb = GameServer.Database.SelectAllObjects<DBSpell>();

                Dictionary<int, DBSpell> dictionary = new Dictionary<int, DBSpell>(spelldb.Count);
                m_dbSpells = dictionary;
                Dictionary<int, Spell> dictionary1 = new Dictionary<int, Spell>();
                m_spells = dictionary1;

                for (int i = 0; i < spelldb.Count; i++)
                {
                    DBSpell spell = spelldb[i];
                    m_dbSpells.Add(spell.SpellID, spell);
                    try
                    {
                        m_spells.Add(spell.SpellID, new Spell(spell, 1));
                    }
                    catch (Exception e)
                    {
                        log.Error(e.Message + " et spellid = " + spell.SpellID + " spell.TS= " + spell.ToString());
                    }
                }
                if (log.IsInfoEnabled)
                    log.Info("Spells loaded: " + spelldb.Count);


            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }

        public static void LoadSpellLines()
        {
            // load all spell lines
            IList<DBSpellLine> dbo = GameServer.Database.SelectAllObjects<DBSpellLine>();
            m_spellLists = new Dictionary<string, List<Spell>>();
            foreach ((DBSpellLine line, List<Spell> spell_list, IList<DBLineXSpell> dbo2) in from DBSpellLine line in dbo
                                                     let spell_list = new List<Spell>()//var dbo2 = GameServer.Database.SelectObjects<DBLineXSpell>("LineName = '" + GameServer.Database.Escape(line.KeyName) + "'");
                                                     let dbo2 = GameServer.Database.SelectObjects<DBLineXSpell>("`LineName` = @LineName", new QueryParameter("@LineName", line.KeyName))
                                                     select (line, spell_list, dbo2))
            {
                for (int i = 0; i < dbo2.Count; i++)
                {
                    DBLineXSpell lxs = dbo2[i];
                    if (!m_dbSpells.TryGetValue(lxs.SpellID, out DBSpell spell))
                    {
                        log.WarnFormat("Spell with ID {0} not found but is referenced from LineXSpell table", lxs.SpellID);
                        continue;
                    }
                    spell_list.Add(new Spell(spell, lxs.Level));
                }

                spell_list.Sort(delegate (Spell sp1, Spell sp2) { return sp1.Level.CompareTo(sp2.Level); });
                m_spellLists.Add(line.KeyName, spell_list);
                RegisterSpellLine(new SpellLine(line.KeyName, line.Name, line.Spec, line.IsBaseLine));
                if (log.IsDebugEnabled)
                    log.Debug("SpellLine: " + line.KeyName + ", " + dbo2.Count + " spells");
            }

            if (log.IsInfoEnabled)
                log.Info("Total spell lines loaded: " + dbo.Count);

        }

        /// <summary>
        /// Reload all the DB spells from the database. 
        /// Useful to load new spells added in preperation for ReloadSpellLine(linename) to update a spell line live
        /// We want to add any new spells in the DB to the global spell list, m_spells, but not remove any added by scripts
        /// </summary>
        public static void ReloadDBSpells()
        {
            // lock skillbase for writes
            m_syncLockUpdates.EnterWriteLock();
            try
            {
                //load all spells
                if (log.IsInfoEnabled)
                    log.Info("Reloading DB spells...");

                IList<DBSpell> spelldb = GameServer.Database.SelectAllObjects<DBSpell>();

                m_dbSpells = new Dictionary<int, DBSpell>(spelldb.Count);

                int count = 0;

                for (int i = 0; i < spelldb.Count; i++)
                {
                    DBSpell spell = spelldb[i];
                    m_dbSpells.Add(spell.SpellID, spell);

                    if (m_spells.ContainsKey(spell.SpellID) == false)
                    {
                        m_spells.Add(spell.SpellID, new Spell(spell, 1));
                        count++;
                    }
                }

                if (log.IsInfoEnabled)
                {
                    log.Info("Spells loaded from DB: " + spelldb.Count);
                    log.Info("Spells added to global spell list: " + count);
                }

            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }


        public static int ReloadSpellLine(string lineName)
        {
            int spellsLoaded = 0;

            // GameServer.Database.SelectObjects<DBGuild>("`KeyName` = @KeyName", new QueryParameter("@KeyName", GameServer.Database.Escape(lineName)));

            DBSpellLine line = GameServer.Database.SelectObjects<DBSpellLine>("`KeyName` = @KeyName", new QueryParameter("@KeyName", GameServer.Database.Escape(lineName))).FirstOrDefault();
            //GameServer.Database.SelectObject<DBSpellLine>("KeyName = '" + GameServer.Database.Escape(lineName) + "'");

            if (line != null)
            {
                RemoveSpellList(lineName);

                List<Spell> spell_list = new List<Spell>();

                // GameServer.Database.SelectObjects<DBLineXSpell>("`KeyName` = @KeyName", new QueryParameter("@KeyName", GameServer.Database.Escape(line.KeyName)))
                IList<DBLineXSpell> spells = GameServer.Database.SelectObjects<DBLineXSpell>("`LineName` = @LineName", new QueryParameter("@LineName", GameServer.Database.Escape(line.KeyName)));
                //GameServer.Database.SelectObjects<DBLineXSpell>("LineName = '" + GameServer.Database.Escape(line.KeyName) + "'");

                for (int i = 0; i < spells.Count; i++)
                {
                    DBLineXSpell lxs = spells[i];
                    if (!m_dbSpells.TryGetValue(lxs.SpellID, out DBSpell spell))
                    {
                        log.WarnFormat("Spell with ID {0} not found but is referenced from LineXSpell table", lxs.SpellID);
                        continue;
                    }
                    spell_list.Add(new Spell(spell, lxs.Level));
                }

                spell_list.Sort(comparison: delegate (Spell sp1, Spell sp2) { return sp1.Level.CompareTo(sp2.Level); });
                m_spellLists.Add(line.KeyName, spell_list);

                spellsLoaded = spells.Count;
            }

            return spellsLoaded;
        }

        private static void LoadAbilities()
        {
            // load Abilities
            log.Info("Loading Abilities...");

            IList<DBAbility> abilities = GameServer.Database.SelectAllObjects<DBAbility>();
            if (abilities != null)
            {
                for (int i = 0; i < abilities.Count; i++)
                {
                    DBAbility dba = abilities[i];
                    if (m_abilitiesByName.ContainsKey(dba.KeyName) == false)
                    {
                        m_abilitiesByName.Add(dba.KeyName, dba);
                    }

                    if (string.IsNullOrEmpty(dba.Implementation) == false && m_implementationTypeCache.ContainsKey(dba.Implementation) == false)
                    {
                        Type type = ScriptMgr.GetType(dba.Implementation);
                        if (type != null)
                        {
                            Type typeCheck = new Ability(dba).GetType();
                            if (type != typeCheck && type.IsSubclassOf(typeCheck))
                            {
                                m_implementationTypeCache.Add(dba.Implementation, type);
                            }
                            else
                            {
                                log.Warn("Ability implementation " + dba.Implementation + " is not derived from Ability. Cannot be used.");
                            }
                        }
                        else
                        {
                            log.Warn("Ability implementation " + dba.Implementation + " for ability " + dba.Name + " not found");
                        }
                    }
                }
            }
            if (log.IsInfoEnabled)
            {
                log.Info("Total abilities loaded: " + ((abilities != null) ? abilities.Count : 0));
            }
        }

        private static void LoadClassRealmAbilities()
        {
            log.Info("Loading class to realm ability associations...");
            IList<ClassXRealmAbility> classxra = GameServer.Database.SelectAllObjects<ClassXRealmAbility>();

            if (classxra != null)
            {
                for (int i = 0; i < classxra.Count; i++)
                {
                    ClassXRealmAbility cxra = classxra[i];
                    List<RealmAbility> raList;

                    if (!m_classRealmAbilities.ContainsKey(cxra.CharClass))
                    {
                        raList = new List<RealmAbility>();
                        m_classRealmAbilities[cxra.CharClass] = raList;
                    }
                    else
                    {
                        raList = m_classRealmAbilities[cxra.CharClass];
                    }

                    Ability ab = GetAbility(cxra.AbilityKey, 1);

                    if (ab.Name.StartsWith("?"))
                    {
                        log.Warn("Realm Ability " + cxra.AbilityKey + " assigned to class " + cxra.CharClass + " but does not exist");
                    }
                    else
                    {
                        if (ab is RealmAbility)
                        {
                            if (raList.Contains(ab as RealmAbility) == false)
                            {
                                raList.Add(ab as RealmAbility);
                            }
                        }
                        else
                        {
                            log.Warn(ab.Name + " is not a Realm Ability, this most likely is because no Implementation is set or an Implementation is set and is not a Realm Ability");
                        }
                    }
                }
            }

            log.Info("Realm Abilities assigned to classes!");
        }

        public static void LoadProcs()
        {
            //(procs) load all Procs
            if (log.IsInfoEnabled)
                log.Info("Loading procs...");

            m_styleSpells.Clear();

            IList<DBStyleXSpell> styleXSpells = GameServer.Database.SelectAllObjects<DBStyleXSpell>();
            if (styleXSpells != null)
            {
                for (int i = 0; i < styleXSpells.Count; i++)
                {
                    DBStyleXSpell proc = styleXSpells[i];
                    Dictionary<int, List<DBStyleXSpell>> styleClasses;
                    if (m_styleSpells.ContainsKey(proc.StyleID))
                    {
                        styleClasses = m_styleSpells[proc.StyleID];
                    }
                    else
                    {
                        Dictionary<int, List<DBStyleXSpell>> dictionary = new Dictionary<int, List<DBStyleXSpell>>();
                        styleClasses = dictionary;
                        m_styleSpells.Add(proc.StyleID, styleClasses);
                    }

                    List<DBStyleXSpell> classSpells;
                    if (styleClasses.ContainsKey(proc.ClassID))
                    {
                        classSpells = styleClasses[proc.ClassID];
                    }
                    else
                    {
                        classSpells = new List<DBStyleXSpell>();
                        styleClasses.Add(proc.ClassID, classSpells);
                    }

                    if (classSpells.Contains(proc) == false)
                    {
                        classSpells.Add(proc);
                    }
                }
            }

            if (log.IsInfoEnabled)
            {
                log.Info("Total procs loaded: " + ((styleXSpells != null) ? styleXSpells.Count : 0));
            }
        }

        public static void LoadSpecAbility()
        {
            // load Specialization & styles
            if (log.IsInfoEnabled)
                log.Info("Loading specialization & styles...");

            IList<DBSpecXAbility> specabilities = GameServer.Database.SelectAllObjects<DBSpecXAbility>();
            if (specabilities != null)
            {
                for (int i = 0; i < specabilities.Count; i++)
                {
                    DBSpecXAbility sxa = specabilities[i];
                    if (!m_specAbilities.TryGetValue(sxa.Spec, out List<Ability> list))
                    {
                        List<Ability> ability = new List<Ability>();
                        list = ability;
                        m_specAbilities.Add(sxa.Spec, list);
                    }

                    if (m_abilitiesByName.TryGetValue(sxa.AbilityKey, out DBAbility dba))
                    {
                        list.Add(new Ability(dba, sxa.AbilityLevel, sxa.Spec, sxa.SpecLevel));
                    }
                    else if (log.IsWarnEnabled)
                        log.Warn("Associated ability " + sxa.AbilityKey + " for specialization " + sxa.Spec + " not found!");
                }
            }
        }

        public static int LoadSpecializations()
        {
            IList<DBSpecialization> specs = GameServer.Database.SelectAllObjects<DBSpecialization>();
            if (specs != null)
            {
                m_specsByName.Clear();
                m_styleLists.Clear();
                m_stylesByIDClass.Clear();

                for (int i = 0; i < specs.Count; i++)
                {
                    DBSpecialization spec = specs[i];
                    if (spec.Styles != null)
                    {
                        foreach ((DBStyle specStyle, string hashKey) in from DBStyle specStyle in spec.Styles
                                                             let hashKey = string.Format("{0}|{1}", specStyle.SpecKeyName, specStyle.ClassId)
                                                             select (specStyle, hashKey))
                        {
                            if (!m_styleLists.TryGetValue(hashKey, out List<Style> styleList))
                            {
                                List<Style> list = new List<Style>();
                                styleList = list;
                                m_styleLists.Add(hashKey, styleList);
                            }

                            Style style = new Style(specStyle);
                            //(procs) Add procs to the style, 0 is used for normal style
                            if (m_styleSpells.ContainsKey(style.ID))
                            {
                                foreach (byte classID in
                                // now we add every proc to the style (even if ClassID != 0)
                                from byte classID in Enum.GetValues(typeof(eCharacterClass))
                                where m_styleSpells[style.ID].ContainsKey(classID)
                                select classID)
                                {
                                    style.Procs.AddRange(from DBStyleXSpell styleSpells in m_styleSpells[style.ID][classID]
                                                         select styleSpells);
                                }
                            }

                            styleList.Add(style);
                            KeyValuePair<int, int> styleKey = new KeyValuePair<int, int>(style.ID, specStyle.ClassId);
                            if (!m_stylesByIDClass.ContainsKey(styleKey))
                            {
                                m_stylesByIDClass.Add(styleKey, style);
                            }
                        }
                    }

                    RegisterSpec(new Specialization(spec.KeyName, spec.Name, spec.Icon));

                    int specAbCount = 0;
                    if (m_specAbilities.ContainsKey(spec.KeyName))
                    {
                        specAbCount = m_specAbilities[spec.KeyName].Count;
                    }

                    if (log.IsDebugEnabled)
                    {
                        int styleCount = 0;
                        if (spec.Styles != null)
                        {
                            styleCount = spec.Styles.Length;
                        }
                        log.Debug("Specialization: " + spec.Name + ", " + styleCount + " styles, " + specAbCount + " abilities");
                    }
                }

                // We've added all the styles to their respective lists.  Now lets go through and sort them by their level

                SortStylesByLevel();
            }
            if (log.IsInfoEnabled)
                log.Info("Total specializations loaded: " + ((specs != null) ? specs.Count : 0));

            return (specs != null) ? specs.Count : 0;
        }

        public static void SortStylesByLevel()
        {
            using (Dictionary<string, List<Style>>.Enumerator enumer = m_styleLists.GetEnumerator())
            {
                while (enumer.MoveNext())
                    enumer.Current.Value.Sort(delegate (Style style1, Style style2) { return style1.SpecLevelRequirement.CompareTo(style2.SpecLevelRequirement); });
            }
        }

        private static void LoadAbilityHandlers()
        {
            // load skill action handlers
            //Search for ability handlers in the gameserver first
            if (log.IsInfoEnabled)
                log.Info("Searching ability handlers in GameServer");
            Hashtable ht = ScriptMgr.FindAllAbilityActionHandler(Assembly.GetExecutingAssembly());
            foreach ((DictionaryEntry entry, string key) in from DictionaryEntry entry in ht
                                         let key = (string)entry.Key
                                         select (entry, key))
            {
                if (log.IsDebugEnabled)
                    log.Debug("\tFound ability handler for " + key);
                if (!m_abilityActionHandler.ContainsKey(key))
                    m_abilityActionHandler.Add(key, (Type)entry.Value);
                else if (log.IsWarnEnabled)
                    log.Warn("Duplicate type handler for: " + key);
            }

            //Now search ability handlers in the scripts directory and overwrite the ones
            //found from gameserver
            if (log.IsInfoEnabled)
                log.Info("Searching AbilityHandlers in Scripts");
            foreach (Assembly asm in ScriptMgr.Scripts)
            {
                ht = ScriptMgr.FindAllAbilityActionHandler(asm);
                foreach (DictionaryEntry entry in ht)
                {
                    string message;
                    string key = (string)entry.Key;

                    if (m_abilityActionHandler.ContainsKey(key))
                    {
                        message = "\tFound new ability handler for " + key;
                        m_abilityActionHandler[key] = (Type)entry.Value;
                    }
                    else
                    {
                        message = "\tFound ability handler for " + key;
                        m_abilityActionHandler.Add(key, (Type)entry.Value);
                    }

                    if (log.IsDebugEnabled)
                        log.Debug(message);
                }
            }
            if (log.IsInfoEnabled)
                log.Info("Total ability handlers loaded: " + m_abilityActionHandler.Keys.Count);
        }

        private static void LoadSkillHandlers()
        {
            //Search for skill handlers in gameserver first
            if (log.IsInfoEnabled)
                log.Info("Searching skill handlers in GameServer.");
            Hashtable ht = ScriptMgr.FindAllSpecActionHandler(Assembly.GetExecutingAssembly());
            foreach (var (entry, key) in from DictionaryEntry entry in ht
                                         let key = (string)entry.Key
                                         select (entry, key))
            {
                if (log.IsDebugEnabled)
                    log.Debug("\tFound skill handler for " + key);
                if (!m_specActionHandler.ContainsKey(key))
                    m_specActionHandler.Add(key, (Type)entry.Value);
                else if (log.IsWarnEnabled)
                    log.Warn("Duplicate type handler for: " + key);
            }
            //Now search skill handlers in the scripts directory and overwrite the ones
            //found from the gameserver
            if (log.IsInfoEnabled)
                log.Info("Searching skill handlers in Scripts.");
            foreach (Assembly asm in ScriptMgr.Scripts)
            {
                ht = ScriptMgr.FindAllSpecActionHandler(asm);
                foreach (DictionaryEntry entry in ht)
                {
                    string message;
                    string key = (string)entry.Key;

                    if (m_specActionHandler.ContainsKey(key))
                    {
                        message = "Found new skill handler for " + key;
                        m_specActionHandler[key] = (Type)entry.Value;
                    }
                    else
                    {
                        message = "Found skill handler for " + key;
                        m_specActionHandler.Add(key, (Type)entry.Value);
                    }

                    if (log.IsDebugEnabled)
                        log.Debug(message);
                }
            }
            if (log.IsInfoEnabled)
                log.Info("Total skill handlers loaded: " + m_specActionHandler.Keys.Count);
        }

        #region Armor resists

        // lookup table for armor resists
        private const int REALM_BITCOUNT = 2;
        private const int DAMAGETYPE_BITCOUNT = 4;
        private const int ARMORTYPE_BITCOUNT = 3;
        private static readonly int[] m_armorResists = new int[1 << (REALM_BITCOUNT + DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT)];

        /// <summary>
        /// Gets the natural armor resist to the give damage type
        /// </summary>
        /// <param name="armor"></param>
        /// <param name="damageType"></param>
        /// <returns>resist value</returns>
        public static int GetArmorResist(InventoryItem armor, eDamageType damageType, GameLiving target)
        {
            if (armor == null) return 0;
            int realm = target.Realm - eRealm._First;
            //log.ErrorFormat("armor realm = {0}", realm);
            int armorType = armor.Template.Object_Type - (int)eObjectType._FirstArmor;
            int damage = damageType - eDamageType._FirstResist;
            if (realm < 0 || realm > eRealm._LastPlayerRealm - eRealm._First) return 0;
            if (armorType < 0 || armorType > eObjectType._LastArmor - eObjectType._FirstArmor) return 0;
            if (damage < 0 || damage > eDamageType._LastResist - eDamageType._FirstResist) return 0;

            const int realmBits = DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT;

            return m_armorResists[(realm << realmBits) | (armorType << DAMAGETYPE_BITCOUNT) | damage];
        }

        private static void InitArmorResists()
        {
            const int resistant = 10;
            const int vulnerable = -5;

            // melee resists (slash, crush, thrust)

            // alb armor - neutral to slash
            // plate and leather resistant to thrust
            // chain and studded vulnerable to thrust
            WriteMeleeResists(eRealm.Albion, eObjectType.Leather, 0, vulnerable, resistant);
            WriteMeleeResists(eRealm.Albion, eObjectType.Plate, 0, vulnerable, resistant);
            WriteMeleeResists(eRealm.Albion, eObjectType.Studded, 0, resistant, vulnerable);
            WriteMeleeResists(eRealm.Albion, eObjectType.Chain, 0, resistant, vulnerable);


            // hib armor - neutral to thrust
            // reinforced and leather vulnerable to crush
            // scale resistant to crush
            WriteMeleeResists(eRealm.Hibernia, eObjectType.Leather, resistant, vulnerable, 0);
            WriteMeleeResists(eRealm.Hibernia, eObjectType.Reinforced, resistant, vulnerable, 0);
            WriteMeleeResists(eRealm.Hibernia, eObjectType.Scale, vulnerable, resistant, 0);


            // mid armor - neutral to crush
            // studded and leather resistant to thrust
            // chain vulnerabel to thrust
            WriteMeleeResists(eRealm.Midgard, eObjectType.Studded, vulnerable, 0, resistant);
            WriteMeleeResists(eRealm.Midgard, eObjectType.Leather, vulnerable, 0, resistant);
            WriteMeleeResists(eRealm.Midgard, eObjectType.Chain, resistant, 0, vulnerable);


            // magical damage (Heat, Cold, Matter, Energy)
            // Leather
            WriteMagicResists(eRealm.Albion, eObjectType.Leather, vulnerable, resistant, vulnerable, 0);
            WriteMagicResists(eRealm.Hibernia, eObjectType.Leather, vulnerable, resistant, vulnerable, 0);
            WriteMagicResists(eRealm.Midgard, eObjectType.Leather, vulnerable, resistant, vulnerable, 0);

            // Reinforced/Studded
            WriteMagicResists(eRealm.Albion, eObjectType.Studded, resistant, vulnerable, vulnerable, vulnerable);
            WriteMagicResists(eRealm.Hibernia, eObjectType.Reinforced, resistant, vulnerable, vulnerable, vulnerable);
            WriteMagicResists(eRealm.Midgard, eObjectType.Studded, resistant, vulnerable, vulnerable, vulnerable);

            // Chain
            WriteMagicResists(eRealm.Albion, eObjectType.Chain, resistant, 0, 0, vulnerable);
            WriteMagicResists(eRealm.Midgard, eObjectType.Chain, resistant, 0, 0, vulnerable);

            // Scale/Plate
            WriteMagicResists(eRealm.Albion, eObjectType.Plate, resistant, vulnerable, resistant, vulnerable);
            WriteMagicResists(eRealm.Hibernia, eObjectType.Scale, resistant, vulnerable, resistant, vulnerable);
        }

        private static void WriteMeleeResists(eRealm realm, eObjectType armorType, int slash, int crush, int thrust)
        {
            if (realm < eRealm._First || realm > eRealm._LastPlayerRealm)
            {
                throw new ArgumentOutOfRangeException("realm", realm, "Realm should be between _First and _LastPlayerRealm.");
            }

            if (armorType < eObjectType._FirstArmor || armorType > eObjectType._LastArmor)
            {
                throw new ArgumentOutOfRangeException("armorType", armorType, "Armor type should be between _FirstArmor and _LastArmor");
            }

            int off = (realm - eRealm._First) << (DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT);
            off |= (armorType - eObjectType._FirstArmor) << DAMAGETYPE_BITCOUNT;
            m_armorResists[off + (eDamageType.Slash - eDamageType._FirstResist)] = slash;
            m_armorResists[off + (eDamageType.Crush - eDamageType._FirstResist)] = crush;
            m_armorResists[off + (eDamageType.Thrust - eDamageType._FirstResist)] = thrust;
        }

        private static void WriteMagicResists(eRealm realm, eObjectType armorType, int heat, int cold, int matter, int energy)
        {
            if (realm < eRealm._First || realm > eRealm._LastPlayerRealm)
            {
                throw new ArgumentOutOfRangeException("realm", realm, "Realm should be between _First and _LastPlayerRealm.");
            }

            if (armorType < eObjectType._FirstArmor || armorType > eObjectType._LastArmor)
            {
                throw new ArgumentOutOfRangeException("armorType", armorType, "Armor type should be between _FirstArmor and _LastArmor");
            }

            int off = (realm - eRealm._First) << (DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT);
            off |= (armorType - eObjectType._FirstArmor) << DAMAGETYPE_BITCOUNT;
            m_armorResists[off + (eDamageType.Heat - eDamageType._FirstResist)] = heat;
            m_armorResists[off + (eDamageType.Cold - eDamageType._FirstResist)] = cold;
            m_armorResists[off + (eDamageType.Matter - eDamageType._FirstResist)] = matter;
            m_armorResists[off + (eDamageType.Energy - eDamageType._FirstResist)] = energy;
        }

        #endregion

        /// <summary>
        /// Check if property belongs to all of specified types
        /// </summary>
        /// <param name="prop">The property to check</param>
        /// <param name="type">The types to check</param>
        /// <returns>true if property belongs to all types</returns>
        public static bool CheckPropertyType(eProperty prop, ePropertyType type)
        {
            int property = (int)prop;
            return property < 0 || property >= m_propertyTypes.Length ? false : (m_propertyTypes[property] & type) == type;
        }

        /// <summary>
        /// Gets a new AbilityActionHandler instance associated with given KeyName
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static IAbilityActionHandler GetAbilityActionHandler(string keyName)
        {
            m_syncLockUpdates.EnterReadLock();
            bool exists;
            Type handlerType;
            try
            {
                exists = m_abilityActionHandler.TryGetValue(keyName, out handlerType);
            }
            finally
            {
                m_syncLockUpdates.ExitReadLock();
            }

            if (exists)
            {
                return GetNewAbilityActionHandler(handlerType);
            }

            return null;
        }

        /// <summary>
        /// Gets a new SpecActionHandler instance associated with given KeyName
        /// </summary>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public static ISpecActionHandler GetSpecActionHandler(string keyName)
        {
            m_syncLockUpdates.EnterReadLock();
            Type handlerType;
            bool exists;
            try
            {
                exists = m_specActionHandler.TryGetValue(keyName, out handlerType);
            }
            finally
            {
                m_syncLockUpdates.ExitReadLock();
            }

            if (exists)
            {
                try
                {
                    ISpecActionHandler newHndl = GetNewSpecActionHandler(handlerType);
                    return newHndl;
                }
                catch (Exception e)
                {
                    if (log.IsErrorEnabled)
                        log.ErrorFormat("Error while instanciating ISpecActionHandler {0} From Handler {2}: {1}", keyName, e, handlerType);
                }
            }

            return null;
        }

        public static void RegisterSpellLine(SpellLine line)
        {
            m_syncLockUpdates.EnterWriteLock();
            try
            {
                if (m_spellLinesByName.ContainsKey(line.KeyName))
                    m_spellLinesByName[line.KeyName] = line;
                else
                    m_spellLinesByName.Add(line.KeyName, line);
            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }
        /// <summary>
        /// Add a new style to a specialization.  If the specialization does not exist it will be created.
        /// After adding all styles call SortStyles to sort the list by level
        /// </summary>
        /// <param name="style"></param>
        public static void AddScriptedStyle(Specialization spec, DBStyle style)
        {
            string hashKey = string.Format("{0}|{1}", style.SpecKeyName, style.ClassId);

            m_syncLockUpdates.EnterWriteLock();
            try
            {
                if (!m_styleLists.TryGetValue(hashKey, out List<Style> styleList))
                {
                    styleList = new List<Style>();
                    m_styleLists.Add(hashKey, styleList);
                }

                Style st = new Style(style);

                //(procs) Add procs to the style, 0 is used for normal style
                if (m_styleSpells.ContainsKey(st.ID))
                {
                    st.Procs.AddRange(
                 // now we add every proc to the style (even if ClassID != 0)
                    from byte classID in Enum.GetValues(typeof(eCharacterClass))
                    where m_styleSpells[st.ID].ContainsKey(classID)
                    from DBStyleXSpell styleSpells in m_styleSpells[st.ID][classID]
                    select styleSpells);
                }
                styleList.Add(st);

                KeyValuePair<int, int> styleKey = new KeyValuePair<int, int>(st.ID, style.ClassId);
                if (!m_stylesByIDClass.ContainsKey(styleKey))
                {
                    m_stylesByIDClass.Add(styleKey, st);
                }

                if (!m_specsByName.ContainsKey(spec.KeyName))
                {
                    RegisterSpec(spec);
                }
            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }

        /// <summary>
        /// Check and add this spec to m_specsByName
        /// </summary>
        /// <param name="spec"></param>
        public static void RegisterSpec(Specialization spec)
        {
            m_syncLockUpdates.EnterWriteLock();
            try
            {
                Tuple<Type, string, ushort, int> entry = new Tuple<Type, string, ushort, int>(spec.GetType(), spec.Name, spec.Icon, spec.ID);

                if (m_specsByName.ContainsKey(spec.KeyName))
                {
                    m_specsByName[spec.KeyName] = spec;
                }
                else
                {
                    m_specsByName.Add(spec.KeyName, spec);
                }
            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }

        public static void UnRegisterSpellLine(string LineKeyName)
        {
            m_syncLockUpdates.EnterWriteLock();
            try
            {
                if (m_spellLinesByName.ContainsKey(LineKeyName))
                {
                    m_spellLinesByName.Remove(LineKeyName);
                }
            }
            finally
            {
                m_syncLockUpdates.ExitWriteLock();
            }
        }
        /// <summary>
        /// returns level 1 instantiated realm abilities, only for readonly use!
        /// </summary>
        /// <param name="classID"></param>
        /// <returns></returns>
        public static List<RealmAbility> GetClassRealmAbilities(int classID)
        {
            m_syncLockUpdates.EnterReadLock();
            try
            {
                if (m_classRealmAbilities.ContainsKey(classID))
                {
                    return m_classRealmAbilities[classID];
                }
                else
                    return new List<RealmAbility>();
            }
            finally
            {
                m_syncLockUpdates.ExitReadLock();
            }
        }

        public static Ability getClassRealmAbility(int charclass)
        {
            List<RealmAbility> abis = GetClassRealmAbilities(charclass);
            foreach (Ability ab in from Ability ab in abis
                               where ab is RR5RealmAbility
                               select ab)
            {
                return ab;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="keyname"></param>
        /// <returns></returns>
        public static Ability GetAbility(string keyname)
        {
            return GetAbility(keyname, 1);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="keyname"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public static Ability GetAbility(string keyname, int level)
        {
            if (m_abilitiesByName.ContainsKey(keyname))
            {
                DBAbility dba = m_abilitiesByName[keyname];

                Type type = null;
                if (dba.Implementation != null && m_implementationTypeCache.ContainsKey(dba.Implementation))
                {
                    type = m_implementationTypeCache[dba.Implementation];
                }
                else
                {
                    return new Ability(dba, level);
                }

                Ability ability = (Ability)Activator.CreateInstance(type, new object[] { dba, level });
                return ability;
            }

            if (log.IsWarnEnabled)
                log.Warn("Ability '" + keyname + "' unknown");

            Ability ability1 = new Ability(keyname, "?" + keyname, "", 0, 0);
            return ability1;
        }

        /// <summary>
        /// Return the spell line, creating a temporary one if not found
        /// </summary>
        /// <param name="keyname"></param>
        /// <returns></returns>
        public static SpellLine GetSpellLine(string keyname)
        {
            return GetSpellLine(keyname, true);
        }


        /// <summary>
        /// Return a spell line
        /// </summary>
        /// <param name="keyname">The key name of the line</param>
        /// <param name="create">Should we create a temp spell line if not found?</param>
        /// <returns></returns>
        public static SpellLine GetSpellLine(string keyname, bool create)
        {
            if (keyname == GlobalSpellsLines.Mob_Spells)
            {
                return new SpellLine("Mob Spells", "Mob Spells", "", true);
            }

            if (m_spellLinesByName.ContainsKey(keyname))
            {
                return m_spellLinesByName[keyname].Clone() as SpellLine;
            }

            if (create)
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn("Spell-Line " + keyname + " unknown, creating temporary line.");
                }

                SpellLine spellLine = new SpellLine(keyname, "?" + keyname, "", true);
                return spellLine;
            }

            return null;
        }

        /// <summary>
        /// Add an existing spell to a spell line, adding a new line if needed.
        /// The spell level is set based on the spell ID (why I do not know)
        /// Primarily used for Champion spells but can be used to make any custom spell list
        /// From spells already loaded from the DB
        /// </summary>
        /// <param name="spellLineID"></param>
        /// <param name="spellID"></param>
        public static void AddSpellToSpellLine(string spellLineID, int spellID)
        {

            if (!m_spellLists.TryGetValue(spellLineID, out List<Spell> spellList))
            {
                spellList = new List<Spell>();
                m_spellLists.Add(spellLineID, spellList);
            }

            Spell spellToAdd = GetSpellByID(spellID);

            if (spellToAdd == null)
            {
                log.Error("Missing Spell ID: " + spellID);
                return;
            }

            // Make a copy of the existing spell, making this a unique spell so we can set the level
            Spell spell = spellToAdd.Copy();

            spellList.Add(spell);
            // spellList.Sort(delegate(Spell sp1, Spell sp2) { return sp1.ID.CompareTo(sp2.ID); });

            for (int i = 0; i < spellList.Count; i++)
            {
                spellList[i].Level = i + 1;
            }
        }

        /// <summary>
        /// Remove all the spells from a spell line.
        /// </summary>
        /// <param name="spellLineID"></param>
        public static void ClearSpellLine(string spellLineID)
        {

            if (m_spellLists.TryGetValue(spellLineID, out List<Spell> spellList))
            {
                spellList.Clear();
            }
        }

        /// <summary>
        /// Remove a spell list
        /// </summary>
        /// <param name="spellLineID"></param>
        public static void RemoveSpellList(string spellLineID)
        {
            if (m_spellLists.ContainsKey(spellLineID))
            {
                m_spellLists.Remove(spellLineID);
            }
        }

        /// <summary>
        /// Add a spell to a spell line, adding a new line if needed.
        /// The spell level is set based on the order added to the list.
        /// </summary>
        /// <param name="spellLineID"></param>
        /// <param name="spellToAdd"></param>
        /// <returns></returns>
        public static bool AddSpellToSpellLine(string spellLineID, Spell spellToAdd)
        {
            if (spellToAdd == null)
            {
                return false;
            }

            List<Spell> list = null;

            if (m_spellLists.ContainsKey(spellLineID))
            {
                list = m_spellLists[spellLineID];
            }

            // Make a copy of the spell passed in, making this a unique spell so we can set the level
            Spell newspell = spellToAdd.Copy();

            if (list != null)
            {
                if (list.Count > 49)
                {
                    return false;  // can only have spells up to level 50
                }

                foreach (var _ in from Spell spell in list
                                  where spell.Name == spellToAdd.Name
                                  select new { })
                {
                    return false;// spell already in spellline
                }

                newspell.Level = list.Count + 1;
                list.Add(newspell);
            }
            else
            {
                list = new List<Spell>();
                newspell.Level = 1;
                list.Add(newspell);
                m_spellLists[spellLineID] = list;
            }

            return true;
        }


        /// <summary>
        /// Get a loaded specialization, warn if not found and create a dummy entry
        /// </summary>
        /// <param name="keyname"></param>
        /// <returns></returns>
        public static Specialization GetSpecialization(string keyname)
        {
            return GetSpecialization(keyname, true);
        }

        /// <summary>
        /// Get a specialization
        /// </summary>
        /// <param name="keyname"></param>
        /// <param name="create">if not found generate a warning and create a dummy entry</param>
        /// <returns></returns>
        public static Specialization GetSpecialization(string keyname, bool create)
        {
            if (m_specsByName.ContainsKey(keyname))
            {
                if (m_specsByName[keyname] is Specialization spec)
                {
                    return (Specialization)spec.Clone();
                }
            }

            if (create)
            {
                if (log.IsWarnEnabled)
                {
                    log.Warn("Specialization " + keyname + " unknown");
                }

                Specialization specialization = new Specialization(keyname, "?" + keyname, 0);
                return specialization;
            }

            return null;
        }

        /// <summary>
        /// return all styles for a specific specialization
        /// if no style are associated or spec is unknown the list will be empty
        /// </summary>
        /// <param name="specID">KeyName of spec</param>
        /// <param name="classId">ClassID for which style list is requested</param>
        /// <returns>list of styles, never null</returns>
        public static List<Style> GetStyleList(string specID, int classId)
        {
            if (m_styleLists.TryGetValue(specID + "|" + classId, out List<Style> list))
            {
                // Do *NOT* permit random write access to this list!
                // All access should be controlled and ensured to be thread-safe
                List<Style> list1 = new List<Style>(list);
                return list1;
            }
            else
            {
                List<Style> style = new List<Style>(0);
                return style;
            }
        }

        /// <summary>
        /// returns spec dependend abilities
        /// </summary>
        /// <param name="specID">KeyName of spec</param>
        /// <returns>list of abilities or empty list</returns>
        public static List<Ability> GetSpecAbilityList(string specID)
        {
            if (m_specAbilities.TryGetValue(specID, out List<Ability> list))
            {
                return list;
            }
            else
            {
                List<Ability> ability = new List<Ability>(0);
                return ability;
            }
        }

        /// <summary>
        /// return all spells for a specific spell-line
        /// if no spells are associated or spell-line is unknown the list will be empty
        /// </summary>
        /// <param name="spellLineID">KeyName of spell-line</param>
        /// <returns>list of spells, never null</returns>
        public static List<Spell> GetSpellList(string spellLineID)
        {
            if (m_spellLists.TryGetValue(spellLineID, out List<Spell> list))
            {
                return list;
            }
            else
            {
                List<Spell> spells = new List<Spell>(0);
                return spells;
            }
        }

        /// <summary>
        /// Find style with specific id and return a copy of it
        /// </summary>
        /// <param name="styleID">id of style</param>
        /// <param name="classId">ClassID for which style list is requested</param>
        /// <returns>style or null if not found</returns>
        public static Style GetStyleByID(int styleID, int classId)
        {
            KeyValuePair<int, int> styleKey = new KeyValuePair<int, int>(styleID, classId);
           
            if (m_stylesByIDClass.TryGetValue(styleKey, out Style style))
            {
                Style style1 = (Style)style.Clone();
                return style1;
            }

            return null;
        }


        /// <summary>
        /// Update or add a spell to the global spell list.  Useful for adding procs and charges to items without restarting server.
        /// This will not update a spell in a spell line.
        /// </summary>
        /// <param name="spellID"></param>
        /// <returns></returns>
        public static bool UpdateSpell(int spellID)
        {
            //DBSpell dbSpell = GameServer.Database.SelectObject<DBSpell>("SpellID = " + spellID);
            DBSpell dbSpell = GameServer.Database.SelectObjects<DBSpell>("`SpellID` = @SpellID", new QueryParameter("@SpellID", spellID)).FirstOrDefault();

            if (dbSpell != null)
            {
                lock (m_spells)
                {
                    Spell spell = new Spell(dbSpell, 1);
                    if (m_spells.ContainsKey(spellID))
                    {
                        m_spells.Remove(spellID);
                       
                        //LoadSpells();

                        m_spells.Add(spellID, spell);
                       // m_spells[spellID] = spell;

                        
                    }
                    else
                    {
                        m_spells.Add(spellID, spell);
                    }
                }
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns spell with id, level of spell is always 1
        /// </summary>
        /// <param name="spellID"></param>
        /// <returns></returns>
        public static Spell GetSpellByID(int spellID)
        {
            //Elcotek Spell listen bugfix
            lock (((ICollection)m_spells).SyncRoot)
            {
                if (m_spells.TryGetValue(spellID, out Spell spell))
                {
                    return spell;
                }

                return null;
            }
        }

        /// <summary>
        /// Will attempt to find either in the spell line given or in the list of all spells
        /// </summary>
        /// <param name="spellID"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static Spell FindSpell(int spellID, SpellLine line)
        {
            Spell spell = null;

            if (line != null)
            {
                List<Spell> spells = GetSpellList(line.KeyName);
                foreach (var lineSpell in from Spell lineSpell in spells
                                          where lineSpell.ID == spellID
                                          select lineSpell)
                {
                    spell = lineSpell;
                    break;
                }
            }

            if (spell == null)
            {
                spell = GetSpellByID(spellID);
            }

            return spell;
        }


        /// <summary>
        /// Get display name of property and set Language
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static string GetPropertyName(eProperty prop, string Language)
        {
            string name;

            if (Language != null)
            {
                switch (Language)
                {
                    case "DE":
                    case "de":
                        {
                            if (!m_propertyNamesDE.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                    case "FR":
                    case "fr":
                        {
                            if (!m_propertyNamesFR.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                    case "EN":
                    case "en":
                        {
                            if (!m_propertyNamesEN.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                    case "IT":
                    case "it":
                        {
                            if (!m_propertyNamesIT.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                    case "ES":
                    case "es":
                        {
                            if (!m_propertyNamesES.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                    case "CZ":
                    case "cz":
                        {
                            if (!m_propertyNamesCZ.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                    default:
                        {
                            if (!m_propertyNamesEN.TryGetValue(prop, out name))
                            {
                                name = "Property" + ((int)prop);
                            }
                            return name;
                        }
                }
            }
            // log.Warn("Language for stat names not found, load default: EN"); //load default EN if not found
            return "EN";
        }

        /// <summary>
        /// determine race-dependent base resist
        /// </summary>
        /// <param name="race">Value must be greater than 0</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetRaceResist(int race, eResist type)
        {
            if (race == 0)
                return 0;

            int resistValue = 0;

            m_raceResistLock.EnterReadLock();

            try
            {
                if (m_raceResists.ContainsKey(race))
                {
                    int resistIndex;

                    if (type == eResist.Natural)
                    {
                        resistIndex = 9;
                    }
                    else
                    {
                        resistIndex = (int)type - (int)eProperty.Resist_First;
                    }

                    if (resistIndex >= 0 && resistIndex < m_raceResists[race].Length)
                    {
                        resistValue = m_raceResists[race][resistIndex];
                    }
                    else
                    {
                        log.Warn("No resists defined for type:  " + type.ToString());
                    }
                }
                else
                {
                    log.Warn("No resists defined for race:  " + race);
                }
            }
            finally
            {
                m_raceResistLock.ExitReadLock();
            }

            return resistValue;
        }

        /// <summary>
        /// Convert object type to spec needed to use that object
        /// </summary>
        /// <param name="objectType">type of the object</param>
        /// <returns>spec names needed to use that object type</returns>
        public static string ObjectTypeToSpec(eObjectType objectType)
        {
            if (!m_objectTypeToSpec.TryGetValue(objectType, out string res))
            {
                if (log.IsWarnEnabled)
                    log.Warn("Not found spec for object type " + objectType);
            }

            return res;
        }

        /// <summary>
        /// Convert spec to skill property
        /// </summary>
        /// <param name="specKey"></param>
        /// <returns></returns>
        public static eProperty SpecToSkill(string specKey)
        {
            if (!m_specToSkill.TryGetValue(specKey, out eProperty res))
            {
                //if (log.IsWarnEnabled)
                //log.Warn("No skill property found for spec " + specKey);
                return eProperty.Undefined;
            }
            return res;
        }

        /// <summary>
        /// Convert spec to focus
        /// </summary>
        /// <param name="specKey"></param>
        /// <returns></returns>
        public static eProperty SpecToFocus(string specKey)
        {
            if (!m_specToFocus.TryGetValue(specKey, out eProperty res))
            {
                //if (log.IsWarnEnabled)
                //log.Warn("No skill property found for spec " + specKey);
                return eProperty.Undefined;
            }
            return res;
        }
        private static ISpecActionHandler GetNewSpecActionHandler(Type type)
        {
            ISpecActionHandler handl = null;

            try
            {
                handl = (ISpecActionHandler)type.Assembly.CreateInstance(type.FullName);
                return handl;
            }
            catch
            {
            }

            return handl;
        }
        private static IAbilityActionHandler GetNewAbilityActionHandler(Type type)
        {
            IAbilityActionHandler handl = null;

            try
            {
                handl = (IAbilityActionHandler)type.Assembly.CreateInstance(type.FullName);
                return handl;
            }
            catch
            {
            }

            return handl;
        }
    }
}

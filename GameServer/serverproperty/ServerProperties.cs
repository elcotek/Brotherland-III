
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
using log4net;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DOL.GS.ServerProperties
{
    /// <summary>
    /// The abstract ServerProperty class that also defines the
    /// static Init and Load methods for other properties that inherit
    /// </summary>
    public abstract class Properties
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Init the property
        /// </summary>
        static Properties()
        {
            Init(typeof(Properties));
        }

        // Properties: feel free to improve the SPs in the
        // categories below, or extend the category list.

        #region SYSTEM / DEBUG
        /// <summary>
        /// Enable Debug mode - used to alter some features during server startup to make debugging easier
        /// Can be changed while server is running but may require restart to enable all debug features
        /// </summary>
        [ServerProperty("system", "enable_debug", "Enable Debug mode? Used to alter some features during server startup to make debugging easier", false)]
        public static bool ENABLE_DEBUG;

        /// <summary>
        /// Enable Pathing Server - Used to enable/Disable Pathing on the server
        /// Can be changed while server is running but may require restart to enable that feature
        /// </summary>
        [ServerProperty("system", "enable_pathingserver", "Enable the Pathingserver ? Used to enable/Disable Pathing on the server", false)]
        public static bool Enable_PathingServer;

        /// <summary>
        /// Enable Debug mode - used to alter some features during server startup to make debugging easier
        /// Can be changed while server is running but may require restart to enable all debug features
        /// </summary>
        [ServerProperty("system", "enable_pathing_debug", "Enable Debug mode? Used to alter some features during server startup to make debugging easier", false)]
        public static bool ENABLE_Pathig_DEBUG;

        /// <summary>
        /// Enable Debug mode - used to alter some features during server startup to make debugging easier
        /// Can be changed while server is running but may require restart to enable all debug features
        /// </summary>
        [ServerProperty("system", "enable_pathing__visual_debug", "Enable Debug mode? Used to alter some features during server startup to make debugging easier", false)]
        public static bool ENABLE_Pathig_Visual_DEBUG;

        /// <summary>
        /// NoPlayer Countdown on/OFF - used disable the Coundown 
        /// Can be changed while server is running.
        /// </summary>
        [ServerProperty("system", "noplayer_countdown", "Enable No Player Countdown mode? Used to enable the Countdown", false)]
        public static bool NOPLAYER_COUNTDOWN;

        /// <summary>
        /// NoPlayer Countdown on/OFF - used disable the Coundown 
        /// Can be changed while server is running.
        /// </summary>
        [ServerProperty("system", "auto_save_database", "Enable Auto Save to the Database? ", false)]
        public static bool Auto_Save_Database;


        /// <summary>
        /// Enable Debug mode - used to alter some features during server startup to make debugging easier
        /// Can be changed while server is running but may require restart to enable all debug features
        /// </summary>
        [ServerProperty("system", "enable_debug2", "if (checker == source) == return ", false)]
        public static bool ENABLE_DEBUG2;

        /// <summary>
        /// Enable Debug mode - used to alter some features during server startup to make debugging easier
        /// Can be changed while server is running but may require restart to enable all debug features
        /// </summary>
        [ServerProperty("system", "enable_debug3", "if (source is GamePlayer && target is GameNPC ", false)]
        public static bool ENABLE_DEBUG3;
        /// <summary>
        /// Whether to use the sync timer utility or not
        /// </summary>
        [ServerProperty("system", "use_sync_timer", "Shall we use the sync timers utility?", true)]
        public static bool USE_SYNC_UTILITY;

        /// <summary>
        /// Ignore too long outcoming packet or not
        /// </summary>
        [ServerProperty("system", "ignore_too_long_outcoming_packet", "Shall we ignore too long outcoming packet ?", false)]
        public static bool IGNORE_TOO_LONG_OUTCOMING_PACKET;

        /// <summary>
        /// If the server should only accept connections from staff
        /// </summary>
        [ServerProperty("system", "staff_login", "Staff Login Only - Edit this to set weather you wish staff to be the only ones allowed to Log in values True,False", false)]
        public static bool STAFF_LOGIN;

        /// <summary>
        /// The minimum client version required to connect
        /// </summary>
        [ServerProperty("system", "client_version_min", "Minimum Client Version - Edit this to change which client version at the least have to be used: -1 = any, 1.80 = 180", -1)]
        public static int CLIENT_VERSION_MIN;

        /// <summary>
        /// What is the maximum client type allowed to connect
        /// </summary>
        [ServerProperty("system", "client_type_max", "What is the maximum client type allowed to connect", -1)]
        public static int CLIENT_TYPE_MAX;

        /// <summary>
        /// The maximum client version required to connect
        /// </summary>
        [ServerProperty("system", "client_version_max", "Maximum Client Version - Edit this to change which client version at the most have to be used: -1 = any, 1.80 = 180", -1)]
        public static int CLIENT_VERSION_MAX;

        /// <summary>
        /// Should the server load quests
        /// </summary>
        [ServerProperty("system", "load_quests", "Should the server load quests, values True,False", true)]
        public static bool LOAD_QUESTS;

        /// <summary>
		/// Should the server load Buff Tokens
		/// </summary>
		[ServerProperty("system", "load_buff_tokens", "Should the server load buff tokens (npc and items), values True,False", true)]
        public static bool LOAD_BUFF_TOKENS;

        /// <summary>
        /// Disable Bug Reports
        /// </summary>
        [ServerProperty("system", "disable_bug_reports", "Set to true to disable bug reporting, and false to enable bug reporting", true)]
        public static bool DISABLE_BUG_REPORTS;
        /// <summary>
        /// The max number of players on the server
        /// </summary>
        [ServerProperty("system", "max_players", "Max Players - Edit this to set the maximum players allowed to connect at the same time set 0 for unlimited", 0)]
        public static int MAX_PLAYERS;

        /// <summary>
        /// What class should the server use for players
        /// </summary>
        [ServerProperty("system", "player_class", "What class should the server use for players", "DOL.GS.GamePlayer")]
        public static string PLAYER_CLASS;

        /// <summary>
        /// A serialised list of RegionIDs that will load objects
        /// </summary>
        [ServerProperty("system", "debug_load_regions", "Serialized list of region IDs that will load objects, separated by semi-colon (leave this blank to load all regions normally)", "")]
        public static string DEBUG_LOAD_REGIONS;

        /// <summary>
        /// A serialised list of disabled expansion IDs
        /// </summary>
        [ServerProperty("system", "disabled_expansions", "Serialized list of disabled expansions IDs, expansion IDs are client type seperated by ;", "")]
        public static string DISABLED_EXPANSIONS;

        /// <summary>
        /// Server Language
        /// </summary>
        [ServerProperty("system", "server_language", "Language of your server. It can be EN, FR or DE.", "EN")]
        public static string SERV_LANGUAGE;

        /// <summary>
        /// allow_change_language
        /// </summary>
        [ServerProperty("system", "allow_change_language", "Should we allow clients to change their language ?", false)]
        public static bool ALLOW_CHANGE_LANGUAGE;

        /// <summary>
		/// Allow player to change their character face after creation through customizing ?
		/// </summary>
		[ServerProperty("system", "allow_customize_face_after_creation", "Allow player to change their character face after creation through customizing ?", false)]
        public static bool ALLOW_CUSTOMIZE_FACE_AFTER_CREATION;

        /// <summary>
        /// Allow player to change their character starting stats after creation through customizing ?
        /// </summary>
        [ServerProperty("system", "allow_customize_stats_after_creation", "Allow player to change their character starting stats after creation through customizing ?", false)]
        public static bool ALLOW_CUSTOMIZE_STATS_AFTER_CREATION;


        /// <summary>
        /// StatSave Interval
        /// </summary>
        [ServerProperty("system", "statsave_interval", "Interval between each DB Stats store in minutes. -1 for deactivated.", -1)]
        public static int STATSAVE_INTERVAL;

        /// <summary>
        /// Bug Report Email Addresses
        /// </summary>
        [ServerProperty("system", "bug_report_email_addresses", "set to the email addresses you want bug reports sent to (bug reports will only send if the user has set an email address for his account, multiple addresses seperate with ;", "")]
        public static string BUG_REPORT_EMAIL_ADDRESSES;

        /// <summary>
        /// Ban Hackers
        /// </summary>
        [ServerProperty("system", "log_rp_farming", "Should we log RP farmer, if set to true, log will be done in Social Window.", false)]
        public static bool LOG_RP_FARMING;

        /// <summary>
        /// Ban Hackers
        /// </summary>
        [ServerProperty("system", "ban_hackers", "Should we ban hackers, if set to true, bans will be done, if set to false, kicks will be done", false)]
        public static bool BAN_HACKERS;

        /// <summary>
        /// Ban Hackers
        /// </summary>
        [ServerProperty("system", "ban_hackers_for_porthack", "Should we ban hackers for Porthack ?", false)]
        public static bool BAN_HACKERS_For_Porthack;


        /// <summary>
        /// Is the database translated
        /// </summary>
        [ServerProperty("system", "db_language", "What language is the DB", "EN")]
        public static string DB_LANGUAGE;

        [ServerProperty("system", "statprint_frequency", "How often (milliseconds) should statistics be printed on the server console.", 30000)]
        public static int STATPRINT_FREQUENCY;

        /// <summary>
        /// Should users be able to create characters in all realms using the same account
        /// </summary>
        [ServerProperty("system", "allow_all_realms", "should we allow characters to be created on all realms using a single account", false)]
        public static bool ALLOW_ALL_REALMS;


        /// <summary>
        /// Realm Change Wait timer
        /// </summary>
        [ServerProperty("system", "login_other_realm_time", "Time in Minutes to Wait before players can login on other realms. set 0 to disable.", 30)]
        public static int LOGIN_OTHER_RELAM_TIME;

        /// <summary>
        /// Realm Change Wait timer
        /// </summary>
        [ServerProperty("system", "login_other_realm_switch_timer_Minutes", "For single accounts only. Time in Minutes to Wait before players can login RvR regions. set 0 to disable.", 30)]
        public static int LOGIN_OTHER_RELAM_SWITCH_TIMER_MINUTES;

        /// <summary>
        /// Realm Change Wait timer
        /// </summary>
        [ServerProperty("system", "login_other_realm_switch_timer_Hours", "For single accounts only. Time in Hours to Wait before players can login RvR regions. set 0 to disable.", 1)]
        public static int LOGIN_OTHER_RELAM_SWITCH_TIMER_HOURS;


        /// <summary>
        /// Realm Change Wait timer
        /// </summary>
        [ServerProperty("system", "realm_timer_reset_cost", "BPS Cost for the Realmtimer Reset command.", 1000)]
        public static int REALM_TIMER_RESET_COST;

        /// <summary>
        /// This will Allow/Disallow dual loggins
        /// </summary>
        [ServerProperty("system", "allow_dual_logins", "Disable to disallow players to connect with more than 1 account at a time.", true)]
        public static bool ALLOW_DUAL_LOGINS;

        /// <summary>
        /// This will Allow/Disallow dual loggins
        /// </summary>
        [ServerProperty("system", "world_timer_enable", "If the World timer Enabled for RvR Relic Doors?.", true)]
        public static bool World_Timer_Enabled;
                
        /// <summary>
        /// The emote delay
        /// </summary>
        [ServerProperty("system", "emote_delay", "The emote delay, default 3000ms before another emote.", 3000)]
        public static int EMOTE_DELAY;

        /// <summary>
        /// Command spam protection delay
        /// </summary>
        [ServerProperty("system", "command_spam_delay", "The spam delay, default 1500ms before the same command can be issued again.", 1500)]
        public static int COMMAND_SPAM_DELAY;

        /// <summary>
        /// This specifies the max number of inventory items to send in an update packet
        /// </summary>
        [ServerProperty("system", "max_items_per_packet", "Max number of inventory items sent per packet.", 30)]
        public static int MAX_ITEMS_PER_PACKET;

        /// <summary>
        /// Number of times speed hack detected before banning.  Must be multiples of 5 (20, 25, 30, etc)
        /// </summary>
        [ServerProperty("system", "speedhack_tolerance", "Number of times speed hack detected before banning.  Multiples of 5 (20, 25, 30, etc)", 20)]
        public static int SPEEDHACK_TOLERANCE;

        /// <summary>
        /// Turn on move detect
        /// </summary>
        [ServerProperty("system", "enable_movedetect", "Should the move detect code be enabled to kick possible movement hackers?", false)]
        public static bool ENABLE_MOVEDETECT;


        /// <summary>
        /// Coords per second tolerance before player is identified as a hacker?
        /// </summary>
        [ServerProperty("system", "hack_tolerance", "Hack Count tolerance before player is identified as a hacker?", 25)]
        public static int HACK_TOLERANCE;

        /// <summary>
        /// Coords per second tolerance before player is identified as a hacker?
        /// </summary>
        [ServerProperty("system", "hack_tolerance_time", "Hack Count tolerance before player is identified as a hacker?", 5)]
        public static int HACK_TOLERANCE_TIME;

        /// <summary>
        /// Coords per second tolerance before player is identified as a hacker?
        /// </summary>
        [ServerProperty("system", "cps_tolerance", "Coords per second tolerance before player is identified as a hacker?", 1000)]
        public static int CPS_TOLERANCE;

        /// <summary>
        /// Time tolerance before player is identified as move hacker
        /// </summary>
        [ServerProperty("system", "cps_time_tolerance", "Time tolerance for CPS before player is identified as a move hacker?", 200)]
        public static int CPS_TIME_TOLERANCE;

        /// <summary>
        /// Z distance tolerance before player is identified as a jump hacker
        /// </summary>
        [ServerProperty("system", "jump_tolerance", "Z distance tolerance before player is identified as a jump hacker?", 200)]
        public static int JUMP_TOLERANCE;

        /// <summary>
        /// Z distance tolerance before player is identified as a jump hacker
        /// </summary>
        [ServerProperty("system", "jump_tolerancex", "X distance tolerance before player is identified as a jump hacker?", 200)]
        public static int JUMP_TOLERANCEX;


        /// <summary>
        /// Display centered screen messages if a player enters an area.
        /// </summary>
        [ServerProperty("system", "display_area_enter_screen_desc", "Display centered screen messages if a player enters an area.", false)]
        public static bool DISPLAY_AREA_ENTER_SCREEN_DESC;

        /// <summary>
        /// Whether or not to enable the audit log
        /// </summary>
        [ServerProperty("system", "enable_audit_log", "Whether or not to enable the audit log", false)]
        public static bool ENABLE_AUDIT_LOG;

        /// <summary>
		/// Enable a periodic server shutdown. If you run your server into a batch loop, this performs a restart.
		/// </summary>
		[ServerProperty("system", "hours_uptime_between_shutdown", "Hours between a scheduled server shutdown (-1 = no scheduled restart)", -1)]
        public static int HOURS_UPTIME_BETWEEN_SHUTDOWN;


        #endregion

        #region LOGGING
        /// <summary>
        /// Turn on logging of player vs player kills
        /// </summary>
        [ServerProperty("system", "log_pvp_kills", "Turn on logging of pvp kills?", false)]
        public static bool LOG_PVP_KILLS;

        /// <summary>
        /// Log All GM commands
        /// </summary>
        [ServerProperty("system", "log_all_gm_commands", "Log all GM commands on the server", false)]
        public static bool LOG_ALL_GM_COMMANDS;

        /// <summary>
        /// Should the server Log trades
        /// </summary>
        [ServerProperty("system", "log_trades", "Should the server Log all trades a player makes, values True,False", false)]
        public static bool LOG_TRADES;

        /// <summary>
        /// Log Email Addresses
        /// </summary>
        [ServerProperty("system", "log_email_addresses", "set to the email addresses you want logs automatically emailed to, multiple addresses seperate with ;", "")]
        public static string LOG_EMAIL_ADDRESSES;

        /// <summary>
        /// Enable inventory logging (trade, loot, buy, sell, quests,...)
        /// </summary>
        [ServerProperty("log", "log_inventory", "Enable inventory logging (trade, loot, buy, sell, quests,...)", false)]
        public static bool LOG_INVENTORY;

        /// <summary>
        /// Enable trade logging in inventory log (log_inventory must be enabled)
        /// </summary>
        [ServerProperty("log", "log_inventory_trade", "Enable trade logging in inventory log (log_inventory must be enabled)", true)]
        public static bool LOG_INVENTORY_TRADE;

        /// <summary>
        /// Enable craft logging in inventory log (log_inventory must be enabled)
        /// </summary>
        [ServerProperty("log", "log_inventory_craft", "Enable craft logging in inventory log (log_inventory must be enabled)", true)]
        public static bool LOG_INVENTORY_CRAFT;

        /// <summary>
        /// Enable loot logging in inventory log (log_inventory must be enabled)
        /// </summary>
        [ServerProperty("log", "log_inventory_loot", "Enable loot logging in inventory log (log_inventory must be enabled)", true)]
        public static bool LOG_INVENTORY_LOOT;

        /// <summary>
        /// Enable quest logging in inventory log (log_inventory must be enabled)
        /// </summary>
        [ServerProperty("log", "log_inventory_quest", "Enable quest logging in inventory log (log_inventory must be enabled)", true)]
        public static bool LOG_INVENTORY_QUEST;

        /// <summary>
        /// Enable merchant logging in inventory log (log_inventory must be enabled)
        /// </summary>
        [ServerProperty("log", "log_inventory_merchant", "Enable merchant logging in inventory log (log_inventory must be enabled)", true)]
        public static bool LOG_INVENTORY_MERCHANT;

        /// <summary>
        /// Enable other logging in inventory log (log_inventory must be enabled)
        /// </summary>
        [ServerProperty("log", "log_inventory_other", "Enable other logging in inventory log (log_inventory must be enabled)", true)]
        public static bool LOG_INVENTORY_OTHER;
        #endregion

        #region SERVER


        /// <summary>
        /// Enable/Disable Caledonia Event
        /// </summary>
        [ServerProperty("server", "database_cleanup", "Sould the server cleanup, Accounts, Inventory and Itemunique dbs at start? (this will take a long time!!)", false)]
        public static bool Database_Cleanup;

        /// <summary>
		/// Enable/Disable Caledonia Event
		/// </summary>
		[ServerProperty("server", "enable_caledonia_event", "Enable/Disable Caledonia Event.", false)]
        public static bool Enable_Caledonia_Event;

        /// <summary>
        /// Anon Modifier
        /// </summary>
        [ServerProperty("server", "caledonia_Event_drop_chance", "Caledonia Event Drop chance in percent.", 15)]
        public static int Caledonia_Event_Drop_Chance;

        /// <summary>
		/// Enable/Disable Autokick Timer
		/// </summary>
		[ServerProperty("server", "player idle kick", "Enable auto kick for inactive players", false)]
        public static bool KICK_IDLE_PLAYER_STATUS;

        /// <summary>
		/// Enable integrated serverlistupdate script?
		/// </summary>
		[ServerProperty("server", "serverlistupdate_enabled", "Enable in-built serverlistupdate script?", false)]
        public static bool SERVERLISTUPDATE_ENABLED;

        /// <summary>
        /// Enable integrated serverlistupdate script?
        /// </summary>
        [ServerProperty("server", "serverphplistupdate_enabled", "Enable in-built serverstatscounterupdate script?", false)]
        public static bool SERVERPHPListUPDATE_ENABLED;

        /// <summary>
        /// The username for server list update.
        /// </summary>
        [ServerProperty("server", "serverlistupdate_user", "Username for serverlistupdate.", "")]
        public static string SERVER_LIST_UPDATE_USER;

        /// <summary>
        /// The password for server list update.
        /// </summary>
        [ServerProperty("server", "serverlistupdate_password", "Password for serverlistupdate.", "")]
        public static string SERVER_LIST_UPDATE_PASS;

        /// <summary>
        /// The money drop modifier
        /// </summary>
        [ServerProperty("server", "use_new_passives_ras_scaling", "Use new passives realmabilities scaling (1.108+) ?", false)]
        public static bool USE_NEW_PASSIVES_RAS_SCALING;

        /// <summary>
        /// Use pre 1.105 train or livelike
        /// </summary>
        [ServerProperty("server", "custom_train", "Train is custom pre-1.105 one ? (false set it to livelike 1.105+)", true)]
        public static bool CUSTOM_TRAIN;

        /// <summary>
        /// Record news in database
        /// </summary>
        [ServerProperty("server", "record_news", "Record News in database?", true)]
        public static bool RECORD_NEWS;

        /// <summary>
        /// The Server Message of the Day
        /// </summary>
        [ServerProperty("server", "motd", "The Server Message of the Day - Edit this to set what is displayed when a level 2+ character enters the game for the first time, set to \"\" for nothing", "Welcome to a Dawn of Light server, please edit this MOTD")]
        public static string MOTD;

        /// <summary>
        /// The message players get when they enter the game past level 1
        /// </summary>
        [ServerProperty("server", "starting_msg", "The Starting Mesage - Edit this to set what is displayed when a level 1 character enters the game for the first time, set to \"\" for nothing", "Welcome for your first time to a Dawn of Light server, please edit this Starter Message")]
        public static string STARTING_MSG;

        /// <summary>
		/// Update existing rows within the LanguageSystem table from language files.
		/// </summary>
		[ServerProperty("server", "update_existing_db_system_sentences_from_files", "Update existing rows within the LanguageSystem table from language files.", false)]
        public static bool UPDATE_EXISTING_DB_SYSTEM_SENTENCES_FROM_FILES;


        /// <summary>
        /// The broadcast type
        /// </summary>
        [ServerProperty("server", "broadcast_type", "Broadcast Type - Edit this to change what /b does, values 0 = disabled, 1 = area, 2 = visibility distance, 3 = zone, 4 = region, 5 = realm, 6 = server", 1)]
        public static int BROADCAST_TYPE;

        /// <summary>
        /// Anon Modifier
        /// </summary>
        [ServerProperty("server", "anon_modifier", "Various modifying options for anon, 0 = default, 1 = /who shows player but as ANON, -1 = disabled", 0)]
        public static int ANON_MODIFIER;

        /// <summary>
        /// Anon Modifier
        /// </summary>
        [ServerProperty("server", "anon_rvr_only", "Allow for RvR Regions ?", false)]
        public static bool Anon_RvR_Only;

        /// <summary>
        /// Should the server load the example scripts
        /// </summary>
        [ServerProperty("server", "load_examples", "Should the server load the example scripts", true)]
        public static bool LOAD_EXAMPLES;

        /// <summary>
        /// Use Custom Start Locations
        /// </summary>
        [ServerProperty("server", "use_custom_start_locations", "Set to true if you will use another script to set your start locations", false)]
        public static bool USE_CUSTOM_START_LOCATIONS;

        /// <summary>
        /// Death Messages All Realms
        /// </summary>
        [ServerProperty("server", "death_messages_all_realms", "Set to true if you want all realms to see other realms death and kill messages", false)]
        public static bool DEATH_MESSAGES_ALL_REALMS;

        /// <summary>
        /// Enable PvE Speed
        /// </summary>
        [ServerProperty("server", "enable_pve_speed", "Set to true if you wish to enable the extra 25% increase to speed when not in combat or an RvR zone", true)]
        public static bool ENABLE_PVE_SPEED;

        /// <summary>
        /// Enable Encumberance Speed loss
        /// </summary>
        [ServerProperty("server", "enable_encumberance_speed_loss", "Set to true if you wish to enable the encumberance speed loss", true)]
        public static bool ENABLE_ENCUMBERANCE_SPEED_LOSS;
        /// <summary>
        /// Under level 35 mount speed, live like = 135
        /// </summary>
        [ServerProperty("rates", "mount_under_level_35_speed", "What is the speed of player controlled mounts under level 35?", (short)135)]
        public static short MOUNT_UNDER_LEVEL_35_SPEED = 135;

        /// <summary>
        /// Over level 35 mount speed, live like = 145
        /// </summary>
        [ServerProperty("rates", "mount_over_level_35_speed", "What is the speed of player controlled mounts over level 35?", (short)145)]
        public static short MOUNT_OVER_LEVEL_35_SPEED = 145;

        /// <summary>
		/// Relic Bonus Modifier
		/// </summary>
		[ServerProperty("rates", "relic_owning_bonus", "Relic Owning Bonus in percent per relic (default 10%) in effect when owning enemy relic", (short)10)]
        public static short RELIC_OWNING_BONUS;

        /// <summary>
        /// Enable Relic Damage Bonus
        /// </summary>
        [ServerProperty("rates", "enable_relic_damage_bonus", "Set to true if you wish to enable the encumberance speed loss", false)]
        public static bool ENABLE_RELIC_DAMAGE_BONUS;


        /// <summary>
        /// Disable Instances
        /// </summary>
        [ServerProperty("server", "disable_instances", "Enable or disable instances on the server", false)]
        public static bool DISABLE_INSTANCES;

        /// <summary>
        /// Disable Instances
        /// </summary>
        [ServerProperty("server", "disable_taskdungeons", "Enable or disable task dungeons on the server", false)]
        public static bool DISABLE_TASKDUNGEONS;

        /// <summary>
        /// Save QuestItems into Database
        /// </summary>
        [ServerProperty("server", "save_quest_mobs_into_database", "set false if you don't want this", true)]
        public static bool SAVE_QUEST_MOBS_INTO_DATABASE;

        /// <summary>
        /// This specifies the max amount of people in one battlegroup.
        /// </summary>
        [ServerProperty("server", "battlegroup_max_member", "Max number of members allowed in a battlegroup.", 64)]
        public static int BATTLEGROUP_MAX_MEMBER;


        [ServerProperty("server", "battlegroup_loot", "Enable Lootgens For BattleGroups ?", false)]
        public static bool BATTLEGROUP_LOOT;

      
        /// <summary>
        ///  This specifies the max amount of people in one group.
        /// </summary>
        [ServerProperty("server", "group_max_member", "Max number of members allowed in a group.", 8)]
        public static int GROUP_MAX_MEMBER;


        /// <summary>
        ///  This specifies the max amount of people in one group.
        /// </summary>
        [ServerProperty("server", "group_max_member_bg", "Max number of members allowed in a group in BG Thidranki.", 8)]
        public static int GROUP_MAX_MEMBER_BG;

        /// <summary>
        /// Sets the disabled commands for the server split by ;
        /// </summary>
        [ServerProperty("server", "disabled_commands", "Serialized list of disabled commands separated by semi-colon, example /realm;/toon;/quit", "")]
        public static string DISABLED_COMMANDS;

        /// <summary>
        /// Disable Appeal System
        /// </summary>
        [ServerProperty("server", "disable_appeal_system", "Disable the /Appeal System", false)]
        public static bool DISABLE_APPEALSYSTEM;

        /// <summary>
        /// Use Database Language datas instead of files (if empty = build the table from files)
        /// </summary>
        [ServerProperty("server", "use_dblanguage", "Use Database Language datas instead of files (if empty = build the table from files)", false)]
        public static bool USE_DBLANGUAGE;

        /// <summary>
        /// Set the maximum number of objects allowed in a region.  Smaller numbers offer better performance.  This is used to allocate arrays for both Regions and GamePlayers
        /// </summary>
        [ServerProperty("server", "region_max_objects", "Set the maximum number of objects allowed in a region.  Smaller numbers offer better performance.  This can't be changed while the server is running. (256 - 65535)", (ushort)30000)]
        public static ushort REGION_MAX_OBJECTS;

        /// <summary>
        /// Show logins
        /// </summary>
        [ServerProperty("server", "show_logins", "Show login messages when players log in and out of game?", true)]
        public static bool SHOW_LOGINS;

        /// <summary>
        /// Show logins channel
        /// </summary>
        [ServerProperty("server", "show_logins_channel", "What channel should be used for login messages? See eChatType, default is System.", (byte)0)]
        public static byte SHOW_LOGINS_CHANNEL;

        #endregion

        #region WORLD

        /// <summary>
        /// Maximum length for reward quest description text to prevent client crashes
        /// </summary>
        [ServerProperty("system", "max_rewardquest_description_length", "Maximum length for reward quest description text to prevent client crashes.", 1000)]
        public static int MAX_REWARDQUEST_DESCRIPTION_LENGTH;

        /// <summary>
        /// Epic encounters strength: 100 is 100% base strength
        /// </summary>
        [ServerProperty("world", "set_difficulty_on_epic_encounters", "Tune encounters taggued <Epic Encounter>. 0 means auto adaptative, others values are % of the initial difficulty (100%=initial difficulty)", 100)]
        public static int SET_DIFFICULTY_ON_EPIC_ENCOUNTERS;


        /// <summary>
        /// Artifact low level encounters strength: 100 is 100% base strength
        /// </summary>
        [ServerProperty("world", "set_difficulty_on_lowlevel_artifact_encounters", "Tune encounters taggued <Artifact low level Encounter>. 0 means auto adaptative, others values are % of the initial difficulty (100%=initial difficulty)", 1.0)]
        public static double SET_DIFFICULTY_ON_LOWLEVEL_ARTIFACT_ENCOUNTERS;

        /// <summary>
        /// Artifact hight level encounters strength: 100 is 100% base strength
        /// </summary>
        [ServerProperty("world", "set_difficulty_on_artifact_encounters", "Tune encounters taggued <Artifact hight level Encounter>. 0 means auto adaptative, others values are % of the initial difficulty (100%=initial difficulty)", 1.5)]
        public static double SET_DIFFICULTY_ON_ARTIFACT_ENCOUNTERS;

        /// <summary>
        /// A serialised list of disabled RegionIDs
        /// </summary>
        [ServerProperty("world", "disabled_regions", "Serialized list of disabled region IDs, separated by semi-colon or a range with a dash (ie 1-5;7;9)", "")]
        public static string DISABLED_REGIONS;

        /// <summary>
        /// Should the server disable the tutorial zone
        /// </summary>
        [ServerProperty("world", "disable_tutorial", "should the server disable the tutorial zone", false)]
        public static bool DISABLE_TUTORIAL;

        /// <summary>
        /// Should the server enable the TOA ablititys on Amsnan, Warrior, Hero, and Warden ?
        /// </summary>
        [ServerProperty("world", "enable_toa_abilitys", "Should the server enable the TOA ablititys on Amsnan, Warrior, Hero, and Warden ?", false)]
        public static bool ENABLE_TOA_ABILITYS;

        /// <summary>
        /// Should the server enable the TOA ablititys on Amsnan, Warrior, Hero, and Warden ?
        /// </summary>
        [ServerProperty("world", "enable_warden_shield_ability", "Should the server enable the shield ability of Warden ?", false)]
        public static bool ENABLE_WARDEN_SIELD_ABILITY;

        /// <summary>
        /// Should the server disable the tutorial zone
        /// </summary>
        [ServerProperty("world", "disable_toa_teleporter", "should the server disable the tutorial zone", false)]
        public static bool DISABLE_TOA_TELEPORTER;


        /// <summary>
        /// Should the server disable the tutorial zone
        /// </summary>
        [ServerProperty("world", "buffbots_only_rvr", "Allow Buffbots only in RVR Regions ?", false)]
        public static bool BUFFBOTS_ONLY_RVR;


        [ServerProperty("world", "world_item_decay_time", "How long (milliseconds) will an item dropped on the ground stay in the world.", (uint)180000)]
        public static uint WORLD_ITEM_DECAY_TIME;

        [ServerProperty("world", "world_pickup_distance", "How far before you can no longer pick up an object (loot for example).", 256)]
        public static int WORLD_PICKUP_DISTANCE;

        [ServerProperty("world", "world_day_increment", "Day Increment (0 to 512, default is 24).  Larger increments make shorter days.", (uint)24)]
        public static uint WORLD_DAY_INCREMENT;

        [ServerProperty("world", "world_npc_update_interval", "How often (milliseconds) will npc's broadcast updates to the clients. Minimum allowed = 1000 (1 second).", (uint)30000)]
        public static uint WORLD_NPC_UPDATE_INTERVAL;

        [ServerProperty("world", "world_playertoplayer_update_interval", "How often (milliseconds) will other players packet be broadcasted again to the clients. Minimum allowed = 1000 (1 seconds). 0 will disable this update.", (uint)1000)]
        public static uint WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL;

        [ServerProperty("world", "world_object_update_interval", "How often (milliseconds) will objects (static, housing, doors, broadcast updates to the clients. Minimum allowed = 10000 (10 seconds). 0 will disable this update.", (uint)30000)]
        public static uint WORLD_OBJECT_UPDATE_INTERVAL;

        [ServerProperty("world", "world_player_update_interval", "How often (milliseconds) will players be checked for updates. Minimum allowed = 100 (100 milliseconds).", (uint)300)]
        public static uint WORLD_PLAYER_UPDATE_INTERVAL;

        [ServerProperty("world", "weather_check_interval", "How often (milliseconds) will weather be checked for a chance to start a storm.", 5 * 60 * 1000)]
        public static int WEATHER_CHECK_INTERVAL;

        [ServerProperty("world", "weather_chance", "What is the chance of starting a storm.", 5)]
        public static int WEATHER_CHANCE;

        [ServerProperty("world", "weather_log_events", "Should weather events be shown in the Log (and on the console).", true)]
        public static bool WEATHER_LOG_EVENTS;

        /// <summary>
        /// Perform checklos on client with each mob
        /// </summary>
        [ServerProperty("world", "always_check_los", "Perform a LoS check before aggroing. This can involve a huge lag, handle with care!", false)]
        public static bool ALWAYS_CHECK_LOS;

        /// <summary>
        /// Perform checklos on client with each mob
        /// </summary>
        [ServerProperty("world", "check_los_during_cast", "Perform a LOS check during a spell cast.", true)]
        public static bool CHECK_LOS_DURING_CAST;

        /// <summary>
        /// Perform LOS check between controlled NPC's and players
        /// </summary>
        [ServerProperty("world", "always_check_pet_los", "Should we perform LOS checks between controlled NPC's and players?", false)]
        public static bool ALWAYS_CHECK_PET_LOS;

        /// <summary>
        /// LOS check frequency; how often are we allowed to check LOS on the same player (seconds)
        /// </summary>
        [ServerProperty("world", "los_player_check_frequency", "How often are we allowed to check LOS on the same player (seconds)", (ushort)5)]
        public static ushort LOS_PLAYER_CHECK_FREQUENCY;

        /// <summary>
        /// LOS check frequency; how often are we allowed to check LOS on the same player (seconds)
        /// </summary>
        [ServerProperty("world", "los_npc_follow_check_frequency", "How often are we allowed to check LOS on the npc (mili sec seconds)", (long)15000)]
        public static long LOS_NPC_FOLLOW_CHECK_FREQUENCY;


        /// <summary>
        /// HPs gained per champion's level
        /// </summary>
        [ServerProperty("world", "hps_per_championlevel", "The amount of extra HPs gained each time you reach a new Champion's Level", 40)]
        public static int HPS_PER_CHAMPIONLEVEL;

        /// <summary>
        /// Time player must wait after failed task
        /// </summary>
        [ServerProperty("world", "task_pause_ticks", "Time player must wait after failed task check to get new chance for a task, in milliseconds", 5 * 60 * 1000)]
        public static int TASK_PAUSE_TICKS;

        /// <summary>
        /// Should we handle tasks with items
        /// </summary>
        [ServerProperty("world", "task_give_random_item", "Task is also rewarded with ROG ?", false)]
        public static bool TASK_GIVE_RANDOM_ITEM;

        /// <summary>
        /// Should we enable Zone Bonuses?
        /// </summary>
        [ServerProperty("world", "enable_zone_bonuses", "Are Zone Bonuses Enabled?", false)]
        public static bool ENABLE_ZONE_BONUSES;

        /// <summary>
        /// List of ZoneId where personnal mount is allowed
        /// </summary>
        [ServerProperty("world", "allow_personnal_mount_in_regions", "CSV Regions where player mount is allowed", "")]
        public static string ALLOW_PERSONNAL_MOUNT_IN_REGIONS;

        /// <summary>
        /// Immunity Timer length when a player logs into game or zones into a new region, in seconds
        /// </summary>
        [ServerProperty("world", "timer_player_init", "Immunity Timer length when a player logs into game or zones into a new region, in seconds", 15)]
        public static int TIMER_PLAYER_INIT;

        /// <summary>
        /// Display the zonepoint with a choosen model
        /// </summary>
        [ServerProperty("world", "zonepoint_npctemplate", "Display the zonepoint with the following npctemplate. 0 for no display", 0)]
        public static int ZONEPOINT_NPCTEMPLATE;


        /// <summary>
        /// Line of Sight Manager Enable
        /// </summary>
        [ServerProperty("world", "losmgr_enable", "Enable the Line of Sight Manager where overriden methods are implemented.", false)]
        public static bool LOSMGR_ENABLE;

        /// <summary>
        /// Line of Sight Manager Debug Level
        /// </summary>
        [ServerProperty("world", "losmgr_debug_level", "Set the Level of Debug for the Line of Sight (LoS) Manager (do not set level 3 in production), 0 = no debug, 1 = info, 2 = warn, 3 = debug.", 0)]
        public static int LOSMGR_DEBUG_LEVEL;

        /// <summary>
        /// Line of Sight Manager Cleanup Frequency
        /// </summary>
        [ServerProperty("world", "losmgr_cleanup_frequency", "How fast should the Line of Sight (LoS) Manager clean up its data (in milliseconds), don't get under 30000 ms, raise cleanup count if you put high number here.", 120000)]
        public static int LOSMGR_CLEANUP_FREQUENCY;

        /// <summary>
        /// Line of Sight Manager number of entries to Cleanup
        /// </summary>
        [ServerProperty("world", "losmgr_cleanup_entries", "Number of Entries cleaned from Line of Sight (LoS) Manager each Cleanup ticks, if you need to go above 10 000 entries consider using a shorter cleanup frequency.", 1000)]
        public static int LOSMGR_CLEANUP_ENTRIES;

        /// <summary>
        /// Line of Sight Manager Query Timeout
        /// </summary>
        [ServerProperty("world", "losmgr_query_timeout", "Timeout (in milliseconds) until Line of Sight (LoS) Manager tries to resend a LoS check, 0 to disable (don't get under 100ms except for Local Network).", 300)]
        public static int LOSMGR_QUERY_TIMEOUT;

        /// <summary>
        /// Line of Sight Manager PvP Cache Timeout
        /// </summary>
        [ServerProperty("world", "losmgr_player_vs_player_cache_timeout", "Set Timeout (in milliseconds) for PvP Line of Sight (LoS) Manager cache data, 0 to disable.", 200)]
        public static int LOSMGR_PLAYER_VS_PLAYER_CACHE_TIMEOUT;

        /// <summary>
        /// Line of Sight Manager PvE Cache Timeout
        /// </summary>
        [ServerProperty("world", "losmgr_player_vs_environment_cache_timeout", "Set Timeout (in milliseconds) for PvE Line of Sight (LoS) Manager cache data, 0 to disable. (Should be slightly above Brain Think timer to have any effect)", 1500)]
        public static int LOSMGR_PLAYER_VS_ENVIRONMENT_CACHE_TIMEOUT;

        /// <summary>
        /// Line of Sight Manager EvE Cache Timeout
        /// </summary>
        [ServerProperty("world", "losmgr_environment_vs_environment_cache_timeout", "Set Timeout (in milliseconds) for EvE Line of Sight (LoS) Manager cache data, 0 to disable. (Should be at least 2 times PvE cache to lower LoS Queries)", 5000)]
        public static int LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_CACHE_TIMEOUT;

        /// <summary>
        /// Line of Sight Manager Player Check Frequency
        /// </summary>
        [ServerProperty("world", "losmgr_player_check_frequency", "Line of Sight (LoS) Manager will try to reduce player queries to this frequency (in millisecond), raise this if player are experiencing lags.", 100)]
        public static int LOSMGR_PLAYER_CHECK_FREQUENCY;

        /// <summary>
        /// Line of Sight Manager PvP threshold
        /// </summary>
        [ServerProperty("world", "losmgr_player_vs_player_range_threshold", "Line of Sight (LoS) Manager won't check LoS for players within this range. (Should be low to prevent abuses)", 32)]
        public static int LOSMGR_PLAYER_VS_PLAYER_RANGE_THRESHOLD;

        /// <summary>
        /// Line of Sight Manager PvE threshold
        /// </summary>
        [ServerProperty("world", "losmgr_player_vs_environment_range_threshold", "Line of Sight (LoS) Manager won't check LoS for mobs and NPC within this range. (Shouldn't be under default to prevent LoS flood)", 125)]
        public static int LOSMGR_PLAYER_VS_ENVIRONMENT_RANGE_THRESHOLD;

        /// <summary>
        /// Line of Sight Manager EvE threshold
        /// </summary>
        [ServerProperty("world", "losmgr_environment__vs_environment_range_threshold", "Line of Sight (LoS) Manager won't check LoS for Mobs vs Mobs within this range. (Shouldn't be under PvE Threshold)", 350)]
        public static int LOSMGR_ENVIRONMENT_VS_ENVIRONMENT_RANGE_THRESHOLD;

        /// <summary>
        /// Line of Sight Manager EvE Contamination
        /// </summary>
        [ServerProperty("world", "losmgr_max_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Mobs/NPCs in this range of the checker if target is a Mob, and highest value used for contamination LoS updates.", 350)]
        public static int LOSMGR_MAX_CONTAMINATION_RADIUS;

        /// <summary>
        /// Line of Sight Manager PvE Contamination
        /// </summary>
        [ServerProperty("world", "losmgr_npc_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Mobs/NPCs in this range of the checker, and highest value used for Player contamination LoS updates.", 250)]
        public static int LOSMGR_NPC_CONTAMINATION_RADIUS;

        /// <summary>
        /// Line of Sight Manager Pets Contamination
        /// </summary>
        [ServerProperty("world", "losmgr_pet_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Player's Pet in this range of the checker, keep value low to prevent abuses.", 50)]
        public static int LOSMGR_PET_CONTAMINATION_RADIUS;

        /// <summary>
        /// Line of Sight Manager Player Contamination
        /// </summary>
        [ServerProperty("world", "losmgr_players_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Players in this range of the checker, should be disabled to prevent PvP abuses.", 0)]
        public static int LOSMGR_PLAYER_CONTAMINATION_RADIUS;

        /// <summary>
        /// Line of Sight Manager Guards Contamination
        /// </summary>
        [ServerProperty("world", "losmgr_guard_contamination_radius", "Line of Sight (LoS) Manager will update LoS of Keep Guards in this range of the checker, raising this value will pack more Guards when Aggroing.", 200)]
        public static int LOSMGR_GUARD_CONTAMINATION_RADIUS;

        /// <summary>
        /// Line of Sight Manager Contamination Z-Factor in range checks
        /// </summary>
        [ServerProperty("world", "losmgr_contamination_zfactor", "Line of Sight (LoS) Manager Contamination will use this to lower or raise the Z checks when updating LoS checks. 0 = Z must be exact, 1 = Z range is radius.", 0.5)]
        public static double LOSMGR_CONTAMINATION_ZFACTOR;

        #endregion

        #region RATES
        /// <summary>
        /// The Poison Damage Rate
        /// </summary>
        /// <summary>
        /// Items sell ratio
        /// </summary>
        [ServerProperty("rates", "Camouflage_max_range", "Camouflage max Enemy Detect Range Default is 10", 10)]
        public static int CAMOUFLAGE_MAX_RANGE;

        [ServerProperty("rates", "hidden_range", "Camouflage max Enemy DetectHidden Range Default is 2", 2)]
        public static int HIDDEN_RANGE;


        [ServerProperty("rates", "Camouflage_hidden_range", "Camouflage max Enemy DetectHidden Range Default is 2", 2)]
        public static int CAMOUFLAGE_HIDDEN_RANGE;

        [ServerProperty("rates", "Stealther_Mana_Stat", "Maximum Poison-Damage for Infiltrators. Default is 150%", 1)]
        public static int STEALTHER_MANA_STAT;


        [ServerProperty("rates", "global_poison", "Maximum Global Poison-Damage (no for Stealthers). Default is 200%", 0.57)]
        public static double GLOBAL_POISON;

        [ServerProperty("rates", "global_poison_NPC", "Maximum Global Poison-Damage for NPCs (no for Stealthers). Default is 200%", 1.57)]
        public static double GLOBAL_POISON_NPC;

        /// <summary>
        /// Xp Cap for a player.  Given in percent of level.  Default is 125%
        /// </summary>
        [ServerProperty("rates", "XP_Cap_Percent", "Maximum XP a player can earn given in percent of their level. Default is 125%", 125)]
        public static int XP_CAP_PERCENT;

        /// <summary>
        /// Xp Cap for a player in a group.  Given in percent of level.  Default is 125%
        /// </summary>
        [ServerProperty("rates", "XP_Group_Cap_Percent", "Maximum XP a player can earn while in a group, given in percent of their level. Default is 125%", 125)]
        public static int XP_GROUP_CAP_PERCENT;

        /// <summary>
        /// Xp Cap for a player vs player kill.  Given in percent of level.  Default is 125%
        /// </summary>
        [ServerProperty("rates", "XP_PVP_Cap_Percent", "Maximum XP a player can earn killing another player, given in percent of their level. Default is 125%", 125)]
        public static int XP_PVP_CAP_PERCENT;

        /// <summary>
        /// Hardcap XP a player can earn after all other adjustments are applied.  Given in percent of level, default is 500%  There is no live value that corresponds to this cap.
        /// </summary>
        [ServerProperty("rates", "XP_HardCap_Percent", "Hardcap XP a player can earn after all other adjustments are applied. Given in percent of their level. Default is 500%", 500)]
        public static int XP_HARDCAP_PERCENT;

        /// <summary>
        /// The Experience Rate
        /// </summary>
        [ServerProperty("rates", "group_xp_rate", "The Group Experience Points Rate Modifier - Gruppenbonus - Gesammt-XP / 3 ist standard ", 3)]
        public static int GROUP_XP_RATE;

        /// <summary>
        /// The Experience Rate
        /// </summary>
        [ServerProperty("rates", "xp_rate", "The Experience Points Rate Modifier - Edit this to change the rate at which you gain experience points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double XP_RATE;

        /// <summary>
        /// The CL Experience Rate
        /// </summary>
        [ServerProperty("rates", "cl_xp_rate", "The Champion Level Experience Points Rate Modifier - Edit this to change the rate at which you gain CL experience points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double CL_XP_RATE;

        /// <summary>
        /// Low XP Rate
        /// </summary>
        [ServerProperty("rate", "low_xp_rates", "low XP Rates for highter levels? ", true)]
        public static bool LOW_XP_RATES;

        /// <summary>
        /// Low XP Rate
        /// </summary>
        [ServerProperty("rate", "low_xp_rates_group_Level_difference", "low XP Rates for lower group members if the difference highter than 5 level ? ", false)]
        public static bool LOW_XP_RATES_Group_Level_Difference;



        /// <summary>
        /// Player Maxlevel
        /// </summary>
        [ServerProperty("rate", "player_maxlevel", "Player maxlevel? ", 50)]
        public static int Player_MaxLevel;

        /// <summary>
        /// RvR Zones XP Rate
        /// </summary>
        [ServerProperty("rates", "rvr_zones_xp_rate", "The RvR zones Experience Points Rate Modifier", 1.0)]
        public static double RvR_XP_RATE;

        /// <summary>
        /// The Realm Points Rate
        /// </summary>
        [ServerProperty("rates", "realm_rang5_cap", "Realm rang cap 5 ? ", false)]
        public static bool REALM_RANG5_CAP;

        /// <summary>
        /// The Realm Points Rate
        /// </summary>
        [ServerProperty("rates", "relic_rp_rate", "the RP rate for carriing relics on relic pat", 3000)]
        public static int RELIC_RP_RATE;

        /// <summary>
        /// The Realm Points Rate
        /// </summary>
        [ServerProperty("rates", "relic_bp_rate", "the PB rate for carriing relics on relic pat", 50)]
        public static int RELIC_BP_RATE;

        /// <summary>
        /// The Realm Points Rate
        /// </summary>
        [ServerProperty("rates", "rp_rate", "The Realm Points Rate Modifier - Edit this to change the rate at which you gain realm points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double RP_RATE;

        /// <summary>
        /// The Realm Points Rate
        /// </summary>
        [ServerProperty("rates", "low_rp_rate", "The Low Realm Points Rate Modifier - Edit this to change the rate at which you gain realm points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double LOW_RP_RATE;

        /// <summary>
        /// The Realm Points Rate
        /// </summary>
        [ServerProperty("rates", "low_rp_rate_rr6", "The Low Realm Points Rate Modifier - Edit this to change the rate at which you gain realm points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double LOW_RP_RATE_RR6;

        /// <summary>
        /// The Bounty Points Rate
        /// </summary>
        [ServerProperty("rates", "bp_rate", "The Bounty Points Rate Modifier - Edit this to change the rate at which you gain bounty points e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double BP_RATE;


        /// <summary>
        /// Modifier used to adjust damage for pets on keep components
        /// </summary>
        [ServerProperty("pet", "pet_ml9_strength", "Modifier used to adjust damage for pets classes.", 350)]
        public static int PET_ML9_STRENGTH;


        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage_siege_charm", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.1)]
        public static double PVE_MOB_MELEE_DAMAGE_Siege_CHARM;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage_charm", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MOB_MELEE_DAMAGE_CHARM;


        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_mob_melee_damage_tocharm", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MOB_MELEE_DAMAGE_TOCHARM;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage_turret_tank_pets", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MELEE_DAMAGE_TURRET_TANK_PETS;


        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage_pets", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MELEE_DAMAGE_PETS;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_theurgist_melee_damage_erdpets", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_Theurgist_MELEE_DAMAGE_ErdPets;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage_to_topets", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MOB_MELEE_DAMAGE_TOPETS;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage_to_tonecropets", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MOB_MELEE_DAMAGE_TONECROPETS;

        [ServerProperty("server", "pve_pick_attacker_only", "Should the Mob Pick the Attacker directly or Group based ?", false)]
        public static bool PVE_MOB_Pick_Attacker_Only;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_mob_melee_damage", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MOB_MELEE_DAMAGE;

        /// <summary>
        /// The damage players do against monsters with melee
        /// </summary>
        [ServerProperty("rates", "pve_melee_damage", "The PvE Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_MELEE_DAMAGE;

        /// <summary>
        /// The damage players do against monsters with spells
        /// </summary>
        [ServerProperty("rates", "pve_spell_damage", "The PvE Spell Damage Modifier - Edit this to change the amount of spell damage done when fighting mobs e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVE_SPELL_DAMAGE;



        /// <summary>
        /// The percent per con difference (-1 = blue, 0 = yellow, 1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE.  0 is none, 5 is 5% per con, etc.  Default is 10%
        /// </summary>
        [ServerProperty("rates", "pve_spell_conhitpercent", "The percent per con (1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE  Must be >= 0.  0 is none, 5 is 5% per level, etc.  Default is 10%", (uint)10)]
        public static uint PVE_SPELL_CONHITPERCENT;

        /// <summary>
        /// The percent per con difference (-1 = blue, 0 = yellow, 1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVE.  0 is none, 5 is 5% per con, etc.  Default is 10%
        /// </summary>
        [ServerProperty("rates", "pvp_spell_conhitpercent", "The percent per con (1 = OJ, 2 = red ...) subtracted to hitchance for spells in PVP  Must be >= 0.  0 is none, 5 is 5% per level, etc.  Default is 12%", (uint)12)]
        public static uint PVP_SPELL_CONHITPERCENT;

        /// <summary>
        /// The damage players do against monsters with spells
        /// </summary>
        [ServerProperty("spells", "heal_powercost_multiplicator", "Heal powercost multiplicator standard 1.6 ", 1.6)]
        public static double HEAL_POWERCOST_MULTIPLICATOR;

        /// <summary>
        /// The damage players do against monsters with spells
        /// </summary>
        [ServerProperty("spells", "heal_powercost_AOE_multiplicator", "Heal powercost multiplicator standard 1.6 ", 1.6)]
        public static double HEAL_POWERCOST_AOE_MULTIPLICATOR;

        [ServerProperty("spells", "spell_charm_named_check", "Prevents charm spell to work on Named Mobs, 0 = disable, 1 = enable", 1)]
        public static int SPELL_CHARM_NAMED_CHECK;

        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "celerity_melee_damage", "Celerity damage multiplicator, plus damage zur je schneller die waffe  ", 0.001)]
        public static double CELERITY_MELEE_DAMAGE;


        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "celerity_melee_damage_2h", "Celerity damage Multiplicator, plus damage je schneller die zweihand waffe  ", 0.001)]
        public static double CELERITY_MELEE_DAMAGE_2H;


        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "celerity_melee_dps", "Celerity damage multiplicator, plus damage zur je schneller die waffe  ", 0.001)]
        public static double CELERITY_MELEE_DPS;


        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "heretic_ae_tick_damage", "Heretic damage per tick AE ", 0.15)]
        public static double HERETIC_AE_TICK_DAMAGE;
        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "heretic_single_tick_damage", "Heretic damage per tick Single ", 0.05)]
        public static double HERETIC_SINGLE_TICK_DAMAGE;

        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "pvp_melee_damage", "The PvP Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVP_MELEE_DAMAGE;

        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "pvp_melee_damage_pets", "The PvP Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVP_MELEE_DAMAGE_PETS;

        /// <summary>
        /// The damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "pvp_theurgist_melee_damage_erdpets", "The PvP Melee Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVP_Theurgist_MELEE_DAMAGE_ErdPets;


        /// <summary>
        /// The damage players do against players with Turrets
        /// </summary>
        [ServerProperty("rates", "spell_turret_damage", "The PvP Turret Spell Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.1)]
        public static double SPELL_TURRET_DAMAGE;

        /// <summary>
        /// The damage players do against players with WL Bolts
        /// </summary>
        [ServerProperty("rates", "spell_wlbolt_damage", "The PvP WL Bolt Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.1)]
        public static double SPELL_WLBOLT_DAMAGE;

        /// <summary>
        /// The damage players do against players with WL Bolts
        /// </summary>
        [ServerProperty("rates", "spell_bolt_damage", "The PvP WL Bolt Damage Modifier - Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.1)]
        public static double SPELL_BOLT_DAMAGE;

        /// <summary>
        /// The onhand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "archery_damage", "The PvP Archery Damage Modifier Standard is (0.001)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double ARCHERY_DAMAGE;

        /// <summary>
        /// The Dual Wield lefthand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "dual_lefthand_damage", "The Dual Wield Melee Damage Modifier Standard is (1.1)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.006)]
        public static double Dual_Lefthand_Damage;

        /// <summary>
        /// The Dual Wield lefthand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "pvp_dual_lefthand_damage", "The Dual Wield Melee Damage Modifier Standard is (1.1)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.006)]
        public static double PvP_Dual_Lefthand_Damage;


        // <summary>
        /// The Dual Wield lefthand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "dual_leftaxe_damage", "The Dual Wield Melee Damage Modifier Standard is (1.1)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.013)]
        public static double Dual_LeftAxe_Damage;
        // <summary>
        /// The Dual Wield lefthand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "dual_axe_sword_shadowblade_damage", "The Dual Wield Melee Damage Modifier Standard is (0.034)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.034)]
        public static double Dual_Axe_Sword_Shadowblade_Damage;



        /// <summary>
        /// The Dual Wield lefthand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "dual_lefthand_damage_alb", "The Dual Wield Melee Damage Modifier Standard is (1.1)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.017)]
        public static double Dual_Lefthand_Damage_ALB;

        /// <summary>
        /// The Dual Wield lefthand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "pvp_dual_lefthand_damage_alb", "The Dual Wield Melee Damage Modifier Standard is (1.1)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 0.0)]
        public static double PvP_Dual_Lefthand_Damage_ALB;

        /// <summary>
        /// The onhand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "celtic_dual_damage", "The PvP celtic Melee Damage Modifier, Standard is (60.0)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 30.0)]
        public static double CELTIC_DUAL_DAMAGE;

        /// <summary>
        /// The onhand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "cd_mercenary_damage", "The PvP LA Melee Damage Modifier for Berserkers, Standard is (2.5)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double CD_Mercenary_DAMAGE;


        /// <summary>
        /// The onhand damage players do against players with melee
        /// </summary>
        [ServerProperty("rates", "la_bersi_damage", "The PvP LA Melee Damage Modifier for Berserkers, Standard is (2.5)- Edit this to change the amount of melee damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double LA_BERSI_DAMAGE;


        /// <summary>
        /// The Bow Base Critchance for new Archery
        /// </summary>
        [ServerProperty("rates", "bow_base_critchance", "The Bows crit base Chance percent. ", 10)]
        public static int BOW_BASE_CRITCHANCE;

        
        /// <summary>
        /// The damage players do against players with spells
        /// </summary>
        [ServerProperty("rates", "pvp_spell_damage", "The PvP Spell Damage Modifier - Edit this to change the amount of spell damage done when fighting players e.g 1.5 is 50% more damage 2.0 is twice the damage (100%) 0.5 is half the damage (50%)", 1.0)]
        public static double PVP_SPELL_DAMAGE;

        /// <summary>
        /// The highest possible Block Rate against an Enemy (Hard Cap)
        /// </summary>
        [ServerProperty("rates", "block_cap", "Block Rate Cap Modifier - Edit this to change the highest possible block rate against an enemy (Hard Cap) in game e.g .60 = 60%", 0.60)]
        public static double BLOCK_CAP;

        /// <summary>
        /// The highest possible Block Rate decreasing vs Dualwield
        /// </summary>
        [ServerProperty("rates", "block_dw_decreasing", "Block Rate Cap Modifier - Edit this to change the highest possible block rate against an enemy (Hard Cap) in game e.g .60 = 60%", 0.25)]
        public static double BLOCK_DW_DECREASING;


        /// <summary>
        /// The highest possible Supporter Block Rate against an Enemy (Hard Cap)
        /// </summary>
        [ServerProperty("rates", "block_cap_sup", "Block Rate Cap Modifier - Edit this to change the highest possible block rate against an enemy (Hard Cap) in game e.g .60 = 60%", 0.50)]
        public static double BLOCK_CAP_SUP;

        /// <summary>
        /// The highest possible GamePet Block Rate against an Enemy (Hard Cap)
        /// </summary>
        [ServerProperty("rates", "block_cap_pet", "Block Rate Cap Modifier - Edit this to change the highest possible block rate against an enemy (Hard Cap) in game e.g .60 = 60%", 0.50)]
        public static double BLOCK_CAP_PET;

        ///<summary>
        /// The highest possible Evade Rate against an Enemy (Hard Cap)
        /// </summary>
        [ServerProperty("rates", "evade_cap", "Evade Rate Cap Modifier - Edit this to change the highest possible evade rate against an enemy (Hard Cap) in game e.g .50 = 50%", 0.50)]
        public static double EVADE_CAP;

        ///<summary>
        ///The highest possible Parry Rate against an Enemy (Hard Cap)
        /// </summary>
        [ServerProperty("rates", "parry_cap", "Parry Rate Cap Modifier - Edit this to change the highest possible parry rate against an enemy (Hard Cap) in game e.g .50 = 50%", 0.50)]
        public static double PARRY_CAP;

        /// <summary>
        /// Critical strike opening style effectiveness for twohaned.  Increase this to make CS styles BS, BSII and Perf Artery more effective
        /// </summary>
        [ServerProperty("rates", "cs_opening_effectiveness", "Critical strike opening style effectiveness.  Increase this to make CS styles BS, BSII and Perf Artery more effective", 1.0)]
        public static double CS_OPENING_EFFECTIVENESS;


        /// <summary>
        /// Critical strike opening style effectiveness for dual.  Increase this to make CS styles BS, BSII and Perf Artery more effective
        /// </summary>
        [ServerProperty("rates", "cs_opening_effectiveness_dual", "Critical strike opening style effectiveness.  Increase this to make CS styles BS, BSII and Perf Artery more effective", 2.4)]
        public static double CS_OPENING_EFFECTIVENESS_DUAL;

        /// <summary>
        /// Critical strike opening style effectiveness for dual.  Increase this to make CS styles BS, BSII and Perf Artery more effective
        /// </summary>
        [ServerProperty("rates", "unstyled_damage", "Unsyled damage multiplicator", 4.5)]
        public static double UNSTYLED_DAMAGE;

        /// <summary>
        /// The money drop modifier
        /// </summary>
        [ServerProperty("rates", "money_drop", "Money Drop Modifier - Edit this to change the amount of money which is dropped e.g 1.5 is 50% more 2.0 is twice the amount (100%) 0.5 is half the amount (50%)", 1.0)]
        public static double MONEY_DROP;

        /// <summary>
		/// The small chest drop chance
		/// </summary>
		[ServerProperty("rates", "base_smallchest_chance", "Percentage chance that a mob will drop a small chest of money", 10)]
        public static int BASE_SMALLCHEST_CHANCE;

        /// <summary>
        /// The math multiplier for small chest value
        /// </summary>
        [ServerProperty("rates", "smallchest_multiplier", "The math multiplier for small chest value. Increase for more gold", 10)]
        public static int SMALLCHEST_MULTIPLIER;

        /// <summary>
        /// The large chest drop chance
        /// </summary>
        [ServerProperty("rates", "base_largechest_chance", "Percentage chance that a mob will drop a large chest of money", 5)]
        public static int BASE_LARGECHEST_CHANCE;

        /// <summary>
        /// The math multiplier for small chest value
        /// </summary>
        [ServerProperty("rates", "largechest_multiplier", "The math multiplier for large chest value. Increase for more gold", 17)]
        public static int LARGECHEST_MULTIPLIER;

        /// <summary>
        /// The time until a player is worth rps again after death
        /// </summary>
        [ServerProperty("rates", "rp_worth_seconds", "Realm Points Worth Seconds - Edit this to change how many seconds until a player is worth RPs again after being killed ", 300)]
        public static int RP_WORTH_SECONDS;

        /// <summary>
        /// Health Regen Rate
        /// </summary>
        [ServerProperty("rates", "health_regen_rate", "Health regen rate", 1.0)]
        public static double HEALTH_REGEN_RATE;

        /// <summary>
        /// Rates for heal rps % ex default is 10% 
        /// heal * 10 / 100 = rps 
        /// </summary>
        [ServerProperty("rates", "heal_rps_rate", "What should the % be for healing rps?", 10)]
        public static int RP_HEAL_PERCENT;

        /// <summary>
        /// The % value of gainrps when heal a players recently damaged in rvr.
        /// </summary>
        [ServerProperty("rates", "heal_pvp_damage_value_rp", "How many % of heal final value is obtained in rps?", 8)]
        public static int HEAL_PVP_DAMAGE_VALUE_RP;

        /// <summary>
        /// <summary>
        /// Health Regen Rate
        /// </summary>
        [ServerProperty("rates", "endurance_regen_rate", "Endurance regen rate", 1.0)]
        public static double ENDURANCE_REGEN_RATE;

        /// <summary>
        /// Health Regen Rate
        /// </summary>
        [ServerProperty("rates", "mana_regen_rate", "Mana regen rate", 1.0)]
        public static double MANA_REGEN_RATE;

        /// <summary>
        /// Items sell ratio
        /// </summary>
        [ServerProperty("rates", "item_sell_ratio", "Merchants are buying items at the % of initial value", 50)]
        public static int ITEM_SELL_RATIO;

        /// <summary>
        /// Chance for condition loss on weapons and armor
        /// </summary>
        [ServerProperty("rates", "item_condition_loss_chance", "What chance does armor or weapon have to lose condition?", 5)]
        public static int ITEM_CONDITION_LOSS_CHANCE;


        #endregion

        #region CRAFTING

        [ServerProperty("crafting", "calculate_craft_sell_price", "Do we allow new price calculation for cafted items ?", true)]
        public static bool CALCULATE_CRAFT_SELL_PRICE;

        #endregion

        #region NPCs

        /// <summary>
        /// Base Value Aggro increase Range.
        /// </summary>
        [ServerProperty("npc", "mob_petowner_base_aggro", "Base Aggro for pet owners. ", 0.25)]
        public static double MOB_petowner_base_Aggrolevel;


        /// <summary>
        /// Base Value Aggro increase Range.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_canseeall_outsideofdungeons", "seeall, outside dungeons for aggro increase?. ", true)]
        public static bool MOB_Basevalue_Aggro_Range_SeeAll_OutSideOfDungeons;


        /// <summary>
        /// Base Value Aggro increase Range.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_canseeall_insideofdungeons", "seeall, inside dungeons for aggro increase?. ", true)]
        public static bool MOB_Basevalue_Aggro_Range_SeeAll_InsideOfDungeons;


        /// <summary>
        /// Base Value Aggro increase Range.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_checkaggro", "Base Value mob aggro range increasingbase. ", 2500)]
        public static int MOB_Basevalue_Aggro_Range_CheckAggro;


        /// <summary>
        /// Base Value Aggro increase Range.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_increase", "Base Value mob aggro range increasingbase. ", 2000)]
        public static int MOB_Basevalue_Aggro_Range_IncreaseBase;


        /// <summary>
        /// Base Value Aggro range increase SpeedBuff.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_increase_speedbuff", "Base Value mob aggro range multiplicator. ", 2)]
        public static int MOB_Basevalue_Aggro_Range_Increase_SpeedBuff;

        /// <summary>
        /// Base Value Aggro range increase SpeedBuff.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_increase_speedbuffendurance", "Base Value mob aggro range multiplicator. ", 3)]
        public static int MOB_Basevalue_Aggro_Range_Increase_SpeedBuffEndurance;


        /// <summary>
        /// Base Value Aggro range increase Sprint.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_range_increase_sprint", "Base Value mob aggro range multiplicator. ", 2)]
        public static int MOB_Basevalue_Aggro_Range_Increase_Sprint;


        /// <summary>
        /// Base Value Aggro increase SpeedBuff.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_level_increase_speedbuff", "Base Value mob aggro range multiplicator. ", 85)]
        public static int MOB_Basevalue_Aggro_Level_Increase_SpeedBuff;


        /// <summary>
        /// Base Value Aggro increase SpeedBuff.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_level_increase_speedbuffendurance", "Base Value mob aggro range multiplicator. ", 90)]
        public static int MOB_Basevalue_Aggro_Level_Increase_SpeedBuffEndurance;


        /// <summary>
        /// Base Value Aggro increase SpeedBuff.
        /// </summary>
        [ServerProperty("npc", "mob_basevalue_aggro_level_increase_sprint", "Base Value mob aggro range multiplicator. ", 75)]
        public static int MOB_Basevalue_Aggro_Level_Increase_Sprint;


        /// <summary>
        /// Base Value to use when auto-setting STR stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_str_base", "Base Value to use when auto-setting STR stat. ", 30.0)]
        public static double MOB_AUTOSET_STR_BASE;

        /// <summary>
        /// Multiplier to use when auto-setting STR stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_str_multiplier", "Multiplier to use when auto-setting STR stat. ", 1.0)]
        public static double MOB_AUTOSET_STR_MULTIPLIER;

        /// <summary>
        /// Multiplier to use when auto-setting STR stat.
        /// </summary>
        [ServerProperty("npc", "astral_autoset_str_multiplier", "Multiplier to use when auto-setting STR stat. ", 8)]
        public static int ASTRAL_AUTOSET_STR_MULTIPLIER;

        /// <summary>
        /// Base Value to use when auto-setting CON stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_con_base", "Base Value to use when auto-setting CON stat. ", 30.0)]
        public static double MOB_AUTOSET_CON_BASE;

        /// <summary>
        /// Multiplier to use when auto-setting CON stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_con_multiplier", "Multiplier to use when auto-setting CON stat. ", 1.0)]
        public static double MOB_AUTOSET_CON_MULTIPLIER;

        /// <summary>
        /// Base Value to use when auto-setting QUI stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_qui_base", "Base Value to use when auto-setting qui stat. ", 30.0)]
        public static double MOB_AUTOSET_QUI_BASE;

        /// <summary>
        /// Multiplier to use when auto-setting QUI stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_qui_multiplier", "Multiplier to use when auto-setting QUI stat. ", 1.0)]
        public static double MOB_AUTOSET_QUI_MULTIPLIER;

        /// <summary>
        /// Multiplier to use when auto-setting INT stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_int_multiplier", "Multiplier to use when auto-setting QUI stat. ", 1.0)]
        public static double MOB_AUTOSET_INT_MULTIPLIER;



        /// <summary>
        /// Base Value to use when auto-setting DEX stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_dex_base", "Base Value to use when auto-setting DEX stat. ", 30.0)]
        public static double MOB_AUTOSET_DEX_BASE;

        /// <summary>
        /// Base Value to use when auto-setting INT stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_int_base", "Base Value to use when auto-setting DEX stat. ", 30.0)]
        public static double MOB_AUTOSET_INT_BASE;




        /// <summary>
        /// Multiplier to use when auto-setting DEX stat.
        /// </summary>
        [ServerProperty("npc", "mob_autoset_dex_multiplier", "Multiplier to use when auto-setting DEX stat. ", 1.0)]
        public static double MOB_AUTOSET_DEX_MULTIPLIER;

        /// <summary>
        /// Base Value to use when auto-setting pet STR stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_str_base", "Base Value to use when auto-setting Pet STR stat. ", 20.0)]
        public static double PET_AUTOSET_STR_BASE;

        /// <summary>
        /// Multiplier to use when auto-setting pet STR stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_str_multiplier", "Multiplier to use when auto-setting Pet STR stat. ", 6.0)]
        public static double PET_AUTOSET_STR_MULTIPLIER;

        /// </summary>
        /// Base Value to use when auto-setting pet CON stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_con_base", "Base Value to use when auto-setting Pet CON stat. ", 30.0)]
        public static double PET_AUTOSET_CON_BASE;

        /// <summary>
        /// Multiplier to use when auto-setting pet CON stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_con_multiplier", "Multiplier to use when auto-setting Pet CON stat. ", 1.0)]
        public static double PET_AUTOSET_CON_MULTIPLIER;

        /// Base Value to use when auto-setting Pet DEX stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_dex_base", "Base Value to use when auto-setting Pet DEX stat. ", 30.0)]
        public static double PET_AUTOSET_DEX_BASE;

        /// Base Value to use when auto-setting Pet INT stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_int_base", "Base Value to use when auto-setting Pet DEX stat. ", 30.0)]
        public static double PET_AUTOSET_INT_BASE;


        /// <summary>
        /// Multiplier to use when auto-setting pet DEX stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_dex_multiplier", "Multiplier to use when auto-setting Pet DEX stat. ", 1.0)]
        public static double PET_AUTOSET_DEX_MULTIPLIER;

        /// <summary>
        /// Multiplier to use when auto-setting pet INT stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_int_multiplier", "Multiplier to use when auto-setting Pet DEX stat. ", 1.0)]
        public static double PET_AUTOSET_INT_MULTIPLIER;



        /// Base Value to use when auto-setting Pet QUI stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_qui_base", "Base Value to use when auto-setting Pet QUI stat. ", 30.0)]
        public static double PET_AUTOSET_QUI_BASE;

        /// <summary>
        /// Multiplier to use when auto-setting pet QUI stat.
        /// </summary>
        [ServerProperty("npc", "pet_autoset_qui_multiplier", "Multiplier to use when auto-setting Pet QUI stat. ", 1.0)]
        public static double PET_AUTOSET_QUI_MULTIPLIER;

        /// <summary>
		/// Scale pet spell values according to their level?
		/// </summary>
		[ServerProperty("npc", "pet_scale_spell_max_level", "Disabled if 0 or less.  If greater than 0, this value is the level at which pets cast their spells at 100% effectivness, so choose spells for pets assuming they're at the level set here.  Live is max pet level, 44 or 50 depending on patch.", 0)]
        public static int PET_SCALE_SPELL_MAX_LEVEL;


        /// <summary>
        /// What level to start increasing mob damage
        /// </summary>
        [ServerProperty("npc", "mob_damage_increase_startlevel", "What level to start increasing mob damage.", 30)]
        public static int MOB_DAMAGE_INCREASE_STARTLEVEL;


        /// <summary>
        /// What level to start increasing mob damage
        /// </summary>
        [ServerProperty("npc", "theurg_pet_conlevel", "What level to start increasing mob Constitution.", 8)]
        public static int THEURG_PET_CONLEVEL;


        /// <summary>
        /// What level to start increasing mob damage
        /// </summary>
        [ServerProperty("npc", "mob_style_damage_increase", "increasing mob style damage.", 3.0)]
        public static double MOB_STYLE_DAMAGE_INCREASE;

        /// <summary>
        /// How much damage to increase per level
        /// </summary>
        [ServerProperty("npc", "mob_damage_increase_perlevel", "How much damage to increase per level", 0.0)]
        public static double MOB_DAMAGE_INCREASE_PERLEVEL;

        /// <summary>
        /// Minimum respawn time for npc's without a set respawninterval
        /// </summary>
        [ServerProperty("npc", "npc_min_respawn_interval", "Minimum respawn time, in minutes, for npc's without a set respawninterval", 5)]
        public static int NPC_MIN_RESPAWN_INTERVAL;

        /// <summary>
        /// Minimum respawn time for npc's without a set respawninterval
        /// </summary>
        [ServerProperty("npc", "npc_min_thinkinterval", "Minimum think interval time", 1500)]
        public static int NPC_MIN_THINKINTERVAL;

        /// <summary>
        /// Minimum respawn time for npc's without a set respawninterval
        /// </summary>
        [ServerProperty("npc", "pet_min_thinkinterval", "Minimum think interval time", 1500)]
        public static int PET_MIN_THINKINTERVAL;

        /// <summary>
        /// Allow Roam
        /// </summary>
        [ServerProperty("npc", "allow_roam", "Allow mobs to roam on the server", true)]
        public static bool ALLOW_ROAM;

        /// <summary>
        /// Roaming Z Scalefactor
        /// </summary>
        [ServerProperty("npc", "roam_zscalefactor", "The Z factor for flight and swimming mobs ", 8)]
        public static int Roam_ZSCALEFACTOR;


        /// <summary>
        /// Allow Roam
        /// </summary>
        [ServerProperty("npc", "allow_stealther_guards", "Allow Stealth on Guarads: Nightshade, Infiltrator, Shadowblade on the Server", true)]
        public static bool ALLOW_STEALTHER_GUARDS;


        /// <summary>
        /// This is to set the baseHP For NPCs
        /// </summary>
        [ServerProperty("npc", "gamenpc_base_hp", "GameNPC's base HP * level", 500)]
        public static int GAMENPC_BASE_HP;

        /// <summary>
        /// How many hitpoints per point of CON above gamenpc_base_con should an NPC gain.
        /// This modification is applied prior to any buffs
        /// </summary>
        [ServerProperty("npc", "gamenpc_hp_gain_per_con", "How many hitpoints per point of CON above gamenpc_base_con should an NPC gain", 2)]
        public static int GAMENPC_HP_GAIN_PER_CON;

        /// <summary>
        /// What is the base contitution for npc's
        /// </summary>
        [ServerProperty("npc", "gamenpc_base_con", "GameNPC's base Constitution", 30)]
        public static int GAMENPC_BASE_CON;

        /// What is the base contitution for npc's
        /// </summary>
        [ServerProperty("npc", "keep_guard_strength", "GameNPC's base Constitution", 5)]
        public static int KEEP_GUARD_STRENGTH;

        /// <summary>
        /// Chance for NPC to random walk. Default is 20
        /// </summary>
        [ServerProperty("npc", "gamenpc_randomwalk_chance", "Chance for NPC to random walk. Default is 20", 20)]
        public static int GAMENPC_RANDOMWALK_CHANCE;


        /// <summary>
        /// Chance for NPC to random walk. Default is 20
        /// </summary>
        [ServerProperty("npc", "gamenpc_yell_chance", "NPC can yell for help and bring add. Default is 5%", 5)]
        public static int GAMENPC_YELL_CHANCE;

        /// <summary>
        /// Chance for NPC to change attack target
        /// </summary>
        [ServerProperty("npc", "gamenpc_yell_switch_chance", "Chance for add NPC to attack the owner of a pet. Default is 10%", 10)]
        public static int GAMENPC_YELL_Switch_CHANCE;



        /// <summary>
        /// Chance for NPC to random walk. Default is 20
        /// </summary>
        [ServerProperty("npc", "gamenpc_yell_chance_boss", "NPC can yell for help and bring add. Default is 5%", 50)]
        public static int GAMENPC_YELL_CHANCE_BOSS;

        /// <summary>
        /// ADD count max Radom between 1 for BossBrain.
        /// </summary>
        [ServerProperty("npc", "gamenpc_yell_add_count_boss", "ADD count max Radom between 1. Default is 4", 4)]
        public static int GAMENPC_YELL_ADD_Count_BOSS;


        /// <summary>
        /// How often, in milliseconds, to check follow distance.  Lower numbers make NPC follow closer but increase load on server.
        /// </summary>
        [ServerProperty("npc", "gamenpc_followcheck_time", "How often, in milliseconds, to check follow distance. Lower numbers make NPC follow closer but increase load on server.", 500)]
        public static int GAMENPC_FOLLOWCHECK_TIME;


        /// <summary>
        /// Override the classtype of any npc with a classtype of DOL.GS.GameNPC
        /// </summary>
        [ServerProperty("npc", "gamenpc_default_classtype", "Change the classtype of any npc of classtype DOL.GS.GameNPC to this.", "DOL.GS.GameNPC")]
        public static string GAMENPC_DEFAULT_CLASSTYPE;

        /// <summary>
        /// Chances for npc (including pet) to style (chance is calculated randomly according to this value + the number of style the NPC own)
        /// </summary>
        [ServerProperty("npc", "gamenpc_chances_to_style", "Change the chance to fire a style for a mob or a pet", 20)]
        public static int GAMENPC_CHANCES_TO_STYLE;

        /// <summary>
        /// Chances for npc (including pet) to cast (chance is calculated randomly according to this value + the number of spells the NPC own)
        /// </summary>
        [ServerProperty("npc", "gamenpc_chances_to_cast", "Change the chance to cast a spell for a mob or a pet", 25)]
        public static int GAMENPC_CHANCES_TO_CAST;

        /// <summary>
		/// Expand the Wild Minion RA to also improve crit chance for ranged and spell attacks?
		/// </summary>
		[ServerProperty("npc", "expand_wild_minion", "Expand the Wild Minion RA to also improve crit chance for ranged and spell attacks?", false)]
        public static bool EXPAND_WILD_MINION;

        #endregion

        #region PVP / RVR

        /// <summary>
        /// This will Allow/Disallow dual loggins
        /// </summary>
        [ServerProperty("system", "nf_task_timer_enable", "If the World timer Enabled for RvR Relic Doors?.", true)]
        public static bool NF_Task_Timer_Enabled;

              
        /// <summary>
        /// Grace period in minutes to allow relog near enemy structure after link death
        /// </summary>
        [ServerProperty("pvp", "RvRLinkDeathRelogGracePeriod", "The Grace Period in minutes, to allow to relog near enemy structure after a link death.", "20")]
        public static string RVR_LINK_DEATH_RELOG_GRACE_PERIOD;

        /// <summary>
        /// PvP Immunity Timer - Killed by Mobs
        /// </summary>
        [ServerProperty("pvp", "Timer_Killed_By_Mob", "Immunity Timer When player killed in PvP, in seconds", 30)] //30 seconds default
        public static int TIMER_KILLED_BY_MOB;

        /// <summary>
        /// PvP Immunity Timer - Killed by Player
        /// </summary>
        [ServerProperty("pvp", "Timer_Killed_By_Player", "Immunity Timer When player killed in PvP, in seconds", 120)] //2 min default
        public static int TIMER_KILLED_BY_PLAYER;

        /// <summary>
        /// PvP Immunity Timer - Region Changed (Enter World Timer Divided by 3 of this)
        /// </summary>
        [ServerProperty("pvp", "Timer_Region_Changed", "Immunity Timer when player changes regions, in seconds", 30)] //30 seconds default
        public static int TIMER_REGION_CHANGED;

        /// <summary>
        /// Time after a relic lost in nature is returning to his ReturnRelicPad pad
        /// </summary>
        [ServerProperty("pvp", "Relic_Return_Time", "A lost relic will automatically returns to its defined point, in seconds", 20 * 60)] //20 mins default
        public static int RELIC_RETURN_TIME;


        /// <summary>
        /// Should we allow Command /Realm
        /// </summary>
        [ServerProperty("map", "show_realm", "Should we allow Show Warmap", false)]
        public static bool Show_Realm;

        /// <summary>
        /// Allow all realms access to DF
        /// </summary>
        [ServerProperty("pvp", "allow_rps_for_dead_players", "Should we allow 25% RPS and BPS for death palyer? ", false)]
        public static bool ALLOW_RPS_FOR_DEAD_PLAYERS;

        /// <summary>
        /// Allow all realms access to DF
        /// </summary>
        [ServerProperty("pvp", "allow_all_realms_df", "Should we allow all realms access to DF", false)]
        public static bool ALLOW_ALL_REALMS_DF;

        /// <summary>
        /// Allow Bounty Points to be gained in Battlegrounds
        /// </summary>
        [ServerProperty("pvp", "allow_bps_in_bgs", "Allow bounty points to be gained in battlegrounds", false)]
        public static bool ALLOW_BPS_IN_BGS;

        /// <summary>
        /// This if the server battleground zones are open to players
        /// </summary>
        [ServerProperty("pvp", "bg_zones_open", "Can the players teleport to battleground", true)]
        public static bool BG_ZONES_OPENED;

        /// <summary>
        /// Message to display to player if BG zones are closed
        /// </summary>
        [ServerProperty("pvp", "bg_zones_closed_message", "Message to display to player if BG zones are closed", "The battlegrounds are not open on this server.")]
        public static string BG_ZONES_CLOSED_MESSAGE;

        /// <summary>
        /// How many players are required on the relic pad to trigger the pillar?
        /// </summary>
        [ServerProperty("pvp", "relic_players_required_on_pad", "How many players are required on the relic pad to trigger the pillar?", 16)]
        public static int RELIC_PLAYERS_REQUIRED_ON_PAD;

        /// <summary>
        /// Ignore too long outcoming packet or not
        /// </summary>
        [ServerProperty("pvp", "enable_minotaur_relics", "Shall we enable Minotaur Relics ?", false)]
        public static bool ENABLE_MINOTAUR_RELICS;

        /// <summary>
        /// Enable WarMap manager
        /// </summary>
        [ServerProperty("pvp", "enable_warmapmgr", "Shall we enable the WarMap manager ?", false)]
        public static bool ENABLE_WARMAPMGR;
        #endregion

        #region Rewards

        /// <summary>
        /// Allow all realms dayly Rewards
        /// </summary>
        [ServerProperty("rewards", "allow_daily_rewards", "Should we allow all realms dayly Rewards ?", false)]
        public static bool Allow_Daily_Rewards;

        #endregion

        #region KEEPS


        /// <summary>
        /// Anti offline Raid System
        /// </summary>
        [ServerProperty("keeps", "raid_member_count", " wieviele müssen je seite online sein um raiden zu können? 1 = 2 je seite", 1)]
        public static int RAID_MEMBER_COUNT;

        /// <summary>
        /// Anti offline Raid System enable/disable
        /// </summary>
        [ServerProperty("keeps", "raid_system_enabled", "Ist das anti Rais system akive ? ja = true, nein = false", true)]
        public static bool RAID_SYSTEM_ENABLED;

        /// <summary>
        /// Anti offline Raid System enable/disable
        /// </summary>
        [ServerProperty("keeps", "allow_claim_own_keeps_only", "We allow claim own Keeps only", true)]
        public static bool Allow_Claim_Own_Keeps_Only;

        /// <summary>
        /// Guild Claim cost enable/disable
        /// </summary>
        [ServerProperty("keeps", "guild_claim_cost", " Allow Claim cost for NF Towers and keeps?", false)]
        public static bool Guild_Claim_Cost;

        /// <summary>
        /// Guild Tower Claim cost
        /// </summary>
        [ServerProperty("keeps", "guild_Tower_claim_cost", " Claim cost per hours for NF Towers", 25)]
        public static long Guild_Tower_Claim_Cost;

        /// <summary>
        /// Guild Keep Claim cost
        /// </summary>
        [ServerProperty("keeps", "guild_keep_claim_cost", "Claim cost per hours for NF Keeps", 50)]
        public static long Guild_Keep_Claim_Cost;


        /// <summary>
        /// Anti offline Raid System enable/disable
        /// </summary>
        [ServerProperty("keeps", "mission_master_disabled", "We allow claim own Keeps only", false)]
        public static bool Mission_Master_Disabled;

        /// <summary>
        /// Fixed Keep Component hight
        /// </summary>
        [ServerProperty("keeps", "use_fixed_keep_hight", "Set to true if you want to use fixed max Hight of keeps/Towrs = 0-1 ", false)]
        public static bool USE_FIXED_KEEP_HIGHT;


        /// <summary>
        /// Number of seconds between allowed LOS checks for keep guards
        /// </summary>
        [ServerProperty("keeps", "keep_guard_los_check_time", "Number of seconds between allowed LOS checks for keep guards", 5)]
        public static int KEEP_GUARD_LOS_CHECK_TIME;

        /// <summary>
        /// The number of players needed for claiming
        /// </summary>
        [ServerProperty("keeps", "claim_num", "Players Needed For Claim - Edit this to change the amount of players required to claim a keep, towers are half this amount", 8)]
        public static int CLAIM_NUM;

        /// <summary>
        /// Use Keep Balancing
        /// </summary>
        [ServerProperty("keeps", "use_keep_balancing", "Set to true if you want keeps to be higher level in NF the less you have, and lower level the more you have", false)]
        public static bool USE_KEEP_BALANCING;

        /// <summary>
        /// Use Keep Balancing
        /// </summary>
        [ServerProperty("keeps", "cant_use_keepdoors_in_combat", "Set to true if you want keeps to be higher level in NF the less you have, and lower level the more you have", false)]
        public static bool CANT_USE_KEEPDOORS_IN_COMBAT;
        /// <summary>
        /// Use Live Keep Bonuses
        /// </summary>
        [ServerProperty("keeps", "use_live_keep_bonuses", "Set to true if you want to use the live keeps bonuses, for example 3% extra xp", false)]
        public static bool USE_LIVE_KEEP_BONUSES;

        /// <summary>
        /// Use Supply Chain
        /// </summary>
        [ServerProperty("keeps", "use_supply_chain", "Set to true if you want to use the live supply chain for keep teleporting, set to false to allow teleporting to any keep that your realm controls (and towers)", false)]
        public static bool USE_SUPPLY_CHAIN;

        /// <summary>
        /// Load Hookpoints
        /// </summary>
        [ServerProperty("keeps", "load_hookpoints", "Load keep hookpoints", true)]
        public static bool LOAD_HOOKPOINTS;

        /// <summary>
        /// Load Keeps
        /// </summary>
        [ServerProperty("keeps", "load_keeps", "Load keeps", true)]
        public static bool LOAD_KEEPS;

        /// <summary>
        /// The level keeps start at when not claimed - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "starting_keep_level", "The level an unclaimed keep starts at.", 4)]
        public static int STARTING_KEEP_LEVEL;

        /// <summary>
        /// The level keeps start at when claimed - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "starting_keep_claim_level", "The level a claimed keep starts at.", 5)]
        public static int STARTING_KEEP_CLAIM_LEVEL;

        /// <summary>
        /// The maximum keep level - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "siege_catapult_maxrange", "The max range for siege catapults .", 2500)]
        public static int Siege_Catapult_Maxrange;

        /// <summary>
        /// The Ram Damage 
        /// </summary>
        [ServerProperty("keeps", "siege_ram_damage", "The Structure Damage reduction for siege Rams .", 11)]
        public static int Siege_Ram_Damage;

        /// <summary>
        /// The Trebuchet Damage
        /// </summary>
        [ServerProperty("keeps", "siege_trebuchet_damage", "The Structure Damage reduction for siege trebuchet .", 11)]
        public static int Siege_Trebuchet_Damage;

        /// <summary>
        /// The Catapult Damage
        /// </summary>
        [ServerProperty("keeps", "siege_catapult_damage", "The Structure Damage reduction for siege catapults .", 10)]
        public static int Siege_Catapult_Damage;

        /// <summary>
        /// The maximum keep level - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "siege_trebuchet_maxrange", "The max range for siege catapults .", 6500)]
        public static int Siege_Trebuchet_Maxrange;

        /// <summary>
        /// The maximum keep level - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "siege_balista_maxrange", "The max range for siege catapults .", 2500)]
        public static int Siege_Balista_Maxrange;

        /// <summary>
        /// The maximum keep level - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "max_keep_level", "The maximum keep level.", 5)]
        public static int MAX_KEEP_LEVEL;

        /// <summary>
        /// The maximum keep level - please note only levels 4 and 5 are supported correctly at this time
        /// </summary>
        [ServerProperty("keeps", "max_keep_tower_level", "The maximum keep level.", 5)]
        public static int MAX_KEEP_TOWER_LEVEL;

        /// <summary>
        /// Enable the keep upgrade timer to slowly raise keep levels
        /// </summary>
        [ServerProperty("keeps", "enable_keep_upgrade_timer", "Enable the keep upgrade timer to slowly raise keep levels?", false)]
        public static bool ENABLE_KEEP_UPGRADE_TIMER;

        /// <summary>
        /// Define toughness for keep and tower walls: 100 is 100% player's damages inflicted.
        /// </summary>
        [ServerProperty("keeps", "set_structures_toughness", "This value is % of total damages inflicted to walls. (100=full damages)", 100)]
        public static int SET_STRUCTURES_TOUGHNESS;

        /// <summary>
        /// Define toughness for keep doors: 100 is 100% player's damages inflicted.
        /// </summary>
        [ServerProperty("keeps", "set_keep_door_toughness", "This value is % of total damages inflicted to level 1 door. (100=full damages)", 100)]
        public static int SET_KEEP_DOOR_TOUGHNESS;

        /// <summary>
        /// Define toughness for tower doors: 100 is 100% player's damages inflicted.
        /// </summary>
        [ServerProperty("keeps", "set_tower_door_toughness", "This value is % of total damages inflicted to level 1 door. (100=full damages)", 100)]
        public static int SET_TOWER_DOOR_TOUGHNESS;

        /// <summary>
        /// Allow player pets to attack keep walls
        /// </summary>
        [ServerProperty("keeps", "structures_allowpetattack", "Allow player pets to attack keep and tower walls?", true)]
        public static bool STRUCTURES_ALLOWPETATTACK;

        /// <summary>
        /// Allow player pets to attack keep and tower doors
        /// </summary>
        [ServerProperty("keeps", "doors_allowpetattack", "Allow player pets to attack keep and tower doors?", true)]
        public static bool DOORS_ALLOWPETATTACK;

        /// <summary>
        /// Multiplier used in determining RP reward for claiming towers.
        /// </summary>
        [ServerProperty("keeps", "tower_rp_claim_multiplier", "Integer multiplier used in determining RP reward for claiming towers.", 100)]
        public static int TOWER_RP_CLAIM_MULTIPLIER;

        /// <summary>
        /// Multiplier used in determining RP reward for keeps.
        /// </summary>
        [ServerProperty("keeps", "keep_rp_claim_multiplier", "Integer multiplier used in determining RP reward for claiming keeps.", 1000)]
        public static int KEEP_RP_CLAIM_MULTIPLIER;

        /// <summary>
        /// Turn on logging of keep captures
        /// </summary>
        [ServerProperty("keeps", "log_keep_captures", "Turn on logging of keep captures?", false)]
        public static bool LOG_KEEP_CAPTURES;


        /// <summary>
        /// Keeps RP capture cap
        /// </summary>
        [ServerProperty("keeps", "keep_capture_wortrp_cap", "Max allowed captures of keeps to get RP", 250)]
        public static int KEEP_CAPTURE_WORTRP_CAP;

        /// <summary>
        /// Tower RP capture cap
        /// </summary>
        [ServerProperty("keeps", "tower_capture_wortrp_cap", "Base RP value of a keep", 500)]
        public static int TOWER_CAPTURE_WORTRP_CAP;


        /// <summary>
        /// Base RP value of a keep
        /// </summary>
        [ServerProperty("keeps", "keep_rp_base", "Base RP value of a keep", 4500)]
        public static int KEEP_RP_BASE;

        /// <summary>
        /// Base RP value of a tower
        /// </summary>
        [ServerProperty("keeps", "tower_rp_base", "Base RP value of a tower", 500)]
        public static int TOWER_RP_BASE;

        /// <summary>
        /// The number of seconds from last kill the this lord is worth no RP
        /// </summary>
        [ServerProperty("keeps", "lord_rp_worth_seconds", "The number of seconds from last kill the this lord is worth no RP.", 300)]
        public static int LORD_RP_WORTH_SECONDS;

        /// <summary>
        /// Multiplier used to add or subtract RP worth based on keep level difference from 50.
        /// </summary>
        [ServerProperty("keeps", "keep_rp_multiplier", "Integer multiplier used to increase/decrease RP worth based on keep level difference from 50.", 50)]
        public static int KEEP_RP_MULTIPLIER;

        /// <summary>
        /// Multiplier used to add or subtract RP worth based on tower level difference from 50.
        /// </summary>
        [ServerProperty("keeps", "tower_rp_multiplier", "Integer multiplier used to increase/decrease RP worth based on tower level difference from 50.", 50)]
        public static int TOWER_RP_MULTIPLIER;

        /// <summary>
        /// Multiplier used to add or subtract RP worth based on upgrade level > 1
        /// </summary>
        [ServerProperty("keeps", "upgrade_multiplier", "Integer multiplier used to increase/decrease RP worth based on upgrade level (0..10)", 100)]
        public static int UPGRADE_MULTIPLIER;

        /// <summary>
        /// Multiplier used to determine keep level changes when balancing.  For each keep above or below normal multiply by this.
        /// </summary>
        [ServerProperty("keeps", "keep_balance_multiplier", "Multiplier used to determine keep level changes when balancing.  For each keep above or below normal multiply by this.", 1.0)]
        public static double KEEP_BALANCE_MULTIPLIER;

        /// <summary>
        /// Multiplier used to determine keep level changes when balancing.  For each keep above or below normal multiply by this.
        /// </summary>
        [ServerProperty("keeps", "tower_balance_multiplier", "Multiplier used to determine tower level changes when balancing.  For each keep above or below normal multiply by this.", 0.15)]
        public static double TOWER_BALANCE_MULTIPLIER;

        /// <summary>
        /// Balance Towers and Keeps separately
        /// </summary>
        [ServerProperty("keeps", "balance_towers_separate", "Balance Towers and Keeps separately?", true)]
        public static bool BALANCE_TOWERS_SEPARATE;

        /// <summary>
        /// Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.
        /// </summary>
        [ServerProperty("keeps", "keep_guard_level_multiplier", "Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.", 1.6)]
        public static double KEEP_GUARD_LEVEL_MULTIPLIER;

        /// <summary>
        /// Multiplier used in determining RP reward for keeps.
        /// </summary>
        [ServerProperty("keeps", "keep_guard_respawn_interval", "Keep Guard respawn interval.", 1000)]
        public static int KEEP_GUARD_RESPAWN_INTERVAL;


        /// <summary>
        /// Modifier used to adjust damage for pets on keep components
        /// </summary>
        [ServerProperty("keeps", "pet_damage_multiplier", "Modifier used to adjust damage for pets classes.", 1.0)]
        public static double PET_DAMAGE_MULTIPLIER;

        /// <summary>
        /// Modifier used to adjust damage for pet spam classes (currently animist and theurgist) on keep components
        /// </summary>
        [ServerProperty("keeps", "pet_spam_damage_multiplier", "Modifier used to adjust damage for pet spam classes (currently animist and theurgist).", 1.0)]
        public static double PET_SPAM_DAMAGE_MULTIPLIER;

        /// <summary>
        /// Multiplier used to determine keep guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.
        /// </summary>
        [ServerProperty("keeps", "tower_guard_level_multiplier", "Multiplier used to determine tower guard levels.  This is applied to the bonus level (usually 4) and added after balance adjustments.", 1.0)]
        public static double TOWER_GUARD_LEVEL_MULTIPLIER;

        /// <summary>
        /// Keeps to load. 0 for Old Keeps, 1 for new keeps, 2 for both.
        /// </summary>
        [ServerProperty("keeps", "use_new_keeps", "Keeps to load. 0 for Old Keeps, 1 for new keeps, 2 for both.", 2)]
        public static int USE_NEW_KEEPS;

        /// <summary>
        /// Should guards loaded from db be equipped by Keepsystem? (false=load equipment from db)
        /// </summary>
        [ServerProperty("keeps", "autoequip_guards_loaded_from_db", "Should guards loaded from db be equipped by Keepsystem? (false=load equipment from db)", true)]
        public static bool AUTOEQUIP_GUARDS_LOADED_FROM_DB;

        /// <summary>
        /// Should guards loaded from db be modeled by Keepsystem? (false=load from db)
        /// </summary>
        [ServerProperty("keeps", "automodel_guards_loaded_from_db", "Should guards loaded from db be modeled by Keepsystem? (false=load from db)", true)]
        public static bool AUTOMODEL_GUARDS_LOADED_FROM_DB;

        /// <summary>
        /// Are unclaimed keeps considered the enemy in PvP mode?
        /// </summary>
        [ServerProperty("keeps", "pvp_unclaimed_keeps_enemy", "Are unclaimed keeps considered the enemy in PvP mode?", false)]
        public static bool PVP_UNCLAIMED_KEEPS_ENEMY;

        /// <summary>
        /// Do you allowed player to climb towers?
        /// </summary>
        [ServerProperty("keeps", "allow_tower_climb", "Do you allowed player to climb towers? Set True for yes, False for not.", false)]
        public static bool ALLOW_TOWER_CLIMB;

        #endregion

        #region PVE / TOA

        /// <summary>
        /// Adjustment to missrate per number of attackers
        /// </summary>
        [ServerProperty("pve", "missrate_reduction_per_attackers", "Adjustment to missrate per number of attackers", 0)]
        public static int MISSRATE_REDUCTION_PER_ATTACKERS;

        /// <summary>
        /// Adjustment to missrate per number of attackers
        /// </summary>
        [ServerProperty("pvp", "pvp_missrate_reduction_per_attackers", "Adjustment to missrate per number of attackers", 0)]
        public static int PVP_MISSRATE_REDUCTION_PER_ATTACKERS;

        /// <summary>
        /// Spell damage reduction multiplier based on hitchance below 55%.
        /// Default is 4.3, which will produce the minimum 1 damage at a 33% chance to hit
        /// Lower numbers reduce damage reduction
        /// </summary>
        [ServerProperty("pve", "spell_hitchance_damage_reduction_multiplier", "Spell damage reduction multiplier based on hitchance if < 55%. Lower numbers reduce damage reduction.", 4.3)]
        public static double SPELL_HITCHANCE_DAMAGE_REDUCTION_MULTIPLIER;

        /// <summary>
        /// Spell Charm Resist reduction multiplier based on hitchance below 55%.
        /// Default is 2, which will produce the minimum Resist Chance, based on Realm Level
        /// /// highter numbers reduce Resist Chance
        /// </summary>
        [ServerProperty("pve", "spell_charm_resistchance_reduction_multiplier", "Spell Charm Resist reduction multiplier highter numbers reduce the Resist Chance, based on Realm Level.", 0.33)]
        public static double SPELL_CHARM_RESISTCHANCE_REDUCTION_MULTIPLIER;
        /// <summary>
        /// TOA Artifact XP rate
        /// </summary>
        [ServerProperty("pve", "artifact_xp_rate", "Adjust the rate at which all artifacts gain xp.  Higher numbers mean slower XP gain. XP / this = result", 350)]
        public static int ARTIFACT_XP_RATE;

        /// <summary>
        /// TOA Scroll drop rate
        /// </summary>
        [ServerProperty("pve", "scroll_drop_rate", "Adjust the drop rate (percent chance) for scrolls.", 25)]
        public static int SCROLL_DROP_RATE;

        /// <summary>
        /// Max camp bonus
        /// </summary>
        [ServerProperty("pve", "max_camp_bonus", "Max camp bonus", 2.0)]
        public static double MAX_CAMP_BONUS;

        /// <summary>
        /// Minimum privilege level to be able to enter Atlantis through teleporters.
        /// </summary>
        [ServerProperty("pve", "atlantis_teleport_plvl", "Set the minimum privilege level required to enter Atlantis zones.", 2)]
        public static int ATLANTIS_TELEPORT_PLVL;

        /// <summary>
        /// Time Before Adventure Wings Instances Destroy when Empty
        /// </summary>
        [ServerProperty("pve", "adventurewing_time_to_destroy", "Set the time before Instanced Adventure Wings (Catacombs) are destroy when empty (in minutes).", 5)]
        public static int ADVENTUREWING_TIME_TO_DESTROY;

        #endregion

        #region HOUSING
        /// <summary>
        /// Maximum number of houses supported on this server.  Limits the size of the housing array used for updates
        /// </summary>
        [ServerProperty("housing", "max_num_houses", "Max number of houses supported on this server.", 5000)]
        public static int MAX_NUM_HOUSES;

        /// <summary>
        /// The starting NPCTemplate ID to use for housing NPC's
        /// </summary>
        [ServerProperty("housing", "housing_starting_npctemplate_id", "The starting NPCTemplate ID to use for housing NPC's", 500)]
        public static int HOUSING_STARTING_NPCTEMPLATE_ID;

        /// <summary>
        /// Sets the max allowed items inside a house.
        /// </summary>
        [ServerProperty("housing", "max_indoor_house_items", "Max number of items allowed inside a players house.", 40)]
        public static int MAX_INDOOR_HOUSE_ITEMS;

        /// <summary>
        /// Max outdoor items.  If Outdoor is increased past 30 they vanish. It seems to be hardcoded in client
        /// </summary>
        [ServerProperty("housing", "max_outdoor_house_items", "Max number of items allowed in a players garden.", 30)]
        public static int MAX_OUTDOOR_HOUSE_ITEMS;

        [ServerProperty("housing", "indoor_items_depend_on_size", "If true the max number of allowed House indoor items are set like live (40, 60, 80, 100)", true)]
        public static bool INDOOR_ITEMS_DEPEND_ON_SIZE;

        [ServerProperty("housing", "housing_rent_cottage", "Rent price for a cottage.", 20L * 100L * 100L)] // 20g
        public static long HOUSING_RENT_COTTAGE;

        [ServerProperty("housing", "housing_rent_house", "Rent price for a house.", 35L * 100L * 100L)] // 35g
        public static long HOUSING_RENT_HOUSE;

        [ServerProperty("housing", "housing_rent_villa", "Rent price for a villa.", 60L * 100L * 100L)] // 60g
        public static long HOUSING_RENT_VILLA;

        [ServerProperty("housing", "housing_rent_mansion", "Rent price for a mansion.", 100L * 100L * 100L)] // 100g
        public static long HOUSING_RENT_MANSION;

        [ServerProperty("housing", "housing_lot_price_start", "Starting lot price before per hour reductions", 95L * 1000L * 100L * 100L)] // 95p
        public static long HOUSING_LOT_PRICE_START;

        [ServerProperty("housing", "housing_lot_price_per_hour", "Lot price reduction per hour.", (long)(1.2 * 1000 * 100 * 100))] // 1.2p
        public static long HOUSING_LOT_PRICE_PER_HOUR;

        [ServerProperty("housing", "housing_lot_price_minimum", "Minimum lot price.", 300L * 100L * 100L)] // 300g
        public static long HOUSING_LOT_PRICE_MINIMUM;

        /// <summary>
        /// How often, in days, is rent due?  0 for never, negative for testing repossession
        /// </summary>
        [ServerProperty("housing", "rent_due_days", "How often, in days, is rent due?  0 for never, negative for testing repossession.", 7)]
        public static int RENT_DUE_DAYS;
                
        /// <summary>
        /// How often, in minutes, do we check for rent?
        /// </summary>
        [ServerProperty("housing", "rent_check_interval", "How often, in minutes, do we check for rent?", 120)]
        public static int RENT_CHECK_INTERVAL;

        /// <summary>
        /// How many rent payments can be stored in the lockbox?
        /// </summary>
        [ServerProperty("housing", "rent_lockbox_payments", "How many rent payments can be stored in the lockbox?", 4)]
        public static int RENT_LOCKBOX_PAYMENTS;

        /// <summary>
        /// The worth of 1 (one) bounty point in gold (e.g. 1 bp = 1g -> 10000, 1bp = 10g -> 100000)
        /// </summary>
        [ServerProperty("housing", "rent_bounty_point_to_gold", "The worth of 1 (one) bounty point in gold (e.g. 1 bp = 1g -> 10000, 1bp = 10g -> 100000)", 10000)]
        public static long RENT_BOUNTY_POINT_TO_GOLD;

        /// <summary>
        /// Do housing consignment merchants use BP instead of money?
        /// </summary>
        [ServerProperty("housing", "consignment_use_bp", "If true the housing consignment merchants use BP instead of money.", false)]
        public static bool CONSIGNMENT_USE_BP;
        
        /// <summary>
        /// Enable consignment merchants and market cache
        /// </summary>
        [ServerProperty("housing", "market_enable", "If true the market explorers are enabled and the cache is initialized on server start.", true)]
        public static bool MARKET_ENABLE;

        /// <summary>
        /// List Items from all Realm, or only for Hibs, Mids, ALbs Lot ? 
        /// </summary>
        [ServerProperty("housing", "market_list_seperate", "set to false if you wish to list Items from all Realms on the Marketer, if true, all seperate", true)]
        public static bool MARKET_LIST_SEPERATE;

        /// <summary>
        /// List Items from all Realm, or only for Hibs, Mids, ALbs Lot ? 
        /// </summary>
        [ServerProperty("housing", "load_housing_items", "Allow load housing items ?", true)]
        public static bool LOAD_HOUSING_ITEMS;


        /// <summary>
        /// List Items from all Realm, or only for Hibs, Mids, ALbs Lot ? 
        /// </summary>
        [ServerProperty("housing", "load_housing_npc", "Allow load housing npcs ?", true)]
        public static bool LOAD_HOUSING_NPC;

        /// <summary>
        /// Enable logging of all market activity
        /// </summary>
        [ServerProperty("housing", "market_enable_log", "Enable debug logging of all market activity", true)]
        public static bool MARKET_ENABLE_LOG;

        /// <summary>
        /// What is the additional fee (%) charged to players using the market explorer?
        /// </summary>
        [ServerProperty("housing", "market_fee_percent", "What is the additional fee (%) charged to players using the market explorer?", 20)]
        public static int MARKET_FEE_PERCENT;

        /// <summary>
        /// How many items can the market search return?
        /// </summary>
        [ServerProperty("housing", "market_search_limit", "How many items can the market search return?", 300)]
        public static int MARKET_SEARCH_LIMIT;






        #endregion

        #region CLASSES
        /*
        /// <summary>
        /// Do we want to allow items to be equipped regardless of realm?
        /// </summary>
        [ServerProperty("classes", "allow_dualwield_hit", "Do we not want dualhits with onhand Styles?", true)]
        public static bool ALLOW_DUALWIELD_HIT;
        */
        [ServerProperty("classes", "allow_overcap", "Do we allow overcap Stats ?", true)]
        public static bool ALLOW_OVERCAP;

        /// <summary>
        /// The message players get when they enter the game at level 1
        /// </summary>
        [ServerProperty("classes", "starting_realm_level", "Starting Realm level - Edit this to set which realm level a new player starts the game with", 0)]
        public static int STARTING_REALM_LEVEL;

        /// <summary>
        /// The amount of copper a player starts with
        /// </summary>
        [ServerProperty("classes", "starting_money", "Starting Money - Edit this to change the amount in copper of money new characters start the game with, max 214 plat", 0)]
        public static long STARTING_MONEY;

        /// <summary>
        /// The amount of Bounty Points a player starts with
        /// </summary>
        [ServerProperty("classes", "starting_bps", "Starting Bounty Points - Edit this to change the amount of Bounty Points the new characters start the game with", 0)]
        public static long STARTING_BPS;

        /// <summary>
        /// The level of experience a player should start with
        /// </summary>
        [ServerProperty("classes", "starting_level", "Starting Level - Edit this to set which levels experience a new player start the game with", 1)]
        public static int STARTING_LEVEL;

        /// <summary>
		/// Allow players to /train without having a trainer present
		/// </summary>
		[ServerProperty("classes", "allow_train_anywhere", "Allow players to use the /train command to open a trainer window anywhere in the world?", false)]
        public static bool ALLOW_TRAIN_ANYWHERE;


        /// <summary>
        /// Disable some classes from being created
        /// </summary>
        [ServerProperty("classes", "disabled_classes", "Serialized list of disabled classes, separated by semi-colon or a range with a dash (ie 1-5;7;9)", "")]
        public static string DISABLED_CLASSES;

        /// <summary>
        /// Disable some races from being created
        /// </summary>
        [ServerProperty("classes", "disabled_races", "Serialized list of disabled races, separated by semi-colon or a range with a dash (ie 1-5;7;9)", "")]
        public static string DISABLED_RACES;

        /// <summary>
        /// Days before your elligable for a free level in Albion
        /// </summary>
        [ServerProperty("classes", "freelevel_days_albion", "days before your elligable for a free level in Albion, use -1 to deactivate", 7)]
        public static int FREELEVEL_DAYS_ALBION;

        /// <summary>
        /// Days before your elligable for a free level in Midgard
        /// </summary>
        [ServerProperty("classes", "freelevel_days_midgard", "days before your elligable for a free level in Midgard, use -1 to deactivate", 7)]
        public static int FREELEVEL_DAYS_MIDGARD;

        /// <summary>
        /// Days before your elligable for a free level in Hibernia
        /// </summary>
        [ServerProperty("classes", "freelevel_days_hibernia", "days before your elligable for a free level in Hibernia, use -1 to deactivate", 7)]
        public static int FREELEVEL_DAYS_HIBERNIA;
        /// <summary>
        /// Buff Range, 0 for unlimited
        /// </summary>
        [ServerProperty("classes", "buff_range", "The range that concentration buffs can last from the owner before it expires.  0 for unlimited.", 0)]
        public static int BUFF_RANGE;

        /// <summary>
        /// Buff Range, 0 for unlimited
        /// </summary>
        [ServerProperty("classes", "buff_remove_on_porting", "Should we remove buffs from players if porting?", false)]
        public static bool BUFF_Remove_on_porting;

        /// <summary>
        /// Buff Range, 0 for unlimited
        /// </summary>
        [ServerProperty("classes", "custom_buffsher_value", "Custom Buffsher value add: standard 0", 0)]
        public static int CUSTOM_BUFFSHEAR_VALUE;

        /// <summary>
        /// Allow Cata Slash Level
        /// </summary>
        [ServerProperty("classes", "allow_cata_slash_level", "Allow catacombs classes to use /level command", false)]
        public static bool ALLOW_CATA_SLASH_LEVEL;

        /// <summary>
        /// Sets the Cap for Player Turrets
        /// </summary>
        [ServerProperty("classes", "turret_player_cap_count", "Sets the cap of turrets for a Player", 5)]
        public static int TURRET_PLAYER_CAP_COUNT;

        /// <summary>
        /// Sets the Cap for Player Turrets
        /// </summary>
        [ServerProperty("classes", "turret_player_cap_count_pve", "Sets the cap of turrets for a Player", 10)]
        public static int TURRET_PLAYER_CAP_COUNT_PVE;


        /// <summary>
        /// Sets the Area Cap for Turrets
        /// </summary>
        [ServerProperty("classes", "turret_area_cap_count", "Sets the cap of the Area for turrets", 10)]
        public static int TURRET_AREA_CAP_COUNT;

        /// <summary>
        /// Sets the Circle of the Area to check for Turrets
        /// </summary>
        [ServerProperty("classes", "turret_area_cap_radius", "Sets the Radius which is checked for the turretareacap", 1000)]
        public static int TURRET_AREA_CAP_RADIUS;

        [ServerProperty("classes", "theurgist_pet_cap", "Sets the maximum number of pets a Theurgist can summon", 16)]
        public static int THEURGIST_PET_CAP;

        /// <summary>
        /// Do we want to allow items to be equipped regardless of realm?
        /// </summary>
        [ServerProperty("classes", "allow_cross_realm_items", "Do we want to allow items to be equipped regardless of realm?", false)]
        public static bool ALLOW_CROSS_REALM_ITEMS;

        /// <summary>
        /// What level should /level bring you to? 0 to disable
        /// </summary>
        [ServerProperty("classes", "slash_level_target", "What level should /level bring you to? 0 is disabled.", 0)]
        public static int SLASH_LEVEL_TARGET;

        /// <summary>
        /// What level should you have on your account to be able to use /level?
        /// </summary>
        [ServerProperty("classes", "slash_level_requirement", "What level should you have on your account be able to use /level?", 50)]
        public static int SLASH_LEVEL_REQUIREMENT;

        /// <summary>
        /// What levels did we allow a DOL respec ? serialized
        /// </summary>
        [ServerProperty("classes", "give_dol_respec_at_level", "What levels does we give a DOL respec ? separated by a semi-colon or a range with a dash (ie 1-5;7;9)", "0")]
        public static string GIVE_DOL_RESPEC_AT_LEVEL;

        /// <summary>
        /// Should the server start characters as Base Class?
        /// </summary>
        [ServerProperty("classes", "start_as_base_class", "Should we start all players as their base class? true if yes (e.g. Armsmen become Fighters on Creation)", false)]
        public static bool START_AS_BASE_CLASS;

        /// <summary>
        /// Should the server start characters as Base Class?
        /// </summary>
        [ServerProperty("classes", "allow_old_archery", "Should we allow archers to be able to use arrows from their quiver?", false)]
        public static bool ALLOW_OLD_ARCHERY;

        /// <summary>
        /// arrows for new Archery
        /// </summary>
        [ServerProperty("classes", "allow_new_archery_check_arrows", "Should we allow archers to be able to use arrows for new Archery from their quiver?", false)]
        public static bool Allow_New_Archery_Check_Arrows;

        // <summary>
        /// Heretic Dot Damage Cap Value
        /// </summary>
        [ServerProperty("classes", "heretic_dotpulse_damagecap", " Damage = Taget.MaxHealth / Cap ", 3)]
        public static int Heretic_DoTPulse_DamageCap;



        #endregion

        #region BUFFS/Debuffs

        /// <summary>
        /// Spells-related properties for Shield Buffs
        /// </summary>
        [ServerProperty("spells", "shieldbuff_damage_add", "add damage to Shieldbuffs (global)", 14.1)]
        public static double SHIELDBUFF_DAMAGE_ADD;

        #endregion
        #region SPELLS


        /// <summary>
        /// Spells-related properties
        /// </summary>

        [ServerProperty("spells", "spell_interrupt_duration", "", 4500)]
        public static int SPELL_INTERRUPT_DURATION;

        [ServerProperty("spells", "spell_interrupt_recast", "", 2000)]
        public static int SPELL_INTERRUPT_RECAST;

        [ServerProperty("spells", "spell_interrupt_again", "", 100)]
        public static int SPELL_INTERRUPT_AGAIN;

        [ServerProperty("spells", "spell_interrupt_maxstagelength", "Max length of stage 1 and 3, 1000 = 1 second", 1500)]
        public static int SPELL_INTERRUPT_MAXSTAGELENGTH;

        [ServerProperty("spells", "spell_gtae_need_los", "Does GTAE spells Need LoS to be Casted ?", true)]
        public static bool SPELL_GTAE_NEED_LOS;

        #endregion

        #region GUILDS / ALLIANCES
        /// <summary>
        /// The a starting guild should be used
        /// </summary>
        [ServerProperty("guild", "starting_guild", "Starter Guild - Edit this to change the starter guild options, values True,False", true)]
        public static bool STARTING_GUILD;


        /// <summary>
        /// Guild Due level
        /// </summary>
        [ServerProperty("guild", "guild_due_level", "Guild Level needed for Due - Edit this to change the Level needed for Guild Due standard is 5", 5)]
        public static int GUILD_DUE_LEVEL;

        /// <summary>
        /// The max number of guilds in an alliance
        /// </summary>
        [ServerProperty("guild", "alliance_max", "Max Guilds In Alliance - Edit this to change the maximum number of guilds in an alliance -1 = unlimited, 0=disable alliances", -1)]
        public static int ALLIANCE_MAX;

        /// <summary>
        /// The number of players needed to form a guild
        /// </summary>
        [ServerProperty("guild", "guild_num", "Players Needed For Guild Form - Edit this to change the amount of players required to form a guild", 8)]
        public static int GUILD_NUM;

        /// <summary>
        /// This enables or disables new guild dues. Live standard is 2% dues
        /// </summary>
        [ServerProperty("guild", "new_guild_dues", "Guild dues can be set from 1-100% if enabled, or standard 2% if not", false)]
        public static bool NEW_GUILD_DUES;

        /// <summary>
        /// Do we allow guild members from other realms
        /// </summary>
        [ServerProperty("guild", "allow_cross_realm_guilds", "Do we allow guild members from other realms?", false)]
        public static bool ALLOW_CROSS_REALM_GUILDS;

        /// <summary>
        /// How many things do we allow guilds to claim?
        /// </summary>
        [ServerProperty("guild", "guilds_claim_limit", "How many things do we allow guilds to claim?", 1)]
        public static int GUILDS_CLAIM_LIMIT;

        /// <summary>
        /// Guild Crafting Buff bonus amount
        /// </summary>
        [ServerProperty("guild", "guild_buff_crafting", "Percent speed gain for the guild crafting buff?", (ushort)5)]
        public static ushort GUILD_BUFF_CRAFTING;

        /// <summary>
        /// Guild XP Buff bonus amount
        /// </summary>
        [ServerProperty("guild", "guild_buff_xp", "Extra XP gain percent for the guild PvE XP buff?", (ushort)5)]
        public static ushort GUILD_BUFF_XP;

        /// <summary>
        /// Guild RP Buff bonus amount
        /// </summary>
        [ServerProperty("guild", "guild_buff_rp", "Extra RP gain percent for the guild RP buff?", (ushort)2)]
        public static ushort GUILD_BUFF_RP;

        /// <summary>
        /// Guild BP Buff bonus amount -  this is not available on live, disabled by default
        /// </summary>
        [ServerProperty("guild", "guild_buff_bp", "Extra BP gain percent for the guild BP buff?", (ushort)0)]
        public static ushort GUILD_BUFF_BP;

        /// <summary>
        /// Guild artifact XP Buff bonus amount
        /// </summary>
        [ServerProperty("guild", "guild_buff_artifact_xp", "Extra artifact XP gain percent for the guild artifact XP buff?", (ushort)5)]
        public static ushort GUILD_BUFF_ARTIFACT_XP;

        /// <summary>
        /// Guild masterlevel XP Buff bonus amount
        /// </summary>
        [ServerProperty("guild", "guild_buff_masterlevel_xp", "Extra masterlevel XP gain percent for the guild masterlevel XP buff?", (ushort)20)]
        public static ushort GUILD_BUFF_MASTERLEVEL_XP;

        /// <summary>
        /// How much merit to reward guild when dragon is killed, if any.
        /// </summary>
        [ServerProperty("guild", "guild_merit_on_dragon_kill", "How much merit to reward guild when dragon is killed, if any.", (ushort)0)]
        public static ushort GUILD_MERIT_ON_DRAGON_KILL;

        /// <summary>
        /// When a banner is lost to the enemy how long is the wait before purchase is allowed?  In Minutes.
        /// </summary>
        [ServerProperty("guild", "guild_banner_lost_time", "When a banner is lost to the enemy how many minutes is the wait before purchase is allowed?", (ushort)1440)]
        public static ushort GUILD_BANNER_LOST_TIME;



        #endregion

        #region CRAFT / SALVAGE
        /// <summary>
        /// The crafting speed modifier
        /// </summary>
        [ServerProperty("craft", "crafting_speed", "Crafting Speed Modifier - Edit this to change the speed at which you craft e.g 1.5 is 50% faster 2.0 is twice as fast (100%) 0.5 is half the speed (50%)", 1.0)]
        public static double CRAFTING_SPEED;

        /// <summary>
        /// Crafting skill gain bonus in capital cities
        /// </summary>
        [ServerProperty("craft", "capital_city_crafting_skill_gain_bonus", "Crafting skill gain bonus % in capital cities; 5 = 5%", 5)]
        public static int CAPITAL_CITY_CRAFTING_SKILL_GAIN_BONUS;

        /// <summary>
        /// Crafting speed bonus in capital cities
        /// </summary>
        [ServerProperty("craft", "capital_city_crafting_speed_bonus", "Crafting speed bonus in capital cities; 2 = 2x, 3 = 3x, ..., 1 = standard", 1.0)]
        public static double CAPITAL_CITY_CRAFTING_SPEED_BONUS;

        /// <summary>
        /// Use salvage per realm and get back material to use in chars realm
        /// </summary>
        [ServerProperty("salvage", "use_salvage_per_realm", "Enable to get back material to use in chars realm. Disable to get back the same material in all realms.", false)]
        public static bool USE_SALVAGE_PER_REALM;

        /// <summary>
        /// Use salvage per realm and get back material to use in chars realm
        /// </summary>
        [ServerProperty("salvage", "use_new_salvage", "Enable to use a new system calcul of salvage count based on object_type.", false)]
        public static bool USE_NEW_SALVAGE;

        #endregion

        #region ACCOUNT

        /// <summary>
        /// How often, in days, is a login Event
        /// </summary>
        [ServerProperty("account", "daily_events_timer", "How often, in days, is a login event? ", 1)]
        public static int Daily_Events_timer;


        /// <summary>
        /// Allow auto-account creation  This is also set in serverconfig.xml and must be enabled for this property to work.
        /// </summary>
        [ServerProperty("account", "allow_auto_account_creation", "Allow auto-account creation  This is also set in serverconfig.xml and must be enabled for this property to work.", true)]
        public static bool ALLOW_AUTO_ACCOUNT_CREATION;

        /// <summary>
        /// Account bombing prevention
        /// </summary>
        [ServerProperty("account", "time_between_account_creation", "The time in minutes between 2 accounts creation. This avoid account bombing with dynamic ip. 0 to disable", 0)]
        public static int TIME_BETWEEN_ACCOUNT_CREATION;

        /// <summary>
        /// Account IP bombing prevention
        /// </summary>
        [ServerProperty("account", "time_between_account_creation_sameip", "The time in minutes between accounts creation from the same ip after 2 creations.", 15)]
        public static int TIME_BETWEEN_ACCOUNT_CREATION_SAMEIP;

        /// <summary>
        /// Total number of account allowed for the same IP
        /// </summary>
        [ServerProperty("account", "total_accounts_allowed_sameip", "Total number of account allowed for the same IP", 20)]
        public static int TOTAL_ACCOUNTS_ALLOWED_SAMEIP;

        /// <summary>
        /// Should we backup deleted characters and not delete associated content?
        /// </summary>
        [ServerProperty("account", "backup_deleted_characters", "Should we backup deleted characters and not delete associated content?", true)]
        public static bool BACKUP_DELETED_CHARACTERS;


        #endregion

        #region LANGUAGE

        /// <summary>
        /// Holds all custom language keys that are allowed to use on the server, separated by semi-colon (DE; FR; IT etc.). There is no need to add english (EN), it's supported by default.
        /// </summary>
        [ServerProperty("language", "allowed_custom_language_keys", "Holds all custom language keys that are allowed to use on the server, separated by semi-colon (DE; FR; IT etc.). There is no need to add english (EN), it's supported by default.", "CU; DE; FR; IT")]
        public static string ALLOWED_CUSTOM_LANGUAGE_KEYS;

        /// <summary>
        /// Should we use the new language system? True = yes, False = no | Default: false
        /// </summary>
        [ServerProperty("language", "use_new_language_system", "Should we use the new language system? True = yes, False = no", false)]
        public static bool USE_NEW_LANGUAGE_SYSTEM;

        #endregion

        /// <summary>
        /// This method loads the property from the database and returns
        /// the value of the property as strongly typed object based on the
        /// type of the default value
        /// </summary>
        /// <param name="attrib">The attribute</param>
        /// <returns>The real property value</returns>
        public static object Load(ServerPropertyAttribute attrib)
        {
            var key = attrib.Key;

            var property = GameServer.Database.SelectObjects<ServerProperty>("`key` = @key", new QueryParameter("@key", key)).FirstOrDefault();

            if (property == null)
            {
                property = new ServerProperty
                {
                    Category = attrib.Category,
                    Key = attrib.Key,
                    Description = attrib.Description,
                    DefaultValue = attrib.DefaultValue.ToString(),
                    Value = attrib.DefaultValue.ToString()
                };
                GameServer.Database.AddObject(property);
                log.Debug($"Cannot find server property {key}, creating it.");
            }

            log.Debug($"Loading {key}. Value is {property.Value}");

            try
            {
                var myCIintl = new CultureInfo("en-US", false);
                var provider = myCIintl.NumberFormat;
                return Convert.ChangeType(property.Value, attrib.DefaultValue.GetType(), provider);
            }
            catch (FormatException fe)
            {
                log.Error($"Format exception in ServerProperties Load: {fe.Message}", fe);
            }
            catch (Exception e)
            {
                log.Error("Exception in ServerProperties Load: ", e);
            }

            log.Error($"Trying to load {key}. Value is {property.Value}");
            return null;
        }


        /// <summary>
        /// This method is the key. It checks all fields of a specific type and
        /// if the field is a ServerProperty it loads the value from the database.
        /// </summary>
        /// <param name="type">The type to analyze</param>
        protected static void Init(Type type)
        {
            foreach (FieldInfo field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attrib = field.GetCustomAttribute<ServerPropertyAttribute>();
                if (attrib != null)
                {
                    try
                    {
                        field.SetValue(null, Load(attrib));
                    }
                    catch (Exception ex)
                    {
                        // Hier könnte Logging hinzugefügt werden oder eine spezifische Fehlerbehandlung
                        Console.WriteLine($"Fehler beim Setzen des Wertes für das Feld {field.Name}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Refreshes the server properties from the DB
        /// </summary>
        public static void Refresh()
        {
            log.Info("Refreshing server properties...");
            Init(typeof(Properties));
        }
    }
}
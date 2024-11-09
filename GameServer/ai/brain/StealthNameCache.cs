using System.Collections.Generic;

namespace DOL.ai.brain
{
    /// <summary>
    /// A static class that provides a cache of stealther names for quick lookup.
    /// </summary>
    public static class StealthNameCache
    {
        /// <summary>
        /// A HashSet that contains the stealther names.
        /// </summary>
        private static readonly HashSet<string> stealthNames;

        /// <summary>
        /// Static constructor that initializes the HashSet with a predefined list of stealther names.
        /// </summary>
        static StealthNameCache()
        {
            stealthNames = new HashSet<string>
            {
                "shadow", "Renegade Nightshade", "Renegade Shadowblade", "Renegade Infiltrator",
                "demented master infiltrator", "bean-nighe", "blackmane rogue", "bloodmoon footpad",
                "bogman hunter", "corrupt shadowweaver", "corrupted pixie scout", "crazed deceptor",
                "crazed huntsman", "darkwood ambusher", "darkwood assassin", "darkwood petty thief",
                "darkwood pilferer", "deranged assassin", "deranged scout", "fanatical hunter",
                "fanatical hand of Loki", "fanatical hand of Skadi", "ferocious master marksman",
                "glashtin sneak", "haunted assassin", "haunted master spy", "jagged fang sneak",
                "Lur'tai", "maniacal shadow", "outcast bloody axe scout", "outcast broken tusk thief",
                "outcast scratchfoot sneak", "outcast thallooniagh rogue", "plutonian lurker",
                "raving thief", "Shambling Shade", "tenebrous brawler", "tenebrous cabalist",
                "tenebrous cleric", "tenebrous deacon", "tenebrous doomguard", "tenebrous fighter",
                "tenebrous gray knight", "tenebrous guardian", "Tenebrous Illusionist",
                "tenebrous infiltrator", "tenebrous invoker", "tenebrous legionnaire", "Master Aluk",
                "Master Dare", "Master Kirs", "cyclops scout", "pixie scout", "outcast rogue",
                "red dwarf thief", "spriggarn ambusher", "svartalf infiltrator", "Darius",
                "tenebrous blue hand", "tenebrous curate", "tenebrous elementalist", "tenebrous imbuer",
                "plutonian assassin", "plutonian shade", "tenebrous soldier", "tenebrous creator",
                "tenebrous reaver", "tenebrous prelate", "tenebrous infantryman", "urchin ambusher",
                "Undead Assassin", "tenebrous zealot", "tenebrous wizard", "tenebrous warrior",
                "tenebrous spawn", "tenebrous priest", "Tenebrous Packmaster", "tenebrous necromancer",
                "tenebrous mine spider", "tenebrous mine rat", "tenebrous mercenary", "Archon Reiche",
                "corrupt minotaur shade", "minotaur shade", "nightshade", "tomb raider scout",
                "Spirit Priest of Vartiketh", "sylvan goblin hunter", "Nightshade", "Shadowblade",
                "Infiltrator"
            };
        }

        /// <summary>
        /// Checks if the specified name exists in the stealth names cache.
        /// </summary>
        /// <param name="name">The name to check for existence in the cache.</param>
        /// <returns>True if the name exists in the cache; otherwise, false.</returns>
        public static bool Contains(string name)
        {
            return stealthNames.Contains(name);
        }
    }
}

using DOL.AI.Brain;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Scripts
{
    public class SpellEffectMob : GameNPC
    {
        public SpellEffectMob()
            : base()
        {
        }

        private bool IsRandom = false;
        private bool Virgin = true; //touched for the very first time ;)
        public const int INTERVAL = 2 * 1000;
        private ushort spellID = 0;
        private int random = 0;

        protected virtual int Timer(RegionTimer callingTimer)
        {
            try
            {
                if (this.Brain == null)
                {
                    return INTERVAL;
                }

                int range = ((this.Brain as StandardMobBrain).AggroRange);
                foreach (GamePlayer player in this.GetPlayersInRadius((ushort)range)) //each player that is in Aggro Range distance of the mob
                {
                    if (Name == "New Mob")
                    {
                        player.Out.SendMessage(Name + " says change my name to a spell number!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return 0;
                    }

                    if (GuildName == "random" || GuildName == "Random" || GuildName == "RANDOM")
                    {
                        IsRandom = true;
                    }
                    else
                    {
                        IsRandom = false;
                    }

                    if (IsRandom)
                    {
                        random = Util.Random(1, Level);
                    }

                    spellID = Convert.ToUInt16(Name); //Take the mob's name and convert it to a number and store that in SpellID.

                    // Uncomment the line below to see the randoms in game each time a new one is picked.
                    // player.Out.SendMessage(Name + " says my random number was " + random + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    if (IsRandom && (random == 1 || random == Level))
                    {
                        player.Out.SendSpellEffectAnimation(this, this, spellID, 0, false, 1);
                    }
                    else if (!IsRandom)
                    {
                        player.Out.SendSpellEffectAnimation(this, this, spellID, 0, false, 1);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Exception in Timer: {ex.Message}");
            }

            return INTERVAL;
        }

        public override bool AddToWorld()
        {
            bool success = base.AddToWorld();
            if (Virgin)
            {
                if (success)
                {
                    new RegionTimer(this, new RegionTimerCallback(Timer), INTERVAL);
                }
                Virgin = false; //I'm a dirty girl, this isn't the first time.
            }
            return success;
        }
    }
}

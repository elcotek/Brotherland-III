using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    class Dockmaster : GameNPC
    {

        /// <summary>
        /// Constructor
        /// </summary>
        public Dockmaster()
            : base()
        {
            Flags = eFlags.PEACE;
        }


        string Classname = "";

        public override bool Interact(GamePlayer player)
        {

            if (!base.Interact(player))
                return false;
            TurnTo(player, 10000);
            Classname = player.CharacterClass.Name;

            player.Out.SendMessage(" " + Name + " says, Greetings to you, " + Classname + ". I am " +
                "" + Name + ", one of the many dock keepers that tend the boats of Atlantis. " +
                "My job is to keep watch over Atlanean, the Sea Spray, and the Song of the Seas as they travel from here in Stygia to the docks in Volcanus. " +
                "If you wish to travel from here to Vulcanus, all you must do is board the boat.Do you [need instructions] " +
                "or would you like to hear [about the sights] along the way ? ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

            return true;
        }

        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            TurnTo(t.X, t.Y);



            switch (str)
            {
                case "need instructions":
                    {
                        t.Out.SendMessage(" " + Name + " says, For the ferry boat, a couble of different methods will work.You can double click on the boat or target it and use the Get key. You can also target the boat and use /v to get on the boat. You will be automatically removed from the boat wehn it reaches its next stop.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "about the sights":
                    {
                        t.Out.SendMessage(" " + Name + " says, I shall tell you of your trip from Stygia to [Volcanus]. When you arrive in Volcanus, speak to the dockmaster of Dockmistress thre for details of the next leg or your journey.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "Volcanus":
                    {
                        t.Out.SendMessage(" " + Name + " says, As the boat leaves Stygia, it will pass close to the entrance of the river that runs through the land of Stygia.The boat will continue to follow the coast, passing by three islands covered with the nests of crocodiles.The boat will then continue to travek into the waters of Oceanus[Anatole]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "Anatole":
                    {
                        t.Out.SendMessage(" " + Name + " says, In Oceanus Anatole, you will pass by a series of islands.The first is just an island of grass and trees.The second island you will pass is the home of statues that they say have come to life! Shortly before  you arrive in the Haven of Volcanus you will pass three islands liked together by two bridges.I have heard the island is very dangerous, but Ido not think a little danger will stop a strong " + Classname + " like you. I wish you well in your travels!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
            }
            return true;
        }
    }
}
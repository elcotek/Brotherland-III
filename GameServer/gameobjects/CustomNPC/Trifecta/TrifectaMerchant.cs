using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections;

using System.Threading;




namespace DOL.GS
{
    /// <summary>
    /// Represents an in-game merchant
    /// </summary>
    public class TrifectaMerchant : GameMerchant
    {
        private readonly static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        /// <summary>
        /// Constructor
        /// </summary>
        public TrifectaMerchant()
            : base()
        {
            Flags = eFlags.PEACE;
        }

        #region GetExamineMessages / Interact

        //Rules Message
        private const string RuleMessage = @"Trifecta is a simble game of chance using three spezial made decks of cards.
Eatch deck is based on a common theme. Wehn you are ready, i will ask you to choose a card from each
of the dree decks. For each choice you make, I will give you a proxy of that card and
 once you have selected the cards you wish for each deck, I will reshuffle
the decks and draw three cards at random. If any of your proxy cards match
what i drw, you will be given a reward voucher you can redeem for a random
fabulous prize. The more  cards you match, the better your potenzial pize will be.
 If you manage to match all three, we call that a Trifecta and we reserver
our grandest prizes for those! To start playing, simply hand me a Trifecta tiken.
 If you need more tokens, I'll be happy to sell you some! if you need quit current game  
and I'll take back the cards. There are no refunds.";


        //Prizes Message
        private const string PrizesMessage1 = @"Match one of the three cards right and you
 could win flowers for your spezial someone.You could also win a mug, pitcher,
or stein you can carry proudly! Aurulite chips and Emerald seels are also up fro grab.
All you need to do is match one choice! [2/3]";


        private const string PrizesMessage2 = @"Match two of three cards right and you might win on of your specially 
imported pet summons! we also have sappire and diamond seals and even a number of of
100 gold rent Tokens! There might even be something for your garden! [3/3]";


        private const string PrizesMessage3 = @"Match all tree cards right, and we have some special prizes!
Among them are emerals for  your weapons, but that's not all!
You might also win one of our Luminescent Stones or Juweline Pets, Zahurs Ring or Mythical equipment!";


        private const string StartPlayingMessage = @"select an object from one of the dree following categories. [Category 1]";

        private const string SelectMessage1 = @"[Albion], [Midgard] or [Hibernia]";

        private const string SelectMessage2 = @" select one of category 2: [Legion], [Golestandt], [Gjalpinulva], [Cuuldurach], [Apocalypse], [King Tuscar], [Olcasgea], [Draco], [Grand Summoner Govannon]";

        private const string SelectMessage3 = @" select one of Category 3: [Caer Benowyc], [Caer Berkstead], [Caer Erasleigh], [Caer Boldiam], [Bledmeer Faste], [Nottmoor Faste], [Blendrake Faste], [Glenlock Faste], [Dun Crauchon], [Dun Crimthain], [Dun Bolg], [Dun da Behnn]";


        static readonly string[] NPCCategory1 = { "Albion", "Midgard", "Hibernia" };
        static readonly string[] NPCCategory2 = { "Legion", "Golestandt", "Gjalpinulva", "Cuuldurach", "Apocalypse", "King Tuscar", "Olcasgea", "Draco", "Grand Summoner Govannon" };
        static readonly string[] NPCCategory3 = { "Caer Benowyc", "Caer Berkstead", "Caer Erasleigh", "Caer Boldiam", "Bledmeer Faste", "Nottmoor Faste", "Blendrake Faste", "Glenlock Faste", "Dun Crauchon", "Dun Crimthain", "Dun Bolg", "Dun da Behnn" };

        static readonly string[] PlayerLooseText = { ": You have zero. Nada. Niente. Nonearoni. The big nothing. Which happens to be exactly your price."
                , ": So you've lost. Don't worry about it.It's just gold. And it is definitely better than spending your gold on alcohol."
                , ": You know, despite your defeat, I'm sure you want to play again. I can feel your happiness turning. Really, believe me... "
                ,": Wasn't It An Exciting Game? Too bad you didn't win. Maybe, next time. Just buy another brand and try again.You know you want it. "
                ,": Oh, too bad. You have lost. It wasn't even enough for a flowerpot." };


        /// <summary>
        /// Get random Text for Category1
        /// </summary>
        /// <returns>string</returns>
        private string GetRandomFromcategory1()
        {
            string cat1;
            int number;

            number = Util.Random(0, 2);

            cat1 = NPCCategory1[number];


            return cat1;
        }

        /// <summary>
        /// Get random Text for Category2
        /// </summary>
        /// <returns>string</returns>
        private string GetRandomFromcategory2()
        {
            string cat1;
            int number;

            number = Util.Random(0, 8);

            cat1 = NPCCategory2[number];


            return cat1;
        }

        /// <summary>
        /// Get random Text for Category3
        /// </summary>
        /// <returns>string</returns>
        private string GetRandomFromcategory3()
        {
            string cat1;
            int number;

            number = Util.Random(0, 11);

            cat1 = NPCCategory3[number];


            return cat1;
        }

        /// <summary>
        /// Get random Loose Text
        /// </summary>
        /// <returns>string</returns>
        private string GetRandomLooseText()
        {
            string LooseTxt;
            int number;

            number = Util.Random(0, 4);

            LooseTxt = PlayerLooseText[number];


            return LooseTxt;
        }

        /// <summary>
        /// Adds messages to ArrayList which are sent when object is targeted
        /// </summary>
        /// <param name="player">GamePlayer that is examining this object</param>
        /// <returns>list with string messages</returns>
        public override IList GetExamineMessages(GamePlayer player)
        {
            IList list = base.GetExamineMessages(player);
            list.RemoveAt(list.Count - 1);
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.YouExamine",
                                                GetName(0, false, player.Client.Account.Language, this), GetPronoun(0, true, player.Client.Account.Language),
                                                GetAggroLevelString(player, false)));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.GetExamineMessages.RightClick"));
            return list;
        }

        /// <summary>
        /// Called when a player right clicks on the merchant
        /// </summary>
        /// <param name="player">Player that interacted with the merchant</param>
        /// <returns>True if succeeded</returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;
            TurnTo(player, 10000);
            SendMerchantWindow(player);

            player.Out.SendMessage(" " + Name + " says, Greetings! Welcome to Trifecta! Do you want to know the [Rules] of the game, see a list of potenial [Prizes] or are you ready to start [Playing]?! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

            return true;
        }

        bool payed2 = false;
        bool payed3 = false;
        bool payed4 = false;
        bool payed5 = false;


        string Category1;
        string Category2;
        string Category3;
        string CategoryNPC1;
        string CategoryNPC2;
        string CategoryNPC3;
        bool hit1 = false;
        bool hit2 = false;
        bool hit3 = false;
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            TurnTo(t.X, t.Y);



            switch (str)
            {
                case "Rules":
                    t.Out.SendMessage(" " + Name + " says, " + RuleMessage + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    break;

                case "Prizes":
                    t.Out.SendMessage(" " + Name + " says, " + PrizesMessage1 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    break;
                case "2/3":

                    t.Out.SendMessage(" " + Name + " says, " + PrizesMessage2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    break;
                case "3/3":

                    t.Out.SendMessage(" " + Name + " says, " + PrizesMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                    break;

                case "Playing":
                    {
                        InventoryItem item = t.Inventory.GetFirstItemByName("Trifecta token", eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

                        if (t != null && item != null)
                        {
                            t.Inventory.RemoveItem(item);
                            t.UpdatePlayerStatus();
                            GamePlayerUtils.SendClearPopupWindow(t);
                            t.Out.SendMessage(" " + Name + " says, " + StartPlayingMessage + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            payed2 = true;

                        }
                        else
                        {

                            t.Out.SendMessage(" " + Name + " says, buy first a token to play the Game!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            payed2 = false;
                        }
                    }
                    break;

                case "Category 1":
                    if (payed2)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage1 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed2 = false; payed3 = true;
                    }
                    break;

                case "Albion":
                    if (payed3)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC1 = GetRandomFromcategory1();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC1 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category1 = "Albion";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed3 = false; payed4 = true;
                    }
                    break;

                case "Midgard":
                    if (payed3)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC1 = GetRandomFromcategory1();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC1 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category1 = "Midgard";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed3 = false; payed4 = true;
                    }
                    break;

                case "Hibernia":
                    if (payed3)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC1 = GetRandomFromcategory1();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC1 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category1 = "Hibernia";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed3 = false; payed4 = true;
                    }
                    break;

                case "Legion":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Legion";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;

                case "Golestandt":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Golestandt";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;
                case "Gjalpinulva":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Gjalpinulva";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;
                case "Cuuldurach":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Cuuldurach";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;
                case "Apocalypse":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Apocalypse";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;
                case "King Tuscar":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "King Tuscar";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;
                case "Olcasgea":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Olcasgea";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;

                case "Draco":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Draco";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;

                case "Grand Summoner Govannon":
                    if (payed4)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC2 = GetRandomFromcategory2();
                        t.Out.SendMessage(" " + Name + " says, my Card: = " + CategoryNPC2 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category2 = "Grand Summoner Govannon";
                        t.Out.SendMessage(" " + Name + " says, " + SelectMessage3 + " ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        payed4 = false; payed5 = true;
                    }
                    break;

                case "Caer Benowyc":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Caer Benowyc";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);


                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;

                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {

                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;
                case "Caer Berkstead":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Caer Berkstead";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Caer Erasleigh":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Caer Erasleigh";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Caer Boldiam":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Caer Boldiam";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Bledmeer Faste":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Bledmeer Faste";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Nottmoor Faste":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Nottmoor Faste";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Blendrake Faste":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Blendrake Faste";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Glenlock Faste":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Glenlock Faste";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;

                case "Dun Crauchon":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Dun Crauchon";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;
                case "Dun Crimthain":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Dun Crimthain";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;
                case "Dun Bolg":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Dun Bolg";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;
                case "Dun da Behnn":
                    if (payed5)
                    {
                        GamePlayerUtils.SendClearPopupWindow(t);
                        CategoryNPC3 = GetRandomFromcategory3();
                        t.Out.SendMessage(" " + Name + " says, my Cards: = " + CategoryNPC1 + ", " + CategoryNPC2 + ", " + CategoryNPC3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        Category3 = "Dun da Behnn";
                        t.Out.SendMessage(" " + Name + " says, you select: " + Category1 + ", " + Category2 + ", " + Category3, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                        //Calculate Rewards
                        if (CategoryNPC1 == Category1)
                            hit1 = true;
                        if (CategoryNPC2 == Category2)
                            hit2 = true;
                        if (CategoryNPC3 == Category3)
                            hit3 = true;

                        if (hit1 && hit2 == false && hit3 == false || hit1 == false && hit2 && hit3 == false || hit1 == false && hit2 == false && hit3)
                        {
                            GetRewardForCategory(1, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a top prize, but you still did better than a rivet!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 == false && hit2 && hit3 || hit1 && hit2 == false && hit3 || hit1 && hit2 && hit3 == false)
                        {
                            GetRewardForCategory(2, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you didn't win a main prize, but you got one of the second best prizes!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else if (hit1 && hit2 && hit3)
                        {
                            GetRewardForCategory(3, t);
                            t.Out.SendMessage(" " + Name + " says, Congratulations  " + t.Name + ", you have won one of our main prizes, today is your day!!!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                            hit1 = false; hit2 = false; hit3 = false;
                        }
                        else
                        {
                            t.Out.SendMessage(" " + Name + " says, " + GetRandomLooseText(), eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        }
                        payed5 = false;
                    }
                    break;




                //"[Albion], [Midgard] or[Hibernia]"
                default: break;
            }
            return true;
        }

        /// <summary>
        /// Reward and Category
        /// </summary>
        /// <param name="cat">1-3</param>
        /// <param name="player"></param>
        public virtual void GetRewardForCategory(short cat, GamePlayer player)
        {
            int numberCat1 = Util.Random(1, 5);
            int numberCat2 = Util.Random(1, 25);
            int numberCat3 = Util.Random(1, 28);
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


                            ItemTemplate tgeneric0 = GameServer.Database.FindObjectByKey<ItemTemplate>("merchant_stone");
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
                        break;
                    }
            }
        }

        /// <summary>
        /// send the merchants item offer window to a player
        /// </summary>
        /// <param name="player"></param>
        public override void SendMerchantWindow(GamePlayer player)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(SendMerchantWindowCallback), player);
        }

        /// <summary>
        /// Sends merchant window from threadpool thread
        /// </summary>
        /// <param name="state">The game player to send to</param>
        protected override void SendMerchantWindowCallback(object state)
        {
            ((GamePlayer)state).Out.SendMerchantWindow(m_tradeItems, eMerchantWindowType.Normal);
        }
        #endregion

        #region Items List
        /*

        /// <summary>
        /// Items available for sale
        /// </summary>
        protected MerchantTradeItems m_tradeItems;

        /// <summary>
        /// Gets the items available from this merchant
        /// </summary>
        public MerchantTradeItems TradeItems
        {
            get { return m_tradeItems; }
            set { m_tradeItems = value; }
        }
        */
        #endregion

        #region Buy / Sell / Apparaise

        /// <summary>
        /// Called when a player buys an item
        /// </summary>
        /// <param name="player">The player making the purchase</param>
        /// <param name="item_slot">slot of the item to be bought</param>
        /// <param name="number">Number to be bought</param>
        /// <returns>true if buying is allowed, false if buying should be prevented</returns>
        public override void OnPlayerBuy(GamePlayer player, int item_slot, int number)
        {

            //Get the template
            int pagenumber = item_slot / MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
            int slotnumber = item_slot % MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

            ItemTemplate template = this.TradeItems.GetItem(pagenumber, (eMerchantWindowSlot)slotnumber);
            if (template == null) return;

            //Calculate the amout of items
            int amountToBuy = number;
            if (template.PackSize > 0)
                amountToBuy *= template.PackSize;

            if (amountToBuy <= 0) return;

            //Calculate the value of items
            long totalValue = number * template.Price;

            lock (player.Inventory)
            {

                if (player.GetCurrentMoney() < totalValue)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (!player.Inventory.AddTemplate(GameInventoryItem.Create<ItemTemplate>(template), amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                //InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, template, amountToBuy);
                //Generate the buy message
                string message;
                if (amountToBuy > 1)
                    message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.BoughtPieces", amountToBuy, template.GetName(1, false), Money.GetString(totalValue));
                else
                    message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerBuy.Bought", template.GetName(1, false), Money.GetString(totalValue));

                // Check if player has enough money and subtract the money
                if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
                {
                    throw new Exception("Money amount changed while adding items.");
                }
                //InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, totalValue);


            }
        }

        /// <summary>
        /// Called when a player sells something
        /// </summary>
        /// <param name="player">Player making the sale</param>
        /// <param name="item">The InventoryItem to be sold</param>
        /// <returns>true if selling is allowed, false if it should be prevented</returns>
        public override void OnPlayerSell(GamePlayer player, InventoryItem item)
        {
            if (item == null || player == null) return;

            //trade while crafing
            if (player.CraftTimer != null && player.CraftTimer.IsAlive)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.ReceiveTradeItemWhileCrafting.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }


            if (!item.IsDropable)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.CantBeSold"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                return;
            }

            if (!this.IsWithinRadius(player, GS.ServerProperties.Properties.WORLD_PICKUP_DISTANCE)) // tested
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.TooFarAway", GetName(0, true)), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                return;
            }

            long itemValue = OnPlayerAppraise(player, item, true);

            if (itemValue == 0)
            {
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.IsntInterested", GetName(0, true), item.GetName(0, false)), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                return;
            }

            if (player.Inventory.RemoveItem(item))
            {
                string message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.GivesYou", GetName(0, true), Money.GetString(itemValue), item.GetName(0, false));
                player.AddMoney(itemValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                //InventoryLogging.LogInventoryAction(player, this, eInventoryActionType.Merchant, item.Template, item.Count);
                //InventoryLogging.LogInventoryAction(this, player, eInventoryActionType.Merchant, itemValue);
                return;
            }
            else
                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.CantBeSold"), eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
        }

        /// <summary>
        /// Called to appraise the value of an item
        /// </summary>
        /// <param name="player">The player whose item needs appraising</param>
        /// <param name="item">The item to be appraised</param>
        /// <param name="silent"></param>
        /// <returns>The price this merchant will pay for the offered items</returns>
        public override long OnPlayerAppraise(GamePlayer player, InventoryItem item, bool silent)
        {
            if (item == null)
                return 0;

            int itemCount = Math.Max(1, item.Count);
            int packSize = Math.Max(1, item.PackSize);

            long val = item.Price * itemCount / packSize * ServerProperties.Properties.ITEM_SELL_RATIO / 100;

            if (!item.IsDropable)
            {
                val = 0;
            }

            if (!silent)
            {
                string message;
                if (val == 0)
                {
                    message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerSell.IsntInterested", GetName(0, true), item.GetName(0, false));
                }
                else
                {
                    message = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameMerchant.OnPlayerAppraise.Offers", GetName(0, true), Money.GetString(val), item.GetName(0, false));
                }
                player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
            }
            return val;
        }

        #endregion

        #region NPCTemplate
        public override void LoadTemplate(INpcTemplate template)
        {
            base.LoadTemplate(template);

            if (template != null && string.IsNullOrEmpty(template.ItemsListTemplateID) == false)
            {
                TradeItems = new MerchantTradeItems(template.ItemsListTemplateID);
            }
        }
        #endregion NPCTemplate

        #region Database

        /// <summary>
        /// Loads a merchant from the DB
        /// </summary>
        /// <param name="merchantobject">The merchant DB object</param>
        public override void LoadFromDatabase(DataObject merchantobject)
        {
            base.LoadFromDatabase(merchantobject);
            if (!(merchantobject is Mob)) return;
            Mob merchant = (Mob)merchantobject;
            if (merchant.ItemsListTemplateID != null && merchant.ItemsListTemplateID.Length > 0)
                m_tradeItems = new MerchantTradeItems(merchant.ItemsListTemplateID);
        }

        /// <summary>
        /// Saves a merchant into the DB
        /// </summary>
        public override void SaveIntoDatabase()
        {
            Mob merchant = null;
            if (InternalID != null)
                merchant = GameServer.Database.FindObjectByKey<Mob>(InternalID);
            if (merchant == null)
                merchant = new Mob();

            merchant.Name = Name;
            merchant.Guild = GuildName;
            merchant.X = X;
            merchant.Y = Y;
            merchant.Z = Z;
            merchant.Heading = Heading;
            merchant.Speed = MaxSpeedBase;
            merchant.Region = CurrentRegionID;
            merchant.Realm = (byte)Realm;
            merchant.RoamingRange = RoamingRange;
            merchant.Model = Model;
            merchant.Size = Size;
            merchant.Level = Level;
            merchant.Gender = (byte)Gender;
            merchant.Flags = (uint)Flags;
            merchant.PathID = PathID;
            merchant.PackageID = PackageID;
            merchant.OwnerID = OwnerID;

            IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
            if (aggroBrain != null)
            {
                merchant.AggroLevel = aggroBrain.AggroLevel;
                merchant.AggroRange = aggroBrain.AggroRange;
            }
            merchant.ClassType = this.GetType().ToString();
            merchant.EquipmentTemplateID = EquipmentTemplateID;
            if (m_tradeItems == null)
            {
                merchant.ItemsListTemplateID = null;
            }
            else
            {
                merchant.ItemsListTemplateID = m_tradeItems.ItemsListID;
            }

            if (InternalID == null)
            {
                GameServer.Database.AddObject(merchant);
                InternalID = merchant.ObjectId;
            }
            else
            {
                GameServer.Database.SaveObject(merchant);
            }
        }

        /// <summary>
        /// Deletes a merchant from the DB
        /// </summary>
        public override void DeleteFromDatabase()
        {
            if (InternalID != null)
            {
                Mob merchant = GameServer.Database.FindObjectByKey<Mob>(InternalID);
                if (merchant != null)
                    GameServer.Database.DeleteObject(merchant);
            }
            InternalID = null;
        }

        #endregion
    }
}







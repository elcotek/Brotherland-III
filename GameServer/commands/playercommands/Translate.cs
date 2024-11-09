/*
 * BluRaven 2/17/09
 * /Translate command.  It allows players to translate a sentance using google translation from right within the game!
 * Credit goes to: Myself for the idea.  This blog: http://blogs.msdn.com/shahpiyush/archive/2007/06/09/3188246.aspx
 * for the google translation code (which did not work and needed minor tweaking to make it work properly).
 * The dol core for slash command code and the code for preventing the command from being spammed.
 * How to use: place the script in the scripts folder and restart the server.  Go in game and type /translate
 * to get a list of language codes and an example of how to use it.
 * What to check if the script stops working: google may change in the future how it returns the results.
 * If that happens you may need to adjust the line that says "result_box dir=" and "</div>" to what it was changed to.
 * If people find this really useful, perhaps it can be adopted to automatically translate what people say in
 * /broadcast, which would be possible because we already support /language to specify our language we speak.
 * I've put the request to google in a try/catch block so that incase google dosn't reply it won't crash your server.
 * If you modify this script please post your updated version with your notes on what you changed.  Enjoy!  -BluRaven
 */


using System.Net;
using System.Text;
using System;
using System.Globalization;
using DOL.GS.PacketHandler;
using System.Collections.Generic;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&translate",
        ePrivLevel.Player,
        "Translate a sentance from a supported language to your client default language.",
        "/translate <input language> <output language> <sentance to translate>",
        "example: /translate fr en Le chat paresseux saute sur le chien dormant",
         "/translate <broad (GMS only)> <group> <say> <reset> broatcast the translated result into group, Broat, Say or Chat")]
    public class TranslateCommandHandler : AbstractCommandHandler, ICommandHandler
    {

        /// <summary>
        /// IntoBroadcast flag
        /// </summary>
        protected bool m_intoBroadcast = false;

        /// <summary>
        /// IntoBroadcast flag
        /// </summary>
        public bool IntoBroadcast
        {
            get { return m_intoBroadcast; }
            set { m_intoBroadcast = value; }
        }

        /// <summary>
        /// IntoBroadcast flag
        /// </summary>
        protected bool m_intoGroup = false;

        /// <summary>
        /// IntoBroadcast flag
        /// </summary>
        public bool IntoGroup
        {
            get { return m_intoGroup; }
            set { m_intoGroup = value; }
        }
        /// <summary>
        /// IntoBroadcast flag
        /// </summary>
        protected bool m_intoSay = false;

        /// <summary>
        /// IntoBroadcast flag
        /// </summary>
        public bool IntoSay
        {
            get { return m_intoSay; }
            set { m_intoSay = value; }
        }



        public void OnCommand(GameClient client, string[] args)
        {
            const string TRANS_TICK = "Trans_Tick";


            if (args.Length < 3)
            {
                string command = String.Empty;

                if (args.Length > 1)
                {
                    command = args[1].ToLower();
                }

                switch (command)
                {
                    case "broad":
                        {

                            if (client.Account.PrivLevel == (int)ePrivLevel.Player)
                            {
                                client.Player.Out.SendMessage("Translate to Broadcast is only avaible for the Staff!.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                                return;
                            }
                            if (IntoBroadcast == true)
                            {
                                client.Player.Out.SendMessage("Translate to Broadcast OFF.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                                IntoBroadcast = false;
                                return;
                            }
                            if (IntoBroadcast == false)
                                client.Player.Out.SendMessage("Translate to Broadcast ON.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                            IntoBroadcast = true;
                            IntoGroup = false;
                            IntoSay = false;
                            return;
                        }
                    case "group":
                        {
                            if (IntoGroup == true)
                            {
                                client.Player.Out.SendMessage("Translate to Group OFF.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                                IntoGroup = false;
                                return;
                            }
                            if (IntoGroup == false)
                                client.Player.Out.SendMessage("Translate to Group ON.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                            IntoBroadcast = false;
                            IntoSay = false;
                            IntoGroup = true;
                            return;
                        }
                    case "say":
                        {
                            if (IntoSay == true)
                            {
                                client.Player.Out.SendMessage("Translate to Say OFF.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                                IntoSay = false;
                                return;
                            }
                            if (IntoSay == false)
                                client.Player.Out.SendMessage("Translate to say ON.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                            IntoSay = true;
                            IntoBroadcast = false;
                            IntoGroup = false;
                            return;
                        }
                    case "chat":
                        {
                            IntoSay = false;
                            IntoBroadcast = false;
                            IntoGroup = false;
                            client.Player.Out.SendMessage("Translate reset to standard chat.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                            return;
                        }


                    default:
                        DisplayUsage(client);
                        DisplayLanguages(client);
                        return;
                }
            }


            if (client.Player.IsMuted)
            {
                client.Player.Out.SendMessage("You are muted. Spammers are not permitted to use the /translate command.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                return;
            }


            long TransTick = client.Player.TempProperties.getProperty(TRANS_TICK, 0);
            if (TransTick > 0 && TransTick - client.Player.CurrentRegion.Time <= 0)
            {
                client.Player.TempProperties.removeProperty(TRANS_TICK);
            }
            long changeTime = client.Player.CurrentRegion.Time - TransTick;
            if (changeTime < 3200 && TransTick > 0)
            {
                client.Player.Out.SendMessage("Please do not abuse the translate command!  Wait a while and try again.", eChatType.CT_Staff, eChatLoc.CL_ChatWindow);
                client.Player.TempProperties.setProperty(TRANS_TICK, client.Player.CurrentRegion.Time);
                return;
            }
            string message = string.Join(" ", args, 3, args.Length - 3);
            //TODO test args[1] and args[2] are valid language codes before trying to translate.
            string translated = TranslateText(message, args[1], args[2]);

            //broad must be GM
            if (IntoBroadcast == true && client.Account.PrivLevel >= (int)ePrivLevel.GM)
            {
                string realms = String.Empty;
                //marks GM as staff Member
                if (client.Account.PrivLevel >= (int)ePrivLevel.GM)
                {
                    realms = "Staff ";
                }
                client.Player.Out.SendMessage(realms + client.Player.Name + " " + translated, eChatType.CT_Broadcast, eChatLoc.CL_ChatWindow);
            }
            //group
            if (IntoGroup == true)
            {
                if ((client.Player is GamePlayer && client.Player.Group != null))
                {
                    foreach (GamePlayer players in client.Player.Group.GetPlayersInTheGroup())
                    {
                        if (players == null) continue;
                        players.Out.SendMessage(client.Player.Name + " " + translated, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
                    }
                }
                else
                {
                    client.Player.Out.SendMessage(client.Player.Name + " " + translated, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
                }
            }
            //say
            if (IntoSay == true)
            {
                client.Player.Out.SendMessage(client.Player.Name + " " + translated, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            }
            //chat
            if (IntoBroadcast == false && IntoGroup == false && IntoSay == false)
            {
                client.Player.Out.SendMessage(translated, eChatType.CT_Chat, eChatLoc.CL_ChatWindow);

            }

            client.Player.TempProperties.setProperty(TRANS_TICK, client.Player.CurrentRegion.Time);
            return;
        }

        public static string ConvertWesternEuropeanToASCII(string str)
        {
            return Encoding.ASCII.GetString(Encoding.GetEncoding(1251).GetBytes(str));
        }

        //Transforms the culture of a letter to its equivalent representation in the 0-127 ascii table, such as the letter 'é' is substituted by an 'e'
        public static string RemoveDiacritics(string s)
        {
            string normalizedString = null;
            StringBuilder stringBuilder = new StringBuilder();
            normalizedString = s.Normalize(NormalizationForm.FormD);
            int i = 0;
            char c = '\0';

            for (i = 0; i <= normalizedString.Length - 1; i++)
            {
                c = normalizedString[i];
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().ToLower();
        }

        /*
        /// <summary>
        /// Remove Diacritics from ASCII String
        /// </summary>
        /// <param name="stIn"></param>
        /// <returns></returns>
        public static string RemoveDiacritics(string stIn)
        {
            string stFormD = stIn.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }
        */
        /// <summary>
        /// Translate Text using Google Translate
        /// Google URL - http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="inlang">The two letter language code of what is being passed in.
        /// <param name="outlang">The two letter language code of what you want to get back.
        /// E.g. inlang ar and outlang en means to translate from Arabic to English</param>
        /// <returns>Translated to String</returns>
        public static string TranslateText(string input, string inlang, string outlang)
        {
           
            
            input.Replace("ö", "oe");
            input.Replace("ä", "ae");
            input.Replace("ü", "ue");
            input.Replace("ß", "ss");


            string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}|{2}", input, inlang, outlang);
            WebClient webClient = new WebClient();
            webClient.Encoding = System.Text.Encoding.UTF8;
            try
            {
                
                string newOut2 = webClient.DownloadString(url);
                string newOut = "";
                string result = newOut2;


                if (result.Contains("ì"))
                    result.Replace("ì", "i");
                    if (newOut2.Contains("è"))
                    result.Replace("è", "e");






                //find the translation result and cut off everything before it.
                result = result.Substring(result.IndexOf("this.style.backgroundColor='#fff") + 35, 500);
                //Console.WriteLine("text länge = {0}", result.Length);
                //find the end of the translation and cut off everything after it.
                result = result.Substring(0, result.IndexOf("</span"));


               
                newOut = ConvertWesternEuropeanToASCII(result);
                

                if (newOut.Contains("</span"))
                {
                    newOut.Replace("</span", "");
                }
                
                //what you should have left is the pure translation result.
                return newOut;
            }
            catch
            {
                //either google timed out, no internet connection was available, or something else went wrong.
                string newOut = "An error occurred while translating.  Translation is not available.";
                return newOut;
            }
        }


        private void DisplayUsage(GameClient client)
        {
            DisplayMessage(client, "-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
            DisplayMessage(client, "Correct usage: /translate <from code> <to code> <sentance to translate>");
            DisplayMessage(client, "common language codes: en=english, es=spanish, fr=french, de=german");
            DisplayMessage(client, "example: /translate fr en Le chat paresseux saute sur le chien dormant");
            DisplayMessage(client, "example: /translate en fr hello world");
            DisplayMessage(client, "/translate <broad (GMS only)> <group> <say> <reset>");
            DisplayMessage(client, "broatcast the translated result into group, broat, Say or Chat");
            DisplayMessage(client, "example: /translate group | /translate say | /translate Chat");
            DisplayMessage(client, "-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-");
            return;
        }
        private void DisplayLanguages(GameClient client)
        {
            List<string> text = new List<string>();
            text.Add("en=English, fi=Finnish, fr=French, de=German, el=Greek, it=Italian, es=Spanish, sv=Swedish.");
            text.Add("example: /translate en fr hello world | /translate group | /translate say | /translate Chat");
            client.Out.SendCustomTextWindow("-ALL Available LANGUAGE CODES-", text);
            return;
        }

    }

}


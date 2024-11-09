using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    public class AgramonTeleporter : GameNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override bool AddToWorld()
        {
            Level = 50;
            base.AddToWorld();
            return true;
        }
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;

            TurnTo(player.X, player.Y);



            if (player.InCombat == false)
            {
                player.Out.SendMessage(" " + Name + " Say, Welcome! Before you move on, I have a little help on your way through this world."
                 + " Please " + player.Name + " take this.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                //targetobjet for the spell
                TargetObject = player;

                if (TargetObject != null)
                {
                    CastSpell(SkillBase.GetSpellByID(GameHastener.SPEEDOFTHEREALMID), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Fronier.Hastener.Message", player.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                }
            }
            
            if (player.InCombat == false && player.Realm == eRealm.Albion)
            {
                player.Out.SendMessage("\nHello brave fighter! Where should I take you " + player.Name + " ? "
                 + "  \n[Agramon Albion Entrance], [Agramon Albion right Gate] or [Agramon Albion left Gate] ?"
                 + "  \n and for the heroic among you, [Into the heart of Agramon albion] ? Attention, you are not safe there!"
                 + "  \n or do you want to [Castle Excalibur Relic Town] or [Castle Myrddin Relic Town] to defend our relics?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else if (player.InCombat == false && player.Realm == eRealm.Hibernia)
            {
                player.Out.SendMessage("\nHello brave fighter! Where should I take you " + player.Name + " ? "
                  + "  \n[Agramon Hibernia Entrance], [Agramon Hibernia right Gate] or [Agramon Hibernia left Gate] ?"
                  + "  \n and for the heroic among you, [Into the heart of Agramon Hibernia] ? Attention, you are not safe there!"
                  + "  \n or do you want to [Dun Lamfhota Relic Town] or [Dun Dagda Relic Town] to defend our relics?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            else if (player.InCombat == false && player.Realm == eRealm.Midgard)
            {
                player.Out.SendMessage("\nHello brave fighter! Where should I take you " + player.Name + " ? "
                  + "  \n[Agramon Midgard Entrance], [Agramon Midgard right Gate] or [Agramon Midgard left Gate] ?"
                  + "  \n and for the heroic among you, [Into the heart of Agramon Midgard] ? Attention, you are not safe there!"
                  + "  \n or do you want to [Mjollner Faste Relic Town] or [Grallarhorn Faste Relic Town] to defend our relics?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            }
            return true;
        }

        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;
            //TurnTo(t.X,t.Y);
            switch (str)
            {

                //Albion
                case "Agramon Albion Entrance":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 546575, 480326, 9456, 3122);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Agramon Albion right Gate":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 545366, 470330, 9491, 3557);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Agramon Albion left Gate":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 537098, 478424, 9222, 1322);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Into the heart of Agramon albion":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 523644, 464295, 9448, 1848);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Castle Excalibur Relic Town":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 677424, 568420, 8088, 968);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;
               
                case "Castle Myrddin Relic Town":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 566140, 670841, 8088, 3160);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;



                //Hibernia
                case "Agramon Hibernia Entrance":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 506017, 477751, 9330, 3630);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Agramon Hibernia right Gate":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 513583, 476965, 9172, 2773);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Agramon Hibernia left Gate":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 507437, 470971, 9050, 2345);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Into the heart of Agramon Hibernia":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 523639, 464286, 9448, 3700);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Dun Lamfhota Relic Town":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 373895, 572819, 8040, 160);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Dun Dagda Relic Town":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 482005, 668200, 7840, 811);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;


                //Midgard
                case "Agramon Midgard Entrance":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 525469, 440069, 9440, 1382);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Agramon Midgard right Gate":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 532159, 447347, 9474, 1795);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Agramon Midgard left Gate":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 518059, 445153, 9479, 251);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Into the heart of Agramon Midgard":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 524433, 464153, 9448, 824);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Mjollner Faste Relic Town":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 597665, 305132, 8088, 1586);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                case "Grallarhorn Faste Relic Town":
                    Say("I will port you to the place of your choice!");
                    t.MoveTo(163, 700749, 418514, 8088, 2009);
                    t.BroadcastUpdate();
                    t.StartInvulnerabilityTimer(ServerProperties.Properties.TIMER_PLAYER_INIT * 1000, null);
                    break;

                default: break;
            }
            return true;
        }
        private void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
    }
}
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class ToThronAlb : GameNPC
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

            if (player.Level > 49)
            {
                TurnTo(player.X, player.Y);
            }
            else
            {
                player.Out.SendMessage("You can't port while are you no Level 50, come back if you Level 50!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                return false;
            }

            player.Out.SendMessage("" + Name + " says, Good day to you, citizen. Do you require entrance to the throne room ?" +
                " Your resonse [yes] or [no]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            return true;
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;


            switch (str)
            {
                case "no":
                    {
                        t.Out.SendMessage("" + Name + " says, I see. Safe journey to you.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }
                case "yes":
                    {
                        Say("I will port you to the throne room.");
                        GS.Spells.PortalSpell.SetDestinationAlb(394, 32329, 33114, 15901, 4091, eRealm.Albion);
                        GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                        break;
                    }


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
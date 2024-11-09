using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class RunihuraAlb : GameNPC
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
            player.Out.SendMessage("" + Name + " says, Hail there, two legger! Your kind must love diving from this cliff edge here, I see you do it all the time.What? You didn't want to dive here? Did you [fall]?", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "fall":
                    {
                        t.Out.SendMessage(" " + Name + " says, Having legs must truly be a burden. I cannot see how you function with them. A tail would surly be better. Alas, you cannot help how you were created. Well, since you are looking for a way back up, I can [help] you.", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                        break;
                    }


                case "help":
                    {
                        Say("Ranel calls forth the mighty waves of the ocean!");
                        GS.Spells.PortalSpell.SetDestinationAlb(73, 333210, 466195, 8805, 107, eRealm.Albion);
                        GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                        Say("Walk safe to legger!");
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
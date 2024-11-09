using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class ToaPorterHibernia : GameNPC
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
            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel? [Labyrinth] or [Poc]!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            return true;
        }
        public override bool WhisperReceive(GameLiving source, string str)
        {
            if (!base.WhisperReceive(source, str)) return false;
            if (!(source is GamePlayer)) return false;
            GamePlayer t = (GamePlayer)source;

            switch (str)
            {

                case "Labyrinth":
                    Say("I will port you to the place of your choice!");
                    // t.MoveTo(245, 30129, 20054, 30273, 3439);
                    GS.Spells.PortalSpell.SetDestinationHib(245, 30129, 20054, 30273, 3439, eRealm.Hibernia);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;
                case "Poc":
                    Say("I will port you to the place of your choice!");
                    //t.MoveTo(244, 47257, 48400, 17402, 959);
                    GS.Spells.PortalSpell.SetDestinationHib(244, 47257, 48400, 17402, 959, eRealm.Hibernia);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
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
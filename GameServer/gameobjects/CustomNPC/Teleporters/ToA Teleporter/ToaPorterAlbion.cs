using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS.Scripts
{
    public class ToaPorterAlbion : GameNPC
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
                    // t.MoveTo(245, 65173, 42681, 30257, 1397);
                    GS.Spells.PortalSpell.SetDestinationAlb(245, 65173, 42681, 30257, 1397, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Poc":
                    Say("I will port you to the place of your choice!");
                    //t.MoveTo(244, 27546, 51880, 18048, 1815);
                    GS.Spells.PortalSpell.SetDestinationAlb(244, 27546, 51880, 18048, 1815, eRealm.Albion);
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
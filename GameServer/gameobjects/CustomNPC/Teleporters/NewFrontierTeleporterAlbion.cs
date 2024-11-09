using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    public class NewFrontierTeleporterAlbion : GameNPC
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

            if (player.Realm != eRealm.Albion)
            {
                player.Out.SendMessage("An intruder is here, grab it!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return false;
            }

            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel? (no save Zone, use at your own risk!!) "
            + "  [Midgard Odins Tor], [Hibernia Emain Macha] or Homeland near [Caer Benowyc] or [Labyrinth], [Poc],"
            + " [Castle Sauvage Border Keep]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Midgard Odins Tor":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(163, 541215, 416818, 8214, 2192, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Hibernia Emain Macha":
                    Say("I will port you to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(163, 432000, 500143, 8560, 3622, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Caer Benowyc":
                    Say("I will port you to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(163, 579938, 512584, 8042, 1248, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Labyrinth":
                    Say("I will port you to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(245, 65173, 42681, 30257, 1397, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Poc":
                    Say("I will port you to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(244, 27546, 51880, 18048, 1815, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Thidranki":
                    Say("I will port you to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(238, 563505, 574362, 5408, 2051, eRealm.Albion);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;


                case "Castle Sauvage Border Keep":
                    Say("I will port you to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationAlb(163, 653811, 616998, 9560, 2040, eRealm.Albion);
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
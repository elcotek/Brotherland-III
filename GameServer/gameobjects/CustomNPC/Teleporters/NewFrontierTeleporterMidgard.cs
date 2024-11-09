using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    public class NewFrontierTeleporterMidgard : GameNPC
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

            if (player.Realm != eRealm.Midgard)
            {
                player.Out.SendMessage("An intruder is here, grab it!", eChatType.CT_Say, eChatLoc.CL_SystemWindow);
                return false;
            }


            player.Out.SendMessage("Stop!" + player.Name + "!Where do you want to travel? (no save Zone, use at your own risk!!)"
                + "You can choose the [Hibernia]or [Albion] or Homeland near [BledmeerFaste]or [Labyrinth], [Poc],"
                + " [Svasud Border Keep]", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
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
                case "Hibernia":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(163, 429863, 517563, 8984, 2269, eRealm.Midgard);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Albion":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(163, 566470, 514811, 8406, 2714, eRealm.Midgard);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "BledmeerFaste":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(163, 548978, 398533, 8018, 155, eRealm.Midgard);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Labyrinth":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(245, 66127, 14006, 30255, 464, eRealm.Midgard);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Poc":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(244, 53773, 25020, 16533, 2904, eRealm.Midgard);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Thidranki":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(238, 570012, 540325, 5404, 4090, eRealm.Midgard);
                    GameNPCHelper.CastSpellOnOwnerAndPets(this, t, SkillBase.GetSpellByID(5612), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells), false);
                    break;

                case "Svasud Border Keep":
                    Say("I will port you now to the place of your choice!");
                    GS.Spells.PortalSpell.SetDestinationMid(163, 651460, 313758, 9432, 1004, eRealm.Midgard);
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
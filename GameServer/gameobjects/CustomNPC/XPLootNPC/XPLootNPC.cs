/*
 * This NPC was originally written by crazys (kyle). 
 * Updated 10-23-08 by BluRaven
 * I have only changed (added) the eMLLine enum to get it to compile
 * as a standalone script with the current SVN of DOL.  Place both scripts into your scripts folder and restart your server.
 * to create in game type: /mob create DOL.GS.Scripts.MLNPC
 */

using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.ServerProperties;
using log4net;
using System.Reflection;

namespace DOL.GS
{
    public class XPLootNPC : GameNPC
    {


        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public XPLootNPC()
            : base()
        {
            Flags |= GameNPC.eFlags.PEACE;
        }
        #region Add To World
        public override bool AddToWorld()
        {
            GuildName = "XP Loot NPC";
            Level = 50;
            base.AddToWorld();
            return true;
        }
        #endregion Add To World



        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {

            GamePlayer t = source as GamePlayer;
            if (source != null && source.IsWithinRadius(this, WorldMgr.INTERACT_DISTANCE) == false)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }
            if (t != null && item != null && item.PackageID == "XPLoot")
            {

                if (t.GainXP == false)
                {
                    t.Out.SendMessage(" " + Name + " says, Your xp flag is OFF. You can not gain experience points. Use '/xp on' to start gaining experience points.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    return false;
                }

                if (t.XPStoneCount + item.Count > 20 && t.XPStoneCount < 20)
                {
                    t.Out.SendMessage(" " + Name + " says, Too mutch stones in stack, please shrink the stack of " + item.Name + " first!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                    return false;
                }

                long XPPercent = t.Experience * 1 / 100;

                if (XPPercent < 1)
                    XPPercent = 1;

                if (t.XPStoneCount < 20)
                {
                    XPPercent *= item.Count;

                    if (t.Level < 50)
                    {
                        t.Out.SendMessage("You receive " + XPPercent + " XP from the " + item.Name + "! ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        t.GainExperience(eXPSource.Player, XPPercent, 0, 0, 0, false, false, true);
                    }
                    else
                    {
                        t.Out.SendMessage(" " + Name + " says, You are over the maxlevel for my service, you will recive Champion-XP instead", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        t.Out.SendMessage("You receive " + XPPercent + " Champion-XP from the " + item.Name + "! ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                        t.GainChampionExperience(XPPercent);
                    }

                    t.Inventory.RemoveItem(item);
                    t.XPStoneCount += item.Count;
                    t.Out.SendUpdatePoints();
                    t.SaveIntoDatabase();

                    t.Out.SendMessage("The npc has received " + t.XPStoneCount + " of max 20 pieces for today ", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
                else

                    t.Out.SendMessage(" " + Name + " says, You can only receive XP from " + t.XPStoneCount + " pieces of XP Loot a day. Come back tomorrow at the same time or later!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

            }

            return base.ReceiveItem(source, item);
        }




        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            TurnTo(player, 10000);

            string msg;

            msg = " " + Name + " says, hello " + player.Name + " I'm the XP Loot Exchanger in this Town - Hand your XP loot to me and I grant you some XP.";
           
            if (player.XPStoneCount >= 20)
            {
                msg = " " + Name + " says, " + player.Name + " You can only receive XP from " + player.XPStoneCount + " pieces of XP Loot a day. Come back to me tomorrow at the same time or later!";
            }

            player.Out.SendMessage(msg, eChatType.CT_Say, eChatLoc.CL_PopupWindow);

            return true;
        }



        public void SendReply(GamePlayer target, string msg)
        {
            target.Client.Out.SendMessage(
                msg,
                eChatType.CT_Say, eChatLoc.CL_PopupWindow);
        }
    }

}
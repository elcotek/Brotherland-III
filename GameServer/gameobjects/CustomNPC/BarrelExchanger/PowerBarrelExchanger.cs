/*
 * This NPC was originally written by crazys (kyle). 
 * Updated 10-23-08 by BluRaven
 * I have only changed (added) the eMLLine enum to get it to compile
 * as a standalone script with the current SVN of DOL.  Place both scripts into your scripts folder and restart your server.
 * to create in game type: /mob create DOL.GS.Scripts.MLNPC
 */

using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;
using System.Reflection;


namespace DOL.GS
{


    public class PowerBarrelExchanger : GameNPC
    {


        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public PowerBarrelExchanger()
            : base()
        {
            Flags |= GameNPC.eFlags.PEACE;
        }
        #region Add To World
        public override bool AddToWorld()
        {
            GuildName = "Power Barrel Exchanger";
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
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (t != null && item != null && item.Name == "Elixir of Instant Power")
            {
                if (t.BarrelForPowerElixirCount >= 34)
                {
                    t.Out.SendMessage(Name + " says, You can only receive one Barrel a day. Come back tomorrow! ", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                    return false;
                }

                //34 * 3 = 102
                if (t.BarrelForPowerElixirCount + item.Count > 34 && t.BarrelForPowerElixirCount < 34)
                {
                    t.Out.SendMessage("Too mutch Elixir in stack, shrink the stack of " + item.Name + " first!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return false;
                }
                t.Inventory.RemoveItem(item);
                t.BarrelForPowerElixirCount += item.Count;
                t.SaveIntoDatabase();

                if (t.BarrelForPowerElixirCount < 34)
                {

                    t.Out.SendMessage(Name + "Receive one " + t.BarrelForPowerElixirCount + " of 34 pices of " + item.Name + "! ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    Emote(eEmote.Induct);
                    //t.Out.SendNPCsQuestEffect(this, eQuestIndicator.Finish);
                    //BroadcastUpdate();
                }
                else
                {
                    t.Out.SendMessage("You receive the Barrel of Instant Power from " + Name + " ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    t.ReceiveItem(this, "barrel_of_instant_power", 0);
                    t.UpdatePlayerStatus();
                    t.Out.SendMessage("You can only receive one Barrel a day. Come back tomorrow! ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    Emote(eEmote.Military);
                }
            }

            return base.ReceiveItem(source, item);
        }




        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            TurnTo(player, 10000);



            if (player.GetCraftingSkillValue(eCraftingSkill.Alchemy) < 1000)
            {
                player.Out.SendMessage("You must be Alchemist and must be skill 1000+ for my Service", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                Emote(eEmote.No);
                return false;
            }

            if (player.BarrelForPowerElixirCount >= 34)
            {
                player.Out.SendMessage(Name + " says, You can only receive one Barrel a day. Come back tomorrow! ", eChatType.CT_Say, eChatLoc.CL_PopupWindow);
                Emote(eEmote.No);
                return false;
            }

            string msg;

            msg = Name + " says, I'm the Barrel Exchanger.  Hand me 34 pices of (Elixir of Instant Power) to me, and I Crafting for you one Barrel." +
                " " + Name + " Has Received " + player.BarrelForPowerElixirCount + " of 34 pices of (Elixir of Instant Power) to create a Barrel for today.";

            player.Out.SendMessage(msg, eChatType.CT_Say, eChatLoc.CL_PopupWindow);
            player.Out.SendMessage(Name + " Has Received " + player.BarrelForPowerElixirCount + " of 34 pices (Elixir of Instant Power) to create a Barrel for today.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

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

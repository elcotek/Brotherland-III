/*Reworked by Tinantiol to work with the lastest SVN (2350).
 * I don't know who are/is the original autor/autors, but the 
 * credits go to them, i just take the script into User Release 
 * Section and fixed to work with the lastest SVN.
 * Please guys, Dawn Of Light is an open source project, you all 
 * are using Tortoise SVN to get the lastest version of the code
 * that DOL staff is fixing for you, so now go to upload all your
 * scripts into the User Releases Section and get DOL at the TOP!
 */

using DOL.Database;
using DOL.GS.PacketHandler;
using System;

namespace DOL.GS.Scripts
{

    public class MLRespecNPC : GameNPC
    {
        private const string REFUND_ITEM_WEAK = "refunded item";

        public override bool AddToWorld()
        {
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            switch (Realm)
            {
                case eRealm.Albion:
                    template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2230); break;
                case eRealm.Midgard:
                    template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2232);
                    template.AddNPCEquipment(eInventorySlot.ArmsArmor, 2233);
                    template.AddNPCEquipment(eInventorySlot.LegsArmor, 2234);
                    template.AddNPCEquipment(eInventorySlot.HandsArmor, 2235);
                    template.AddNPCEquipment(eInventorySlot.FeetArmor, 2236);
                    break;
                case eRealm.Hibernia:
                    template.AddNPCEquipment(eInventorySlot.TorsoArmor, 2231); ; break;
            }

            Inventory = template.CloseTemplate();
            Flags = eFlags.PEACE; // Peace flag.
            return base.AddToWorld();
        }

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer player = source as GamePlayer;

            if (player != null && source.IsWithinRadius(this, WorldMgr.INTERACT_DISTANCE) == false)
            {
                player.Out.SendMessage("You are too far away to give anything to " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }
           
            if (player != null && player.MLLevel < 10)
            {
                player.Out.SendMessage("You must first have completed all MLs in order to be able to use my service." + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }
           


            if (player != null && player.Inventory.IsSlotsFree(10, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
            {
                
            }
            else
            {
                player.Out.SendMessage("You need first ten free inventory slots so that the Respec can continue. " + GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_PopupWindow);
                return false;
            }


            if (player != null && item != null)
            {

                if ((item.Id_nb == "Star_of_Destiny") && (player.MLLevel == 10))
                {
                    player.Out.SendMessage("Excellent, you have an rare Star of Destiny.\n" +
                    "Here, please take your Tokens back!", eChatType.CT_Say, eChatLoc.CL_PopupWindow);

                    



                    player.RemoveSpellLine("ML1 Battlemaster");
                    player.RemoveSpellLine("ML2 Battlemaster");
                    player.RemoveSpellLine("ML3 Battlemaster");
                    player.RemoveSpellLine("ML4 Battlemaster");
                    player.RemoveSpellLine("ML5 Battlemaster");
                    player.RemoveSpellLine("ML6 Battlemaster");
                    player.RemoveSpellLine("ML7 Battlemaster");
                    player.RemoveSpellLine("ML8 Battlemaster");
                    player.RemoveSpellLine("ML9 Battlemaster");
                    player.RemoveSpellLine("ML10 Battlemaster");

                    player.RemoveSpellLine("ML1 Banelord");
                    player.RemoveSpellLine("ML2 Banelord");
                    player.RemoveSpellLine("ML3 Banelord");
                    player.RemoveSpellLine("ML4 Banelord");
                    player.RemoveSpellLine("ML5 Banelord");
                    player.RemoveSpellLine("ML6 Banelord");
                    player.RemoveSpellLine("ML7 Banelord");
                    player.RemoveSpellLine("ML8 Banelord");
                    player.RemoveSpellLine("ML9 Banelord");
                    player.RemoveSpellLine("ML10 Banelord");

                    player.RemoveSpellLine("ML1 Convoker");
                    player.RemoveSpellLine("ML2 Convoker");
                    player.RemoveSpellLine("ML3 Convoker");
                    player.RemoveSpellLine("ML4 Convoker");
                    player.RemoveSpellLine("ML5 Convoker");
                    player.RemoveSpellLine("ML6 Convoker");
                    player.RemoveSpellLine("ML7 Convoker");
                    player.RemoveSpellLine("ML8 Convoker");
                    player.RemoveSpellLine("ML9 Convoker");
                    player.RemoveSpellLine("ML10 Convoker");

                    player.RemoveSpellLine("ML1 Perfecter");
                    player.RemoveSpellLine("ML2 Perfecter");
                    player.RemoveSpellLine("ML3 Perfecter");
                    player.RemoveSpellLine("ML4 Perfecter");
                    player.RemoveSpellLine("ML5 Perfecter");
                    player.RemoveSpellLine("ML6 Perfecter");
                    player.RemoveSpellLine("ML7 Perfecter");
                    player.RemoveSpellLine("ML8 Perfecter");
                    player.RemoveSpellLine("ML9 Perfecter");
                    player.RemoveSpellLine("ML10 Perfecter");

                    player.RemoveSpellLine("ML1 Sojourner");
                    player.RemoveSpellLine("ML2 Sojourner");
                    player.RemoveSpellLine("ML3 Sojourner");
                    player.RemoveSpellLine("ML4 Sojourner");
                    player.RemoveSpellLine("ML5 Sojourner");
                    player.RemoveSpellLine("ML6 Sojourner");
                    player.RemoveSpellLine("ML7 Sojourner");
                    player.RemoveSpellLine("ML8 Sojourner");
                    player.RemoveSpellLine("ML9 Sojourner");
                    player.RemoveSpellLine("ML10 Sojourner");

                    player.RemoveSpellLine("ML1 Stormlord");
                    player.RemoveSpellLine("ML2 Stormlord");
                    player.RemoveSpellLine("ML3 Stormlord");
                    player.RemoveSpellLine("ML4 Stormlord");
                    player.RemoveSpellLine("ML5 Stormlord");
                    player.RemoveSpellLine("ML6 Stormlord");
                    player.RemoveSpellLine("ML7 Stormlord");
                    player.RemoveSpellLine("ML8 Stormlord");
                    player.RemoveSpellLine("ML9 Stormlord");
                    player.RemoveSpellLine("ML10 Stormlord");

                    player.RemoveSpellLine("ML1 Spymaster");
                    player.RemoveSpellLine("ML2 Spymaster");
                    player.RemoveSpellLine("ML3 Spymaster");
                    player.RemoveSpellLine("ML4 Spymaster");
                    player.RemoveSpellLine("ML5 Spymaster");
                    player.RemoveSpellLine("ML6 Spymaster");
                    player.RemoveSpellLine("ML7 Spymaster");
                    player.RemoveSpellLine("ML8 Spymaster");
                    player.RemoveSpellLine("ML9 Spymaster");
                    player.RemoveSpellLine("ML10 Spymaster");

                    player.RemoveSpellLine("ML1 Warlord");
                    player.RemoveSpellLine("ML2 Warlord");
                    player.RemoveSpellLine("ML3 Warlord");
                    player.RemoveSpellLine("ML4 Warlord");
                    player.RemoveSpellLine("ML5 Warlord");
                    player.RemoveSpellLine("ML6 Warlord");
                    player.RemoveSpellLine("ML7 Warlord");
                    player.RemoveSpellLine("ML8 Warlord");
                    player.RemoveSpellLine("ML9 Warlord");
                    player.RemoveSpellLine("ML10 Warlord");

                    player.ReceiveItem(this, "ml1token", 0);
                    player.ReceiveItem(this, "ml2token", 0);
                    player.ReceiveItem(this, "ml3token", 0);
                    player.ReceiveItem(this, "ml4token", 0);
                    player.ReceiveItem(this, "ml5token", 0);
                    player.ReceiveItem(this, "ml6token", 0);
                    player.ReceiveItem(this, "ml7token", 0);
                    player.ReceiveItem(this, "ml8token", 0);
                    player.ReceiveItem(this, "ml9token", 0);
                    player.ReceiveItem(this, "ml10token", 0);



                  
                   
                    player.Inventory.RemoveItem(item);
                    player.UpdateSpellLineLevels(true);
                    player.Out.SendUpdatePlayerSkills();
                    player.Out.SendUpdatePlayer();
                    player.UpdatePlayerStatus();
                    player.SaveIntoDatabase();
                    player.Client.Out.SendPlayerQuit(false);
                    player.Quit(true);
                    
                    player.TempProperties.setProperty(REFUND_ITEM_WEAK, new WeakRef("Star_of_Destiny"));
                }

            }
            return base.ReceiveItem(source, item);
        }

            public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                    return false;

            SayTo(player, "Hello " + player.Name + ",For this Service need you ML10 !! . Hand me an Star of Destiny and I shall remove your current ML choice. \n" +
                "I will also return your ML Tokens, you will need 10 empty inventory slots");

            return true;
        }

        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow);
        }
    }
}
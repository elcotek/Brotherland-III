/*
* Niko
* 20.03.2012 Updatet by Elcotek
*
*/
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections;
/*
namespace DOL.GS
{

   /// <summary>
   /// Represents an House VaultKeeper NPC
   /// </summary>
   [NPCGuildScript("Vault")]
   public class CoffreSummon : GameNPC
   {
       /// <summary>
       /// Constructor
       /// </summary>
       public CoffreSummon()
           : base()
       {
       }
       private void SendReply(GamePlayer target, string msg) { target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_PopupWindow); }

       public override bool AddToWorld()
       {
           Name = "Vault Keeper";
           Model = 633;
           Size = 50;
           GuildName = "Old Stonehenge";
           Level = 55;
           Realm = eRealm.None;
           Flags = eFlags.PEACE;
           return base.AddToWorld();
       }

       #region Examine Message

       /// <summary>
       /// Adds messages to ArrayList which are sent when object is targeted
       /// </summary>
       /// <param name="player">GamePlayer that is examining this object</param>
       /// <returns>list with string messages</returns>
       public override IList GetExamineMessages(GamePlayer player)
       {
           IList list = new ArrayList
           {
               LanguageMgr.GetTranslation(player.Client.Account.Language, "VaultKeeper.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)),
               LanguageMgr.GetTranslation(player.Client.Account.Language, "VaultKeeper.RightClick")
           };
           return list;
       }

       #endregion Examine Message

       #region Interact

       /// <summary>
       /// Called when a player right clicks on the vaultkeeper
       /// </summary>
       /// <param name="player">Player that interacted with the vaultkeeper</param>
       /// <returns>True if succeeded</returns>
       public override bool Interact(GamePlayer player)
       {
           if (!base.Interact(player))
               return false;
           TurnTo(player, 10000);

            if (player.CurrentRegionID == 163)
           {
               SendReply(player, "my Service don't work in New Frontiers!"
                   + " please use horses for your siege equipment transfer");
               return false;
           }

           var items = player.Inventory.GetItemRange(eInventorySlot.FirstVault, eInventorySlot.LastVault);
           player.Out.SendInventoryItemsUpdate(eInventoryWindowType.PlayerVault, items.Count > 0 ? items : null);
           return true;
       }

       #endregion examine message
   }
}
   */
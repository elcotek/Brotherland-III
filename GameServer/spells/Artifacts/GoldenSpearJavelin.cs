/*
* DAWN OF LIGHT - The first free open source DAoC server emulator
*
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
*
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
*
* You should have received a copy of the GNU General Public License
* along with this program; if not, write to the Free Software
* Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
*
*/
namespace DOL.GS.Spells
{
    using System;
    using DOL.GS.PacketHandler;
    using System.Collections;
    using Database;
    using Events;
    using DOL.Language;


    /// <summary>
    /// NOTE: PLEASE CHECK YOUR SPELL ID FOR JAVELIN OR CREATE YOUR OWN ITEM
    /// </summary>
    [SpellHandler("GoldenSpearJavelin")]
    public class GoldenSpearJavelin : SummonItemSpellHandler
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string WEAPON_ID1 = "Artef_Javelin";
		private ItemTemplate m_artefJavelin;

        public GoldenSpearJavelin(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            GamePlayer player = Caster as GamePlayer;
            
            {
                
                m_artefJavelin = GameServer.Database.FindObjectByKey<ItemTemplate>("Artef_Javelin") ?? Javelin;
                if (Caster.Inventory.GetFirstItemByID(WEAPON_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                {
                    items.Add(GameInventoryItem.Create<ItemTemplate>(m_artefJavelin));
                    //player.Inventory.RemoveItem(invItem);
                }

                else
                   //player.Out.SendMessage("you already have this Weapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client, "SpellHandler.YouAlreadyHaveThisItem"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
        }
               
            

        private ItemTemplate Javelin
        {
            get
            {
                m_artefJavelin = (ItemTemplate) GameServer.Database.FindObjectByKey<ItemTemplate>("Artef_Javelin");
                if(m_artefJavelin == null)
                {
                    if(log.IsWarnEnabled) log.Warn("Could not find Artef_Javelin, loading it ...");
                    m_artefJavelin = new ItemTemplate();
                    m_artefJavelin.Id_nb = "Artef_Javelin";
                    m_artefJavelin.Name = "Golden Javelin";
                    m_artefJavelin.Level = 50;
                    m_artefJavelin.MaxDurability = 50000;
                    m_artefJavelin.MaxCondition = 50000;
                    m_artefJavelin.Quality = 100;
                    m_artefJavelin.Object_Type = (int) eObjectType.Magical;
                    m_artefJavelin.Item_Type = 41;
                    m_artefJavelin.Model = 23;
                    m_artefJavelin.IsPickable = false;
                    m_artefJavelin.IsDropable = false;
                    m_artefJavelin.CanDropAsLoot = false;
                    m_artefJavelin.IsTradable = false;
                    m_artefJavelin.MaxCount = 1;
                    m_artefJavelin.PackSize = 1;
                    m_artefJavelin.Charges = 5;
                    m_artefJavelin.MaxCharges = 5;
                    m_artefJavelin.CanUseEvery = 30;
                    m_artefJavelin.SpellID = 38076;
                }
                return m_artefJavelin;
            }
        }

        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }

        private static void OnPlayerLeft(DOLEvent e,object sender,EventArgs arguments)
        {
            if(!(sender is GamePlayer)) 
                return;
          
            GamePlayer player = sender as GamePlayer;
            lock(player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                foreach(InventoryItem invItem in items)
                {
                    if (invItem.Id_nb.Equals("Artef_Javelin"))
                    {
                        player.Inventory.RemoveItem(invItem);
                    }
                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}
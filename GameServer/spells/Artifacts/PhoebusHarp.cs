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

    [SpellHandler("SummonPhoebusHarp")]
    public class SummonPhoebusHarp : SummonItemSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string WEAPON_ID1 = "Ethereal_Phoebus_Harp";
        private ItemTemplate m_PhoebusHarp;

        public SummonPhoebusHarp(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            GamePlayer player = Caster as GamePlayer;

            {
#pragma warning disable CS0618 // "IObjectDatabase.SelectObject<TObject>(string)" ist veraltet: "Use Parametrized Select Queries for best perfomances"
                m_PhoebusHarp = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='Ethereal_Phoebus_Harp'") ?? Light;
#pragma warning restore CS0618 // "IObjectDatabase.SelectObject<TObject>(string)" ist veraltet: "Use Parametrized Select Queries for best perfomances"
                if (Caster.Inventory.GetFirstItemByID(WEAPON_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                {

                    if (player.IsCharcterClass(eCharacterClass.Bard)|| player.IsCharcterClass(eCharacterClass.Minstrel))
                    {
                        items.Add(GameInventoryItem.Create<ItemTemplate>(m_PhoebusHarp));
                        player.Out.SendMessage("The Ethereal Harp is now in your backpack", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }
                }
            }
        }



        private ItemTemplate Light
        {
            get
            {
                m_PhoebusHarp = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("Ethereal_Phoebus_Harp");
                if (m_PhoebusHarp == null)
                {
                    //if (log.IsWarnEnabled) log.Warn("Could not find Ethereal_Phoebus_Harp, loading it ...");
                    m_PhoebusHarp = new ItemTemplate();
                    m_PhoebusHarp.Id_nb = "Ethereal_Phoebus_Harp";
                    m_PhoebusHarp.Name = "Ethereal Phoebus Harp";
                    m_PhoebusHarp.Level = 50;
                    m_PhoebusHarp.Durability = 50000;
                    m_PhoebusHarp.MaxDurability = 50000;
                    m_PhoebusHarp.Condition = 50000;
                    m_PhoebusHarp.MaxCondition = 50000;
                    m_PhoebusHarp.Quality = 100;
                    m_PhoebusHarp.DPS_AF = 4;
                    m_PhoebusHarp.SPD_ABS = 40;
                    m_PhoebusHarp.Type_Damage = 1;
                    m_PhoebusHarp.Object_Type = 45;
                    m_PhoebusHarp.Item_Type = 13;
                    m_PhoebusHarp.Hand = 1;
                    m_PhoebusHarp.Color = 0;
                    m_PhoebusHarp.Effect = 0;
                    m_PhoebusHarp.Model = 2116;
                    m_PhoebusHarp.Bonus = 35;
                    m_PhoebusHarp.Bonus1 = 2;
                    m_PhoebusHarp.Bonus2 = 2;
                    m_PhoebusHarp.Bonus3 = 2;
                    m_PhoebusHarp.Bonus4 = 3;
                    m_PhoebusHarp.Bonus5 = 0;
                    m_PhoebusHarp.Bonus1Type = 16;
                    m_PhoebusHarp.Bonus2Type = 11;
                    m_PhoebusHarp.Bonus3Type = 18;
                    m_PhoebusHarp.Bonus4Type = 176;
                    m_PhoebusHarp.Bonus5Type = 0;
                    m_PhoebusHarp.IsPickable = false;
                    m_PhoebusHarp.IsDropable = false;
                    m_PhoebusHarp.CanDropAsLoot = false;
                    m_PhoebusHarp.IsTradable = false;
                    m_PhoebusHarp.MaxCount = 1;
                    m_PhoebusHarp.PackSize = 1;
                    m_PhoebusHarp.Weight = 25;
                    m_PhoebusHarp.ProcSpellID = 0;
                    m_PhoebusHarp.PassiveSpell = 0;

                }
                return m_PhoebusHarp;
            }
        }



        public override void OnDirectEffect(GameLiving target, double effectiveness)
        {
            base.OnDirectEffect(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }

        private static void OnPlayerLeft(DOLEvent e, object sender, EventArgs arguments)
        {
            if (!(sender is GamePlayer))
                return;

            GamePlayer player = sender as GamePlayer;
            lock (player.Inventory)
            {
                var items = player.Inventory.GetItemRange(eInventorySlot.MinEquipable, eInventorySlot.LastBackpack);
                foreach (InventoryItem invItem in items)
                {
                    if (invItem.Id_nb.Equals("Ethereal_Phoebus_Harp"))
                        player.Inventory.RemoveItem(invItem);


                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}
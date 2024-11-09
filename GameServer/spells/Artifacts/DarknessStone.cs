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



    [SpellHandler("DarknessStone")]
    public class DarknessStone : SummonItemSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string WEAPON_ID1 = "darkness_Stone";
        private ItemTemplate m_DarknessStone;

        public DarknessStone(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            GamePlayer player = Caster as GamePlayer;

            {
               // m_DarknessStone = GameServer.Database.SelectObject<ItemTemplate>("`Id_nb` = @darkness_Stone", new QueryParameter("@darkness_Stone", ""+ Darkness + ""));
#pragma warning disable CS0618 // "IObjectDatabase.SelectObject<TObject>(string)" ist veraltet: "Use Parametrized Select Queries for best perfomances"
                m_DarknessStone = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='darkness_Stone'") ?? Darkness;
#pragma warning restore CS0618 // "IObjectDatabase.SelectObject<TObject>(string)" ist veraltet: "Use Parametrized Select Queries for best perfomances"

                if (Caster.Inventory.GetFirstItemByID(WEAPON_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                {
                    items.Add(GameInventoryItem.Create<ItemTemplate>(m_DarknessStone));
                    //player.Inventory.RemoveItem(invItem);
                }

                else
                    //player.Out.SendMessage("you already have this Weapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client, "SpellHandler.YouAlreadyHaveThisItem"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
        }

        private ItemTemplate Darkness
        {
            get
            {
                m_DarknessStone = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("darkness_Stone");
                if (m_DarknessStone == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find darkness_stone, loading it ...");
                    m_DarknessStone = new ItemTemplate();
                    m_DarknessStone.Id_nb = "darkness_Stone";
                    m_DarknessStone.Name = "Darkness Stone";
                    m_DarknessStone.Level = 50;
                    m_DarknessStone.Durability = 50000;
                    m_DarknessStone.MaxDurability = 50000;
                    m_DarknessStone.Condition = 50000;
                    m_DarknessStone.MaxCondition = 50000;
                    m_DarknessStone.Quality = 100;
                    m_DarknessStone.DPS_AF = 0;
                    m_DarknessStone.SPD_ABS = 0;
                    m_DarknessStone.Type_Damage = 0;
                    m_DarknessStone.Object_Type = 41;
                    m_DarknessStone.Item_Type = 29;
                    m_DarknessStone.Color = 0;
                    m_DarknessStone.Effect = 0;
                    m_DarknessStone.Model = 101;
                    m_DarknessStone.Bonus = 35;
                    m_DarknessStone.Bonus1 = 60;
                    m_DarknessStone.Bonus2 = 5;
                    m_DarknessStone.Bonus3 = 5;
                    m_DarknessStone.Bonus4 = 5;
                    m_DarknessStone.Bonus5 = 0;
                    m_DarknessStone.Bonus1Type = 10;
                    m_DarknessStone.Bonus2Type = 18;
                    m_DarknessStone.Bonus3Type = 11;
                    m_DarknessStone.Bonus4Type = 12;
                    m_DarknessStone.Bonus5Type = 0;
                    m_DarknessStone.IsPickable = false;
                    m_DarknessStone.IsDropable = false;
                    m_DarknessStone.CanDropAsLoot = false;
                    m_DarknessStone.IsTradable = false;
                    m_DarknessStone.MaxCount = 1;
                    m_DarknessStone.PackSize = 1;
                    m_DarknessStone.ProcSpellID = 0;
                    m_DarknessStone.PassiveSpell = 2299;
                }
                return m_DarknessStone;
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
                    if (invItem.Id_nb.Equals("darkness_Stone"))
                        player.Inventory.RemoveItem(invItem);


                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}
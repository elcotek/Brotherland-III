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



    [SpellHandler("LightStone")]
    public class LightStone : SummonItemSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string WEAPON_ID1 = "light_stone";
        private ItemTemplate m_LightStone;

        public LightStone(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {
            GamePlayer player = Caster as GamePlayer;

            {
#pragma warning disable CS0618 // "IObjectDatabase.SelectObject<TObject>(string)" ist veraltet: "Use Parametrized Select Queries for best perfomances"
                m_LightStone = GameServer.Database.SelectObject<ItemTemplate>("Id_nb='light_stone'") ?? Light;
#pragma warning restore CS0618 // "IObjectDatabase.SelectObject<TObject>(string)" ist veraltet: "Use Parametrized Select Queries for best perfomances"

                if (Caster.Inventory.GetFirstItemByID(WEAPON_ID1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
                {
                    items.Add(GameInventoryItem.Create<ItemTemplate>(m_LightStone));
                    //player.Inventory.RemoveItem(invItem);
                }

                else
                    //player.Out.SendMessage("you already have this Weapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client, "SpellHandler.YouAlreadyHaveThisItem"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }
        }



        private ItemTemplate Light
        {
            get
            {
                m_LightStone = (ItemTemplate)GameServer.Database.FindObjectByKey<ItemTemplate>("light_stone");
                if (m_LightStone == null)
                {
                    if (log.IsWarnEnabled) log.Warn("Could not find Light_Stone, loading it ...");
                    m_LightStone = new ItemTemplate();
                    m_LightStone.Id_nb = "light_stone";
                    m_LightStone.Name = "Light Stone";
                    m_LightStone.Level = 50;
                    m_LightStone.Durability = 50000;
                    m_LightStone.MaxDurability = 50000;
                    m_LightStone.Condition = 50000;
                    m_LightStone.MaxCondition = 50000;
                    m_LightStone.Quality = 100;
                    m_LightStone.DPS_AF = 0;
                    m_LightStone.SPD_ABS = 0;
                    m_LightStone.Type_Damage = 0;
                    m_LightStone.Object_Type = 41;
                    m_LightStone.Item_Type = 29;
                    m_LightStone.Color = 0;
                    m_LightStone.Effect = 0;
                    m_LightStone.Model = 101;
                    m_LightStone.Bonus = 35;
                    m_LightStone.Bonus1 = 60;
                    m_LightStone.Bonus2 = 5;
                    m_LightStone.Bonus3 = 5;
                    m_LightStone.Bonus4 = 5;
                    m_LightStone.Bonus5 = 0;
                    m_LightStone.Bonus1Type = 10;
                    m_LightStone.Bonus2Type = 15;
                    m_LightStone.Bonus3Type = 14;
                    m_LightStone.Bonus4Type = 16;
                    m_LightStone.Bonus5Type = 0;
                    m_LightStone.IsPickable = false;
                    m_LightStone.IsDropable = false;
                    m_LightStone.CanDropAsLoot = false;
                    m_LightStone.IsTradable = false;
                    m_LightStone.MaxCount = 1;
                    m_LightStone.PackSize = 1;
                    m_LightStone.ProcSpellID = 0;
                    m_LightStone.PassiveSpell = 22100;

                }
                return m_LightStone;
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
                    if (invItem.Id_nb.Equals("light_stone"))
                        player.Inventory.RemoveItem(invItem);


                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }
}
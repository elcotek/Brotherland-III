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
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Quests;
using NUnit.Framework;
using System;

namespace DOL.Server.Tests
{
    /// <summary>
    /// Unit Test for Money Task.
    /// </summary>
    [TestFixture]
    public class MoneyTaskTest : ServerTests
    {
        public MoneyTaskTest()
        {
        }

        [Test, Explicit]
        public void CreateMoneyTask()
        {
            GamePlayer player = CreateMockGamePlayer();

            GameMerchant merchant = new GameMerchant
            {
                Name = "Tester",
                Realm = eRealm.Albion
            };
            Console.WriteLine(player.Name);

            if (MoneyTask.CheckAvailability(player, merchant))
            {
                if (MoneyTask.BuildTask(player, merchant))
                {
                    MoneyTask questTask = (MoneyTask)player.QuestTask;


                    Assert.IsNotNull(questTask);
                    Console.WriteLine("XP" + questTask.RewardXP);
                    Console.WriteLine("Item:" + questTask.ItemName);
                    Console.WriteLine("Item:" + questTask.Name);
                    Console.WriteLine("Item:" + questTask.Description);

                    // Check Notify Event handling
                    InventoryItem item = GameInventoryItem.Create(new ItemTemplate());
                    item.Name = questTask.ItemName;

                    GameNPC npc = new GameNPC
                    {
                        Name = questTask.RecieverName
                    };
                    questTask.Notify(GamePlayerEvent.GiveItem, player, new GiveItemEventArgs(player, npc, item));

                    if (player.QuestTask.TaskActive || player.QuestTask == null)
                        Assert.Fail("Task did not finished proper in Notify");
                }
            }
        }
    }
}
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
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.ServerProperties;

namespace DOL.GS.Commands
{
    [CmdAttribute(
    "&relics",
    new string[] { "&relic" },
    ePrivLevel.Player,
    "Displays the current relic status.", "/relics")]
    public class RelicCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        /*          Relic status
         *
         * Albion Relics:
         * Strength: OwnerRealm
         * Power: OwnerRealm
         *
         * Midgard Relics:
         * Strength: OwnerRealm
         * Power: OwnerRealm
         *
         * Hibernia Relics:
         * Strength: OwnerRealm
         * Power: OwnerRealm
         *
         * Use '/realm' for Realm Info.
         */
        bool cptured = false;
        string underway = "Captured/Unerway";
        private GameRelicPad relicLocation;
        private GameRelicPad relicPad;
        public void OnCommand(GameClient client, string[] args)
        {
            if (Properties.Show_Realm == false)
            {
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "This Feature is currently disabled on the Server!"));
                return;
            }
            if (IsSpammingCommand(client.Player, "relic"))
                return;

            string albStr = "", albPwr = "", midStr = "", midPwr = "", hibStr = "", hibPwr = "";
            var relicInfo = new List<string>();

            #region Reformat Relics  '[Type]: [OwnerRealm]'

            foreach (GameRelic relic in RelicMgr.GetNFRelics())
            {
                if (relic != null)
                {
                    relicLocation = RelicMgr.GetPadAtRelicLocation(relic);
                }

                if (relicLocation != null)
                {
                    relicPad = relicLocation;
                }
                else
                    cptured = true;


                switch (relic.OriginalRealm)
                {
                    case eRealm.Albion:
                        {
                            if (relic.RelicType == eRelicType.Strength)

                                if (cptured)
                                {
                                    albStr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Strength", "Scabbard of Excalibur", underway);
                                    cptured = false;
                                }
                                else
                                    albStr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Strength", "Scabbard of Excalibur", relicPad.Name);
                            if (relic.RelicType == eRelicType.Magic)
                                if (cptured)
                                {
                                    albPwr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Power", "Merlins Staff", underway);
                                    cptured = false;
                                }
                                else
                                    albPwr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Power", "Merlins Staff", relicPad.Name);
                            break;
                        }

                    case eRealm.Midgard:
                        {
                            if (relic.RelicType == eRelicType.Strength)
                                if (cptured)
                                {
                                    midStr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Strength", "Thors Hammer", underway);
                                    cptured = false;
                                }
                                else
                                    midStr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Strength", "Thors Hammer", relicPad.Name);
                            if (relic.RelicType == eRelicType.Magic)
                                if (cptured)
                                {
                                    midPwr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Power", "Horn of Valhalla", underway);
                                    cptured = false;
                                }
                                else
                                    midPwr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Power", "Horn of Valhalla", relicPad.Name);
                            break;
                        }

                    case eRealm.Hibernia:
                        {
                            if (relic.RelicType == eRelicType.Strength)
                                if (cptured)
                                {
                                    hibStr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Strength", "Lughs Spear of Lightning", underway);
                                    cptured = false;
                                }
                                else
                                    hibStr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Strength", "Lughs Spear of Lightning", relicPad.Name);
                            if (relic.RelicType == eRelicType.Magic)
                                if (cptured)
                                {
                                    hibPwr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Power", "Cauldron of Dagda", underway);
                                    cptured = false;
                                }
                                else
                                    hibPwr = LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Power", "Cauldron of Dagda", relicPad.Name);
                            break;
                        }
                }
            }
            #endregion

            relicInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.AlbRelics") + ":");
            relicInfo.Add(albStr);
            relicInfo.Add(albPwr);
            relicInfo.Add("");
            relicInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.MidRelics") + ":");
            relicInfo.Add(midStr);
            relicInfo.Add(midPwr);
            relicInfo.Add("");
            relicInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.HibRelics") + ":");
            relicInfo.Add(hibStr);
            relicInfo.Add(hibPwr);
            relicInfo.Add("");
            relicInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.UseRealmCommand"));
            relicInfo.Add("");
            //relicInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "AntiRaidMsg2", (Properties.RAID_MEMBER_COUNT + 1)));


            client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Relic.Title"), relicInfo);
        }
    }
}

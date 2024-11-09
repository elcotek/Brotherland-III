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
using System;
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.Keeps;
using DOL.GS.ServerRules;
using DOL.GS.ServerProperties;

namespace DOL.GS.Commands
{
    [CmdAttribute(
       "&realm",
       ePrivLevel.Player,
         "Displays the current realm status.", "/realm")]
    public class RealmCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        /*          Realm status
		 *
		 * Albion Keeps:
		 * Caer Benowyc: OwnerRealm (Guild)
		 * Caer Berkstead: OwnerRealm (Guild)
		 * Caer Erasleigh: OwnerRealm (Guild)
		 * Caer Boldiam: OwnerRealm (Guild)
		 * Caer Sursbrooke: OwnerRealm (Guild)
		 * Caer Hurbury: OwnerRealm (Guild)
		 * Caer Renaris: OwnerRealm (Guild)
		 *
		 * Midgard Keeps:
		 * Bledmeer Faste: OwnerRealm (Guild)
		 * Notmoor Faste: OwnerRealm (Guild)
		 * Hlidskialf Faste: OwnerRealm (Guild)
		 * Blendrake Faste: OwnerRealm (Guild)
		 * Glenlock Faste: OwnerRealm (Guild)
		 * Fensalir Faste: OwnerRealm (Guild)
		 * Arvakr Faste: OwnerRealm (Guild)
		 *
		 * Hibernia Keeps:
		 * Dun Chrauchon: OwnerRealm (Guild)
		 * Dun Crimthainn: OwnerRealm (Guild)
		 * Dun Bolg: OwnerRealm (Guild)
		 * Dun na nGed: OwnerRealm (Guild)
		 * Dun da Behnn: OwnerRealm (Guild)
		 * Dun Scathaig: OwnerRealm (Guild)
		 * Dun Ailinne: OwnerRealm (Guild)
		 *
		 * Darkness Falls: DFOwnerRealm
		 *
		 * Type '/relic' to display the relic status.
		 */



        public void OnCommand(GameClient client, string[] args)
        {
            bool isBg = false;
            /*
            if (Properties.Show_Realm == false)
            {
                DisplayMessage(client, LanguageMgr.GetTranslation(client, "This Feature is currently disabled on the Server!"));
                return;
            }
            */

            if (IsSpammingCommand(client.Player, "realm"))
                return;

            string albKeeps1 = String.Empty;
            string midKeeps1 = String.Empty;
            string hibKeeps1 = String.Empty;
            string albKeeps2 = String.Empty;
            string midKeeps2 = String.Empty;
            string hibKeeps2 = String.Empty;
            ICollection<AbstractGameKeep> keepList = KeepMgr.GetNFKeeps();
            foreach (AbstractGameKeep keep in keepList)
            {
                if (keep is GameKeep)
                {
                    switch (keep.OriginalRealm)
                    {
                        case eRealm.Albion:
                            albKeeps1 += KeepStringBuilder(keep);
                            break;
                        case eRealm.Hibernia:
                            hibKeeps1 += KeepStringBuilder(keep);
                            break;
                        case eRealm.Midgard:
                            midKeeps1 += KeepStringBuilder(keep);
                            break;
                    }
                }
            }


            var realmInfo = new List<string>();
            realmInfo.Add("New Frontiers: ");
            realmInfo.Add(" ");
            realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.AlbKeeps") + ":");
            realmInfo.Add(albKeeps1);
            realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.MidKeeps") + ":");
            realmInfo.Add(midKeeps1);
            realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.HibKeeps") + ":");
            realmInfo.Add(hibKeeps1);


            string albBGKeeps = String.Empty;
            string midBGKeeps = String.Empty;
            string hibBGKeeps = String.Empty;
            string noneBGKeeps = String.Empty;
            ICollection<AbstractGameKeep> bgkeepList = KeepMgr.GetAllKeeps();
            foreach (AbstractGameKeep keep in bgkeepList)
            {
                if (keep.CurrentRegion.IsBG && keep.BaseLevel <= 50 && keep is GameKeep)
                {
                    switch (keep.Realm)
                    {
                        case eRealm.Albion:
                            albKeeps2 += BGKeepStringBuilder(keep);
                            break;
                        case eRealm.Hibernia:
                            hibKeeps2 += BGKeepStringBuilder(keep);
                            break;
                        case eRealm.Midgard:
                            midKeeps2 += BGKeepStringBuilder(keep);
                            break;
                        case eRealm.None:
                            noneBGKeeps += BGKeepStringBuilder(keep);
                            break;
                    }
                }
                switch (keep.Name)
                {


                    case "Caer Caledon":
                        isBg = true;
                        break;
                    case "Dun Orseo":
                        isBg = true;
                        break;
                    case "Dun Leirvik Castle":
                        isBg = true;
                        break;
                    case "Molvik Faste":
                        isBg = true;
                        break;
                    case "Caer Wilton":
                        isBg = true;
                        break;
                    case "Keep of Thidranki":
                        isBg = true;
                        break;
                    case "Dun Killaloe":
                        isBg = true;
                        break;
                    case "Caer Claret":
                        isBg = true;
                        break;
                    case "Leonis Keep":
                        isBg = true;
                        break;
                    case "Fort Brelorn":
                        isBg = true;
                        break;

                }
            }

            if (isBg)
            {
                realmInfo.Add(" ");
                realmInfo.Add("Battlegrounds: ");
                realmInfo.Add(" ");
                realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.AlbKeeps") + ":");
                realmInfo.Add(albKeeps2);
                realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.MidKeeps") + ":");
                realmInfo.Add(midKeeps2);
                realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.HibKeeps") + ":");
                realmInfo.Add(hibKeeps2);
                realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.None") + " keeps" + ":");
                realmInfo.Add(noneBGKeeps);
            }








            if (Properties.ALLOW_ALL_REALMS_DF == false)
            {
                //DFEnterJumpPoint.CheckDFOwner();
                if (DFEnterJumpPoint.DarknessFallOwner == eRealm.None)
                {
                    realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.DarknessFallsOpen"));
                }
                else
                realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.DarknessFalls") + ": " + GlobalConstants.RealmToName(DFEnterJumpPoint.DarknessFallOwner));
            }
            if (Properties.ALLOW_ALL_REALMS_DF == true)
                realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.DarknessFallsOpen"));

            realmInfo.Add(" ");
            realmInfo.Add(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Realm.UseRelicCommand"));
            client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "Scripts.Players.Realm.Title"), realmInfo);
            

        }

        private string KeepStringBuilder(AbstractGameKeep keep)
        {
            string buffer = String.Empty;
            buffer += keep.Name + ": " + GlobalConstants.RealmToName(keep.Realm);
            if (keep.Guild != null)
            {
                buffer += " (" + keep.Guild.Name + ")";
            }
            buffer += "\n";
            return buffer;
        }

        private string BGKeepStringBuilder(AbstractGameKeep keep)
        {
            string buffer = String.Empty;
            buffer += keep.Name + ": " + GlobalConstants.RealmToName(keep.Realm);
            if (keep.Guild != null)
            {
                buffer += " (" + keep.Guild.Name + ")";
            }
            buffer += "\n";
            return buffer;
        }


    }
}

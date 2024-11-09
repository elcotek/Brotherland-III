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
using System.Linq;
using System.Collections.Generic;
using DOL.Events;
using DOL.Database;

namespace DOL.GS.GameEvents
{
    public class ZonePointEffect
    {
        [ScriptLoadedEvent]
        public static void OnScriptsCompiled(DOLEvent e, object sender, EventArgs args)
        {
            // What npctemplate should we use for the zonepoint ?
            ushort model;
            NpcTemplate zp;
            try
            {
                model = (ushort)ServerProperties.Properties.ZONEPOINT_NPCTEMPLATE;

                zp = new NpcTemplate(GameServer.Database.SelectObjects<DBNpcTemplate>("`TemplateId` = @TemplateId", new QueryParameter("@TemplateId", model.ToString())).FirstOrDefault());
                if (model <= 0 || zp == null) throw new ArgumentNullException();
            }
            catch
            {
                return;
            }

            // processing all the ZP
            IList<ZonePoint> zonePoints = GameServer.Database.SelectAllObjects<ZonePoint>();
            for (int i = 0; i < zonePoints.Count; i++)
            {
                ZonePoint z = zonePoints[i];
                if (z.SourceRegion == 0)
                    continue;
                GameNPC npc = new GameNPC(zp)
                {
                    CurrentRegionID = z.SourceRegion,
                    X = z.SourceX,
                    Y = z.SourceY,
                    Z = z.SourceZ
                };

                //find region
                Region r = WorldMgr.GetRegion(z.TargetRegion);
                npc.Name = r.Description;
                if (r.IsDisabled)
                    npc.GuildName = "ZonePoint (Closed)";
                else npc.GuildName = "ZonePoint (Open)";
                npc.AddToWorld();
            }
        }
    }
}
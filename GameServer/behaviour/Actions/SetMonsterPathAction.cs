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
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.Movement;
using log4net;
using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace DOL.GS.Behaviour.Actions
{
    [ComVisible(false)]
    [ActionAttribute(ActionType = eActionType.SetMonsterPath,DefaultValueP=eDefaultValueConstants.NPC)]
    public class SetMonsterPathAction : AbstractAction<PathPoint,GameNPC>
    {

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public SetMonsterPathAction(GameNPC defaultNPC,  Object p, Object q)
            : base(defaultNPC, eActionType.SetMonsterPath, p, q)
        {                
        }


        public SetMonsterPathAction(GameNPC defaultNPC,  PathPoint firstPathPoint, GameNPC npc)
            : this(defaultNPC,  (object)firstPathPoint, (object)npc) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {
            GameNPC npc = Q;

            if (npc.Brain is RoundsBrain)
            {
                RoundsBrain brain = (RoundsBrain)npc.Brain;
                npc.CurrentWayPoint = P;
                brain.Start();                
            }
            else
            {
                if (log.IsWarnEnabled)
                    log.Warn("Mob without RoundsBrain was assigned to walk along Path");                
            }
            
        }
    }
}

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
using DOL.GS;

namespace DOL.AI.Brain
{
    public class GuardBrain : StandardMobBrain
	{
        
        public GuardBrain()
			: base()
		{
			base.ThinkInterval = 2000;
		}

		public override int AggroLevel
		{
			get { return 90; }
		}

		public override int AggroRange
		{
			get { return 750; }
		}

		protected override void CheckPlayerAggro()
		{
			//Check if we are already attacking, return if yes
			//if (Body.AttackState)	return;
			if (HasAggro) return;


			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
			{
				if (m_aggroTable.ContainsKey(player))
					continue; // add only new players
				if (!player.IsAlive || player.ObjectState != GameObject.eObjectState.Active || player.IsStealthed)
					continue;
				if (player.Steed != null)
					continue; //do not attack players on steed
				if (!GameServer.ServerRules.IsAllowedToAttack(Body, player, true))
					continue;
				if (!Body.IsWithinRadius(player, AggroRange))
					continue;

				//AddToAggroList(player, player.EffectiveLevel << 1, true);
				AddToAggroList(player, 1, true);
				return;
			}
		}

		protected override void CheckNPCAggro()
		{
			//Check if we are already attacking, return if yes
			//if (Body.AttackState)	return;

			if (HasAggro) return;


			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
			{
				if (m_aggroTable.ContainsKey(npc))
					continue; // add only new npcs
				if ((npc.Flags & GameNPC.eFlags.FLYING) != 0)
					continue; // let's not try to attack flying mobs
				if (!GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
					continue;
				if (!npc.IsWithinRadius(Body, AggroRange))
					continue;

				//AddToAggroList(npc, npc.Level << 1, true);
				if (npc.Brain is ControlledNpcBrain) // This is a pet or charmed creature, checkLOS
					AddToAggroList(npc, 1, true);
				else
					AddToAggroList(npc, 1);
				return;
			}
		}

		/// <summary>
		/// We override this because we want guards to attack even gray npcs
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public override int CalculateAggroLevelToTarget(GameLiving target)
		{
			if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
				return 0;

			return AggroLevel;
		}
	}
}

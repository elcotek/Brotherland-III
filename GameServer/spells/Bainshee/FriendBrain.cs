using DOL.GS;
using DOL.GS.Spells;

namespace DOL.AI.Brain
{
    public class FriendBrain : StandardMobBrain
	{
        readonly SpellHandler m_spellHandler = null;
		public FriendBrain(SpellHandler spellHandler) : base()
		{
			ThinkInterval = 3000;
			m_spellHandler = spellHandler;
		}

		protected override void CheckPlayerAggro()
		{
			//Check if we are already attacking, return if yes
			//if (Body.AttackState) return;

			if (HasAggro) return;

			if (m_spellHandler!=null)
			{
				foreach (GamePlayer player in Body.GetPlayersInRadius((ushort) AggroRange))
				{
					if (m_aggroTable.ContainsKey(player))
						continue; // add only new players
					if (!player.IsAlive || player.ObjectState != GameObject.eObjectState.Active || player.IsStealthed)
						continue;
					if (player.Steed != null)
						continue; //do not attack players on steed
					if(player == m_spellHandler.Caster)
						continue;
					if (!GameServer.ServerRules.IsAllowedToAttack(m_spellHandler.Caster, player, true))
						continue;



					//AddToAggroList(player, player.EffectiveLevel<<1);
					AddToAggroList(player, 1, true);

				}
			}
		}

		protected override void CheckNPCAggro()
		{
			//if (Body.AttackState) return;

			if (HasAggro) return;

			if (m_spellHandler!=null)
			{
				foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
				{
					if (GameServer.ServerRules.IsAllowedToAttack(m_spellHandler.Caster, npc, true))
					{

						if (npc.Brain is ControlledNpcBrain) // This is a pet or charmed creature, checkLOS
							AddToAggroList(npc, 1, true);
						else
							AddToAggroList(npc, 1);

					}
				}
			}
		}
	}
}

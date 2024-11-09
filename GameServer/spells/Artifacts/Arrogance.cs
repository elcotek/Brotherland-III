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
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;
using log4net;
using System.Reflection;

namespace DOL.GS.Spells.Atlantis
{
    /// <summary>
    /// Arrogance spell handler
    /// </summary>
    [SpellHandlerAttribute("Arrogance")]
    public class Arrogance : SpellHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);


        GamePlayer playertarget = null;
    	
        /// <summary>
        /// The timer that will cancel the effect
        /// </summary>
        protected RegionTimer m_expireTimer;



        public void AttackByEnemyEvent(DOLEvent e, object sender, EventArgs args)
        {
            GameLiving player = sender as GameLiving;

            if (player == null) return;
            int Damage = 0;
            AttackedByEnemyEventArgs attackArgs = args as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackArgs == null) return;
            if (living == null) return;
            {
                if (attackArgs.AttackData.DamageType == eDamageType.Crush || attackArgs.AttackData.DamageType == eDamageType.Slash || attackArgs.AttackData.DamageType == eDamageType.Thrust)
                {

                    switch (attackArgs.AttackData.AttackResult)
                    {
                        case GameLiving.eAttackResult.HitStyle:
                        case GameLiving.eAttackResult.HitUnstyled:
                            {
                                if (attackArgs.AttackData.Attacker is GamePlayer) ((GamePlayer)attackArgs.AttackData.Attacker).Out.SendMessage(string.Format("{0} is currently immune to melee damage!", attackArgs.AttackData.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

                                //Damage calculation
                                Damage = (int)Spell.Value / attackArgs.AttackData.Damage * 100;

                                if (Damage < attackArgs.AttackData.Damage)

                                attackArgs.AttackData.Damage -= Damage;
                                return;
                            }
                    }
                }
            }
        }
        

        public override void OnEffectStart(GameSpellEffect effect)
        {
            GameEventMgr.AddHandler(effect.Owner, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(AttackByEnemyEvent));
            


            base.OnEffectStart(effect);
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Dexterity] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Strength] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Constitution] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Acuity] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Piety] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Empathy] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Quickness] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Intelligence] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Charisma] += (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] += (int)m_spell.Value;                       
            
            if (effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();
            }
        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(AttackByEnemyEvent));
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Dexterity] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Strength] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Constitution] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Acuity] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Piety] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Empathy] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Quickness] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Intelligence] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.Charisma] -= (int)m_spell.Value;
            effect.Owner.BaseBuffBonusCategory[(int)eProperty.ArmorAbsorption] -= (int)m_spell.Value;
             
            if (effect.Owner is GamePlayer)
            {
            	GamePlayer player = effect.Owner as GamePlayer;
                player.Out.SendCharStatsUpdate();
                player.UpdateEncumberance();
                player.UpdatePlayerStatus();
            	player.Out.SendUpdatePlayer();  
                Start(player);
            }
            return base.OnEffectExpires(effect,noMessages);
        }

        protected virtual void Start(GamePlayer player)
        {
        	playertarget = player;
            StartTimers();
            player.DebuffCategory[(int)eProperty.Dexterity] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Strength] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Constitution] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Acuity] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Piety] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Empathy] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Quickness] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Intelligence] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.Charisma] += (int)m_spell.Value;
            player.DebuffCategory[(int)eProperty.ArmorAbsorption] += (int)m_spell.Value;
            
            player.Out.SendCharStatsUpdate();
            player.UpdateEncumberance();
            player.UpdatePlayerStatus();
          	player.Out.SendUpdatePlayer();
            MessageToLiving(Caster, Spell.Message1, eChatType.CT_Spell);
        }

        protected virtual void Stop()
        {
            if (playertarget != null)
            {     
	            playertarget.DebuffCategory[(int)eProperty.Dexterity] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Strength] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Constitution] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Acuity] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Piety] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Empathy] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Quickness] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Intelligence] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.Charisma] -= (int)m_spell.Value;;
	            playertarget.DebuffCategory[(int)eProperty.ArmorAbsorption] -= (int)m_spell.Value;;
	            
            	playertarget.Out.SendCharStatsUpdate();
            	playertarget.UpdateEncumberance();
            	playertarget.UpdatePlayerStatus();
          		playertarget.Out.SendUpdatePlayer();
                MessageToLiving(Caster, Spell.Message2, eChatType.CT_Spell);
            }
            StopTimers();
        }
        protected virtual void StartTimers()
        {
            StopTimers();
            m_expireTimer = new RegionTimer(playertarget, new RegionTimerCallback(ExpiredCallback), 10000);
        }
        protected virtual void StopTimers()
        {
            if (m_expireTimer != null)
            {
                m_expireTimer.Stop();
                m_expireTimer = null;
            }
        }
        protected virtual int ExpiredCallback(RegionTimer callingTimer)
        {
            Stop();
            return 0;
        }

        public Arrogance(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}

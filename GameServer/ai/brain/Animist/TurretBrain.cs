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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using System;

using System.Collections.Generic;

namespace DOL.AI.Brain
{
    public class TurretBrain : ControlledNpcBrain
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

      

        protected readonly List<GameLiving> m_listDefensiveTarget;

        public TurretBrain(GameLiving owner)
            : base(owner)
        {
            m_listDefensiveTarget = new List<GameLiving>();
        }

        public List<GameLiving> ListDefensiveTarget
        {
            get { return m_listDefensiveTarget; }
        }

        public override int ThinkInterval
        {
            get { return 300; }
        }
        private int _thinkCounter = 5;

        private GamePlayer playerowner = null;
        public virtual void update(GamePlayer player)
        {
            if (player != null)
            {
                if ((GameTimer.GetTickCount() - player.Client.GameObjectUpdateArray[new Tuple<ushort, ushort>(Body.CurrentRegionID, (ushort)Body.ObjectID)]) > ThinkInterval)
                {
                    player.Out.SendObjectUpdate(Body);
                }
            }
        }


       

        /// <summary>
        /// [Ganrod] Nidel:
        /// Cast only Offensive or Defensive spell.
        /// <para>If Offensive spell is true, Defensive spell isn't casted.</para>
        /// </summary>
        public override void Think()
        {

            if (playerowner == null)
                playerowner = GetPlayerOwner();

           
            if (playerowner != null)
            {
                // fast thinking part - just to turn the turret in front of its target so casting doesn't fail
                if (Body.TargetObject != null)
                    Body.TurnTo(Body.TargetObject);

                _thinkCounter++;
                if (_thinkCounter < 5)
                    return;
                _thinkCounter = 0;

                
                if (!CheckSpells(eCheckSpellType.Defensive))
                {
                    AttackMostWanted();
                }
            }
        }


        public override bool CheckSpells(eCheckSpellType type)
        {
            if (Body == null || ((TurretPet)Body).TurretSpell == null)
            {
                return false;
            }

            if (Body.IsCasting)
            {
                return true;
            }
            Spell spell = ((TurretPet)Body).TurretSpell;

            switch (type)
            {
                case eCheckSpellType.Defensive:
                    return CheckDefensiveSpells(spell);
                case eCheckSpellType.Offensive:
                    return CheckOffensiveSpells(spell);
            }
            return false;
        }

        protected override bool CheckDefensiveSpells(Spell spell)
        {
            switch (spell.SpellType)
            {
                case "HeatColdMatterBuff":
                case "BodySpiritEnergyBuff":
                case "ArmorAbsorptionBuff":
                case "AblativeArmor":
                    TrustCast(spell, eCheckSpellType.Defensive);
                    return true;
            }
            return false;
        }

        protected override bool CheckOffensiveSpells(Spell spell)
        {
            switch (spell.SpellType)
            {
                case "DirectDamageTurret":
                case "DirectDamage":
                case "DamageSpeedDecrease":
                case "SpeedDecrease":
                case "Taunt":
                case "MeleeDamageDebuff":
                case "BodyResistDebuff":
                    TrustCast(spell, eCheckSpellType.Offensive);
                    return true;
            }
            return false;
        }

        protected override void AttackMostWanted()
        {
            CheckSpells(eCheckSpellType.Offensive);
        }

        public bool TrustCast(Spell spell, eCheckSpellType type)
        {
            if (AggressionState == eAggressionState.Passive)
                return false;

            if (Body.GetSkillDisabledDuration(spell) != 0)
            {
                return false;
            }

            if (spell.Radius == 0 && spell.Range > 0)
            {
                GameLiving target;
                if (type == eCheckSpellType.Defensive)
                {
                    target = GetDefensiveTarget(spell);
                }
                else
                {
                    CheckPlayerAggro();
                    CheckNPCAggro();


                    target = CalculateNextAttackTarget();
                    
                }
                if (target != null && Body.IsWithinRadius(target, spell.Range))
                {
                    if (!Body.IsAttacking || target != Body.TargetObject)
                    {
                        Body.TargetObject = target;

                        if (spell.CastTime > 0)
                        {
                            Body.TurnTo(Body.TargetObject);
                        }
                        Body.CastSpell(spell, m_mobSpellLine, false);


                    }
                }
                else
                {
                    if (Body.IsAttacking)
                    {
                        Body.StopAttack();
                    }
                    if (Body.SpellTimer != null && Body.SpellTimer.IsAlive)
                    {
                        Body.SpellTimer.Stop();
                    }
                    return false;
                }
            }
            else //Radius spell don't need targets
            {

                return GetBuffsToCast(Body, spell);
               
            }
            return false;
        }

        public static bool GetBuffsToCast(GameNPC Body, Spell spell)
        {
            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)spell.Radius))
            {
                if (player == null)
                    continue;

                GameSpellEffect effect1 = SpellHandler.FindEffectOnTarget(player, "HeatColdMatterBuff");
                GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget(player, "BodySpiritEnergyBuff");
                GameSpellEffect effect3 = SpellHandler.FindEffectOnTarget(player, "ArmorAbsorptionBuff");
                GameSpellEffect effect4 = SpellHandler.FindEffectOnTarget(player, "AblativeArmor");
               
                if (GameServer.ServerRules.IsAllowedToAttack(Body, player, true))
                    return false;

                if (spell.SpellType == "HeatColdMatterBuff" && effect1 == null)

                    Body.CastSpell(spell, m_mobSpellLine);

                if (spell.SpellType == "BodySpiritEnergyBuff" && effect2 == null)

                    Body.CastSpell(spell, m_mobSpellLine);

                if (spell.SpellType == "ArmorAbsorptionBuff" && effect3 == null)

                    Body.CastSpell(spell, m_mobSpellLine);

                if (spell.SpellType == "AblativeArmor" && effect4 == null)

                    Body.CastSpell(spell, m_mobSpellLine);



                return true;
            }

            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Radius))
            {
                if (npc == null)
                    continue;

                GameSpellEffect effect1 = SpellHandler.FindEffectOnTarget(npc, "HeatColdMatterBuff");
                GameSpellEffect effect2 = SpellHandler.FindEffectOnTarget(npc, "BodySpiritEnergyBuff");
                GameSpellEffect effect3 = SpellHandler.FindEffectOnTarget(npc, "ArmorAbsorptionBuff");
                GameSpellEffect effect4 = SpellHandler.FindEffectOnTarget(npc, "AblativeArmor");
                if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
                    return false;

                if (spell.SpellType == "HeatColdMatterBuff" && effect1 == null)

                    Body.CastSpell(spell, m_mobSpellLine);

                if (spell.SpellType == "BodySpiritEnergyBuff" && effect2 == null)

                    Body.CastSpell(spell, m_mobSpellLine);

                if (spell.SpellType == "ArmorAbsorptionBuff" && effect3 == null)

                    Body.CastSpell(spell, m_mobSpellLine);

                if (spell.SpellType == "AblativeArmor" && effect4 == null)

                    Body.CastSpell(spell, m_mobSpellLine);
                
                return true;

            }
            return false;
        }

            /// <summary>
            /// [Ganrod] Nidel: Find and get random target in radius for Defensive spell, like 1.90 EU off servers.
            /// <para>Get target only if:</para>
            /// <para>- same realm (based on ServerRules)</para>
            /// <para>- don't have effect</para>
            /// <para>- is alive</para>
            /// </summary>
            /// <param name="spell"></param>
            /// <returns></returns>
            public GameLiving GetDefensiveTarget(Spell spell)
        {
            foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)spell.Range))
            {
                if (player == null)
                    continue;
                if (GameServer.ServerRules.IsAllowedToAttack(Body, player, true))
                    continue;

                if (!player.IsAlive)
                    continue;

                update(playerowner);

                if (LivingHasEffect(player, spell))
                {
                    if (ListDefensiveTarget.Contains(player))
                    {
                        ListDefensiveTarget.Remove(player);
                    }
                    continue;
                }

                if (player == GetPlayerOwner())
                    return player;

                ListDefensiveTarget.Add(player);
            }
            foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)spell.Range))
            {
                if (npc == null)
                    continue;
                if (GameServer.ServerRules.IsAllowedToAttack(Body, npc, true))
                    continue;

                if (!npc.IsAlive)
                    continue;

                if (LivingHasEffect(npc, spell))
                {
                    if (ListDefensiveTarget.Contains(npc))
                    {
                        ListDefensiveTarget.Remove(npc);
                    }
                    continue;
                }

                if (npc == Body)
                {
                    return Body;
                }
                if (npc == GetLivingOwner())
                    return npc;

                ListDefensiveTarget.Add(npc);
            }
            // Get one random target.
            return ListDefensiveTarget.Count > 0 ? ListDefensiveTarget[Util.Random(ListDefensiveTarget.Count - 1)] : null;
        }

        public override bool Stop()
        {
            ClearAggroList();
            ListDefensiveTarget.Clear();
            return base.Stop();
        }

        #region AI

        public override void FollowOwner()
        {
        }

        public override void Follow(GameObject target)
        {
        }

        protected override void OnFollowLostTarget(GameObject target)
        {
        }

        public override void Goto(GameObject target)
        {
        }

        public override void ComeHere()
        {
        }

        public override void Stay()
        {
        }

        #endregion
    }
}
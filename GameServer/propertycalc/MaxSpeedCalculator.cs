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
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;

namespace DOL.GS.PropertyCalc
{
    /// <summary>
    /// The Max Speed calculator
    /// 
    /// BuffBonusCategory1 unused
    /// BuffBonusCategory2 unused
    /// BuffBonusCategory3 unused
    /// BuffBonusCategory4 unused
    /// BuffBonusMultCategory1 used for all multiplicative speed bonuses
    /// </summary>
    [PropertyCalculator(eProperty.MaxSpeed)]
    public class MaxSpeedCalculator : PropertyCalculator
    {
        public static readonly double SPEED1 = 1.753;
        public static readonly double SPEED2 = 1.816;
        public static readonly double SPEED3 = 1.91;
        public static readonly double SPEED4 = 1.989;
        public static readonly double SPEED5 = 2.068;

        public override int CalcValue(GameLiving living, eProperty property)
        {
            if (living.IsMezzed || living.IsStunned) return 0;

            double speed = living.BuffBonusMultCategory1.Get((int)property);

            if (living is GamePlayer)
            {
                GamePlayer player = (GamePlayer)living;
                //				Since Dark Age of Camelot's launch, we have heard continuous feedback from our community about the movement speed in our game. The concerns over how slow
                //				our movement is has continued to grow as we have added more and more areas in which to travel. Because we believe these concerns are valid, we have decided
                //				to make a long requested change to the game, enhancing the movement speed of all players who are out of combat. This new run state allows the player to move
                //				faster than normal run speed, provided that the player is not in any form of combat. Along with this change, we have slightly increased the speed of all
                //				secondary speed buffs (see below for details). Both of these changes are noticeable but will not impinge upon the supremacy of the primary speed buffs available
                //				to the Bard, Skald and Minstrel.
                //				- The new run speed does not work if the player is in any form of combat. All combat timers must also be expired.
                //				- The new run speed will not stack with any other run speed spell or ability, except for Sprint.
                //				- Pets that are not in combat have also received the new run speed, only when they are following, to allow them to keep up with their owners.
                double horseSpeed = (player.IsOnHorse ? player.ActiveHorse.Speed * 0.01 : 1.0);
                if (speed > horseSpeed)
                    horseSpeed = 1.0;

                if (ServerProperties.Properties.ENABLE_PVE_SPEED)
                {
                    if (speed == 1 && !player.InCombat && !player.IsStealthed && !player.CurrentRegion.IsRvR)
                        speed *= 1.25; // new run speed is 125% when no buff
                }

                if (player.IsOverencumbered && player.Client.Account.PrivLevel < 2 && ServerProperties.Properties.ENABLE_ENCUMBERANCE_SPEED_LOSS)
                {
                    double Enc = player.Encumberance; // calculating player.Encumberance is a bit slow with all those locks, don't call it much
                    if (Enc > player.MaxEncumberance)
                    {
                        speed *= ((((player.MaxSpeedBase * 1.0 / GamePlayer.PLAYER_BASE_SPEED) * (-Enc)) / (player.MaxEncumberance * 0.35f)) + (player.MaxSpeedBase / GamePlayer.PLAYER_BASE_SPEED) + ((player.MaxSpeedBase / GamePlayer.PLAYER_BASE_SPEED) * player.MaxEncumberance / (player.MaxEncumberance * 0.35)));
                        if (speed <= 0)
                        {
                            speed = 0;
                            player.Out.SendMessage("You are encumbered and cannot move.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        }
                    }
                    else
                    {
                        player.IsOverencumbered = false;
                    }
                }
                if (player.IsStealthed)
                {
                    MasteryOfStealthAbility mos = player.GetAbility<MasteryOfStealthAbility>();
                    GameSpellEffect bloodrage = SpellHandler.FindEffectOnTarget(player, "BloodRage");
                    VanishEffect vanish = player.EffectList.GetOfType<VanishEffect>();

                    double stealthSpec = player.GetModifiedSpecLevel(Specs.Stealth);
                    if (stealthSpec > player.Level)
                        stealthSpec = player.Level;
                    speed *= 0.3 + (stealthSpec + 10) * 0.3 / (player.Level + 10);
                    if (vanish != null)
                        speed *= vanish.SpeedBonus;
                    if (mos != null)
                        speed *= 1 + MasteryOfStealthAbility.GetSpeedBonusForLevel(mos.Level);
                    if (bloodrage != null)
                        speed *= 1 + (bloodrage.Spell.Value * 0.01); // 25 * 0.01 = 0.25 (a.k 25%) value should be 25.

                }

                if (GameRelic.IsPlayerCarryingRelic(player))
                {
                    if (speed > 1.0)
                    {
                        speed = 1.0;
                    }

                    horseSpeed = 1.0;
                }

                if (player.IsSprinting)
                {
                    speed *= 1.3;
                }

                speed *= horseSpeed;
            }
            else if (living is GameNPC)
            {
                if (!living.InCombat)
                {
                    IControlledBrain brain = ((GameNPC)living).Brain as IControlledBrain;
                    if (brain != null)
                    {
                        GameLiving owner = brain.GetLivingOwner();
                        if (owner != null)
                        {
                            if (owner == brain.Body.CurrentFollowTarget)
                            {
                                speed *= 1.25;

                                double ownerSpeedAdjust = (double)owner.MaxSpeed / (double)GamePlayer.PLAYER_BASE_SPEED;

                                if (ownerSpeedAdjust > 1.0)
                                {
                                    speed *= ownerSpeedAdjust;
                                }

                                if (owner is GamePlayer && (owner as GamePlayer).IsOnHorse)
                                {
                                    speed *= 1.45;
                                }
                            }
                        }
                    }
                }

                double healthPercent = living.Health / (double)living.MaxHealth;
                if (healthPercent < 0.33)
                {
                    speed *= 0.2 + healthPercent * (0.8 / 0.33); //33%hp=full speed 0%hp=20%speed
                }
            }

            speed = living.MaxSpeedBase * speed + 0.5; // 0.5 is to fix the rounding error when converting to int so root results in speed 2 (191*0.01=1.91+0.5=2.41)

            GameSpellEffect iConvokerEffect = SpellHandler.FindEffectOnTarget(living, "SpeedWrap");
            if (iConvokerEffect != null && living.EffectList.GetOfType<ChargeEffect>() == null)
            {
                if (living.EffectList.GetOfType<SprintEffect>() != null && speed > 248)
                {
                    return 248;
                }
                else if (speed > 191)
                {
                    return 191;
                }
            }

            if (speed < 0)
                return 0;

            return (int)speed;
        }
    }
}

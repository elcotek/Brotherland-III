using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Effects;
using DOL.Events;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Summary description for RangeShield.
    /// </summary>
    [SpellHandlerAttribute("ValewalkerRangeShield")]
    public class ValewalkerRangeShield : BladeturnSpellHandler
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            GameEventMgr.AddHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
        }
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GameEventMgr.RemoveHandler(effect.Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnAttacked));
            return base.OnEffectExpires(effect, noMessages);
        }
        protected virtual void OnAttacked(DOLEvent e, object sender, EventArgs arguments)
        {
            AttackedByEnemyEventArgs attackedByEnemy = arguments as AttackedByEnemyEventArgs;
            //AttackedByEnemyEventArgs attackArgs = arguments as AttackedByEnemyEventArgs;
            GameLiving living = sender as GameLiving;
            if (attackedByEnemy == null) return;
            if (living == null) return;
            AttackData ad = null;
            int chance = 0;


            switch (attackedByEnemy.AttackData.AttackType)
            {
                case AttackData.eAttackType.Ranged:

                    
                        chance = (int)Spell.Value;


                        if (Util.Chance(chance))
                        {
                            switch (attackedByEnemy.AttackData.DamageType)
                            {
                                case eDamageType.Crush:
                                case eDamageType.Thrust:
                                case eDamageType.Slash:

                                   
                                        if (attackedByEnemy != null)
                                            ad = attackedByEnemy.AttackData;

                                        if (ad.Attacker.IsObjectInFront(living, 90))
                                        {
                                            if (ad.Attacker != living && ad.Attacker is GamePlayer && ad.Damage > 0)
                                            {
                                                ad.Damage = 0;
                                                ad.CriticalDamage = 0;
                                                ad.AttackResult = GameLiving.eAttackResult.Missed;
                                                GamePlayer player = ad.Attacker as GamePlayer;

                                                player.Out.SendMessage(living.Name + " is protected by his " + Spell.Name + " and absorbs your damage!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                                (living as GamePlayer).Out.SendMessage("You was protected by " + Spell.Name + " !", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                            }
                                            if (ad.Attacker is GameNPC)
                                            {
                                                ad.Damage = 0;
                                                ad.CriticalDamage = 0;
                                                ad.AttackResult = GameLiving.eAttackResult.Missed;

                                            }
                                        }
                                        break;
                           }
                    }
                    break;



                case AttackData.eAttackType.Spell:

                    if (attackedByEnemy.AttackData.SpellHandler.Spell.SpellType == "Archery")
                    {
                      
                        
                            chance = (int)Spell.Value;


                            if (Util.Chance(chance))
                            {
                                switch (attackedByEnemy.AttackData.DamageType)
                                {

                                    case eDamageType.Crush:
                                    case eDamageType.Thrust:
                                    case eDamageType.Slash:

                                                                                  if (attackedByEnemy != null)
                                                ad = attackedByEnemy.AttackData;

                                            if (living.IsObjectInFront(ad.Attacker, 90))
                                            {
                                                if (ad.Attacker != living && ad.Attacker is GamePlayer && ad.Damage > 0)
                                                {
                                                    ad.Damage = 0;
                                                    ad.CriticalDamage = 0;
                                                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                                                    GamePlayer player = ad.Attacker as GamePlayer;

                                                    player.Out.SendMessage(living.Name + " is protected by his " + Spell.Name + " and absorbs your damage!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                                    (living as GamePlayer).Out.SendMessage("You was protected by " + Spell.Name + " !", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
                                                }
                                                if (ad.Attacker is GameNPC)
                                                {

                                                    ad.Damage = 0;
                                                    ad.CriticalDamage = 0;
                                                    ad.AttackResult = GameLiving.eAttackResult.Missed;
                                                    living.TempProperties.setProperty(GameLiving.LAST_ATTACK_DATA, ad);
                                                    living.Notify(GameLivingEvent.AttackFinished, living, new AttackFinishedEventArgs(ad));
                                                }
                                            }
                                            break;
                                        
                              
                            }

                        }
                    }
                    break;
            }
        }
        public ValewalkerRangeShield(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}

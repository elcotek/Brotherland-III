
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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections.Specialized;




namespace DOL.GS.Spells
{
    /// <summary>
    /// Artifact Egg Resurection SpellHandler
    /// </summary>
    [SpellHandlerAttribute("EiResurrection")]
    public class EiRez : SpellHandler
    {
        private const string RESURRECT_CASTER_PROPERTY = "RESURRECT_CASTER";
        protected readonly ListDictionary m_resTimersByLiving = new ListDictionary();
        public const int RVRIllnes = 2435;

        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {


            foreach (GamePlayer allplayer in target.GetPlayersInRadius((ushort)m_spell.Radius))
            {

                if (allplayer == null || allplayer.Realm != Caster.Realm)
                    return;

                if (!(allplayer.IsAlive))
                {
                    allplayer.Out.SendCustomDialog("Do you allow " + m_caster.GetName(0, true) + " to resurrected you\nwith " + m_spell.ResurrectHealth + " percent hits?", new CustomDialogResponse(ResurrectResponceHandler));
                }
            }
            base.ApplyEffectOnTarget(target, effectiveness);
        }

        /// <summary>
        /// Resurrects target if it accepts
        /// </summary>
        /// <param name="player"></param>
        /// <param name="response"></param>
        protected virtual void ResurrectResponceHandler(GamePlayer player, byte response)
        {
            if (response == 1)
            {
                GameTimer resurrectExpiredTimer = null;
                lock (m_resTimersByLiving.SyncRoot)
                {
                    resurrectExpiredTimer = (GameTimer)m_resTimersByLiving[player];
                    m_resTimersByLiving.Remove(player);
                }
                if (resurrectExpiredTimer != null)
                {
                    resurrectExpiredTimer.Stop();
                }

                GameLiving rezzer = (GameLiving)player.TempProperties.getProperty<object>(RESURRECT_CASTER_PROPERTY, null);

                player.Health = player.MaxHealth * m_spell.ResurrectHealth / 100;
               
                player.Mana = player.MaxMana * m_spell.ResurrectMana / 100;
               // player.Mana = player.MaxMana;
                player.Endurance = player.MaxEndurance * m_spell.ResurrectMana / 100;
                player.MoveTo(Caster.CurrentRegionID, Caster.X, Caster.Y, Caster.Z,
                                        Caster.Heading);

                player.StopReleaseTimer();
                player.Out.SendPlayerRevive(player);
                player.Out.SendStatusUpdate();
                //player.Out.SendMessage("You have been resurrected by " + Caster.GetName(0, false) + "!",  eChatType.CT_System, eChatLoc.CL_SystemWindow);
                (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "ResurectSpellHandler.YouHaveBeenResurrectedBy", Caster.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Notify(GamePlayerEvent.Revive, player, new RevivedEventArgs(Caster, Spell));

                player.CastSpell(SkillBase.GetSpellByID(RVRIllnes), SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells));
                
                //Lifeflight add this should make it so players who have been ressurected don't take damage for 5 seconds
                RezDmgImmunityEffect rezImmune = new RezDmgImmunityEffect();
              
                rezImmune.Start(player);



            }
            else
               //player.Out.SendMessage("You decline to be resurrected.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "ResurectSpellHandler.YouDeclineToBeResurrected"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return;
        }




        // constructor
        public EiRez(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
    }
}



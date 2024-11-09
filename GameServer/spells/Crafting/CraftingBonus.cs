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
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS.Spells
{
    /// <summary>
    /// Chance to get Crafting Masterpieces
    /// </summary>
    [SpellHandlerAttribute("CraftingBetterBonus")]
    public class CraftingBetterBonus : SpellHandler
    {
        public override void OnEffectStart(GameSpellEffect effect)
        {

            base.OnEffectStart(effect);


            if (effect == null || effect.Owner == null)
            {
                effect.Cancel(false);
                return;
            }

            if (effect.Owner != null && effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CraftBonusStarts", Spell.Value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }

        }

        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            base.OnEffectExpires(effect, noMessages);

            if (effect.Owner != null && effect.Owner is GamePlayer)
            {
                GamePlayer player = effect.Owner as GamePlayer;

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CraftBonusExpire", Spell.Value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
            }


            return 0;
        }
        public CraftingBetterBonus(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }

        /// <summary>
        /// Crafting Skill Bonus
        /// </summary>
        [SpellHandlerAttribute("CraftingSkillBonus")]
        public class CraftingSkillBonus : SpellHandler
        {
            public override void OnEffectStart(GameSpellEffect effect)
            {

                base.OnEffectStart(effect);


                if (effect == null || effect.Owner == null)
                {
                    effect.Cancel(false);
                    return;
                }

                if (effect.Owner != null && effect.Owner is GamePlayer)
                {
                    GamePlayer player = effect.Owner as GamePlayer;

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CraftBonusStarts", Spell.Value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }

            }

            public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
            {
                base.OnEffectExpires(effect, noMessages);

                if (effect.Owner != null && effect.Owner is GamePlayer)
                {
                    GamePlayer player = effect.Owner as GamePlayer;

                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CraftBonusExpire", Spell.Value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }


                return 0;
            }
            public CraftingSkillBonus(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }

            /// <summary>
            /// Crafting Speed Bonus
            /// </summary>
            [SpellHandlerAttribute("CraftingSpeedBonus")]
            public class CraftingSpeedBonus : SpellHandler
            {
                public override void OnEffectStart(GameSpellEffect effect)
                {

                    base.OnEffectStart(effect);


                    if (effect == null || effect.Owner == null)
                    {
                        effect.Cancel(false);
                        return;
                    }

                    if (effect.Owner != null && effect.Owner is GamePlayer)
                    {
                        GamePlayer player = effect.Owner as GamePlayer;

                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CraftBonusStarts", Spell.Value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }

                }

                public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
                {
                    base.OnEffectExpires(effect, noMessages);

                    if (effect.Owner != null && effect.Owner is GamePlayer)
                    {
                        GamePlayer player = effect.Owner as GamePlayer;

                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CraftBonusExpire", Spell.Value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    }


                    return 0;
                }


                public CraftingSpeedBonus(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }

            }
        }
    }
}
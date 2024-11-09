using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.SkillHandler
{
    public class SpellCastingAbilityHandler : IAbilityActionHandler
    {
        protected Ability m_ability = null;
        public Ability Ability
        {
            get { return m_ability; }
        }
        public virtual int SpellID
        {
            get { return 0; }
        }

        public virtual long Preconditions
        {
            get { return 0; }
        }
        protected Spell m_spell = null;
        public virtual Spell Spell
        {
            get
            {
                if (m_spell == null)
                    m_spell = SkillBase.GetSpellByID(SpellID);
                return m_spell;
            }
        }

        protected SpellLine m_spellLine = null;
        public virtual SpellLine SpellLine
        {
            get
            {
                if (m_spellLine == null)
                    m_spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Character_Abilities);
                return m_spellLine;
            }
        }

        /// <summary>
        /// By Elcotek: Pets and NPCs don't walk/attack highter Keep/tower positions
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true/false</returns>
        public virtual bool ShadowStrikeTargetAllowedToAttack(GameObject target)
        {
            if (target != null)
            {

                var keepList = new AbstractGameKeep[KeepMgr.GetNFKeeps().Count];
                KeepMgr.GetNFKeeps().CopyTo(keepList, 0);


                //ICollection<AbstractGameKeep> keepList = KeepMgr.GetNFKeeps();

                foreach (AbstractGameKeep keep in keepList)
                {
                    if (keep is GameKeep || keep is GameKeepTower)

                        foreach (GameKeepComponent keepComponent in keep.KeepComponents)
                        {
                            if (target.IsWithinRadius(keepComponent, 1200) && keepComponent is GameKeepComponent && keepComponent != null)
                            {
                                return false;
                            }
                        }
                }
            }

            return true;
        }

        public void Execute(Ability ab, GamePlayer player)
        {
            if (player == null)
                return;

            m_ability = ab;
            if ((ab.KeyName == "Shadow Strike" || ab.KeyName == "Assassinate") && player.IsArcherClass())
                return;
            {
                if ((ab.KeyName == "Shadow Strike" || ab.KeyName == "Assassinate") && !player.IsStealthed)
                {
                    //player.Out.SendMessage("You must be stealthed to  use this ability !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouMustStealthed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (ab.KeyName == "Shadow Strike" && (ShadowStrikeTargetAllowedToAttack(player.TargetObject) == false || ShadowStrikeTargetAllowedToAttack(player) == false))
                {
                    ((GamePlayer)player).Out.SendMessage("This ability don't work in radius 1200 of Keeps or towers!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (ab.KeyName == "Shadow Strike" && ((GamePlayer)player).IsWithinRadius(player.TargetObject, 1000) == false)
                {
                    ((GamePlayer)player).Out.SendMessage("You target is out of ability range!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (ab.KeyName == "Shadow Strike" && player.TargetObject is GameNPC)
                {
                    ((GamePlayer)player).Out.SendMessage("This ability work only on enemy player's!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (ab.KeyName == "Shadow Strike" && player.GetModifiedSpecLevel(Specs.Critical_Strike) < 2)
                {
                    ((GamePlayer)player).Out.SendMessage("You need minimum spec level 2 of critical strike for this ability!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;

                }

                if (((GamePlayer)player).AttackWeapon == null)
                {
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "GamePlayer.StartAttack.CannotWithoutWeapon"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
                    return;
                }

                if (CheckPreconditions(player, Preconditions))
                {
                    if (ab.KeyName == "Shadow Strike")
                    {
                        ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.WaitFor"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                    return;
                }
                if (ab.KeyName == "Shadow Strike")
                {
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.WaitForStart"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

                }
                if (player.IsCasting && player.IsCastingRealmAbility == false)
                {
                    ((GamePlayer)player).Out.SendMessage("You can't cast a ability while cast another spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
                    return;
                }
                if (SpellLine != null && Spell != null)
                {

                    player.CastSpell(this);
                }
            }
        }


        /// <summary>
        /// Checks for any of the given conditions and returns true if there was any
        /// prints messages
        /// </summary>
        /// <param name="living"></param>
        /// <param name="bitmask"></param>
        /// <returns></returns>
        public virtual bool CheckPreconditions(GameLiving living, long bitmask)
        {
            GamePlayer player = living as GamePlayer;
            if ((bitmask & DEAD) != 0 && !living.IsAlive && Spell.SpellType != "PveResurrectionIllness")
            {
                if (player != null)
                {
                    //player.Out.SendMessage("You cannot use this ability while dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.WileDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if ((bitmask & MEZZED) != 0 && living.IsMezzed)
            {
                if (player != null)
                {
                    //player.Out.SendMessage("You cannot use this ability while mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.WileMessmerize"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if ((bitmask & STUNNED) != 0 && living.IsStunned)
            {
                if (player != null)
                {
                    //player.Out.SendMessage("You cannot use this ability while stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.WileStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if ((bitmask & SITTING) != 0 && living.IsSitting)
            {
                if (player != null)
                {
                    //player.Out.SendMessage("You cannot use this ability while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.WileSitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if ((bitmask & INCOMBAT) != 0 && living.InCombat)
            {
                if (player != null)
                {
                    //player.Out.SendMessage("You have been in combat recently and cannot use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouAreInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if ((bitmask & NOTINCOMBAT) != 0 && !living.InCombat)
            {
                if (player != null)
                {
                    //player.Out.SendMessage("You must be in combat recently to use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouMustInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if ((bitmask & STEALTHED) != 0 && living.IsStealthed)
            {
                if (player != null)
                {
                    ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouCantUseInStalth"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    //player.Out.SendMessage("You cannot use this ability while stealthed!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                }
                return true;
            }
            if (player != null && (bitmask & NOTINGROUP) != 0 && player.Group == null)
            {
                //player.Out.SendMessage("You must be in a group use this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouNeedAGroup"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return true;
            }
            if (player != null && (bitmask & TARGET) != 0 && player.TargetObject == null)
            {
                //player.Out.SendMessage("You need a target for this ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                ((GamePlayer)player).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)player).Client.Account.Language, "SpellCastingAbilityHandler.SendCastMessage.YouNeedATarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return true;
            }
            return false;
        }

        /*
		 * Stored in hex, different values in binary
		 * e.g.
		 * 16|8|4|2|1
		 * ----------
		 * 1
		 * 0|0|0|0|1 stored as 0x00000001
		 * 2
		 * 0|0|0|1|0 stored as 0x00000002
		 * 4
		 * 0|0|1|0|0 stored as 0x00000004
		 * 8
		 * 0|1|0|0|0 stored as 0x00000008
		 * 16
		 * 1|0|0|0|0 stored as 0x00000010
		 */
        public const long DEAD = 0x00000001;
        public const long SITTING = 0x00000002;
        public const long MEZZED = 0x00000004;
        public const long STUNNED = 0x00000008;
        public const long INCOMBAT = 0x00000010;
        public const long NOTINCOMBAT = 0x00000020;
        public const long NOTINGROUP = 0x00000040;
        public const long STEALTHED = 0x000000080;
        public const long TARGET = 0x000000100;
    }
}

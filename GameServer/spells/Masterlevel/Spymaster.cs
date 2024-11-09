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
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.Language;
using System;
using System.Collections.Generic;

namespace DOL.GS.Spells
{
    //http://www.camelotherald.com/masterlevels/ma.php?ml=Spymaster
    #region Spymaster-1
    //AbstractServerRules OnPlayerKilled
    #endregion

    #region Spymaster-2
    [SpellHandlerAttribute("Decoy")]
	public class DecoySpellHandler : SpellHandler
	{
		private GameDecoy decoy;
		private GameSpellEffect m_effect;
		/// <summary>
		/// Execute Decoy summon spell
		/// </summary>
		/// <param name="target"></param>
		public override void FinishSpellCast(GameLiving target)
		{
			m_caster.Mana -= PowerCost(target);
			base.FinishSpellCast(target);
		}
		public override bool IsOverwritable(GameSpellEffect compare)
		{
			return false;
		}

		public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
		{
			GameSpellEffect neweffect = CreateSpellEffect(target, effectiveness);
			decoy.AddToWorld();
			neweffect.Start(decoy);
		}

		public override void OnEffectStart(GameSpellEffect effect)
		{
			base.OnEffectStart(effect);
			m_effect = effect;
			if (effect.Owner == null || !effect.Owner.IsAlive)
				return;
			GameEventMgr.AddHandler(decoy, GameLivingEvent.Dying, new DOLEventHandler(DecoyDied));
		}
		public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
		{
			GameEventMgr.RemoveHandler(decoy, GameLivingEvent.Dying, new DOLEventHandler(DecoyDied));
			if (decoy != null)
			{
				decoy.Health = 0;
				decoy.Delete();
			}
			return base.OnEffectExpires(effect, noMessages);
		}
		private void DecoyDied(DOLEvent e, object sender, EventArgs args)
		{
			GameNPC kDecoy = sender as GameNPC;
			if (kDecoy == null) return;
			if (e == GameLivingEvent.Dying)
			{
				MessageToCaster("Your Decoy has fallen!", eChatType.CT_SpellExpires);
				OnEffectExpires(m_effect, true);
				return;
			}
		}
		public DecoySpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			Random m_rnd = new Random();
			decoy = new GameDecoy();
			//Fill the object variables
			decoy.CurrentRegion = caster.CurrentRegion;
			decoy.Heading = (ushort)((caster.Heading + 2048) % 4096);
			decoy.Level = 50;
			decoy.Realm = caster.Realm;
			decoy.X = caster.X;
			decoy.Y = caster.Y;
			decoy.Z = caster.Z;
			string TemplateId = "";
			switch (caster.Realm)
			{
				case eRealm.Albion:
					decoy.Name = "Avalonian Unicorn Knight";
					decoy.Model = (ushort)m_rnd.Next(61, 68);
					TemplateId = "e3ead77b-22a7-4b7d-a415-92a29295dcf7";
					break;
				case eRealm.Midgard:
					decoy.Name = "Kobold Elding Herra";
					decoy.Model = (ushort)m_rnd.Next(169, 184);
					TemplateId = "ee137bff-e83d-4423-8305-8defa2cbcd7a";
					break;
				case eRealm.Hibernia:
					decoy.Name = "Elf Gilded Spear";
					decoy.Model = (ushort)m_rnd.Next(334, 349);
					TemplateId = "a4c798a2-186a-4bda-99ff-ccef228cb745";
					break;
			}
			GameNpcInventoryTemplate load = new GameNpcInventoryTemplate();
			if (load.LoadFromDatabase(TemplateId))
			{
				decoy.EquipmentTemplateID = TemplateId;
				decoy.Inventory = load;
				decoy.UpdateNPCEquipmentAppearance();
			}
			decoy.CurrentSpeed = 0;
			decoy.GuildName = String.Empty;
		}
	}
	#endregion

	#region Spymaster-3
	//Gameliving - StartWeaponMagicalEffect
	#endregion

	#region Spymaster-4
	[SpellHandlerAttribute("Sabotage")]
	public class SabotageSpellHandler : SpellHandler
	{
		public override void OnDirectEffect(GameLiving target, double effectiveness)
		{
			base.OnDirectEffect(target, effectiveness);
			if (target is GameFont)
			{
				GameFont targetFont = target as GameFont;
				targetFont.Delete();
				MessageToCaster("Selected ward has been saboted!", eChatType.CT_SpellResisted);
			}
		}

		public SabotageSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
	}
	#endregion

    //shared timer 1
	#region Spymaster-5
	[SpellHandlerAttribute("TangleSnare")]
	public class TangleSnareSpellHandler : MineSpellHandler
	{
		// constructor
		public TangleSnareSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			Unstealth = false;

			//Construct a new mine.
			mine = new GameMine();
			mine.Model = 2592;
			mine.Name = spell.Name;
			mine.Realm = caster.Realm;
			mine.X = caster.X;
			mine.Y = caster.Y;
			mine.Z = caster.Z;
			mine.CurrentRegionID = caster.CurrentRegionID;
			mine.Heading = caster.Heading;
			mine.Owner = (GamePlayer)caster;

			// Construct the mine spell
			dbs = new DBSpell();
			dbs.Name = spell.Name;
            dbs.Description = "Rune that snares enemies when it detonates. Breaks stealth when cast.";
            dbs.Icon = 7220;
			dbs.ClientEffect = 7220;
			dbs.Damage = spell.Damage;
			dbs.DamageType = (int)spell.DamageType;
			dbs.Target = "Enemy";
			dbs.Radius = 0;
			dbs.Type = "SpeedDecrease";
			dbs.Value = spell.Value;
			dbs.Duration = spell.ResurrectHealth;
			dbs.Frequency = spell.ResurrectMana;
			dbs.Pulse = 0;
			dbs.PulsePower = 0;
			dbs.LifeDrainReturn = spell.LifeDrainReturn;
			dbs.Power = 0;
			dbs.CastTime = 0;
			dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
			sRadius = 350;
			s = new Spell(dbs, 1);
			sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
			trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
		}
	}
	#endregion

    //shared timer 1
	#region Spymaster-6
	[SpellHandlerAttribute("PoisonSpike")]
	public class PoisonSpikeSpellHandler : MineSpellHandler
	{
		// constructor
		public PoisonSpikeSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			Unstealth = false;

			//Construct a new font.
			mine = new GameMine();
			mine.Model = 2589;
			mine.Name = spell.Name;
            mine.Realm = caster.Realm;
			mine.X = caster.X;
			mine.Y = caster.Y;
			mine.Z = caster.Z;
			mine.CurrentRegionID = caster.CurrentRegionID;
			mine.Heading = caster.Heading;
			mine.Owner = (GamePlayer)caster;

			// Construct the mine spell
			dbs = new DBSpell();
			dbs.Name = spell.Name;
			dbs.Icon = 7281;
			dbs.ClientEffect = 7281;
			dbs.Damage = spell.Damage;
			dbs.DamageType = (int)spell.DamageType;
            dbs.Description = "Rune that poisons enemies when it detonates. Breaks stealth when cast.";
            dbs.Target = "Enemy";
			dbs.Radius = 350;
			dbs.Type = "PoisonspikeDot";
			dbs.Value = spell.Value;
			dbs.Duration = spell.ResurrectHealth;
			dbs.Frequency = spell.ResurrectMana;
			dbs.Pulse = 0;
			dbs.PulsePower = 0;
			dbs.LifeDrainReturn = spell.LifeDrainReturn;
			dbs.Power = 0;
			dbs.CastTime = 0;
			dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
			sRadius = 350;
			s = new Spell(dbs, 1);
			sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
			trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
		}
	}
	#region Subspell
	[SpellHandlerAttribute("PoisonspikeDot")]
	public class Spymaster6DotHandler : DoTSpellHandler
	{
        
        
        /// <summary>
        /// No variance for DOT spells
        /// </summary>
        /// <param name="target"></param>
        /// <param name="distance"></param>
        /// <param name="radius"></param>
        /// <returns></returns>
        protected override double CalculateAreaVariance(GameLiving target, int distance, int radius)
        {
            return 0;
        }

		// constructor
		public Spymaster6DotHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }
		public override int CalculateSpellResistChance(GameLiving target) { return 0; }
		protected override GameSpellEffect CreateSpellEffect(GameLiving target, double effectiveness)
		{
			return new GameSpellEffect(this, m_spell.Duration, m_spellLine.IsBaseLine ? 5000 : 4000, effectiveness);
		}
	}
    #endregion
    #endregion



    #region Spymaster-7
    [SpellHandler("Loockout")]
    public class LoockoutSpellHandler : MasterlevelHandling
    {
        public LoockoutSpellHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        private GameSpellEffect m_effect;


        public override bool CheckBeginCast(GameLiving selectedTarget)
        {
            if (!(selectedTarget is GamePlayer)) return false;
            if (!selectedTarget.IsSitting) { MessageToCaster("Target must be sitting!", eChatType.CT_System); return false; }

            return base.CheckBeginCast(selectedTarget);
        }

        public override void OnEffectStart(GameSpellEffect effect)
        {
            base.OnEffectStart(effect);
            m_effect = effect;
            if (effect.Owner is GamePlayer)
            {
                GamePlayer playerTarget = effect.Owner as GamePlayer;
                new LoockoutOwner().Start(Caster);
                if (effect.Owner != Caster)
                {

                    GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler(playerTarget, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAction));
                }
            }
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {

            if (effect.Owner != Caster && effect.Owner is GamePlayer)
            {

                //effect.Owner.BuffBonusCategory1[(int)eProperty.Skill_Stealth] -= 80;
                GamePlayer playerTarget = effect.Owner as GamePlayer;
                GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler(playerTarget, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAction));
                //playerTarget.Stealth(false);


                GameSpellEffect effect11 = SpellHandler.FindEffectOnTarget(playerTarget, "Loockout");
                if (effect11 != null)
                    effect11.Cancel(false);
                IGameEffect effect12 = FindStaticEffectOnTarget(playerTarget, typeof(LoockoutOwner));
                if (effect12 != null)
                    effect12.Cancel(false);

                GameSpellEffect effect13 = SpellHandler.FindEffectOnTarget(Caster, "Loockout");
                if (effect13 != null)
                    effect13.Cancel(false);
                IGameEffect effect15 = FindStaticEffectOnTarget(Caster, typeof(LoockoutOwner));
                if (effect15 != null)
                    effect15.Cancel(false);

            }
            return base.OnEffectExpires(effect, noMessages);
        }

        private void PlayerAction(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;


            if (player == null) return;



            if (args is AttackFinishedEventArgs)
            {
               
                //MessageToLiving((GameLiving)player, "You are attacking. Your concentration fades!", eChatType.CT_SpellResisted);
                 if (player is GamePlayer)
                {
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouAttacking.YourConcentrationFades"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }
                OnEffectExpires(m_effect, true);
                return;
            }
            if (args is DyingEventArgs)
            {
                OnEffectExpires(m_effect, false);
                return;
            }
            if (args is CastingEventArgs)
            {
                if ((args as CastingEventArgs).SpellHandler.Caster != Caster)
                    return;
                //MessageToLiving((GameLiving)player, "You are casting a spell. Your concentration fades!!", eChatType.CT_SpellResisted);
                if (player is GamePlayer)
                {
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouCasting.YourConcentrationFades"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }

                OnEffectExpires(m_effect, true);
                return;
            }
            if (e == GamePlayerEvent.Moving)
            {
                if (player is GamePlayer)
                {
                    (player as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((player as GamePlayer).Client.Account.Language, "SpellHandler.YouAreMoving.YourConcentrationFades"), eChatType.CT_SpellExpires, eChatLoc.CL_SystemWindow);
                }


                // MessageToLiving((GameLiving)player, "You are moving. Your concentration fades!!", eChatType.CT_SpellResisted);
                OnEffectExpires(m_effect, true);
                return;
            }
        }
    }
       
    
    #endregion

    //shared timer 1
	#region Spymaster-8
	[SpellHandlerAttribute("SiegeWrecker")]
	public class SiegeWreckerSpellHandler : MineSpellHandler
	{
		public override void OnEffectPulse(GameSpellEffect effect)
		{
			if (mine == null || mine.ObjectState == GameObject.eObjectState.Deleted)
			{
				effect.Cancel(false);
				return;
			}

			if (trap == null) return;
			bool wasstealthed = ((GamePlayer)Caster).IsStealthed;
			foreach (GameNPC npc in mine.GetNPCsInRadius((ushort)s.Range))
			{
                if (npc is GameSiegeWeapon && npc.IsAlive && GameServer.ServerRules.IsAllowedToAttack(Caster, npc, true))
				{
					trap.StartSpell((GameLiving)npc);
					if (!Unstealth) ((GamePlayer)Caster).Stealth(wasstealthed);
					return;
				}
			}
		}
		// constructor
		public SiegeWreckerSpellHandler(GameLiving caster, Spell spell, SpellLine line)
			: base(caster, spell, line)
		{
			Unstealth = false;

			//Construct a new mine.
			mine = new GameMine();
			mine.Model = 2591;
			mine.Name = spell.Name;
			mine.Realm = caster.Realm;
			mine.X = caster.X;
			mine.Y = caster.Y;
			mine.Z = caster.Z;
			mine.MaxSpeedBase = 0;
			mine.CurrentRegionID = caster.CurrentRegionID;
			mine.Heading = caster.Heading;
			mine.Owner = (GamePlayer)caster;

			// Construct the mine spell
			dbs = new DBSpell();
			dbs.Name = spell.Name;
			dbs.Icon = 7301;
			dbs.ClientEffect = 7301;
			dbs.Damage = spell.Damage;
			dbs.DamageType = (int)spell.DamageType;
			dbs.Target = "Enemy";
			dbs.Radius = 0;
			dbs.Type = "DirectDamage";
			dbs.Value = spell.Value;
			dbs.Duration = spell.ResurrectHealth;
			dbs.Frequency = spell.ResurrectMana;
			dbs.Pulse = 0;
			dbs.PulsePower = 0;
			dbs.LifeDrainReturn = spell.LifeDrainReturn;
			dbs.Power = 0;
			dbs.CastTime = 0;
			dbs.Range = WorldMgr.VISIBILITY_DISTANCE;
			sRadius = 350;
			s = new Spell(dbs, 1);
			sl = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
			trap = ScriptMgr.CreateSpellHandler(m_caster, s, sl);
		}
	}
	#endregion

	#region Spymaster-9
	[SpellHandlerAttribute("EssenceFlare")]
	public class EssenceFlareSpellHandler : SummonItemSpellHandler
	{
        public EssenceFlareSpellHandler(GameLiving caster, Spell spell, SpellLine line)
            : base(caster, spell, line)
        {

            ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>("Meschgift");
            if (template != null && ((GamePlayer)caster) != null)
            {
                items.Add(GameInventoryItem.Create<ItemTemplate>(template));
                foreach (InventoryItem item in items)
                {
                    if (item.IsStackable)
                    {
                        item.Count = 1;
                        item.Weight = item.Count * item.Weight;
                    }
                }
            }
        }
        public override void ApplyEffectOnTarget(GameLiving target, double effectiveness)
        {
            base.ApplyEffectOnTarget(target, effectiveness);
            GameEventMgr.AddHandler(Caster, GamePlayerEvent.Quit, OnPlayerLeft);
        }

        private static void OnPlayerLeft(DOLEvent e,object sender,EventArgs arguments)
        {
            if(!(sender is GamePlayer)) return;
            GamePlayer caster = sender as GamePlayer;
            lock(caster.Inventory)
            {
                var items = caster.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
                foreach(InventoryItem invItem in items)
                {
                    if(invItem.Id_nb.Equals("Meschgift"))
                    {
                        caster.Inventory.RemoveItem(invItem);
                    }
                }
            }
            GameEventMgr.RemoveHandler(sender, GamePlayerEvent.Quit, OnPlayerLeft);
        }
    }

    #endregion

	#region Spymaster-10
    [SpellHandler("BlanketOfCamouflage")]
    public class GroupstealthHandler : SpellHandler
    {
        public GroupstealthHandler(GameLiving caster, Spell spell, SpellLine line) : base(caster, spell, line) { }

        GameSpellEffect m_effect = null;
        private int stealthSpec = 0;

        public override void FinishSpellCast(GameLiving target)
        {
            base.FinishSpellCast(target);
        }


        public override void OnEffectStart(GameSpellEffect effect)
        {
            m_effect = effect;
            base.OnEffectStart(effect);
            stealthSpec = effect.Owner.GetModifiedSpecLevel(Specs.Stealth);
            GamePlayer player = effect.Owner as GamePlayer;
            if (player == null) return;
            {
                if (stealthSpec > 0)
                {
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.Released, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerAction));
                }
                if (stealthSpec == 0 && effect.Owner is GamePlayer && effect.Owner.IsAlive)
                {
                    player.Stealth(true);
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerAction));
                    GameEventMgr.AddHandler((GamePlayer)effect.Owner, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAction));
                }
            }
        }

        /// <summary>
        /// When an applied effect expires.
        /// Duration spells only.
        /// </summary>
        /// <param name="effect">The expired effect</param>
        /// <param name="noMessages">true, when no messages should be sent to player and surrounding</param>
        /// <returns>immunity duration in milliseconds</returns>
        public override int OnEffectExpires(GameSpellEffect effect, bool noMessages)
        {
            GamePlayer player = effect.Owner as GamePlayer;
            if (player == null) return base.OnEffectExpires(effect, noMessages);

            if (stealthSpec > 0)
            {
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.Quit, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.Released, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.Linkdeath, new DOLEventHandler(PlayerAction));
            }
            {
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.AttackedByEnemy, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.AttackFinished, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.CastStarting, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.Moving, new DOLEventHandler(PlayerAction));
                GameEventMgr.RemoveHandler((GamePlayer)effect.Owner, GamePlayerEvent.Dying, new DOLEventHandler(PlayerAction));
            }
            return base.OnEffectExpires(effect, noMessages);
        }
        private void PlayerAction(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = (GamePlayer)sender;
            if (player == null) return;
            if (args is AttackFinishedEventArgs)
            {
                MessageToLiving((GameLiving)player, "You are attacking. Your camouflage fades!", eChatType.CT_SpellResisted);
                OnEffectExpires(m_effect, true);
                return;
            }
            if (args is DyingEventArgs)
            {
                OnEffectExpires(m_effect, false);
                player.Stealth(false);
                return;
            }
            if (args is CastingEventArgs)
            {
                if ((args as CastingEventArgs).SpellHandler.Caster != Caster)
                    return;
                MessageToLiving((GameLiving)player, "You are casting a spell. Your camouflage fades!", eChatType.CT_SpellResisted);
                OnEffectExpires(m_effect, true);
                player.Stealth(false);
                return;
            }
            if (e == GamePlayerEvent.AttackedByEnemy)
            {
                OnEffectExpires(m_effect, false);
                player.Stealth(false);
                return;
            }
            if (e == GamePlayerEvent.Moving)
            {
                MessageToLiving((GameLiving)player, "You are moving. Your camouflage fades!", eChatType.CT_SpellResisted);
                OnEffectExpires(m_effect, true);
                player.Stealth(false);
                return;
            }
            if (e == GamePlayerEvent.Quit || e == GamePlayerEvent.Released || e == GamePlayerEvent.Linkdeath)
            {
                if ((player is GamePlayer) && (player.Group != null))
                {
                    foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
                    {

                        p.Stealth(false);
                    }
                }
                else
                {
                    player.Stealth(false);
                    
                }
            }
        }
    }
	#endregion
}
//to show an Icon & informations to the caster
namespace DOL.GS.Effects
{
    public class LoockoutOwner : StaticEffect, IGameEffect
	{
		public LoockoutOwner() : base() { }
		public void Start(GamePlayer Caster) { base.Start(Caster); }
		public override void Stop() { base.Stop(); }
		public override ushort Icon { get { return 2616; } }
		public override string Name { get { return "Loockout"; } }
		public override IList<string> DelveInfo(GamePlayer player)
        {
			
			{
				var delveInfoList = new List<string>();
				delveInfoList.Add("Your stealth range is increased.");
                delveInfoList.Add("Target of lookout must be sitting and not move. Once you move, the effect breaks and normal stealth detection range applies.");
                
                return delveInfoList;
			}
		}
	}
}

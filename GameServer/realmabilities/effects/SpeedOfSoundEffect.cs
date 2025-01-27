using System;
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.PropertyCalc;
using DOL.Events;
using System.Collections.Generic;

namespace DOL.GS.Effects
{
	/// <summary>
	/// Effect handler for Barrier Of Fortitude
	/// </summary> 
	public class SpeedOfSoundEffect : TimedEffect, IGameEffect
	{
		public SpeedOfSoundEffect(int duration)
			: base(duration)
		{ }

		DOLEventHandler m_attackFinished = new DOLEventHandler(AttackFinished);
        
        public bool IsSummoningMount
        {
            get { return m_whistleMountTimer != null && m_whistleMountTimer.IsAlive; }
        }
      
        protected RegionTimer m_whistleMountTimer;
		/// <summary>
		/// Called when effect is to be started
		/// </summary>
		/// <param name="living">The living to start the effect for</param>
        public override void Start(GameLiving living)
        {
            base.Start(living);

           

            SpeedOfSoundEffect effect2 = living.EffectList.GetOfType<SpeedOfSoundEffect>();
           // if (effect2 != null && living.IsStunned || living.IsMezzed || (living is GamePlayer && (living as GamePlayer).IsSummoningMount) || (living is GamePlayer && (living as GamePlayer).IsRiding) || (living is GamePlayer && (living as GamePlayer).IsOnHorse))
                if (effect2 != null && (living is GamePlayer && (living as GamePlayer).IsSummoningMount) || (living is GamePlayer && (living as GamePlayer).IsRiding) || (living is GamePlayer && (living as GamePlayer).IsOnHorse))
            {
                effect2.Cancel(false);
                ((GamePlayer)living).Out.SendMessage("you loose your effect!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
                
                return;
            }
            GameSpellEffect effect3 = Spells.SpellHandler.FindEffectOnTarget(living, "StyleSpeedDecrease");
            if (effect3 != null)
                effect3.Cancel(false);
            GameSpellEffect speed2 = Spells.SpellHandler.FindEffectOnTarget(living, "SpeedOfTheRealm");
            if (speed2 != null)
                speed2.Cancel(false);
            GameSpellEffect speed3 = Spells.SpellHandler.FindEffectOnTarget(living, "ArcherSpeedEnhancement");
            if (speed3 != null)
                speed3.Cancel(false);

            GameSpellEffect effect4 = Spells.SpellHandler.FindEffectOnTarget(living, "SpeedDecrease");
            if (effect4 != null)
                effect4.Cancel(false);
			GameSpellEffect effect5 = Spells.SpellHandler.FindEffectOnTarget(living, "HereticPiercingMagic");
			if (effect5 != null)
				effect5.Cancel(false);

			GameSpellEffect DecreaseLOP = Spells.SpellHandler.FindEffectOnTarget(living, "HereticDamageSpeedDecreaseLOP");

			if (DecreaseLOP != null)
			{
				DecreaseLOP.Cancel(false);
			}
		

			
			ChargeEffect effect1 = living.EffectList.GetOfType<ChargeEffect>();
            if (effect1 != null)
                effect1.Cancel(false);
            m_owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
            if (m_owner is GamePlayer)
               (m_owner as GamePlayer).Out.SendUpdateMaxSpeed();
            {
                living.TempProperties.setProperty("Charging", true);
                GameEventMgr.AddHandler(living, GameLivingEvent.AttackFinished, m_attackFinished);
                GameEventMgr.AddHandler(living, GameLivingEvent.CastFinished, m_attackFinished);
                living.BuffBonusMultCategory1.Set((int)eProperty.MaxSpeed, this, PropertyCalc.MaxSpeedCalculator.SPEED4);

                if (living is GamePlayer)
                    (living as GamePlayer).Out.SendUpdateMaxSpeed();
            }
        }

		/// <summary>
		/// Called when the effectowner attacked an enemy
		/// </summary>
		/// <param name="e">The event which was raised</param>
		/// <param name="sender">Sender of the event</param>
		/// <param name="args">EventArgs associated with the event</param>
		private static void AttackFinished(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if (e == GameLivingEvent.CastFinished)
			{
				CastingEventArgs cfea = args as CastingEventArgs;

				if (cfea.SpellHandler.Caster != player)
					return;

				//cancel if the effectowner casts a non-positive spell
				if (!cfea.SpellHandler.HasPositiveEffect)
				{
					SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
					if (effect != null)
						effect.Cancel(false);
                
                }
			}
			else if (e == GameLivingEvent.AttackFinished)
			{
				AttackFinishedEventArgs afargs = args as AttackFinishedEventArgs;
				if (afargs == null)
					return;

				if (afargs.AttackData.Attacker != player)
					return;

				switch (afargs.AttackData.AttackResult)
				{
					case GameLiving.eAttackResult.HitStyle:
					case GameLiving.eAttackResult.HitUnstyled:
					case GameLiving.eAttackResult.Blocked:
					case GameLiving.eAttackResult.Evaded:
					case GameLiving.eAttackResult.Fumbled:
					case GameLiving.eAttackResult.Missed:
					case GameLiving.eAttackResult.Parried:
						SpeedOfSoundEffect effect = player.EffectList.GetOfType<SpeedOfSoundEffect>();
						if (effect != null)
							effect.Cancel(false);
						break;
				}
			}
		}

		public override void Stop()
		{
			base.Stop();
			m_owner.TempProperties.removeProperty("Charging");
			m_owner.BuffBonusMultCategory1.Remove((int)eProperty.MaxSpeed, this);
			if (m_owner is GamePlayer)
				(m_owner as GamePlayer).Out.SendUpdateMaxSpeed();
			GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.AttackFinished, m_attackFinished);
			GameEventMgr.RemoveHandler(m_owner, GameLivingEvent.CastFinished, m_attackFinished);
		}


		/// <summary>
		/// Name of the effect
		/// </summary>
		public override string Name
		{
			get
			{
				return "Speed of Sound";
			}
		}

		/// <summary>
		/// Icon ID
		/// </summary>
		public override UInt16 Icon
		{
			get
			{
				return 3020;
			}
		}

		/// <summary>
		/// Delve information
		/// </summary>
		public override IList<string> DelveInfo(GamePlayer player)
        {
			
			{
				var delveInfoList = new List<string>();
				delveInfoList.Add("Gives immunity to stun/snare/root and mesmerize spells and provides unbreakeable speed.");
				delveInfoList.Add(" ");

				int seconds = (int)(RemainingTime / 1000);
				if (seconds > 0)
				{
					delveInfoList.Add(" ");
					delveInfoList.Add("- " + seconds + " seconds remaining.");
				}

				return delveInfoList;
			}
		}
	}
}
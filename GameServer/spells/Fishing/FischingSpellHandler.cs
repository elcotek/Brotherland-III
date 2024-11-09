using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Spells;
using DOL.Language;
using System;
using System.Collections.Generic;

namespace DOL.spells.Fishing
{
	/// <summary>
	/// 
	/// </summary>
	[SpellHandlerAttribute("Fishing")]
	public class FischingSpellHandler : SpellHandler
	{
		protected const string Fishing_Chanceled = "FISHINGCHANNELED";

		/// <summary>
		/// When an applied effect starts
		/// duration spells only
		/// </summary>
		/// <param name="effect"></param>
		public override void OnEffectStart(GameSpellEffect effect)
		{
			int _UmgebungSound = 0;
			_UmgebungSound = Util.Random(455, 475);


			GamePlayer player = effect.Owner as GamePlayer;
			if (player != null)
			{
				GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.AddHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FishingFocusSpellAction));

				player.TempProperties.setProperty(Fishing_Chanceled, false);
				MessageToCaster("Ihr beginnt zu angeln!", eChatType.CT_SpellResisted);
				//skilllevel für Timer abfragen
				player.Out.SendTimerWindow("Ihr kontzentriert Euch. Ihr angelt..", Util.Random(25, 35));
				player.Out.SendSoundEffect((ushort)_UmgebungSound, player.CurrentRegionID, (ushort)player.X, (ushort)player.Y, (ushort)player.Z, 1000);

				//player.DisableSkill(Spell, 30);


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

			int _fang = 0;
			_fang = Util.Random(1, 5);
			int _KleinerFishSound = 0;
			_KleinerFishSound = Util.Random(118, 1017);
			int _GroßerFishSound = 0;
			_GroßerFishSound = Util.Random(1162, 1165);


			int duration = Spell.Duration;
			//1017 - 118 
			GamePlayer player = effect.Owner as GamePlayer;

			if (player != null && player.TempProperties.getProperty<bool>(Fishing_Chanceled) == false)
			{


				switch (_fang)
				{
					case 1:
						{
							if (Util.Chance(20))
							{
								//MessageToCaster("Ihr hattet Glück, und habt einen Kleinen Fisch gefangen!", eChatType.CT_SpellResisted);
								//player.Out.SendSoundEffect((ushort)_KleinerFishSound, player.CurrentRegionID, (ushort)player.X, (ushort)player.Y, (ushort)player.Z, 1000);
								player.Out.SendSoundEffect((ushort)_KleinerFishSound, 0, 0, 0, 0, 0);
								//ins inventar laden
								switch (Util.Random(1, 4))
								{
									case 1:
										{
											//Kleiner karpfen
											MessageToCaster("Ihr hattet Glück, und habt einen Kleinen Karpfen gefangen!", eChatType.CT_SpellResisted);
											break;
										}
									case 2:
										{
											//Kleine sprotte
											MessageToCaster("Ihr hattet Glück, und habt eine Kleine Sprotte gefangen!", eChatType.CT_SpellResisted);
											break;
										}
									case 3:
										{
											//Kleiner Stinnt
											MessageToCaster("Ihr hattet Glück, und habt eine Kleinen Stinnt gefangen!", eChatType.CT_SpellResisted);
											break;
										}
									case 4:
										{
											//Kleiner Stichling
											MessageToCaster("Ihr hattet Glück, und habt eine Kleinen Stichling gefangen!", eChatType.CT_SpellResisted);
											break;
										}

								}

								MessageToCaster("Ihr freut eich darüber und verstaut den Fang in eurem Inventar.", eChatType.CT_SpellResisted);
								///Emote für Sieg
								//erhhöhe den Fischfang Level um 10 punkte
								MessageToCaster("ihr steigert Euer Wissen über Fischfang um +10 Punke.", eChatType.CT_SpellResisted);
								break;
							}
							else
								MessageToCaster("Pech gehabt! Ihr habt nichts gefangen!", eChatType.CT_SpellResisted);
							player.Out.SendSoundEffect((ushort)924, 0, 0, 0, 0, 0);
							break;
						}
					case 2:
						{
							if (Util.Chance(10))
							{
								//MessageToCaster("Glückwunsch!! Ihr habte einen Großen Fisch gefangen!!", eChatType.CT_SpellResisted);
								player.Out.SendSoundEffect((ushort)_GroßerFishSound, 0, 0, 0, 0, 0);
								//ins inventar laden
								switch (Util.Random(1, 4))
								{
									case 1:
										{
											//ordentlicher Barsch
											MessageToCaster("Glückwunsch!! Ihr habte einen ordentlichen Barsch gefangen!!", eChatType.CT_SpellResisted);
											break;
										}
									case 2:
										{
											//Riesen-Sprotte
											MessageToCaster("Glückwunsch!! Ihr habte eine Riesen-Sprotte gefangen!!", eChatType.CT_SpellResisted);
											break;
										}
									case 3:
										{
											//Großer Heilbutt
											MessageToCaster("Glückwunsch!! Ihr habte einen großen Heilbutt gefangen!!", eChatType.CT_SpellResisted);
											break;
										}
									case 4:
										{
											//ordentlicher Aal
											MessageToCaster("Glückwunsch!! Ihr habte einen ordentlichen Aal gefangen!!", eChatType.CT_SpellResisted);
											break;
										}

								}
								
								MessageToCaster("Ihr freut eich riesig und verstaut den Fang in eurem Inventar.", eChatType.CT_SpellResisted);
								///Emote für Sieg
								//erhhöhe den Fischfang Level um 30 punkte
								MessageToCaster("ihr steigert Euer Wissen über Fischfang um +30 Punke.", eChatType.CT_SpellResisted);
								break;
							}
							else
								MessageToCaster("Wieder Pech gehabt! Ihr habt nichts gefangen!", eChatType.CT_SpellResisted);
							player.Out.SendSoundEffect((ushort)924, 0, 0, 0, 0, 0);
							break;
						}
					case 3:
						{

							MessageToCaster("Pech gehabt! Ihr habt nichts gefangen!", eChatType.CT_SpellResisted);
							player.Out.SendSoundEffect((ushort)924, 0, 0, 0, 0, 0);
							break;
						}
					case 4:
						{
							if (Util.Chance(5))
							{
								MessageToCaster("Euer Angelharken ist abgerissen! Ihr verliert einen Harken", eChatType.CT_SpellResisted);
								player.Out.SendSoundEffect((ushort)1710, 0, 0, 0, 0, 0);
								player.Out.SendSoundEffect((ushort)22, 0, 0, 0, 0, 0);
								//Angelharken aus dem inventar nehmen
								break;
							}
							else
								MessageToCaster("Pech gehabt! Ihr habt wieder nichts gefangen!", eChatType.CT_SpellResisted);
							player.Out.SendSoundEffect((ushort)924, 0, 0, 0, 0, 0);
							break;

						}
					case 5:
						{
							if (Util.Chance(5))
							{
								MessageToCaster("Euere Angelsehne ist abgerissen! Ihr verliert ein stück Sehne!", eChatType.CT_SpellResisted);
								player.Out.SendSoundEffect((ushort)1709, 0, 0, 0, 0, 0);
								player.Out.SendSoundEffect((ushort)22, 0, 0, 0, 0, 0);
								//Sehne aus dem inventar nehmen
								break;
							}
							else
								MessageToCaster("Pech gehabt! Ihr habt wieder nichts gefangen!", eChatType.CT_SpellResisted);
							player.Out.SendSoundEffect((ushort)924, 0, 0, 0, 0, 0);
							break;
						}

				}

				
				player.Out.SendCloseTimerWindow();
				MessageToCaster("Ihr fischt nicht mehr!", eChatType.CT_SpellResisted);
				if (player != null)
				{
					player.Out.SendSoundEffect((ushort)1709, 0, 0, 0, 0, 0);
				}
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FishingFocusSpellAction));
				GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FishingFocusSpellAction));
			}
			else
			{
				player.TempProperties.setProperty(Fishing_Chanceled, false);
			}

			return 0;
		}

		/// <summary>
		/// Hold events for focus spells
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void FishingFocusSpellAction(DOLEvent e, object sender, EventArgs args)
		{
			GameLiving living = sender as GameLiving;
			if (living == null) return;

			GameSpellEffect effect = SpellHandler.FindEffectOnTarget(living, Spell.SpellType);

			if (e == GameLivingEvent.Moving || e == GameLivingEvent.CastStarting
				|| e == GameLivingEvent.Dying || e == GameLivingEvent.AttackFinished || e == GameLivingEvent.AttackedByEnemy)
			{
				Caster.TempProperties.setProperty(Fishing_Chanceled, true);

				if (effect != null)
				{
					effect.Cancel(false);
					(Caster as GamePlayer).Out.SendCloseTimerWindow();
					MessageToCaster(String.Format("Ihr bewegt euch, Ihr könnt euch nicht mehr aufs fischen  konzentrieren!"), eChatType.CT_SpellExpires);
					MessageToCaster("Ihr fischt nicht mehr!", eChatType.CT_SpellResisted);

				}
				if (Caster is GamePlayer)
				{

					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FishingFocusSpellAction));
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.AttackFinished, new DOLEventHandler(FishingFocusSpellAction));
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.CastStarting, new DOLEventHandler(FishingFocusSpellAction));
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Moving, new DOLEventHandler(FishingFocusSpellAction));
					GameEventMgr.RemoveHandler(Caster, GameLivingEvent.Dying, new DOLEventHandler(FishingFocusSpellAction));
					(Caster as GamePlayer).Out.SendSoundEffect((ushort)601, 0, 0, 0, 0, 0);
				}
			}
		}
		/// <summary>
		/// Delve Info
		/// </summary>
		public override IList<string> DelveInfo(GamePlayer player)
		{

			{
				var list = new List<string>();

				list.Add(" "); //empty line
				list.Add(Spell.Description);
				list.Add(" "); //empty line
				list.Add("Fangt so viele Fische wie ihr nur könnt, um einer der Besten in der top-Liste der Fischer zu werden.");
				return list;
			}
		}






		/// <summary>
		/// Calculates the effect duration in milliseconds
		/// </summary>
		/// <param name="target">The effect target</param>
		/// <param name="effectiveness">The effect effectiveness</param>
		/// <returns>The effect duration in milliseconds</returns>
		protected override int CalculateEffectDuration(GameLiving target, double effectiveness)
		{
			int duration = Spell.Duration;

			duration -= Util.Random(2, 6);

			return duration;
		}

		protected String Profession { get; }

		public static String GetTitleFormat(int skillLevel, GamePlayer player)
		{
			if (skillLevel < 0)
				throw new ArgumentOutOfRangeException("Fisher skill level must be >= 0");



			switch (skillLevel / 100)
			{
				case 0: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Blutiger Angler Anfänger");
				case 1: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Möchtegern Angler");
				case 2: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Wasserläufer");
				case 3: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Wurmsucher");
				case 4: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Wurmfinder");
				case 5: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Harken Spezialist");
				case 6: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Angler");
				case 7: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Fortschrittlicher Angler");
				case 8: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Glückseeliger Angler");
				case 9: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Angehender Angel Meister");
				case 10: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Sportfischer");
				default: return LanguageMgr.GetTranslation(player.Client.Account.Language, "Meisterfischer");
			}
		}

		public String GetTitle(int skillLevel, GamePlayer player)
		{
			try
			{
				return String.Format(GetTitleFormat(skillLevel, player), Profession);
			}
			catch
			{
				return "<you may want to check your Crafting.txt language file>";
			}
		}


		public FischingSpellHandler(GameLiving caster, Spell spell, SpellLine spellLine) : base(caster, spell, spellLine) { }

	}
}


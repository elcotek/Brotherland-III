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
using DOL.Database;
using System.Collections;


namespace DOL.GS
{
    /// <summary>
    /// The Midgard dragon.
    /// </summary>
    /// <author>Aredhel</author>	
    public class Gjalpinulva : GameDragon
	{
		#region Add Spawns

        private readonly ArrayList m_retrieverList = new ArrayList();


		/// <summary>
		/// Spawn dogs that will despawn again after 60 seconds; there is
		/// a 25% chance that a retriever will spawn.
		/// </summary>
		/// <returns>Whether or not any retrievers were spawned.</returns>
		public override bool CheckAddSpawns()
		{
			base.CheckAddSpawns();  // In order to reset HealthPercentOld.

			Yell("Come to me my babies!");

			GameNPC dogSpawn;
			bool isRetriever = false;
			m_retrieverList.Clear();

			// Spawn dogs, in most cases (75% chance) these dogs will be level 
			// 37 and con green (decoys), in some cases (25%) they will be
			// retrievers, who will try to get out of the lair and, if successful,
			// cause Gjalpinulva to spawn a couple of deep purple adds.

			for (int dog = 1; dog <= 10; ++dog)
			{
				isRetriever = Util.Chance(25);
				dogSpawn = SpawnTimedAdd((isRetriever) ? 610 : 611, 
					(isRetriever) ? Util.Random(47, 53) : 37, 
					X + Util.Random(300, 600), Y + Util.Random(300, 600), 60, isRetriever);

				// We got a retriever, tell it who its master is and which exit
				// to run to.

				if (isRetriever)
				{
					if (dogSpawn != null && dogSpawn.Brain != null && dogSpawn.Brain is RetrieverMobBrain)
					{
						dogSpawn.Faction = Faction; //testen
						

						(dogSpawn.Brain as RetrieverMobBrain).Master = this;
						m_retrieverList.Add(dogSpawn);

						//dogSpawn.WalkTo(GetExitCoordinates(Util.Random(1, 4)), 200);    // Pick 1 out of 4 possible exits.
						Point3D point = new Point3D(GetExitCoordinates(Util.Random(1, 4)));// converting to Point

						dogSpawn.PathTo(point.X, point.Y, point.Z, 200);// Pick 1 out of 4 possible exits.
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Get coordinates for the given exit (1 = NW, 2 = SW, 
		/// 3 = SE, 4 = NE).
		/// </summary>
		/// <returns>Coordinates.</returns>
		private Point3D GetExitCoordinates(int exitNo)
		{
			// Get target coordinates (hardcoded). Yeah I know, this is
			// ugly, but to get this right NPC pathing is a must; as it
			// is at the moment, there is no way of knowing where the exits 
			// are (from the PoV of an NPC).

			switch (exitNo)
			{
				case 1: return new Point3D(707026, 1019564, 0);
				case 2: return new Point3D(706924, 1023596, 0);
				case 3: return new Point3D(711441, 1023175, 0);
				case 4: return new Point3D(710708, 1018894, 0);
				default: return SpawnPoint;
			}
		}

		/// <summary>
		/// Invoked when retriever type mob has reached its target location.
		/// </summary>
		/// <param name="sender">The retriever mob.</param>
		public override void OnRetrieverArrived(GameNPC sender)
		{
			base.OnRetrieverArrived(sender);
			if (sender == null || sender == this) return;

			// Spawn nasty adds.

			if (m_retrieverList.Contains(sender))
                SpawnDrakulvs(Util.Random(7, 10), sender.X, sender.Y);
		}

		/// <summary>
		/// Spawn some (10-20) nasty level 62-68 drakulvs around the spot the
		/// retriever has reported back from, then make these spawns aggro the
		/// raid inside the lair.
		/// </summary>
		/// <param name="numAdds"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		private void SpawnDrakulvs(int numAdds, int x, int y)
		{
			
			GameNPC drakulv;
			bool isDisciple = false;
			for (int add = 0; add < numAdds; ++add)
			{
				isDisciple = Util.Chance(25);
				drakulv = SpawnTimedAdd((isDisciple) ? 613 : 612, Util.Random(62, 68), x + Util.Random(250), y + Util.Random(250), 120, false);

				if (drakulv != null && drakulv.Brain is StandardMobBrain && this.Brain is DragonBrain)
				{
					drakulv.Faction = Faction;
					
					(Brain as DragonBrain).AddAggroListTo(drakulv.Brain as StandardMobBrain);
					
				}

			}
		}

		#endregion

		#region Glare

		/// <summary>
		/// The Glare spell.
		/// </summary>
		protected override Spell Glare
		{
			get
			{
				if (m_glareSpell == null)
				{
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = false,
                        CastTime = 0,
                        ClientEffect = 5701,
                        Description = "Glare",
                        Name = "Dragon Glare",
                        Range = 2500,
                        Radius = 700,
                        Damage = 1800 * DragonDifficulty / 100,
                        RecastDelay = 10,
                        DamageType = (int)eDamageType.Cold,
                        SpellID = 6001,
                        Target = "Enemy",
                        Type = "DirectDamage"
                    };
                    m_glareSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_glareSpell);
				}
				return m_glareSpell;
			}
		}

		#endregion

		#region Breath

		/// <summary>
		/// The Breath spell.
		/// </summary>
		protected override Spell Breath
		{
			get
			{
				if (m_breathSpell == null)
				{
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = false,
                        CastTime = 0,
                        Uninterruptible = true,
                        ClientEffect = 208,
                        Description = "Nuke",
                        Name = "Dragon Nuke",
                        Range = 700,
                        Radius = 700,
                        Damage = 1800 * DragonDifficulty / 100,
                        DamageType = (int)eDamageType.Cold,
                        SpellID = 6012,
                        Target = "Enemy",
                        Type = "DirectDamage"
                    };
                    m_breathSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_breathSpell);
				}
				return m_breathSpell;
			}
		}

		/// <summary>
		/// The resist debuff spell.
		/// </summary>
		protected override Spell ResistDebuff
		{
			get
			{
				if (m_resistDebuffSpell == null)
				{
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = false,
                        CastTime = 0,
                        Uninterruptible = true,
                        ClientEffect = 778,
                        Icon = 778,
                        Description = "Cold Resist Debuff",
                        Name = "Biting Cold",
                        Range = 700,
                        Radius = 700,
                        Value = 30,
                        Duration = 30 * DragonDifficulty / 100,
                        Damage = 0,
                        DamageType = (int)eDamageType.Heat,
                        SpellID = 6013,
                        Target = "Enemy",
                        Type = "ColdResistDebuff",
                        Message1 = "You feel more vulnerable to cold!",
                        Message2 = "{0} seems vulnerable to cold!"
                    };
                    m_resistDebuffSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_resistDebuffSpell);
				}
				return m_resistDebuffSpell;
			}
		}

		#endregion

		#region Melee Debuff

		/// <summary>
		/// The melee debuff spell.
		/// </summary>
		protected override Spell MeleeDebuff
		{
			get
			{
				if (m_meleeDebuffSpell == null)
				{
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = false,
                        CastTime = 0,
                        Uninterruptible = true,
                        ClientEffect = 13082,
                        Icon = 13082,
                        Description = "Fumble Chance Debuff",
                        Name = "Growing Trepidation",
                        Range = 700,
                        Radius = 700,
                        Value = 50,
                        Duration = 90 * DragonDifficulty / 100,
                        Damage = 0,
                        DamageType = (int)eDamageType.Heat,
                        SpellID = 6003,
                        Target = "Enemy",
                        Type = "FumbleChanceDebuff"
                    };
                    m_meleeDebuffSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_meleeDebuffSpell);
				}
				return m_meleeDebuffSpell;
			}
		}

		#endregion

		#region Ranged Debuff

		/// <summary>
		/// The ranged debuff spell.
		/// </summary>
		protected override Spell RangedDebuff
		{
			get
			{
				if (m_rangedDebuffSpell == null)
				{
                    DBSpell spell = new DBSpell
                    {
                        AllowAdd = false,
                        CastTime = 0,
                        Uninterruptible = true,
                        ClientEffect = 590,
                        Icon = 590,
                        Description = "Nearsight",
                        Name = "Dazzling Light",
                        Range = 700,
                        Radius = 700,
                        Value = 100,
                        Duration = 90 * DragonDifficulty / 100,
                        Damage = 0,
                        DamageType = (int)eDamageType.Heat,
                        SpellID = 6003,
                        Target = "Enemy",
                        Type = "Nearsight",
                        Message1 = "You are blinded!",
                        Message2 = "{0} is blinded!"
                    };
                    m_rangedDebuffSpell = new Spell(spell, 70);
					SkillBase.GetSpellList(GlobalSpellsLines.Mob_Spells).Add(m_rangedDebuffSpell);
				}
				return m_rangedDebuffSpell;
			}
		}

		#endregion
	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS.RealmAbilities
{
	/// <summary>
	/// Falcon's Eye RA
	/// </summary>
	public class FalconsEyeAbility : RAPropertyEnhancer
	{
		public FalconsEyeAbility(DBAbility dba, int level)
			: base(dba, level, eProperty.CriticalSpellHitChance)
		{
		}

		public override int GetAmountForLevel(int level)
		{
			if (level < 1) return 0;
			if (ServerProperties.Properties.USE_NEW_PASSIVES_RAS_SCALING)
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 6;
						case 3: return 9;
						case 4: return 13;
						case 5: return 17;
						case 6: return 22;
						case 7: return 27;
						case 8: return 33;
						case 9: return 39;
						default: return 39;
				}
			}
			else
			{
				switch (level)
				{
						case 1: return 3;
						case 2: return 9;
						case 3: return 17;
						case 4: return 27;
						case 5: return 39;
						default: return 39;
				}
			}
		}

		public override IList<string> DelveInfo(GamePlayer player)
		{
			
			{
				var list = new List<string>();
				list.Add(m_description);
				list.Add("");
				for (int i = 1; i <= MaxLevel; i++)
				{
					list.Add("Level " + i + ": Amount: " + Level * 5 + "% / " + GetAmountForLevel(i));
				}
				return list;
			}
		}
	}
}
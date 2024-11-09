


using DOL.Database.Attributes;
using System;

namespace DOL.Database
{
	/// <summary>
	/// PlayerCountDB
	/// </summary>
	[DataTable(TableName = "StatsCounter")]
	public class DBStatsCounter : DataObject
	{
		private string m_Source;
		private int m_Count;
		
		

		public DBStatsCounter()
		{
			m_Source = string.Empty;

			m_Count = 0;
					
		}

		

		[DataElement(AllowDbNull = true)]
		public string Source
		{
			get
			{
				return m_Source;
			}
			set
			{
				Dirty = true;
				m_Source = value;
			}
		}


		[DataElement(AllowDbNull = true)]
		public int Count
		{
			get
			{
				return m_Count;
			}
			set
			{
				Dirty = true;
				m_Count = value;
			}
		}
	}
}
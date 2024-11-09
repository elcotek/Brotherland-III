using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Keeps
{
    public class GameKeepArea : Area.Circle
	{
		public AbstractGameKeep Keep = null;
		private const int PK_RADIUS = 4000;
		private const int KEEP_RADIUS = 3000;
		private const int TOWER_RADIUS = 1500;

		public GameKeepArea()
			: base()
		{ }

		public GameKeepArea(AbstractGameKeep keep)
			: base(keep.Name, keep.X, keep.Y, 0, keep.IsPortalKeep ? PK_RADIUS : (keep is GameKeepTower ? TOWER_RADIUS : KEEP_RADIUS)
)		{
			Keep = keep;
		}

		public override void OnPlayerEnter(GamePlayer player)
		{
			//[Ganrod] Nidel: NPE
			if (player == null || Keep == null)
			{
				return;
			}
			base.OnPlayerEnter(player);
			if (Keep.Guild != null)
				player.Out.SendMessage("Controlled by " + Keep.Guild.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		public void ChangeRadius(int newRadius)
		{
			KeepMgr.Logger.Debug("ChangeRadius called for " + Keep.Name + " currently is " + m_Radius + " changing to " + newRadius);

			//setting radius to default
			if (newRadius == 0 && m_Radius != 0)
			{
				if (m_dbArea != null)
					GameServer.Database.DeleteObject(m_dbArea);
				m_Radius = Keep is GameKeep ? (Keep.IsPortalKeep ? PK_RADIUS : KEEP_RADIUS) : TOWER_RADIUS;
				return;
			}

			//setting different radius when radius was already something
			if (newRadius > 0 && m_Radius >= 0)
			{
				m_Radius = newRadius;
				if (m_dbArea != null)
				{
					m_dbArea.Radius = m_Radius;
					GameServer.Database.SaveObject(m_dbArea);
				}
				else
				{
                    m_dbArea = new DBArea
                    {
                        CanBroadcast = this.CanBroadcast,
                        CheckLOS = this.CheckLOS,
                        ClassType = this.GetType().ToString(),
                        Description = this.Description,
                        Radius = this.Radius,
                        Region = (ushort)this.Keep.Region,
                        Sound = this.Sound,
                        X = this.X,
                        Y = this.Y,
                        Z = this.Z
                    };

                    GameServer.Database.AddObject(m_dbArea);
				}
			}
		}

		public override void LoadFromDatabase(DBArea area)
		{
			base.LoadFromDatabase(area);
			KeepMgr.Logger.Debug("KeepArea " + area.Description + " LoadFromDatabase called");
			KeepMgr.Logger.Debug("X: " + area.X + "(" + m_X + ") Y: " + area.Y + "(" + m_Y + ") Region:" + area.Region + " Radius: " + m_Radius);
		}
	}
}

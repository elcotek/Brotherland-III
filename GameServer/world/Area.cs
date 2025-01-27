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
using System;
using System.Collections.Generic;

namespace DOL.GS
{
    /// <summary>
    /// Collection of basic area shapes
    /// Circle
    /// Square
    /// </summary>
    public class Area 
	{
		public class Square : AbstractArea
		{
			/// <summary>
			/// The X coordinate of this Area
			/// </summary>
			protected int m_X;

			/// <summary>
			/// The Y coordinate of this Area 
			/// </summary>
			protected int m_Y;

			/// <summary>
			/// The width of this Area 
			/// </summary>
			protected int m_Width;

			/// <summary>
			/// The height of this Area 
			/// </summary>
			protected int m_Height;

			public Square()
				: base()
			{ }

			public Square(string desc, int x, int y, int width, int height): base(desc)
			{
				m_X = x;
				m_Y = y;
				m_Height = height;
				m_Width = width;
			}

			/// <summary>
			/// Returns the X Coordinate of this Area
			/// </summary>
			public int X
			{
				get { return m_X; }
			}

			/// <summary>
			/// Returns the Y Coordinate of this Area
			/// </summary>
			public int Y
			{
				get { return m_Y; }
			}

			/// <summary>
			/// Returns the Width of this Area
			/// </summary>
			public int Width
			{
				get { return m_Width; }
			}

			/// <summary>
			/// Returns the Height of this Area
			/// </summary>
			public int Height
			{
				get { return m_Height; }
			}

			/// <summary>
			/// Checks wether area intersects with given zone
			/// </summary>
			/// <param name="zone"></param>
			/// <returns></returns>
			public override bool IsIntersectingZone(Zone zone)
			{
				if (X+Width < zone.XOffset)
					return false;
				if (X-Width >= zone.XOffset + 65536)
					return false;
				if (Y+Height < zone.YOffset)
					return false;
				if (Y-Height >= zone.YOffset + 65536)
					return false;

				return true;
			}	

			/// <summary>
			/// Checks wether given point is within area boundaries
			/// </summary>
			/// <param name="p"></param>
			/// <returns></returns>
			public override bool IsContaining(IPoint3D p)
			{
				return IsContaining(p, true);
			}

			public override bool IsContaining(int x, int y, int z)
			{
				return IsContaining(x, y, z, true);
			}

			public override bool IsContaining(IPoint3D p, bool checkZ)
			{
				return IsContaining(p.X, p.Y, p.Z, checkZ);
			}

			public override bool IsContaining(int x, int y, int z, bool checkZ)
			{
				long m_xdiff = (long)x - X;
				if (m_xdiff < 0 || m_xdiff > Width)
					return false;

				long m_ydiff = (long)y - Y;
				if (m_ydiff < 0 || m_ydiff > Height)
					return false;

				/*
				//SH: Removed Z checks when one of the two Z values is zero(on ground)
				if (Z != 0 && spotZ != 0)
				{
					long m_zdiff = (long) spotZ - Z;
					if (m_zdiff> Radius)
						return false;
				}
				*/

				return true;
			}

			public override void LoadFromDatabase(DBArea area)
			{
				m_dbArea = area;
				m_Description = area.Description;
				m_X = area.X;
				m_Y = area.Y;
				m_Width = area.Radius;
				m_Height = area.Radius;
			}
		}

		public class Circle : AbstractArea
		{
			
			/// <summary>
			/// The X coordinate of this Area
			/// </summary>
			protected int m_X;

			/// <summary>
			/// The Y coordinate of this Area
			/// </summary>
			protected int m_Y;

			/// <summary>
			/// The Z coordinate of this Area
			/// </summary>
			protected int m_Z;

			/// <summary>
			/// The radius of the area in Coordinates
			/// </summary>
			protected int m_Radius;

			protected long m_distSq;

			public Circle()
				: base()
			{
			}

			public Circle( string desc, int x, int y, int z, int radius) : base(desc)
			{															
				m_Description = desc;
				m_X = x;
				m_Y = y;
				m_Z= z;
				m_Radius= radius;
					
				m_RadiusRadius = radius*radius;
			}

			/// <summary>
			/// Returns the X Coordinate of this Area
			/// </summary>
			public int X
			{
				get { return m_X; }
			}

			/// <summary>
			/// Returns the Y Coordinate of this Area
			/// </summary>
			public int Y
			{
				get { return m_Y; }
			}

			/// <summary>
			/// Returns the Width of this Area
			/// </summary>
			public int Z
			{
				get { return m_Z; }
			}

			/// <summary>
			/// Returns the Height of this Area
			/// </summary>
			public int Radius
			{
				get { return m_Radius; }
			}

			/// <summary>
			/// Cache for radius*radius to increase performance of circle check,
			/// radius is still needed for square check
			/// </summary>
			protected int m_RadiusRadius;
			

			/// <summary>
			/// Checks wether area intersects with given zone
			/// </summary>
			/// <param name="zone"></param>
			/// <returns></returns>
			public override bool IsIntersectingZone(Zone zone)
			{
				if (X+Radius < zone.XOffset)
					return false;
				if (X-Radius >= zone.XOffset + 65536)
					return false;
				if (Y+Radius < zone.YOffset)
					return false;
				if (Y-Radius >= zone.YOffset + 65536)
					return false;

				return true;
			}

			public override bool IsContaining(IPoint3D spot)
			{
				return IsContaining(spot, true);
			}

			public override bool IsContaining(int x, int y, int z, bool checkZ)
			{
				// spot is not in square around circle no need to check for circle...
				long m_xdiff = (long)x - X;
				if (m_xdiff > Radius)
					return false;

				long m_ydiff = (long)y - Y;
				if (m_ydiff > Radius)
					return false;


				// check if spot is in circle
				m_distSq = m_xdiff * m_xdiff + m_ydiff * m_ydiff;

				if (Z != 0 && z != 0 && checkZ)
				{
					long m_zdiff = (long)z - Z;
					m_distSq += m_zdiff * m_zdiff;
				}

				return (m_distSq <= m_RadiusRadius);
			}

			public override bool IsContaining(int x, int y, int z)
			{
				return IsContaining(x, y, z, true);
			}

			/// <summary>
			/// Checks wether given point is within area boundaries
			/// </summary>
			/// <param name="p"></param>
			/// <param name="checkZ"></param>
			/// <returns></returns>
			public override bool IsContaining(IPoint3D p, bool checkZ)
			{
				return IsContaining(p.X, p.Y, p.Z, checkZ);
			}

			public override void LoadFromDatabase(DBArea area)
			{
				m_Description = area.Description;
				m_X = area.X;
				m_Y = area.Y;
				m_Z = area.Z;
				m_Radius = area.Radius;
				m_RadiusRadius = area.Radius * area.Radius;
			}
		}

        public class Polygon : AbstractArea
        {
            /// <summary>
            /// The X coordinate of this Area (center, not important)
            /// </summary>
            protected int m_X;

            /// <summary>
            /// The Y coordinate of this Area (center, not important)
            /// </summary>
            protected int m_Y;

            /// <summary>
            /// The Y coordinate of this Area (center, not important)
            /// </summary>
            protected int m_Z;

            /// <summary>
            /// Returns the Height of this Area
            /// </summary>
            protected int m_Radius;

            /// <summary>
            /// The radius of the area in Coordinates
            /// </summary>
            public int Radius
            {
                get { return m_Radius; }
            }

            /// <summary>
            /// The Points string
            /// </summary>
            protected string m_stringpoints;

            /// <summary>
            /// The Points list
            /// </summary>
            protected IList<Point2D> m_points;

            public Polygon()
                : base()
            {
            }

            public Polygon(string desc, int x, int y, int z, int radius, string points)
                : base(desc)
            {
                m_Description = desc;
                m_X = x;
                m_Y = y;
                m_Z = z;
                m_Radius = radius;
                StringPoints = points;
            }

            /// <summary>
            /// Returns the X Coordinate of this Area (center, not important)
            /// </summary>
            public int X
            {
                get { return m_X; }
            }

            /// <summary>
            /// Returns the Y Coordinate of this Area (center, not important)
            /// </summary>
            public int Y
            {
                get { return m_Y; }
            }

            /// <summary>
            /// Returns the Z Coordinate of this Area (center, not important)
            /// </summary>
            public int Z
            {
                get { return m_Z; }
            }
           
            /// <summary>
            /// Get / Set(init) the serialized points
            /// </summary>
            public string StringPoints
            {
                get
                {
                    return m_stringpoints;
                }
                set
                {
                    m_stringpoints = value;
                    m_points = new List<Point2D>();
                    if (m_stringpoints.Length < 1) return;
                    string[] points = m_stringpoints.Split('|');
                    foreach (string point in points)
                    {
                        string[] pts = point.Split(';');
                        if (pts.Length != 2) continue;
                        int x = Convert.ToInt32(pts[0]);
                        int y = Convert.ToInt32(pts[1]);
                        Point2D p = new Point2D(x, y);
                        if (!m_points.Contains(p)) m_points.Add(p);
                    }
                }
            }

            /// <summary>
            /// Checks wether area intersects with given zone
            /// </summary>
            /// <param name="zone"></param>
            /// <returns></returns>
            public override bool IsIntersectingZone(Zone zone)
            {
                // TODO if needed
                if (X + Radius < zone.XOffset)
                    return false;
                if (X - Radius >= zone.XOffset + 65536)
                    return false;
                if (Y + Radius < zone.YOffset)
                    return false;
                if (Y - Radius >= zone.YOffset + 65536)
                    return false;

                return true;
            }

            public override bool IsContaining(int x, int y, int z, bool checkZ)
            {
                return IsContaining(new Point3D(x, y, z));
            }

            public override bool IsContaining(int x, int y, int z)
            {
                return IsContaining(new Point3D(x, y, z));
            }

            public override bool IsContaining(IPoint3D obj, bool checkZ)
            {
                return IsContaining(obj);
            }

            public override bool IsContaining(IPoint3D obj)
            {
                if (m_points.Count < 3) return false;
                Point2D p1, p2;
                bool inside = false;

                Point2D oldpt = new Point2D(m_points[m_points.Count - 1].X, m_points[m_points.Count - 1].Y);

                foreach (Point2D pt in m_points)
                {
                    Point2D newpt = new Point2D(pt.X, pt.Y);

                    if (newpt.X > oldpt.X) { p1 = oldpt; p2 = newpt; }
                    else { p1 = newpt; p2 = oldpt; }

                    if ((newpt.X < obj.X) == (obj.X <= oldpt.X)
                        && (obj.Y - p1.Y) * (p2.X - p1.X) < (p2.Y - p1.Y) * (obj.X - p1.X))
                        inside = !inside;

                    oldpt = newpt;
                }
                return inside;
            }

            public override void LoadFromDatabase(DBArea area)
            {
                m_Description = area.Description;
                m_X = area.X;
                m_Y = area.Y;
                m_Radius = area.Radius;
                StringPoints = area.Points;
            }
        }

		public class BindArea : Circle
		{
			protected BindPoint m_dbBindPoint;

			public BindArea()
				: base()
			{
				m_displayMessage = false;
			}

			public BindArea(string desc, BindPoint dbBindPoint)
				: base(desc, dbBindPoint.X, dbBindPoint.Y, dbBindPoint.Z, dbBindPoint.Radius)
			{
				m_dbBindPoint = dbBindPoint;
				m_displayMessage = false;
			}

			public BindPoint BindPoint
			{
				get { return m_dbBindPoint; }
			}

			public override void LoadFromDatabase(DBArea area)
			{
				base.LoadFromDatabase(area);

                m_dbBindPoint = new BindPoint
                {
                    Radius = (ushort)area.Radius,
                    X = area.X,
                    Y = area.Y,
                    Z = area.Z,
                    Region = area.Region
                };
            }
		}

		public class SafeArea : Circle
		{
			public SafeArea()
				: base()
			{
				m_safeArea = true;
			}

			public SafeArea(string desc, int x, int y, int z, int radius)
				: base
				(desc, x, y, z, radius)
			{
				m_safeArea = true;
			}
		}
	}
}

﻿using System;
using System.Numerics;

namespace DOL.GS.Geometry
{
	public readonly struct AABoundingBox : ICollider
	{
		public readonly Vector3 Min;
		public readonly Vector3 Max;
		public AABoundingBox Box => this;

		public AABoundingBox(Vector3 min, Vector3 max)
		{
			Min = min;
			Max = max;
		}

		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance)
		{
			if (ContainsPoint(origin))
				return 0;

			var t1 = (Min - origin) / direction;
			var t2 = (Max - origin) / direction;
			var tMin = Math.Max(Math.Max(Math.Min(t1.X, t2.X), Math.Min(t1.Y, t2.Y)), Math.Min(t1.Z, t2.Z));
			var tMax = Math.Min(Math.Min(Math.Max(t1.X, t2.X), Math.Max(t1.Y, t2.Y)), Math.Max(t1.Z, t2.Z));
			
			if (float.IsNaN(tMin) || float.IsNaN(tMax))
			{
				t1 = new Vector3(
					float.IsNaN(t1.X) ? float.PositiveInfinity : t1.X,
					float.IsNaN(t1.Y) ? float.PositiveInfinity : t1.Y,
					float.IsNaN(t1.Z) ? float.PositiveInfinity : t1.Z
				);
				t2 = new Vector3(
					float.IsNaN(t2.X) ? float.PositiveInfinity : t2.X,
					float.IsNaN(t2.Y) ? float.PositiveInfinity : t2.Y,
					float.IsNaN(t2.Z) ? float.PositiveInfinity : t2.Z
				);
				tMin = Math.Max(Math.Max(Math.Min(t1.X, t2.X), Math.Min(t1.Y, t2.Y)), Math.Min(t1.Z, t2.Z));
				tMax = Math.Min(Math.Min(Math.Max(t1.X, t2.X), Math.Max(t1.Y, t2.Y)), Math.Max(t1.Z, t2.Z));
			}

			// AABB is behind the origin
			if (tMax < 0)
				return maxDistance;

			// doesn't intersect
			if (tMin > tMax)
				return maxDistance;

			if (tMin < 0)
				return Math.Min(maxDistance, tMax);
			return Math.Min(maxDistance, tMin);
		}
		public float CollideWithRay(Vector3 origin, Vector3 direction, float maxDistance, ref RaycastStats stats)
		{
			stats.nbAABBTests += 1;
			return CollideWithRay(origin, direction, maxDistance);
		}

		public bool CollideWithAABB(AABoundingBox box)
		{
			return
				(Min.X <= box.Max.X && Max.X >= box.Min.X) &&
				(Min.Y <= box.Max.Y && Max.Y >= box.Min.Y) &&
				(Min.Z <= box.Max.Z && Max.Z >= box.Min.Z);
		}

		public bool ContainsPoint(Vector3 point)
		{
			return
				Min.X <= point.X && point.X <= Max.X &&
				Min.Y <= point.Y && point.Y <= Max.Y &&
				Min.Z <= point.Z && point.Z <= Max.Z;
		}
	}
}

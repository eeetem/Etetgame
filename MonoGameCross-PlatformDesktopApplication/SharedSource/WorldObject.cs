using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno.Prefabs;

namespace MultiplayerXeno
{
	public partial class WorldObject
	{

		public WorldObject(Vector2Int position, WorldObjectType type, int id)
		{

			this.Id = id;
			Position = position;
			this.Type = type;
#if CLIENT
			Transform = new Transform2();
#endif
		}

		public readonly int Id;


		public void Face(Direction dir)
		{
			if(!Type.Faceable) return;
			while (dir < 0)
			{
				dir += 8;
			}

			while (dir > (Direction) 7)
			{
				dir -= 8;
			}

			Facing = dir;
		}

	//	public Point3 UniqueId()
	//	{
	//		WorldObjectManager.ge
	//	}

		public readonly WorldObjectType Type;	
	
		public Direction Facing { get; private set;}

	

		public Vector2Int Position { get; protected set; }
		

		public Cover GetCover(Direction dir)
		{
			Direction resulting = dir - (int) Facing;
			if (resulting < 0)
			{
				resulting += 8;
			}

			return Type.Covers[resulting];

		}


		public enum Cover
		{
			None=0,//empty terain tiles
			Low=1,//small fences and such, visible when crouch
			High=2,//small walls and such, hidden when crouched
			Full=3,//full impassible walls
		}
		public enum Direction
		{
			North = 0,
			NorthEast = 1,
			East = 2,
			SouthEast = 3,
			South = 4,
			SouthWest = 5,
			West = 6,
			NorthWest = 7
			
		}
	}
	[Serializable]
	public struct Vector2Int
	{
		public bool Equals(Vector2Int other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector2Int other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}

		public int X;
		public int Y;

		public Vector2Int(int x, int y)
		{
			X = x;
			Y = y;
		}
		public static implicit operator Vector2(Vector2Int vec) => new Vector2(vec.X,vec.Y);
	
		public static Vector2Int operator*(Vector2Int a, int b)
			=> new Vector2Int(a.X*b, a.Y*b);
		public static Vector2 operator+(Vector2 a, Vector2Int b)
			=> new Vector2(a.X+b.X, a.Y+b.Y);
		public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => (lhs.X == rhs.X)&&(lhs.Y == rhs.Y);

		public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

		public override string ToString()
		{
			return "{X: " + X + " Y:" + Y + "}";
		}
	}
}
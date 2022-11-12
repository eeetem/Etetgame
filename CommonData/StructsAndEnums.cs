using System;
using Microsoft.Xna.Framework;

namespace CommonData
{
	
	[Serializable]
	public partial struct WorldTileData
	{
		public WorldObjectData? NorthEdge;
		public WorldObjectData? WestEdge;
		public WorldObjectData? ObjectAtLocation;
		public WorldObjectData? Surface;
		public Vector2Int position;
		public WorldTileData(Vector2Int position)
		{
			this.position = position;
			NorthEdge = null;
			WestEdge = null;
			ObjectAtLocation = null;
			Surface = null;
		}
	}
	[Serializable]
	public partial struct WorldObjectData
	{
		public Direction Facing;
		public int Id;

		public bool fliped;
		//health
		public string Prefab;
		public ControllableData? ControllableData;
		public WorldObjectData(string prefab)
		{
			this.Prefab = prefab;
			this.Id = -1;
			Facing = Direction.North;
			ControllableData = null;
			fliped = false;
		}
	}
	[Serializable]
	public partial struct ControllableData
	{
		public bool Team1;
		public int ActionPoints;
		public int MovePoints;
		public int TurnPoints;
		public int Health;
		public int Awareness;
		public ControllableData(bool team1)
		{
			Team1 = team1;
			ActionPoints = -1;
			MovePoints = -1;
			TurnPoints = -1;
		}
		public ControllableData(bool team1, int actionPoints, int movePoints, int turnPoints, int health, int awareness)
		{
			Team1 = team1;
			ActionPoints = actionPoints;
			MovePoints = movePoints;
			TurnPoints = turnPoints;
			Health = health;
			Awareness = awareness;
		}
	}
	public enum Cover
	{
		None=0,//empty terain tiles
		Low=1,//small fences and such, visible when crouch
		High=2,//small walls and such, hidden when crouched
		Full=3,//full impassible walls
	}
	[Serializable]
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
	[Serializable]
	public class Vector2Int//should be a struct but networking library is cringe once again
	{
		public bool Equals(Vector2Int other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			return obj is Vector2Int other && Equals(other);
		}

		public double Magnitude()
		{
			return Math.Sqrt((X * X) + (Y * Y));
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(X, Y);
		}

		public static double SqrDistance(Vector2Int from, Vector2Int to)
		{
			int x = from.X - to.X;
			int y = from.Y - to.Y;
			return y * y + x * x;

		}
	

		public int X { get; set; }
		public int Y { get; set; }



		public Vector2Int(int x, int y)
		{
			X = x;
			Y = y;
		}
		public static implicit operator Vector2(Vector2Int vec) => new Vector2(vec.X,vec.Y);
		public static implicit operator Vector2Int(Vector2 vec) => new Vector2Int((int)vec.X,(int)vec.Y);
	
		public static Vector2Int operator*(Vector2Int a, int b)
			=> new Vector2Int(a.X*b, a.Y*b);
		public static Vector2 operator+(Vector2 a, Vector2Int b)
			=> new Vector2(a.X+b.X, a.Y+b.Y);
		
		public static Vector2Int operator+(Vector2Int a, Vector2Int b)
			=> new Vector2Int(a.X+b.X, a.Y+b.Y);
		
		public static Vector2Int operator-(Vector2Int a, Vector2Int b)
			=> new Vector2Int(a.X-b.X, a.Y-b.Y);
		public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => (lhs?.X == rhs?.X)&&(lhs?.Y == rhs?.Y);

		public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

		public override string ToString()
		{
			return "{X: " + X + " Y:" + Y + "}";
		}
	
		public void Deconstruct(out int X, out int Y)
		{
			X = this.X;
			Y = this.Y;
		}
		
	
	}
}
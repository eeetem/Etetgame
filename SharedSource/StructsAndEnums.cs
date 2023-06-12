using Microsoft.Xna.Framework;

namespace MultiplayerXeno
{
	
	[Serializable]
	public struct WorldTileData
	{
		public WorldObjectData? NorthEdge;
		public WorldObjectData? WestEdge;
		public WorldObjectData? Surface;
		public WorldObjectData? UnitAtLocation;
		public Vector2Int position;
		public List<WorldObjectData> ObjectsAtLocation;
		public WorldTileData(Vector2Int position)
		{
			this.position = position;
			NorthEdge = null;
			WestEdge = null;
			Surface = null;
			UnitAtLocation = null;
			ObjectsAtLocation = new List<WorldObjectData>();
		}
	}


	

	public enum TargetingType
	{
		Auto,
		High,
		Low
	}


	[Serializable]
	public partial struct WorldObjectData
	{
		public Direction Facing;
		public int ID;

		public bool Fliped;
		//health
		public string Prefab;
		public UnitData? ControllableData;
		public int Health;
		public int Lifetime;
		public WorldObjectData(string prefab)
		{
			Prefab = prefab;
			ID = -1;
			Facing = Direction.North;
			ControllableData = null;
			Fliped = false;
			Health = -100;
			Lifetime = -100;
		}
	}
	[Serializable]
	public struct UnitData
	{
		public bool Team1;
		public int ActionPoints;
		public int MovePoints;
		public bool? canTurn;
		public int Determination;
		public bool Crouching;
		public bool Panic;
		public bool JustSpawned;
		public bool overwatch;
		public int selectIndex;
		public string? LastItem;
		public List<string?> Inventory { get; set; }
		public List<Tuple<string?, int>> StatusEffects { get; set; }
		
		public UnitData(bool team1)
		{
			Team1 = team1;
			ActionPoints = -100;
			MovePoints = -100;
			canTurn = null;
			Determination = -100;
			Crouching = false;
			Panic = false;
			JustSpawned = true;//it's always truea nd only set to false in getData
			overwatch = false;
			Inventory = new List<string?>();
			StatusEffects = new List<Tuple<string?, int>>();
			selectIndex = -1;
			LastItem = null;
		}
		public UnitData(bool team1, int actionPoints, int movePoints, bool canTurn, int determination, bool crouching,bool panic,List<string?> inv, List<Tuple<string?, int>> sts,bool overwatch,int selectIndex,string? lastItem)
		{
			Team1 = team1;
			ActionPoints = actionPoints;
			MovePoints = movePoints;
			this.canTurn = canTurn;
			Determination = determination;
			Crouching =	crouching;
			JustSpawned = true;
			Panic = panic;
			Inventory = inv;
			StatusEffects = sts;
			this.overwatch = overwatch;
			this.selectIndex = selectIndex;
			LastItem = lastItem;
		}

	
	}
	[Serializable]
	public class RayCastOutcome//fuck the network library holy moly
	{
		public Vector2 CollisionPointLong;
		public Vector2 CollisionPointShort;
		public Vector2 StartPoint;
		public Vector2 EndPoint;
		public Vector2 VectorToCenter;
		public List<Vector2Int> Path;
		public int HitObjId{ get; set; }

		public bool hit{ get; set; }

		public RayCastOutcome(Vector2 start, Vector2 end)
		{
			CollisionPointLong = new Vector2(0, 0);
			CollisionPointShort = new Vector2(0, 0);
			hit = false;
			EndPoint = end;
			HitObjId = -1;
			StartPoint = start;
			Path = new List<Vector2Int>();
	
		}
	}
	public enum Visibility
	{
		None=0,
		Partial=1,
		Full=2
	}
	public enum GameState
	{
		Lobby=0,
		Setup=1,
		Playing=2,
		Over=3,
		Editor = 4,
	}

	public enum Cover
	{
		None=0,//empty terain tiles
		Low=1,//small fences and such, visible when crouch
		High=2,//small walls and such, hidden when crouched
		Full=3,//full impassible walls
		Ignore=1000,//for raycasts
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

	public struct PreviewData
	{
		public int totalDmg;
		public int finalDmg;
		public int coverBlock;
		public int distanceBlock;
		public int determinationBlock;
		public int detDmg;

		public PreviewData(int totalDmg, int detDmg)
		{
			this.totalDmg = totalDmg;
			finalDmg = 0;
			coverBlock = 0;
			distanceBlock = 0;
			determinationBlock = 0;
			this.detDmg = detDmg;
		}

	}

	[Serializable]
	public class Vector2Int//should be a struct but networking library is cringe once again
	{
		public bool Equals(Vector2Int other)
		{
			return X == other.X && Y == other.Y;
		}

		public override bool Equals(object? obj)
		{
			return obj is Vector2Int other && Equals(other);
		}

		public double Magnitude()
		{
			return Math.Sqrt(X * X + Y * Y);
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
		public static Vector2 operator+(Vector2Int a, Vector2 b)
			=> new Vector2(a.X+b.X, a.Y+b.Y);
		
		public static Vector2Int operator+(Vector2Int a, Vector2Int b)
			=> new Vector2Int(a.X+b.X, a.Y+b.Y);
		
		public static Vector2Int operator-(Vector2Int a, Vector2Int b)
			=> new Vector2Int(a.X-b.X, a.Y-b.Y);
		public static bool operator ==(Vector2Int? lhs, Vector2Int? rhs) => lhs?.X == rhs?.X&&lhs?.Y == rhs?.Y;
	
		public static bool operator !=(Vector2Int? lhs, Vector2Int? rhs) => !(lhs == rhs);
	
		public override string ToString()
		{
			return "{X: " + X + " Y:" + Y + "}";
		}
	
		public void Deconstruct(out int X, out int Y)
		{
			X = this.X;
			Y = this.Y;
		}


		public static Vector2Int Parse(string innerText)
		{
			var split = innerText.Split(",");
			return new Vector2Int(int.Parse(split[0]) ,int.Parse(split[1]));
		}
	}
}
using System;
using Microsoft.Xna.Framework;
using Riptide;

namespace DefconNull;



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

public struct Vector2Int : IMessageSerializable
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
	public static bool operator ==(Vector2Int? lhs, Vector2Int? rhs) => lhs.Equals(rhs);
	
	public static bool operator !=(Vector2Int? lhs, Vector2Int? rhs) => !(lhs == rhs);
	
	public override string ToString()
	{
		return "{X: " + X + " Y: " + Y + "}";
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

	public void Serialize(Message message)
	{
		message.AddInt(X);
		message.AddInt(Y);
	}

	public void Deserialize(Message message)
	{
		X = message.GetInt();
		Y = message.GetInt();
	}
}
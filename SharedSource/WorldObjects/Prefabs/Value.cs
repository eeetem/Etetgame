using System;

namespace DefconNull.World.WorldObjects;

public struct Value
{
	public bool Equals(Value other)
	{
		return Max == other.Max && Current == other.Current;
	}

	public override bool Equals(object? obj)
	{
		return obj is Value other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Max, Current);
	}

	public int Max;
	public int Current;

	public void Reset()
	{
		Current = Max;
	}

	public Value(int current, int max)
	{
		Max = max;
		Current = current;
	}
	


	public static bool operator <=(Value a, int b)
	{
		return a.Current <= b;
	}
	
	public static bool operator ==(int a, Value b)
	{
		return a == b.Current;
	}

	public static bool operator !=(int a, Value b)
	{
		return !(a == b);
	}

	public static bool operator ==(Value a, int b)
	{
		return a.Current == b;
	}

	public static bool operator !=(Value a, int b)
	{
		return !(a == b);
	}

	public static bool operator >=(Value a, int b)
	{
		return a.Current >= b;
	}
	public static bool operator >=(int a, Value b)
	{
		return a >= b.Current;
	}

	public static bool operator <=(int a, Value b)
	{
		return a <= b.Current;
	}

	public static bool operator <(Value a, int b)
	{
		return a.Current < b;
	}

	public static bool operator >(Value a, int b)
	{
		return a.Current > b;
	}
	
	public static bool operator <(int a, Value b)
	{
		return a < b.Current;
	}

	public static bool operator >(int a, Value b)
	{
		return a > b.Current;
	}
	public static Value operator +(Value a, int b)
	{
		return new Value( a.Current + b,a.Max);
	}
	public static Value operator +(int a, Value b)
	{
		return new Value( a + b.Current,b.Max);
	}

	public static Value operator -(Value a, int b)
	{
		return new Value( a.Current - b,a.Max);
	}
	public static Value operator ++(Value a)
	{
		return new Value(a.Current + 1,a.Max);
	}
	public static Value operator --(Value a)
	{
		return new Value(a.Current - 1,a.Max);
	}


}
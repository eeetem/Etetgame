﻿using Riptide;

namespace DefconNull.WorldObjects;

public struct ValueChange : IMessageSerializable
{
	public bool Set = false;
	public bool Cap = false;
	public int Value;

	public override string ToString()
	{
		return $"{nameof(Set)}: {Set}, {nameof(Cap)}: {Cap}, {nameof(Value)}: {Value}";
	}

	public bool Equals(ValueChange other)
	{
		return Set == other.Set && Cap == other.Cap && Value == other.Value;
	}

	public override bool Equals(object? obj)
	{
		return obj is ValueChange other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = Set.GetHashCode();
			hashCode = (hashCode * 397) ^ Cap.GetHashCode();
			hashCode = (hashCode * 397) ^ Value;
			return hashCode;
		}
	}

	public ValueChange()
	{
		Set = false;
		Cap = false;
		Value = 0;
	}
	
	public void Serialize(Message message)
	{
		message.Add(Set);
		message.Add(Cap);
		message.Add(Value);
	}

	public void Deserialize(Message message)
	{
		Set = message.GetBool();
		Cap = message.GetBool();
		Value = message.GetInt();
	}

	public ValueChange(int input)
	{
		Value = input;
	}

	public ValueChange(string input)
	{
		if (input.Contains("[cap]"))
		{
			Cap = true;
			input = input.Replace("[cap]", "");
		}

		if (input.Contains("[set]"))
		{
			Set = true;
			input = input.Replace("[set]", "");
		}

		Value = int.Parse(input);
	}

	public readonly int GetChange(Value field)
	{
		int newValue = field.Current;
		if (Set)
		{
			newValue = Value;
		}
		else
		{
			newValue += Value;
		}

		if (Cap)
		{
			if (newValue > field.Max)
			{
				newValue = field.Max;
				if (newValue < field.Current)//if we were over the cap dont pull us down
				{
					newValue=field.Current;
				}
			}

			if (newValue < 0)
			{
				newValue = 0;
				if (field.Current < 0)//if we were over the cap dont pull us down
				{
					newValue=field.Current;
				}
			}
		}


		return newValue - field.Current;
	}

	public readonly void Apply(ref Value field)
	{
		
		field += GetChange(field);
	}


}
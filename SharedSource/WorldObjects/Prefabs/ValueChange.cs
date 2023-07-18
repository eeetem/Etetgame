using Riptide;

namespace DefconNull.World.WorldObjects;

public struct ValueChange : IMessageSerializable
{
	public bool Set = false;
	public bool Cap = false;
	public int Value;

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

	public int GetChange(Value field)
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

	public void Apply(ref Value field)
	{
		
		field += GetChange(field);
	}


}
namespace MultiplayerXeno;

public struct ValueChange
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
			}

			if (field < 0)
			{
				field.Current = 0;
			}
		}


		return newValue - field.Current;
	}

	public void Apply(ref Value field)
	{
		field += GetChange(field);

	}
}
using System;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class VariableValue
{

	string? value;
	string? var;
	string? varParam;
	bool _targetSelfNotOther;
	public VariableValue(string input)
	{
		if (!input.Contains("{"))
		{
			value = input;
			var = null;
			return;
		}
		input = input.Replace("{", "");
		input = input.Replace("}", "");
		var inputs = input.Split(".");
		if (inputs[0]=="this")
		{
			_targetSelfNotOther = true;
		}else if(inputs[0]=="target")
		{
			_targetSelfNotOther = false;
		}
		else
		{
			throw new Exception("Invalid Variable Value");
		}

		var = inputs[1];
		varParam = inputs[2];
		value = null;
	}

	public string GetValue(Controllable User,Controllable Other)
	{
		if (value != null)
		{
			return value;
		}

		if (_targetSelfNotOther)
		{
			return User.GetVar(var,varParam);
		}
		
		return Other.GetVar(var,varParam);
		
		


	}
}
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

	public string GetValue(Unit user,Unit other)
	{
		if (value != null)
		{
			return value;
		}

		if (_targetSelfNotOther)
		{
			return user.GetVar(var!,varParam);
		}
		
		return other.GetVar(var!,varParam);
		
		


	}
}
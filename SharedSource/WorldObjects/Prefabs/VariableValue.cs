using System;
using Riptide;

namespace DefconNull.World.WorldObjects;

public class VariableValue : IMessageSerializable
{

	string? value;
	string? var;
	string? varParam;
	bool _targetSelfNotOther;
	
	public void Serialize(Message message)
	{
		message.Add(_targetSelfNotOther);
		message.AddNullableString(var);
		message.AddNullableString(varParam);
		message.AddNullableString(value);
	}

	public void Deserialize(Message message)
	{
		_targetSelfNotOther = message.GetBool();
		var = message.GetNullableString();
		varParam = message.GetNullableString();
		value = message.GetNullableString();
	}
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

	public VariableValue()
	{
		
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
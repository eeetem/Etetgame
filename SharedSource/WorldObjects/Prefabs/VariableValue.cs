using System;
using Riptide;

namespace DefconNull.WorldObjects;

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

	protected bool Equals(VariableValue other)
	{
		return value == other.value && var == other.var && varParam == other.varParam && _targetSelfNotOther == other._targetSelfNotOther;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((VariableValue) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hashCode = (value != null ? value.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (var != null ? var.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ (varParam != null ? varParam.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ _targetSelfNotOther.GetHashCode();
			return hashCode;
		}
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

	public string GetValue(Unit? user,Unit? other)
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
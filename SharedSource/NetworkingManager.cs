using System;
using Riptide;

namespace DefconNull;


public class SquadMember : IMessageSerializable
{
	public string Prefab { get; set; } = "";
	public Vector2Int Position { get; set; } = new Vector2Int(0, 0);

	public void Serialize(Message message)
	{
		message.Add(Prefab);
		message.Add(Position);

	}

	protected bool Equals(SquadMember other)
	{
		return Prefab == other.Prefab && Position.Equals(other.Position);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
		return Equals((SquadMember) obj);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Prefab, Position);
	}

	public void Deserialize(Message message)
	{
		Prefab = message.GetString();
		Position = message.GetSerializable<Vector2Int>();
	
	}
	
}
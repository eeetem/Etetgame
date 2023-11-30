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

	public void Deserialize(Message message)
	{
		Prefab = message.GetString();
		Position = message.GetSerializable<Vector2Int>();
	
	}
}
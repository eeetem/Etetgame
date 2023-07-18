using Riptide;
using System.Collections.Generic;
using System.Linq;

namespace DefconNull.Networking;


public class SquadMember : IMessageSerializable
{
	public string Prefab { get; set; } = "";
	public Vector2Int Position { get; set; } = new Vector2Int(0, 0);
	public List<string> Inventory { get; set; } = new List<string>();
		
	public void Serialize(Message message)
	{
		message.Add(Prefab);
		message.Add(Position);
		message.AddStrings(Inventory.ToArray());
			
	}

	public void Deserialize(Message message)
	{
		Prefab = message.GetString();
		Position = message.GetSerializable<Vector2Int>();
		Inventory =message.GetStrings().ToList();
	}
}
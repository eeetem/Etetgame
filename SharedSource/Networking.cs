using System.Collections.Generic;
using System.Linq;
using Riptide;

namespace MultiplayerXeno;

public partial class Networking
{
	public enum NetworkMessageID : ushort
	{
		
		Notify = 1,
		Chat = 2,
		PreGameData = 3,
		Kick = 4,
		MapDataInitiate =5,
		MapDataFinish =6,
		GameData =7,
		GameAction =8,
		StartGame =9,
		EndTurn =10,
		SquadComp =11,
		MapUpload =12,
		TileUpdate=13,
		MapDataInitiateConfirm = 14,
		ReplaySequence = 15,
	}
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



	
	
}
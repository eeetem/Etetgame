using Network.Packets;

namespace MultiplayerXeno
{
	public class GameActionPacket : Packet
	{
		public ActionType Type { get; set; }
		public int ID { get; set; }
		
		public Vector2Int? Target { get; set; }
		
		public List<string> args { get; set;}

		public GameActionPacket(int id, Vector2Int? target, ActionType type)
		{
			ID = id;
			Target = target;
			Type = type;
			args = new List<string>();
		}

	}

	public enum ActionType
	{
		EndTurn=0,
		Attack=1,
		Move=2,
		Face=3,
		Crouch=4,
		OverWatch = 5,
		UseItem = 6,
		UseAbility = 7,
		
	}





}
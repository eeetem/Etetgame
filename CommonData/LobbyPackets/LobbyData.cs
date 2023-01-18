using Network.Packets;

namespace CommonData;
	public class LobbyData: Packet
	{
		public string Name { get; set; }
		public int Port{ get; set; }
		public int Players{ get; set; }
		
		public bool HasPassword{ get; set; }
		//spectators
		public LobbyData(string name, int playerCount, int port)
		{
			this.Name = name;
			this.Players = playerCount;
			this.Port = port;
			HasPassword = false;
		}
	}



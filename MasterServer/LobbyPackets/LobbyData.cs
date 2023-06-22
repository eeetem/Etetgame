using Network.Packets;

namespace MultiplayerXeno;
	public class LobbyData: Packet
	{
		public string Name { get; set; }
		public string MapName { get; set; }
		public int PlayerCount { get; set; }
		public int Spectators { get; set; }
		public string GameState { get; set; }
		public int Port{ get; set; }
		public bool HasPassword{ get; set; }
		//spectators
		public LobbyData(string name, int port)
		{
			Name = name;
			Port = port;
			HasPassword = false;
			MapName = "Unknown";
			PlayerCount = 0;
			Spectators = 0;
			GameState = "Starting...";
		}
	}



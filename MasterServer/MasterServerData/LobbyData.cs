using Riptide;

namespace DefconNull.Networking;
	public class LobbyData:IMessageSerializable
	{
		public string Name { get; set; } = "";
		public string MapName { get; set; } = "";
		public int PlayerCount { get; set; }
		public int Spectators { get; set; }
		public string GameState { get; set; } = "";
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

		public LobbyData()
		{
			
		}

		public void Serialize(Message message)
		{
			message.Add(Name);
			message.Add(MapName);
			message.Add(PlayerCount);
			message.Add(Spectators);
			message.Add(GameState);
			message.Add(Port);
			message.Add(HasPassword);
		}

		public void Deserialize(Message message)
		{
			Name = message.GetString();
			MapName = message.GetString();
			PlayerCount = message.GetInt();
			Spectators = message.GetInt();
			GameState = message.GetString();
			Port = message.GetInt();
			HasPassword = message.GetBool();
		}
	}



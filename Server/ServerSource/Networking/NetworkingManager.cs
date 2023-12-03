using System.Reflection;
using DefconNull.ReplaySequence;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{
	private static Server server = null!;
	private static string selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Ground Zero.mapdata";
	private static bool SinglePlayerFeatures = false;
	public static void Start(ushort port, bool allowSP)
	{
		SinglePlayerFeatures = allowSP;
		RiptideLogger.Initialize(Console.WriteLine, Console.WriteLine,Console.WriteLine,Console.WriteLine, true);
		//1. Start listen on a portw
		server = new Server(new TcpServer());
		server.TimeoutTime = 10000;
		Message.MaxPayloadSize = 2048*2;
#if DEBUG
		server.TimeoutTime = ushort.MaxValue;
#endif

		server.ClientConnected += (a, b) => { Console.WriteLine($" {b.Client.Id} connected (Clients: {server.ClientCount}), awaiting registration...."); };//todo kick without registration
		server.HandleConnection += HandleConnection;
		server.ClientDisconnected += ClientDisconnected;

		selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Ground Zero.mapdata";
		WorldManager.Instance.LoadMap(selectedMap);
		server.Start(port, 10);
			
		Console.WriteLine("Started server at port" + port);
		
	}

	private static void HandleConnection(Connection connection, Message connectmessage)
	{
		string name = connectmessage.GetString();
		Console.WriteLine("Begining Client Register: "+name);
		if(name.Contains('.')||name.Contains(';')||name.Contains(':')||name.Contains(',')||name.Contains('[')||name.Contains(']')||name=="AI"||name=="Practice Opponent")
		{
			var msg = Message.Create();
			msg.AddString("Invalid Name");
			server.Reject(connection,msg);
			return;
		}



		if (GameManager.Player1 == null)
		{
			GameManager.Player1 = new ClientInstance(name,connection);
			SendChatMessage(name+" joined as Player 1");
		}
		else if (GameManager.Player1.Name == name)
		{
			if (GameManager.Player1.Connection != null && !GameManager.Player1.Connection.IsNotConnected)
			{
				var msg = Message.Create();
				msg.AddString("Player with same name is already in the game");
				server.Reject(connection,msg);
				return;
			}

			GameManager.Player1.Connection = connection;//reconnection
			SendChatMessage(name+" reconnected as Player 1");
				
		}
		else if (GameManager.Player2 == null)
		{
			
			GameManager.Player2 = new ClientInstance(name,connection);
			SendChatMessage(name+" joined as Player 2");
		}
		else if (GameManager.Player2.Name == name)
		{
			if (GameManager.Player2.Connection != null && !GameManager.Player2.Connection.IsNotConnected)
			{
				var msg = Message.Create();
				msg.AddString("Player with same name is already in the game");
				server.Reject(connection,msg);
				return;
			}
			GameManager.Player2.Connection = connection;//reconnection
			SendChatMessage(name+" reconnected as Player 2");
		}
		else
		{
			GameManager.Spectators.Add(new ClientInstance(name,connection));
			SendChatMessage(name+" joined the spectators");
		}

		

		Console.WriteLine("Client Register Done");
		server.Accept(connection);
		SendGameData();
		SendPreGameInfo(); 
		//SendMapData(connection);

	}
		
	public static readonly object UpdateLock = new object();
	private static void SendMapData(ushort senderID)
	{
		Connection c;
		server.TryGetClient(senderID, out c);
		SendMapData(c);
	}
	public static void SendMapData(Connection connection)
	{
	
		Console.WriteLine("initiating sending map data to "+connection.Id+"...");
		WorldManager.Instance.SaveCurrentMapTo("temp.mapdata");//we dont actually read the file but we call this so the currentMap updates
		var packet = Message.Create(MessageSendMode.Reliable, NetworkMessageID.MapDataInitiate);
		packet.AddString(WorldManager.Instance.CurrentMap.Name);
		packet.AddString(WorldManager.Instance.CurrentMap.Author);
		packet.AddInt(WorldManager.Instance.CurrentMap.unitCount);
		server.Send(packet,connection);
		
		Task.Run(() => {
			while (!ClientsReadyForMap.Contains(connection.Id) || SequenceManager.SequenceRunning)
			{
				Thread.Sleep(100);
			}

			lock (UpdateLock)
			{
				try
				{
					Console.WriteLine("Actually sending map data to " + connection.Id + "...");

					for (int x = 0; x < 100; x++)
					{
						for (int y = 0; y < 100; y++)
						{
							WorldTile tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
							if (tile.NorthEdge != null || tile.WestEdge != null || tile.Surface != null || tile.ObjectsAtLocation.Count != 0 || tile.UnitAtLocation != null)
							{
								
								ForceSendTileUpdate(tile, connection); //only send updates about tiles that have something on them
							}

							
						}
					}

					Console.WriteLine("finished sending map data to " + connection.Id);
					var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.MapDataFinish);
					msg.AddString(WorldManager.Instance.GetMapHash());
					server.Send(msg, connection);

				}catch(Exception e)
				{
					Console.WriteLine("Error sending map data to " + connection.Id);
					Console.WriteLine(e);
				}

		
			}

		});

	}

	public static void Kick(string reason, ushort id)
	{
		Connection c;
		server.TryGetClient(id, out c);
		Kick(reason,c);
	}

	public static void Kick(string reason,Connection connection)
	{
		Console.WriteLine("Kicking " + connection.Id + " for " + reason);
		if (connection.IsConnected)
		{
			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.Notify);
			msg.AddString(reason);
			server.DisconnectClient(connection,msg);
		}

		if (GameManager.Player1?.Connection == connection)
		{
			GameManager.Player1.Connection = null;
		}else if (GameManager.Player2?.Connection == connection)
		{
			GameManager.Player2.Connection = null;
		}
	}
		
	private static void ClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
	{
		Console.WriteLine($"Connection lost. Reason {e.Reason} {server.ClientCount}");
		string name;
		if (e.Client == GameManager.Player1?.Connection)
		{
			name = GameManager.Player1.Name;
			GameManager.Player1.Connection = null;
			SendChatMessage(name+" left the game");
		}else if (e.Client == GameManager.Player2?.Connection)
		{
			name = GameManager.Player2.Name;
			GameManager.Player2.Connection = null;
			SendChatMessage(name+" left the game");
		}
		else
		{
			var client = GameManager.Spectators.Find((a) => { return a.Connection == e.Client; });
			name = client.Name;
			GameManager.Spectators.Remove(client);
			SendChatMessage(name+" stopped spectating");

		}
		Thread.Sleep(1000);
		SendPreGameInfo();
#if !DEBUG
				Task.Run(() => { 
			Thread.Sleep(15000);
			if (server.ClientCount == 0)
			{
				Environment.Exit(0);
			}});

#endif


	}



	public static void NotifyAll(string message)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.Notify);
		msg.AddString(message);
		server.SendToAll(msg);
	}

	public static void SendChatMessage(string text)
	{
		text = "[Yellow]" + text + "[-]";
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.Chat);
		msg.AddString(text);
		server.SendToAll(msg);

	}

	public static void SendPreGameInfo()
	{
		var data = new GameManager.PreGameDataStruct();
		data.HostName = GameManager.Player1 != null ? GameManager.Player1.Name : "None";
		data.Player2Name = GameManager.Player2 != null ? GameManager.Player2.Name : "None";
		if (GameManager.Player2 != null &&( GameManager.Player2.Connection ==null || GameManager.Player2.Connection.IsNotConnected))
		{
			data.Player2Name = "Reserved: " + data.Player2Name;
		}
		data.Spectators = new List<string>();
		foreach (var spectator in GameManager.Spectators)
		{
			data.Spectators.Add(spectator.Name);
		}
		data.MapList = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/", "*.mapdata").ToList();

		string customMapDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Maps/Custom";
		if(Directory.Exists(customMapDirectory)){
			data.CustomMapList = Directory.GetFiles(customMapDirectory, "*.mapdata").ToList();
		}

		data.SinglePLayerFeatures = SinglePlayerFeatures;
		


		data.SelectedMap = selectedMap;
		data.TurnTime = GameManager.PreGameData.TurnTime;

		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.PreGameData);
		msg.Add(data);
		server.SendToAll(msg);
		GameManager.PreGameData = data;
		Program.InformMasterServer();
	}


	public static void ForceSendTileUpdate(WorldTile tile, Connection con)
	{
		Console.WriteLine("force Sending tile at " + tile.Position.X + "," + tile.Position.Y);
		var msg = Message.Create(MessageSendMode.Unreliable, NetworkMessageID.TileUpdate);
		WorldTile.WorldTileData worldTileData = tile.GetData();
		msg.Add(worldTileData);
		server.Send(msg,con);
	}

	static Dictionary<Vector2Int,ValueTuple<string,string>> tileUpdateLog = new Dictionary<Vector2Int, (string, string)>();
	public static void SendTileUpdate(WorldTile tile)
	{
		
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.TileUpdate);
		WorldTile.WorldTileData worldTileData = tile.GetData();
		//worldTileData.forceRebuild = false;
		msg.Add(worldTileData);
		


		//add the entry if it's missing
		tileUpdateLog.TryAdd(tile.Position, (string.Empty,string.Empty));

		bool sent = false;
		if (tile.IsVisible(team1:true)&& GameManager.Player1 is not null && GameManager.Player1.Connection is not null &&tileUpdateLog[tile.Position].Item1 != tile.GetHash())
		{
			tileUpdateLog[tile.Position] = (tile.GetHash(), tileUpdateLog[tile.Position].Item2);
			
			server.Send(msg,GameManager.Player1.Connection,false);
			sent = true;
		}
		if (tile.IsVisible(team1:false) && GameManager.Player2 is not null && GameManager.Player2.Connection is not null &&tileUpdateLog[tile.Position].Item2 != tile.GetHash())
		{
			tileUpdateLog[tile.Position] = (tileUpdateLog[tile.Position].Item1,tile.GetHash());
			server.Send(msg,GameManager.Player2.Connection,false);
			sent = true;
		}

		//if we sent atleast to 1 player also update the spectators
		if (sent)
		{
			foreach (var spectator in GameManager.Spectators)
			{
				server.Send(msg, spectator.Connection, false);
			}
		}

		msg.Release();
		
	}

	public static void Update()
	{
		lock (UpdateLock)
		{
			server.Update();
		}
	}

	public static void SendGameData()
	{
		Console.WriteLine("sending game data");
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.GameData);
		var state = GameManager.GetState();
		state.IsPlayerOne = true;
		msg.Add(state);
		if (GameManager.Player1 is not null  && GameManager.Player1.Connection is not null)
		{
			server.Send(msg, GameManager.Player1?.Connection);
		}

		var msg2 = Message.Create(MessageSendMode.Reliable, NetworkMessageID.GameData);
		state.IsPlayerOne = false;
		msg2.Add(state);
		if (GameManager.Player2 is not null && GameManager.Player2.Connection is not null && !GameManager.Player2.IsPracticeOpponent && !GameManager.Player2.IsAI) 
		{
			server.Send(msg2, GameManager.Player2?.Connection); 
		}
		
		var msg3 = Message.Create(MessageSendMode.Reliable, NetworkMessageID.GameData);
		state.IsPlayerOne = null;//spectators dont care about isPlayerOne field
		msg3.Add(state);

		foreach (var spectator in GameManager.Spectators)
		{
			server.Send(msg3, spectator.Connection,false);
		}
		msg3.Release();
		Program.InformMasterServer();
			
	}

	public static void SendSequence(SequenceAction action, bool force = false)
	{
		SendSequence(new List<SequenceAction>(){action},force);
	}

	public static void SendSequence(IEnumerable<SequenceAction> actions)
	{
		SendSequence(actions.ToList());
	}

	public static void SendSequence(List<SequenceAction> actions, bool force = false)
	{
		actions = new List<SequenceAction>(actions);
		actions.RemoveAll(x => !x.ShouldDo());
		//send in chunks
		if (actions.Count() > 25)
		{
			SendSequence(actions.GetRange(0, 25));
			actions.RemoveRange(0, 25);
			SendSequence(actions);
			return;
		}


		lock (UpdateLock)
		{
			
            
			if (force)
			{
				var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.ReplaySequence);
				msg.Add((ushort)ReplaySequenceTarget.All);
				
				foreach (var a in actions)
				{
					msg.Add((int) a.GetSequenceType());
					msg.AddSerializable(a);
				}
				server.SendToAll(msg);
			}
			else
			{

				SendSequenceToPlayer(actions, true);
				SendSequenceToPlayer(actions, false);
			}

		}

	}

	public static void SendSequenceToPlayer(List<SequenceAction> actions, bool player1)
	{
		actions.RemoveAll(x => !x.ShouldDoServerCheck(player1));
		lock (UpdateLock)
		{
			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.ReplaySequence);
			if (player1)
			{
				msg.Add((ushort)ReplaySequenceTarget.Player1);
			}
			else
			{
				msg.Add((ushort)ReplaySequenceTarget.Player2);
			}

			foreach (var a in actions)
			{
				msg.Add((int) a.GetSequenceType());
				msg.AddSerializable(a);
			}

			if (player1 && GameManager.Player1 != null && GameManager.Player1.Connection != null)
			{
				server.Send(msg,GameManager.Player1.Connection);
			}
			else if (!player1 && GameManager.Player2 != null && GameManager.Player2.Connection != null)
			{
				server.Send(msg,GameManager.Player2.Connection);
			}
			

			foreach (var spec in GameManager.Spectators)
			{
				server.Send(msg, spec.Connection);
			}

			
		}
	}

	public static void SendEndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.EndTurn);
		server.SendToAll(msg);
	}


}
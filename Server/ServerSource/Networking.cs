using System.Reflection;
using MultiplayerXeno.ReplaySequence;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Transports.Udp;
using Riptide.Utils;

namespace MultiplayerXeno;

public static partial class Networking
{
	private static Server server = null!;
	private static string selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Ground Zero.mapdata";

	public static void Start(ushort port)
	{
		
		RiptideLogger.Initialize(Console.WriteLine, Console.WriteLine,Console.WriteLine,Console.WriteLine, true);
		//1. Start listen on a portw
		server = new Server(new TcpServer());
		Message.MaxPayloadSize = 2048;
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
		if(name.Contains('.')||name.Contains(';')||name.Contains(':')||name.Contains(',')||name.Contains('[')||name.Contains(']'))
		{
			var msg = Message.Create();
			msg.AddString("Invalid Name");
			server.Reject(connection,msg);
			return;
		}

		if (GameManager.Player1 == null)
		{
			GameManager.Player1 = new Client(name,connection);
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
			
			GameManager.Player2 = new Client(name,connection);
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
			GameManager.Spectators.Add(new Client(name,connection));
			SendChatMessage(name+" joined the spectators");
		}

			

		Console.WriteLine("Client Register Done");
		server.Accept(connection);
		SendGameData();
		SendPreGameInfo();
		SendMapData(connection);

	}
		
	public static readonly object mapUploadLock = new object();
	public static void SendMapData(Connection connection)
	{
	
		Console.WriteLine("initiating sending map data to "+connection.Id+"...");
		WorldManager.Instance.SaveCurrentMapTo("temp.mapdata");//we dont actually read the file but we call this so the currentMap updates
		var packet = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.MapDataInitiate);
		packet.AddString(WorldManager.Instance.CurrentMap.Name);
		packet.AddString(WorldManager.Instance.CurrentMap.Author);
		packet.AddInt(WorldManager.Instance.CurrentMap.unitCount);
		server.Send(packet,connection);
		
		Task.Run(() => {
			while (!ClientsReadyForMap.Contains(connection.Id))
			{
				Thread.Sleep(100);
			}

			lock (mapUploadLock)
			{
				try
				{
					Console.WriteLine("Actually sending map data to " + connection.Id + "...");

					for (int x = 0; x < 100; x++)
					{
						for (int y = 0; y < 100; y++)
						{
							var tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
							if (tile.NorthEdge != null || tile.WestEdge != null || tile.Surface != null || tile.ObjectsAtLocation.Count != 0 || tile.UnitAtLocation != null)
							{
								//Console.WriteLine("Sending tile at " + x + "," + y);
								SendTileUpdate(tile, connection); //only send updates about tiles that have something on them
							}

							
						}
					}

					Console.WriteLine("finished sending map data to " + connection.Id);
					var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.MapDataFinish);
					server.Send(msg, connection);

				}catch(Exception e)
				{
					Console.WriteLine("Error sending map data to " + connection.Id);
					Console.WriteLine(e);
				}

		
			}

		});

	}

	public static void Kick(string reason,Connection connection)
	{
		Console.WriteLine("Kicking " + connection.Id + " for " + reason);
		if (connection.IsConnected)
		{
			var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.Notify);
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
		Task.Run(() => { 
			Thread.Sleep(15000);
			if (server.ClientCount == 0)
			{
				Environment.Exit(0);
			}});
		
	}



	public static void NotifyAll(string message)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.Notify);
		msg.AddString(message);
		server.SendToAll(msg);
	}

	public static void SendChatMessage(string text)
	{
		text = "[Yellow]" + text + "[-]";
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.Chat);
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
		data.CustomMapList = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Custom", "*.mapdata").ToList();
		data.SelectedMap = selectedMap;
		data.TurnTime = GameManager.PreGameData.TurnTime;

		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.PreGameData);
		msg.Add(data);
		server.SendToAll(msg);
		GameManager.PreGameData = data;
		Program.InformMasterServer();
	}


	public static void SendTileUpdate(WorldTile tile, Connection? connection = null)
	{

		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.TileUpdate);
		WorldTile.WorldTileData worldTileData = tile.GetData();
		msg.Add(worldTileData);
		

		if(connection == null)
			server.SendToAll(msg);
		else
			server.Send(msg,connection);
	}

	public static void Update()
	{
		lock (mapUploadLock)
		{
			server.Update();
		}
	}

	public static void SendGameData()
	{

		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.GameData);
		var state = GameManager.GetState();
		state.IsPlayerOne = true;
		msg.Add(state);
		if (GameManager.Player1 is not null  && GameManager.Player1.Connection is not null)
		{
			server.Send(msg, GameManager.Player1?.Connection);
		}

		var msg2 = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.GameData);
		state.IsPlayerOne = false;
		msg2.Add(state);
		if (GameManager.Player2 is not null  && GameManager.Player2.Connection is not null)
		{
			server.Send(msg2, GameManager.Player2?.Connection); //spectators dont care about isPlayerOne field
		}
		
		var msg3 = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.GameData);
		state.IsPlayerOne = null;
		msg3.Add(state);

		foreach (var spectator in GameManager.Spectators)
		{
			server.Send(msg3, spectator.Connection,false);
		}
		msg3.Release();
		Program.InformMasterServer();
			
	}

	public static void SendSequence(Queue<SequenceAction> actions)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.ReplaySequence);
		foreach (var a in actions)
		{
			msg.Add((int) a.SqcType);
			msg.AddSerializable(a);
		}
		server.SendToAll(msg);
	}

	public static void SendEndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.EndTurn);
		server.SendToAll(msg);
	}
}
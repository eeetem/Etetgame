﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldObjects;
using DefconNull.WorldObjects.Units.Actions;
using MD5Hash;
using Microsoft.Xna.Framework;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{
	private static Server server = null!;
	private static bool SinglePlayerFeatures = false;
	public static bool HasPendingMessages => (GameManager.Player1!= null && (!GameManager.Player1.HasDeliveredAllMessages || GameManager.Player1.HasSequencesToSend)) || (GameManager.Player2 != null && (!GameManager.Player2.HasDeliveredAllMessages || GameManager.Player2.HasSequencesToSend));

	public static void Start(ushort port, bool allowSP)
	{
		SinglePlayerFeatures = allowSP;
#if DEBUG
		SinglePlayerFeatures = true;
#endif
		RiptideLogger.Initialize(LogNetCode, LogNetCode,LogNetCode,LogNetCode, false);

		Message.MaxPayloadSize = 2048 * (int)Math.Pow(2, 5);
		//1. Start listen on a portw
		server = new Server(new TcpServer());
		server.TimeoutTime = 20000;
        
#if DEBUG
		server.TimeoutTime = ushort.MaxValue;
#endif

		server.ClientConnected += (a, b) => { Log.Message("NETWORKING",$" {b.Client.Id} connected (Clients: {server.ClientCount}), awaiting registration...."); };//todo kick without registration
		server.HandleConnection += HandleConnection;
		server.ClientDisconnected += ClientDisconnected;
		server.MessageReceived += (msg, args) =>
		{
			Log.Message("NETWORKING",$"Message received from {args.FromConnection} with ID {args.MessageId}");
		};


		if (!SinglePlayerFeatures)
		{ 
			SelectMap(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Ground Zero.mapdata");
		}
		server.Start(port, 10);
			
		Log.Message("NETWORKING","Started server at port" + port);



	}

	private static void SelectMap(string path)
	{

		WorldManager.Instance.LoadMap(path);
		SendMapData(GameManager.Player1.Connection);
		if (GameManager.Player2 != null)
			if (GameManager.Player2.Connection != null)
				SendMapData(GameManager.Player2.Connection);

	}

	private static void HandleConnection(Connection connection, Message connectmessage)
	{
		connection.MaxSendAttempts = 100;
		connection.MaxAvgSendAttempts = 10;
		connection.AvgSendAttemptsResilience = 25;
#if DEBUG
		connection.CanQualityDisconnect = false;
#endif
		string name = connectmessage.GetString();
		Log.Message("NETWORKING","Begining Client Register: "+name);
		if(name.Contains('.')||name.Contains(';')||name.Contains(':')||name.Contains(',')||name.Contains('[')||name.Contains(']')||name=="AI"||name=="Practice Opponent")
		{
			Kick("Invalid name",connection);	
			return;
		}
		

		if (GameManager.Player1 == null)
		{
			GameManager.Player1 = new GameManager.ClientInstance(name,connection);
			SendChatMessage(name+" joined as Player 1");
		}
		else if (GameManager.Player1.Name == name)
		{
			if (GameManager.Player1.Connection != null && GameManager.Player1.Connection.IsConnected)
			{
				Kick("Player with same name is already in the game",connection);	
				return;
			}

			GameManager.Player1.Reconnect(connection);//reconnection
			SendChatMessage(name+" reconnected as Player 1");
				
		}
		else if (GameManager.Player2 == null)
		{
			
			GameManager.Player2 = new GameManager.ClientInstance(name,connection);
			SendChatMessage(name+" joined as Player 2");
			

		}
		else if (GameManager.Player2.Name == name)
		{
			if (GameManager.Player2.Connection != null && GameManager.Player2.Connection.IsConnected)
			{
				Kick("Player with same name is already in the game",connection);	
				return;
			}
			GameManager.Player2.Reconnect(connection);;//reconnection
			SendChatMessage(name+" reconnected as Player 2");
		}
		else
		{
			GameManager.Spectators.Add(new GameManager.ClientInstance(name,connection));
			SendChatMessage(name+" joined the spectators");
		}

		

		Log.Message("NETWORKING","Client Register Done");

		server.Accept(connection);

		
		
		
		SendGameData();
		SendPreGameInfo(); 
		SendMapData(connection);
        
	}
		
	public static readonly object UpdateLock = new object();
	private static void SendMapData(ushort senderID)
	{
		Connection c;
		if(server.TryGetClient(senderID, out c))
			SendMapData(c);
	}

	private static Task? currentMapDataSendTask;
	public static void SendMapData(Connection connection)
	{
	
		Log.Message("NETWORKING","initiating sending map data to "+connection.Id+"...");
		WorldManager.Instance.SaveCurrentMapTo("temp.mapdata");//we dont actually read the file but we call this so the currentMap updates
		var packet = Message.Create(MessageSendMode.Reliable, NetworkMessageID.MapDataInitiate);
		packet.AddString(GetMapHashForConnection(connection));
		packet.AddString(WorldManager.Instance.CurrentMap.Name);
		packet.AddString(WorldManager.Instance.CurrentMap.Author);
		packet.AddInt(WorldManager.Instance.CurrentMap.unitCount);
		server.Send(packet,connection);
		lock (UpdateLock)
		{
			Thread.Sleep(100);//yeah this is bad
			Log.Message("NETWORKING","Actually sending map data to " + connection.Id + "...");
			if(SequenceManager.SequenceRunning) return;//we're probably arleady sending it and if not the client will do another request soon
			List<SequenceAction> act = new List<SequenceAction>();
			int sendTiles = 0;
			for (int x = 0; x < 100; x++)
			{	
				for (int y = 0; y < 100; y++)
				{
					
					WorldTile tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
					if (tile.NorthEdge != null || tile.WestEdge != null || tile.Surface != null || tile.ObjectsAtLocation.Count != 0 || tile.UnitAtLocation != null)
					{
						act.Add(TileUpdate.Make(new Vector2Int(x, y),true,false));
						sendTiles++;

					}
				}
			}

			Log.Message("NETWORKING","finished sending map data to " + connection.Id);
                    
			var player = GameManager.GetPlayer(connection);
			
			SendAllSeenUnitPositions();
			if (player == null)
			{
				SendSequenceMessageToConnection(act,connection);
			}
			else
			{
				bool player1 = player == GameManager.Player1;
				act.ForEach(x=>x.FilterForPlayer(player1));
				SendSequenceMessageToConnection(act,connection);
				
			}
			
			SendAllSeenUnitPositions();

			
		}
	
	}

	private static string GetMapHashForConnection(Connection connection)
	{
		var p = GameManager.GetPlayer(connection);
		
		string hash = "";
		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				if (p != null && p.WorldState.TryGetValue(new Vector2Int(x, y), out var tile))
				{
					hash+=tile.GetHash();
				}
				else
				{
					hash+=WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y)).GetData().GetHash();
				}
			} 
		}
		var md5 = hash.GetMD5();
		return md5;
	}

	public static void Kick(string reason, ushort id)
	{
		Connection c;
		server.TryGetClient(id, out c);
		Kick(reason,c);
	}

	public static void Kick(string reason,Connection connection)
	{
		Log.Message("NETWORKING","Kicking " + connection.Id + " for " + reason);
	
		var msg = Message.Create();
		msg.AddString(reason);
		server.Reject(connection,msg);
		server.DisconnectClient(connection,msg);
	
		if (GameManager.Player1?.Connection == connection)
		{
			GameManager.Player1.Disconnect();
		}else if (GameManager.Player2?.Connection == connection)
		{
			GameManager.Player2.Disconnect();
		}
	}
		
	private static void ClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
	{
		Log.Message("NETWORKING",$"Connection lost. Reason {e.Reason} {server.ClientCount}");
		string name;
		if (e.Client == GameManager.Player1?.Connection)
		{
			name = GameManager.Player1.Name;
			GameManager.Player1.Disconnect();
			SendChatMessage(name+" left the game");

		}else if (e.Client == GameManager.Player2?.Connection)
		{
			name = GameManager.Player2.Name;
			GameManager.Player2.Disconnect();
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
			Thread.Sleep(30000);
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
		

		data.TurnTime = GameManager.PreGameData.TurnTime;

		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.PreGameData);
		msg.Add(data);
		server.SendToAll(msg);
		GameManager.PreGameData = data;
		Program.InformMasterServer();
	}



	private static readonly ConcurrentQueue<ValueTuple<Message,Connection>> MessageQueue = new ConcurrentQueue<ValueTuple<Message, Connection>>();
	public static void Update()
	{
		while (MessageQueue.TryDequeue(out var msg))
		{
			server.Send(msg.Item1,msg.Item2);
		}
		lock (UpdateLock)
		{
			server.Update();
		}

		SendSequenceIfShould(false);//second playerirst because of ai and practice mode
		SendSequenceIfShould(true);
		
		if (!SequenceManager.SequenceRunning &!HasPendingMessages)
		{
			if (GameManager.ShouldRecalculateUnitPositions)
			{
				GameManager.UpdatePlayerSideUnitPositions();//make sure up to date info
			}
			if (GameManager.ShouldUpdateUnitPositions)
			{
				SendAllSeenUnitPositions();
			}
		
		}


		lock (UpdateLock)
		{
			server.Update();
		}
	}

	private static void SendSequenceIfShould(bool player1)
	{
		var p = GameManager.GetPlayer(player1);
		if(p== null) return;
		if (!p.IsConnected) return;
		if(!p.HasDeliveredAllMessages) return;
		if(!p.HasSequencesToSend || !p.HasDeliveredAllMessages || !p.ReadyForNextSequence)
		{
			return;
		}

		SequencePacket packet = p.GetNextSequence();
		
		Log.Message("NETWORKING","Sending sequence to: "+player1+" with ID: "+packet.ID);
		if (player1)
		{
			sequencesToExecute[packet.ID] = new Tuple<bool, bool, List<SequenceAction>>(true, sequencesToExecute[packet.ID].Item2, sequencesToExecute[packet.ID].Item3);
		}
		else
		{
			sequencesToExecute[packet.ID] = new Tuple<bool, bool, List<SequenceAction>>(sequencesToExecute[packet.ID].Item1, true, sequencesToExecute[packet.ID].Item3);
		}

		if (!p.IsConnected)//just discard all the shit since we have no connection and they will re-recive everything anyways when connecting
		{
			packet.Actions.ForEach(x=>x.Return());
			return;
		}

		List<SequenceAction> actsToDo = new List<SequenceAction>();
		foreach (var act in packet.Actions.ToList())
		{
			act.FilterForPlayer(player1);
			if (!act.ShouldSendToPlayerServerCheck(player1))
			{
				packet.Actions.Remove(act);
				act.Return();
				continue;
			}
			
			var infoacts = act.SendPrerequesteInfoToPlayer(player1);
			infoacts.ForEach(x=>x.FilterForPlayer(player1));
			actsToDo.AddRange(infoacts);
			actsToDo.Add(act);
		}
		
			
		SendSequenceMessageToPlayer(actsToDo,player1,true);


		//execute the sequence serverside if all players have it
		if((sequencesToExecute[packet.ID].Item1 || !GameManager.Player1!.IsConnected) && (sequencesToExecute[packet.ID].Item2|| !GameManager.Player2!.IsConnected) )
		{
			Log.Message("NETWORKING", "all players have recived sequence: "+packet.ID);


			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.ReplaySequence);
			msg.Add((ushort) ReplaySequenceTarget.All);
			msg.Add(sequencesToExecute[packet.ID].Item3.Count);
			foreach (var sqc in sequencesToExecute[packet.ID].Item3)
			{
				msg.Add((int) sqc.GetSequenceType());
				msg.AddSerializable(sqc);
			}
			foreach (var spec in GameManager.Spectators)
			{
				if (spec.Connection != null && spec.Connection.IsConnected)
				{
					server.Send(msg, spec.Connection,false);
				}
			}

			if (GameManager.Player2 != null && GameManager.Player2.IsPracticeOpponent)
			{
				server.Send(msg, GameManager.Player1!.Connection,false);
			}
			msg.Release();

			SequenceManager.AddSequence(sequencesToExecute[packet.ID].Item3);
			sequencesToExecute.Remove(packet.ID);
		}

	}

	private static void SendSequenceMessageToPlayer(List<SequenceAction> actsToSend, bool player1, bool waitForPlayerReady = true)
	{
		Log.Message("NETWORKING","Preping sequence for: "+player1);
		var p = GameManager.GetPlayer(player1);
		
		if (actsToSend.Count>0)
		{
			Log.Message("NETWORKING", "Sending sequence to player: " + player1);

			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.ReplaySequence);
			if (player1)
			{
				msg.Add((ushort) ReplaySequenceTarget.Player1);
			}
			else
			{
				msg.Add((ushort) ReplaySequenceTarget.Player2);
			}

			msg.Add(actsToSend.Count);
			foreach (var a in actsToSend)
			{
				if (!a.Active) throw new Exception("sending inactive sequence");
				msg.Add((int) a.GetSequenceType());
				msg.AddSerializable(a);
				a.Return();
			}
			
			if(waitForPlayerReady) p!.IsReadyForNextSequence = false;
			server.Send(msg, p!.Connection,false);
			foreach (var spec in GameManager.Spectators)
			{
				if (spec.Connection != null && spec.Connection.IsConnected)
				{
					server.Send(msg, spec.Connection,false);
				}
			}
			msg.Release();
		}

	}

	private static void SendSequenceMessageToConnection(List<SequenceAction> actions, Connection c)
	{
		var originalList = new List<SequenceAction>(actions);
		foreach (var a in originalList)
		{
			if(actions.Contains(a)) continue;
			a.Return();//release actions that are not being sent
		}
		int chunkNumber = 25;
		if (actions.Count() > chunkNumber)
		{
			SendSequenceMessageToConnection(actions.GetRange(0, chunkNumber),c);
			actions.RemoveRange(0, chunkNumber);
			SendSequenceMessageToConnection(actions,c);
			return;
		}
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.ReplaySequence);
		msg.Add((ushort)ReplaySequenceTarget.All);

		msg.Add(actions.Count);
		foreach (var a in actions)
		{
			if (!a.Active) throw new Exception("sending inactive sequence");
			msg.Add((int) a.GetSequenceType());
			msg.AddSerializable(a);
			a.Return();
		}
		server.Send(msg, c);
	}




	public static void SendGameData()
	{
		Log.Message("NETWORKING","sending game data");
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.GameData);
		var state = GameManager.GetState();
		state.IsPlayerOne = true;
		if(GameManager.Player2 != null && GameManager.Player2.IsPracticeOpponent) state.IsPlayerOne = null;
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
		
			
	}

	public static void AddSequenceToSendQueue(SequenceAction action)
	{
		AddSequenceToSendQueue(new List<SequenceAction>(){action});
	}

	public static void AddSequenceToSendQueue(IEnumerable<SequenceAction> actions)
	{
		AddSequenceToSendQueue(actions.ToList());
	}

	static Dictionary<int,Tuple<bool,bool,List<SequenceAction>>> sequencesToExecute = new Dictionary<int, Tuple<bool, bool, List<SequenceAction>>>();
	public struct SequencePacket
	{
		public int ID;
		public List<SequenceAction> Actions;

		public SequencePacket(int ID, List<SequenceAction> actions)
		{
			this.ID = ID;
			Actions = actions;

		}
	}

	private static int sequencePacketID = 0;
	private static void AddSequenceToSendQueue(List<SequenceAction> actions, bool? player1 = null)
	{
		var originalList = new List<SequenceAction>(actions);
		foreach (var a in originalList)
		{
			if(actions.Contains(a)) continue;
			a.Return();//release actions that are not being sent
		}
		int chunkNumber = 25;
		if (actions.Count() > chunkNumber)
		{
			AddSequenceToSendQueue(actions.GetRange(0, chunkNumber));
			actions.RemoveRange(0, chunkNumber);
			AddSequenceToSendQueue(actions);
			return;
		}
		

		var playerList = new List<bool>();
		if(player1 == null)
		{
			playerList.Add(true);
			playerList.Add(false);
		}
		else
		{
			playerList.Add(player1.Value);
		}
		lock (UpdateLock)
		{
			

			sequencePacketID++;
			Log.Message("NETWORKING","sequence added to queues with id for sending: "+sequencePacketID);
			var t = new Tuple<bool, bool, List<SequenceAction>>(true, true, actions);
			foreach (var pid in playerList)
			{
				var p = GameManager.GetPlayer(pid);
				
				if (p != null)
				{
					var tempActList = new List<SequenceAction>(actions.Count);
					actions.ForEach(x => tempActList.Add(x.Clone()));
					var packet = new SequencePacket(sequencePacketID, tempActList);
					p.AddToSequenceQueue(packet);
					if (pid)
					{
						t= new Tuple<bool, bool, List<SequenceAction>>(false, t.Item2, t.Item3);
					}
					else
					{
						t= new Tuple<bool, bool, List<SequenceAction>>(t.Item1, false, t.Item3);
					}
					
				}
				
			}
			sequencesToExecute.Add(sequencePacketID, t);

			
		}

	}



	public static void SendEndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.EndTurn);
		server.SendToAll(msg);
	}
    
	public static void SendAllSeenUnitPositions()
	{
		Log.Message("UNITS","sending unit position updates");

		var act = UnitUpdate.Make();
		AddSequenceToSendQueue(act);
			
		GameManager.ShouldUpdateUnitPositions = false;

	}


	public static void CaptureNotify(Vector2Int capPoint, int currentScore)
	{
		Message msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.CaptureNotif);
		msg.AddSerializable(capPoint);
		msg.Add(currentScore);
		server.SendToAll(msg);
	}
}
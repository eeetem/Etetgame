using System.Collections.Concurrent;
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
	private static string selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Ground Zero.mapdata";
	private static bool SinglePlayerFeatures = false;
	public static bool HasPendingMessages => (GameManager.Player1!= null && (!GameManager.Player1.HasDeliveredAllMessages || GameManager.Player1.SequenceQueue.Count >0)) || (GameManager.Player2 != null && (!GameManager.Player2.HasDeliveredAllMessages || GameManager.Player2.SequenceQueue.Count >0));

	public static void Start(ushort port, bool allowSP)
	{
		SinglePlayerFeatures = allowSP;
		RiptideLogger.Initialize(LogNetCode, LogNetCode,LogNetCode,LogNetCode, false);
	
		Message.MaxPayloadSize = 2048 * 2 * 2*2 *2 ;
		//1. Start listen on a portw
		server = new Server(new TcpServer());
		server.TimeoutTime = 10000;
        
#if DEBUG
		server.TimeoutTime = ushort.MaxValue;
#endif

		server.ClientConnected += (a, b) => { Log.Message("NETWORKING",$" {b.Client.Id} connected (Clients: {server.ClientCount}), awaiting registration...."); };//todo kick without registration
		server.HandleConnection += HandleConnection;
		server.ClientDisconnected += ClientDisconnected;
        
        

		selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Ground Zero.mapdata";
		//selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/testmap.mapdata";
		WorldManager.Instance.LoadMap(selectedMap);
		server.Start(port, 10);
			
		Log.Message("NETWORKING","Started server at port" + port);
		
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
			GameManager.Player1 = new ClientInstance(name,connection);
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
			
			GameManager.Player2 = new ClientInstance(name,connection);
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
			GameManager.Spectators.Add(new ClientInstance(name,connection));
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
		server.TryGetClient(senderID, out c);
		SendMapData(c);
	}
    
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
		
		Task.Run(() =>
		{
			while (!ClientsReadyForMap.Contains(connection.Id) || SequenceManager.SequenceRunning)
			{
				Thread.Sleep(100);
			}

			lock (UpdateLock)
			{
				try
				{
					Log.Message("NETWORKING","Actually sending map data to " + connection.Id + "...");

					int sendTiles = 0;
					for (int x = 0; x < 100; x++)
					{
						for (int y = 0; y < 100; y++)
						{
							WorldTile tile = WorldManager.Instance.GetTileAtGrid(new Vector2Int(x, y));
							if (tile.NorthEdge != null || tile.WestEdge != null || tile.Surface != null || tile.ObjectsAtLocation.Count != 0 || tile.UnitAtLocation != null)
							{
								ForceSendTileUpdate(tile, connection); //only send updates about tiles that have something on them
								sendTiles++;
								if (sendTiles >30)
								{
									Thread.Sleep(10);
									sendTiles = 0;
								}
							}
						}
					}

					Log.Message("NETWORKING","finished sending map data to " + connection.Id);
                    
	

				}catch(Exception e)
				{
					Log.Message("NETWORKING","Error sending map data to " + connection.Id);
					Log.Message("NETWORKING",e.ToString());
				}
                
				SendUnitUpdates();
			}
            
		});

	}

	private static string GetMapHashForConnection(Connection connection)
	{
		bool player1 = GameManager.Player1?.Connection == connection;
		if(! player1 && GameManager.Player2?.Connection != connection) return WorldManager.Instance.GetMapHash();
	
		string hash = "";
		for (int x = 0; x < 100; x++)
		{
			for (int y = 0; y < 100; y++)
			{
				if (tileUpdateLog.TryGetValue(new Vector2Int(x, y), out var tile))
				{
					if (player1)
					{
						hash+=tile.Item1.GetHash();
					}
					else
					{
						hash+=tile.Item2.GetHash();
					}
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
		if (connection.IsConnected)
		{
			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.Notify);
			msg.AddString(reason);
			server.Reject(connection,msg);
			server.DisconnectClient(connection,msg);
		}

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

		var msg = Message.Create(MessageSendMode.Unreliable, NetworkMessageID.TileUpdate);
		if (GameManager.Player1?.Connection == con && tileUpdateLog.ContainsKey(tile.Position))
		{
			msg.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
			msg.Add(tileUpdateLog[tile.Position].Item1);
			server.Send(msg,con);
			return;
		}
		if (GameManager.Player2?.Connection == con && tileUpdateLog.ContainsKey(tile.Position))
		{
			msg.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
			msg.Add(tileUpdateLog[tile.Position].Item2!);
			server.Send(msg,con);
			return;
		}
		WorldTile.WorldTileData worldTileData = tile.GetData();
		msg.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		msg.Add(worldTileData);
		server.Send(msg,con);
	}

	static readonly Dictionary<Vector2Int, ValueTuple<WorldTile.WorldTileData, WorldTile.WorldTileData>>
		tileUpdateLog = new Dictionary<Vector2Int, (WorldTile.WorldTileData, WorldTile.WorldTileData)>();
	public static void SendTileUpdate(WorldTile tile)
	{
		if(tile.Surface == null) return;//ignore empty tiles
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.TileUpdate);
		WorldTile.WorldTileData worldTileData = tile.GetData();
		//worldTileData.forceRebuild = false;
		msg.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		msg.Add(worldTileData);

		//add the entry if it's missing
		tileUpdateLog.TryAdd(tile.Position, (worldTileData,worldTileData));

        

        

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("updating tile at ");
		stringBuilder.Append(tile.Position);
		stringBuilder.Append("with hash: ");
		stringBuilder.Append(worldTileData);
		Log.Message("TILEUPDATES",stringBuilder.ToString());
        
        
		bool sent = false;
		if (tile.IsVisible(team1:true))
		{
			if (GameManager.Player1 is not null && GameManager.Player1.Connection is not null && !tileUpdateLog[tile.Position].Item1!.Equals(worldTileData))
			{
				Log.Message("TILEUPDATES","Sending tile to player 1: " + tile.Position);
				tileUpdateLog[tile.Position] = (worldTileData, tileUpdateLog[tile.Position].Item2);

				server.Send(msg, GameManager.Player1.Connection, false);
				sent = true;
			}
			else
			{
				Log.Message("TILEUPDATES","Not sending tile to player 1: " + tile.Position +" becuase sent hash is the same: "+tileUpdateLog[tile.Position].Item1);
			}
		}
		else
		{
			Log.Message("TILEUPDATES","Not sending tile to player 1: " + tile.Position +" because tile is not visible");
		} 

		if (tile.IsVisible(team1:false))
		{
			if (GameManager.Player2 is not null && GameManager.Player2.Connection is not null && !tileUpdateLog[tile.Position].Item2!.Equals(worldTileData))
			{
				Log.Message("TILEUPDATES","Sending tile to player 2: " + tile.Position);
				tileUpdateLog[tile.Position] = (tileUpdateLog[tile.Position].Item1, worldTileData);
				server.Send(msg, GameManager.Player2.Connection, false);
				sent = true;
			}
			else
			{
				Log.Message("TILEUPDATES","Not sending tile to player 2: " + tile.Position +" becuase sent hash is the same: "+tileUpdateLog[tile.Position].Item2);
			}
		}
		else
		{
			Log.Message("TILEUPDATES","Not sending tile to player 2: " + tile.Position +" because tile is not visible");
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

		if (!SequenceManager.SequenceRunning && GameManager.PlayerUnitPositionsDirty && (GameManager.Player1 == null || GameManager.Player1.Connection == null || GameManager.Player1.HasDeliveredAllMessages) && (GameManager.Player2 == null || GameManager.Player2.Connection == null || GameManager.Player2.HasDeliveredAllMessages))
		{
			SendUnitUpdates();
			
		}
        
		if(GameManager.Player1 != null && GameManager.Player1.SequenceQueue.Count > 0 && GameManager.Player1.HasDeliveredAllMessages)
		{
			List<SequenceAction> result;
			GameManager.Player1.SequenceQueue.TryPeek(out result);
			if (result != null && result.Count > 0)
			{
				var outcome = SendSequenceToPlayer(result,true);
				if (outcome)
				{
					GameManager.Player1.SequenceQueue.TryDequeue(out result);
				}
			}
			
		}
		if(GameManager.Player2 != null && GameManager.Player2.SequenceQueue.Count > 0 && GameManager.Player2.HasDeliveredAllMessages)
		{
			List<SequenceAction> result;
			GameManager.Player2.SequenceQueue.TryPeek(out result);
			if (result != null && result.Count > 0)
			{
				var outcome = SendSequenceToPlayer(result,false);
				if (outcome)
				{
					GameManager.Player2.SequenceQueue.TryDequeue(out result);
				}
			}
		}
	}

	public static void SendGameData()
	{
		Log.Message("NETWORKING","sending game data");
		SendUnitUpdates();
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
		SendSequence(new List<SequenceAction>(){action.Clone()},force);
	}

	public static void SendSequence(IEnumerable<SequenceAction> actions)
	{
		
		var clonedActions = new List<SequenceAction>(actions.Count());
		foreach (var a in actions)
		{
			clonedActions.Add(a.Clone());
		}
		SendSequence(clonedActions);
	}

	private static void SendSequence(List<SequenceAction> actions, bool force = false)
	{
		var originalList = new List<SequenceAction>(actions);
		actions.RemoveAll(x => !x.ShouldDo());
		foreach (var a in originalList)
		{
			if(actions.Contains(a)) continue;
			a.Return();//release actions that are not being sent
		}
		int chunkNumber = 25;
		//for (int i = 0; i < Math.Min(actions.Count,chunkNumber); i++)
		//{
		//	if (actions[i] is FaceUnit)
		//	{
		//		chunkNumber = i+1;
		//		break;
		//	}
		//}
		//send in chunks
		if (actions.Count() > chunkNumber)
		{
			SendSequence(actions.GetRange(0, chunkNumber));
			actions.RemoveRange(0, chunkNumber);
			SendSequence(actions);
			return;
		}
		Log.Message("NETWORKING","sequence submited for sending "+actions.Count);


		lock (UpdateLock)
		{
			if (force)
			{
				var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.ReplaySequence);
				msg.Add((ushort)ReplaySequenceTarget.All);
				msg.Add(actions.Count);
				foreach (var a in actions)
				{
					msg.Add((int) a.GetSequenceType());
					msg.AddSerializable(a);
				}
				server.SendToAll(msg);
                
			}
			else
			{
				var tempActList = new List<SequenceAction>(actions.Count);
				actions.ForEach(x => tempActList.Add(x.Clone()));
				tempActList.RemoveAll(x => !x.ShouldSendToPlayerServerCheck(true));
				EnqueueSendSequnceToPlayer(tempActList, true);
                    
				tempActList = new List<SequenceAction>(actions.Count);
				actions.ForEach(x => tempActList.Add(x.Clone()));
				tempActList.RemoveAll(x => !x.ShouldSendToPlayerServerCheck(false));
				EnqueueSendSequnceToPlayer(new List<SequenceAction>(tempActList), false);
                   
			}

		}
		actions.ForEach(x=>x.Return());

	}
	public static void EnqueueSendSequnceToPlayer(List<SequenceAction> actions, bool player1)
	{
		if (actions.Count == 0) return;
		var p =GameManager.GetPlayer(player1);
		p.SequenceQueue.Enqueue(actions);
	}
	public static bool SendSequenceToPlayer(List<SequenceAction> actions, bool player1)
	{
		if (actions.Count == 0) return true;
        
		var p =GameManager.GetPlayer(player1);
 
		actions.ForEach(x=>x.FilterForPlayer(player1));
		if (!p.HasDeliveredAllMessages)
			return false;
        
		Log.Message("NETWORKING","Sending sequence to player: "+player1);
  
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
			msg.Add(actions.Count);
			foreach (var a in actions)
			{
				if (!a.Active) throw new Exception("sending inactive sequence");
				msg.Add((int) a.GetSequenceType());
				msg.AddSerializable(a);
				a.Return();
			}

			if (player1 && GameManager.Player1 != null && GameManager.Player1.Connection != null)
			{
				var id = server.Send(msg,GameManager.Player1.Connection,false);
				GameManager.Player1.RegisterMessageToBeDelivered(id);
			}
			else if (!player1 && GameManager.Player2 != null && GameManager.Player2.Connection != null)
			{
				var id = server.Send(msg,GameManager.Player2.Connection,false);
				GameManager.Player2.RegisterMessageToBeDelivered(id);
			}
			

			foreach (var spec in GameManager.Spectators)
			{
				server.Send(msg, spec.Connection,false);
			}
			msg.Release();
		}

		return true;
	}

	public static void SendEndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.EndTurn);
		server.SendToAll(msg);
	}
    
	public static void SendUnitUpdates()
	{
		Log.Message("UNITS","sending unit position updates");

		if (GameManager.Player1 != null && GameManager.Player1.Connection != null)
		{
			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.UnitUpdate);
			msg.Add(GameManager.Player1UnitPositions.Count);
			foreach (var p in GameManager.Player1UnitPositions)
			{
				msg.Add(p.Key);
				msg.Add(p.Value.Item1);
				msg.Add(p.Value.Item2);
			}
			var id = server.Send(msg, GameManager.Player1.Connection);
			GameManager.Player1.RegisterMessageToBeDelivered(id);
		}

		if (GameManager.Player2 != null && GameManager.Player2.Connection != null)
		{
			var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.UnitUpdate);
			msg.Add(GameManager.Player2UnitPositions.Count);
			foreach (var p in GameManager.Player2UnitPositions)
			{
				msg.Add(p.Key);
				msg.Add(p.Value.Item1);
				msg.Add(p.Value.Item2);
			}
			var id = server.Send(msg, GameManager.Player2.Connection);
			GameManager.Player2.RegisterMessageToBeDelivered(id);
		}
		GameManager.PlayerUnitPositionsDirty = false;

	}

//only for oposite team
	public static void DetectUnit(Unit unit, Vector2Int position)
	{
		if (!unit.IsPlayer1Team)
		{
			if (GameManager.Player1UnitPositions.ContainsKey(unit.WorldObject.ID))
			{
				if(GameManager.Player1UnitPositions[unit.WorldObject.ID].Item1 == position && GameManager.Player1UnitPositions[unit.WorldObject.ID].Item2.Equals(unit.WorldObject.GetData())) return;
               
				GameManager.Player1UnitPositions.Remove(unit.WorldObject.ID);
			}
			GameManager.Player1UnitPositions.Add(unit.WorldObject.ID,(position,unit.WorldObject.GetData()));
		}
		else
		{
			if (GameManager.Player2UnitPositions.ContainsKey(unit.WorldObject.ID))
			{
				if(GameManager.Player2UnitPositions[unit.WorldObject.ID].Item1 == position && GameManager.Player2UnitPositions[unit.WorldObject.ID].Item2.Equals(unit.WorldObject.GetData())) return;
				GameManager.Player2UnitPositions.Remove(unit.WorldObject.ID);
			}
			GameManager.Player2UnitPositions.Add(unit.WorldObject.ID,(position,unit.WorldObject.GetData()));
		}
		SendUnitUpdates();
        
		
	}


}
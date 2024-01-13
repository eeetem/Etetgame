﻿using System.Reflection;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
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
		RiptideLogger.Initialize(LogNetCode, LogNetCode,LogNetCode,LogNetCode, false);
	
		//1. Start listen on a portw
		server = new Server(new TcpServer());
		server.TimeoutTime = 10000;
		Message.MaxPayloadSize = 2048*2;
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
		string name = connectmessage.GetString();
		Log.Message("NETWORKING","Begining Client Register: "+name);
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

		

		Log.Message("NETWORKING","Client Register Done");
		server.Accept(connection);
		connection.MaxSendAttempts = 50;
		connection.MaxAvgSendAttempts = 5;
		connection.AvgSendAttemptsResilience = 10;
		
		
		
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
		packet.AddString(WorldManager.Instance.GetMapHash());
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
		Log.Message("NETWORKING","Kicking " + connection.Id + " for " + reason);
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
		Log.Message("NETWORKING",$"Connection lost. Reason {e.Reason} {server.ClientCount}");
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

		var msg = Message.Create(MessageSendMode.Unreliable, NetworkMessageID.TileUpdate);
		WorldTile.WorldTileData worldTileData = tile.GetData();
		msg.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		msg.Add(worldTileData);
		server.Send(msg,con);
	}

	static readonly Dictionary<Vector2Int,ValueTuple<string,string>> tileUpdateLog = new Dictionary<Vector2Int, (string, string)>();
	public static void SendTileUpdate(WorldTile tile)
	{
		if(tile.Surface == null) return;//ignore empty tiles
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.TileUpdate);
		WorldTile.WorldTileData worldTileData = tile.GetData();
		//worldTileData.forceRebuild = false;
		msg.Add(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		msg.Add(worldTileData);

		//add the entry if it's missing
		tileUpdateLog.TryAdd(tile.Position, (string.Empty,string.Empty));

		
		string tileHash = tile.GetHash();


		Log.Message("NETWORKING","updating tile at "+tile.Position+"with hash: "+tileHash);
		
	

		
		bool sent = false;
		if (tile.IsVisible(team1:true))
		{
			if (GameManager.Player1 is not null && GameManager.Player1.Connection is not null && tileUpdateLog[tile.Position].Item1 != tileHash)
			{
				Log.Message("NETWORKING","Sending tile to player 1: " + tile.Position);
				tileUpdateLog[tile.Position] = (tile.GetHash(), tileUpdateLog[tile.Position].Item2);

				server.Send(msg, GameManager.Player1.Connection, false);
				sent = true;
			}
			else
			{
				Log.Message("NETWORKING","Not sending tile to player 1: " + tile.Position +" becuase sent hash is the same: "+tileUpdateLog[tile.Position].Item1);
			}
		}
		else
		{
			Log.Message("NETWORKING","Not sending tile to player 1: " + tile.Position +" because tile is not visible");
		} 

		if (tile.IsVisible(team1:false))
		{
			if (GameManager.Player2 is not null && GameManager.Player2.Connection is not null && tileUpdateLog[tile.Position].Item2 != tileHash)
			{
				Log.Message("NETWORKING","Sending tile to player 2: " + tile.Position);
				tileUpdateLog[tile.Position] = (tileUpdateLog[tile.Position].Item1, tile.GetHash());
				server.Send(msg, GameManager.Player2.Connection, false);
				sent = true;
			}
			else
			{
				Log.Message("NETWORKING","Not sending tile to player 2: " + tile.Position +" becuase sent hash is the same: "+tileUpdateLog[tile.Position].Item2);
			}
		}
		else
		{
			Log.Message("NETWORKING","Not sending tile to player 2: " + tile.Position +" because tile is not visible");
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
		Log.Message("NETWORKING","sending game data");
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

				SendSequenceToPlayer(new List<SequenceAction>(actions), true);
				SendSequenceToPlayer(new List<SequenceAction>(actions), false);
			}

		}
		
		
		actions.ForEach(x=>x.ReleaseIfShould());
	}
	

	public static void SendSequenceToPlayer(List<SequenceAction> actions, bool player1)
	{
		actions.RemoveAll(x => !x.ShouldSendToPlayerServerCheck(player1));
		
		List<SequenceAction> filteredActions = new List<SequenceAction>();
		actions.ForEach(x=>filteredActions.Add(x.FilterForPlayer(player1)));
		
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
			msg.Add(filteredActions.Count);
			foreach (var a in filteredActions)
			{
				msg.Add((int) a.GetSequenceType());
				msg.AddSerializable(a);
			}

			if (player1 && GameManager.Player1 != null && GameManager.Player1.Connection != null)
			{
				server.Send(msg,GameManager.Player1.Connection,false);
			}
			else if (!player1 && GameManager.Player2 != null && GameManager.Player2.Connection != null)
			{
				server.Send(msg,GameManager.Player2.Connection,false);
			}
			

			foreach (var spec in GameManager.Spectators)
			{
				server.Send(msg, spec.Connection,false);
			}
			msg.Release();
		}
		filteredActions.ForEach(x=>x.ReleaseIfShould());
	}

	public static void SendEndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.EndTurn);
		server.SendToAll(msg);
	}

	static readonly Dictionary<int,(Unit.UnitData,Unit.UnitData)> UnitUpdateLog = new Dictionary<int, (Unit.UnitData, Unit.UnitData)>();

	
	public static void SendUnitUpdate(Unit unit)
	{


		var tile = (WorldTile)unit.WorldObject.TileLocation!;
		//add the entry if it's missing
		if (UnitUpdateLog.ContainsKey(unit.WorldObject.ID) == false)
		{
			var pseudoMsg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.UnitUpdate);
			pseudoMsg.AddLong(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
			var pseudoData = new WorldObject.WorldObjectData("emptyUnit");
			pseudoData.ID = unit.WorldObject.ID;
			pseudoData.UnitData = new Unit.UnitData(unit.IsPlayer1Team);
	
			pseudoMsg.Add(new UnitUpdate(pseudoData,null));
			server.SendToAll(pseudoMsg);
			UnitUpdateLog.TryAdd(unit.WorldObject.ID, (new Unit.UnitData(),new Unit.UnitData()));
		}

		


		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.UnitUpdate);
		msg.AddLong(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
		msg.Add(new UnitUpdate(unit.WorldObject.GetData(),unit.WorldObject.TileLocation.Position));
		
		var data = unit.GetData();
		bool sent = false;
		if (unit.IsPlayer1Team || tile.IsVisible(team1:true))
		{
			if (GameManager.Player1 is not null && GameManager.Player1.Connection is not null && !UnitUpdateLog[unit.WorldObject.ID].Item1.Equals(data))
			{
				Log.Message("NETWORKING","Sending unit to player 1: " + tile.Position);
				UnitUpdateLog[unit.WorldObject.ID] = (data, UnitUpdateLog[unit.WorldObject.ID].Item2);

				server.Send(msg, GameManager.Player1.Connection, false);
				sent = true;
			}
			else
			{
				Log.Message("NETWORKING","Not sending unit to player 1: " + tile.Position +" becuase sent hash is the same: "+UnitUpdateLog[unit.WorldObject.ID].Item1);
			}
		}
		else
		{
			Log.Message("NETWORKING","Not sending unit to player 1: " + tile.Position +" because unit is not visible");
		} 

		if (!unit.IsPlayer1Team || tile.IsVisible(team1:false))
		{
			if (GameManager.Player2 is not null && GameManager.Player2.Connection is not null && !UnitUpdateLog[unit.WorldObject.ID].Item2.Equals(data))
			{
				Log.Message("NETWORKING","Sending unit to player 2: " + tile.Position);
				UnitUpdateLog[unit.WorldObject.ID] = (UnitUpdateLog[unit.WorldObject.ID].Item1, data);
				server.Send(msg, GameManager.Player2.Connection, false);
				sent = true;
			}
			else
			{
				Log.Message("NETWORKING","Not sending unit to player 2: " + tile.Position +" becuase sent hash is the same: "+UnitUpdateLog[unit.WorldObject.ID].Item2);
			}
		}
		else
		{
			Log.Message("NETWORKING","Not sending unit to player 2: " + tile.Position +" because unit is not visible");
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
}
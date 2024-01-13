using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Rendering;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
using Myra.Graphics2D.UI;
using Riptide;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{
	private static Dialog? mapLoadMsg;
	[MessageHandler((ushort)NetworkMessageID.MapDataInitiate)]
	private static void StartMapRecive(Message message)
	{
		UI.Desktop.Widgets.Remove(mapLoadMsg);
		mapLoadMsg  = UI.OptionMessage("Loading Map...", "Please Wait","",null,"",null);
		var oldname = WorldManager.Instance.CurrentMap.Name;
		string hash = message.GetString();
		WorldManager.Instance.CurrentMap = new WorldManager.MapData();
		WorldManager.Instance.CurrentMap.Name = message.GetString();
		WorldManager.Instance.CurrentMap.Author = message.GetString();
		WorldManager.Instance.CurrentMap.unitCount = message.GetInt();
		if (WorldManager.Instance.CurrentMap.Name != oldname)
		{
			WorldManager.Instance.WipeGrid();
		}

		
		var msg = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageID.MapDataInitiateConfirm);
		client?.Send(msg);
		
		Task t = new Task(delegate
		{
			do
			{
				Thread.Sleep(1000);
			} while (SequenceManager.SequenceRunning || RecievedTiles.Count>0); //still loading the map

			UI.Desktop.Widgets.Remove(mapLoadMsg);
			if (WorldManager.Instance.GetMapHash() != hash)
			{
				Log.Message("NETWORKING","Map Hash Mismatch, resending");
				var msg = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageID.MapReaload);
				client?.Send(msg);
			
			}
			
			
		});
		t.Start();
	}
	[MessageHandler((ushort) NetworkMessageID.EndTurn)]
	private static void RecieveEndTrugn(Message message)
	{
		GameManager.SetEndTurn();
	}


	

	[MessageHandler((ushort)NetworkMessageID.GameData)]
	private static void ReciveGameUpdate(Message message)
	{
		GameManager.GameStateData data = message.GetSerializable<GameManager.GameStateData>();
		GameManager.SetData(data);
	}

	//cache tiles to be updated and load them all at once
	public static readonly Dictionary<Vector2Int,ValueTuple<long,WorldTile.WorldTileData>> RecievedTiles = new Dictionary<Vector2Int, ValueTuple<long,WorldTile.WorldTileData>>();

	[MessageHandler((ushort)NetworkMessageID.TileUpdate)]
	private static void ReciveTileUpdate(Message message)
	{
		long timestamp = message.GetLong();
		WorldTile.WorldTileData data = message.GetSerializable<WorldTile.WorldTileData>();


		Log.Message("NETWORKING","TILE Update recived: " + data);

		if (RecievedTiles.ContainsKey(data.position))
		{
			Log.Message("NETWORKING","tile already present");
			if (RecievedTiles[data.position].Item1 <= timestamp)
			{
				Log.Message("NETWORKING","update is newer, discarding old");
				RecievedTiles.Remove(data.position);
				RecievedTiles.Add(data.position, (timestamp,data));
			}
			else
			{
				Log.Message("NETWORKING","old update, discarding");
			}
		}
		else
		{
			Log.Message("NETWORKING","new tile, adding");
			RecievedTiles.Add(data.position, (timestamp,data));
		}
			
		
	
	}
	
	public static readonly Dictionary<int,ValueTuple<long,WorldObject.WorldObjectData>> RecivedUnits = new Dictionary<int, ValueTuple<long,WorldObject.WorldObjectData>>();

	[MessageHandler((ushort)NetworkMessageID.UnitUpdate)]
	private static void ReciveUnitUpdate(Message message)
	{
		long timestamp = message.GetLong();
		WorldObject.WorldObjectData data = message.GetSerializable<WorldObject.WorldObjectData>();
		int id = data.ID;



		Log.Message("NETWORKING","UNIT update recived: " + data);

		if (RecivedUnits.ContainsKey(id))
		{
			Log.Message("NETWORKING","tile already present");
			if (RecivedUnits[id].Item1 <= timestamp)
			{
				Log.Message("NETWORKING","update is newer, discarding old");
				RecivedUnits.Remove(id);
				RecivedUnits.Add(id, (timestamp,data));
			}
			else
			{
				Log.Message("NETWORKING","old update, discarding");
			}
		}
		else
		{
			Log.Message("NETWORKING","new unit, adding");
			RecivedUnits.Add(data.ID, (timestamp,data));
		}
			
		
	
	}
	
	[MessageHandler((ushort)NetworkMessageID.Notify)]
	private static void ReciveNotify(Message message)
	{
		string i = message.GetString();
		UI.ShowMessage("Server Notice",i);
	}
	
	
	[MessageHandler((ushort)NetworkMessageID.PreGameData)]
	private static void RecivePreGameData(Message message)
	{
		Log.Message("NETWORKING","LobbyData Recived");
		GameManager.PreGameData = message.GetSerializable<GameManager.PreGameDataStruct>();
	}
	
	[MessageHandler((ushort)NetworkMessageID.Chat)]
	private static void ReciveChat(Message message)
	{
		Chat.ReciveMessage(message.GetString());	
	}

	
	[MessageHandler((ushort)NetworkMessageID.ReplaySequence)]
	private static void RecieveReplaySequence(Message message)
	{
		Queue<SequenceAction> actions = new Queue<SequenceAction>();
		ReplaySequenceTarget t = (ReplaySequenceTarget)message.GetUShort();
		switch (t)
		{//CLIENT side filtering, only relevant topractice mode and spectator
			case ReplaySequenceTarget.Player1:
				if(!GameManager.IsPlayer1)return;
				break;
			case ReplaySequenceTarget.Player2:
				if(GameManager.IsPlayer1)return;
				break;
		}
		int lenght = message.GetInt();
		for (int i = 0; i < lenght; i++)
		{

			SequenceAction.SequenceType type = (SequenceAction.SequenceType) message.GetInt();
			SequenceAction sqc;
			sqc = SequenceAction.GetAction(type, message);
			
			actions.Enqueue(sqc);
		}
		SequenceManager.AddSequence(actions);
	
	}
}
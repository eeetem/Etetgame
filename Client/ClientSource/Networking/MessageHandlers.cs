﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Rendering;
using DefconNull.ReplaySequence;
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
	}
	[MessageHandler((ushort) NetworkMessageID.EndTurn)]
	private static void RecieveEndTrugn(Message message)
	{
		GameManager.SetEndTurn();
	}


	[MessageHandler((ushort) NetworkMessageID.MapDataFinish)]
	private static void FinishMapRecieve(Message message)
	{
		Task t = new Task(delegate
		{
			while (SequenceManager.SequenceRunning)//still loading the map
			{
				Thread.Sleep(100);
			}

			UI.Desktop.Widgets.Remove(mapLoadMsg);
			string hash = message.GetString();
			if (WorldManager.Instance.GetMapHash() != hash)
			{
				Console.WriteLine("Map Hash Mismatch, resending");
				var msg = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageID.MapReaload);
				client?.Send(msg);
			
			}
			
			
		});
		t.Start();

		
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


		Console.WriteLine("tileupdate recived: " + data.position);
		if (data.position == new Vector2Int(52,78))
		{
			Console.WriteLine("TARGET TILE UPDATE RECIVED: "+data);
		}
		if (RecievedTiles.ContainsKey(data.position))
		{
			Console.WriteLine("tile already present");
			if (RecievedTiles[data.position].Item1 < timestamp)
			{
				Console.WriteLine("update is newer, discarding old");
				RecievedTiles.Remove(data.position);
				RecievedTiles.Add(data.position, (timestamp,data));
			}
			else
			{
				Console.WriteLine("old update, discarding");
			}
		}
		else
		{
			Console.WriteLine("new tile, adding");
			RecievedTiles.Add(data.position, (timestamp,data));
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
		Console.WriteLine("LobbyData Recived");
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
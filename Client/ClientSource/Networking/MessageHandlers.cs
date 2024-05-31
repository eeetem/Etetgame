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
		WorldManager.Instance.Maploading = true;
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
				do
				{
					Thread.Sleep(1000);
				} while (SequenceManager.SequenceRunning); //still loading the map
				Thread.Sleep(1000);
			} while (SequenceManager.SequenceRunning);
			//holy shit this is atrocious
			
			
			
			UI.Desktop.Widgets.Remove(mapLoadMsg);
			WorldManager.Instance.Maploading = false;
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
		Log.Message("SEQUENCENETOWRKING","recived sequence for: "+t);
		switch (t)
		{//CLIENT side filtering, only relevant to practice mode and spectator
			case ReplaySequenceTarget.Player1:
				if(!GameManager.IsPlayer1)return;
				break;
			case ReplaySequenceTarget.Player2:
				if(GameManager.IsPlayer1)return;
				break;
		}
		int lenght = message.GetInt();
		Log.Message("SEQUENCENETOWRKING","sequence legnhts: "+lenght);
		for (int i = 0; i < lenght; i++)
		{
			SequenceAction.SequenceType type = (SequenceAction.SequenceType) message.GetInt();
			SequenceAction sqc;
			sqc = SequenceAction.GetAction(type, message);
			if(t == ReplaySequenceTarget.All && (type != SequenceAction.SequenceType.UnitUpdate && type != SequenceAction.SequenceType.TileUpdate)) continue;//unit update is the only message type we listen for for the oposite team while spectating
			actions.Enqueue(sqc);
			Log.Message("SEQUENCENETOWRKING","actions: "+sqc);
		}
		SequenceManager.AddSequence(actions);
	
	}
}
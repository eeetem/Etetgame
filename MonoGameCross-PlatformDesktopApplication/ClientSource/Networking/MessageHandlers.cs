using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MultiplayerXeno.ReplaySequence;
using Myra.Graphics2D.UI;
using Riptide;

namespace MultiplayerXeno;

public static partial class Networking
{
	private static Dialog? mapLoadMsg;
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.MapDataInitiate)]
	private static void StartMapRecive(Message message)
	{
		mapLoadMsg  = UI.OptionMessage("Loading Map...", "Please Wait","",null,"",null);
		WorldManager.Instance.CurrentMap = new WorldManager.MapData();
		WorldManager.Instance.CurrentMap.Name = message.GetString();
		WorldManager.Instance.CurrentMap.Author = message.GetString();
		WorldManager.Instance.CurrentMap.unitCount = message.GetInt();
		WorldManager.Instance.WipeGrid();
		var msg = Message.Create(MessageSendMode.Reliable, (ushort)NetMsgIds.NetworkMessageID.MapDataInitiateConfirm);
		client?.Send(msg);
	}
	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.EndTurn)]
	private static void RecieveEndTrugn(Message message)
	{
		GameManager.SetEndTurn();
	}


	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.MapDataFinish)]
	private static void FinishMapRecieve(Message message)
	{
		UI.Desktop.Widgets.Remove(mapLoadMsg);
	}

	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.GameData)]
	private static void ReciveGameUpdate(Message message)
	{
		GameManager.GameStateData data = message.GetSerializable<GameManager.GameStateData>();
		GameManager.SetData(data);
	}

	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.TileUpdate)]
	private static void ReciveTileUpdate(Message message)
	{

		WorldTile.WorldTileData data = message.GetSerializable<WorldTile.WorldTileData>();
		WorldManager.Instance.AddSequence(new UpdateTile(data));
		

	}
	
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.Notify)]
	private static void ReciveNotify(Message message)
	{
		string i = message.GetString();
		UI.ShowMessage("Server Notice",i);
	}
	
	
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.PreGameData)]
	private static void RecivePreGameData(Message message)
	{
		Console.WriteLine("LobbyData Recived");
		GameManager.PreGameData = message.GetSerializable<GameManager.PreGameDataStruct>();
	}
	
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.Chat)]
	private static void ReciveChat(Message message)
	{
		Chat.ReciveMessage(message.GetString());	
	}

	
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.ReplaySequence)]
	private static void RecieveReplaySequence(Message message)
	{
		Queue<SequenceAction> actions = new Queue<SequenceAction>();
		while (message.UnreadLength>0)
		{
			SequenceAction.SequenceType type = (SequenceAction.SequenceType) message.GetInt();
			SequenceAction sqc;
			int id = message.GetInt();
			switch (type)
			{
					
				case SequenceAction.SequenceType.Face:
					sqc = new ReplaySequence.Face(id, message);
					break;
				case SequenceAction.SequenceType.Move:
					sqc = new ReplaySequence.Move(id, message);
					break;
				case SequenceAction.SequenceType.Crouch:
					sqc = new ReplaySequence.Crouch(id, message);
					break;
				case SequenceAction.SequenceType.WorldEffect:
					sqc = new ReplaySequence.WorldChange(id, message);
					break;
				case SequenceAction.SequenceType.Action:
					sqc = new ReplaySequence.DoAction(id, message);
					break;
				case SequenceAction.SequenceType.SelectItem:
					sqc = new ReplaySequence.SelectItem(id, message);
					break;
				case SequenceAction.SequenceType.UseItem:
					sqc = new ReplaySequence.UseSelectedItem(id, message);
					break;
				case SequenceAction.SequenceType.Overwatch:
					sqc = new ReplaySequence.OverWatch(id, message);
					break;
				default:
					throw new Exception("Unknown Sequence Type Recived: "+type);
			
			}
			actions.Enqueue(sqc);
		}
		WorldManager.Instance.AddSequence(actions);
	
	}
}
using System;
using System.Collections.Generic;

using DefconNull.Rendering;
using DefconNull.ReplaySequence.ActorSequenceAction;
using DefconNull.SharedSource.Units.ReplaySequence;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence.ActorSequenceAction;
using Myra.Graphics2D.UI;
using Riptide;

namespace DefconNull.Networking;

public static partial class NetworkingManager
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
			if ((int) type >= 100) //over 100 is a non unit action
			{
				switch (type)
				{
					case SequenceAction.SequenceType.PlaySound:
						sqc = new PlaySound(message);
						break;
					case SequenceAction.SequenceType.PostProcessingEffect:
						sqc = new PostProcessingEffect(message);
						break;
					case SequenceAction.SequenceType.TakeDamage:
						sqc = new TakeDamage(message);
						break;
					case SequenceAction.SequenceType.MakeWorldObject:
						sqc = new MakeWorldObject(message);
						break;
					default:
						throw new Exception("Unknown Sequence Type Recived: " + type);

				}
			}
			else
			{
				int id = message.GetInt();
				switch (type)
				{
					
					case SequenceAction.SequenceType.Face:
						sqc = new FaceUnit(id, message);
						break;
					case SequenceAction.SequenceType.Move:
						sqc = new UnitMove(id, message);
						break;
					case SequenceAction.SequenceType.Crouch:
						sqc = new CrouchUnit(id, message);
						break;
					case SequenceAction.SequenceType.SelectItem:
						sqc = new UnitSelectItem(id, message);
						break;
					case SequenceAction.SequenceType.UseItem:
						sqc = new UseUpSelectedItem(id, message);
						break;
					case SequenceAction.SequenceType.Overwatch:
						sqc = new UnitOverWatch(id, message);
						break;
					case SequenceAction.SequenceType.ChangeUnitValues:
						sqc = new ChangeUnitValues(id, message);
						break;
					case SequenceAction.SequenceType.Suppress:
						sqc = new Suppress(id, message);
						break;
					case SequenceAction.SequenceType.UnitStatusEffect:
						sqc = new UnitStatusEffect(id, message);
						break;
					case SequenceAction.SequenceType.AbilityToggle:
						sqc = new UnitAbilitToggle(id, message);
						break;
					case SequenceAction.SequenceType.DelayedAbilityUse:
						sqc = new DelayedAbilityUse(id, message);
						break;
					default:
						throw new Exception("Unknown Unit Sequence Type Recived: "+type);
			
				}
			}

			
			actions.Enqueue(sqc);
		}
		WorldManager.Instance.AddSequence(actions);
	
	}
}
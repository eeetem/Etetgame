using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Riptide;
using Action = DefconNull.WorldObjects.Units.Actions.Action;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{

	[MessageHandler((ushort) NetworkMessageID.MapReaload)]
	private static void ResendMap(ushort senderID, Message message)
	{
		SendMapData(senderID);
	}
	
	[MessageHandler((ushort) NetworkMessageID.SequenceFinished)]
	private static void SequenceFinished(ushort senderID, Message message)
	{
		if(server.TryGetClient(senderID, out var c))
		{
			GameManager.SequenceFinished(c);
		}
		
	}


	
	[MessageHandler((ushort) NetworkMessageID.DoAI)]
	private static void AiFinish(ushort senderID, Message message)
	{
		GameManager.FinishTurnWithAI();
	}

	[MessageHandler((ushort)NetworkMessageID.Kick)]
	private static void KickRequest(ushort senderID, Message message)
	{
		if (senderID != GameManager.Player1?.Connection?.Id)return;

		if (GameManager.Player2 != null)
		{
			//dont kick host when kicking ai or practice bot
			if (GameManager.Player2?.Connection != null && GameManager.Player2.Connection != GameManager.Player1.Connection) Kick("Kicked by host", GameManager.Player2.Connection);
			GameManager.Player2 = null;
		}

		SendPreGameInfo();
	}
	[MessageHandler((ushort)NetworkMessageID.EndTurn)]
	private static void HandleEndTurn(ushort senderID,Message message)
	{
		GameManager.ClientInstance? currentPlayer;
		if (GameManager.IsPlayer1Turn)
		{
			
			currentPlayer = GameManager.Player1;
		}
		else
		{
			currentPlayer = GameManager.Player2;
		}

		if (currentPlayer?.Connection?.Id != senderID && !currentPlayer!.IsPracticeOpponent)
		{
			//out of turn action. perhaps desync or hax? kick perhaps
			return;
		}

		GameManager.SetEndTurn();
	}
		
	[MessageHandler((ushort)NetworkMessageID.StartGame)]
	private static void StartGameHandler(ushort senderID,Message message)
	{
		if(senderID != GameManager.Player1?.Connection?.Id) return;
		if( GameManager.GameState != GameState.Lobby) SendGameData();//incase the ui is desynced
		GameManager.StartSetup();
	}
	[MessageHandler((ushort)NetworkMessageID.StartTutorial)]
	private static void Tutorial(ushort senderID,Message message)
	{
		if(senderID != GameManager.Player1?.Connection?.Id) return;
		if(!SinglePlayerFeatures) return;
		if( GameManager.GameState != GameState.Lobby) SendGameData();//incase the ui is desynced
		GameManager.StartTutorial();
	}
	[MessageHandler((ushort)NetworkMessageID.SquadComp)]
	private static void ReciveSquadComp(ushort senderID,Message message)
	{
		
		if (message.GetBool())
		{
			if (GameManager.Player2 != null && !GameManager.Player2.IsPracticeOpponent)
			{
				Kick("This server is not running in practice mode", senderID);
				return;
			}

			
			GameManager.Player1!.SetSquadComp(message.GetSerializables<SquadMember>().ToList());

			GameManager.Player2?.SetSquadComp(message.GetSerializables<SquadMember>().ToList());
			GameManager.StartGame();
			return;
		}
		List<SquadMember> squadMembers = message.GetSerializables<SquadMember>().ToList();

		if (GameManager.Player1?.Connection?.Id == senderID)
		{
			GameManager.Player1.SetSquadComp(squadMembers);
		}else if (GameManager.Player2?.Connection?.Id == senderID)
		{
			GameManager.Player2.SetSquadComp(squadMembers);
		}

		if (GameManager.Player1!.SquadComp != null && (GameManager.Player2!.SquadComp != null || GameManager.Player2.IsAI))
		{
			GameManager.StartGame();
		}
	}
		
	[MessageHandler((ushort)NetworkMessageID.PreGameData)]
	private static void RecivePreGameUpdate(ushort senderID,Message message)
	{
		if(senderID != GameManager.Player1?.Connection?.Id  || GameManager.GameState != GameState.Lobby) return;
		var data = message.GetSerializable<GameManager.PreGameDataStruct>();

					
		GameManager.PreGameData.TurnTime = data.TurnTime;
		SendPreGameInfo();
		if (WorldManager.Instance.CurrentMap.Name != data.SelectedMap)
		{
			WorldManager.Instance.LoadMap(data.SelectedMap);

			SendMapData(GameManager.Player1.Connection);
			if (GameManager.Player2 != null)
				if (GameManager.Player2.Connection != null)
					SendMapData(GameManager.Player2.Connection);
		}
	}
	[MessageHandler((ushort)NetworkMessageID.Chat)]
	private static void ReciveChatMsg(ushort senderID,Message message)
	{
		string text = message.GetString();
		string name;
		if (GameManager.Player1?.Connection?.Id == senderID)
		{
			name = "[Red]"+GameManager.Player1.Name+"[-]";
		}
		else if (GameManager.Player2?.Connection?.Id == senderID)
		{
			name = "[Blue]"+GameManager.Player2.Name+"[-]";
		}
		else
		{
			return;
		}
		text = text.Replace("\n", "");
		text = text.Replace("[", "");
		text = text.Replace("]", "");

		text = $"{name}: {text}";
		SendChatMessage(text);
	}

	//[MessageHandler((ushort) NetworkMessageID.MapUpload)]
	private static void ReciveMapUpload(ushort senderID, Message message)
	{
		//var data = message.GetSerializable<WorldManager.MapData>();
		//File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Custom/" + data.Name + ".mapdata");
		//File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Custom/" + data.Name + ".mapdata", data.ToJSON());
		//SendPreGameInfo();
	}

	private static List<ushort> ClientsReadyForMap = new List<ushort>();
	[MessageHandler((ushort) NetworkMessageID.MapDataInitiateConfirm)]
	private static void MapRecivedConfirm(ushort senderID, Message message)
	{
		Log.Message("NETWORKING","Recived map confirm from: " + senderID);
		ClientsReadyForMap.Add(senderID);
	}

	[MessageHandler((ushort)NetworkMessageID.GameAction)]
	private static void ParseGameAction(ushort senderID, Message message)
	{

		Log.Message("NETWORKING","Recived Game Action from: " + senderID);
		if (!GameManager.Player2!.IsPracticeOpponent)
		{
			if (GameManager.Player1 != null && GameManager.Player1.Connection?.Id == senderID)
			{
				if (!GameManager.IsPlayer1Turn)
				{
					Log.Message("NETWORKING","Client sent an action out of turn");
					return;
				}
			}
			else if (GameManager.Player2 != null && GameManager.Player2.Connection?.Id == senderID)
			{
				if (GameManager.IsPlayer1Turn)
				{
					Log.Message("NETWORKING","Client sent an action out of turn");
					return;
				}
			}
		}

		if (!(GameManager.Player1 != null && GameManager.Player1.Connection!.Id == senderID) && !(GameManager.Player2 != null && GameManager.Player2.Connection!.Id == senderID))
		{
			Log.Message("NETWORKING","Spectator tried to control a unit");
			return;
		}
		


		Action.GameActionPacket packet = message.GetSerializable<Action.GameActionPacket>();

		if (WorldObjectManager.GetObject(packet.UnitId) == null)
		{
			Log.Message("NETWORKING","Recived packet for a non existant object: " + packet.UnitId);
			return;
		}

		Unit? controllable = WorldObjectManager.GetObject(packet.UnitId)?.UnitComponent;
		if(controllable == null)
		{
			Log.Message("NETWORKING","Recived packet for a non controllable object: " + packet.UnitId);
			return;
		}
		if(controllable.IsPlayer1Team != GameManager.IsPlayer1Turn)
		{
			Log.Message("NETWORKING","Client sent an action for worng teams Unit");
			return;
		}

		Log.Message("NETWORKING","DOING GAME ACTION: " + packet.Type + " " + packet.Args.Target + " " + packet.Args.TargetObj?.ID + " " + packet.Args.AbilityIndex);
		controllable.DoAction(packet.Type,packet.Args);


	}
	

	[MessageHandler((ushort) NetworkMessageID.AddAI)]
	private static void AddAI(ushort senderID, Message message)
	{
		if(!SinglePlayerFeatures) return;
		if (GameManager.Player2 != null) return;

		GameManager.Player2 = new GameManager.ClientInstance("AI", null);
		GameManager.Player2.IsAI = true;
		SendPreGameInfo();
	}

	[MessageHandler((ushort) NetworkMessageID.PracticeMode)]
	private static void PracticeMode(ushort senderID, Message message)
	{
		if(!SinglePlayerFeatures) return;
		if (GameManager.Player2 != null) return;
		GameManager.PracticeMode(GameManager.Player1!.Connection);

	}

}
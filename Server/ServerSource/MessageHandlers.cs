
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.Actions;
using Riptide;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{

	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.MapReaload)]
	private static void ResendMap(ushort senderID, Message message)
	{
		NetworkingManager.SendMapData(senderID);
	}

	
	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.DoAI)]
	private static void AiFinish(ushort senderID, Message message)
	{
		GameManager.FinishTurnWithAI();
	}

	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.Kick)]
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
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.EndTurn)]
	private static void HandleEndTurn(ushort senderID,Message message)
	{
		ClientInstance? currentPlayer;
		if (GameManager.IsPlayer1Turn)
		{
			
			currentPlayer = GameManager.Player1;
		}
		else
		{
			currentPlayer = GameManager.Player2;
		}

		if (currentPlayer?.Connection?.Id != senderID)
		{
			//out of turn action. perhaps desync or hax? kick perhaps
			return;
		}

		GameManager.SetEndTurn();
	}
		
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.StartGame)]
	private static void StartGameHandler(ushort senderID,Message message)
	{
		if(senderID != GameManager.Player1?.Connection?.Id || GameManager.GameState != GameState.Lobby) return;
		GameManager.StartSetup();
	}
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.SquadComp)]
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
		
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.PreGameData)]
	private static void RecivePreGameUpdate(ushort senderID,Message message)
	{
		if(senderID != GameManager.Player1?.Connection?.Id  || GameManager.GameState != GameState.Lobby) return;
		var data = message.GetSerializable<GameManager.PreGameDataStruct>();
		WorldManager.Instance.LoadMap(data.SelectedMap);
		GameManager.PreGameData.TurnTime = data.TurnTime;
		SendPreGameInfo();
	}
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.Chat)]
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

	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.MapUpload)]
	private static void ReciveMapUpload(ushort senderID, Message message)
	{
		//var data = message.GetSerializable<WorldManager.MapData>();
		//File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Custom/" + data.Name + ".mapdata");
		//File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "/Maps/Custom/" + data.Name + ".mapdata", data.ToJSON());
		//SendPreGameInfo();
	}

	private static List<ushort> ClientsReadyForMap = new List<ushort>();
	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.MapDataInitiateConfirm)]
	private static void MapRecivedConfirm(ushort senderID, Message message)
	{
		Console.WriteLine("Recived map confirm from: " + senderID);
		ClientsReadyForMap.Add(senderID);
	}

	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.GameAction)]
	private static void ParseGameAction(ushort senderID, Message message)
	{
		if (!GameManager.Player2!.IsPracticeOpponent)
		{
			if (GameManager.Player1 != null && GameManager.Player1.Connection!.Id == senderID)
			{
				if (!GameManager.IsPlayer1Turn)
				{
					Console.WriteLine("Client sent an action out of turn");
					return;
				}
			}
			else if (GameManager.Player2 != null && GameManager.Player2.Connection!.Id == senderID)
			{
				if (GameManager.IsPlayer1Turn)
				{
					Console.WriteLine("Client sent an action out of turn");
					return;
				}
			}
		}
		else
		{
			if (!(GameManager.Player1 != null && GameManager.Player1.Connection!.Id == senderID) && !(GameManager.Player2 != null && GameManager.Player2.Connection!.Id == senderID))
			{
				Console.WriteLine("Spectator tried to control a unit");
				return;
			}
		}


		Action.GameActionPacket packet = message.GetSerializable<Action.GameActionPacket>();

		if (WorldManager.Instance.GetObject(packet.UnitId) == null)
		{
			Console.WriteLine("Recived packet for a non existant object: " + packet.UnitId);
			return;
		}

		Unit? controllable = WorldManager.Instance.GetObject(packet.UnitId)?.UnitComponent;
		if(controllable == null)
		{
			Console.WriteLine("Recived packet for a non controllable object: " + packet.UnitId);
			return;
		}
		if(controllable.IsPlayer1Team != GameManager.IsPlayer1Turn)
		{
			Console.WriteLine("Client sent an action for worng teams Unit");
			return;
		}

	
		controllable.DoAction(packet.Type,packet.Args);
		

	}
	

	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.AddAI)]
	private static void AddAI(ushort senderID, Message message)
	{
		if(!SinglePlayerFeatures) return;
		if (GameManager.Player2 != null) return;
		Connection c;
		server.TryGetClient(senderID, out c);
		GameManager.Player2 = new ClientInstance("AI", c);
		GameManager.Player2.IsAI = true;
		SendPreGameInfo();
	}

	[MessageHandler((ushort) NetMsgIds.NetworkMessageID.PracticeMode)]
	private static void PracticeMode(ushort senderID, Message message)
	{
		if(!SinglePlayerFeatures) return;
		if (GameManager.Player2 != null) return;
		Connection c;
		server.TryGetClient(senderID, out c);
		GameManager.Player2 = new ClientInstance("Practice Opponent", c);
		GameManager.Player2.IsPracticeOpponent = true;
		SendPreGameInfo();
	}

}
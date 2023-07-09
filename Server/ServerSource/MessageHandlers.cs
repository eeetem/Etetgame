using System.Reflection;
using MultiplayerXeno.Items;
using Riptide;

namespace MultiplayerXeno;

public static partial class Networking
{
			
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.Kick)]
	private static void KickRequest(ushort senderID, Message message)
	{
		if (senderID != GameManager.Player1?.Connection?.Id)return;

		if (GameManager.Player2 != null)
		{
			if (GameManager.Player2?.Connection != null) Kick("Kicked by host", GameManager.Player2.Connection);
			GameManager.Player2 = null;
		}

		SendPreGameInfo();
	}
	[MessageHandler((ushort)NetMsgIds.NetworkMessageID.EndTurn)]
	private static void HandleEndTurn(ushort senderID,Message message)
	{
		Client? currentPlayer;
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
		List<SquadMember> squadMembers = message.GetSerializables<SquadMember>().ToList();
		if (GameManager.Player1?.Connection?.Id == senderID)
		{
			GameManager.Player1.SetSquadComp(squadMembers);
		}else if (GameManager.Player2?.Connection?.Id == senderID)
		{
			GameManager.Player2.SetSquadComp(squadMembers);
		}

		if (GameManager.Player1?.SquadComp != null && GameManager.Player2?.SquadComp != null)
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
		if (GameManager.Player1.Connection.Id == senderID)
		{
			if (!GameManager.IsPlayer1Turn){
				Console.WriteLine("Client sent an action out of turn");
				return;
			}
		}
		else if (GameManager.Player2.Connection.Id == senderID)
		{
			if (GameManager.IsPlayer1Turn){
				Console.WriteLine("Client sent an action out of turn");
				return;
			}
		}


		Action.GameActionPacket packet = message.GetSerializable<Action.GameActionPacket>();
		Console.WriteLine("recived action packet: " + packet.Type + " " + packet.UnitId + " " + packet.Target);
			
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
		if(controllable.IsPlayerOneTeam != GameManager.IsPlayer1Turn)
		{
			Console.WriteLine("Client sent an action for worng teams Unit");
			return;
		}

		Action act = Action.Actions[packet.Type]; //else get controllable specific actions
		if (act.Type == Action.ActionType.UseAbility)
		{
			int ability = int.Parse(packet.Args[0]);
			UseAbility.AbilityIndex = ability;
			UseAbility.abilityLock = true;
			IExtraAction a = controllable.GetAction(ability);
			if (a.WorldAction.DeliveryMethods.Find(x => x is Shootable) != null)
			{
				string target = packet.Args[1];
				switch (target)
				{
					case "Auto":
						Shootable.targeting = Shootable.targeting = Shootable.TargetingType.Auto;
						break;
					case "High":
						Shootable.targeting = Shootable.targeting = Shootable.TargetingType.High;
						break;
					case "Low":
						Shootable.targeting = Shootable.targeting = Shootable.TargetingType.Low;
						break;
					default:
						throw new ArgumentException("Invalid target type");
				}
			}
		}

		act.PerformServerSide(controllable, packet.Target);
		UseAbility.abilityLock = false;
			
	}

}
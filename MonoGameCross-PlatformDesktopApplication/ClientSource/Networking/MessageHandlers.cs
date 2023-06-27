using System;
using Myra.Graphics2D.UI;
using Riptide;

namespace MultiplayerXeno;

public static partial class Networking
{
	private static Dialog? mapLoadMsg;
	[MessageHandler((ushort)NetworkMessageID.MapDataInitiate)]
	private static void StartMapRecive(Message message)
	{
		mapLoadMsg  = UI.OptionMessage("Loading Map...", "Please Wait","",null,"",null);
		WorldManager.Instance.CurrentMap = new WorldManager.MapData();
		WorldManager.Instance.CurrentMap.Name = message.GetString();
		WorldManager.Instance.CurrentMap.Author = message.GetString();
		WorldManager.Instance.CurrentMap.unitCount = message.GetInt();
		WorldManager.Instance.WipeGrid();
		var msg = Message.Create(MessageSendMode.Reliable, (ushort)NetworkMessageID.MapDataInitiateConfirm);
		client?.Send(msg);
	}

	[MessageHandler((ushort) NetworkMessageID.MapDataFinish)]
	private static void FinishMapRecieve(Message message)
	{

		UI.Desktop.Widgets.Remove(mapLoadMsg);
	}

	[MessageHandler((ushort)NetworkMessageID.GameData)]
	private static void ReciveGameUpdate(Message message)
	{
		GameManager.GameStateData data = message.GetSerializable<GameManager.GameStateData>();
		GameManager.SetData(data);
	}

	[MessageHandler((ushort)NetworkMessageID.TileUpdate)]
	private static void ReciveTileUpdate(Message message)
	{
		
		WorldTile.WorldTileData data = message.GetSerializable<WorldTile.WorldTileData>();
		WorldManager.Instance.LoadWorldTile(data);
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

	

	/*

			ServerConnection.RegisterStaticPacketHandler<ProjectilePacket>(ReciveProjectilePacket);

		private static void ReciveProjectilePacket(ProjectilePacket packet, Connection connection)
		{
			new Projectile(packet);
		}

		public static void DoAction(GameActionPacket packet)
		{
			ServerConnection?.Send(packet);
		}





	 */
}
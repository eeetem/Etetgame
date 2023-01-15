using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using CommonData;
using Microsoft.Xna.Framework;
using Network;
using Network.Converter;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		public static TcpConnection serverConnection;
		private static string Ipport="";
		private static string Name="";
		public static ConnectionResult Connect(string ipport,string name)
		{
			Ipport = ipport;
			Name = name;
			ConnectionResult connectionResult = ConnectionResult.TCPConnectionNotAlive;
			//1. Establish a connection to the server.
			var ipAndPort = ipport.Split(":");
			serverConnection = ConnectionFactory.CreateTcpConnection(ipAndPort[0], int.Parse(ipAndPort[1]), out connectionResult);
			
			///serverConnection.NoDelay
			//2. Register what happens if we get a connection
			if(connectionResult != ConnectionResult.Connected)
			{
				return connectionResult;
			}
			Console.WriteLine($"{serverConnection.ToString()} Connection established");


			serverConnection.ConnectionClosed += (a, s) => UI.OptionMessage("Lost Connection: "+a.ToString(),"Do you want to reconnect?","no",null,"yes",Reconnect);
			serverConnection.TIMEOUT = 1000000000;
		
			
			serverConnection.RegisterRawDataHandler("mapUpdate",ReciveMapUpdate);
			serverConnection.RegisterStaticPacketHandler<GameDataPacket>(ReciveGameUpdate);

			serverConnection.RegisterRawDataHandler("TileUpdate",ReciveTileUpdate);
			serverConnection.RegisterRawDataHandler("notify",ReciveNotify);

			
			serverConnection.RegisterRawDataHandler("chatmsg",ReciveChatMessage);

			serverConnection.RegisterStaticPacketHandler<GameActionPacket>(ReciveAction);
			serverConnection.RegisterStaticPacketHandler<ProjectilePacket>(ReciveProjectilePacket);
			serverConnection.RegisterStaticPacketHandler<PreGameDataPacket>((i, a) =>
			{
				Console.WriteLine("LobbyData Recived");
				GameManager.PreGameData = i;

			});
			Console.WriteLine("Registered Handlers");
			Thread.Sleep(500);//give server  a second to register the packet handler
			Console.WriteLine("Registering Player");
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("register", name));
		
			


			return connectionResult;

		}

		private static void Reconnect(object sender, EventArgs e)
		{
			Connect(Ipport, Name);
		}

	

		private static void ReciveNotify(RawData text,Connection connection)
		{
			
			UI.ShowMessage("Server Notice",RawDataConverter.ToUnicodeString(text));
		}

		private static void ReciveTileUpdate(RawData rawData, Connection connection)
		{
			
			using (Stream dataStream = new MemoryStream(rawData.Data))
			{
				BinaryFormatter bformatter = new BinaryFormatter();
				WorldTileData prefabData = (WorldTileData)bformatter.Deserialize(dataStream);
				Console.WriteLine("recived tile update "+prefabData.position);
				WorldManager.Instance.LoadWorldTile(prefabData);



			}

			
		}

		private static void ReciveAction(GameActionPacket packet, Connection connection)
		{
			GameManager.ParsePacket(packet);
		}

		private static void ReciveMapUpdate(RawData rawData, Connection connection)
		{
			var msg  = UI.OptionMessage("Loading Map...", "Please Wait","",null,"",null);
			WorldManager.Instance.LoadData(rawData.Data);
			msg.RemoveFromDesktop();
			
		}
		private static void ReciveGameUpdate(GameDataPacket packet, Connection connection)
		{
			GameManager.SetData(packet);
			
		}

		private static void ReciveProjectilePacket(ProjectilePacket packet, Connection connection)
		{
			new Projectile(packet);
			
		}

		public static void DoAction(GameActionPacket packet)
		{
			serverConnection.Send(packet);
		}

		public static void ChatMSG(string content)
		{
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("chatmsg",content));
			
		}

		private static void ReciveChatMessage(RawData rawData, Connection connection)
		{
			UI.RecieveChatMessage(RawDataConverter.ToUTF8String(rawData));
		}



		public static void SendPreGameUpdate()
		{
			serverConnection.SendRawData(RawDataConverter.FromInt32("mapSelect",GameManager.PreGameData.SelectedIndex));
		}

		public static void SendStartGame()
		{
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("gameState",""));
		}
	}
}
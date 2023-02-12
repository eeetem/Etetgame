using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using CommonData;
using Microsoft.Xna.Framework;
using Network;
using Network.Converter;
using Network.Enums;
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


			serverConnection.ConnectionClosed += (a, s) =>
			{
				if (a == CloseReason.Timeout)
				{
					UI.OptionMessage("Lost Connection: " + a.ToString(), "Do you want to reconnect?", "no", (a,b)=> { Disconnect(); }, "yes", Reconnect);
				}
				else
				{
					UI.ShowMessage("Lost Connection", a.ToString());
					Disconnect();
				}


			};
			serverConnection.TIMEOUT = 1000000000;
		
			
			serverConnection.RegisterStaticPacketHandler<MapDataPacket>((data, b) =>
			{
				var msg  = UI.OptionMessage("Loading Map...", "Please Wait","",null,"",null);
				WorldManager.Instance.LoadMap(data.MapData);
				msg.RemoveFromDesktop();
			});
			serverConnection.RegisterStaticPacketHandler<GameDataPacket>(ReciveGameUpdate);

			serverConnection.RegisterRawDataHandler("TileUpdate",ReciveTileUpdate);
			serverConnection.RegisterRawDataHandler("notify", (i, a) =>
			{
				UI.ShowMessage("Server Notice",RawDataConverter.ToUnicodeString(i));
			});

			
			serverConnection.RegisterRawDataHandler("chatmsg", (rawData, b) =>
			{
				UI.RecieveChatMessage(RawDataConverter.ToUTF8String(rawData));	
			});

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
		public static void Disconnect()
		{
			serverConnection.Close(CloseReason.ClientClosed);
			WorldManager.Instance.WipeGrid();
			GameManager.intated = false;
			if (MasterServerNetworking.serverConnection != null && MasterServerNetworking.serverConnection.IsAlive)
			{
				UI.SetUI(UI.LobbyBrowser);
			}
			else
			{
				UI.SetUI(UI.MainMenu);
			}
		}

		public static void UploadMap(string path)
		{
			
			var packet = new MapDataPacket(MapData.Deserialse(File.ReadAllText(path)));
			serverConnection.Send(packet);
		}

		private static void Reconnect(object sender, EventArgs e)
		{
			Connect(Ipport, Name);
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
		



		public static void SendPreGameUpdate()
		{
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("mapSelect",GameManager.PreGameData.SelectedMap));
		}

		public static void SendStartGame()
		{
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("gameState",""));
		}
	}
}
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using MultiplayerXeno;
using MultiplayerXeno.UILayouts;
using Network;
using Network.Converter;
using Network.Enums;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		public static TcpConnection? ServerConnection;
		private static string Ipport="";
		private static string Name="";
		public static ConnectionResult Connect(string ipport,string name)
		{
			Ipport = ipport;
			Name = name;
			ConnectionResult connectionResult = ConnectionResult.TCPConnectionNotAlive;
			//1. Establish a connection to the server.
			var ipAndPort = ipport.Split(":");
			ServerConnection = ConnectionFactory.CreateTcpConnection(ipAndPort[0], int.Parse(ipAndPort[1]), out connectionResult);
			///serverConnection.NoDelay
			//2. Register what happens if we get a connection
			if(connectionResult != ConnectionResult.Connected)
			{
				return connectionResult;
			}
			Console.WriteLine($"{ServerConnection.ToString()} Connection established");


			ServerConnection.ConnectionClosed += (a, s) =>
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
			ServerConnection.TIMEOUT = 1000000000;
		
			
			ServerConnection.RegisterStaticPacketHandler<MapDataPacket>((data, b) =>
			{
				var msg  = UI.OptionMessage("Loading Map...", "Please Wait","",null,"",null);
				WorldManager.Instance.LoadMap(data.MapData);
				msg.RemoveFromDesktop();
			});
			ServerConnection.RegisterStaticPacketHandler<GameDataPacket>(ReciveGameUpdate);

			ServerConnection.RegisterRawDataHandler("TileUpdate",ReciveTileUpdate);
			ServerConnection.RegisterRawDataHandler("notify", (i, a) =>
			{
				UI.ShowMessage("Server Notice",RawDataConverter.ToUnicodeString(i));
			});
			ServerConnection.RegisterStaticPacketHandler<PreGameDataPacket>((i, a) =>
			{
				Console.WriteLine("LobbyData Recived");
				GameManager.PreGameData = i;

			});
			
			ServerConnection.RegisterRawDataHandler("chatmsg", (rawData, b) =>
			{
				Chat.ReciveMessage(RawDataConverter.ToUTF8String(rawData));	
			});

			ServerConnection.RegisterStaticPacketHandler<GameActionPacket>(ReciveAction);
			ServerConnection.RegisterStaticPacketHandler<ProjectilePacket>(ReciveProjectilePacket);
			ServerConnection.RegisterStaticPacketHandler<PreGameDataPacket>((i, a) =>
			{
				Console.WriteLine("LobbyData Recived");
				GameManager.PreGameData = i;

			});
	
			Console.WriteLine("Registered Handlers");
			Thread.Sleep(500);//give server  a second to register the packet handler
			Console.WriteLine("Registering Player");
			ServerConnection.SendRawData(RawDataConverter.FromUTF8String("register", name));
		
			


			return connectionResult;

		}
		public static void Disconnect()
		{
			if (ServerConnection != null)
			{
				ServerConnection.Close(CloseReason.ClientClosed);
			}

			WorldManager.Instance.WipeGrid();
			GameManager.intated = false;
			if (MasterServerNetworking.serverConnection != null && MasterServerNetworking.serverConnection.IsAlive)
			{
				UI.SetUI(new LobbyBrowserLayout());
			}
			else
			{
				UI.SetUI(new MainMenuLayout());
			}
			GameManager.ResetGame();
		}

		public static void UploadMap(string path)
		{
			
			var packet = new MapDataPacket(MapData.Deserialse(File.ReadAllText(path)));
			ServerConnection.Send(packet);
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
#pragma warning disable SYSLIB0011
				WorldTileData prefabData = (WorldTileData)bformatter.Deserialize(dataStream);
#pragma warning restore SYSLIB0011
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
			try
			{
				GameManager.SetData(packet);
			}catch(Exception e)
			{
				Console.WriteLine("Game Update Error:"+e);
			}
		}

		private static void ReciveProjectilePacket(ProjectilePacket packet, Connection connection)
		{
			new Projectile(packet);
		}

		public static void DoAction(GameActionPacket packet)
		{
			ServerConnection?.Send(packet);
		}

		public static void ChatMSG(string content)
		{
			ServerConnection?.SendRawData(RawDataConverter.FromUTF8String("chatmsg",content));
			
		}

		public static void SendPreGameUpdate()
		{
			ServerConnection?.SendRawData(RawDataConverter.FromUTF8String("mapSelect",GameManager.PreGameData.SelectedMap));
			ServerConnection?.Send(GameManager.PreGameData);
		}

		public static void SendStartGame()
		{
			ServerConnection?.SendRawData(RawDataConverter.FromUTF8String("gameState",""));
		}
	}
}
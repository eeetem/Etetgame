using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MultiplayerXeno.UILayouts;

namespace MultiplayerXeno;

public class MasterServerNetworking
{
		public static TcpConnection? serverConnection;
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
			Console.WriteLine($"{serverConnection.ToString()} Connection to master server established");
			
			serverConnection.ConnectionClosed += (a, s) =>
			{
				if(Networking.ServerConnection == null || !Networking.ServerConnection.IsAlive)
				{
					UI.SetUI(new MainMenuLayout());
					Console.WriteLine("lost connection to master server");
					UI.ShowMessage("Lost Connection To Master Server", a.ToString());
				}
			};
			
			serverConnection.RegisterRawDataHandler("notify", (i, a) =>
			{
				UI.ShowMessage("Server Notice",RawDataConverter.ToUnicodeString(i));
			});
			serverConnection.RegisterRawDataHandler("playerList", (data, a) =>
			{
				string list = RawDataConverter.ToUTF8String(data);
				Players = list.Split(";").ToList();
				UI.SetUI(null);
			});
			
			serverConnection.RegisterStaticPacketHandler<LobbyData>((data, a) =>
			{
				if (AwaitingLobby)
				{
					AwaitingLobby = false;
					Console.WriteLine("Connecting to started lobbby...");
					var result = Networking.Connect(ipport.Split(":")[0]+":"+data.Port,name);
					Console.WriteLine("result: "+result);
					
					//rety 5 times
					int i = 0;
					while (result == ConnectionResult.Timeout)
					{
						if(i >5)
						{
							Console.WriteLine("Failed to connect to lobby");
							break;
						}
						Console.WriteLine("retrying...");
						Thread.Sleep(100);
						result = Networking.Connect(ipport.Split(":")[0]+":"+data.Port,name);
						Console.WriteLine("result: "+result);
						i++;
					}
					
				}
				else
				{
					Lobbies.Add(data);
					UI.SetUI(new LobbyBrowserLayout());
				}
			});
			
	/*		serverConnection.RegisterRawDataHandler("chatmsg", (rawData, b) =>
			{
				if(Networking.ServerConnection == null || !Networking.ServerConnection.IsAlive)
				{
					Chat.ReciveMessage(RawDataConverter.ToUTF8String(rawData));	
				}
				
			});*/
			Thread.Sleep(1000);
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("register", name));
			RefreshServers();
			return connectionResult;

		}

		public static List<LobbyData> Lobbies = new List<LobbyData>();
		public static List<string> Players = new List<string>();

		private static bool AwaitingLobby;
		public static void CreateLobby(LobbyStartPacket packet)
		{
			serverConnection?.Send(packet);
			AwaitingLobby = true;

		}

		public static void RefreshServers()
		{
			Lobbies.Clear();
			serverConnection?.SendRawData(RawDataConverter.FromUTF8String("RequestLobbies",""));
		}
		public static void ChatMSG(string content)
		{
			//serverConnection?.SendRawData(RawDataConverter.FromUTF8String("chatmsg",content));
			
		}

		public static void Disconnect()
		{
			UI.SetUI(new MainMenuLayout());
			serverConnection?.Close(CloseReason.ClientClosed);
		}
}
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Microsoft.Xna.Framework;

using CommonData;
using Network;
using Network.Converter;
using Network.Enums;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		private static ServerConnectionContainer serverConnectionContainer;
		public static void Start()
		{
			//1. Start listen on a portw
			serverConnectionContainer = ConnectionFactory.CreateServerConnectionContainer(52233, false);

			serverConnectionContainer.ConnectionLost += (a, b, c) =>
			{
				Console.WriteLine($"{serverConnectionContainer.Count} {b.ToString()} Connection lost {a.IPRemoteEndPoint.Address}. Reason {c.ToString()}");
				string name;
				if (a == GameManager.Player1?.Connection)
				{
					name = GameManager.Player1.Name;
				}else if (a == GameManager.Player2?.Connection)
				{
					name = GameManager.Player2.Name;
				}
				else
				{
					return;
				}

				SendChatMessage(name+" left the game");
			};
			serverConnectionContainer.ConnectionEstablished += ConnectionEstablished;
			serverConnectionContainer.AllowUDPConnections = true;



	
			serverConnectionContainer.Start();
			
			Console.WriteLine("Started server at " + serverConnectionContainer.IPAddress +":"+ serverConnectionContainer.Port);
		
		}

		public static void Kick(string reason,Connection connection)
		{
			Console.WriteLine("Kicking " + connection.IPRemoteEndPoint.Address + " for " + reason);
			connection.SendRawData(RawDataConverter.FromUnicodeString("notify",reason));
			Thread.Sleep(1000);
			connection.Close(CloseReason.ClientClosed);
		}

		public static void NotifyAll(string message)
		{
		
			Notify(message,true);
			Notify(message,false);
		}

		public static void Notify(string message, bool player1)
		{
			if (player1)
			{
				GameManager.Player1?.Connection.SendRawData(RawDataConverter.FromUnicodeString("notify",message));	
			}
			else
			{
				GameManager.Player2?.Connection.SendRawData(RawDataConverter.FromUnicodeString("notify",message));	
			}
		}
		


		/// <summary>
		/// We got a connection.
		/// </summary>
		/// <param name="connection">The connection we got. (TCP or UDP)</param>
		private static void ConnectionEstablished(Connection connection, ConnectionType type)
		{
			Console.WriteLine($"{serverConnectionContainer.Count} {connection.GetType()} connected on port {connection.IPRemoteEndPoint.Port}");
			connection.EnableLogging = true;
			connection.TIMEOUT = 10000;
			
		//	connection.RegisterRawDataHandler("mapDownload",SendMapData);
			connection.RegisterRawDataHandler("register",RegisterClient);
			connection.RegisterRawDataHandler("chatmsg",ReciveChatMessage);
			
			connection.RegisterStaticPacketHandler<GameActionPacket>(ReciveAction);
			connection.RegisterStaticPacketHandler<StartDataPacket>(ReciveStartData);
			
			//3. Register packet listeners.
			//connection.RegisterRawDataHandler("HelloWorld", (rawData, con) => Console.WriteLine($"RawDataPacket received. Data: {rawData.ToUTF8String()}"));
		
		}

		private static void ReciveStartData(StartDataPacket packet, Connection connection)
		{
			if (GameManager.Player1?.Connection == connection)
			{
				GameManager.Player1.SetStartData(packet);
			}else if (GameManager.Player2?.Connection == connection)
			{
				GameManager.Player2.SetStartData(packet);
			}

			if (GameManager.Player1?.StartData != null && GameManager.Player2?.StartData != null)
			{
				GameManager.StartGame();
			}
		}


		private static void RegisterClient(RawData rawData, Connection connection)
		{
			string name = RawDataConverter.ToUTF8String(rawData);

			if (GameManager.Player1 == null)
			{
			
				GameManager.Player1 = new Client(name,connection,1);
			}
			else if (GameManager.Player1.Name == name)
			{
				if (GameManager.Player1.Connection.IsAlive)
				{
					Kick("Player with same name is already in the game",connection);
					return;
				}

				GameManager.Player1.Connection = connection;//reconnection
				
			}
			else if (GameManager.Player2 == null)
			{
			
				GameManager.Player2 = new Client(name,connection,2);
			}
			else if (GameManager.Player2.Name == name)
			{
				if (GameManager.Player2.Connection.IsAlive)
				{
					Kick("Player with same name is already in the game",connection);
					return;
				}
				GameManager.Player2.Connection = connection;//reconnection
			}
			else
			{
				Kick("Server Full",connection);
				return;
			}

			GameManager.SendData();
			SendMapData(connection);
			SendChatMessage(name+" joined the game");


		}

		public static void SendTileUpdate(WorldTile wo)
		{
			WorldTileData worldTileData = wo.GetData();

			using (MemoryStream stream = new MemoryStream())
			{
				BinaryFormatter bf = new BinaryFormatter();
				bf.Serialize(stream,worldTileData);
				GameManager.Player1?.Connection.SendRawData("TileUpdate",stream.ToArray());
				GameManager.Player2?.Connection.SendRawData("TileUpdate",stream.ToArray());
			}
			
			
			

		}

//since this is just a request we dont read raw data at all
	
		

		private static void SendMapData(Connection connection)
		{
			WorldManager.Instance.SaveData("temp.mapdata");
			byte[] mapData = File.ReadAllBytes("temp.mapdata");
			connection.SendRawData("mapUpdate", mapData);

		}

		private static void ReciveAction(GameActionPacket packet, Connection connection)
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

			if (currentPlayer?.Connection != connection)
			{
				//out of turn action. perhaps desync or hax? kick perhaps
				
				return;
			}

			GameManager.ParsePacket(packet);




		}

		public static void DoAction(Packet packet)
		{
			GameManager.Player1?.Connection.Send(packet);
			GameManager.Player2?.Connection.Send(packet);

		}


		public static void ReciveChatMessage(RawData rawData, Connection connection)
		{
			string message = RawDataConverter.ToUTF8String(rawData);
			string name;
			if (GameManager.Player1?.Connection == connection)
			{
				name = GameManager.Player1.Name;
			}
			else if (GameManager.Player2?.Connection == connection)
			{
				name = GameManager.Player2.Name;
			}
			else
			{
				return;
			}

			message = $"{name}: {message}";
			SendChatMessage(message);
		}

		public static void SendChatMessage(string text)
		{
			GameManager.Player1?.Connection.SendRawData(RawDataConverter.FromUTF8String("chatmsg",text));
			GameManager.Player2?.Connection.SendRawData(RawDataConverter.FromUTF8String("chatmsg",text));
		}

		public static void StartGame()
		{
			GameManager.Player1?.Connection.SendRawData(RawDataConverter.FromUTF8String("gamestate","start"));
			GameManager.Player2?.Connection.SendRawData(RawDataConverter.FromUTF8String("gamestate","start"));
		}
	}
}
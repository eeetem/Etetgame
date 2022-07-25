using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Xna.Framework;
using Packets;
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
			serverConnectionContainer = ConnectionFactory.CreateServerConnectionContainer(5555, false);

			//2. Apply optional settings.

			serverConnectionContainer.ConnectionLost += (a, b, c) => Console.WriteLine($"{serverConnectionContainer.Count} {b.ToString()} Connection lost {a.IPRemoteEndPoint.Port}. Reason {c.ToString()}");
			serverConnectionContainer.ConnectionEstablished += ConnectionEstablished;
			serverConnectionContainer.AllowUDPConnections = true;
			
			
			
	
			serverConnectionContainer.Start();
		}
		
		/// <summary>
		/// We got a connection.
		/// </summary>
		/// <param name="connection">The connection we got. (TCP or UDP)</param>
		private static void ConnectionEstablished(Connection connection, ConnectionType type)
		{
			Console.WriteLine($"{serverConnectionContainer.Count} {connection.GetType()} connected on port {connection.IPRemoteEndPoint.Port}");

			
			
			connection.RegisterRawDataHandler("mapDownload",SendMapData);
			connection.RegisterRawDataHandler("register",RegisterClient);
			connection.RegisterStaticPacketHandler<GameActionPacket>(ReciveAction);
			//3. Register packet listeners.
			//connection.RegisterRawDataHandler("HelloWorld", (rawData, con) => Console.WriteLine($"RawDataPacket received. Data: {rawData.ToUTF8String()}"));
		
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
				GameManager.Player1.Connection = connection;//reconnection
			}
			else if (GameManager.Player2 == null)
			{
			
				GameManager.Player2 = new Client(name,connection,2);
			}
			else if (GameManager.Player2.Name == name)
			{
				GameManager.Player2.Connection = connection;//reconnection
			}
			else
			{
				connection.Close(CloseReason.ClientClosed);//kick extra connections
			}

			GameManager.SendData();


		}

//since this is just a request we dont read raw data at all
		private static void SendMapData(RawData rawData, Connection connection)
		{
			byte[] mapData = File.ReadAllBytes("map.mapdata");
			connection.SendRawData("mapUpdate", mapData);

		}

		private static void ReciveAction(GameActionPacket packet, Connection connection)
		{
			Client currentPlayer;
			if (GameManager.IsPlayer1Turn)
			{
				currentPlayer = GameManager.Player1;
			}
			else
			{
				currentPlayer = GameManager.Player2;
			}

			if (currentPlayer.Connection != connection)
			{
				//out of turn action. perhaps desync or hax? kick perhaps
				
				return;
			}

			if (packet.Type == ActionType.EndTurn)
			{
				GameManager.NextTurn();
			}
			
			
			
		}
	}
}
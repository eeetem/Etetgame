using System;
using System.IO;
using Network;
using Network.Enums;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		private static ServerConnectionContainer serverConnectionContainer;
		public static void Start()
		{
			//1. Start listen on a port
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
			//3. Register packet listeners.
			//connection.RegisterRawDataHandler("HelloWorld", (rawData, con) => Console.WriteLine($"RawDataPacket received. Data: {rawData.ToUTF8String()}"));
		
		}

//since this is just a request we dont read raw data at all
		private static void SendMapData(RawData rawData, Connection connection)
		{
			byte[] mapData = File.ReadAllBytes("map.mapdata");
			connection.SendRawData("mapUpdate", mapData);

		}
	}
}
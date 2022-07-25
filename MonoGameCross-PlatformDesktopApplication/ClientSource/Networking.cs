using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Packets;
using Network;
using Network.Converter;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		public static TcpConnection serverConnection;
		public static bool Connect(string ip,string name)
		{
			ConnectionResult connectionResult = ConnectionResult.TCPConnectionNotAlive;
			//1. Establish a connection to the server.
			serverConnection = ConnectionFactory.CreateTcpConnection(ip, 5555, out connectionResult);
			//2. Register what happens if we get a connection
			if(connectionResult != ConnectionResult.Connected)
			{
				return false;
			}
			Console.WriteLine($"{serverConnection.ToString()} Connection established");


			serverConnection.ConnectionClosed += (a, s) => UI.PopUp("Lost connection", a.ToString());
				
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("register", name));
			
			serverConnection.RegisterRawDataHandler("mapUpdate",ReciveMapUpdate);
			serverConnection.RegisterStaticPacketHandler<GameDataPacket>(ReciveGameUpdate);
		
			serverConnection.SendRawData("mapDownload",new byte[1]);
		
			


			return true;

		}

	

		private static void ReciveMapUpdate(RawData rawData, Connection connection)
		{
			WorldObjectManager.LoadData(rawData.Data);
			
		}
		private static void ReciveGameUpdate(GameDataPacket packet, Connection connection)
		{
			GameManager.SetData(packet);
			
		}

	

	}
}
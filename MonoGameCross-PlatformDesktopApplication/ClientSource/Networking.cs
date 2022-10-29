﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CommonData;
using Network;
using Network.Converter;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		public static TcpConnection serverConnection;
		public static ConnectionResult Connect(string ip,string name)
		{
			ConnectionResult connectionResult = ConnectionResult.TCPConnectionNotAlive;
			//1. Establish a connection to the server.
			serverConnection = ConnectionFactory.CreateTcpConnection(ip, 52233, out connectionResult);
			
			//2. Register what happens if we get a connection
			if(connectionResult != ConnectionResult.Connected)
			{
				return connectionResult;
			}
			Console.WriteLine($"{serverConnection.ToString()} Connection established");


			serverConnection.ConnectionClosed += (a, s) => UI.ShowMessage("Lost connection", a.ToString());
				
			serverConnection.SendRawData(RawDataConverter.FromUTF8String("register", name));
			
			serverConnection.RegisterRawDataHandler("mapUpdate",ReciveMapUpdate);
			serverConnection.RegisterStaticPacketHandler<GameDataPacket>(ReciveGameUpdate);

			serverConnection.RegisterRawDataHandler("TileUpdate",ReciveTileUpdate);
			serverConnection.RegisterRawDataHandler("notify",ReciveNotify);

			
			serverConnection.RegisterStaticPacketHandler<GameActionPacket>(ReciveAction);
			serverConnection.RegisterStaticPacketHandler<MovementPacket>(ReciveAction);
			serverConnection.RegisterStaticPacketHandler<FacePacket>(ReciveAction);
			serverConnection.RegisterStaticPacketHandler<FirePacket>(ReciveAction);
		
			


			return connectionResult;

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
				WorldManager.Instance.LoadWorldTile(prefabData);



			}

			
		}

		private static void ReciveAction(GameActionPacket packet, Connection connection)
		{
			GameManager.ParsePacket(packet);
		}

		private static void ReciveMapUpdate(RawData rawData, Connection connection)
		{
			WorldManager.Instance.LoadData(rawData.Data);
			
		}
		private static void ReciveGameUpdate(GameDataPacket packet, Connection connection)
		{
			GameManager.SetData(packet);
			
		}
		
		public static void DoAction(GameActionPacket packet)
		{
			serverConnection.Send(packet);
		}

	

	}
}
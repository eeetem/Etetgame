using System;
using Network;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking 
	{

		public static bool Connect(string ip)
		{
			ConnectionResult connectionResult = ConnectionResult.TCPConnectionNotAlive;
			//1. Establish a connection to the server.
			TcpConnection tcpConnection = ConnectionFactory.CreateTcpConnection(ip, 5555, out connectionResult);
			//2. Register what happens if we get a connection
			if(connectionResult != ConnectionResult.Connected)
			{
				return false;
			}
			Console.WriteLine($"{tcpConnection.ToString()} Connection established");
			//3. Send a raw data packet request.
			/*
			tcpConnection.SendRawData(RawDataConverter.FromUTF8String("HelloWorld", "Hello, this is the RawDataExample!"));
			tcpConnection.SendRawData(RawDataConverter.FromBoolean("BoolValue", true));
			tcpConnection.SendRawData(RawDataConverter.FromBoolean("BoolValue", false));
			tcpConnection.SendRawData(RawDataConverter.FromDouble("DoubleValue", 32.99311325d));
			//4. Send a raw data packet request without any helper class
			tcpConnection.SendRawData("HelloWorld", Encoding.UTF8.GetBytes("Hello, this is the RawDataExample!"));
			tcpConnection.SendRawData("BoolValue", BitConverter.GetBytes(true));
			tcpConnection.SendRawData("BoolValue", BitConverter.GetBytes(false));
			tcpConnection.SendRawData("DoubleValue", BitConverter.GetBytes(32.99311325d));
			*/

			tcpConnection.ConnectionClosed += (a, s) => UI.PopUp("Lost connection", a.ToString());
				
			tcpConnection.RegisterRawDataHandler("mapUpdate",ReciveMapUpdate);
			tcpConnection.SendRawData("mapDownload",new byte[1]);
		
			


			return true;

		}

		private static void ReciveMapUpdate(RawData rawData, Connection connection)
		{
			WorldObjectManager.LoadData(rawData.Data);
			
		}

	}
}
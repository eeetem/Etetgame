using System.Collections.Generic;
using CommonData;
using Network;

namespace MultiplayerXeno
{
	public class Client
	{
		public int Team;
		public Connection Connection;
		public string Name;
		public StartDataPacket? StartData { get; private set; }

		public Client(string name,Connection con, int team)
		{
			this.Name = name;
			Connection = con;
			Team = team;
		}

		public void SetStartData(StartDataPacket packet)
		{
			if (packet.Scouts + packet.Soldiers > 5)
			{
				Console.WriteLine("Recived "+packet.Scouts + packet.Soldiers+" total units. StartData Rejected");
				return;
			}

			StartData = packet;
		}


		//public static List<Client> Clients = new List<Client>();
	}
}
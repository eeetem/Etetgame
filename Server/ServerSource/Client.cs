using System.Collections.Generic;
using Network;

namespace MultiplayerXeno
{
	public class Client
	{
		public int Team;
		public Connection Connection;
		public string Name;

		public Client(string name,Connection con, int team)
		{
			this.Name = name;
			Connection = con;
			Team = team;
		}


		//public static List<Client> Clients = new List<Client>();
	}
}
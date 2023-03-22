﻿using System.Collections.Generic;
using CommonData;
using Network;

namespace MultiplayerXeno
{
	public class Client
	{
		public Connection Connection;
		public string Name;
		public SquadCompPacket? SquadComp { get; private set; }

		public Client(string name,Connection con)
		{
			this.Name = name;
			Connection = con;
		}

		public void SetSquadComp(SquadCompPacket packet)
		{
			//todo verify
			SquadComp = packet;
		}


		//public static List<Client> Clients = new List<Client>();
	}
}
﻿using Network.Packets;

namespace MultiplayerXeno
{
	
		public class GameDataPacket : Packet
		{
			public bool? IsPlayerOne { get; set; }
			public bool IsPlayer1Turn{ get; set; }

			public int Score { get; set; }
			public GameState GameState { get; set; }


		}
	
}
﻿namespace MultiplayerXeno;

public class NetMsgIds
{
	public enum NetworkMessageID : ushort
	{
		//masterserver
		PlayerList = 0,
		LobbyCreated =1,
		LobbyStart =2,
		LobbyList = 3,
		Refresh =4,
		//server

		MapDataInitiate =5,
		MapDataFinish =6,
		GameData =7,
		GameAction =8,
		StartGame =9,
		EndTurn =10,
		SquadComp =11,
		MapUpload =12,
		TileUpdate =13,
		MapDataInitiateConfirm = 14,
		ReplaySequence = 15,
		Notify = 16,
		Chat = 17,
		PreGameData = 18,
		Kick = 19,
	}
}
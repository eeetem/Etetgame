using Network.Packets;
using System;
using System.Collections.Generic;

namespace MultiplayerXeno;

public class PreGameDataPacket : Packet
{
	public List<string> MapList { get; set; } = new List<string>();
	public List<string> CustomMapList { get; set; }= new List<string>();
	public string HostName { get; set; } = "";
	public string Player2Name { get; set; } = "";
	public List<string> Spectators { get; set; } = new List<string>();
	public string SelectedMap { get; set; } = "";
	public int TurnTime { get; set; } = 180;

}

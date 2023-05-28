﻿using System.Collections.Generic;

namespace MultiplayerXeno;

public static class Chat
{
	public static List<string> Messages = new List<string>();
	public static void ReciveMessage(string message)
	{
		Messages.Add(message);
		if (Messages.Count > 20)
		{
			Messages.RemoveAt(0);
		}
		Audio.PlaySound("UI/chat");
	}
	public static void SendMessage(string message)
	{
		if (message != "")
		{ 
			if (Networking.ServerConnection != null && Networking.ServerConnection.IsAlive)
			{
				Networking.ChatMSG(message);
			}
			else
			{
				MasterServerNetworking.ChatMSG(message);
			}
		}
	}
	
}
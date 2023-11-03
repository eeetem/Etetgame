﻿using System.Collections.Generic;
using DefconNull.Networking;

namespace DefconNull;

public static class Chat
{
	public static Queue<string> Messages = new Queue<string>();
	private static bool deleteLoop = false;
	public static void ReciveMessage(string message)
	{
		Messages.Enqueue(message);
		if (Messages.Count > 15)
		{
			Messages.Dequeue();
		}
		Audio.PlaySound("UI/chat");
		deleteLoop = true;

	}

	private static float ticker = 60000;
	public static void Update(float delta)
	{
		if (deleteLoop)
		{
			ticker -= delta;
			if(ticker <= 0)
			{
				ticker = 60000;
				Messages.Dequeue();
				if (Messages.Count == 0)
				{
					deleteLoop = false;
				}
			}
		}
	}

	public static void SendMessage(string message)
	{
		if (message != "")
		{ 
			if (NetworkingManager.Connected )
			{
				NetworkingManager.ChatMSG(message);
			}
			//	MasterServerNetworking.ChatMSG(message);
		}
	}
	
}
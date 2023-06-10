using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MultiplayerXeno;

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

	private static float ticker = 600;
	public static void Update(float delta)
	{
		if (deleteLoop)
		{
			ticker -= delta;
			if(ticker <= 0)
			{
				ticker = 600;
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
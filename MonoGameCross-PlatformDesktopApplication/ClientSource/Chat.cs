using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiplayerXeno;

public static class Chat
{
	public static List<string> Messages = new List<string>();
	public static void ReciveMessage(string message)
	{
		Messages.Add(message);
		if (Messages.Count > 15)
		{
			Messages.RemoveAt(0);
		}
		Audio.PlaySound("UI/chat");
		int i = Messages.Count - 1;
		Task.Factory.StartNew(() =>
		{
			Task.Delay(60*1000).Wait();
			Messages.RemoveAt(i);
		});
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
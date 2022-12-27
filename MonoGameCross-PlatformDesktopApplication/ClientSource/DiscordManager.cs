using System;
using NetDiscordRpc;
using NetDiscordRpc.Core.Logger;
using NetDiscordRpc.RPC;

namespace MultiplayerXeno;

public static class DiscordManager
{
	public static DiscordRPC client { get; private set; }
	private static bool enabled = false;
	public static string details = "gaming";
	static DateTime startTime = DateTime.Now;
	public static void Init()
	{
		client = new DiscordRPC("1056950888956178485");
		//Connect to the RPC
		//Set the logger
		client.Logger = new ConsoleLogger();

		client.Initialize();
		client.OnReady += (sender, e) =>
		{
			Console.WriteLine("Received Ready from user {0}", e.User.Username);
		};

		client.OnPresenceUpdate += (sender, e) =>
		{
			Console.WriteLine("Received Update! {0}", e.Presence);
		};
		enabled = true;
		client.SetPresence(new RichPresence()
		{
			Details = "https://discord.gg/SjfmWDaJzz",
			State = "In Menu",
			Timestamps = new Timestamps()
			{
				Start = startTime,

			},			
			Assets = new Assets()
			{
				LargeImageKey = "main",
				LargeImageText = "if you read this you're gay",
				SmallImageKey = "main",
				SmallImageText = "gay aswell",
			}

		}); 
	}


	public static void Update()
	{
		if(!enabled)return;
		
		
	}
}
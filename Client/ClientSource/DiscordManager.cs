using System;
using NetDiscordRpc;
using NetDiscordRpc.RPC;

namespace DefconNull;

public static class DiscordManager
{
	public static DiscordRPC Client { get; private set; } = null!;
	private static bool enabled;
	public static void Init()
	{
		Client = new DiscordRPC("1056950888956178485");
		//Connect to the RPC
		//Set the logger
		//client.Logger = new ConsoleLogger();

		Client.Initialize();
		Client.OnReady += (sender, e) =>
		{
			Console.WriteLine("Received Ready from user {0}", e.User.Username);
		};

		Client.OnPresenceUpdate += (sender, e) =>
		{
			Console.WriteLine("Received Update! {0}", e.Presence);
		};
		enabled = true;
		Client.SetPresence(new RichPresence()
		{
			///Details = "https://discord.gg/TrmAJbMaQ3",
			State = "In Menu",
			Timestamps = new Timestamps()
			{
				Start = DateTime.Now,

			},			
			Assets = new Assets()
			{
				LargeImageKey = "main",
				LargeImageText = "if you read this you're gay",
				SmallImageKey = "main",
				SmallImageText = "gay aswell",
			},
			Buttons = new Button[1]
			{
				new Button()
				{
					Label = "Discord",
					Url = "https://discord.gg/TrmAJbMaQ3"
				}
			}

		}); 
	}


	public static void Update()
	{
		if(!enabled)return;
		
		
	}
}
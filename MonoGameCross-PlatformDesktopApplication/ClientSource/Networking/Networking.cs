using System;
using System.Collections.Generic;
using System.IO;
using MultiplayerXeno.UILayouts;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Transports.Udp;


namespace MultiplayerXeno;

public static partial class Networking
{
	private static Client? client;
	private static string Ipport="";
	private static string Name="";
	public static bool Connected => client != null && client.IsConnected;
	public static bool Connect(string ipport,string name)
	{
		Ipport = ipport;
		Name = name;
		Message.MaxPayloadSize = 50000000;
		client = new Client( new UdpClient());
		client.TimeoutTime = ushort.MaxValue;
		var msg = Message.Create();
		msg.AddString(name);
		bool result = client.Connect(ipport,message:msg);
		if (!result)
		{
			return false;
		}
	
		Console.WriteLine($"{client} Connection established");

		client.ConnectionFailed += (a, b) =>
		{
			UI.ShowMessage("Connection Failed",b.Message?.GetString() ?? string.Empty );
		};
		client.Disconnected += (a, b) =>
		{
			if (b.Reason == DisconnectReason.TimedOut)
			{
				UI.OptionMessage("Lost Connection: " + a.ToString(), "Do you want to reconnect?", "no", (a,b)=> { Disconnect(); }, "yes", (a, b) => { Reconnect(); });
			}
			else
			{
				UI.ShowMessage("Connection Rejected By Server", b.ToString());
				Disconnect();
			}
		};
		client.MessageReceived += (a, b) =>
		{
			Console.WriteLine("Recived Message: " + (NetworkMessageID)b.MessageId);
		};




		return true;

	}
	public static void SendStartGame()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.StartGame);
		client.Send(msg);
	}
	public static void Disconnect()
	{
		client?.Disconnect();
		WorldManager.Instance.WipeGrid();
		GameManager.intated = false;
		/*	if (MasterServerNetworking.serverConnection != null && MasterServerNetworking.serverConnection.IsAlive)
			{
				UI.SetUI(new LobbyBrowserLayout());
			}
			else
			{*/
		UI.SetUI(new MainMenuLayout());
		//}
		GameManager.ResetGame();
	}

	public static void UploadMap(string path)
	{
		//var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.MapUpload);
	//	msg.Add(WorldManager.MapData.FromJSON(File.ReadAllText(path)));
		////client?.Send(msg);
	}

	private static void Reconnect()
	{
		Connect(Ipport, Name);
	}
		
		

	public static void Update()
	{
		client?.Update();
	}

	public static void ChatMSG(string message)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.Chat);
		msg.AddString(message);
		client?.Send(msg);
	}

	public static void EndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.EndTurn);
		client?.Send(msg);
	}

	public static void SendSquadComp(List<SquadMember> composition)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.SquadComp);
		msg.AddSerializables<>(composition.ToString());
		client?.Send(msg);
	}
		
	public static void SendPreGameUpdate()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.PreGameData);
		msg.AddSerializable(GameManager.PreGameData);
		client?.Send(msg);
	}


	public static void KickRequest()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.Kick);
		client?.Send(msg);
	}
	

	
}
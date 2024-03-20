﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using DefconNull.ReplaySequence;
using Riptide;
using Riptide.Transports.Tcp;

namespace DefconNull.Networking;

public class MasterServerNetworking
{
	public static Client? Client;
	private static string Ipport="";
	private static string Name="";
	public static bool IsConnected => Client != null && Client.IsConnected;
		
	public static bool Connect(string ipport,string name)
	{
		if(Client!=null && Client.IsConnected)
			Client.Disconnect();
		
		Ipport = ipport;
		Name = name;


		Client = new Client( new TcpClient());
		Message.MaxPayloadSize = 2048*2;
		Client.TimeoutTime = 11000;
		Client.HeartbeatInterval = 5000;
		
		var msg = Message.Create();
		msg.AddString(name);
		bool result = Client.Connect(ipport,message:msg);
		if (!result)
		{
			return false;
		}
	
			

		Client.Connected += (a, b) =>
		{
			Console.WriteLine($"{Client} Connection to master server established");
			
			UI.SetUI(new LobbyBrowserLayout());
		
		};
		Client.ConnectionFailed += (a, b) =>
		{
			UI.ShowMessage("Error", "Could not connect to master server");
			UI.ShowMessage("Connection Failed",b.Message?.GetString() ?? string.Empty );
		};
		Client.Disconnected += (a, b) =>
		{
			
			Console.WriteLine("lost connection to master server");
			if(!NetworkingManager.Connected)//only change ui if we're not in a server
			{
				UI.SetUI(new MainMenuLayout());
				Task t = new Task(() =>
				{
					System.Threading.Thread.Sleep(1000);
					UI.ShowMessage("Lost Connection To Master Server", b.Reason.ToString());
				});
				SequenceManager.RunNextAfterFrames(t);
				
			}
		};
		Client.MessageReceived += (a, b) =>
		{
			Console.WriteLine("Recived Message: " + ( NetworkingManager.NetworkMessageID)b.MessageId);
		};


		RefreshServers();


		return true;
		
	}

	public static List<LobbyData> Lobbies = new List<LobbyData>();
	public static List<string> Players = new List<string>();
	public static void CreateLobby(string name,string password = "")
	{
		var msg = Message.Create(MessageSendMode.Unreliable, (ushort)  NetworkingManager.MasterServerNetworkMessageID.LobbyStart);
		msg.Add(name);
		msg.Add(password);
		Client?.Send(msg);

	}
		

	public static void RefreshServers()
	{
		Lobbies.Clear();
		var msg = Message.Create(MessageSendMode.Unreliable, (ushort)  NetworkingManager.MasterServerNetworkMessageID.Refresh);
		Client?.Send(msg);
	}


	public static void Disconnect()
	{
		Client?.Disconnect();
	}

	[MessageHandler((ushort)  NetworkingManager.MasterServerNetworkMessageID.PlayerList)]
	private static void RecivePlayetList(Message message)
	{
		string list = message.GetString();
		Players = list.Split(";").ToList();
		UI.SetUI(null);
	}
		
		
	[MessageHandler((ushort)  NetworkingManager.MasterServerNetworkMessageID.LobbyCreated)]
	private static void ReciveCreatedLobby(Message message)
	{
		Console.WriteLine("Connecting to started lobbby...");
		var data = message.GetSerializable<LobbyData>();
		var result = NetworkingManager.Connect(Ipport.Split(":")[0]+":"+data.Port,Name);
		Console.WriteLine("result: "+result);
	
	}
		
	[MessageHandler((ushort)  NetworkingManager.MasterServerNetworkMessageID.LobbyList)]
	private static void RecieveLobbies(Message message)
	{
		Lobbies.Clear();
		int count = message.GetInt();
		for (int i = 0; i < count; i++)
		{
			Lobbies.Add(message.GetSerializable<LobbyData>());
		}
		UI.SetUI(new LobbyBrowserLayout());
	
	}


	public static void Update()
	{
		Client?.Update();
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using DefconNull.ReplaySequence;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;

namespace DefconNull.Networking;

public class MasterServerNetworking
{
	public static Client? Client;
	private static string Ipport="";
	private static string Name="";
	public static bool IsConnected => Client != null && Client.IsConnected;
	private static void LogNetCode(string msg)
	{
		Log.Message("RIPTIDE",msg);
	}
	public static bool Connect(string ipport,string name)
	{
		if(Client!=null && Client.IsConnected)
			Client.Disconnect();
		RiptideLogger.Initialize(LogNetCode, LogNetCode,LogNetCode,LogNetCode, false);
		Ipport = ipport;
		Name = name;


		Client = new Client( new TcpClient());
		
		
		var msg = Message.Create();
		msg.AddString(name);
		bool result = Client.Connect(ipport,message:msg,maxConnectionAttempts:3);
		if (!result)
		{
			return false;
		}

		Client.Connection.CanQualityDisconnect = false;

		Client.TimeoutTime = 10000;
		Client.HeartbeatInterval = 2000;
		
		Client.Connected += (a, b) =>
		{
			Console.WriteLine($"{Client} Connection to master server established");

			MainMenuLayout.RefreshLobbies();

		};
		Client.ConnectionFailed += (a, b) =>
		{
			//UI.ShowMessage("Error", "Could not connect to master server");
			UI.ShowMessage("Connection Failed",b.Message?.GetString() ?? string.Empty );
		};
		Client.Disconnected += (a, b) =>
		{
			
			Log.Message("MASTER SERVER","lost connection to master server");
			if(!NetworkingManager.Connected)//only change ui if we're not in a server
			{
				MainMenuLayout.RefreshLobbies();
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
			Log.Message("MASTER SERVER","Recived Message: " + ( NetworkingManager.NetworkMessageID)b.MessageId);
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
		MainMenuLayout.RefreshLobbies();
	}
		
		
	[MessageHandler((ushort)  NetworkingManager.MasterServerNetworkMessageID.LobbyCreated)]
	private static void ReciveCreatedLobby(Message message)
	{
		Log.Message("NETWORKING", "Connecting to started lobbby...");
		var data = message.GetSerializable<LobbyData>();
		for (int i = 0; i < 10; i++)
		{
			Log.Message("NETWORKING", "Connecting to started lobbby attempt "+i+"...");
			var result = NetworkingManager.Connect(Ipport.Split(":")[0]+":"+data.Port,Name);
			Log.Message("NETWORKING", "result: "+result);
			if(result)
				break;
		}
		
	
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
		MainMenuLayout.RefreshLobbies();
	
	}


	public static void Update()
	{
		Client?.Update();
	}

	public static void ChatMSG(string message)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkingManager.MasterServerNetworkMessageID.Chat);
		msg.AddString(message);
		Client?.Send(msg);
		
	}

	[MessageHandler((ushort) NetworkingManager.MasterServerNetworkMessageID.Chat)]
	private static void ReciveChat(Message message)
	{
		if (!NetworkingManager.Connected)
		{
			Chat.ReciveMessage(message.GetString());
		}
	}

}

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using DefconNull.World;
using Riptide;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;
using TcpClient = Riptide.Transports.Tcp.TcpClient;

namespace DefconNull.Networking;

public static partial class NetworkingManager
{
	private static Client? client;
	private static string Ipport="";
	private static string Name="";
	public static bool Connected => client != null && client.IsConnected;
	
	public static string GetLocalIPAddress()
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				return ip.ToString();
			}
		}
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}
	public static bool Connect(string ipport,string name)
	{
		ipport = ipport.Trim();
		ipport = ipport.Replace("localhost", GetLocalIPAddress());
		if(client!=null)
			client.Disconnect();
		
		Ipport = ipport;
		Name = name;
		
		client = new Client( new TcpClient());
		Message.MaxPayloadSize = 2048;

		client.TimeoutTime = 10000;
#if DEBUG
		client.TimeoutTime = ushort.MaxValue;
#endif

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
				UI.OptionMessage("Lost Connection: " + a, "Do you want to reconnect?", "no", (a, b) =>
				{
					Disconnect();
					if (MasterServerNetworking.IsConnected)
					{
						UI.SetUI(new LobbyBrowserLayout());
					}
					else
					{
						UI.SetUI(new MainMenuLayout());
					}
				}, "yes", (a, b) => { Reconnect(); });
				
			}
			else
			{
				UI.ShowMessage("Connection Rejected By Server", b.ToString()!);
				Disconnect();
				if (MasterServerNetworking.IsConnected)
				{
					UI.SetUI(new LobbyBrowserLayout());
				}
				else
				{
					UI.SetUI(new MainMenuLayout());
				}
			}
		};
		client.MessageReceived += (a, b) =>
		{
			Console.WriteLine("Recived Message: " + (NetMsgIds.NetworkMessageID)b.MessageId);
		};




		return true;

	}
	public static void SendStartGame(bool singleplayer = false)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.StartGame);
		msg.AddBool(singleplayer);
		client?.Send(msg);
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
	//	UI.SetUI(new MainMenuLayout());
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
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.Chat);
		msg.AddString(message);
		client?.Send(msg);
	}

	public static void EndTurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.EndTurn);
		client?.Send(msg);
	}

	public static void SendSquadComp(List<SquadMember> composition)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.SquadComp);
		msg.Add(false);
		msg.AddSerializables(composition.ToArray());
		client?.Send(msg);
	}
	public static void SendDualSquadComp(List<SquadMember> composition1,List<SquadMember> composition2)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.SquadComp);
		msg.Add(true);
		msg.AddSerializables(composition1.ToArray());
		msg.AddSerializables(composition2.ToArray());
		client?.Send(msg);
	}
		
	public static void SendPreGameUpdate()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.PreGameData);
		msg.AddSerializable(GameManager.PreGameData);
		client?.Send(msg);
	}


	public static void KickRequest()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.Kick);
		client?.Send(msg);
	}


	public static void SendGameAction(Action.GameActionPacket act)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.GameAction);
		msg.AddSerializable(act);
		client?.Send(msg);
	}


	public static void PracticeMode()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.PracticeMode);
		client?.Send(msg);
	}

	public static void AddAI()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.AddAI);
		client?.Send(msg);
	}

	public static void SendAITurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetMsgIds.NetworkMessageID.DoAI);
		client?.Send(msg);
	}
}
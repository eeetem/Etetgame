using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using DefconNull.Rendering.UILayout.GameLayout;
using Riptide;
using Riptide.Utils;
using Action = DefconNull.WorldObjects.Units.Actions.Action;
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
		Disconnect();

		if (client != null)
		{
			Thread.Sleep(1000);
			client?.Update();
			Thread.Sleep(1000);
		}

		RiptideLogger.Initialize(LogNetCode, LogNetCode,LogNetCode,LogNetCode, false);
		ipport = ipport.Trim();
		ipport = ipport.Replace("localhost", GetLocalIPAddress());
		Ipport = ipport;
		Name = name;
		
		client = new Client( new TcpClient());
		


		var msg = Message.Create();
		msg.AddString(name);
		bool result = client.Connect(ipport,message:msg);
		if (!result)
		{
			return false;
		}
	
		Log.Message("NETWORKING",$"{client} Connection established");

		Game1.config.SetValue("config", "LastServer", ipport);
		
		client.ConnectionFailed += (a, b) =>
		{
			Disconnect();
			//UI.OptionMessage("Connection Failed",b.Message?.GetString() ?? string.Empty, "OK",(sender, args) => {Disconnect();},"OK",(sender, args) => {Disconnect();});
		};
		client.Disconnected += (a, b) =>
		{
	
			UI.OptionMessage("Lost Connection: " + b.Reason, "Do you want to reconnect?", "no", (a, b) =>
			{
				Disconnect();
				GameLayout.CleanUp();

				UI.SetUI(new MainMenuLayout());
				
			}, "yes", (a, b) => { Reconnect(); });

		};
		client.MessageReceived += (a, b) =>
		{
			//Log.Message("RIPTIDE","Recived Message: " + (NetworkMessageID)b.MessageId);
		};

		client.TimeoutTime = 20000;
#if DEBUG
		client.TimeoutTime = ushort.MaxValue;
#endif
		client.Connection.MaxSendAttempts = 100;
		client.Connection.MaxAvgSendAttempts = 10;
		client.Connection.AvgSendAttemptsResilience = 25;
#if DEBUG
		client.Connection.CanQualityDisconnect = false;
#endif




		return true;

	}
	public static void StartTutorial()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.StartTutorial);
		client?.Send(msg);
	}
	public static void SendStartGame(bool singleplayer = false)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.StartGame);
		msg.AddBool(singleplayer);
		client?.Send(msg);
	}
	public static void Disconnect()
	{
		client?.Disconnect();
		WorldManager.Instance.WipeGrid();
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
		msg.Add(false);
		msg.AddSerializables(composition.ToArray());
		client?.Send(msg);
	}
	public static void SendDualSquadComp(List<SquadMember> composition1,List<SquadMember> composition2)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.SquadComp);
		msg.Add(true);
		msg.AddSerializables(composition1.ToArray());
		msg.AddSerializables(composition2.ToArray());
		client?.Send(msg);
	}

	public static void SwapMap(string name)
	{
		var data = GameManager.PreGameData;
		data.SelectedMap = name;
		GameManager.PreGameData = data;
					
		SendPreGameUpdate();
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


	public static void SendGameAction(Action.GameActionPacket act)
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.GameAction);
		msg.AddSerializable(act);
		client?.Send(msg);
	}


	public static void PracticeMode()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.PracticeMode);
		client?.Send(msg);
	}

	public static void AddAI()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.AddAI);
		client?.Send(msg);
	}

	public static void SendAITurn()
	{
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.DoAI);
		client?.Send(msg);
	}

	public static void SendSequenceExecuted()
	{
		Log.Message("NETWORKING","Sending Sequence Executed");
		var msg = Message.Create(MessageSendMode.Reliable, NetworkMessageID.SequenceFinished);
		client?.Send(msg);
	}
}
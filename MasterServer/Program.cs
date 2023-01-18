﻿using System;
using System.Diagnostics;
using System.IO;
using CommonData;
using Network;
using Network.Converter;
using Network.Enums;
using Network.Packets;


namespace MultiplayerXeno // Note: actual namespace depends on the project name.
{
	public static class Program
	{
	
		static int tickrate = 1;
		static float MSperTick = 1000 / tickrate;
		static Stopwatch stopWatch = new Stopwatch();
		public static Dictionary<string, Connection> Players = new Dictionary<string, Connection>();
		private static ServerConnectionContainer serverConnectionContainer;
		
		static void Main(string[] args)
		{

			serverConnectionContainer = ConnectionFactory.CreateServerConnectionContainer(1630, false);

			serverConnectionContainer.ConnectionLost += (Connection, b, c) =>
			{
				Console.WriteLine($"{serverConnectionContainer.Count} {b.ToString()} Connection lost {Connection.IPRemoteEndPoint.Address}. Reason {c.ToString()}");
				string disconnectedPlayer = "";
				foreach (var con in Players)
				{
					if (Connection == con.Value)
					{
						SendChatMessage(con.Key+" left the game");
						disconnectedPlayer = con.Key;
						break;
					}
				}
				if (disconnectedPlayer != "")
				{
					Players.Remove(disconnectedPlayer);
				}

			};
			serverConnectionContainer.ConnectionEstablished += ConnectionEstablished;



	
			serverConnectionContainer.Start();
			
			Console.WriteLine("Started master-server at " + serverConnectionContainer.IPAddress +":"+ serverConnectionContainer.Port);
			var packet = new LobbyStartPacket();
			packet.LobbyName = "amongus lobby";
			packet.Password = "";
			StartLobby(packet,null);
			 packet = new LobbyStartPacket();
			packet.LobbyName = "amongus 2 lobby";
			packet.Password = "";
			StartLobby(packet,null);
			
			UpdateLoop();
			
			
		}
		private static void ConnectionEstablished(Connection connection, ConnectionType type)
		{
			Console.WriteLine($"{serverConnectionContainer.Count} {connection.GetType()} connected on port {connection.IPRemoteEndPoint.Port}");
			connection.EnableLogging = true;
			connection.TIMEOUT = 10000;
			//need some sort of ddos protection
			connection.RegisterStaticPacketHandler<LobbyStartPacket>(StartLobby);
			connection.RegisterRawDataHandler("register",RegisterClient);
			connection.RegisterRawDataHandler("chatmsg", (data, Connection) =>
			{
				foreach (var con in Players)
				{
					if (Connection == con.Value)
					{
						var message = $"{con.Key}: {RawDataConverter.ToUTF8String(data)}";
						SendChatMessage(message);
						break;
					}
				}
				
			});
			connection.RegisterRawDataHandler("RequestLobbies", (a, b) =>
			{
				Console.WriteLine("RequestLobbies");
				foreach (var lobby in Lobbies)
				{
					b.Send(lobby.Value.Item2);
				}
			});

			Console.WriteLine("Registered handlers");
		}
		private static void RegisterClient(RawData rawData, Connection connection)
		{
			Console.WriteLine("Begining Client Register");
			string name = RawDataConverter.ToUTF8String(rawData);
			if (Players.ContainsKey(name))
			{
				Kick("Player with the same name already exists",connection);
				return;
			}
			Players.Add(name,connection);
			Console.WriteLine("Client Register Done");


		}
		public static void Kick(string reason,Connection connection)
		{
			Console.WriteLine("Kicking " + connection.IPRemoteEndPoint.Address + " for " + reason);
			connection.SendRawData(RawDataConverter.FromUnicodeString("notify",reason));
			Thread.Sleep(1000);
			connection.Close(CloseReason.ClientClosed);
		}
		private static void StartLobby(LobbyStartPacket lobbyStartPacket, Connection connection)
		{

				Console.WriteLine("StartLobby");
				int port = GetNextFreePort();
				Console.WriteLine("Port: " + port); //ddos or spam protection is needed
				var process = new Process();
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.RedirectStandardOutput= true;
				process.StartInfo.FileName = "./Server.exe";
				List<string> args = new List<string>();
				args.Add(port.ToString());
				args.Add( lobbyStartPacket.Password);
				process.StartInfo.Arguments = string.Join(" ", args);
				process.ErrorDataReceived += (a, b) =>
				{
					Console.WriteLine("ERROR - Server("+port+"):"+b.Data);
				};
				process.OutputDataReceived += (sender, args) =>
				{
					Console.WriteLine("Server("+port+"): "+args.Data);
				};
				process.Start();
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();
				LobbyData lobbyData = new LobbyData(lobbyStartPacket.LobbyName, 0, port);
				if(lobbyStartPacket.Password != "")
				{
					lobbyData.HasPassword = true;
				}
				Lobbies.Add(port,new Tuple<Process, LobbyData>(process,lobbyData));

			
		}

		private static  Dictionary<int, Tuple<Process,LobbyData>> Lobbies = new Dictionary<int,  Tuple<Process,LobbyData>>();

		public static void SendChatMessage(string text)
		{
			foreach (var p in Players)
			{
				p.Value.SendRawData(RawDataConverter.FromUTF8String("chatmsg",text));
			}

		}
		public static int GetNextFreePort()
		{
			int port = 1631;
			while (Lobbies.ContainsKey(port))
			{
				port++;
			}

			return port;
		}

	

		static void UpdateLoop()
		{

			while (true)
			{
				stopWatch.Restart();

				List<int> toDelete = new List<int>();
				foreach (var lobby in Lobbies)
				{
					Process p = lobby.Value.Item1;
					
					if (p.HasExited)
					{
						toDelete.Add(lobby.Key);
					}
				}

				foreach (var port in toDelete)
				{
					Lobbies.Remove(port);
				}
				toDelete.Clear();


				stopWatch.Stop();

				TimeSpan ts = stopWatch.Elapsed;
				if (ts.Milliseconds >= MSperTick)
				{
					Console.WriteLine("WARNING: SERVER CAN'T KEEP UP WITH TICK");
					Console.WriteLine(ts.Milliseconds);
				}
				else
				{
					// Console.WriteLine("update took: "+ts.Milliseconds);
					Thread.Sleep((int)(MSperTick - ts.Milliseconds));
				}
				
			}
		}
	}
};
﻿using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using MultiplayerXeno;
using Network;
using Network.Converter;
using Network.Enums;
using Network.Packets;

namespace MultiplayerXeno
{
	public static class Networking
	{
		private static ServerConnectionContainer? serverConnectionContainer;
		public static void Start(int port)
		{
		
			//1. Start listen on a portw
			serverConnectionContainer = ConnectionFactory.CreateServerConnectionContainer(port, false);
			
			serverConnectionContainer.ConnectionLost += (conn, b, cl) =>
			{
				Console.WriteLine($"{serverConnectionContainer.Count} {b.ToString()} Connection lost {conn.IPRemoteEndPoint.Address}. Reason {cl.ToString()}");
				string name;
				if (conn == GameManager.Player1?.Connection)
				{
					name = GameManager.Player1.Name;
				}else if (conn == GameManager.Player2?.Connection)
				{
					name = GameManager.Player2.Name;
				}
				else
				{
					return;
				}
				conn.KeepAlive = false;
				if (GameManager.Player1?.Connection == conn)
				{
					GameManager.Player1.Connection = null;
				}else if (GameManager.Player2?.Connection == conn)
				{
					GameManager.Player2.Connection = null;
				}

				
				SendChatMessage(name+" left the game");
				Thread.Sleep(1000);
				SendPreGameInfo();
				Thread.Sleep(10000);
				if (serverConnectionContainer.Count == 0)
				{
					Environment.Exit(0);
				}
			};
			serverConnectionContainer.ConnectionEstablished += ConnectionEstablished;
			serverConnectionContainer.AllowUDPConnections = false;


			selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Map1.mapdata";
			WorldManager.Instance.LoadMap(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Ground Zero.mapdata");
			serverConnectionContainer.Start();
			
			Console.WriteLine("Started server at " + serverConnectionContainer.IPAddress +":"+ serverConnectionContainer.Port);
		
		}

		public static void Kick(string reason,Connection connection)
		{
			Console.WriteLine("Kicking " + connection.IPRemoteEndPoint?.Address + " for " + reason);
			if (connection.IsAlive)
			{
				connection.SendRawData(RawDataConverter.FromUnicodeString("notify", reason));
			}
			connection.KeepAlive = false;
			if (GameManager.Player1?.Connection == connection)
			{
				GameManager.Player1.Connection = null;
			}else if (GameManager.Player2?.Connection == connection)
			{
				GameManager.Player2.Connection = null;
			}

			Thread.Sleep(1000);
			connection.Close(CloseReason.ClientClosed);
		}

		public static void NotifyAll(string message)
		{
		
			Notify(message,true);
			Notify(message,false);
		}

		public static void Notify(string message, bool player1)
		{
			if (player1)
			{
				GameManager.Player1?.Connection?.SendRawData(RawDataConverter.FromUnicodeString("notify",message));	
			}
			else
			{
				GameManager.Player2?.Connection?.SendRawData(RawDataConverter.FromUnicodeString("notify",message));	
			}
		}
		


		/// <summary>
		/// We got a connection.
		/// </summary>
		/// <param name="connection">The connection we got. (TCP or UDP)</param>
		private static void ConnectionEstablished(Connection connection, ConnectionType type)
		{
			Console.WriteLine($"{serverConnectionContainer!.Count} {connection.GetType()} connected on port {connection.IPRemoteEndPoint.Port}");
			connection.EnableLogging = false;
			connection.TIMEOUT = 10000;

			connection.RegisterRawDataHandler("register",RegisterClient);
			connection.RegisterRawDataHandler("chatmsg",ReciveChatMessage);
			connection.RegisterRawDataHandler("kick", (i, conn) =>
			{
				if (conn != GameManager.Player1?.Connection)return;

				if (GameManager.Player2 != null)
				{
					if (GameManager.Player2?.Connection != null) Kick("Kicked by host", GameManager.Player2.Connection);
					GameManager.Player2 = null;
				}

				SendPreGameInfo();

			});
			connection.RegisterRawDataHandler("gameState", (packet, con) =>
			{
				if(con != GameManager.Player1?.Connection || GameManager.GameState != GameState.Lobby)
					return;
				GameManager.StartSetup();
			});
			connection.RegisterRawDataHandler("mapSelect", (i, con) =>
			{
				if(con != GameManager.Player1?.Connection  || GameManager.GameState != GameState.Lobby)
					return;
				selectedMap = RawDataConverter.ToUTF8String(i);
				WorldManager.Instance.LoadMap(selectedMap);
	
				SendPreGameInfo();
			});
			
			connection.RegisterStaticPacketHandler<PreGameDataPacket>((data, con) =>
			{
				if(con != GameManager.Player1?.Connection || GameManager.GameState != GameState.Lobby)
					return;
				GameManager.PreGameData.TurnTime = data.TurnTime;
				SendPreGameInfo();
			});
			
			connection.RegisterStaticPacketHandler<MapDataPacket>((data, con) =>
			{
				if(con != GameManager.Player1?.Connection || GameManager.GameState != GameState.Lobby)
					return;
				File.Delete(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Custom/"+data.MapData.Name+".mapdata");
				File.WriteAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Custom/"+data.MapData.Name+".mapdata", data.MapData.Serialise());
				SendPreGameInfo();
			});
			
			connection.RegisterStaticPacketHandler<GameActionPacket>(ReciveAction);
			connection.RegisterStaticPacketHandler<SquadCompPacket>(ReciveSquadComp);
			Console.WriteLine("Registered handlers");

		}

		private static void ReciveSquadComp(SquadCompPacket packet, Connection connection)
		{
			if (GameManager.Player1?.Connection == connection)
			{
				GameManager.Player1.SetSquadComp(packet);
			}else if (GameManager.Player2?.Connection == connection)
			{
				GameManager.Player2.SetSquadComp(packet);
			}

			if (GameManager.Player1?.SquadComp != null && GameManager.Player2?.SquadComp != null)
			{
				GameManager.StartGame();
			}
		}


		private static void RegisterClient(RawData rawData, Connection connection)
		{
		
			string name = RawDataConverter.ToUTF8String(rawData).ToLower();
			Console.WriteLine("Begining Client Register: "+name);
			if(name.Contains('.')||name.Contains(';')||name.Contains(':')||name.Contains(',')||name.Contains('[')||name.Contains(']'))
			{
				Kick("Invalid name",connection);
				return;
			}

			if (GameManager.Player1 == null)
			{
			
				GameManager.Player1 = new Client(name,connection);
				SendChatMessage(name+" joined as Player 1");
			}
			else if (GameManager.Player1.Name == name)
			{
				if (GameManager.Player1.Connection != null && GameManager.Player1.Connection.IsAlive)
				{
					Kick("Player with same name is already in the game",connection);
					return;
				}

				GameManager.Player1.Connection = connection;//reconnection
				SendChatMessage(name+" reconnected as Player 1");
				
			}
			else if (GameManager.Player2 == null)
			{
			
				GameManager.Player2 = new Client(name,connection);
				SendChatMessage(name+" joined as Player 2");
			}
			else if (GameManager.Player2.Name == name)
			{
				if (GameManager.Player2.Connection != null && GameManager.Player2.Connection.IsAlive)
				{
					Kick("Player with same name is already in the game",connection);
					return;
				}
				GameManager.Player2.Connection = connection;//reconnection
				SendChatMessage(name+" reconnected as Player 2");
			}
			else
			{
				GameManager.Spectators.Add(new Client(name,connection));
				SendChatMessage(name+" joined the spectators");
			}

			
			GameManager.SendData();
			SendPreGameInfo();
			SendMapData(connection);

			Console.WriteLine("Client Register Done");
			


		}

		private static string selectedMap = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/map.mapdata";
		public static void SendPreGameInfo()
		{
			var data = new PreGameDataPacket();
			data.HostName = GameManager.Player1 != null ? GameManager.Player1.Name : "None";
			data.Player2Name = GameManager.Player2 != null ? GameManager.Player2.Name : "None";
			if (GameManager.Player2 != null &&( GameManager.Player2.Connection ==null || !GameManager.Player2.Connection.IsAlive))
			{
				data.Player2Name = "Reserved: " + data.Player2Name;
			}
			data.Spectators = new List<string>();
			foreach (var spectator in GameManager.Spectators)
			{
				data.Spectators.Add(spectator.Name);
			}
			data.MapList = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/", "*.mapdata").ToList();
			data.CustomMapList = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Maps/Custom", "*.mapdata").ToList();
			data.SelectedMap = selectedMap;
			data.TurnTime = GameManager.PreGameData.TurnTime;
			if (GameManager.Player1 != null)
				GameManager.Player1.Connection?.Send(data);
			if (GameManager.Player2 != null)
				GameManager.Player2.Connection?.Send(data);
			
			foreach (var spectator in GameManager.Spectators)
			{
				spectator.Connection?.Send(data);
				
			}

			GameManager.PreGameData = data;
			Program.InformMasterServer();
		}

		public static void SendTileUpdate(WorldTile wo)
		{
			WorldTileData worldTileData = wo.GetData();

		
				
			string s = Newtonsoft.Json.JsonConvert.SerializeObject(worldTileData);
			GameManager.Player1?.Connection?.SendRawData(RawDataConverter.FromUTF8String("TileUpdate", s));
			GameManager.Player2?.Connection?.SendRawData(RawDataConverter.FromUTF8String("TileUpdate", s));
			foreach (var spectator in GameManager.Spectators)
			{
				spectator.Connection?.SendRawData(RawDataConverter.FromUTF8String("TileUpdate", s));
			}
			
			
			
			

		}

//since this is just a request we dont read raw data at all
	
		

		public static void SendMapData(Connection connection)
		{
			WorldManager.Instance.SaveCurrentMapTo("temp.mapdata");//we dont actually read the file but we call this so the currentMap updates
			var packet = new MapDataPacket(WorldManager.Instance.CurrentMap);
			connection.Send(packet);

		}

		private static void ReciveAction(GameActionPacket packet, Connection connection)
		{
			Client? currentPlayer;
			if (GameManager.IsPlayer1Turn)
			{
			
				currentPlayer = GameManager.Player1;
			}
			else
			{
				currentPlayer = GameManager.Player2;
			}

			if (currentPlayer?.Connection != connection)
			{
				//out of turn action. perhaps desync or hax? kick perhaps
				
				return;
			}

			GameManager.ParseActionPacket(packet);




		}

		public static void DoAction(Packet packet)
		{
			GameManager.Player1?.Connection?.Send(packet);
			GameManager.Player2?.Connection?.Send(packet);
			foreach (var spectator in GameManager.Spectators)
			{
				spectator.Connection?.Send(packet);
			}

		}


		public static void ReciveChatMessage(RawData rawData, Connection connection)
		{
			string message = RawDataConverter.ToUTF8String(rawData);
			string name;
			if (GameManager.Player1?.Connection == connection)
			{
				name = "[Red]"+GameManager.Player1.Name+"[-]";
			}
			else if (GameManager.Player2?.Connection == connection)
			{
				name = "[Blue]"+GameManager.Player2.Name+"[-]";
			}
			else
			{
				return;
			}
			message = message.Replace("\n", "");
			message = message.Replace("[", "");
			message = message.Replace("]", "");

			message = $"{name}: {message}";
			SendChatMessage(message);
		}

		public static void SendChatMessage(string text)
		{
			text = "[Yellow]" + text + "[-]";
			GameManager.Player1?.Connection?.SendRawData(RawDataConverter.FromUTF8String("chatmsg",text));
			GameManager.Player2?.Connection?.SendRawData(RawDataConverter.FromUTF8String("chatmsg",text));
			foreach (var spectator in GameManager.Spectators)
			{
				spectator.Connection?.SendRawData(RawDataConverter.FromUTF8String("chatmsg",text));
			}
		}

	
	}
}
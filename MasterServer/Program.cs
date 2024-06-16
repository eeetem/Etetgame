using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using DefconNull;
using DefconNull.Networking;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;

namespace MasterServer; // Note: actual namespace depends on the project name.

public static class Program
{
	
	static int tickrate = 1;
	static readonly float MSperTick = 1000f / tickrate;
	static Stopwatch stopWatch = new Stopwatch();
	public static Dictionary<string, Connection> Players = new Dictionary<string, Connection>();
	private static Server server = null!;
	private static void LogNetCode(string msg)
	{
		Console.WriteLine(msg);
	}

	private static ushort startPort = 52233;
	static void Main(string[] args)
	{
		AppDomain currentDomain = default(AppDomain);
		currentDomain = AppDomain.CurrentDomain;
		// Handler for unhandled exceptions.
		currentDomain.UnhandledException += GlobalUnhandledExceptionHandler;
		RiptideLogger.Initialize(LogNetCode, LogNetCode,LogNetCode,LogNetCode, true);
		server = new Server(new TcpServer());
			

		server.ClientConnected += (a, b) => { Console.WriteLine($" {b.Client.Id} connected (Clients: {server.ClientCount}), awaiting registration...."); };
		server.HandleConnection += HandleConnection;
		server.TimeoutTime = 10000;
		server.HeartbeatInterval = (int) (MSperTick*2f);

		server.ClientDisconnected += (a, b) =>
		{
			Console.WriteLine($"Connection lost {b.Client.Id}. Reason {b.Reason}");
			string disconnectedPlayer = "";
			foreach (var con in Players)
			{
				if (b.Client == con.Value)
				{
					//SendChatMessage(con.Key+" left the game");
					disconnectedPlayer = con.Key;
					break;
				}
			}
			Console.WriteLine("Removing player: "+disconnectedPlayer);
			if (disconnectedPlayer != "")
			{
				Players.Remove(disconnectedPlayer);
			}
			SendChatMessage(disconnectedPlayer+" connected");
		};

		server.MessageReceived += (a, b) => { Console.WriteLine($"Received message from {b.FromConnection.Id}: {b.MessageId}"); };


		if(args.Length>0)
		{
			startPort = ushort.Parse(args[0]);
		}
		server.Start(startPort,100);
			
		Console.WriteLine("Started master-server");
		UpdateLoop();
			
			
	}

	private static void HandleConnection(Connection connection, Message connectmessage)
	{
		string name = connectmessage.GetString();
		Console.WriteLine("Registering client: " + name);
		if(name.Contains('.')||name.Contains(';')||name.Contains(':')||name.Contains(',')||name.Contains('[')||name.Contains(']'))
		{
			var msg = Message.Create();
			msg.AddString("Invalid Name");
			server.Reject(connection,msg);
			return;
		}
		if (Players.ContainsKey(name))
		{
			var exist = Players[name];
			if (exist.IsNotConnected)
			{
				Players.Remove(name);
			}
			else
			{
				var msg = Message.Create();
				msg.AddString("Player with the same name already exists");
				server.Reject(connection, msg);
				return;
			}
		}
		Players.Add(name,connection);

		server.Accept(connection);
		foreach (var p in Players)
		{
			SendPlayerList(p.Value);
		}
		SendChatMessage(name+" connected");
	}

	public static readonly object syncobj = new object();

	private static void SendPlayerList(Connection connection)
	{
		var msg = Message.Create(MessageSendMode.Unreliable,  NetworkingManager.MasterServerNetworkMessageID.PlayerList);
		msg.AddString(String.Join(";", Players.Keys));
		server.Send(msg,connection);
	}

	[MessageHandler((ushort)  NetworkingManager.MasterServerNetworkMessageID.Refresh)]
	private static void RefreshRequest(ushort senderID, Message message)
	{
		Console.WriteLine("RequestLobbies");
		var msg = Message.Create(MessageSendMode.Reliable,  NetworkingManager.MasterServerNetworkMessageID.LobbyList);
		msg.Add(Lobbies.Count);
		foreach (var lobby in Lobbies)
		{
			msg.Add(lobby.Value.Item2);
		}
		server.Send(msg, senderID);
		Connection? c;
		server.TryGetClient(senderID, out c);
		if(c!=null){
			SendPlayerList(c);
		}
	
	}


	[MessageHandler((ushort)  NetworkingManager.MasterServerNetworkMessageID.LobbyStart)]
	private static void StartLobby(ushort senderID,Message message)
	{
		Console.WriteLine("StartLobby Requested");
		lock (syncobj)
		{
			string name = message.GetString();
			string pass = message.GetString();


			Console.WriteLine("Starting lobby:");
			int port = GetNextFreePort();
			Console.WriteLine("Port: " + port); //ddos or spam protection is needed
			var process = new Process();
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.RedirectStandardOutput = true;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			{
				Console.WriteLine("Filename: ./Server");
				process.StartInfo.FileName = "./Server";
			}
			else
			{
				Console.WriteLine("Filename: ./Server.exe");
				process.StartInfo.FileName = "./Server.exe";
			}
				
			List<string> args = new List<string>();
			args.Add(port.ToString());
			args.Add("false");
			args.Add(pass);
			process.StartInfo.Arguments = string.Join(" ", args);

			process.Exited += (a, b) => { Console.WriteLine("Server(" + port + ") Exited"); };
			DateTime date = DateTime.Now;
			long id = date.ToFileTime();
			string path = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + "/Logs/Server"+ name +"(" + port + ")" + id + ".log";
			Console.WriteLine("Creating server log at " +path);
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + "/Logs/");
				File.Create(path).Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
			process.ErrorDataReceived += (a, b) =>
			{
				if (b.Data!= null && b.Data.ToString() != "")
				{
					

					//copy to crashes folder
					Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + "/Logs/Crashes/");
					var destination = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location) + "/Logs/Crashes/Server" + name + "(" + port + ")" + id + ".log";
					File.Delete(destination);
					File.Copy(path, destination);
					File.AppendAllText(destination, "ERROR - Server(" + port + "):" + b.Data?.ToString());
				}
			};
			process.OutputDataReceived += (sender, args) =>
			{
				if (args.Data != null && args.Data.Contains("[UPDATE]"))
				{
					Lobbies[port].Item2.GameState = ExtractBetweenTags(args.Data, "STATE");
					Lobbies[port].Item2.PlayerCount = Int32.Parse(ExtractBetweenTags(args.Data, "PLAYERCOUNT"));
					Lobbies[port].Item2.MapName = ExtractBetweenTags(args.Data, "MAP");
					Lobbies[port].Item2.Spectators =  Int32.Parse(ExtractBetweenTags(args.Data, "SPECTATORS"));
				}

				//Console.WriteLine("Server(" + port + "): " + args.Data);
				//log
				if (args.Data != null)
				{
					File.AppendAllText(path, "[" + DateTime.Now + "] " + args.Data + "\n");
				}
			};
			Console.WriteLine("starting...");
			try
			{
				process.Start();
				Console.WriteLine("process started with id: "+process.Id);
				process.BeginErrorReadLine();
				process.BeginOutputReadLine();
				LobbyData lobbyData = new LobbyData(name, port);
				if (pass != "")
				{
					lobbyData.HasPassword = true;
				}

				Lobbies.Add(port, new Tuple<Process, LobbyData>(process, lobbyData));
				Thread.Sleep(1000);
				
				var msg = Message.Create(MessageSendMode.Unreliable,  NetworkingManager.MasterServerNetworkMessageID.LobbyCreated);
				msg.Add(lobbyData);
				server.Send(msg, senderID);

			}catch(Exception e)
			{
				Console.WriteLine(e);
			}

				
				
				
		}

	}   
	[MessageHandler((ushort)NetworkingManager.MasterServerNetworkMessageID.Chat)]
	private static void ReciveChatMsg(ushort senderID,Message message)
	{
		string text = message.GetString();
		text = text.Replace("\n", "");
		text = text.Replace("[", "");
		text = text.Replace("]", "");
		if(server.TryGetClient(senderID, out var con)){
			string name = Players.First(x=>x.Value == con).Key;
			text = $"[Green]{name}[-]: {text}";
			var msg = Message.Create(MessageSendMode.Unreliable, NetworkingManager.MasterServerNetworkMessageID.Chat);
			msg.AddString(text);
			server.SendToAll(msg);
		}

	}
	private static void SendChatMessage(string message)
	{
		var msg = Message.Create(MessageSendMode.Unreliable, NetworkingManager.MasterServerNetworkMessageID.Chat);
		msg.AddString("[Yellow]"+message+"[-]");
		server.SendToAll(msg);
	}
	public static string ExtractBetweenTags(string STR , string tag)
	{       
		string FinalString;     
		int Pos1 = STR.IndexOf("["+tag+"]") + tag.Length+2;
		int Pos2 = STR.IndexOf("[/"+tag+"]");
		FinalString = STR.Substring(Pos1, Pos2 - Pos1);
		return FinalString;
	}

	private static  Dictionary<int, Tuple<Process,LobbyData>> Lobbies = new Dictionary<int,  Tuple<Process,LobbyData>>();

	public static int GetNextFreePort()
	{
		int port = startPort+1;
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
			
			server.Update();

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
	private static void GlobalUnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
	{

	    
		DateTime date = DateTime.Now;
			
		File.WriteAllText("MASTER SERVER Crash"+date.ToFileTime()+".txt", e.ExceptionObject.ToString());
	
	}
}
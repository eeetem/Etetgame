using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;

namespace MultiplayerXeno; // Note: actual namespace depends on the project name.

public static class Program
{
	
	static int tickrate = 1;
	static readonly float MSperTick = 1000f / tickrate;
	static Stopwatch stopWatch = new Stopwatch();
	public static Dictionary<string, Connection> Players = new Dictionary<string, Connection>();
	private static Server server;
		
	static void Main(string[] args)
	{
		RiptideLogger.Initialize(Console.WriteLine, Console.WriteLine,Console.WriteLine,Console.WriteLine, true);
		server = new Server(new TcpServer());
			

		server.ClientConnected += (a, b) => { Console.WriteLine($" {b.Client.Id} connected (Clients: {server.ClientCount}), awaiting registration...."); };//todo kick without registration
		server.HandleConnection += HandleConnection;


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
		};
	
		server.Start(1630,100);
			
		Console.WriteLine("Started master-server");
		UpdateLoop();
			
			
	}

	private static void HandleConnection(Connection connection, Message connectmessage)
	{
		Console.WriteLine($"{server.ClientCount} new connection");

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
			var msg = Message.Create();
			msg.AddString("Player with the same name already exists");
			server.Reject(connection,msg);
			return;
		}
		Players.Add(name,connection);

		server.Accept(connection);
		foreach (var p in Players)
		{
			SendPlayerList(p.Value);
		}
		
	}

	public static readonly object syncobj = new object();

	private static void SendPlayerList(Connection connection)
	{
		var msg = Message.Create(MessageSendMode.Unreliable,  NetMsgIds.NetworkMessageID.PlayerList);
		msg.AddString(String.Join(";", Players.Keys));
		server.Send(msg,connection);
	}

	[MessageHandler((ushort)  NetMsgIds.NetworkMessageID.Refresh)]
	private static void RefreshRequest(ushort senderID, Message message)
	{
		Console.WriteLine("RequestLobbies");
		var msg = Message.Create(MessageSendMode.Reliable,  NetMsgIds.NetworkMessageID.LobbyList);
		msg.Add(Lobbies.Count);
		foreach (var lobby in Lobbies)
		{
			msg.Add(lobby.Value.Item2);
		}
		server.Send(msg, senderID);
		Connection c = null;
		server.TryGetClient(senderID, out c);
		if(c!=null){
			SendPlayerList(c);
		}
	
	}


	[MessageHandler((ushort)  NetMsgIds.NetworkMessageID.LobbyStart)]
	private static void StartLobby(ushort senderID,Message message)
	{
		lock (syncobj)
		{
			string name = message.GetString();
			string pass = message.GetString();


			Console.WriteLine("StartLobby");
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
			args.Add(pass);
			process.StartInfo.Arguments = string.Join(" ", args);
			process.ErrorDataReceived += (a, b) => { Console.WriteLine("ERROR - Server(" + port + "):" + b.Data); };
			DateTime date = DateTime.Now;
			long id = date.ToFileTime();
			Console.WriteLine("Creating server log at " + Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Logs/Server(" + port+name+")" +id+ ".log");
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "/Logs/");
				File.Create(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Logs/Server(" + port+name+")" +id+ ".log").Close();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
				
			process.OutputDataReceived += (sender, args) =>
			{
				if (args.Data != null && args.Data.Contains("[UPDATE]"))
				{
					Lobbies[port].Item2.GameState = ExtractBetweenTags(args.Data, "STATE");
					Lobbies[port].Item2.PlayerCount = Int32.Parse(ExtractBetweenTags(args.Data, "PLAYERCOUNT"));
					Lobbies[port].Item2.MapName = ExtractBetweenTags(args.Data, "MAP");
					Lobbies[port].Item2.Spectators =  Int32.Parse(ExtractBetweenTags(args.Data, "SPECTATORS"));
				}

				Console.WriteLine("Server(" + port + "): " + args.Data);
				//log
				if (args.Data != null)
				{
					File.AppendAllText(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)+"/Logs/Server(" + port+name+")" +id+ ".log", "[" + DateTime.Now + "] " + args.Data + "\n");
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
				
				var msg = Message.Create(MessageSendMode.Unreliable,  NetMsgIds.NetworkMessageID.LobbyCreated);
				msg.Add(lobbyData);
				server.Send(msg, senderID);

			}catch(Exception e)
			{
				Console.WriteLine(e);
			}

				
				
				
		}

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
}
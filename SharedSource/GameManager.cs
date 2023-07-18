using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DefconNull.Networking;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Riptide;
#if  CLIENT
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull;

public static partial class GameManager
{
	public static bool IsPlayer1Turn = true;

	public static GameState GameState;
	public static float TimeTillNextTurn;
		
	private static bool endTurnNextFrame;
	private static bool playedWarning;
	public static void Update(float delta)
	{
		if (endTurnNextFrame)
		{
			NextTurn();
			return;
		}
			

		if (PreGameData.TurnTime != 0 && GameState == GameState.Playing)
		{
			TimeTillNextTurn -= delta;
#if CLIENT
			if(TimeTillNextTurn/1000f < PreGameData.TurnTime*0.15f && !playedWarning && IsMyTurn())
			{
				playedWarning = true;
				Audio.PlaySound("UI/alert");
			}
#endif
				
#if SERVER
				if (TimeTillNextTurn < 0)
				{
					NextTurn();
				}
#endif
		}
			
	}

	public static List<WorldObject> CapturePoints = new List<WorldObject>();
	public static readonly List<Vector2Int> T1SpawnPoints = new();
	public static readonly List<Vector2Int> T2SpawnPoints = new();
	public static int score;
	public static void Forget(WorldObject wo)
	{
		T1SpawnPoints.Remove(wo.TileLocation.Position);
		T2SpawnPoints.Remove(wo.TileLocation.Position);
		CapturePoints.Remove(wo);
#if SERVER
			if(T1Units.Contains(wo.ID)){
				T1Units.Remove(wo.ID);
			}
			if(T2Units.Contains(wo.ID)){
				T2Units.Remove(wo.ID);
			}
#endif
	}

	private static void EndGame(bool player1Win)
	{
#if SERVER
			GameState = GameState.Over;
			if (player1Win)
			{
				NetworkingManager.NotifyAll(Player1!.Name + " Wins!");	
			}
			else
			{
				NetworkingManager.NotifyAll(Player2!.Name + " Wins!");	
			}


#endif
	}

	private static void NextTurn()
	{
		playedWarning = false;
		endTurnNextFrame = false;
		IsPlayer1Turn = !IsPlayer1Turn;
		WorldManager.Instance.NextTurn(IsPlayer1Turn);
		bool team1Present = false;
		bool team2Present = false;
		Vector2Int capPoint = Vector2.Zero;
		foreach (var point in CapturePoints)
		{
			bool? team1 = point.TileLocation.UnitAtLocation?.IsPlayerOneTeam;
			if (team1 == null) continue;

			if ((bool) team1)
			{
				team1Present = true;
				capPoint = point.TileLocation.Position;
			}
			else
			{
				team2Present = true;
				capPoint = point.TileLocation.Position;
			}

		}

		if (team1Present && !team2Present)
		{
#if CLIENT
			Audio.PlaySound("capture");
			Thread.Sleep(2000);
			Camera.SetPos(capPoint);
#endif
			score++;
		}
		else if(!team1Present && team2Present){
				
#if CLIENT
			Audio.PlaySound("capture");
			Thread.Sleep(2000);
			Camera.SetPos(capPoint);
#endif
			score--;
		}
#if CLIENT
		GameLayout.SetScore(score);
		Audio.PlaySound("UI/turn");
#endif
			
		if(score > 8)EndGame(true);
		if(score < -8)EndGame(false);

		




#if SERVER
			Console.WriteLine("turn: "+IsPlayer1Turn);
			NetworkingManager.SendEndTurn();
#endif

		TimeTillNextTurn = PreGameData.TurnTime*1000;
	}

	
	public static void SetEndTurn()
	{
		endTurnNextFrame = true;
	}

	public static GameStateData GetState()
	{
		GameStateData data = new GameStateData();
		data.IsPlayer1Turn = IsPlayer1Turn;
		data.Score = score;
		data.GameState = GameState;
		return data;
	}

	public struct GameStateData : IMessageSerializable
	{
		public bool? IsPlayerOne { get; set; }
		public bool IsPlayer1Turn{ get; set; }

		public int Score { get; set; }
		public GameState GameState { get; set; }


		public void Serialize(Message message)
		{
			message.Add(IsPlayer1Turn);
			message.Add(Score);
			message.Add((int)GameState);
			message.Add(IsPlayerOne == null);
			if (IsPlayerOne != null) message.Add(IsPlayerOne.Value);
		}

		public void Deserialize(Message message)
		{
			IsPlayer1Turn = message.GetBool();
			Score = message.GetInt();
			GameState = (GameState)message.GetInt();
			bool isNull = message.GetBool();
			if (!isNull) IsPlayerOne = message.GetBool();
		}
	}
	public struct PreGameDataStruct : IMessageSerializable
	{
		public PreGameDataStruct()
		{
		}


		public string HostName { get; set; } = "";
		public string Player2Name { get; set; } = "";
		public List<string> MapList { get; set; } = new List<string>();
		public List<string> CustomMapList { get; set; } = new List<string>();

		public List<string> Spectators { get; set; } = new List<string>();
		public string SelectedMap { get; set; } = "";
		public int TurnTime { get; set; } = 180;
		
		public bool SinglePlayerLobby { get; set; } = false;

		public void Serialize(Message message)
		{
			message.Add(HostName);
			message.Add(Player2Name);


			message.AddStrings(MapList.ToArray(), true);
			message.AddStrings(CustomMapList.ToArray(), true);
			message.AddStrings(Spectators.ToArray(), true);

			message.Add(SelectedMap);
			message.Add(TurnTime);
			message.Add(SinglePlayerLobby);
		}

		public void Deserialize(Message message)
		{
			HostName = message.GetString();
			Player2Name = message.GetString();

			MapList = message.GetStrings().ToList();
			CustomMapList = message.GetStrings().ToList();
			Spectators = message.GetStrings().ToList();

			SelectedMap = message.GetString();
			TurnTime = message.GetInt();
			SinglePlayerLobby = message.GetBool();
		}

	}
		
}
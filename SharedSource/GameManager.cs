using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DefconNull.Networking;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Riptide;
#if  CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
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

		if (wo.Type.Surface&&T1SpawnPoints.Contains(wo.TileLocation.Position))
		{
			Console.WriteLine("removing spawn point FOR T1");
			T1SpawnPoints.Remove(wo.TileLocation.Position);
		}

		if(wo.Type.Surface&&T2SpawnPoints.Contains(wo.TileLocation.Position))
		{
			Console.WriteLine("removing spawn point FOR T2");
			T2SpawnPoints.Remove(wo.TileLocation.Position);
		}

		CapturePoints.Remove(wo);

		if(T1Units.Contains(wo.ID)){
			T1Units.Remove(wo.ID);
		}
		if(T2Units.Contains(wo.ID)){
			T2Units.Remove(wo.ID);
		}

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

	public static void NextTurn()
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
			bool? team1 = point.TileLocation.UnitAtLocation?.IsPlayer1Team;
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

		if (Player2.IsAI)// && !IsPlayer1Turn) 
		{
			FinishTurnWithAI();
		}

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

		public enum LobbyMode
		{
			Multiplayer,
			Practice,
			Aiskirmish
		}
		public string HostName { get; set; } = "";
		public string Player2Name { get; set; } = "";
		public List<string> MapList { get; set; } = new List<string>();
		public List<string> CustomMapList { get; set; } = new List<string>();
		public List<string> Spectators { get; set; } = new List<string>();
		public string SelectedMap { get; set; } = "";
		public int TurnTime { get; set; } = 180;
		public bool SinglePLayerFeatures { get; set; } = false;
		public LobbyMode Mode { get; set; } = LobbyMode.Multiplayer;

		public void Serialize(Message message)
		{
			message.Add(HostName);
			message.Add(Player2Name);


			message.AddStrings(MapList.ToArray(), true);
			message.AddStrings(CustomMapList.ToArray(), true);
			message.AddStrings(Spectators.ToArray(), true);

			message.Add(SelectedMap);
			message.Add(TurnTime);
			message.Add(SinglePLayerFeatures);
			message.Add((int)Mode);
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
			SinglePLayerFeatures = message.GetBool();
			Mode = (LobbyMode)message.GetInt();
		}

	}

	public static readonly List<int> T1Units = new();
	public static readonly List<int> T2Units = new();
#if CLIENT
	public static List<Unit> GetMyTeamUnits(int dimension = -1)
	{
		return GetTeamUnits(IsPlayer1,dimension);
	}
	public static List<Unit> GetEnemyTeamUnits(int dimension = -1)
	{
		return GetTeamUnits(!IsPlayer1,dimension);
	}

#endif
  
	public static List<Unit> GetTeamUnits(bool team1, int dimension = -1)
	{
		List<int>ids = new List<int>();

		ids = team1 ? T1Units : T2Units;

		List<Unit> units = new List<Unit>();
		
		foreach (var id in ids)
		{
			var obj = PseudoWorldManager.GetObject(id, dimension);
			if(obj == null || obj.UnitComponent == null)
				throw new Exception("unit is null for a registered unit");
			units.Add(obj.UnitComponent!);
		}

		return units;

		


	}

	public static List<Unit> GetAllUnits(int dimension = -1)
	{
		var list = GetTeamUnits(true,dimension);
		list.AddRange(GetTeamUnits(false,dimension));
		return list;
	}
}
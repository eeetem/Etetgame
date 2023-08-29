using System.Collections.Concurrent;
using DefconNull.Networking;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.Actions;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;


namespace DefconNull;

public static partial class GameManager
{
	public static ClientInstance? Player1;
	public static ClientInstance? Player2;
	public static List<ClientInstance> Spectators = new();
	public static PreGameDataStruct PreGameData = new();



	public static readonly List<int> T1Units = new();
	public static readonly List<int> T2Units = new();

	public static void StartSetup()
	{
		if (GameState != GameState.Lobby) return;
		if (Player1 == null || Player2 == null) return;

		GameState = GameState.Setup;


		NetworkingManager.SendGameData();
	}


	public static void StartGame()
	{
		if (GameState != GameState.Setup)
		{
			return;
		}

	


		//not a fan of this, should probably be made a single function
		

		int i = 0;

		foreach (var spawn in Player1!.SquadComp!)
		{
			Unit.UnitData cdata = new Unit.UnitData(true);
			if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
			if (T1SpawnPoints.Contains(spawn.Position))
			{
				int id = WorldManager.Instance.GetNextId();
				WorldManager.Instance.MakeWorldObject(spawn.Prefab, spawn.Position, Direction.North, id, unitData: cdata);
				T1Units.Add(id);
				i++;

			}
		}

		var newComcp = new List<SquadMember>();
		if (Player2!.IsAI)
		{
			for (int j = 0; j < WorldManager.Instance.CurrentMap.unitCount; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = "Grunt";
				Vector2Int pos = new Vector2Int(0, 0);
				do
				{
					pos = T2SpawnPoints[Random.Shared.Next(T2SpawnPoints.Count)];
				}while(newComcp.Find((s) => s.Position == pos) != null);
				sq.Position = pos;
				newComcp.Add(sq);
			}
			Player2.SetSquadComp(newComcp);
		}

		i = 0;
		foreach (var spawn in Player2!.SquadComp!)
		{
			Unit.UnitData cdata = new Unit.UnitData(false);
			if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
			if (T2SpawnPoints.Contains(spawn.Position))
			{
				int id = WorldManager.Instance.GetNextId();
				WorldManager.Instance.MakeWorldObject(spawn.Prefab, spawn.Position, Direction.North, id, unitData: cdata);
				T2Units.Add(id);
				i++;

			}

		}


		var t = new Task(delegate
		{

			GameState = GameState.Playing;
		
		
			var rng = Random.Shared.Next(100);
			NextTurn(true);
			Console.WriteLine("turn rng: "+rng);
			if (rng > 50)
			{
		//		NextTurn();
			}

			Thread.Sleep(1000); //just in case
			NetworkingManager.SendGameData();


		});
		WorldManager.Instance.RunNextAfterFrames(t,5);//let units be created and sent before we swtich to playing



	}
	public static void FinishTurnWithAI()
	{
		if (IsPlayer1Turn)
		{
			List<Unit> units = new List<Unit>();
			foreach (var u in T1Units)
			{
				units.Add(WorldManager.Instance.GetObject(u)!.UnitComponent ?? throw new Exception("team unit ids are not actual units"));
			}
			AI.AI.DoAITurn(units);
		}
		else
		{
			List<Unit> units = new List<Unit>();
			foreach (var u in T2Units)
			{
				units.Add(WorldManager.Instance.GetObject(u)!.UnitComponent ?? throw new Exception("team unit ids are not actual units"));
			}
			AI.AI.DoAITurn(units);
		}
	}
	
}
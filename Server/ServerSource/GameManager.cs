using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;


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
				var objMake = WorldObjectManager.MakeWorldObject.Make(spawn.Prefab, spawn.Position, Direction.North, cdata);
				SequenceManager.AddSequence(objMake);
				NetworkingManager.SendSequence(objMake,true);
				T1Units.Add(objMake.data.ID);
				i++;

			}
		}

		var newComcp = new List<SquadMember>();
		if (Player2!.IsAI)
		{
			for (int j = 0; j < PrefabManager.UnitPrefabs.Keys.Count; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = PrefabManager.UnitPrefabs.Keys.ToList()[j];
				Vector2Int pos = new Vector2Int(0, 0);
				do
				{
					pos = T2SpawnPoints[Random.Shared.Next(T2SpawnPoints.Count)];
				}while(newComcp.Find((s) => s.Position == pos) != null);
				sq.Position = pos;
				newComcp.Add(sq);
			}
			for (int j = 0; j < WorldManager.Instance.CurrentMap.unitCount-PrefabManager.UnitPrefabs.Keys.Count; j++)
			{
				var sq = new SquadMember();
				sq.Prefab = PrefabManager.UnitPrefabs.Keys.ToList()[Random.Shared.Next(PrefabManager.UnitPrefabs.Count)];
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
				var objMake = WorldObjectManager.MakeWorldObject.Make(spawn.Prefab, spawn.Position, Direction.North, cdata);
				SequenceManager.AddSequence(objMake);
				NetworkingManager.SendSequence(objMake,true);
				T1Units.Add(objMake.data.ID);
				i++;

			}

		}


		var t = new Task(delegate
		{

			GameState = GameState.Playing;
		
		
			var rng = Random.Shared.Next(100);
			NextTurn();
			Console.WriteLine("turn rng: "+rng);
			if (rng > 50 || Player2!.IsAI)
			{
				NextTurn();
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
				units.Add(WorldObjectManager.GetObject(u)!.UnitComponent ?? throw new Exception("team unit ids are not actual units"));
			}
			AI.AI.DoAITurn(units);
		}
		else
		{
			List<Unit> units = new List<Unit>();
			foreach (var u in T2Units)
			{
				units.Add(WorldObjectManager.GetObject(u)!.UnitComponent ?? throw new Exception("team unit ids are not actual units"));
			}
			AI.AI.DoAITurn(units);
		}
	}
	
}
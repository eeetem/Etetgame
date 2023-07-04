namespace MultiplayerXeno;

public static partial class GameManager
{
	public static Client? Player1;
	public static Client? Player2;
	public static List<Client> Spectators = new();
	public static PreGameDataStruct PreGameData = new();



	public static readonly List<int> T1Units = new();
	public static readonly List<int> T2Units = new();

	public static void StartSetup()
	{
		if (GameState != GameState.Lobby) return;
		if (Player1 == null || Player2 == null) return;

		GameState = GameState.Setup;


		Networking.SendGameData();
	}


	public static void StartGame()
	{
		if (GameState != GameState.Setup)
		{
			return;
		}

		GameState = GameState.Playing;


		//not a fan of this, should probably be made a single function
		Unit.UnitData cdata = new Unit.UnitData(true);

		int i = 0;

		foreach (var spawn in Player1!.SquadComp!)
		{
			if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
			if (T1SpawnPoints.Contains(spawn.Position))
			{
				int id = WorldManager.Instance.GetNextId();
				cdata.Inventory = spawn.Inventory;
				WorldManager.Instance.MakeWorldObject(spawn.Prefab, spawn.Position, Direction.North, id, unitData: cdata);
				T1Units.Add(id);
				i++;

			}
		}

		cdata = new Unit.UnitData(false);
		i = 0;
		foreach (var spawn in Player2!.SquadComp!)
		{
			if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
			if (T2SpawnPoints.Contains(spawn.Position))
			{
				int id = WorldManager.Instance.GetNextId();
				cdata.Inventory = spawn.Inventory;
				WorldManager.Instance.MakeWorldObject(spawn.Prefab, spawn.Position, Direction.North, id, unitData: cdata);
				T2Units.Add(id);
				i++;

			}

		}




		Thread.Sleep(1000); //let the clients process spawns
		Networking.SendGameData();
		Thread.Sleep(1000); //let the clients process UI

		var rng = Random.Shared.Next(100);
		NextTurn();
		Console.WriteLine("turn rng: "+rng);
		if (rng > 50)
		{
			
			NextTurn();
		}

		Thread.Sleep(2000); //just in case
		Networking.SendGameData();

	}


	
}
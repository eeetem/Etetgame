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
				foreach (var c in spawn.Inventory)
				{
					if(PrefabManager.UseItems.ContainsKey(c) && PrefabManager.UseItems[c].allowedUnits.Count > 0)
					{
						if(!PrefabManager.UseItems[c].allowedUnits.Contains(spawn.Prefab)) continue;
					}
					cdata.Inventory.Add(c);
				}
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
				var l = new List<string>();
				l.Add("Flash");
				sq.Inventory = l;
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
				cdata.Inventory = new List<string>();
				foreach (var c in spawn.Inventory)
				{
					if(PrefabManager.UseItems.ContainsKey(c) && PrefabManager.UseItems[c].allowedUnits.Count > 0)
					{
						if(!PrefabManager.UseItems[c].allowedUnits.Contains(spawn.Prefab)) continue;
					}
					cdata.Inventory.Add(c);
				}
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
				NextTurn();
			}

			Thread.Sleep(1000); //just in case
			NetworkingManager.SendGameData();


		});
		WorldManager.Instance.RunNextAfterFrames(t,5);//let units be created and sent before we swtich to playing



	}
	private static void StartAITurn()
	{
		var t = new Task(delegate
		{
			Task.Run(() =>
			{
				foreach (var u in T2Units)
				{
					var unit = WorldManager.Instance.GetObject(u)!.UnitComponent;
					Console.WriteLine("---------AI acting with unit: "+unit!.WorldObject.TileLocation.Position);
					while (unit!.MovePoints > 0)
					{
						Console.WriteLine("moving("+unit.MovePoints.Current+")");
						var allLocations = unit.GetPossibleMoveLocations();
						ConcurrentBag<Tuple<Vector2Int, int>> scoredLocations = new ConcurrentBag<Tuple<Vector2Int, int>>();
						if (allLocations.Length == 0)
						{
							Console.WriteLine("unable to move, cancleing");
							break;
						}

						Parallel.ForEach(allLocations[0], l =>
						{
							int score = WorldManager.Instance.GetTileMovementScore(l, unit);
							scoredLocations.Add(new Tuple<Vector2Int, int>(l,score));
						});
						int score = WorldManager.Instance.GetTileMovementScore(unit.WorldObject.TileLocation.Position, unit);
						scoredLocations.Add(new Tuple<Vector2Int, int>(unit.WorldObject.TileLocation.Position,score));

						int bestOf = Math.Min(scoredLocations.Count, 1);
						
						
						var result = scoredLocations
							.OrderByDescending(x => x.Item2)
							.Take(bestOf)
							.ToArray();
						//pick random location out of top 3
						int r = Random.Shared.Next(bestOf);
						Vector2Int target = result[r].Item1;

						if (target == unit.WorldObject.TileLocation.Position)
						{
							Console.WriteLine("AI decided to stay put");
							break;
						}
						

						Console.WriteLine("ordering move action from: "+unit.WorldObject.TileLocation.Position+" to: "+target+" with score: "+result[r].Item2);
						unit.DoAction(Action.Actions[Action.ActionType.Move], target);
						Console.WriteLine(" waiting for sequence to clear.....");
						do
						{
							Thread.Sleep(1000);
						} while (WorldManager.Instance.SequenceRunning);

					}
					Console.WriteLine("---------AI DONE ---- acting with unit: "+unit!.WorldObject.TileLocation.Position);
				}
				Console.WriteLine("AI turn over, ending turn"); 
				NextTurn();
			});
		});
		WorldManager.Instance.RunNextAfterFrames(t,2);

	}


	
}
using System.Diagnostics;
using MultiplayerXeno;
using MultiplayerXeno.Items;

namespace MultiplayerXeno
{
	public static partial class GameManager
	{
		public static Client? Player1;
		public static Client? Player2;
		public static List<Client> Spectators = new();
		public static PreGameDataPacket PreGameData = new();



		public static readonly List<int> T1Units = new();
		public static readonly List<int> T2Units = new();

		public static void StartSetup()
		{
			if (GameState != GameState.Lobby) return;
			if(Player1==null || Player2==null)return;

			GameState = GameState.Setup;

			if (Player1.Connection != null) Networking.SendMapData(Player1.Connection);
			if (Player2.Connection != null) Networking.SendMapData(Player2.Connection);

			SendData();
		}
	

		public static void StartGame()
		{
			if (GameState != GameState.Setup)
			{
				return;
			}

			GameState = GameState.Playing;
			
				
			//not a fan of this, should probably be made a single function
			UnitData cdata = new UnitData(true);
			
			int i = 0;

			foreach (var spawn in Player1!.SquadComp!.Composition!)
			{
				if (i >= WorldManager.Instance.CurrentMap.unitCount) break;
				if (T1SpawnPoints.Contains(spawn.Position))
				{
					int id = WorldManager.Instance.GetNextId();
					cdata.Inventory = spawn.Inventory;
					WorldManager.Instance.MakeWorldObject(spawn.Prefab, spawn.Position, Direction.North, id, controllableData: cdata);
					T1Units.Add(id);
					i++;
					
				}
			}

			cdata = new UnitData(false);
			i = 0;
			foreach (var spawn in Player2!.SquadComp!.Composition!)
			{
				if(i>=WorldManager.Instance.CurrentMap.unitCount) break;
				if (T2SpawnPoints.Contains(spawn.Position))
				{
					int id = WorldManager.Instance.GetNextId();
					cdata.Inventory = spawn.Inventory;
					WorldManager.Instance.MakeWorldObject(spawn.Prefab, spawn.Position,Direction.North, id, controllableData: cdata);
					T2Units.Add(id);
					i++;
					
				}

			}



		
			Thread.Sleep(1000);//let the clients process spawns
			SendData();
			Thread.Sleep(1000);//let the clients process UI
			if (Random.Shared.Next(100) > 50)
			{
				NextTurn();
			}
			
			Thread.Sleep(2000);//just in case
			SendData();
			
		}

		public static void SendData()
		{
			GameDataPacket packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = true,
				Score = score,
				GameState = GameState
			};
			

			//packet.
			Player1?.Connection?.Send(packet);
			
			packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = false,
				Score = score,
				GameState = GameState
			};
			Player2?.Connection?.Send(packet);
			packet = new GameDataPacket
			{
				IsPlayer1Turn = IsPlayer1Turn,
				IsPlayerOne = null,
				Score = score,
				GameState = GameState
			};
			foreach (var spectator in Spectators)
			{
				spectator?.Connection?.Send(packet);
			}
			Program.InformMasterServer();
		}
		
		
		public static void ParseActionPacket(GameActionPacket packet)
		{
			try
			{
				Console.WriteLine("recived action packet: " + packet.Type + " " + packet.UnitId + " " + packet.Target);
				if (packet.Type == ActionType.EndTurn)
				{
					endTurnNextFrame = true;
					return;
				}

				if (WorldManager.Instance.GetObject(packet.UnitId) == null)
				{
					Console.WriteLine("Recived packet for a non existant object: " + packet.UnitId);
					return;
				}

				Unit? controllable = WorldManager.Instance.GetObject(packet.UnitId)?.UnitComponent;
				if(controllable == null)
				{
					Console.WriteLine("Recived packet for a non controllable object: " + packet.UnitId);
					return;
				}
				Action act = Action.Actions[packet.Type]; //else get controllable specific actions
				if (act.ActionType == ActionType.UseAbility)
				{
					int ability = int.Parse(packet.args[0]);
					UseExtraAbility.AbilityIndex = ability;
					UseExtraAbility.abilityLock = true;
					IExtraAction a = controllable.extraActions[ability];
					if (a.WorldAction.DeliveryMethods.Find(x => x is Shootable) != null)
					{
						string target = packet.args[1];
						switch (target)
						{
							case "Auto":
								Shootable.targeting = Shootable.targeting = TargetingType.Auto;
								break;
							case "High":
								Shootable.targeting = Shootable.targeting = TargetingType.High;
								break;
							case "Low":
								Shootable.targeting = Shootable.targeting = TargetingType.Low;
								break;
							default:
								throw new ArgumentException("Invalid target type");
						}
					}
				}
				else if (act.ActionType == ActionType.Attack)
				{
					string target = packet.args[0];
					switch (target)
					{
						case "Auto":
							Shootable.targeting = Shootable.targeting = TargetingType.Auto;
							break;
						case "High":
							Shootable.targeting = Shootable.targeting = TargetingType.High;
							break;
						case "Low":
							Shootable.targeting = Shootable.targeting = TargetingType.Low;
							break;
						default:
							throw new ArgumentException("Invalid target type");
					}
				}

				act.PerformServerSide(controllable, packet.Target);
				UseExtraAbility.abilityLock = false;
			}catch(Exception e)
			{
				Console.WriteLine("Error parsing packet: "+e);
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Threading;
using MultiplayerXeno;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Items;

#if  CLIENT
using MultiplayerXeno.UILayouts;//probably seperate this into a clientsidegamemanager
#endif

namespace MultiplayerXeno
{
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
		private static int score;
		public static void Forget(WorldObject wo)
		{
			T1SpawnPoints.Remove(wo.TileLocation.Position);
			T2SpawnPoints.Remove(wo.TileLocation.Position);
			CapturePoints.Remove(wo);
		}

		private static void EndGame(bool player1Win)
		{
#if SERVER
			GameState = GameState.Over;
			if (player1Win)
			{
				Networking.NotifyAll(Player1!.Name + " Wins!");	
			}
			else
			{
				Networking.NotifyAll(Player2!.Name + " Wins!");	
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
			Networking.DoAction(new GameActionPacket(-1,null,ActionType.EndTurn));//defaults to end turn
#endif

			TimeTillNextTurn = PreGameData.TurnTime*1000;
		}

	

		public static void ParsePacket(GameActionPacket packet)
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

				act.PerformFromPacket(controllable, packet.Target);
				UseExtraAbility.abilityLock = false;
			}catch(Exception e)
			{
				Console.WriteLine("Error parsing packet: "+e);
			}
		}


		
	}
	
}
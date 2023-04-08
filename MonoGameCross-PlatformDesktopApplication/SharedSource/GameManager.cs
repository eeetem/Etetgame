using System;
using System.Collections.Generic;
using System.Threading;
using CommonData;
using Microsoft.Xna.Framework;

#if  CLIENT
using MultiplayerXeno.UILayouts;//probably seperate this into a clientsidegamemanager
#endif

namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1Turn = true;

		public static GameState GameState;
		public static float TimeTillNextTurn = 0;
		public static void Update(float delta)
		{
			if (PreGameData.TurnTime != 0)
			{
				TimeTillNextTurn -= delta;
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
		private static int score = 0;
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
				Networking.NotifyAll(Player1.Name + " Wins!");	
			}
			else
			{
				Networking.NotifyAll(Player2.Name + " Wins!");	
			}


#endif
		}

		public static void NextTurn()
		{
			IsPlayer1Turn = !IsPlayer1Turn;
			WorldManager.Instance.NextTurn(IsPlayer1Turn);
			bool team1Present = false;
			bool team2Present = false;
			Vector2Int capPoint = Vector2.Zero;
			foreach (var point in CapturePoints)
			{
				bool? team1 = point.TileLocation.ObjectAtLocation?.ControllableComponent?.IsPlayerOneTeam;
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
			Audio.PlaySound("turn");
#endif
			
			if(score > 6)EndGame(true);
			if(score < -6)EndGame(false);


	
			






#if SERVER
			Console.WriteLine("turn: "+IsPlayer1Turn);
			Networking.DoAction(new GameActionPacket(-1,null,ActionType.EndTurn));//defaults to end turn
#else
			GameLayout.SetMyTurn(IsMyTurn());
#endif


			TimeTillNextTurn = PreGameData.TurnTime*1000;
		}

	

		public static void ParsePacket(GameActionPacket packet)
		{

			if (packet.Type == ActionType.EndTurn)
			{
				NextTurn();
				return;
			}

			if (WorldManager.Instance.GetObject(packet.ID) == null)
			{
				Console.WriteLine("Recived packet for a non existant object: "+packet.ID);
				return;
			}
			Controllable controllable = WorldManager.Instance.GetObject(packet.ID).ControllableComponent;
			Action act = Action.Actions[packet.Type];//else get controllable specific actions
			if (act.ActionType == ActionType.Attack)
			{
				string target = packet.args[0];
				switch (target)
				{
					case "Auto":
						Attack.targeting = Attack.targeting = TargetingType.Auto;
						break;
					case "High":
						Attack.targeting = Attack.targeting = TargetingType.High;
						break;
					case "Low":
						Attack.targeting = Attack.targeting = TargetingType.Low;
						break; 
					default:
						throw new ArgumentException("Invalid target type");
				}
			}
			act.PerformFromPacket(controllable, packet.Target);
		
		}


		
	}
	
}
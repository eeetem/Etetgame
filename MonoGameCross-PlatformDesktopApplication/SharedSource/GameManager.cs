using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Network.Packets;

namespace MultiplayerXeno
{
	public static partial class GameManager
	{

		public static bool IsPlayer1Turn = true;

		public static bool GameStarted = false;
		public static void Update(float delta)
		{

		}

		public static List<WorldObject> CapturePoints = new List<WorldObject>();
		private static int score = 0;


		private static void EndGame(bool player1Win)
		{
#if SERVER
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
			WorldManager.Instance.ResetControllables(IsPlayer1Turn);
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
			UI.SetScore(score);
			Audio.PlaySound("turn");
#endif
			
			if(score > 6)EndGame(true);
			if(score < -6)EndGame(false);
			






#if SERVER
			Console.WriteLine("turn: "+IsPlayer1Turn);
			Networking.DoAction(new GameActionPacket(-1,null,ActionType.EndTurn));//defaults to end turn
			#else
			UI.SetMyTurn(IsMyTurn());
			#endif
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
			act.PerformFromPacket(controllable, packet.Target);
		
		}



	}
	
}
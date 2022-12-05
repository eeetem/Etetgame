using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
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
			foreach (var point in CapturePoints)
			{
				bool? team1 = point.TileLocation.ObjectAtLocation?.ControllableComponent?.IsPlayerOneTeam;
				if (team1 == null) continue;

				if ((bool) team1)
				{
					team1Present = true;
				}
				else
				{
					team2Present = true;
				}

			}

			if (team1Present && !team2Present)
			{
				score++;
			}
			else if(!team1Present && team2Present){
				score--;
			}
#if CLIENT
			UI.SetScore(score);
			#endif
			
			if(score > 5)EndGame(true);
			if(score < -5)EndGame(false);
			






#if SERVER
			Console.WriteLine("turn: "+IsPlayer1Turn);
			Networking.DoAction(new GameActionPacket());//defaults to end turn
			#else
			UI.SetMyTurn(IsMyTurn());
			#endif
		}

		public static void ParsePacket(GameActionPacket packet)
		{
			if (packet.Type != ActionType.EndTurn && WorldManager.Instance.GetObject(packet.ID) == null)
			{
				Console.WriteLine("Recived packet for a non existant object: "+packet.ID);
				return;
			}
			Controllable controllable = WorldManager.Instance.GetObject(packet.ID).ControllableComponent;
			if (packet.Type == ActionType.Move)
			{
				MovementPacket movementPacket = (MovementPacket)packet;
#if CLIENT
				controllable.DoMove(movementPacket.Path,movementPacket.MovePointsUsed);
				#else
				controllable.MoveAction(movementPacket.Path.Last());
				#endif
				//if client we just initiate the move and trust the server since that's the move from another player
				//if server we verify the move and then tell both clients to move
			}
			else if (packet.Type == ActionType.Turn)
			{
				FacePacket facePacket = (FacePacket) packet;
			
#if CLIENT
				controllable.DoFace(facePacket.Dir);
#else
				controllable.FaceAction(controllable.worldObject.TileLocation.Position + Utility.DirToVec2(facePacket.Dir));
#endif
			}
			else if (packet.Type == ActionType.Attack)
			{
				FirePacket firePacket = (FirePacket) packet;
#if CLIENT
				controllable.DoFire(firePacket.Target);
#else
				//Console.WriteLine("warning: ClientSide Fire(obsolete)");
				controllable.FireAction(firePacket.Target);
#endif
			}
			else if (packet.Type == ActionType.EndTurn)
			{
				NextTurn();
				
			}else if (packet.Type == ActionType.Crouch)
			{
#if CLIENT
				controllable.DoCrouch();
#else
				controllable.CrouchAction();
#endif
			}
		}



	}
	
}
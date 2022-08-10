using System;
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



		public static void Update(float delta)
		{

		}
		public static void NextTurn()
		{
			IsPlayer1Turn = !IsPlayer1Turn;
			WorldManager.ResetControllables(IsPlayer1Turn);
			#if SERVER
			Console.WriteLine("turn: "+IsPlayer1Turn);
			Networking.DoAction(new GameActionPacket());//defaults to end turn
			#endif
		}

		public static void ParsePacket(GameActionPacket packet)
		{
			if (packet.Type == ActionType.Move)
			{
				MovementPacket movementPacket = (MovementPacket)packet;
				Controllable controllable = WorldManager.GetObject(movementPacket.ID).ControllableComponent;
				#if CLIENT
				controllable.Move(movementPacket.Path);
				#else
				controllable.MoveAction(movementPacket.Path.Last());
				#endif
				//if client we just initiate the move and trust the server since that's the move from another player
				//if server we verify the move and then tell both clients to move
			}
			else if ( packet.Type == ActionType.EndTurn)
			{
				NextTurn();
				
			}
		}



	}
	
}
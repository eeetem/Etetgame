using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using CommonData;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Pathfinding;

#nullable enable


namespace MultiplayerXeno
{
	public partial class Controllable
	{
		public WorldObject worldObject { get; private set; }
		public ControllableType Type { get; private set; }

		public Controllable(bool isPlayerOneTeam, WorldObject worldObject, ControllableType type, int movePoints = -1,  int turnPoints = -1, int actionPoints = -1)
		{
			this.worldObject = worldObject;
			Type = type;
			IsPlayerOneTeam = isPlayerOneTeam;
			Health = type.MaxHealth;
#if CLIENT
			WorldManager.Instance.CalculateFov();
#endif
			StartTurn();
			if (movePoints != -1)
			{
				this.movePoints = movePoints;
			}
			if (actionPoints != -1)
			{
				this.actionPoints = actionPoints;
			}
			if (turnPoints != -1)
			{
				this.turnPoints = turnPoints;
			}
			
			
		}
		public int movePoints { get; private set; } = 2;
		public int turnPoints { get; private set; } = 2;
		public int actionPoints { get; private set; } = 1;

		public int Health = 0;
		public int Awareness = 0;

		public List<Vector2Int>[] GetPossibleMoveLocations()
		{ 
			List<Vector2Int>[] possibleMoves = new List<Vector2Int>[movePoints];
			for (int i = 0; i < movePoints; i++)
			{
				possibleMoves[i] = PathFinding.GetAllPaths(this.worldObject.TileLocation.Position, Type.MoveRange*(i+1));
			}

			return possibleMoves;
		}

		public bool IsPlayerOneTeam { get; private set;}

		public void TakeDamage(int ammount)
		{
			
			Console.WriteLine(this + " hit for "+ammount);
			if (Awareness > 0)
			{
				Awareness--;
				Health--;
			}
			else
			{
				Health -= ammount;
			}

			
			if (Health <= 0)
			{
				WorldManager.Instance.DeleteWorldObject(this.worldObject);//dead
			}

		}

		public int GetSightRange()
		{
			//apply effects and offests
			return Type.SightRange;
		}

		public void StartTurn()
		{
			movePoints = 2;
			turnPoints = 2;
			actionPoints = 1;
			if (Awareness < 0)
			{
				Awareness = 0;
			}

			if (Awareness < Type.MaxAwareness)
			{
				Awareness++;
			}

		}

		public void FireAction(Vector2Int position)
		{
			if (actionPoints <= 0)
			{
				return;
			}
			
			var packet = new FirePacket(worldObject.Id,position);
			Networking.DoAction(packet);

#if SERVER
		DoFire(position);
#endif
			
			
		}

		public void FaceAction(Vector2Int position)
		{


			if (turnPoints <= 0 && movePoints <= 0)
			{
				return;
			}

			var targetDir = Utility.ToClampedDirection(this.worldObject.TileLocation.Position - position);

			
			var packet = new FacePacket(worldObject.Id,targetDir);
			Networking.DoAction(packet);

			#if SERVER
			this.DoFace(targetDir);
			#endif
		}

		public void MoveAction(Vector2Int position)
		{
			
			
			PathFinding.PathFindResult result = PathFinding.GetPath(this.worldObject.TileLocation.Position, position);
			if (result.Cost == 0)
			{
				#if CLIENT
				Selected = null;
				#endif
				return;//no path
			}

			int moveUse = 1;
			while (result.Cost > this.Type.MoveRange*moveUse)
			{
				moveUse++;

				
				
			}
			if (moveUse > this.movePoints)
			{
#if CLIENT
				Selected = null;
#endif
				return;
			}
			

			
			//todo move animation
			
			var packet = new MovementPacket(worldObject.Id,result.Path,moveUse);
			Networking.DoAction(packet);
			//we only move as server - as client we send the action, let server verify it and then the server will force us to do the action
			#if SERVER
			DoMove(result.Path,moveUse);
			#endif


		}

		private List<Vector2Int> CurrentPath = new List<Vector2Int>();
		private bool moving;
		private float MoveCounter;
		public void DoMove(List<Vector2Int> path,int pointCost)
		{
			this.movePoints -= pointCost;
			if (movePoints < 0)
			{
				//desync
#if CLIENT
				UI.ShowMessage("Desync Error","Unit Does not have enough move points");
#endif
				return;
			}

			moving = true;
			CurrentPath = path;
		}

		public void DoFace(Direction dir)
		{
			if (turnPoints > 0)
			{
				turnPoints--;
			}else if (movePoints > 0)
			{
				movePoints--;
				turnPoints = 2;
				turnPoints--;
			}
			else
			{
				//desync
#if CLIENT
				UI.ShowMessage("Desync Error","Unit Does not have enough turn points");
#endif
				return;
			}


			worldObject.Face(dir);
		}

		public void DoFire(Vector2Int pos)
		{
#if CLIENT
			Camera.SetPos(pos);
			Targeting = false;
#endif

			
			if (actionPoints > 0)
			{
				actionPoints--;
				Awareness--;

			}
			else
			{
				//desync
#if CLIENT
				UI.ShowMessage("Desync Error","Unit Does not have enough turn points");
#endif
				return;
			}

			Projectile p = new Projectile(worldObject.TileLocation.Position,pos,3);
			p.Fire();

		}

		public void EndTurn()
		{
			
			
		}
		public void Update(float gameTime)
		{
			if (moving)
			{
				MoveCounter += gameTime;
				if (MoveCounter > 500)
				{
					MoveCounter = 0;

					worldObject.Face(Utility.Vec2ToDir(CurrentPath[0] - worldObject.TileLocation.Position ));
					worldObject.Move(CurrentPath[0]);
					CurrentPath.RemoveAt(0);
					if (CurrentPath.Count == 0)
					{
						moving = false;
					}
					//todo jump view to move
#if CLIENT
					WorldManager.Instance.CalculateFov();
#endif
				}
			}

		}

		public ControllableData GetData()
		{
			var data = new ControllableData(this.IsPlayerOneTeam,actionPoints,movePoints,turnPoints);

			return data;
		}
	}
}
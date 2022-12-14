using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using CommonData;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Pathfinding;
using Action = MultiplayerXeno.Action;
#nullable enable


namespace MultiplayerXeno
{
	public partial class Controllable
	{
		public WorldObject worldObject { get; private set; }
		public ControllableType Type { get; private set; }

		public Controllable(bool isPlayerOneTeam, WorldObject worldObject, ControllableType type, ControllableData data)
		{
			this.worldObject = worldObject;
			Type = type;
			IsPlayerOneTeam = isPlayerOneTeam;
			if (data.Health == -1)
			{
				Health = type.MaxHealth;
			}
			else
			{
				Health = data.Health;
			}
			if (data.Awareness == -1)
			{
				Awareness = type.MaxAwareness;
			}
			else
			{
				Awareness = data.Awareness;
			}


#if CLIENT
			WorldManager.Instance.MakeFovDirty();
#endif
			
			if (data.MovePoints != -1)
			{
				this.MovePoints = data.MovePoints;
			}
			if (data.ActionPoints != -1)
			{
				this.ActionPoints = data.ActionPoints;
			}
			if (data.TurnPoints != -1)
			{
				this.TurnPoints = data.TurnPoints;
			}

			if (data.JustSpawned)
			{
				StartTurn();
			}
		}
		public int MovePoints { get;  set; } = 0;
		public int  TurnPoints { get;  set; } = 0;
		public int ActionPoints { get;  set; } = 0;

		public int Health = 0;
		public int Awareness = 0;

		public bool Crouching { get;  set; } = false;



		public int GetMoveRange()
		{
			int range = Type.MoveRange;
			if (Crouching)
			{
				range -= 2;
			}

			return range;
		}

		public List<Vector2Int>[] GetPossibleMoveLocations()
		{ 
			List<Vector2Int>[] possibleMoves = new List<Vector2Int>[MovePoints];
			for (int i = 0; i < MovePoints; i++)
			{
				possibleMoves[i] = PathFinding.GetAllPaths(this.worldObject.TileLocation.Position, GetMoveRange()*(i+1));
			}

			return possibleMoves;
		}

		public bool IsPlayerOneTeam { get; private set;}

		public void TakeDamage(int ammount)
		{
			
			Console.WriteLine(this +"(health:"+this.Health+") hit for "+ammount);
			if (Awareness > 0)
			{
				Console.WriteLine("blocked by awareness");
				List<WorldTile> tiles = WorldManager.Instance.GetTilesAround(this.worldObject.TileLocation.Position);
				foreach (var tile in tiles)
				{
					if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null)
					{
						tile.ObjectAtLocation.ControllableComponent.Awareness--;
					}
				}

				ammount= (int)Math.Floor(ammount/2f);

			}
			
			Console.WriteLine("health - "+ammount);
			Health -= ammount;
			

			
			if (Health <= 0)
			{
				Console.WriteLine("dead");
				WorldManager.Instance.DeleteWorldObject(this.worldObject);//dead
#if CLIENT
				Audio.PlaySound("death",this.worldObject.TileLocation.Position);
#endif
				
			}
			else
			{
#if CLIENT
				Audio.PlaySound("grunt",this.worldObject.TileLocation.Position);
#endif
			}

		}

		public int GetSightRange()
		{
			//apply effects and offests
			return Type.SightRange;
		}

		public void StartTurn()
		{
			MovePoints = Type.MaxMovePoints;
			TurnPoints = Type.MaxTurnPoints;
			ActionPoints = 1;
			if (Awareness < 0)
			{
				Awareness = 0;
			}

			if (Awareness < Type.MaxAwareness)
			{
				Awareness++;
			}

		}


	
	




		private List<Vector2Int> CurrentPath = new List<Vector2Int>();
		private bool _thisMoving;
		public static bool moving;
		private float _moveCounter;


		public void MoveAnimation(List<Vector2Int> path)
		{
			moving = true;
			_thisMoving = true;
			CurrentPath = path;
		}
		
		public void EndTurn()
		{
			
			
		}
		public void Update(float gameTime)
		{
			if (_thisMoving)
			{
				_moveCounter += gameTime;
				if (_moveCounter > 350)
				{
					_moveCounter = 0;
					try
					{
						worldObject.Face(Utility.Vec2ToDir(CurrentPath[0] - worldObject.TileLocation.Position));
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception when facing, the values are: "+CurrentPath[0]+" and " +worldObject.TileLocation.Position + " exception: "+e);
					}

					worldObject.Move(CurrentPath[0]);
					Console.WriteLine("moved");
					CurrentPath.RemoveAt(0);
					if (CurrentPath.Count == 0)
					{
						moving = false;
						_thisMoving = false;
#if CLIENT
						if (Selected != null)
						{
							UI.UnitUI(Selected.worldObject);
						}
						else
						{
							UI.UnitUI(this.worldObject);
						}
#endif
						Console.WriteLine("done moving ");
					}
					//todo jump view to move
#if CLIENT
					WorldManager.Instance.MakeFovDirty();
					if (worldObject.IsVisible())
					{
						Audio.PlaySound("footstep", Utility.GridToWorldPos(worldObject.TileLocation.Position));
					}
#endif
				
				}
			}

		}

		public ControllableData GetData()
		{
			var data = new ControllableData(this.IsPlayerOneTeam,ActionPoints,MovePoints,TurnPoints,Health,Awareness);
			data.JustSpawned = false;
			return data;
		}
	}
}
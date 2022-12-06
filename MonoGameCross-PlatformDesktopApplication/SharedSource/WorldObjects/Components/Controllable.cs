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
		public int MovePoints { get; private set; } = 0;
		public int TurnPoints { get; private set; } = 0;
		public int ActionPoints { get; private set; } = 0;

		public int Health = 0;
		public int Awareness = 0;

		public bool Crouching { get; private set; } = false;


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
				Awareness--; 
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

		public void CrouchAction()
		{
			if (MovePoints <= 0) return;
			
			var packet = new GameActionPacket();
			packet.ID = worldObject.Id;
			packet.Type = ActionType.Crouch;
			Networking.DoAction(packet);
#if SERVER
		DoCrouch();	
#endif
		}

		public void DoCrouch()
		{
			if (MovePoints > 0)
			{
				Crouching = !Crouching;
				MovePoints--;
#if CLIENT
		WorldManager.Instance.MakeFovDirty();	
		if (Selected != null)
		{
			UI.UnitUI(Selected.worldObject);
		}
		else
		{
			UI.UnitUI(this.worldObject);
		}
#endif
			}
			else
			{
#if CLIENT
				UI.ShowMessage("Desync Error","Unit Does not have enough move points");
#endif
				return;
			}

		}

		public void FireAction(Vector2Int position)
		{
			if (position == this.worldObject.TileLocation.Position)
			{
				return;
			}

			if (ActionPoints <= 0)
			{
				return;
			}
			if (!Type.RunAndGun && MovePoints <= 0)
			{
				return;
			}
#if CLIENT
			if (!UI.validShot)
			{
				return;
			}
#endif
			
			var packet = new FirePacket(worldObject.Id,position);
			Networking.DoAction(packet);
			


#if SERVER
		DoFire(position);
#endif
			
			
		}

		public void FaceAction(Vector2Int position)
		{


			if (TurnPoints <= 0 && MovePoints <= 0)
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
				Console.WriteLine("move action rejected: cost is 0");
				#if CLIENT
				Selected = null;
				#endif
				return;//no path
			}

			int moveUse = 1;
			while (result.Cost > GetMoveRange()*moveUse)
			{
				moveUse++;

				
				
			}
			if (moveUse > this.MovePoints)
			{
#if CLIENT
				Selected = null;
#endif
				
				Console.WriteLine("client attempted to move past move points at: "+this.worldObject.TileLocation.Position +" to "+result.Path.Last());
				
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
		private bool _thisMoving;
		private static bool moving;
		private float _moveCounter;
		public void DoMove(List<Vector2Int> path,int pointCost)
		{
			Console.WriteLine("DoMove started");
			if(moving)return;
			if (MovePoints - pointCost < 0)
			{
				//desync
#if CLIENT
				UI.ShowMessage("Desync Error","Unit Does not have enough move points");
#else
				Console.WriteLine("move action rejected: not enough  movepoints");
#endif
				return;
			}
			this.MovePoints -= pointCost;
			moving = true;
			_thisMoving = true;
			CurrentPath = path;
			Console.WriteLine("DoMove finished without returning");
		}

		public void DoFace(Direction dir)
		{
			if (TurnPoints > 0)
			{
				TurnPoints--;
			}else if (MovePoints > 0)
			{
				MovePoints--;
				TurnPoints = 2;
				TurnPoints--;
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

			
			if (ActionPoints > 0)
			{
				ActionPoints--;
				Awareness--;
				if (!Type.RunAndGun)
				{
					MovePoints--;
				}

			}
			else
			{
				//desync
#if CLIENT
				UI.ShowMessage("Desync Error","Unit Does not have enough turn points");
#endif
				return;
			}
			
			//client shouldnt be allowed to judge what got hit
#if SERVER
			bool lowShot = false;
			if (this.Crouching)
			{
				lowShot = true;
			}else
			{
				WorldTile tile = WorldManager.Instance.GetTileAtGrid(pos);
				if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null && tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent.Crouching)
				{
					lowShot = true;
				}
			}
			Vector2 shotDir = Vector2.Normalize(pos - worldObject.TileLocation.Position);
			Projectile p = new Projectile(worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),pos+new Vector2(0.5f,0.5f),Type.WeaponDmg,Type.WeaponRange,lowShot);
			p.Fire();
			Networking.DoAction(new ProjectilePacket(p.result,p.covercast,Type.WeaponDmg,p.dropoffRange));

#endif
			#if CLIENT
			if (Type.WeaponRange > 6)
			{
				ObjectSpawner.Burst(this.worldObject.TileLocation.Position, pos);
			}
			else
			{
				ObjectSpawner.ShotGun(this.worldObject.TileLocation.Position,pos);	
			}
#endif
			worldObject.Face(Utility.ToClampedDirection( this.worldObject.TileLocation.Position-pos));
		
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
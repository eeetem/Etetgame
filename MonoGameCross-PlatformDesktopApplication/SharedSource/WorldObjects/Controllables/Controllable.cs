using System;
using System.Collections.Generic;
using System.Linq;
using MultiplayerXeno;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Items;
using MultiplayerXeno.Pathfinding;
#if CLIENT
using MultiplayerXeno.UILayouts;
#endif

#nullable enable


namespace MultiplayerXeno
{
	public partial class Controllable
	{
		public WorldObject worldObject { get; private set; }
		public ControllableType Type { get; private set; }

		public UsableItem? SelectedItem { get; private set; }

		public List<UsableItem> inventory = new List<UsableItem>();

		public void RemoveItem(UsableItem item)
		{
			inventory.Remove(item);
			if (SelectedItem == item)
			{
				if(inventory.Count>0){
					SelectedItem = inventory[0];
				}
				else
				{
					SelectedItem = null;
				}
			}
		}

		public Controllable(bool isPlayerOneTeam, WorldObject worldObject, ControllableType type, ControllableData data)
		{
			
			this.worldObject = worldObject;
			Type = type;
			IsPlayerOneTeam = isPlayerOneTeam;


			if (data.Determination == -100)
			{
				Determination = type.Maxdetermination;
			}
			else
			{
				Determination = data.Determination;
			}

			this.Crouching = data.Crouching;
			this.paniced = data.Panic;


#if CLIENT
			WorldManager.Instance.MakeFovDirty();
#endif

			if (data.MovePoints != -100)
			{
				this.MovePoints = data.MovePoints;
			}

			if (data.ActionPoints != -100)
			{
				this.FirePoints = data.ActionPoints;
			}

			if (data.canTurn != null)
			{
				this.canTurn = (bool) data.canTurn;
			}

			inventory = new List<UsableItem>();
			for (int i = 0; i < type.InventorySize; i++)
			{
				if (data.Inventory.Count > i && data.Inventory[i] != null)
				{
					inventory.Add(PrefabManager.UseItems[data.Inventory[i]]);
				}
			}

			if (inventory.Count > 0)
			{
				SelectedItem = inventory[0];
			}

			if (data.JustSpawned)
			{
				StartTurn();
			}
		}

		public int MovePoints { get; set; } = 0;
		public bool canTurn { get; set; } = false;
		public int FirePoints { get; set; } = 0;
		public int Determination { get; private set; } = 0;

		public bool Crouching { get; set; } = false;



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
				possibleMoves[i] = PathFinding.GetAllPaths(this.worldObject.TileLocation.Position, GetMoveRange() * (i + 1));
			}

			return possibleMoves;
		}

		public bool IsPlayerOneTeam { get; private set; }

		public void Suppress(int detDmg,bool nopanic = false)
		{
			if (detDmg <= 0)
			{
				return;
			}

			Determination -= detDmg;
			if (Determination <= 0 && !nopanic)
			{
				Panic();
			}
		}



		public void TakeDamage(int dmg, int detResis)
		{
			Console.WriteLine(this +"(health:"+worldObject.Health+") hit for "+dmg);
			if (Determination > 0)
			{
				Console.WriteLine("blocked by determination");
				dmg = dmg - detResis;

			}

			if (dmg <= 0)
			{
				Console.WriteLine("0 damage");
				return;
			}


			worldObject.Health -= dmg;
			
			Console.WriteLine("unit hit for: "+dmg);
			Console.WriteLine("outcome: health="+worldObject.Health);
			if (worldObject.Health <= 0)
			{
				Console.WriteLine("dead");
				ClearOverWatch();
				if (_thisMoving)
				{
					moving = false;
				}

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

		public bool CanHit(Vector2Int target, bool lowTarget = false)
		{
			Vector2 shotDir = Vector2.Normalize(target - worldObject.TileLocation.Position);
			Projectile proj = new Projectile(worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),target+new Vector2(0.5f,0.5f),0,100,lowTarget,Crouching,0,0,0);
			
				
			if (proj.result.hit)
			{
				var hitobj = WorldManager.Instance.GetObject(proj.result.hitObjID);
				if (hitobj.Type.Edge || hitobj.TileLocation.Position != target)
				{
					return false;
				}
			}

			return true;
		}

		public void StartTurn()
		{
			MovePoints = Type.MaxMovePoints;
			canTurn = true;
			FirePoints = Type.MaxFirePoints;
			if (Determination < 0)
			{
				Determination = 0;
			}

			if (Determination < Type.Maxdetermination)
			{
				Determination++;
			}
			if(paniced)
			{
#if CLIENT
				if (worldObject.IsVisible())
				{
					new PopUpText("Recovering From Panic", this.worldObject.TileLocation.Position);
				}	
#endif
			

				paniced = false;
				Determination--;
				MovePoints--;
				canTurn = false;
			}
			ClearOverWatch();

		}
		public void DoAction(Action a,Vector2Int target)
		{
#if CLIENT
			if (!IsMyTeam()) return;
			if (!GameManager.IsMyTurn()) return;
#endif
			var result = a.CanPerform(this, target);
			if (!result.Item1)
			{
#if CLIENT
				new PopUpText(result.Item2, this.worldObject.TileLocation.Position);
#else
				Console.WriteLine("tried to do action but failed: "+result.Item2);
#endif
				
				return;
			}
#if CLIENT
			a.ToPacket(this, target);
#else
			a.Perform(this, target);
			a.ToPacket(this,target);
#endif
			
			
		}

		public bool paniced { get; private set; }= false;
		public void Panic()
		{
			Crouching = true;
			paniced = true;
			if (moving)
			{
				moving = false;
				_thisMoving = false;
			}

		
#if CLIENT
			if (worldObject.IsVisible())
			{
				new PopUpText("Panic!", this.worldObject.TileLocation.Position);	
			}
#endif
			ClearOverWatch();
			
		}
		
		

		public bool overWatch { get; set; } = false;
		public List<Vector2Int> overWatchedTiles = new List<Vector2Int>();
		public void OverWatchSpoted(Vector2Int location)
		{
			
#if SERVER
			bool isFriendly = this.IsPlayerOneTeam == WorldManager.Instance.GetTileAtGrid(location).ControllableAtLocation.ControllableComponent.IsPlayerOneTeam;
			//make this "can player see" fucntion
			List<int> units;
			if (this.IsPlayerOneTeam)
			{
				units = GameManager.T1Units;
			}
			else
			{
				units = GameManager.T2Units;
			}

			Visibility vis = Visibility.None;
			foreach (var unit in units)
			{
				var WO = WorldManager.Instance.GetObject(unit);
				if (WO != null)
				{
					var tempVis = WorldManager.Instance.CanSee(WO.ControllableComponent, location);
					if (tempVis > vis)
					{
						vis = tempVis;
					}
				}

			
			}
			
			Console.WriteLine("overwatch spotted by "+this.worldObject.TileLocation.Position+" is friendly: "+isFriendly+" vis: "+vis);
			if (!isFriendly && CanHit(location)&& vis >= WorldManager.Instance.GetTileAtGrid(location).ControllableAtLocation.GetMinimumVisibility())
			{
				Console.WriteLine("overwatch fired by "+this.worldObject.TileLocation.Position);
				DoAction(Action.Actions[ActionType.Attack], location);
			}
#endif
			
		}


		public void ClearOverWatch()
		{
			overWatch = false;
			foreach (var tile in overWatchedTiles)
			{
				WorldManager.Instance.GetTileAtGrid(tile).UnWatch(this);
			}
			overWatchedTiles.Clear();
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
				if (_moveCounter > 250)
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
					CurrentPath.RemoveAt(0);
					if (CurrentPath.Count == 0)
					{
						moving = false;
						_thisMoving = false;

					}
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
			List<string?> inv = new List<string?>();
			foreach (var i in inventory)
			{
				if (i != null)
				{
					inv.Add(i.name);
				}
				else
				{
					inv.Add(null);
				}
			}
			var data = new ControllableData(this.IsPlayerOneTeam,FirePoints,MovePoints,canTurn,Determination,Crouching,paniced,inv);
			data.JustSpawned = false;
			return data;
		}

		protected bool Equals(Controllable other)
		{
			return worldObject.Equals(other.worldObject);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Controllable) obj);
		}

		public override int GetHashCode()
		{
			return worldObject.GetHashCode();
		}
	}
}
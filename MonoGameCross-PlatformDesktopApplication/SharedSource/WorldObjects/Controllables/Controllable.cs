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

		

		public readonly WorldAction?[] Inventory;
		
	

		public void AddItem(WorldAction item)
		{
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] == null)
				{
					Inventory[i] = item;
#if CLIENT
					if (SelectedItemIndex == -1)
					{
						SelectedItemIndex = i;
					}
#endif
					
					return;
				}
			}
		}
		public void RemoveItem(int index)
		{
			Inventory[index] = null;
#if CLIENT
			if (SelectedItemIndex == index && IsMyTeam() && GameManager.IsMyTurn())
			{
				SelectAnyItem();
			}
#endif
		}
		public List<IExtraAction> extraActions = new List<IExtraAction>();
		public Controllable(bool isPlayerOneTeam, WorldObject worldObject, ControllableType type, ControllableData data)
		{
			
			this.worldObject = worldObject;
			Type = type;
			IsPlayerOneTeam = isPlayerOneTeam;
	
			
			type.extraActions.ForEach(extraAction =>
			{
				extraActions.Add((IExtraAction) extraAction.Clone());
			});


			if (data.Determination == -100)
			{ Determination = type.Maxdetermination;
			}
			else
			{
				Determination = data.Determination;
			}

			Crouching = data.Crouching;
			paniced = data.Panic;


#if CLIENT
			WorldManager.Instance.MakeFovDirty();
#endif

			MovePoints = new Value(0, type.MaxMovePoints);
			if (data.MovePoints != -100)
			{
				MovePoints.Current = data.MovePoints;
			}

			ActionPoints = new Value(0, type.MaxActionPoints);
			if (data.ActionPoints != -100)
			{
				ActionPoints.Current = data.ActionPoints;
			}

			if (data.canTurn != null)
			{
				canTurn = (bool) data.canTurn;
			}

			Inventory = new WorldAction[type.InventorySize];
			for (int i = 0; i < type.InventorySize; i++)
			{
				if (data.Inventory.Count > i && data.Inventory[i] != null)
				{
					AddItem(PrefabManager.UseItems[data.Inventory[i]]);
				}
			}
#if CLIENT
			SelectAnyItem();
#endif
			

			foreach (var effect in data.StatusEffects)
			{
				ApplyStatus(effect.Item1,effect.Item2);
			}
			if (data.JustSpawned)
			{
				StartTurn();
			}
		}





		public bool canTurn { get; set; } = false;
		
		
		public Value MovePoints;
		public Value ActionPoints;
		public int Determination;

		public bool Crouching { get; set; } = false;


		public Value MoveRangeEffect = new Value(0, 0);
		public int GetMoveRange()
		{
			int range = Type.MoveRange;
			if (Crouching)
			{
				range -= 2;
			}

			int result= range + MoveRangeEffect.Current;
			if(result<=0){
				return 1;
			}

			return result;
		}

		public List<Vector2Int>[] GetPossibleMoveLocations()
		{
			List<Vector2Int>[] possibleMoves = new List<Vector2Int>[MovePoints.Current];
			for (int i = 0; i < MovePoints; i++)
			{
				possibleMoves[i] = PathFinding.GetAllPaths(worldObject.TileLocation.Position, GetMoveRange() * (i + 1));
			}

			return possibleMoves;
		}

		public bool IsPlayerOneTeam { get; private set; }




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

				WorldManager.Instance.DeleteWorldObject(worldObject);//dead
#if CLIENT
				Audio.PlaySound("death",worldObject.TileLocation.Position);
#endif
				
			}
			else
			{
#if CLIENT
				Audio.PlaySound("grunt",worldObject.TileLocation.Position);
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
			MovePoints.Reset();
			canTurn = true;
			ActionPoints.Reset();
			MoveRangeEffect.Reset();
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
					new PopUpText("Recovering From Panic", worldObject.TileLocation.Position);
				}	
#endif
			

				paniced = false;
				Determination--;
				MovePoints--;
				canTurn = false;
			}
			ClearOverWatch();
			foreach (var effect in StatusEffects)
			{
				effect.Apply(this);
				
			}

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
				new PopUpText(result.Item2, worldObject.TileLocation.Position);
				new PopUpText(result.Item2, target);
#else
				Console.WriteLine("tried to do action but failed: "+result.Item2);
#endif
				
				return;
			}
#if CLIENT
			a.ToPacket(this, target);
#else
			a.ToPacket(this,target);
			a.Perform(this, target);
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
				new PopUpText("Panic!", worldObject.TileLocation.Position);	
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



		private List<Vector2Int>? CurrentPath = new List<Vector2Int>();
		private bool _thisMoving;
		public static bool moving;
		private float _moveCounter;


		public void MoveAnimation(List<Vector2Int>? path)
		{
			moving = true;
			_thisMoving = true;
			CurrentPath = path;
		}
		
		public void EndTurn()
		{
			StatusEffects.RemoveAll(x => x.duration <= 0);

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
#if CLIENT
						GameLayout.ReMakeMovePreview();
#endif
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
			foreach (var i in Inventory)
			{
				if (i != null)
				{
					inv.Add(i.Name);
				}
				else
				{
					inv.Add(null);
				}
			}

			List<Tuple<string, int>> sts = new List<Tuple<string, int>>();
			foreach (var st in StatusEffects)
			{
				sts.Add(new Tuple<string, int>(st.type.name,st.duration));
			}
			var data = new ControllableData(IsPlayerOneTeam,ActionPoints.Current,MovePoints.Current,canTurn,Determination,Crouching,paniced,inv,sts,overWatch);
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
			if (obj.GetType() != GetType()) return false;
			return Equals((Controllable) obj);
		}

		public override int GetHashCode()
		{
			return worldObject.GetHashCode();
		}

		public List<StatusEffectInstance> StatusEffects = new List<StatusEffectInstance>();
		public void ApplyStatus(string effect, int duration)
		{
			var statuseffect = new StatusEffectInstance(PrefabManager.StatusEffects[effect],duration);
			StatusEffects.Add(statuseffect);
			statuseffect.Apply(this);
		}

		public void RemoveStatus(string effectName)
		{
			StatusEffects.RemoveAll(x => x.type.name == effectName);
		}

		public void Suppress(int supression, bool noPanic = false)
		{
			if(supression==0) return;
			
			Determination-= supression;
			if (Determination <= 0 && !noPanic)
			{
				Panic();
			}

			if (paniced && Determination > 0)
			{
				paniced = false;
			}
			if(Determination>Type.Maxdetermination) Determination = Type.Maxdetermination;
		}
	}
}
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Items;
using MultiplayerXeno.Pathfinding;
using Riptide;
#if CLIENT
using MultiplayerXeno.UILayouts;
#endif

#nullable enable


namespace MultiplayerXeno
{
	public partial class Unit
	{
		public WorldObject WorldObject { get; private set; }
		public UnitType Type { get; private set; }

		

		public readonly WorldAction?[] Inventory;
		public Unit(bool isPlayerOneTeam, WorldObject wo, UnitType type, UnitData data)
		{
			
			WorldObject = wo;
			Type = type;
			IsPlayerOneTeam = isPlayerOneTeam;
	
			
			type.ExtraActions.ForEach(extraAction =>
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
			ThisMoving = data.ThisMoving;


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

			
			canTurn = data.CanTurn;
			

			MoveRangeEffect.Current = data.MoveRangeEffect;

			Inventory = new WorldAction[type.InventorySize];
			for (int i = 0; i < type.InventorySize; i++)
			{
				if (data.Inventory.Count > i && data.Inventory[i] != null)
				{
					AddItem(PrefabManager.UseItems[data.Inventory[i]!]);
				}
			}
			overWatch = data.Overwatch;
			SelectedItemIndex = data.SelectIndex;

			if (data.LastItem != null)
			{
				LastItem = PrefabManager.UseItems[data.LastItem];
			}
			foreach (var effect in data.StatusEffects)
			{
				ApplyStatus(effect.Item1,effect.Item2);
			}
			if (data.JustSpawned)
			{
				StartTurn();
			}
		}

	
		public void SelectAnyItem()
		{
			if(SelectedItem!= null) return;
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] != null)
				{
					DoAction(Action.Actions[Action.ActionType.SelectItem], new Vector2Int(i, 0));
					return;
				}
			}
			DoAction(Action.Actions[Action.ActionType.SelectItem], new Vector2Int(-1, 0));
		}

		public void AddItem(WorldAction item)
		{
			for (int i = 0; i < Inventory.Length; i++)
			{
				if (Inventory[i] == null)
				{
					Inventory[i] = item;
#if SERVER
					if (SelectedItemIndex == -1)
					{
						DoAction(Action.Actions[Action.ActionType.SelectItem], new Vector2Int(i, 0));
					}
#endif
					
					return;
				}
			}
		}
		public void RemoveItem(int index)
		{
			Inventory[index] = null;
		}
		public List<IExtraAction> extraActions = new List<IExtraAction>();


		public bool canTurn { get; set; }
		
		
		public Value MovePoints;
		public Value ActionPoints;
		public int Determination;

		public bool Crouching { get; set; }


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
			if (MovePoints > 0)
			{
				List<Vector2Int>[] possibleMoves = new List<Vector2Int>[MovePoints.Current];
				for (int i = 0; i < MovePoints; i++)
				{
					possibleMoves[i] = PathFinding.GetAllPaths(WorldObject.TileLocation.Position, GetMoveRange() * (i + 1));
				}return possibleMoves;
			}

			return new List<Vector2Int>[0];
			

			
		}

		public bool IsPlayerOneTeam { get; private set; }




		public void TakeDamage(int dmg, int detResis)
		{
			Console.WriteLine(this +"(health:"+WorldObject.Health+") hit for "+dmg);
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


			WorldObject.Health -= dmg;
			
			Console.WriteLine("unit hit for: "+dmg);
			Console.WriteLine("outcome: health="+WorldObject.Health);
			if (WorldObject.Health <= 0)
			{
				Console.WriteLine("dead");
				ClearOverWatch();
				if (ThisMoving)
				{
					AnyUnitMoving = false;
					ThisMoving = false;
				}

				WorldManager.Instance.DeleteWorldObject(WorldObject);//dead
#if CLIENT
				Audio.PlaySound("death",WorldObject.TileLocation.Position);
#endif
				
			}
			else
			{
#if CLIENT
				Audio.PlaySound("grunt",WorldObject.TileLocation.Position);
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
			Vector2 shotDir = Vector2.Normalize(target - WorldObject.TileLocation.Position);
			Projectile proj = new Projectile(WorldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+shotDir/new Vector2(2.5f,2.5f),target+new Vector2(0.5f,0.5f),0,100,lowTarget,Crouching,0,0,0);
			
				
			if (proj.Result.hit)
			{
				var hitobj = WorldManager.Instance.GetObject(proj.Result.HitObjId);
				if (hitobj!.Type.Edge || hitobj.TileLocation.Position != target)
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
				if (WorldObject.IsVisible())
				{
					new PopUpText("Recovering From Panic", WorldObject.TileLocation.Position);
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
#if SERVER
			SelectAnyItem();
#endif
			

		}
		public void DoAction(Action a,Vector2Int target)
		{
#if CLIENT
			if (!IsMyTeam()) return;
			if (!GameManager.IsMyTurn()) return;
#endif
			var result = a.CanPerform(this, ref target);
			if (!result.Item1)
			{
#if CLIENT
				new PopUpText(result.Item2, WorldObject.TileLocation.Position);
				new PopUpText(result.Item2, target);
#else
				Console.WriteLine("tried to do action but failed: "+result.Item2);
#endif
				
				return;
			}
#if CLIENT
			a.SendToServer(this, target);
			a.ExecuteClientSide(this, target);
#else
			a.PerformServerSide(this, target);
#endif
			
			
		}

		public bool paniced { get; private set; }
		public void Panic()
		{


		
#if CLIENT
			if (WorldObject.IsVisible())
			{
				var t = new PopUpText("Panic!", WorldObject.TileLocation.Position);	
				t.scale = 2;
				t.Color = Color.Red;
			}
#endif
			
			Crouching = true;
			paniced = true;
			if (AnyUnitMoving)
			{
				AnyUnitMoving = false;
				ThisMoving = false;
	
			}
			ClearOverWatch();
			
		}
		
		

		public bool overWatch { get; set; }


		public List<Vector2Int> overWatchedTiles = new List<Vector2Int>();
		
		
		private bool overwatchShotThisMove = false;

		public void OverWatchSpoted(Vector2Int location)
		{
			
	

			
#if SERVER

			Unit unit = WorldManager.Instance.GetTileAtGrid(location).UnitAtLocation;
			if (unit == null)
			{
				throw new Exception("overwatch spoted with no unit at location");
			}
			bool isFriendly = IsPlayerOneTeam == unit.IsPlayerOneTeam;
			//make this "can player see" fucntion
			List<int> units;
			if (IsPlayerOneTeam)
			{
				units = GameManager.T1Units;
			}
			else
			{
				units = GameManager.T2Units;
			}

			Visibility vis = Visibility.None;
			foreach (var u in units)
			{
				var WO = WorldManager.Instance.GetObject(u);
				if (WO != null)
				{
					var tempVis = WorldManager.Instance.CanSee(WO.UnitComponent, location);
					if (tempVis > vis)
					{
						vis = tempVis;
					}
				}

			
			}
			
			Console.WriteLine("overwatch spotted by "+WorldObject.TileLocation.Position+" is friendly: "+isFriendly+" vis: "+vis);
			if(overwatchShotThisMove) return;
			if (!isFriendly && CanHit(location)&& vis >= unit.WorldObject.GetMinimumVisibility())
			{
				Console.WriteLine("overwatch fired by "+WorldObject.TileLocation.Position);
				DoAction(Action.Actions[Action.ActionType.Attack], location);
				overwatchShotThisMove = true;
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
		public bool ThisMoving;
		public static bool AnyUnitMoving;
		public static bool AnyUnitActuallyMoving;
		private float _moveCounter;


		public void MoveAnimation(List<Vector2Int>? path)
		{
			AnyUnitMoving = true;
			ThisMoving = true;
			CurrentPath = path;
		}
		
		public void EndTurn()
		{
			AnyUnitMoving = false;
			StatusEffects.RemoveAll(x => x.duration <= 0);

			
		}
		
		
		public void Update(float gameTime)
		{
			AnyUnitActuallyMoving = false;

			
			if (ThisMoving)
			{
				AnyUnitActuallyMoving = true;
				_moveCounter -= gameTime;
				if (_moveCounter < 0)
				{
					
					try
					{
						WorldObject.Face(Utility.Vec2ToDir(CurrentPath[0] - WorldObject.TileLocation.Position));
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception when facing, the values are: "+CurrentPath[0]+" and " +WorldObject.TileLocation.Position + " exception: "+e);
					}

					WorldObject.Move(CurrentPath[0]);
					CurrentPath.RemoveAt(0);
					if (CurrentPath.Count == 0)
					{
						AnyUnitMoving = false;
						ThisMoving = false;
#if SERVER
						foreach (var u in GameManager.T1Units)
						{
							WorldManager.Instance.GetObject(u).UnitComponent.overwatchShotThisMove = false;
						}
						foreach (var u in GameManager.T2Units)
						{
							WorldManager.Instance.GetObject(u).UnitComponent.overwatchShotThisMove = false;
						}
#endif
						
#if CLIENT
						GameLayout.ReMakeMovePreview();
#endif
					}
					else
					{
						_moveCounter = (float)WorldManager.Instance.GetTileAtGrid(CurrentPath[0]).TraverseCostFrom(WorldObject.TileLocation.Position)*250f;
					}
#if CLIENT
					WorldManager.Instance.MakeFovDirty();
					if (WorldObject.IsVisible())
					{
						Audio.PlaySound("footstep", Utility.GridToWorldPos(WorldObject.TileLocation.Position));
					}
#endif
					
				}
			}

			if (!AnyUnitActuallyMoving && AnyUnitMoving)
			{
				AnyUnitMoving = false;//hacky fix
			}

		}
		[Serializable]
		public struct UnitData : IMessageSerializable
		{
			public bool Team1;
			public int ActionPoints;
			public int MovePoints;
			public bool CanTurn;
			public int Determination;
			public bool Crouching;
			public bool Panic;
			public bool JustSpawned;
			public bool Overwatch;
			public int SelectIndex;
			public string? LastItem;
			public int MoveRangeEffect;
			public bool ThisMoving;
			public List<string> Inventory { get; set; }
			public List<Tuple<string, int>> StatusEffects { get; set; }
		
			public UnitData(bool team1)
			{
				Team1 = team1;
				ActionPoints = -100;
				MovePoints = -100;
				CanTurn = false;
				Determination = -100;
				Crouching = false;
				Panic = false;
				JustSpawned = true;//it's always truea nd only set to false in getData
				Overwatch = false;
				Inventory = new List<string>();
				StatusEffects = new List<Tuple<string, int>>();
				SelectIndex = -1;
				MoveRangeEffect = 0;
				LastItem = null;
				ThisMoving = false;
			}
			public UnitData(Unit u)
			{
				Team1 = u.IsPlayerOneTeam;
				ActionPoints = u.ActionPoints.Current;
				MovePoints = u.MovePoints.Current;
				CanTurn = u.canTurn;
				Determination = u.Determination;
				Crouching =	u.Crouching;
				JustSpawned = true;
				Panic = u.paniced;
				Inventory = new List<string>();
				foreach (var i in u.Inventory)
				{
					if (i != null)
					{
						Inventory.Add(i.Name);
					}
					else
					{
						Inventory.Add("");
					}
				}
				StatusEffects = new List<Tuple<string, int>>();
				foreach (var st in u.StatusEffects)
				{
					StatusEffects.Add(new Tuple<string, int>(st.type.name,st.duration));
				}
				Overwatch = u.overWatch;
				SelectIndex = u.SelectedItemIndex;
				LastItem = u.LastItem?.Name;
				MoveRangeEffect = u.MoveRangeEffect.Current;
				ThisMoving = u.ThisMoving;
			}


			public void Serialize(Message message)
			{
				message.Add(Team1);
				message.Add(ActionPoints);
				message.Add(MovePoints);
				message.AddBool(CanTurn);
				message.Add(Determination);
				message.Add(Crouching);
				message.Add(Panic);
				message.Add(JustSpawned);
				message.Add(Overwatch);
				message.Add(SelectIndex);
				message.AddNullableString(LastItem);
				message.Add(MoveRangeEffect);
				message.Add(ThisMoving);

				message.Add(Inventory.Count);
				foreach (var i in Inventory)
				{
					message.Add(i);
				}
				message.Add(StatusEffects.Count);
				foreach (var i in StatusEffects)
				{
					message.Add(i.Item1);
					message.Add(i.Item2);
				}

			}

			public void Deserialize(Message message)
			{
				Team1 = message.GetBool();
				ActionPoints = message.GetInt();
				MovePoints = message.GetInt();
				CanTurn = message.GetBool();
				Determination = message.GetInt();
				Crouching = message.GetBool();
				Panic = message.GetBool();
				JustSpawned = message.GetBool();
				Overwatch = message.GetBool();
				SelectIndex = message.GetInt();
				LastItem = message.GetNullableString();
				MoveRangeEffect = message.GetInt();
				ThisMoving = message.GetBool();

				Inventory = new List<string>();
				var count = message.GetInt();
				for (int i = 0; i < count; i++)
				{
					Inventory.Add(message.GetString());
				}
				StatusEffects = new List<Tuple<string, int>>();
				count = message.GetInt();
				for (int i = 0; i < count; i++)
				{
					StatusEffects.Add(new Tuple<string, int>(message.GetString(),message.GetInt()));
				}
			}
		}
		public UnitData GetData()
		{
			
			var data = new UnitData(this);
			data.JustSpawned = false;
			return data;
		}

		protected bool Equals(Unit other)
		{
			return WorldObject.Equals(other.WorldObject);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Unit) obj);
		}

		public override int GetHashCode()
		{
			return WorldObject.GetHashCode();
		}

		public List<StatusEffectInstance> StatusEffects = new List<StatusEffectInstance>();
		public void ApplyStatus(string? effect, int duration)
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
		
		public WorldAction? SelectedItem
		{
			get
			{
				if(SelectedItemIndex == -1) return null;
				return Inventory[SelectedItemIndex];
			}
		}
		private int _selectedItemIndex = -1;
		public int SelectedItemIndex
		{
			get => _selectedItemIndex;
			set
			{
				
				Console.WriteLine("Selected Item: "+value);
				_selectedItemIndex = value;
			}
		}

		public WorldAction? LastItem;
		

		public string GetVar(string var,string? param = null)
		{
			Console.WriteLine("getting value "+var+" with param "+param);
			var type = GetType();
			var field = type.GetField(var);
			object? value = null;
			if (field == null)
			{
				var property = type.GetProperty(var);
				value = property?.GetValue(this);
			}
			else
			{
				value = field?.GetValue(this);
			}

			if (param == null)
			{
				Console.WriteLine("returning "+value);
				return value.ToString();
			}

			var innerField = value.GetType().GetField(param);
			var innerValue = innerField.GetValue(value);
			Console.WriteLine("returning "+innerValue);
			return innerValue.ToString();

		}
	}
}
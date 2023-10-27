using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects.Units;

using Microsoft.Xna.Framework;
using Riptide;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;
#if CLIENT

using DefconNull.Rendering;
#endif

#nullable enable


namespace DefconNull.World.WorldObjects
{
	public partial class Unit
	{
		public WorldObject WorldObject { get; private set; }
		public UnitType Type { get; private set; }


		public Unit(WorldObject parent, UnitType type, UnitData data)
		{

			WorldObject = parent;
			Type = type;
			IsPlayer1Team = data.Team1;
			parent.UnitComponent = this;


			Determination = new Value(0, type.Maxdetermination);
			if (data.Determination != -100)
			{
				Determination.Current = data.Determination;
			}
			else
			{
				Determination.SetToMax();
			}


			Crouching = data.Crouching;
			Paniced = data.Panic;

			WorldManager.Instance.MakeFovDirty();



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
            
	

			if (data.JustSpawned)
			{
				foreach (var effect in data.StatusEffects)
				{
					ApplyStatus(effect.Item1, effect.Item2);
				}
#if SERVER
				if (type.SpawnEffect != null)
				{
					Task t = new Task(delegate
					{
						foreach (var c in type.SpawnEffect.GetApplyConsiqunces(WorldObject.TileLocation.Position))
						{
							WorldManager.Instance.AddSequence(c);
							Networking.NetworkingManager.SendSequence(c);
						}
					});
					WorldManager.Instance.RunNextAfterFrames(t);
				}
#endif



				
				StartTurn();
			}

			Overwatch = data.Overwatch;
			foreach (var t in data.OverWatchedTiles)
			{
				overWatchedTiles.Add(t);
				WorldManager.Instance.GetTileAtGrid(t).Watch(this);
			}
		}
        
        

		public List<UnitAbility> Abilities = new List<UnitAbility>();

		public bool canTurn { get; set; }


		public Value MovePoints;
		public Value ActionPoints;
		public Value Determination;

		public bool Crouching { get; set; }


		public Value MoveRangeEffect = new Value(0, 0);

		public int GetMoveRange()
		{
			int range = Type.MoveRange;
			if (Crouching)
			{
				range -= 2;
			}

			int result = range + MoveRangeEffect.Current;
			if (result <= 0)
			{
				return 1;
			}

			return result;
		}

		public List<Vector2Int>[] GetPossibleMoveLocations(int moveRange = -1)
		{
			if (MovePoints > 0)
			{
				if(moveRange==-1) moveRange = GetMoveRange();
				List<Vector2Int>[] possibleMoves = new List<Vector2Int>[MovePoints.Current];
				for (int i = 0; i < MovePoints; i++)
				{
					possibleMoves[i] = PathFinding.GetAllPaths(WorldObject.TileLocation.Position, moveRange * (i + 1));
				}

				for (int i = MovePoints.Current - 1; i > 0; i--)
				{
					possibleMoves[i].RemoveAll(x => possibleMoves[i - 1].Contains(x));
				}
				return possibleMoves;
			}

			return Array.Empty<List<Vector2Int>>();
            

		}

		public bool IsPlayer1Team { get; private set; }
		

		public void TakeDamage(int dmg, int detResis)
		{
			if (dmg != 0) //log spam prevention
			{
				Console.WriteLine(this + "(health:" + WorldObject.Health + ") hit for " + dmg);
			}

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

			Console.WriteLine("unit hit for: " + dmg);
			Console.WriteLine("outcome: health=" + WorldObject.Health);
			if (WorldObject.Health <= 0)
			{
				Console.WriteLine("dead");
				ClearOverWatch();
	
				WorldManager.Instance.DeleteWorldObject(WorldObject); //dead
				
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



		public void StartTurn()
		{
			MovePoints.SetToMax();
			canTurn = true;
			ActionPoints.SetToMax();
			MoveRangeEffect.SetToMax();
			if (Determination < 0)
			{
				Determination.Current = 0;
			}

			if (Determination < Type.Maxdetermination)
			{
				Determination++;
			}

			if (Paniced)
			{
#if CLIENT
				if (WorldObject.IsVisible())
				{
					new PopUpText("Recovering From Panic", WorldObject.TileLocation.Position,Color.White);
				}
#endif


				Paniced = false;
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

		public void DoAction(Action.ActionType atype, Vector2Int target, List<string>? args =null)
		{
			if (args == null) args = new List<string>();
			
#if CLIENT
			if(WorldManager.Instance.SequenceRunning) return;
			if (!IsMyTeam()) return;
			if (!GameManager.IsMyTurn()) return;
#endif
			var a = Action.Actions[atype];
			var result = a.CanPerform(this, target,args);
			if (!result.Item1)
			{
#if CLIENT
				new PopUpText(result.Item2, WorldObject.TileLocation.Position,Color.White);
				new PopUpText(result.Item2, target,Color.White);
#else
				Console.WriteLine("tried to do action but failed: " + result.Item2);
#endif
				return;
			}
			Console.WriteLine("performing actionL " + a.Type + " on " + target);
#if CLIENT
			a.SendToServer(this, target,args);
			a.ExecuteClientSide(this, target,args);
#else
			a.PerformServerSide(this, target,args);
#endif


		}

		public bool Paniced { get; private set; }

		public void Panic()
		{

#if CLIENT
			if (WorldObject.IsVisible())
			{
				var t = new PopUpText("Panic!", WorldObject.TileLocation.Position,Color.Red);	
				t.scale = 2;
			}
#endif

			Crouching = true;
			Paniced = true;

			ClearOverWatch();

		}



		public Tuple<bool, int> Overwatch = new Tuple<bool, int>(false,-1);


		public List<Vector2Int> overWatchedTiles = new List<Vector2Int>();

		

		public void ClearOverWatch()
		{
			Overwatch = new Tuple<bool, int>(false,-1);
			foreach (var tile in overWatchedTiles)
			{
				((WorldTile)WorldManager.Instance.GetTileAtGrid(tile)).UnWatch(this);
			}

			overWatchedTiles.Clear();
		}

	

		public void EndTurn()
		{
			
			StatusEffects.RemoveAll(x => x.duration <= 0);

		}
		

		public void Update(float gameTime)
		{
		
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
			public Tuple<bool,int> Overwatch;
			public int MoveRangeEffect;
			public List<Vector2Int> OverWatchedTiles;

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
				Overwatch = new Tuple<bool, int>(false,-1);	
				StatusEffects = new List<Tuple<string, int>>();
				MoveRangeEffect = 0;
				OverWatchedTiles = new List<Vector2Int>();

			}
			public UnitData(Unit u)
			{
				Team1 = u.IsPlayer1Team;
				ActionPoints = u.ActionPoints.Current;
				MovePoints = u.MovePoints.Current;
				CanTurn = u.canTurn;
				Determination = u.Determination.Current;
				Crouching =	u.Crouching;
				JustSpawned = true;
				Panic = u.Paniced;
				
				StatusEffects = new List<Tuple<string, int>>();
				foreach (var st in u.StatusEffects)
				{
					StatusEffects.Add(new Tuple<string, int>(st.type.name,st.duration));
				}

				OverWatchedTiles = new List<Vector2Int>();
				OverWatchedTiles.AddRange(u.overWatchedTiles);
				Overwatch = u.Overwatch;

				MoveRangeEffect = u.MoveRangeEffect.Current;
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
				message.Add(Overwatch.Item1);
				message.Add(Overwatch.Item2);

				message.Add(MoveRangeEffect);

				message.AddSerializables(OverWatchedTiles.ToArray());
				
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
				Overwatch = new Tuple<bool, int>(message.GetBool(),message.GetInt());

				MoveRangeEffect = message.GetInt();

				OverWatchedTiles = new List<Vector2Int>(message.GetSerializables<Vector2Int>());
				
				StatusEffects = new List<Tuple<string, int>>();
				int count = message.GetInt();
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

		public void Suppress(int supression)
		{
			if(supression==0) return;
			
			Determination-= supression;
			if (Determination <= 0)
			{
				Panic();
			}

			if (Paniced && Determination > 0)
			{
				Paniced = false;
			}
			if(Determination>Type.Maxdetermination) Determination.Current = Type.Maxdetermination;
			if(Determination<0) Determination.Current = 0;
		}


		public int Health => WorldObject.Health;
		public ConcurrentDictionary<Vector2Int, Visibility> VisibleTiles = new ConcurrentDictionary<Vector2Int, Visibility>();


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
			var innerValue = innerField?.GetValue(value);
			Console.WriteLine("returning "+innerValue);
			return innerValue.ToString();

		}


		public string GetHash()
		{
			return WorldObject.GetHash();
		}

		public HashSet<Vector2Int> GetOverWatchPositions(Vector2Int target, int abilityIndex)
		{

			UnitAbility action = this.Abilities[(abilityIndex)];
			if(!action.CanOverWatch) return new HashSet<Vector2Int>();

			var tiles = WorldManager.Instance.GetTilesAround(target, action.OverWatchRange);
			HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
			foreach (var endTile in tiles)
			{
				WorldManager.RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(WorldObject.TileLocation.Position,endTile.Position,Cover.Full);
				foreach (var pos in outcome.Path)
				{
					positions.Add(pos);
				}

			}

			HashSet<Vector2Int> result = new HashSet<Vector2Int>();
			foreach (var position in positions)
			{
				if (action.CanPerform(this, position, false, true).Item1)
				{
					result.Add(position);
				}
			}

			return result;
		}
	}
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldActions.UnitAbility;
using DefconNull.WorldObjects.Units;
using Microsoft.Xna.Framework;
using Riptide;
using System.Collections;
using System.Linq;
using Action = DefconNull.WorldObjects.Units.Actions.Action;
#if CLIENT

using DefconNull.Rendering;
#endif

#nullable enable


namespace DefconNull.WorldObjects
{
	public partial class Unit
	{
		public WorldObject WorldObject { get; private set; }
		public UnitType Type { get; private set; }


		public Unit(WorldObject parent, UnitType type, UnitData data, bool justSpawned)
		{

			WorldObject = parent;
			Type = type;
			parent.UnitComponent = this;
			SetData(data,justSpawned);

		}

		public List<UnitAbility> Abilities => Type.actions;

		public void SetData(UnitData data, bool justSpawned)
		{
			IsPlayer1Team = data.Team1;
			Determination = new Value(0, Type.Maxdetermination);
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




			MovePoints = new Value(0, Type.MaxMovePoints);
			if (data.MovePoints != -100)
			{
				MovePoints.Current = data.MovePoints;
			}

			ActionPoints = new Value(0, Type.MaxActionPoints);
			if (data.ActionPoints != -100)
			{
				ActionPoints.Current = data.ActionPoints;
			}


			canTurn = data.CanTurn;


			MoveRangeEffect.Current = data.MoveRangeEffect;
            
			StatusEffects.Clear();
			foreach (var effect in data.StatusEffects)//dont apply them since their aplication would already be considered by the rest of the data
			{
				StatusEffects.Add(new StatusEffectInstance(PrefabManager.StatusEffects[effect.Item1],effect.Item2,this));
			}
			

			ClearOverWatch();
			Overwatch = data.Overwatch;
			if(Overwatch.Item1 && Overwatch.Item2 == -1) throw new Exception("overwatch is active but no ability is selected");
			foreach (var t in data.OverWatchedTiles)
			{
				overWatchedTiles.Add(t);
				WorldManager.Instance.GetTileAtGrid(t).Watch(this);
			}
            
			if (justSpawned)
			{

#if SERVER
				if (Type.SpawnEffect != null)
				{
					Task t = new Task(delegate
					{
						foreach (var c in Type.SpawnEffect.GetApplyConsequnces(WorldObject,WorldObject))
						{
							NetworkingManager.SendSequence(c);
						}
					});
					SequenceManager.RunNextAfterFrames(t);
				}
#endif
				
				StartTurn();
			}

		}

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

		public List<(Vector2Int,PathFinding.PathFindResult)>[] GetPossibleMoveLocations(int moveRange = -1, int moveOverride = -1, bool generatePaths = false)
		{
			int mp = MovePoints.Current;
			if(moveOverride!=-1) mp = moveOverride;
			if (mp > 0)
			{
				if(moveRange==-1) moveRange = GetMoveRange();
				List<(Vector2Int,PathFinding.PathFindResult)>[] possibleMoves = new List<(Vector2Int,PathFinding.PathFindResult)>[mp];
				for (int i = 0; i < mp; i++)
				{
					possibleMoves[i] = PathFinding.GetAllPaths(WorldObject.TileLocation.Position, moveRange * (i + 1),generatePaths);
				}

				for (int i = mp - 1; i > 0; i--)
				{
					possibleMoves[i].RemoveAll(x => possibleMoves[i - 1].Exists(y => y.Item1 == x.Item1));
				}
				return possibleMoves;
			}

			return Array.Empty<List<(Vector2Int,PathFinding.PathFindResult)>>();
            

		}

		public bool IsPlayer1Team { get; private set; }
		
		

		public int GetSightRange()
		{
			//apply effects and offests
			return Type.SightRange;
		}

		public (int, int, int) GetPointsNextTurn()
		{
			Value mp = MovePoints;
			mp.SetToMax();
			Value ap = ActionPoints;
			ap.SetToMax();
			Value det = Determination;
			
			if(Paniced) mp--;
			else det.Current++;
			
			foreach (var st in new List<StatusEffectInstance>(StatusEffects))
			{
				if(st.Duration<=0) continue;
				//incorrect if status effect doesnt actually affect this unit but whatever
				var cons = st.Type.Conseqences.GetApplyConsequnces(WorldObject,WorldObject);
				List<ChangeUnitValues> vals =cons.FindAll(x => x is ChangeUnitValues).ConvertAll(x => (ChangeUnitValues) x);
				foreach (var v in vals)
				{
					v.MoveChange.Apply(ref mp);
					v.ActChange.Apply(ref ap);
					v.DetChange.Apply(ref det);
				}
			}

			return (mp.Current, ap.Current,det.Current);
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
				effect.Apply();
			}


		}

		public void DoOverwatch(Vector2Int tile, int ability)
		{
			Action.ActionExecutionParamters args = new Action.ActionExecutionParamters(tile);
			args.AbilityIndex = ability;
			DoAction(Action.ActionType.OverWatch,args);
		}
		public void DoAbility(WorldObject target, int ability)
		{
			Action.ActionExecutionParamters args = new Action.ActionExecutionParamters(target.TileLocation.Position);
			args.TargetObj = target;
			args.AbilityIndex = ability;
			DoAction(Action.ActionType.UseAbility,args);
		}
		
		public void DoAction(Action.ActionType type, Action.ActionExecutionParamters args)
		{
			
#if CLIENT
			if(SequenceManager.SequenceRunning) return;
			if (!GameManager.IsMyTurn()) return;
#endif
			if (Overwatch.Item1) return;
			if (IsPlayer1Team != GameManager.IsPlayer1Turn) return;
			
			var a = Action.Actions[type];
			var result = a.CanPerform(this, args);
			
			if (!result.Item1)
			{
#if CLIENT
				if(args.Target.HasValue){
					new PopUpText(result.Item2, args.Target.Value,Color.White);
				}
#else
				Log.Message("UNITS","tried to do action but failed: " + result.Item2);
#endif
				return;
			}
			
			Log.Message("UNITS","performing action " + a.Type);
#if CLIENT
			a.SendToServer(this,args);
#elif SERVER
			a.PerformServerSide(this,args);
#endif


		}

		public bool Paniced { get; set; }

		public void Panic()
		{

			
#if CLIENT
			if(!Paniced){
				if (WorldObject.IsVisible())
				{
					var t = new PopUpText("Panic!", WorldObject.TileLocation.Position,Color.Red);	
					t.scale = 2;
				}

			}
#endif

			Crouching = true;
			Paniced = true;

			ClearOverWatch();

		}



		public ValueTuple<bool, int> Overwatch = new ValueTuple<bool, int>(false,-1);


		public List<Vector2Int> overWatchedTiles = new List<Vector2Int>();

		

		public void ClearOverWatch()
		{
			Overwatch = new ValueTuple<bool, int>(false,-1);
			foreach (var tile in overWatchedTiles)
			{
				((WorldTile)WorldManager.Instance.GetTileAtGrid(tile)).UnWatch(this);
			}

			overWatchedTiles.Clear();
		}

	

		public void EndTurn()
		{
			
			StatusEffects.RemoveAll(x => x.Duration <= 0);

		}
		

		public void Update(float gameTime)
		{
		
		}
		[Serializable]
		public struct UnitData : IMessageSerializable
		{
	
			public override string ToString()
			{
				return $"{nameof(Team1)}: {Team1}, {nameof(ActionPoints)}: {ActionPoints}, {nameof(MovePoints)}: {MovePoints}, {nameof(CanTurn)}: {CanTurn}, {nameof(Determination)}: {Determination}, {nameof(Crouching)}: {Crouching}, {nameof(Panic)}: {Panic}, {nameof(Overwatch)}: {Overwatch}, {nameof(MoveRangeEffect)}: {MoveRangeEffect}, {nameof(OverWatchedTiles)}: {OverWatchedTiles}, {nameof(StatusEffects)}: {StatusEffects}";
			}

			public bool Team1;
			public int ActionPoints;
			public int MovePoints;
			public bool CanTurn;

			public bool Equals(UnitData other)
			{
				return Team1 == other.Team1 && ActionPoints == other.ActionPoints && MovePoints == other.MovePoints && CanTurn == other.CanTurn && Determination == other.Determination && Crouching == other.Crouching && Panic == other.Panic && Overwatch.Equals(other.Overwatch) && MoveRangeEffect == other.MoveRangeEffect && OverWatchedTiles.SequenceEqual(other.OverWatchedTiles) && StatusEffects.SequenceEqual(other.StatusEffects);
			}

			public override bool Equals(object? obj)
			{
				return obj is UnitData other && Equals(other);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					int hashCode = Team1.GetHashCode();
					hashCode = (hashCode * 397) ^ ActionPoints;
					hashCode = (hashCode * 397) ^ MovePoints;
					hashCode = (hashCode * 397) ^ CanTurn.GetHashCode();
					hashCode = (hashCode * 397) ^ Determination;
					hashCode = (hashCode * 397) ^ Crouching.GetHashCode();
					hashCode = (hashCode * 397) ^ Panic.GetHashCode();
					hashCode = (hashCode * 397) ^ Overwatch.GetHashCode();
					hashCode = (hashCode * 397) ^ MoveRangeEffect;
					hashCode = (hashCode * 397) ^ OverWatchedTiles.GetHashCode();
					hashCode = (hashCode * 397) ^ StatusEffects.GetHashCode();
					return hashCode;
				}
			}

			public int Determination;
			public bool Crouching;
			public bool Panic;
			public ValueTuple<bool,int> Overwatch;
			public int MoveRangeEffect;
			public List<Vector2Int> OverWatchedTiles = new List<Vector2Int>();

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
				Overwatch = new ValueTuple<bool, int>(false,-1);	
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
				Panic = u.Paniced;
				
				StatusEffects = new List<Tuple<string, int>>();
				foreach (var st in u.StatusEffects)
				{
					StatusEffects.Add(new Tuple<string, int>(st.Type.Name,st.Duration));
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
				Overwatch = new ValueTuple<bool, int>(message.GetBool(),message.GetInt());

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
			return new UnitData(this);
		
		}

		protected bool Equals(Unit other)
		{
			return Abilities.Equals(other.Abilities) && MovePoints.Equals(other.MovePoints) && ActionPoints.Equals(other.ActionPoints) && Determination.Equals(other.Determination) && MoveRangeEffect.Equals(other.MoveRangeEffect) && Overwatch.Equals(other.Overwatch) && overWatchedTiles.Equals(other.overWatchedTiles) && StatusEffects.Equals(other.StatusEffects) && VisibleTiles.Equals(other.VisibleTiles) && WorldObject.Equals(other.WorldObject) && Type.Equals(other.Type) && canTurn == other.canTurn && Crouching == other.Crouching && IsPlayer1Team == other.IsPlayer1Team && Paniced == other.Paniced;
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
			unchecked
			{
				int hashCode = Abilities.GetHashCode();
				hashCode = (hashCode * 397) ^ MovePoints.GetHashCode();
				hashCode = (hashCode * 397) ^ ActionPoints.GetHashCode();
				hashCode = (hashCode * 397) ^ Determination.GetHashCode();
				hashCode = (hashCode * 397) ^ MoveRangeEffect.GetHashCode();
				hashCode = (hashCode * 397) ^ Overwatch.GetHashCode();
				hashCode = (hashCode * 397) ^ overWatchedTiles.GetHashCode();
				hashCode = (hashCode * 397) ^ StatusEffects.GetHashCode();
				hashCode = (hashCode * 397) ^ VisibleTiles.GetHashCode();
				hashCode = (hashCode * 397) ^ Type.GetHashCode();
				hashCode = (hashCode * 397) ^ canTurn.GetHashCode();
				hashCode = (hashCode * 397) ^ Crouching.GetHashCode();
				hashCode = (hashCode * 397) ^ IsPlayer1Team.GetHashCode();
				hashCode = (hashCode * 397) ^ Paniced.GetHashCode();
				return hashCode;
			}
		}


		public List<StatusEffectInstance> StatusEffects = new List<StatusEffectInstance>();
		

		public void ApplyStatus(string? effect, int duration)
		{
			var statuseffect = new StatusEffectInstance(PrefabManager.StatusEffects[effect],duration,this);
			StatusEffects.Add(statuseffect);
			statuseffect.Apply();
		}

		public void RemoveStatus(string effectName)
		{
			StatusEffects.RemoveAll(x => x.Type.Name == effectName);
		}
		
		
		public int Health => WorldObject.Health;
		public ConcurrentDictionary<Vector2Int, Visibility> VisibleTiles = new ConcurrentDictionary<Vector2Int, Visibility>();
		



		public HashSet<Vector2Int> GetOverWatchPositions(Vector2Int target, int abilityIndex)
		{

			UnitAbility action = Abilities[abilityIndex];
			if(!action.CanOverWatch) return new HashSet<Vector2Int>();

			var tiles = WorldManager.Instance.GetTilesAround(target, action.OverWatchRange);
			HashSet<Vector2Int> positions = new HashSet<Vector2Int>();
			foreach (var endTile in tiles)
			{
				WorldManager.RayCastOutcome outcome = WorldManager.Instance.CenterToCenterRaycast(WorldObject.TileLocation.Position,endTile.Position,Cover.Full,makePath: true);

				foreach (var p in outcome.Path)
				{
					positions.Add(p);
				}
	

			}

			HashSet<Vector2Int> result = new HashSet<Vector2Int>();
			foreach (var position in positions)
			{	
				var tile = WorldManager.Instance.GetTileAtGrid(position);
				if(tile.Surface==null) continue;
				if (action.CanPerform(this, tile.Surface, false, true).Item1)
				{
					result.Add(position);
				}
			}

			return result;
		}

		public void MoveTo(Vector2Int vector2Int)
		{
			if(WorldObject.TileLocation.Position == vector2Int) return;
			Log.Message("UNITS","units re-located from "+WorldObject.TileLocation.Position+" to "+vector2Int);
			var oldtile = WorldObject.TileLocation;
			oldtile.UnitAtLocation = null;
			var newTile = WorldManager.Instance.GetTileAtGrid(vector2Int);

#if SERVER
		if(newTile.IsVisible(WorldObject.GetMinimumVisibility(),team1: true)||((WorldTile)oldtile).IsVisible(WorldObject.GetMinimumVisibility(),true))
		{
			Log.Message("UNITS","moving for player 1 "+WorldObject.TileLocation.Position+" to "+vector2Int);
			if (GameManager.Player1UnitPositions.ContainsKey(WorldObject.ID)) GameManager.Player1UnitPositions.Remove(WorldObject.ID);
	
			GameManager.Player1UnitPositions[WorldObject.ID] = (newTile.Position, WorldObject.GetData());
		}
		if(newTile.IsVisible(WorldObject.GetMinimumVisibility(),team1: false)||((WorldTile)oldtile).IsVisible(WorldObject.GetMinimumVisibility(),team1: false))
		{
			Log.Message("UNITS","moving for player 2 "+WorldObject.TileLocation.Position+" to "+vector2Int);
			if (!GameManager.Player2UnitPositions.ContainsKey(WorldObject.ID)) GameManager.Player2UnitPositions.Remove(WorldObject.ID);
			
			GameManager.Player2UnitPositions[WorldObject.ID] = (newTile.Position, WorldObject.GetData());
		}
#endif
			
			
			WorldObject.TileLocation = newTile;
			newTile.UnitAtLocation = this;
		}

		
	}
}
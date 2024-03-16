#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull;

public partial class WorldTile : IWorldTile
{
	private readonly Vector2Int _position;

	public Vector2Int Position => _position;
	public WorldTile(Vector2Int position)
	{
		_position = position;

	}
	
	public static readonly object Syncobj = new object();

	private readonly List<Unit> _watchers = new List<Unit>();

	public List<Unit> GetOverWatchShooters(Unit target,Visibility requiredVis)
	{
		List<Unit> shooters = new List<Unit>();
		//Console.WriteLine("getting overwatch shooters");

		foreach (var watcher in _watchers)
		{
			
			bool isFriendly = watcher.IsPlayer1Team == target.IsPlayer1Team;

			Visibility vis = WorldManager.Instance.CanTeamSee(Position, watcher.IsPlayer1Team);


			Console.WriteLine("overwatch spotted by " + watcher.WorldObject.TileLocation.Position + " is friendly: " + isFriendly + " vis: " + vis);
			if (!isFriendly && watcher.Abilities[watcher.Overwatch.Item2].CanPerform(watcher,Surface!,false,true).Item1 && vis >= requiredVis)
			{
				shooters.Add(watcher);
			}
		}

		return shooters;
	}

	

	public void Watch(Unit watcher)
	{
		lock (Syncobj)
		{
			_watchers.Add(watcher);
		}
#if CLIENT
		CalcWatchLevel();
#endif
	}
	public void UnWatch(Unit watcher)
	{
		lock (Syncobj)
		{
			_watchers.Remove(watcher);
		}
#if CLIENT
		CalcWatchLevel();
#endif

	}

		

	public void Update(float msDelta)
	{
		lock (Syncobj)
		{
			NorthEdge?.Update(msDelta);
			WestEdge?.Update(msDelta);
			UnitAtLocation?.WorldObject.Update(msDelta);
			Surface?.Update(msDelta);
			foreach (var obj  in ObjectsAtLocation)
			{
				obj.Update(msDelta);
			}
		
		}	
	}


#if SERVER
	public (Visibility, Visibility) TileVisibility {private get; set;} = new ValueTuple<Visibility, Visibility>(Visibility.None,Visibility.None);
#else
	public Visibility TileVisibility {private get; set;} = Visibility.None;
#endif

	public void SetVisibility(bool unitIsPlayer1Team, Visibility visTupleValue)
	{
#if SERVER
		if (unitIsPlayer1Team)
		{
			TileVisibility = new ValueTuple<Visibility, Visibility>(visTupleValue, TileVisibility.Item2);
		}
		else
		{
			TileVisibility = new ValueTuple<Visibility, Visibility>(TileVisibility.Item1, visTupleValue);
		}
#else
		if(GameManager.IsPlayer1 != unitIsPlayer1Team) return;
		TileVisibility = visTupleValue;
#endif	
	}
	
	public Visibility GetVisibility(bool? team1 = null)
	{
		
		Visibility vis;
#if SERVER
		if(team1 is null){
			throw new Exception("requestted visibility wihtout specifying team");
		}
		if (team1.Value)
		{  
			vis = TileVisibility.Item1;
		}else{
			vis = TileVisibility.Item2;
		}
#else
		vis = TileVisibility;
#endif
		return vis;
	}
	public bool IsVisible(Visibility minimum = Visibility.Partial, bool? team1 = null)
	{
		if (GetVisibility(team1) >= minimum)
		{
			return true;
		}

		return false;
	}

	public bool Traversible(Vector2Int from, bool ignoreControllables = false)
	{
		
		if (Surface == null) return false;
		if (!ignoreControllables && UnitAtLocation != null) return false;
		if (Surface != null && Surface.Type.Impassible) return false;
		Cover obstacle =WorldManager.Instance.GetCover(from,Utility.Vec2ToDir(new Vector2Int(_position.X - from.X, _position.Y - from.Y)),ignoreControllables:ignoreControllables);
		if (obstacle > Cover.High) return false;

		foreach (var obj in ObjectsAtLocation)
		{
			if(obj.GetCover(false) > Cover.None) return false;
		}
		return true;

	}
	public double TraverseCostFrom(Vector2Int from)
	{

		var dist = Utility.Distance(from, _position);
		if (dist == 0) return 0;
		Cover obstacle = WorldManager.Instance.GetCover(from,Utility.Vec2ToDir(new Vector2Int(_position.X - from.X, _position.Y - from.Y)),ignoreControllables:true);
		//return dist;
		if (obstacle == Cover.None) return dist;
		if (obstacle == Cover.Low) return dist + 1;
		if (obstacle == Cover.High) return dist + 5;
		if (obstacle == Cover.Full) return dist + 5;
		if (obstacle == Cover.Full)
		{
			throw new Exception("cannot traverse full cover");
		}
#pragma warning disable CS0472
		if (obstacle == null)
#pragma warning restore CS0472
		{
			throw new Exception("ERROR: null obstacle");
		}
		
		throw new Exception("how did we get here");

	}


	private WorldObject? _northEdge;
	public WorldObject? NorthEdge{
		get => _northEdge;
		set
		{
			if (value == null)
			{
				_northEdge = null;
				return;
			}

			if (value != null && (!value.Type.Edge || value.Type.Surface))
				throw new Exception("attempted non edge to edge");
			if (_northEdge != null && !_northEdge.destroyed)
			{
				Log.Message("WARNINGS",$"attempted to place an object({value.ID}) over an existing({_northEdge.ID}) one");
			}

			_northEdge = value;
		}
	}
	private WorldObject? _westEdge;



	public WorldObject? WestEdge 	{
		get => _westEdge;
		set
		{
			if (value == null)
			{
				_westEdge = null;
				return;
			}
			if (value != null && (!value.Type.Edge || value.Type.Surface))
				throw new Exception("attempted non edge to edge");
			if (_westEdge != null && !_westEdge.destroyed)
			{
				Log.Message("WARNINGS",$"attempted to place an object({value.ID}) over an existing({_westEdge.ID}) one");
			}

			_westEdge = value;
		}
	}
	public WorldObject? SouthEdge 	{
		get
		{
			if (WorldManager.IsPositionValid(_position + new Vector2Int(0, 1)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(_position + new Vector2Int(0, 1));
				return tile.NorthEdge;
			}

			return null;
		}
		set
		{
			if (WorldManager.IsPositionValid(_position + new Vector2Int(0, 1)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(_position + new Vector2Int(0, 1));
				tile.NorthEdge = value;
			}
		}
	}
	public WorldObject? EastEdge {
		get
		{
			if (WorldManager.IsPositionValid(_position + new Vector2Int(1, 0)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(_position + new Vector2Int(1, 0));
				return tile.WestEdge;
			}

			return null;
		}
		set
		{
			if (WorldManager.IsPositionValid(_position + new Vector2Int(1, 0)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(_position + new Vector2Int(1, 0));
				tile.WestEdge = value;
			}
		}
	}

	public List<WorldObject> ObjectsAtLocation { get; private set; } = new List<WorldObject>();
	public void PlaceObject(WorldObject obj)
	{
			
		if (obj.Type.Edge || obj.Type.Surface)
			throw new Exception("attempted to set a surface or edge to the main location");
			
		ObjectsAtLocation.Add(obj);
	}
	public void RemoveObject(WorldObject obj)
	{
		ObjectsAtLocation.Remove(obj);
	}

	private Unit? _unitAtLocation;
	public Unit? UnitAtLocation
	{
		get => _unitAtLocation;
		set
		{
				 
			if (value == null)
			{
				_unitAtLocation = null;
				return;
			}
			if (_unitAtLocation != null)
			{
				throw new Exception("attempted to place a Unit over an existing one");
			}

			_unitAtLocation = value;
		}
	}




	private WorldObject? _surface;
	public bool IsOverwatched => _watchers.Count > 0;


	public WorldObject? Surface{
		get => _surface;
		set
		{
			if (value == null)
			{
				_surface = null;
			}
			if (value != null && (value.Type.Edge || !value.Type.Surface))
				throw new Exception("attempted to set a nonsurface to surface");
			if (_surface != null)
			{
				Console.Write("overwritting surface");
				WorldObjectManager.DeleteWorldObject.Make(_surface.ID).GenerateTask().RunTaskSynchronously();
			}
			_surface = value;
		}
	}
		
	public void Wipe(bool full360 = false)
	{
		if (NorthEdge != null) 	SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(NorthEdge.ID));
		if (WestEdge != null) SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(WestEdge.ID));
		if (full360)
		{
			if (SouthEdge != null) SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(SouthEdge.ID));
			if (EastEdge != null) SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(EastEdge.ID));
		}
		if (UnitAtLocation != null) SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(UnitAtLocation.WorldObject.ID));
		if (Surface != null) SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(Surface.ID));
		foreach (var obj in ObjectsAtLocation)
		{
			SequenceManager.AddSequence(WorldObjectManager.DeleteWorldObject.Make(obj.ID));
		}
		_watchers.Clear();
		


	}

	public void Remove(int id)
	{
		if (NorthEdge != null && NorthEdge.ID == id)
		{
			NorthEdge = null;
		}
		if (WestEdge != null && WestEdge.ID == id)
		{
			WestEdge = null;
		}
		if (UnitAtLocation != null && UnitAtLocation.WorldObject.ID == id)
		{
			UnitAtLocation = null;
		}
		if (Surface != null && Surface.ID == id)
		{
			Surface = null;
		}

		foreach (var obj in ObjectsAtLocation)
		{
			if (obj.ID == id)
			{
				ObjectsAtLocation.Remove(obj);
				break;
			}
		}
	}
	[Serializable]
	public struct WorldTileData : IMessageSerializable{
		public override string ToString()
		{
			return $"{nameof(NorthEdge)}: {NorthEdge}, {nameof(WestEdge)}: {WestEdge}, {nameof(EastEdge)}: {EastEdge}, {nameof(SouthEdge)}: {SouthEdge}, {nameof(Surface)}: {Surface}, {nameof(Position)}: {Position}, {nameof(ObjectsAtLocation)}: {ObjectsAtLocation}";
		}

		public bool Equals(WorldTileData other)
		{
			return Nullable.Equals(NorthEdge, other.NorthEdge) && Nullable.Equals(WestEdge, other.WestEdge) && Nullable.Equals(EastEdge, other.EastEdge) && Nullable.Equals(SouthEdge, other.SouthEdge) && Nullable.Equals(Surface, other.Surface) && Position.Equals(other.Position) && ObjectsAtLocation.SequenceEqual(other.ObjectsAtLocation);
		}

		public override bool Equals(object? obj)
		{
			return obj is WorldTileData other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(NorthEdge, WestEdge, EastEdge, SouthEdge, Surface, Position, ObjectsAtLocation);
		}

		public WorldObject.WorldObjectData? NorthEdge;
		public WorldObject.WorldObjectData? WestEdge;
		public WorldObject.WorldObjectData? EastEdge;
		public WorldObject.WorldObjectData? SouthEdge;
		public WorldObject.WorldObjectData? Surface;
		public Vector2Int Position;
		public List<WorldObject.WorldObjectData> ObjectsAtLocation;
		public WorldTileData(Vector2Int position)
		{
			Position = position;
			NorthEdge = null;
			WestEdge = null;
			Surface = null;
			ObjectsAtLocation = new List<WorldObject.WorldObjectData>();
		}
		

		public void Serialize(Message message)
		{
			
			//cast to non-nullables before serialising
			message.AddSerializable(Position);
			bool b = NorthEdge != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(NorthEdge!.Value);
	
			b = WestEdge != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(WestEdge!.Value);
			
			b = EastEdge != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(EastEdge!.Value);
			
			b = SouthEdge != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(SouthEdge!.Value);
			
			b = Surface != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(Surface!.Value);
			
			
			message.AddSerializables(ObjectsAtLocation.ToArray());
			
            

		}

		public void Deserialize(Message message)
		{
			
			Position = message.GetSerializable<Vector2Int>();
			
			if (message.GetBool()) NorthEdge = message.GetSerializable<WorldObject.WorldObjectData>();
			else NorthEdge = null;
			
			if (message.GetBool()) WestEdge = message.GetSerializable<WorldObject.WorldObjectData>();
			else WestEdge = null;
			
			if (message.GetBool()) EastEdge = message.GetSerializable<WorldObject.WorldObjectData>();
			else EastEdge = null;
			
			if (message.GetBool()) SouthEdge = message.GetSerializable<WorldObject.WorldObjectData>();
			else SouthEdge = null;

			if (message.GetBool()) Surface = message.GetSerializable<WorldObject.WorldObjectData>();
			else Surface = null;
			


			ObjectsAtLocation = message.GetSerializables<WorldObject.WorldObjectData>().ToList();
		}

		public string GetHash()
		{

			string hash = "";
			if (NorthEdge.HasValue)
			{
				hash += NorthEdge.Value.GetHash();
			}
			if (WestEdge.HasValue)
			{
				hash += WestEdge.Value.GetHash();
			}
			if (Surface.HasValue)
			{
				hash += Surface.Value.GetHash();
			}
			foreach (var obj in ObjectsAtLocation)
			{
				hash += obj.GetHash();
			}

			return hash;
			
		}
	}
	public WorldTileData GetData(bool forceJustSpawned = false)
	{
		var data = new WorldTileData(_position);
		if (Surface != null)
		{
			data.Surface = Surface.GetData(forceJustSpawned);
		}
		if (NorthEdge != null)
		{
			data.NorthEdge = NorthEdge.GetData(forceJustSpawned);
		}
		if (WestEdge != null)
		{
			data.WestEdge = WestEdge.GetData(forceJustSpawned);
		}
		if (EastEdge != null)
		{
			data.EastEdge = EastEdge.GetData(forceJustSpawned);
		}
		if (SouthEdge != null)
		{
			data.SouthEdge = SouthEdge.GetData(forceJustSpawned);
		}

		data.ObjectsAtLocation = new List<WorldObject.WorldObjectData>();
		foreach (var obj in ObjectsAtLocation)
		{
			data.ObjectsAtLocation.Add(obj.GetData(forceJustSpawned));
		}

	
		return data;
	}


	public List<WorldObject> GetAllEdges()
	{
		
		List<WorldObject> edges = new List<WorldObject>();
		if (NorthEdge is not null)
		{
			edges.Add(NorthEdge);
		}
		if (WestEdge is not null)
		{
			edges.Add(WestEdge);
		}
			
		if (WorldManager.IsPositionValid(_position + new Vector2Int(0, 1)))
		{
			var tile = WorldManager.Instance.GetTileAtGrid(_position + new Vector2Int(0, 1));
			if (tile.NorthEdge is not null)
			{
				edges.Add(tile.NorthEdge);
			}

		}

		if (WorldManager.IsPositionValid(_position + new Vector2Int(1, 0)))
		{
			var tile = WorldManager.Instance.GetTileAtGrid(_position + new Vector2Int(1, 0));
			if (tile.WestEdge is not null)
			{
				edges.Add(tile.WestEdge);
			}
		}
			
		return edges;
	}


	~WorldTile()
	{
		Console.WriteLine("TILE DELETED!: "+_position);
	}

	public void NextTurn(bool team1Turn)
	{
		foreach (var item in new List<WorldObject>(ObjectsAtLocation))
		{
			item.NextTurn();
		}
		WestEdge?.NextTurn();
		NorthEdge?.NextTurn();

		if (UnitAtLocation != null && UnitAtLocation.IsPlayer1Team == team1Turn)
		{
			UnitAtLocation.StartTurn();
		}
		if (UnitAtLocation != null && UnitAtLocation.IsPlayer1Team != team1Turn)
		{
			UnitAtLocation.EndTurn();
		}

		UnitAtLocation?.WorldObject.NextTurn();
		Surface?.NextTurn();
	}



	
	
}
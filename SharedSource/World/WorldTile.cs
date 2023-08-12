#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.World.WorldObjects;
using Riptide;

namespace DefconNull.World;

public partial class WorldTile
{
	public readonly Vector2Int Position;

	public WorldTile(Vector2Int position)
	{
		Position = position;
	}
	public static readonly object syncobj = new object();

	private List<Unit> Watchers = new List<Unit>();
	private List<Unit> UnWatchQueue = new List<Unit>();
	private int HighestWatchLevel;
#if SERVER
	public List<Unit> GetOverWatchShooters(Unit actor,Visibility requiredVis)
	{
		List<Unit> shooters = new List<Unit>();
		foreach (var watcher in Watchers)
		{
			
			bool isFriendly = watcher.IsPlayerOneTeam == actor.IsPlayerOneTeam;
			//make this "can player see" fucntion
			List<int> units;
			if (watcher.IsPlayerOneTeam)
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
					var tempVis = WorldManager.Instance.CanSee(WO.UnitComponent!, Position);
					if (tempVis > vis)
					{
						vis = tempVis;
					}
				} 

			}

			Console.WriteLine("overwatch spotted by " + watcher.WorldObject.TileLocation.Position + " is friendly: " + isFriendly + " vis: " + vis);
			if (!isFriendly && watcher.Type.DefaultAttack.CanHit(watcher,Position,actor.Crouching) && vis >= requiredVis)
			{
				shooters.Add(watcher);
			}
		}

		return shooters;
	}
#endif
	

	public void Watch(Unit watcher)
	{
		lock (syncobj)
		{
			Watchers.Add(watcher);
		}
#if CLIENT
		CalcWatchLevel();
#endif
	}
	public void UnWatch(Unit watcher)
	{
		lock (syncobj)
		{
			UnWatchQueue.Add(watcher);
		}


	}

		

	public void Update(float delta)
	{
		lock (syncobj)
		{
#pragma warning disable CS0219
			bool recalcflag = false;
#pragma warning restore CS0219
			foreach (var Watcher in UnWatchQueue)
			{
				recalcflag = true;
				Watchers.Remove(Watcher);
			}
			UnWatchQueue.Clear();
#if CLIENT
			if (recalcflag)
			{
				CalcWatchLevel();
			}
#endif

		}	
	}

	public bool Traversible(Vector2Int from)
	{
		if (Surface == null) return false;
		if (UnitAtLocation != null) return false;
		if (Surface != null && Surface.Type.Impassible) return false;
		Cover obstacle =WorldManager.Instance.GetTileAtGrid(from).GetCover(Utility.Vec2ToDir(new Vector2Int(Position.X - from.X, Position.Y - from.Y)));
		if (obstacle > Cover.High) return false;
		return true;

	}
	public double TraverseCostFrom(Vector2Int from)
	{

		var dist = Utility.Distance(from, Position);
		if (dist == 0) return 0;
		Cover obstacle = WorldManager.Instance.GetTileAtGrid(from).GetCover(Utility.Vec2ToDir(new Vector2Int(Position.X - from.X, Position.Y - from.Y)));
		if (obstacle == Cover.None) return dist;
		if (obstacle == Cover.Low) return dist + 1;
		if (obstacle == Cover.High) return dist + 5;
		if (obstacle == Cover.Full)
		{
			throw new Exception("cannot traverse full cover");
		}
		if (obstacle == null)
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
			}

			if (value != null && (!value.Type.Edge || value.Type.Surface))
				throw new Exception("attempted non edge to edge");
			if (_northEdge != null)
			{
				throw new Exception("attempted to place an object over an existing one");
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
			}
			if (value != null && (!value.Type.Edge || value.Type.Surface))
				throw new Exception("attempted non edge to edge");
			if (_westEdge != null)
			{
				throw new Exception("attempted to place an object over an existing one");
			}

			_westEdge = value;
		}
	}
	public WorldObject? SouthEdge 	{
		get
		{
			if (WorldManager.IsPositionValid(Position + new Vector2Int(0, 1)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(Position + new Vector2Int(0, 1));
				return tile.NorthEdge;
			}

			return null;
		}
		set
		{
			if (WorldManager.IsPositionValid(Position + new Vector2Int(0, 1)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(Position + new Vector2Int(0, 1));
				tile.NorthEdge = value;
			}
		}
	}
	public WorldObject? EastEdge {
		get
		{
			if (WorldManager.IsPositionValid(Position + new Vector2Int(1, 0)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(Position + new Vector2Int(1, 0));
				return tile.WestEdge;
			}

			return null;
		}
		set
		{
			if (WorldManager.IsPositionValid(Position + new Vector2Int(1, 0)))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(Position + new Vector2Int(1, 0));
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
				throw new Exception("attempted to place an object over an existing one");
			}
			_surface = value;
		}
	}
		
	public void Wipe()
	{

		WorldManager.Instance.DeleteWorldObject(NorthEdge);
		WorldManager.Instance.DeleteWorldObject(WestEdge);
		WorldManager.Instance.DeleteWorldObject(UnitAtLocation?.WorldObject);
		WorldManager.Instance.DeleteWorldObject(Surface);
		foreach (var obj in ObjectsAtLocation)
		{
			WorldManager.Instance.DeleteWorldObject(obj);
		}


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
	public struct WorldTileData : IMessageSerializable
	{
		public WorldObject.WorldObjectData? NorthEdge;
		public WorldObject.WorldObjectData? WestEdge;
		public WorldObject.WorldObjectData? Surface;
		public WorldObject.WorldObjectData? UnitAtLocation;
		public Vector2Int position;
		public List<WorldObject.WorldObjectData> ObjectsAtLocation;
		public WorldTileData(Vector2Int position)
		{
			this.position = position;
			NorthEdge = null;
			WestEdge = null;
			Surface = null;
			UnitAtLocation = null;
			ObjectsAtLocation = new List<WorldObject.WorldObjectData>();
		}

		public void Serialize(Message message)
		{
			
			//cast to non-nullables before serialising
			message.AddSerializable(position);
			
			bool b = NorthEdge != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(NorthEdge!.Value);
	
			b = WestEdge != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(WestEdge!.Value);
			
			b = Surface != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(Surface!.Value);

			b = UnitAtLocation != null;
			message.AddBool(b);
			if (b) 	message.AddSerializable(UnitAtLocation!.Value);
			
			
			message.AddSerializables(ObjectsAtLocation.ToArray());
			



		}

		public void Deserialize(Message message)
		{
			
			position = message.GetSerializable<Vector2Int>();
		


			
			if (message.GetBool())
			{
				NorthEdge = message.GetSerializable<WorldObject.WorldObjectData>();
			}
			else
			{
				NorthEdge = null;
			}
			if (message.GetBool())
			{
				WestEdge = message.GetSerializable<WorldObject.WorldObjectData>();
			}
			else
			{
				WestEdge = null;
			}
			
			if (message.GetBool())
			{
				Surface = message.GetSerializable<WorldObject.WorldObjectData>();
			}
			else
			{
				Surface = null;
			}
			if (message.GetBool())
			{
				UnitAtLocation = message.GetSerializable<WorldObject.WorldObjectData>();
			}
			else
			{
				UnitAtLocation = null;
			}

			ObjectsAtLocation = message.GetSerializables<WorldObject.WorldObjectData>().ToList();
		}
	}
	public WorldTileData GetData()
	{
		var data = new WorldTileData(Position);
		if (Surface != null)
		{
			data.Surface = Surface.GetData();
		}
		if (NorthEdge != null)
		{
			data.NorthEdge = NorthEdge.GetData();
		}
		if (WestEdge != null)
		{
			data.WestEdge = WestEdge.GetData();
		}
		if (UnitAtLocation != null)
		{
			data.UnitAtLocation = UnitAtLocation.WorldObject.GetData();
		}
		data.ObjectsAtLocation = ObjectsAtLocation.Select(x => x.GetData()).ToList();

		return data;
	}

	public Cover GetCover(Direction dir, bool ignoreControllables = false)
	{
		GetCoverObj(dir);

		return GetCoverObj(dir,ignoreControllables).GetCover();
	}
	public List<WorldObject> GetAllEdges()
	{
		List<WorldObject> edges = new List<WorldObject>();
		if (NorthEdge != null)
		{
			edges.Add(NorthEdge);
		}
		if (WestEdge != null)
		{
			edges.Add(WestEdge);
		}
			
		if (WorldManager.IsPositionValid(Position + new Vector2Int(0, 1)))
		{
			var tile = WorldManager.Instance.GetTileAtGrid(Position + new Vector2Int(0, 1));
			if (tile.NorthEdge != null)
			{
				edges.Add(tile.NorthEdge);
			}

		}

		if (WorldManager.IsPositionValid(Position + new Vector2Int(1, 0)))
		{
			var tile = WorldManager.Instance.GetTileAtGrid(Position + new Vector2Int(1, 0));
			if (tile.WestEdge != null)
			{
				edges.Add(tile.WestEdge);
			}
		}
			
		return edges;
	}


	public WorldObject GetCoverObj(Direction dir, bool visibilityCover = false,bool ignoreContollables = false, bool ignoreObjectsAtLoc = true)
	{
		var data = new WorldObject.WorldObjectData();
		data.ID = -1;
		WorldObject biggestCoverObj = new WorldObject(null,null,data);
		dir = Utility.NormaliseDir(dir);
		//Cover biggestCover = Cover.None;
		WorldTile? tileInDir=null;
		if(WorldManager.IsPositionValid(Position + Utility.DirToVec2(dir)))
		{
			tileInDir = WorldManager.Instance.GetTileAtGrid(Position + Utility.DirToVec2(dir));
		}
			
			
		WorldTile tileAtPos = this;

		WorldObject coverObj;
		switch (dir)
		{
			case Direction.East:
				if(tileInDir?.WestEdge != null && tileInDir.WestEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileInDir.WestEdge;
				}
				break;
			case Direction.North:
				if(tileAtPos.NorthEdge != null && tileAtPos.NorthEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileAtPos.NorthEdge;
				}
				break;
				
			case Direction.West:
				if(tileAtPos.WestEdge != null && tileAtPos.WestEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileAtPos.WestEdge;
				}
				break;
			case Direction.South:
				if(tileInDir?.NorthEdge != null && tileInDir.NorthEdge.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = tileInDir.NorthEdge;
				}
				break;
			case Direction.SouthWest:
				coverObj = GetCoverObj(Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}
				coverObj = tileInDir.GetCoverObj(Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = tileInDir.GetCoverObj(Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
					

				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				break;
			case Direction.SouthEast:
				coverObj = GetCoverObj(Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover)  > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}
				coverObj = tileInDir.GetCoverObj(Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = tileInDir.GetCoverObj(Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
					

				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				break;
			case Direction.NorthWest:
				coverObj = GetCoverObj(Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}

				coverObj = tileInDir.GetCoverObj(Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = tileInDir.GetCoverObj(Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

					
				break;
			case Direction.NorthEast:
				coverObj = GetCoverObj(Direction.North,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
				coverObj = GetCoverObj(Direction.East,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				if (tileInDir == null)
				{
					break;
				}
				coverObj = tileInDir.GetCoverObj(Direction.West,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
				if (coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}

				coverObj = tileInDir.GetCoverObj(Direction.South,visibilityCover,ignoreContollables,ignoreObjectsAtLoc);
					

				if(coverObj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = coverObj;
				}
					
				break;
				
		}

		if (!ignoreObjectsAtLoc)
		{
			if (!ignoreContollables)
			{
				if (UnitAtLocation != null && UnitAtLocation.WorldObject.IsVisible() && UnitAtLocation.WorldObject.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (UnitAtLocation.WorldObject.Facing == dir || UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir + 1) || UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir - 1)))
				{

					biggestCoverObj = UnitAtLocation.WorldObject;

				}
			}

			foreach (var obj in ObjectsAtLocation)
			{
				if (obj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = obj;
				}
			}
		}
			

			
			
		//this code is broken but unutill objs at loc can provide cover it doesnt matter
		if (tileInDir != null)
		{

			if (!ignoreContollables && tileInDir.UnitAtLocation != null &&  tileInDir.UnitAtLocation.WorldObject.IsVisible())
			{
				if (tileInDir.UnitAtLocation.WorldObject.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileInDir.UnitAtLocation.WorldObject.Facing == dir || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir + 1) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(dir - 1)))
				{

					biggestCoverObj = tileInDir.UnitAtLocation.WorldObject;

				}

				Direction inverseDir = Utility.NormaliseDir(dir - 4);
				if (tileInDir.UnitAtLocation.WorldObject.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileInDir.UnitAtLocation.WorldObject.Facing == inverseDir || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir + 1) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir + 2) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir - 2) || tileInDir.UnitAtLocation.WorldObject.Facing == Utility.NormaliseDir(inverseDir - 1))) //only hit people from the front
				{

					biggestCoverObj = tileInDir.UnitAtLocation.WorldObject;

				}
			}
			foreach (var obj in tileInDir.ObjectsAtLocation)
			{

				if (obj.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover))
				{
					biggestCoverObj = obj;
				}
			}




		}


		return biggestCoverObj;
	}

	~WorldTile()
	{
		Console.WriteLine("TILE DELETED!: "+Position);
	}

	public void NextTurn()
	{
		foreach (var item in ObjectsAtLocation)
		{
			item.NextTurn();
		}
		WestEdge?.NextTurn();
		NorthEdge?.NextTurn();
		UnitAtLocation?.WorldObject.NextTurn();
		Surface?.NextTurn();
	}
}
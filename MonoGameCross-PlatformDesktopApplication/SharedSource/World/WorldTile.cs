#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using MultiplayerXeno;

namespace MultiplayerXeno
{
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
			if (ControllableAtLocation != null) return false;
			if (Surface != null && Surface.Type.Impassible) return false;
			Cover obstacle =WorldManager.Instance.GetTileAtGrid(from).GetCover(Utility.Vec2ToDir(new Vector2Int(Position.X - from.X, Position.Y - from.Y)));
			if (obstacle > Cover.None) return false;//todo vaulting
			return true;

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

		private WorldObject? _controllableAtLocation;
		public WorldObject? ControllableAtLocation
		{
			get => _controllableAtLocation;
			set
			{
				 
				if (value == null)
				{
					_controllableAtLocation = null;
					return;
				}
				if (value.Type.Edge || value.Type.Surface)
					throw new Exception("attempted to set a surface or edge to the main location");
				if(value.ControllableComponent == null)
					throw new Exception("attempted to set a non controllable to the controllable location");
				if (_controllableAtLocation != null)
				{
					throw new Exception("attempted to place an object over an existing one");
				}

				_controllableAtLocation = value;
				OverWatchTrigger();
			}
		}
		
		
		
		private WorldObject? _surface;

		public void OverWatchTrigger()
		{
			lock (syncobj)
			{
				foreach (var watcher in Watchers)
				{
					watcher.OverWatchSpoted(Position);
				}

			}
		}

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
			WorldManager.Instance.DeleteWorldObject(ControllableAtLocation);
			WorldManager.Instance.DeleteWorldObject(Surface);
			foreach (var obj in ObjectsAtLocation)
			{
				WorldManager.Instance.DeleteWorldObject(obj);
			}


		}

		public void Remove(int id)
		{
			if (NorthEdge != null && NorthEdge.Id == id)
			{
				NorthEdge = null;
			}
			if (WestEdge != null && WestEdge.Id == id)
			{
				WestEdge = null;
			}
			if (ControllableAtLocation != null && ControllableAtLocation.Id == id)
			{
				ControllableAtLocation = null;
			}
			if (Surface != null && Surface.Id == id)
			{
				Surface = null;
			}

			foreach (var obj in ObjectsAtLocation)
			{
				if (obj.Id == id)
				{
					ObjectsAtLocation.Remove(obj);
					break;
				}
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
			if (ControllableAtLocation != null)
			{
				data.ControllableAtLocation = ControllableAtLocation.GetData();
			}
			data.ObjectsAtLocation = ObjectsAtLocation.Select(x => x.GetData()).ToList();

			return data;
		}

		public Cover GetCover(Direction dir, bool ignoreControllables = false)
		{
			WorldObject? obj = GetCoverObj(dir);
		
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
			var data = new WorldObjectData();
			data.Id = -1;
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
					if (ControllableAtLocation != null && ControllableAtLocation.IsVisible() &&ControllableAtLocation.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (ControllableAtLocation.Facing == dir || ControllableAtLocation.Facing == Utility.NormaliseDir(dir + 1) || _controllableAtLocation.Facing == Utility.NormaliseDir(dir - 1)))
					{

						biggestCoverObj = ControllableAtLocation;

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

				if (!ignoreContollables && tileInDir.ControllableAtLocation != null &&  tileInDir.ControllableAtLocation.IsVisible())
				{
					if (tileInDir.ControllableAtLocation.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileInDir.ControllableAtLocation.Facing == dir || tileInDir.ControllableAtLocation.Facing == Utility.NormaliseDir(dir + 1) || tileInDir.ControllableAtLocation.Facing == Utility.NormaliseDir(dir - 1)))
					{

						biggestCoverObj = tileInDir.ControllableAtLocation;

					}

					Direction inverseDir = Utility.NormaliseDir(dir - 4);
					if (tileInDir.ControllableAtLocation.GetCover(visibilityCover) > biggestCoverObj.GetCover(visibilityCover) && (tileInDir.ControllableAtLocation.Facing == inverseDir || tileInDir.ControllableAtLocation.Facing == Utility.NormaliseDir(inverseDir + 1) || tileInDir.ControllableAtLocation.Facing == Utility.NormaliseDir(inverseDir + 2) || tileInDir.ControllableAtLocation.Facing == Utility.NormaliseDir(inverseDir - 2) || tileInDir.ControllableAtLocation.Facing == Utility.NormaliseDir(inverseDir - 1))) //only hit people from the front
					{

						biggestCoverObj = tileInDir.ControllableAtLocation;

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
			ControllableAtLocation?.NextTurn();
			Surface?.NextTurn();
		}
	}
}
#nullable enable
using System;
using System.Collections.Generic;
using CommonData;

namespace MultiplayerXeno
{
	public partial class WorldTile
	{
		public readonly Vector2Int Position;

		public WorldTile(Vector2Int position)
		{
			this.Position = position;
		}
		public static readonly object syncobj = new object();

		private List<Controllable> Watchers = new List<Controllable>();
		private List<Controllable> UnWatchQueue = new List<Controllable>();
		private int HighestWatchLevel = 0;

		public int Smoked = 0;


		public void ApplySmoke(int severity)
		{
			Smoked = severity;
			if (Smoked <= 0)
			{
				Smoked = 0;
				return;
			}
#if CLIENT
			TileEffect = new Smoke(this);
			WorldManager.Instance.MakeFovDirty();
#endif
		}

		public void Watch(Controllable watcher)
		{
			lock (syncobj)
			{
				Watchers.Add(watcher);
			}
#if CLIENT
			CalcWatchLevel();
#endif
		}
		public void UnWatch(Controllable watcher)
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
				bool recalcflag = false;
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
		private WorldObject? _objectAtLocation;
		public WorldObject? ObjectAtLocation
		{
			get => _objectAtLocation;
			set
			 {
				 
				 if (value == null)
				 {
					 _objectAtLocation = null;
					 return;
				 }
				 if (value.Type.Edge || value.Type.Surface)
					 throw new Exception("attempted to set a surface or edge to the main location");
				 if (_objectAtLocation != null)
				 {
					 throw new Exception("attempted to place an object over an existing one");
				 }

				 _objectAtLocation = value;
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
					watcher.OverWatchSpoted(this.Position);
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
			WorldManager.Instance.DeleteWorldObject(ObjectAtLocation);
			WorldManager.Instance.DeleteWorldObject(Surface);
			Smoked = 0;

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
			if (ObjectAtLocation != null && ObjectAtLocation.Id == id)
			{
				ObjectAtLocation = null;
			}
			if (Surface != null && Surface.Id == id)
			{
				Surface = null;
			}
		}

		public WorldTileData GetData()
		{
			var data = new WorldTileData(this.Position);
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
			if (ObjectAtLocation != null)
			{
				data.ObjectAtLocation = ObjectAtLocation.GetData();
			}
			data.smoked = Smoked;

			return data;
		}

		public Cover GetCover(Direction dir, bool ignoreControllables = false)
		{
			WorldObject? obj = GetCoverObj(dir);
		
			return GetCoverObj(dir,ignoreControllables).GetCover();
			



		}

		public WorldObject GetCoverObj(Direction dir, bool ignnoreControllables = false, bool ignoreObjAtLoc = true)
		{
			WorldObject biggestCoverObj = new WorldObject(null,-1,null);
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
					if(tileInDir?.WestEdge != null && tileInDir.WestEdge.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = tileInDir.WestEdge;
					}
					break;
				case Direction.North:
					if(tileAtPos.NorthEdge != null && tileAtPos.NorthEdge.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = tileAtPos.NorthEdge;
					}
					break;
				
				case Direction.West:
					if(tileAtPos.WestEdge != null && tileAtPos.WestEdge.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = tileAtPos.WestEdge;
					}
					break;
				case Direction.South:
					if(tileInDir?.NorthEdge != null && tileInDir.NorthEdge.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = tileInDir.NorthEdge;
					}
					break;
				case Direction.SouthWest:
					coverObj = GetCoverObj(Direction.South,ignnoreControllables);
					if(coverObj.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					coverObj = GetCoverObj(Direction.West,ignnoreControllables);
					if(coverObj.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					if (tileInDir == null)
					{
						break;
					}
					coverObj = tileInDir.GetCoverObj(Direction.North,ignnoreControllables);
					if (coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					coverObj = tileInDir.GetCoverObj(Direction.East,ignnoreControllables);
					

					if(coverObj.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					break;
				case Direction.SouthEast:
					coverObj = GetCoverObj(Direction.South,ignnoreControllables);
					if(coverObj.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					coverObj = GetCoverObj(Direction.East,ignnoreControllables);
					if(coverObj.GetCover()  > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					if (tileInDir == null)
					{
						break;
					}
					coverObj = tileInDir.GetCoverObj(Direction.North,ignnoreControllables);
					if (coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					coverObj = tileInDir.GetCoverObj(Direction.West,ignnoreControllables);
					

					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					break;
				case Direction.NorthWest:
					coverObj = GetCoverObj(Direction.North,ignnoreControllables);
					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					coverObj = GetCoverObj(Direction.West,ignnoreControllables);
					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					if (tileInDir == null)
					{
						break;
					}

					coverObj = tileInDir.GetCoverObj(Direction.East,ignnoreControllables);
					if (coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					coverObj = tileInDir.GetCoverObj(Direction.South,ignnoreControllables);
					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					
					break;
				case Direction.NorthEast:
					coverObj = GetCoverObj(Direction.North,ignnoreControllables);
					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					coverObj = GetCoverObj(Direction.East,ignnoreControllables);
					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					if (tileInDir == null)
					{
						break;
					}
					coverObj = tileInDir.GetCoverObj(Direction.West,ignnoreControllables);
					if (coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}

					coverObj = tileInDir.GetCoverObj(Direction.South,ignnoreControllables);
					

					if(coverObj.GetCover() > biggestCoverObj.GetCover())
					{
						biggestCoverObj = coverObj;
					}
					
					break;
				
			}

			if (!ignoreObjAtLoc)
			{
				if (ObjectAtLocation != null && ObjectAtLocation.GetCover() > biggestCoverObj.GetCover() && (ObjectAtLocation.Facing == dir || ObjectAtLocation.Facing == Utility.NormaliseDir(dir+1) ||  ObjectAtLocation.Facing == Utility.NormaliseDir(dir-1)))
				{
					if (ObjectAtLocation.ControllableComponent == null || !ignnoreControllables)
					{
						biggestCoverObj = ObjectAtLocation;
					}
				}
			}

			if (tileInDir != null)
			{
#if CLIENT
				if(tileInDir.ObjectAtLocation != null && !tileInDir.ObjectAtLocation.IsVisible())
				{
					return biggestCoverObj;
				}
#endif


				Direction inverseDir = Utility.NormaliseDir(dir - 4);
				if (tileInDir.ObjectAtLocation != null && tileInDir.ObjectAtLocation.GetCover() > biggestCoverObj.GetCover() && ( tileInDir.ObjectAtLocation.Facing == inverseDir || tileInDir.ObjectAtLocation.Facing == Utility.NormaliseDir(inverseDir+1) ||  tileInDir.ObjectAtLocation.Facing == Utility.NormaliseDir(inverseDir+2) || tileInDir.ObjectAtLocation.Facing == Utility.NormaliseDir(inverseDir-2) ||tileInDir.ObjectAtLocation.Facing == Utility.NormaliseDir(inverseDir-1)))//only hit people from the front
				{
					if (tileInDir.ObjectAtLocation.ControllableComponent == null || !ignnoreControllables)
					{
						biggestCoverObj = tileInDir.ObjectAtLocation;
					}


				}

			}


			return biggestCoverObj;
		}

		~WorldTile()
		{
			Console.WriteLine("TILE DELETED!: "+this.Position);
		}

		public void NextTurn()
		{
			Smoked--;
#if CLIENT
			if (Smoked <= 0)
			{
				TileEffect = null;
				WorldManager.Instance.MakeFovDirty();
			}
#endif
		}
	}
}
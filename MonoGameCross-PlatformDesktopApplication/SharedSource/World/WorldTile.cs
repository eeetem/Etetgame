using System;
using CommonData;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno
{
	public class WorldTile
	{
		public readonly Vector2Int Position;

		public WorldTile(Vector2Int position)
		{
			this.Position = position;
		}

		private WorldObject _northEdge;
		public WorldObject NorthEdge{
			get => _northEdge;
			set
			{
				if (value != null && (!value.Type.Edge || value.Type.Surface))
					throw new Exception("attempted non edge to edge");
				if (_northEdge != null)
				{
					throw new Exception("attempted to place an object over an existing one");
				}

				_northEdge = value;
			}
		}
		private WorldObject _westEdge;
		public WorldObject WestEdge 	{
			get => _westEdge;
			set
			{
				if (value != null && (!value.Type.Edge || value.Type.Surface))
					throw new Exception("attempted non edge to edge");
				if (_westEdge != null)
				{
					throw new Exception("attempted to place an object over an existing one");
				}

				_westEdge = value;
			}
		}
		private WorldObject _objectAtLocation;
		public WorldObject ObjectAtLocation
		{
			get => _objectAtLocation;
			set
			 {
				 if (value != null && (value.Type.Edge || value.Type.Surface))
					 throw new Exception("attempted to set a surface or edge to the main location");
				 if (_objectAtLocation != null)
				 {
					 throw new Exception("attempted to place an object over an existing one");
				 }

				 _objectAtLocation = value;
			 }
		}
		private WorldObject _surface;
		public WorldObject Surface{
			get => _surface;
			set
			{
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
			WorldManager.DeleteWorldObject(NorthEdge);
			WorldManager.DeleteWorldObject(WestEdge);
			WorldManager.DeleteWorldObject(ObjectAtLocation);
			WorldManager.DeleteWorldObject(Surface);

		}

		public void Remove(int id)
		{
			if (NorthEdge.Id == id)
			{
				NorthEdge = null;
			}
			if (WestEdge.Id == id)
			{
				WestEdge = null;
			}
			if (ObjectAtLocation.Id == id)
			{
				ObjectAtLocation = null;
			}
			if (Surface.Id == id)
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

			return data;
		}

		public Cover GetCover(Direction dir)
		{
			Cover biggestCover = Cover.None;
			WorldTile? tileInDir=null;
			if(WorldManager.IsPositionValid(Position + WorldManager.DirToVec2(dir)))
			{
				tileInDir = WorldManager.GetTileAtGrid(Position + WorldManager.DirToVec2(dir));
			}
			
		
			WorldTile tileAtPos = this;
			if(tileInDir?.ObjectAtLocation != null && tileInDir.ObjectAtLocation.GetCover()  > biggestCover)
			{
					biggestCover = tileInDir.ObjectAtLocation.GetCover();
			}

			Cover cover;
			switch (dir)
			{
				case Direction.East:
					if(tileInDir?.WestEdge != null && tileInDir.WestEdge.GetCover()  > biggestCover)
					{
						biggestCover = tileInDir.WestEdge.GetCover();
					}
					break;
				case Direction.North:
					if(tileAtPos.NorthEdge != null && tileAtPos.NorthEdge.GetCover()  > biggestCover)
					{
						biggestCover = tileAtPos.NorthEdge.GetCover();
					}
					break;
				
				case Direction.West:
					if(tileAtPos.WestEdge != null && tileAtPos.WestEdge.GetCover()  > biggestCover)
					{
						biggestCover = tileAtPos.WestEdge.GetCover();
					}
					break;
				case Direction.South:
					if(tileInDir?.NorthEdge != null && tileInDir.NorthEdge.GetCover()  > biggestCover)
					{
						biggestCover = tileInDir.NorthEdge.GetCover();
					}
					break;
				case Direction.SouthWest:
					cover = GetCover(Direction.South);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}
					cover = GetCover(Direction.West);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}

					if (tileInDir == null)
					{
						break;
					}
					cover = tileInDir.GetCover(Direction.North);
					if (cover > biggestCover)
					{
						biggestCover = cover;
					}

					cover = tileInDir.GetCover(Direction.East);
					

					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}
					break;
				case Direction.SouthEast:
					cover = GetCover(Direction.South);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}
					cover = GetCover(Direction.East);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}

					if (tileInDir == null)
					{
						break;
					}
						cover = tileInDir.GetCover(Direction.North);
						if (cover > biggestCover)
						{
							biggestCover = cover;
						}

						cover = tileInDir.GetCover(Direction.West);
					

					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}
					break;
				case Direction.NorthWest:
					cover = GetCover(Direction.North);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}
					cover = GetCover(Direction.West);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}

					if (tileInDir == null)
					{
						break;
					}

					cover = tileInDir.GetCover(Direction.East);
					if (cover > biggestCover)
					{
						biggestCover = cover;
					}

					cover = tileInDir.GetCover(Direction.South);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}

					
					break;
				case Direction.NorthEast:
					cover = GetCover(Direction.North);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}
					cover = GetCover(Direction.East);
					if(cover  > biggestCover)
					{
						biggestCover = cover;
					}

					if (tileInDir == null)
					{
						break;
					}
						cover = tileInDir.GetCover(Direction.West);
						if (cover > biggestCover)
						{
							biggestCover = cover;
						}

						cover = tileInDir.GetCover(Direction.South);
					

						if(cover  > biggestCover)
						{
							biggestCover = cover;
						}
					
					break;
				
			}
			
		

			return biggestCover;
		}
		
	}
}
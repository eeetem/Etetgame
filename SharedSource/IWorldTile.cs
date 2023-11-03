﻿using System.Collections.Generic;
using DefconNull.World.WorldObjects;

namespace DefconNull;

public interface IWorldTile
{
	Unit? UnitAtLocation { get; set; }
	Vector2Int Position { get;}
	WorldObject? WestEdge { get; set; }
	WorldObject? NorthEdge { get; set; }
	WorldObject? EastEdge { get; set; }
	WorldObject? SouthEdge { get; set; }
	WorldObject? Surface { get; set; }
	List<WorldObject> ObjectsAtLocation { get; }
	void RemoveObject(WorldObject worldObject);
	void PlaceObject(WorldObject worldObject);
	bool Traversible(Vector2Int fromPosition);
	double TraverseCostFrom(Vector2Int tileLocationPosition);

	IEnumerable<WorldObject> GetAllEdges();
}
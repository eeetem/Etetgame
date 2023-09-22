using System;
using System.Collections.Generic;
using DefconNull.World;
using DefconNull.World.WorldObjects;

namespace DefconNull;

public class PseudoTile : IWorldTile
{
	private readonly WorldTile _realParent;

	public PseudoTile(WorldTile realParent)
	{
		_realParent = realParent;
	}

	public bool ForceNoUnit = false;
	private Unit? _unitAtLocation;
	public Unit? UnitAtLocation
	{
		get
		{
			if (ForceNoUnit)
			{
				return null;
			}

			if (_unitAtLocation == null)
			{
				return _realParent.UnitAtLocation;
			}

			return _unitAtLocation;
		}
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

	public Vector2Int Position => _realParent.Position;

	public WorldObject? WestEdge
	{
		get => _realParent.WestEdge;
		set => _realParent.WestEdge = value;
	}
	public WorldObject? EastEdge
	{
		get => _realParent.EastEdge;
		set => _realParent.EastEdge = value;
	}
	public WorldObject? NorthEdge
	{
		get => _realParent.NorthEdge;
		set => _realParent.NorthEdge = value;
	}
	public WorldObject? SouthEdge
	{
		get => _realParent.SouthEdge;
		set => _realParent.SouthEdge = value;
	}
	public WorldObject? Surface
	{
		get => _realParent.Surface;
		set => _realParent.Surface = value;
	}

	public List<WorldObject> ObjectsAtLocation => _realParent.ObjectsAtLocation;


	public void RemoveObject(WorldObject worldObject)
	{
		throw new InvalidOperationException("cannot remove objects from a PseudoTile");
	}
	public void PlaceObject(WorldObject worldObject)
	{
		throw new InvalidOperationException("cannot add objects to a PseudoTile");
	}
	public bool Traversible(Vector2Int fromPosition)
	{
		return _realParent.Traversible(fromPosition);
	}
	public double TraverseCostFrom(Vector2Int tileLocationPosition)
	{
		return _realParent.TraverseCostFrom(tileLocationPosition);
	}

	public IEnumerable<WorldObject> GetAllEdges()
	{
		return _realParent.GetAllEdges();
	}


}
﻿using System;
using System.Collections.Generic;
using DefconNull.WorldObjects;

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
				var realUnit = _realParent.UnitAtLocation;
				return realUnit;
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



	public bool Traversible(Vector2Int fromPosition,bool ignoreControllables = false)
	{
		return _realParent.Traversible(fromPosition,ignoreControllables);
	}
	public double TraverseCostFrom(Vector2Int tileLocationPosition)
	{
		return _realParent.TraverseCostFrom(tileLocationPosition);
	}

	public bool IsVisible()
	{
		return _realParent.IsVisible();
	}

	public Visibility GetVisibility()
	{
		return _realParent.GetVisibility();
	}

	public List<WorldObject> GetAllEdges()
	{
		return _realParent.GetAllEdges();
	}


}
﻿using System;
using System.Collections.Generic;
using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public abstract class UnitSequenceAction : SequenceAction
{

	public TargetingRequirements Requirements;
	public struct TargetingRequirements : IMessageSerializable
	{
		public int ActorID = -1;
		public Vector2Int Position = new Vector2Int(-1, -1);
		public List<string>? TypesToIgnore;

		public TargetingRequirements(int id)
		{
			ActorID = id;
			Position = new Vector2Int(-1, -1);
		}
		public TargetingRequirements(Vector2Int pos)
		{
			Position = pos;
			ActorID = -1;
		}

		public void Serialize(Message message)
		{
			message.Add(ActorID);
			message.Add(Position);
			if (TypesToIgnore == null)
			{
				//send empty array
				message.Add(Array.Empty<string>());
			}
			else
			{
				message.Add(TypesToIgnore.ToArray());
			}

			
		}

		public void Deserialize(Message message)
		{
			ActorID = message.GetInt();
			Position = message.GetSerializable<Vector2Int>();
			TypesToIgnore = new List<string>();
			TypesToIgnore.AddRange(message.GetStrings());
		}
	}
	
	public override bool ShouldDo()
	{
		if (Requirements.ActorID != -1) return true;
		if (Requirements.Position == new Vector2Int(-1, -1)) return false;
		var tile = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
		if(tile.UnitAtLocation == null) return false;
		var obj = tile.UnitAtLocation;
		if(Requirements.TypesToIgnore != null && Requirements.TypesToIgnore.Contains(obj!.Type.Name )) return false;
		
		return true;
	}
#if SERVER
		public override bool ShouldDoServerCheck(bool player1)
	{
		if (Actor == null) return false;

		var wtile = (WorldTile) (Actor.WorldObject.TileLocation);
		return wtile.IsVisible(team1: player1);
	}
#endif


	public Unit? GetAffectedActor(int dimension)
	{
									
		Unit? hitUnit = null;
		if (Requirements.ActorID != -1)
		{
			hitUnit = PseudoWorldManager.GetObject(Requirements.ActorID,dimension)!.UnitComponent;
		}
		else if(Requirements.Position != new Vector2Int(-1, -1))
		{

			hitUnit = PseudoWorldManager.GetTileAtGrid(Requirements.Position,dimension).UnitAtLocation;
		}

		return hitUnit;
	}

	protected Unit? Actor
	{
		get
		{
			if (Requirements.ActorID!= -1)
			{
				var obj =WorldObjectManager.GetObject(Requirements.ActorID);
				if(obj == null) Console.WriteLine("Sequence Actor not found");
				return obj?.UnitComponent;
			}

			if (Requirements.Position != new Vector2Int(-1, -1))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
				var obj = tile.UnitAtLocation;
				if(obj == null) throw new Exception("Sequence Actor not found");
				return obj;
			}

			throw new Exception("Sequence Actor not specified for search");
            

		}
	}
	



	protected override void SerializeArgs(Message message)
	{
		message.Add(Requirements);
	}

	protected override void DeserializeArgs(Message message)
	{
		Requirements = message.GetSerializable<TargetingRequirements>();
	}
}
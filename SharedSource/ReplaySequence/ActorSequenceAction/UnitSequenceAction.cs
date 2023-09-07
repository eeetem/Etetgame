using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public abstract class UnitSequenceAction : SequenceAction
{

	public TargetingRequirements Requirements;
	public struct TargetingRequirements : IMessageSerializable
	{
		public int ActorID = -1;
		public Vector2Int Position = new Vector2Int(-1, -1);
		public List<string> TypesToIgnore = new List<string>();

		public TargetingRequirements(int id)
		{
			ActorID = id;
		}
		public TargetingRequirements(Vector2Int id)
		{
			Position = id;
		}

		public void Serialize(Message message)
		{
			message.Add(ActorID);
			message.Add(Position);
			message.Add(TypesToIgnore.ToArray());
		}

		public void Deserialize(Message message)
		{
			ActorID = message.GetInt();
			Position = message.GetSerializable<Vector2Int>();
			TypesToIgnore = new List<string>();
			TypesToIgnore.AddRange(message.GetStrings());
		}
	}
	public UnitSequenceAction(TargetingRequirements actorID, SequenceAction.SequenceType type) : base(type)
	{
		Requirements = actorID;
	}


	public override bool ShouldDo()
	{
		if (Requirements.ActorID != -1) return true;
		if (Requirements.Position == new Vector2Int(-1, -1)) return false;
		var tile = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
		if(tile.UnitAtLocation == null) return false;
		var obj = tile.UnitAtLocation;
		if(Requirements.TypesToIgnore.Contains(obj!.Type.Name )) return false;

		return true;
	}

	public Unit? GetAffectedActor(int dimension)
	{
									
		Unit? hitUnit = null;
		if (Requirements.ActorID != -1)
		{
			hitUnit = WorldManager.Instance.GetObject(Requirements.ActorID,dimension)!.UnitComponent;
		}
		else if(Requirements.Position != new Vector2Int(-1, -1))
		{

			hitUnit = WorldManager.Instance.GetTileAtGrid(Requirements.Position,dimension).UnitAtLocation;
		}

		return hitUnit;
	}

	protected Unit Actor
	{
		get
		{
			if (Requirements.ActorID!= -1)
			{
				var obj = WorldManager.Instance.GetObject(Requirements.ActorID);
				if(obj == null) throw new Exception("Sequence Actor not found");
				return obj!.UnitComponent!;
			}

			if (Requirements.Position != new Vector2Int(-1, -1))
			{
				var tile = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
				var obj = tile.UnitAtLocation;
				if(obj == null) throw new Exception("Sequence Actor not found");
				return obj!;
			}

			throw new Exception("Sequence Actor not specified for search");
            

		}
	}
	



	protected override void SerializeArgs(Message message)
	{
		message.Add(Requirements.ActorID);
		message.Add(Requirements.Position);
		message.Add(Requirements.TypesToIgnore.ToArray());
	}

}
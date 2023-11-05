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
		public List<string>? TypesToIgnore;

		public TargetingRequirements(int id)
		{
			ActorID = id;
			Position = new Vector2Int(-1, -1);
		}
		public TargetingRequirements(Vector2Int id)
		{
			Position = id;
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
		if (Actor.Overwatch.Item1) return false;
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
		message.Add(Requirements);
	}

	protected override void DeserializeArgs(Message message)
	{
		Requirements = message.GetSerializable<TargetingRequirements>();
	}
}
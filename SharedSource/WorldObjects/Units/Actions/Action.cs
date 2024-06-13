using System;
using System.Collections.Generic;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
#if CLIENT
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.WorldObjects.Units.Actions;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new();
	public readonly ActionType Type;

	public Action(ActionType? type)
	{
		if(type==null) return;
		Type = (ActionType)type;
		Actions.Add((ActionType)type, this);
	}


	public static void Init()
	{
		new Face();
		new Move();
		new Crouch();
		new OverWatch();
		new UseAbility();

	}
	public enum ActionType
	{

		Move=2,
		Face=3,
		Crouch=4,
		OverWatch = 5,
		UseAbility = 7,
		
		
	}


	public abstract Tuple<bool, string> CanPerform(Unit actor, ActionExecutionParamters args);

#if CLIENT
	public virtual void Preview(Unit actor, ActionExecutionParamters args,SpriteBatch spriteBatch){}

	public void SendToServer(Unit actor, ActionExecutionParamters args)
	{
		SendToServer(actor.WorldObject.ID,args);
	}
	public void SendToServer(int actorID, ActionExecutionParamters args)
	{
		var packet = new GameActionPacket(actorID, Type, args);
		NetworkingManager.SendGameAction(packet);
	}
#endif

#if SERVER
	public void PerformServerSide(Unit actor, ActionExecutionParamters args)
	{
		var actions = GetConsequenes(actor,args);
		int i = 1;
		foreach (var queue in actions)
		{
			Task t = new Task(delegate
			{
				NetworkingManager.AddSequenceToSendQueue(queue);//staggered execution becuase some actions need to wait for FOV update
			});
			SequenceManager.RunNextAfterFrames(t,i);
			i += 2;
		}
	}
	
#endif
	public abstract Queue<SequenceAction>[] GetConsequenes(Unit actor, ActionExecutionParamters args);
	public class GameActionPacket : IMessageSerializable
	{
		public ActionExecutionParamters Args { get; set; }
		public int UnitId { get; set; }
		public ActionType Type;

		public GameActionPacket(int unitId, ActionType actionType, ActionExecutionParamters args)
		{
			UnitId = unitId;
			Type = actionType;
			Args = args;
		}
		
		public GameActionPacket()
		{
		}


		public void Serialize(Message message)
		{
			message.Add((int)Type);
			message.Add(UnitId);
			message.Add(Args);
		}

		public void Deserialize(Message message)
		{
			Type = (ActionType)message.GetInt();
			UnitId = message.GetInt();
			Args = message.GetSerializable<ActionExecutionParamters>();
		}
	}
	
	
	public struct ActionExecutionParamters : IMessageSerializable
	{
		
		public Vector2Int Target;
		public TargetOnTile TargetLoc = TargetOnTile.Surface;
		public int AbilityIndex = -1;

		public WorldObject GetTarget()
		{
			var tile = WorldManager.Instance.GetTileAtGrid(Target);
			WorldObject? obj = null;
			switch (TargetLoc)
			{
				case TargetOnTile.North:
					obj = tile.NorthEdge;
					break;
				case TargetOnTile.West:
					obj = tile.WestEdge;
					break;
				case TargetOnTile.Unit:
					obj = tile.UnitAtLocation?.WorldObject;
					break;
				case TargetOnTile.Obj:
					obj = tile.ObjectsAtLocation[0];
					break;
				case TargetOnTile.Surface:
					obj = tile.Surface;
					break;
			}

			if (obj == null)
			{
				obj = tile.Surface;
			}

			return obj!;
		}
		public enum TargetOnTile
		{
			North=0,
			West=1,
			Surface=2,
			Unit=3,
			Obj=4
		}

		public ActionExecutionParamters(Vector2Int target)
		{
			Target = target;
			TargetLoc = TargetOnTile.Surface;
		}
		public ActionExecutionParamters(WorldObject obj)
		{
			Target = obj.TileLocation.Position;
			if(Equals(obj.TileLocation.UnitAtLocation, obj.UnitComponent))
				TargetLoc = TargetOnTile.Unit;
			else if(obj.TileLocation.ObjectsAtLocation.Contains(obj))
				TargetLoc = TargetOnTile.Obj;
			else if (Equals(obj.TileLocation.NorthEdge, obj))
				TargetLoc = TargetOnTile.North;
			else if (Equals(obj.TileLocation.WestEdge, obj))
				TargetLoc = TargetOnTile.West;
			else
				TargetLoc = TargetOnTile.Surface;
		}

		public ActionExecutionParamters()
		{
			
		}

		public void Serialize(Message message)
		{
			message.Add(Target);
			message.Add((short)TargetLoc);
			message.Add(AbilityIndex);
		}

		public void Deserialize(Message message)
		{
			
			Target = message.GetSerializable<Vector2Int>();
			TargetLoc = (TargetOnTile) message.GetInt();
			AbilityIndex = message.GetInt();
		}
	}

	
}
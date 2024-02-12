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
	public abstract List<SequenceAction> Preview(Unit actor, ActionExecutionParamters args,SpriteBatch spriteBatch);

	public void SendToServer(Unit actor, ActionExecutionParamters args)
	{
		var packet = new GameActionPacket(actor.WorldObject.ID, Type, args);
		NetworkingManager.SendGameAction(packet);
	}
#endif


#if SERVER

	public void PerformServerSide(Unit actor, ActionExecutionParamters args)
	{
		
		Task.Run(() =>
		{
			try
			{
				
				var actions = GetConsiquenes(actor,args);
				int i = 1;
				foreach (var queue in actions)
				{
					Task t = new Task(delegate
					{
						NetworkingManager.SendSequence(queue);//staggered execution becuase some actions need to wait for FOV update
						SequenceManager.AddSequence(queue);
					});
					SequenceManager.RunNextAfterFrames(t,i);
					i += 4;
				}
				
				
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
                throw;
            }

		});
		
		
	}
	public abstract Queue<SequenceAction>[] GetConsiquenes(Unit actor, ActionExecutionParamters args);
#endif
	
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
		
		public Vector2Int? Target;
		public WorldObject? TargetObj = null;
		public int AbilityIndex = -1;

		public ActionExecutionParamters(Vector2Int target)
		{
			Target = target;
		}

		public ActionExecutionParamters()
		{
			
		}

		public void Serialize(Message message)
		{
			message.Add(Target ?? new Vector2Int(-1,-1));
			message.Add(TargetObj?.ID ?? -1);
			message.Add(AbilityIndex);
		}

		public void Deserialize(Message message)
		{
			
			Target = message.GetSerializable<Vector2Int>();
			TargetObj =WorldObjectManager.GetObject(message.GetInt());
			AbilityIndex = message.GetInt();
		}
	}

	
}
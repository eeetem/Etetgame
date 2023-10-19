using System;
using System.Collections.Generic;
using System.Linq;
using DefconNull.Networking;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
#if CLIENT
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.World.WorldObjects.Units.Actions;

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


	public abstract Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, List<string> args);

#if CLIENT
	public abstract void Preview(Unit actor, Vector2Int target,SpriteBatch spriteBatch,List<string>? args = null);

	public void SendToServer(Unit actor, Vector2Int target,  List<string> args)
	{
		Console.WriteLine("sending action packet: " + Type + " on " + target + " from " + actor.WorldObject.ID + "");
		var packet = new GameActionPacket(actor.WorldObject.ID, target, Type,args);
		NetworkingManager.SendGameAction(packet);
	}
#endif
	

	
	
#if CLIENT
	public virtual void ExecuteClientSide(Unit actor, Vector2Int target, List<string> args)
	{
		
	}
#else

	public void PerformServerSide(Unit actor,Vector2Int target, List<string> args)
	{

		
		var result = CanPerform(actor, target,args);
		if(!result.Item1)
		{
			Console.WriteLine("Client sent an impossible action: "+result.Item2);
			return;
		}
		Task.Run(() =>
		{
			try
			{
				
				var actions = GetConsiquenes(actor, target,args);
				WorldManager.Instance.AddSequence(actions);
				NetworkingManager.SendSequence(actions);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		});
		
		
	}
	public abstract Queue<SequenceAction> GetConsiquenes(Unit actor, Vector2Int target,  List<string> args);
#endif
	
	public class GameActionPacket : IMessageSerializable
	{
		public ActionType Type { get; set; }
		public int UnitId { get; set; }
		
		public Vector2Int Target { get; set; }

		public List<string> Args { get; set; } = new List<string>();

		public GameActionPacket(int unitId, Vector2Int target, ActionType type, List<string> args)
		{
			UnitId = unitId;
			Target = target;
			Type = type;
			Args = args;
		}
		
		public GameActionPacket()
		{
			
		}


		public void Serialize(Message message)
		{
			message.Add(UnitId);
			message.Add(Target);
			message.Add((int)Type);
			message.AddStrings(Args.ToArray());
		}

		public void Deserialize(Message message)
		{
			UnitId = message.GetInt();
			Target = message.GetSerializable<Vector2Int>();
			Type = (ActionType)message.GetInt();
			Args = message.GetStrings().ToList();
		}
	}

	
}
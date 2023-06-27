using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
using System.Threading;
using MultiplayerXeno.ReplaySequence;

#if CLIENT
using MultiplayerXeno.UILayouts;
#endif

namespace MultiplayerXeno;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new();
	public readonly ActionType Type;
	public static Action? ActiveAction { get; private set; }

	public Action(ActionType? type)
	{
		if(type==null) return;
		Type = (ActionType)type;
		Actions.Add((ActionType)type, this);
	}

	public static void SetActiveAction(ActionType? type)
	{
		
		if (type == null)
		{
			ActiveAction = null;
#if CLIENT
			GameLayout.UpdateActionButtons();	
#endif
			return;
		}
		

		ActiveAction = Actions[(ActionType)type];
		ActiveAction.InitAction();
#if CLIENT
		GameLayout.UpdateActionButtons();	
#endif

	}
	public static ActionType? GetActiveActionType()
	{
		if (ActiveAction != null)
		{
			return ActiveAction.Type;
		}

		return null;
	}

	public static void Init()
	{
		new Attack();
		new Face();
		new Move();
		new Crouch();
		new OverWatch();
		new UseItem();
		new UseExtraAbility();
		new SelectItem();

	}
	public enum ActionType
	{
		Attack=1,
		Move=2,
		Face=3,
		Crouch=4,
		OverWatch = 5,
		UseItem = 6,
		UseAbility = 7,
		SelectItem = 8,
		
	}

	public virtual void InitAction()
	{
#if CLIENT
		GameLayout.ScreenData = null;
#endif
	
	}


	public abstract Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target);

#if CLIENT
	public abstract void Preview(Unit actor, Vector2Int target,SpriteBatch spriteBatch);
	public virtual void Animate(Unit actor, Vector2Int target)
	{
	
		if (WorldManager.Instance.GetTileAtGrid(target).Visible==Visibility.None)
		{
			if (Type == ActionType.Attack)//only fog of war attacks
			{
				Camera.SetPos(actor.WorldObject.TileLocation.Position + new Vector2Int(Random.Shared.Next(-3, 3), Random.Shared.Next(-3, 3)));
			}
		}
		else
		{
			Camera.SetPos(actor.WorldObject.TileLocation.Position);
		}
		Thread.Sleep(800);
	}

	public virtual void SendToServer(Unit actor, Vector2Int target)
	{
		Console.WriteLine("sending action packet: " + Type + " on " + target + " from " + actor.WorldObject.ID + "");
		var packet = new GameActionPacket(actor.WorldObject.ID, target, Type);
		Networking.SendGameAction(packet);
	}
#endif
	

	
	
#if CLIENT
	public virtual void ExecuteClientSide(Unit actor, Vector2Int target)
	{
	}
#else

	public void PerformServerSide(Unit actor,Vector2Int target)
	{

		var result = CanPerform(actor, ref target);
		if(!result.Item1)
		{
			Console.WriteLine("Client sent an impossible action: "+result.Item2);
			return;
		}
		Task.Run(() =>
		{
			try
			{
				var actions = ExecuteServerSide(actor, target);
				WorldManager.Instance.AddSequence(actions);
				Networking.SendSequence(actions);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}

		});
		
		
	}
	public abstract Queue<SequenceAction> ExecuteServerSide(Unit actor, Vector2Int target);
#endif
	
	
	public class GameActionPacket : IMessageSerializable
	{
		public ActionType Type { get; set; }
		public int UnitId { get; set; }
		
		public Vector2Int Target { get; set; }

		public List<string> Args { get; set; } = new List<string>();

		public GameActionPacket(int unitId, Vector2Int target, ActionType type)
		{
			UnitId = unitId;
			Target = target;
			Type = type;
			Args = new List<string>();
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
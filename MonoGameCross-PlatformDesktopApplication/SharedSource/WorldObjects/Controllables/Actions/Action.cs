using System.Collections.Generic;
using System.Numerics;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new Dictionary<ActionType, Action>();
	public readonly ActionType ActionType;
	public static Action? ActiveAction { get; private set; }

	public Action(ActionType type)
	{
		ActionType = type;
		Actions.Add(type, this);
	}

	public static void SetActiveAction(ActionType? type)
	{
		if (type == null)
		{
			ActiveAction = null;
			return;
		}

		ActiveAction = Actions[(ActionType)type];
		ActiveAction.InitAction();
	}
	public static ActionType? GetActiveActionType()
	{
		if (ActiveAction != null)
		{
			return ActiveAction.ActionType;
		}

		return null;
	}

	public static void Init()
	{
		new Face();
		new Move();
		new Crouch();
		new Fire();
		new OverWatch();
		new Sprint();

	}

	public virtual void InitAction()
	{
		
	}

	public abstract bool CanPerform(Controllable actor, Vector2Int target);

	public void Perform(Controllable actor, Vector2Int target)
	{
		
		Execute(actor,target);
		
#if CLIENT
		WorldManager.Instance.MakeFovDirty();	
		Action.SetActiveAction(null);
		return;
		if (UI.SelectedControllable != null)
		{
			UI.UnitUI(UI.SelectedControllable.worldObject);
		}
		else
		{
			UI.UnitUI(actor.worldObject);
		}
#endif
		
	}

	protected abstract void Execute(Controllable actor, Vector2Int target);
#if CLIENT
	public abstract void Preview(Controllable actor, Vector2Int target,SpriteBatch spriteBatch);
#endif
	public void ToPacket(Controllable actor,Vector2Int target)
	{
		var packet = new GameActionPacket(actor.worldObject.Id,target,ActionType);
		Networking.DoAction(packet);
	}
	public void PerformFromPacket(Controllable actor,Vector2Int target)
	{

#if CLIENT
		if(!CanPerform(actor,target))
		{
				UI.ShowMessage("Desync Error","Unit ordered to perform an action it can't perform");
		}
		Perform(actor, target);

#else
		if(!CanPerform(actor,target))
		{
			Console.WriteLine("Cilent sent an impossible action");
			return;
		}
		Perform(actor, target);
		ToPacket(actor,target);
#endif
	}
	
}
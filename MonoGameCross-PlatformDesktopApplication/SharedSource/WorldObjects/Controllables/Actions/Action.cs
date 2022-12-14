using System.Collections.Generic;
using System.Numerics;
using CommonData;

namespace MultiplayerXeno;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new Dictionary<ActionType, Action>();
	public readonly ActionType ActionType;

	public Action(ActionType type)
	{
		ActionType = type;
		Actions.Add(type, this);
	}

	public static void Init()
	{
		new Face();
		new Move();
		new Crouch();
		new Fire();

	}
	

	public abstract bool CanPerform(Controllable actor, Vector2Int target);

	public void Perform(Controllable actor, Vector2Int target)
	{
		
		Execute(actor,target);
#if CLIENT
		WorldManager.Instance.MakeFovDirty();	
		if (Controllable.Selected != null)
		{
			UI.UnitUI(Controllable.Selected.worldObject);
		}
		else
		{
			UI.UnitUI(actor.worldObject);
		}
#endif
		
	}

	protected abstract void Execute(Controllable actor, Vector2Int target);
	
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
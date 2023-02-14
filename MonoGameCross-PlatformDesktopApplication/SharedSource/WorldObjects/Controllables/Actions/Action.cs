using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new Dictionary<ActionType, Action>();
	public readonly ActionType ActionType;
	public static Action? ActiveAction { get; private set; }
	public string Description { get; protected set; } = "";
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
			#if CLIENT
			foreach (var controllable in UI.Controllables)
			{
				controllable.PreviewData = new PreviewData(0,0);
			}
			#endif
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
		new Headshot();
		new Supress();

	}

	public virtual void InitAction()
	{
		
	}

	public abstract Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target);


	public void Perform(Controllable actor, Vector2Int target)
	{
		
		Execute(actor,target);
		
#if CLIENT
		WorldManager.Instance.MakeFovDirty();	
		Action.SetActiveAction(null);
		UI.SetUI(UI.UnitUi);
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


		var result = CanPerform(actor, target);
		if(!result.Item1)
		{
#if CLIENT
				UI.ShowMessage("Desync Error","Unit ordered to perform an action it can't perform: "+result.Item2);
#else
				Console.WriteLine("Client sent an impossible action: "+result.Item2);
#endif
				return;
		}
		Perform(actor, target);
#if SERVER
		ToPacket(actor,target);
#endif
	
	}
}
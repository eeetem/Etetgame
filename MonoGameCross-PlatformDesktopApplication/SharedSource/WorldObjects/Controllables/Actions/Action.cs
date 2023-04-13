using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

#if CLIENT
using MultiplayerXeno.UILayouts;

#endif

namespace MultiplayerXeno;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new();
	public readonly ActionType ActionType;
	public static Action? ActiveAction { get; private set; }
	public string Description { get; protected set; } = "";
	public Action(ActionType? type)
	{
		if(type==null) return;
		ActionType = (ActionType)type;
		Actions.Add((ActionType)type, this);
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
		new SecondWind();
		new Headshot();
		new Supress();
		new UseItem();
		new Inspire();

	}

	public virtual void InitAction()
	{
		
	}

	public abstract Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target);


	public void Perform(Controllable actor, Vector2Int target)
	{
		try
		{
			

#if CLIENT
			Animate(actor,target);
#endif
			Execute(actor, target);
#if CLIENT
			WorldManager.Instance.MakeFovDirty();	
		SetActiveAction(null);
		UI.SetUI(new GameLayout());
#endif
		}catch(Exception e)
		{
			Console.WriteLine(e);
		}
	}

	

	
	public abstract void Execute(Controllable actor, Vector2Int target);
#if CLIENT
	public abstract void Preview(Controllable actor, Vector2Int target,SpriteBatch spriteBatch);
	public abstract void Animate(Controllable actor, Vector2Int target);
#endif
	public virtual void ToPacket(Controllable actor,Vector2Int target)
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
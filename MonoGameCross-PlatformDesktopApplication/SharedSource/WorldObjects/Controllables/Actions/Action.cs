using System;
using System.Collections.Generic;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

#if CLIENT
using System.Threading;
using MultiplayerXeno.UILayouts;

#endif

namespace MultiplayerXeno;

public abstract class Action
{

	public static readonly Dictionary<ActionType, Action> Actions = new();
	public readonly ActionType ActionType;
	public static Action? ActiveAction { get; private set; }

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
			return ActiveAction.ActionType;
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
		UI.SetUI(null);
#endif
		}catch(Exception e)
		{
			Console.WriteLine(e);
		}
	}

	

	
	public abstract void Execute(Controllable actor, Vector2Int target);
#if CLIENT
	public abstract void Preview(Controllable actor, Vector2Int target,SpriteBatch spriteBatch);
	public virtual void Animate(Controllable actor, Vector2Int target)
	{
	
		if (WorldManager.Instance.GetTileAtGrid(target).Visible==Visibility.None)
		{
			if (ActionType == ActionType.Attack)//only fog of war attacks
			{
				Camera.SetPos(actor.worldObject.TileLocation.Position + new Vector2Int(Random.Shared.Next(-3, 3), Random.Shared.Next(-3, 3)));
			}
		}
		else
		{
			Camera.SetPos(actor.worldObject.TileLocation.Position);
		}
		Thread.Sleep(800);
	}
#endif
	public virtual void ToPacket(Controllable actor,Vector2Int target)
	{
		Console.WriteLine("sending action packet: "+ActionType+" on "+target+" from "+actor.worldObject.Id+"");
		var packet = new GameActionPacket(actor.worldObject.Id,target,ActionType);
		Networking.DoAction(packet);
	}
	public void PerformFromPacket(Controllable actor,Vector2Int target)
	{
#if CLIENT
			Shootable.freeFire = true;
#endif
	
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
	
#if CLIENT
		Shootable.freeFire = false;
#endif
	}
}
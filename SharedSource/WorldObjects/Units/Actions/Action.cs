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
			if (ActionType == ActionType.Attack)//only fog of war attacks
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
	public virtual void SendToServer(Unit actor,Vector2Int target)
	{
		Console.WriteLine("sending action packet: "+ActionType+" on "+target+" from "+actor.WorldObject.ID+"");
		var packet = new GameActionPacket(actor.WorldObject.ID,target,ActionType);
		Networking.DoAction(packet);
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
		ExecuteServerSide(actor, target);
		
	}
	public abstract void ExecuteServerSide(Unit actor, Vector2Int target);
#endif
}
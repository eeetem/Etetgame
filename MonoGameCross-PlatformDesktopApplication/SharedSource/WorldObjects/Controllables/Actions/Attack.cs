using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;
#if CLIENT
using MultiplayerXeno.UILayouts;
#endif

namespace MultiplayerXeno;

public class Attack : Action
{
	public Attack() :base(ActionType.Attack)
	{
	}

	
	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		if (actor.overWatch && actor.IsPlayerOneTeam != GameManager.IsPlayer1Turn)//only overwatch out of turn
		{
			return new Tuple<bool, string>(true, "");
		}

		if (actor.ActionPoints < 1)
		{
			return new Tuple<bool, string>(false, "Not Enough Fire Points");
		}
		
		if (actor.MovePoints < 1)
		{
			return new Tuple<bool, string>(false, "Not Enough Move Points");
		}

		return actor.Type.DefaultAttack.CanPerform(actor, target);


	}

	public override void Execute(Controllable actor,Vector2Int target)
	{
		actor.ActionPoints--;
		actor.MovePoints--;
		actor.Type.DefaultAttack.Execute(actor, target);
	}

	public override void ToPacket(Controllable actor, Vector2Int target)
	{
		var packet = new GameActionPacket(actor.worldObject.Id,target,ActionType);
		
		packet.args.Add(Shootable.targeting.ToString());
		
		
		Networking.DoAction(packet);
	}

#if CLIENT
	public override void InitAction()
	{
		GameLayout.SelectedControllable.Type.DefaultAttack.InitPreview();
		base.InitAction();
	}
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.Type.DefaultAttack.Preview(actor, target,spriteBatch);
	}
	public override void Animate(Controllable actor, Vector2Int target)
	{
		base.Animate(actor, target);
		actor.Type.DefaultAttack.Animate(actor,target);
	}
#endif




}


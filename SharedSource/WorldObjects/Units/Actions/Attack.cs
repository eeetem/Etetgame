using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;
using MultiplayerXeno.ReplaySequence;
#if CLIENT
using MultiplayerXeno.UILayouts;
#endif

namespace MultiplayerXeno;

public class Attack : Action
{
	public Attack() :base(ActionType.Attack)
	{
	}

	
	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
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

		return actor.Type.DefaultAttack.CanPerform(actor,ref target);


	}

#if SERVER
		public override Queue<SequenceAction> ExecuteServerSide(Unit actor,Vector2Int target)
	{
		throw new NotImplementedException();
		actor.ActionPoints--;
		actor.MovePoints--;
		actor.Type.DefaultAttack.Execute(actor, target);
	}
#endif




#if CLIENT

	public override void SendToServer(Unit actor, Vector2Int target)
	{
	var packet = new GameActionPacket(actor.WorldObject.ID,target,Type);
	packet.Args.Add(Shootable.targeting.ToString());
	Networking.SendGameAction(packet);
	}


	public override void InitAction()
	{
		GameLayout.SelectedUnit.Type.DefaultAttack.InitPreview();
		base.InitAction();
	}
	public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		actor.Type.DefaultAttack.Preview(actor, target,spriteBatch);
	}
	public override void Animate(Unit actor, Vector2Int target)
	{
		base.Animate(actor, target);
		actor.Type.DefaultAttack.Animate(actor,target);
	}
#endif




}


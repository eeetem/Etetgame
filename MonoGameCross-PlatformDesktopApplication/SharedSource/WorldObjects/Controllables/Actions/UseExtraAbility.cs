using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class UseExtraAbility : Action
{
	
	
		public UseExtraAbility() : base(ActionType.UseAbility)
		{
		}

		public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
		{

			if (abilityIndex == -1)
			{
				return new Tuple<bool, string>(false, "No Action Selected");
			}
			IExtraAction action = actor.extraActions[abilityIndex];

			return action.CanPerform(actor, target);

		}

		public static int abilityIndex = -1;
		public override void Execute(Controllable actor, Vector2Int target)
		{
			IExtraAction action = actor.extraActions[abilityIndex];
			action.Execute(actor, target);
		}

		public override void ToPacket(Controllable actor, Vector2Int target)
		{
			IExtraAction action = actor.extraActions[abilityIndex];
			var packet = new GameActionPacket(actor.worldObject.Id,target,ActionType);
			packet.args.Add(abilityIndex.ToString());
			foreach (var a in action.MakePacketArgs())
			{
				packet.args.Add(a);
			}
		
			Networking.DoAction(packet);
		
		}

#if CLIENT

		public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
		{
			IExtraAction action = actor.extraActions[abilityIndex];
			action.Preview(actor, target,spriteBatch);
		}

		public override void Animate(Controllable actor, Vector2Int target)
		{
			IExtraAction action = actor.extraActions[abilityIndex];
			action.Animate(actor,target);
		}
#endif

	
}
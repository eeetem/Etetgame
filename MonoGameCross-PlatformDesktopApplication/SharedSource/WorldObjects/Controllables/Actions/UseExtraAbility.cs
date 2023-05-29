using System;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class UseExtraAbility : Action
{
	
	
		public UseExtraAbility() : base(ActionType.UseAbility)
		{
		}

		public override Tuple<bool, string> CanPerform(Controllable actor,ref  Vector2Int target)
		{

			if (AbilityIndex == -1)
			{
				return new Tuple<bool, string>(false, "No Action Selected");
			}
			IExtraAction action = actor.extraActions[AbilityIndex];

			return action.CanPerform(actor, ref target);

		}

		public static bool abilityLock = false;
		private static int _abilityIndex = -1;

		public static int AbilityIndex
		{
			get => _abilityIndex;
			set {
				if (abilityLock)
				{
					return;
				}

				_abilityIndex = value;
			}
		}

		public override void Execute(Controllable actor, Vector2Int target)
		{
			IExtraAction action = actor.extraActions[AbilityIndex];
			action.Execute(actor, target);
		}

		public override void ToPacket(Controllable actor, Vector2Int target)
		{
			IExtraAction action = actor.extraActions[AbilityIndex];
			var packet = new GameActionPacket(actor.worldObject.Id,target,ActionType);
			packet.args.Add(AbilityIndex.ToString());
			foreach (var a in action.MakePacketArgs())
			{
				packet.args.Add(a);
			}
		
			Networking.DoAction(packet);
		
		}

#if CLIENT

		public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
		{
			if(AbilityIndex == -1)return;
			IExtraAction action = actor.extraActions[AbilityIndex];
			action.Preview(actor, target,spriteBatch);
		}

		public override void Animate(Controllable actor, Vector2Int target)
		{
			base.Animate(actor,target);
			IExtraAction action = actor.extraActions[AbilityIndex];
			action.Animate(actor,target);
		}
#endif

	
}
using System;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using DefconNull.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldObjects.Units.Actions;

public class UseAbility : Action
{
	
	
	public UseAbility() : base(ActionType.UseAbility)
	{
	}

	public override Tuple<bool, string> CanPerform(Unit actor,  Vector2Int target)
	{
			
		IUnitAbility action = actor.Abilities[(AbilityIndex)];
		return action.CanPerform(actor, target);

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

#if SERVER
	public override Queue<SequenceAction> GetConsiquenes(Unit actor, Vector2Int target)
	{

		IUnitAbility action = actor.Abilities[(AbilityIndex)];
		var res = action.GetConsequences(actor, target);
		var queue = new Queue<SequenceAction>();
		if (AbilityIndex == 0)//hardcode shooter spotting for n
		{
			var m = new MoveCamera(actor.WorldObject.TileLocation.Position, true, 3);
			queue.Enqueue(m);
		}
		else
		{
			var m = new MoveCamera(actor.WorldObject.TileLocation.Position, false, 0);
			queue.Enqueue(m);
		}

		
		foreach (var sequenceAction in res)
		{
			queue.Enqueue(sequenceAction);
		}
		return queue;


	}
#endif
	



#if CLIENT
		public override void SendToServer(Unit actor, Vector2Int target)
		{
			IUnitAbility action = actor.Abilities[AbilityIndex];
			var packet = new GameActionPacket(actor.WorldObject.ID,target,Type);
			packet.Args.Add(AbilityIndex.ToString());

			NetworkingManager.SendGameAction(packet);
		
		}
		public override void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
		{
			IUnitAbility action = actor.Abilities[AbilityIndex];
			action.Preview(actor, target,spriteBatch);
		}


#endif

	
}
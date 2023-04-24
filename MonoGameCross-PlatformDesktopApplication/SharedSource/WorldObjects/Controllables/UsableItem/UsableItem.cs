using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public class UsableItem
{
	public readonly string? name;
	public readonly UsageMethod UsageMethod;
	public readonly WorldEffect Effect;
	public UsableItem(string name, UsageMethod usageMethod, WorldEffect effect)
	{
		this.name = name;
		UsageMethod = usageMethod;
		Effect = effect;
	}

	public void Execute(Controllable actor, Vector2Int target)
	{
		
		target = UsageMethod.ProcessTargetLocation(actor, target);
		Effect.Apply(target);
		actor.RemoveItem(this);
	}




	public Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		return UsageMethod.CanPerform(actor, target);
	}
#if CLIENT

	


	public void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		
		target = UsageMethod.Preview(actor, target, spriteBatch);
		Effect.Preview(target, spriteBatch);
	}

	public void Animate(Controllable actor, Vector2Int target)
	{
		UsageMethod.Animate(actor, target);
		Effect.Animate(target);
	}

#endif


	
}
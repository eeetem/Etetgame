using System;
using System.Collections.Generic;
using MultiplayerXeno;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno.Items;

public class Throwable : UsageMethod
{
	
	readonly int throwRange;
	public Throwable(int throwRange)
	{
		this.throwRange = throwRange;
	}

	private static Vector2Int LastReturned = new Vector2Int(0,0);
	public override Vector2Int ProcessTargetLocation(Controllable actor, Vector2Int target)
	{
		if (Vector2.Distance(target, actor.worldObject.TileLocation.Position) > throwRange)
		{
			return LastReturned;
		}

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.worldObject.TileLocation.Position, target, Cover.Full);
		LastReturned = outcome.CollisionPointShort;
		return outcome.CollisionPointShort;
	}

	


	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		if (Vector2.Distance(actor.worldObject.TileLocation.Position, target) >= throwRange)
		{
			return new Tuple<bool, string>(false, "Too Far");
		}
		
		return new Tuple<bool, string>(true, "");
	}
#if CLIENT
	public override Vector2Int Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		Vector2Int newTarget = ProcessTargetLocation(actor, target);
		spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"),  Utility.GridToWorldPos(newTarget+new Vector2(-1.5f,-0.5f)), Color.Red);
		spriteBatch.DrawLine(Utility.GridToWorldPos(actor.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)) , Utility.GridToWorldPos(newTarget+new Vector2(0.5f,0.5f)), Color.Red, 2);
		
		
		
		return newTarget;
	}

	public override void Animate(Controllable actor, Vector2Int target)
	{
		return;
	}
#endif

}
using System;

using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.World.WorldActions;

public class Throwable : DeliveryMethod
{
	
	readonly int throwRange;
	public Throwable(int throwRange)
	{
		this.throwRange = throwRange;
	}

	private static Vector2Int? LastReturned = new Vector2Int(0,0);
	public override Vector2Int? ExectuteAndProcessLocationChild(Unit actor, Vector2Int target)
	{
		if (target == actor.WorldObject.TileLocation.Position)
		{
			return target;
		}

		if (Vector2.Distance(target, actor.WorldObject.TileLocation.Position) > throwRange)
		{
			if (LastReturned == null) return null;
			target = LastReturned.Value;
			
		}

		var outcome = WorldManager.Instance.CenterToCenterRaycast(actor.WorldObject.TileLocation.Position, target, Cover.Full,false,true);
		LastReturned = outcome.EndPoint;
		return outcome.CollisionPointShort;
	}

	


	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target) > throwRange)
		{
			if (LastReturned != null && Vector2.Distance(actor.WorldObject.TileLocation.Position, LastReturned.Value) <= throwRange)
			{
				target = LastReturned.Value;
				return new Tuple<bool, string>(true, "");
			}

			return new Tuple<bool, string>(false, "Too Far");
		}
		
		return new Tuple<bool, string>(true, "");
	}
#if CLIENT
	public override Vector2Int? PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		Vector2Int? newTarget = ExectuteAndProcessLocation(actor, target);
		if (newTarget == null) return newTarget;
			
		spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"),  Utility.GridToWorldPos((Vector2)newTarget+new Vector2(-1.5f,-0.5f)), Color.Red);
		spriteBatch.DrawLine(Utility.GridToWorldPos(actor.WorldObject.TileLocation.Position+new Vector2(0.5f,0.5f)) , Utility.GridToWorldPos((Vector2)newTarget+new Vector2(0.5f,0.5f)), Color.Red, 2);
		
		
		
		return newTarget;
	}

	public override void InitPreview()
	{
		LastReturned = null;
	}

#endif

}
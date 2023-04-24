using System;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public abstract class UsageMethod
{
	public abstract Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target);

	public abstract Vector2Int ProcessTargetLocation(Controllable actor, Vector2Int target);
#if CLIENT
	public abstract Vector2Int Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch);
	public abstract void Animate(Controllable actor, Vector2Int target);
#endif

}
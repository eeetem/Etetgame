using System;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public abstract class DeliveryMethod
{
	public Vector2Int offset = new Vector2Int(0,0);
	public abstract Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target);

	public Vector2Int? ExectuteAndProcessLocation(Unit actor, Vector2Int target)
	{
		return ExectuteAndProcessLocationChild(actor, target+offset);
	}

	public abstract Vector2Int? ExectuteAndProcessLocationChild(Unit actor, Vector2Int target);
#if CLIENT
	public Vector2Int? Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		return PreviewChild(actor, target+offset,spriteBatch);
	}

	public abstract Vector2Int? PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch);

	public abstract void InitPreview();

	public void Animate(Unit actor, Vector2Int target)
	{
		AnimateChild(actor, target+offset);
	}

	public abstract void AnimateChild(Unit actor, Vector2Int target);
#endif

}
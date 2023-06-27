using System;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public class ImmideateDelivery : DeliveryMethod
{
	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		return new Tuple<bool, string>(true, "");
		
	}

	public override Vector2Int? ExectuteAndProcessLocationChild(Unit actor, Vector2Int target)
	{
		return target;
	}
#if CLIENT
	public override Vector2Int? PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		return target;
	}
	
	public override void InitPreview()
	{
		return;
	}

	public override void AnimateChild(Unit actor, Vector2Int target)
	{
		return;
	}
#endif

}
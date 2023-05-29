using System;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public class ImmideateDelivery : DeliveryMethod
{
	public override Tuple<bool, string> CanPerform(Controllable actor, ref Vector2Int target)
	{
		return new Tuple<bool, string>(true, "");
		
	}

	public override Vector2Int ExectuteAndProcessLocationChild(Controllable actor, Vector2Int target)
	{
		return target;
	}
#if CLIENT
	public override Vector2Int? PreviewChild(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		return target;
	}
	
	public override void InitPreview()
	{
		return;
	}

	public override void AnimateChild(Controllable actor, Vector2Int target)
	{
		return;
	}
#endif

}
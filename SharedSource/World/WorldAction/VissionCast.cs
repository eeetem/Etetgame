using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace MultiplayerXeno.Items;

public class VissionCast : DeliveryMethod
{
	
	readonly int range;
	public VissionCast(int range)
	{
		this.range = range;
	}

	private static Vector2Int? LastReturned;
	public override Vector2Int? ExectuteAndProcessLocationChild(Unit actor, Vector2Int target)
	{
		if (Vector2.Distance(target, actor.WorldObject.TileLocation.Position) > range)
		{
			return LastReturned;
		}
		if (Visibility.None == WorldManager.Instance.CanSee(actor, target, true))
		{
			return LastReturned;
		}

		return target;
	}

	


	public override Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		if (Vector2.Distance(actor.WorldObject.TileLocation.Position, target) >= range)
		{
			return new Tuple<bool, string>(false, "Too Far");
		}
		if (Visibility.None == WorldManager.Instance.CanSee(actor, target, true))
		{
			return new Tuple<bool, string>(true, "No Sight");
		}
		
		return new Tuple<bool, string>(true, "");
	}
#if CLIENT
	public override Vector2Int? PreviewChild(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		Vector2Int? result = ExectuteAndProcessLocation(actor, target);
		if (result != null)
		{
			Vector2Int newTarget = result.Value;
			spriteBatch.Draw(TextureManager.GetTexture("UI/targetingCursor"), Utility.GridToWorldPos(newTarget + new Vector2(-1.5f, -0.5f)), Color.Red);
			spriteBatch.DrawLine(Utility.GridToWorldPos(actor.WorldObject.TileLocation.Position + new Vector2(0.5f, 0.5f)), Utility.GridToWorldPos(newTarget + new Vector2(0.5f, 0.5f)), Color.Red, 2);
		}


		return result;
	}

	public override void InitPreview()
	{
		LastReturned = null;
	}

	public override void AnimateChild(Unit actor, Vector2Int target)
	{

	}
#endif

}
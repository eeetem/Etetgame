using System;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

public class Inspire : Action
{
	public Inspire() : base(ActionType.Inspire)
	{
	}

	public override Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		throw new NotImplementedException();
	}

	public override void Execute(Controllable actor, Vector2Int target)
	{
		throw new NotImplementedException();
	}
#if CLIENT
	public override void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		throw new NotImplementedException();
	}

	public override void Animate(Controllable actor, Vector2Int target)
	{
		throw new NotImplementedException();
	}
#endif
}
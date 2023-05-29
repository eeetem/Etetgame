using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public interface IExtraAction : ICloneable
{
	public Tuple<bool, string> CanPerform(Controllable actor, ref Vector2Int target);
	public Tuple<bool, string> HasEnoughPointsToPerform(Controllable actor);

	List<string> MakePacketArgs();
	void Execute(Controllable actor, Vector2Int target);
	WorldAction WorldAction { get; }
	int[]  GetConsiquences(Controllable c);
	bool ImmideateActivation { get; }
	string Tooltip { get; }
#if CLIENT

	void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch);
	void Animate(Controllable actor, Vector2Int target);
	Texture2D Icon { get; }


#endif

}
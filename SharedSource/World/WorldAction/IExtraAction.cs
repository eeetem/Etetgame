﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public interface IExtraAction : ICloneable
{
	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target);
	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor);

	List<string> MakePacketArgs();
	void Execute(Unit actor, Vector2Int target);
	WorldAction WorldAction { get; }
	int[]  GetConsiquences(Unit c);
	bool ImmideateActivation { get; }
	string Tooltip { get; }
#if CLIENT
	void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
	Texture2D Icon { get; }


#endif

}
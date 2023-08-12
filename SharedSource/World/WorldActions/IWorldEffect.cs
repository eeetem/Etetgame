using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public interface IWorldEffect
{
	//bool IsOptional { get; }
	public float GetOptimalRangeAI();
	Tuple<bool, string> CanPerform(Unit actor, Vector2Int target);
	List<SequenceAction> GetConsiquences(Unit actor, Vector2Int target);

#if CLIENT
		void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
#endif
	
}
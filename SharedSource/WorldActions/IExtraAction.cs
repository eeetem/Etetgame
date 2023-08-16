using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public interface IUnitAbility : ICloneable
{
	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target);
	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor);
	public bool CanHit(Unit actor, Vector2Int target, bool lowTarget = false);

	List<SequenceAction> GetConsequences(Unit actor, Vector2Int target);
	List<IWorldEffect> Effects { get; }
	Tuple<int, int, int> GetCost(Unit c);
	bool ImmideateActivation { get; }
	string Tooltip { get; }
	float GetOptimalRangeAI();
#if CLIENT
	void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
	Texture2D Icon { get; }
	
#endif

}
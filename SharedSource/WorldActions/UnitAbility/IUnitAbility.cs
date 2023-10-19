using System;
using System.Collections.Generic;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Effect = DefconNull.World.WorldActions.Effect;

namespace DefconNull.WorldActions.UnitAbility;

public interface IUnitAbility : ICloneable
{
	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, bool considerTargetAids, bool nextTurn, int dimension = -1);
	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor,bool nextTurn = false);
	public Tuple<bool,string> IsPlausibleToPerform(Unit actor, Vector2Int target, int dimension = -1);
	public Tuple<bool, string> IsValidTarget(Unit actor, Vector2Int target, int dimension);

	List<SequenceAction> GetConsequences(Unit actor, Vector2Int target, int dimension = -1);
	List<Effect> Effects { get; }
	AbilityCost GetCost();
	bool ImmideateActivation { get; }
	string Tooltip { get; }
	string Name { get; }
	int Index { get;}
	int OverwatchRange { get;}
	bool AIExempt { get; }
	float GetOptimalRangeAI();
#if CLIENT
	void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
	Texture2D Icon { get; }

#endif

}
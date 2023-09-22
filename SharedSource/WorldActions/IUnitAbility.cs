﻿using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public interface IUnitAbility : ICloneable
{
	public Tuple<bool, string> CanPerform(Unit actor, Vector2Int target, bool nextturn = false, int dimension = -1);
	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor,bool nextTurn = false);
	public Tuple<bool,string> IsPlausibleToPerform(Unit actor, Vector2Int target, int dimension = -1);

	List<SequenceAction> GetConsequences(Unit actor, Vector2Int target, int dimension = -1);
	List<Effect> Effects { get; }
	Tuple<int, int, int> GetCost(Unit c);
	bool ImmideateActivation { get; }
	string Tooltip { get; }
	string Name { get; }
	int Index { get;}
	bool AIExempt { get; }
	float GetOptimalRangeAI();
#if CLIENT
	void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
	Texture2D Icon { get; }

#endif

}
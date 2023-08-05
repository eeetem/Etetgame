using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public interface IExtraAction : ICloneable
{
	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target);
	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor);

	List<string> MakePacketArgs();
	List<SequenceAction> ExecutionResult(Unit actor, Vector2Int target);
	WorldAction WorldAction { get; }
	Tuple<int, int, int> GetCost(Unit c);
	bool ImmideateActivation { get; }
	string Tooltip { get; }
	float GetOptimalRange();
#if CLIENT
	void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch);
	Texture2D Icon { get; }


#endif

}
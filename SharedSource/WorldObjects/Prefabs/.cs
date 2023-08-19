using System;
using System.Collections.Generic;

using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;

#if CLIENT
using DefconNull.Rendering;
#endif

namespace DefconNull.World.WorldObjects;

public class UsableItem
{
	List<IWorldEffect> _effects;
	public readonly List<string> allowedUnits;

	public UsableItem(string name, string desciption, List<IWorldEffect> effects, List<string> allowedUnits)
	{
		Name = name;
		Description = desciption;
		_effects = effects;
		this.allowedUnits = allowedUnits;
		
#if CLIENT
		if (Name != "")
		{
			Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		}
#endif
	}

	public string Name;
	public string Description;


	public Tuple<bool, string> CanPerform(Unit actor,Vector2Int target)
	{
		foreach (var eff in _effects)
		{
			var result = eff.CanPerform(actor,target);
			if (!result.Item1)
			{
				return result;
			}
			
		}

		return new Tuple<bool, string>(true, "");
	}

	public List<SequenceAction> GetConsequences(Unit actor, Vector2Int target)
	{
		var cons = new List<SequenceAction>();
		foreach (var effect in _effects)
		{
			cons.AddRange(effect.GetConsequences(actor,target));
		}
		return cons;
	}
#if CLIENT
	public Texture2D Icon;
	//public bool Visible => Action.Consiqences.Visible;

	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		foreach (var effect in _effects)
		{
			effect.Preview(actor,target,spriteBatch);
		}

		
	}

#endif


}
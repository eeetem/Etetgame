﻿using System;
using System.Collections.Generic;
using DefconNull.World.WorldActions;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldObjects;

public class UsableItem
{
	WorldAction Action;
	public readonly List<string> allowedUnits;

	public UsableItem(WorldAction action, List<string> allowedUnits)
	{
		Action = action;
		Name =	action.Name;
		this.allowedUnits = allowedUnits;
	}

	public string Name;
	public string Description => Action.Description;


	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		return Action.CanPerform(actor,ref target);
	}

	public void Execute(Unit actor, Vector2Int target)
	{
		Action.Execute(actor,target);
	}
#if CLIENT
	public Texture2D? Icon => Action.Icon;
	public bool Visible => Action.Effect.Visible;

	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		Action.Preview(actor,target,spriteBatch);
	}

	public void InitPreview()
	{
		Action.InitPreview();
	}
#endif


}
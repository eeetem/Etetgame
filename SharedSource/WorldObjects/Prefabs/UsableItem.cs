using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class UsableItem
{
	WorldAction Action;
	public readonly List<string> allowedUnits;

	public UsableItem(WorldAction action, List<string> allowedUnits)
	{
		Action = action;
		this.allowedUnits = allowedUnits;
	}

	public string Name => Action.Name;
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
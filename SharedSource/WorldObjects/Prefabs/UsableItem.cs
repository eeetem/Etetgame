using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class UsableItem
{
	WorldAction Effect;
	public readonly List<string> allowedUnits;

	public UsableItem(WorldAction effect, List<string> allowedUnits)
	{
		Effect = effect;
		this.allowedUnits = allowedUnits;
	}

	public string Name => Effect.Name;
	public string Description => Effect.Description;


	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
		return Effect.CanPerform(actor,ref target);
	}

	public void Execute(Unit actor, Vector2Int target)
	{
		Effect.Execute(actor,target);
	}
#if CLIENT
	public Texture2D? Icon => Effect.Icon;
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		Effect.Preview(actor,target,spriteBatch);
	}

	public void InitPreview()
	{
		Effect.InitPreview();
	}
#endif


}
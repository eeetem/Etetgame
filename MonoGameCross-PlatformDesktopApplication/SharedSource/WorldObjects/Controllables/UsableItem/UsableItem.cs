using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public abstract class UsableItem
{
	public readonly string? name;
	public UsableItem(string? name)
	{
		this.name = name;
	}
	public abstract void Execute(Controllable actor, Vector2Int target);

	public abstract Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target);
#if CLIENT

	public abstract void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch);

	public List<Tuple<string,string,string>> effects = new List<Tuple<string, string, string>>();
	public string sfx = "";
	public void Animate(Controllable actor, Vector2Int target)
	{
		Camera.SetPos(target);
		Audio.PlaySound(sfx,target);
		foreach (var effect in effects)
		{
			PostPorcessing.AddTweenReturnTask(effect.Item1, float.Parse(effect.Item2), float.Parse(effect.Item3),true,10f);
		}
	}

#endif


}
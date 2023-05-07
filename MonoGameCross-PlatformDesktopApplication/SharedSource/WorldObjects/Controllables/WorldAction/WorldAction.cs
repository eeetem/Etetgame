using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno.Items;

public class WorldAction
{
	public readonly string? name;
	public readonly List<DeliveryMethod> DeliveryMethods = new List<DeliveryMethod>();
	public readonly WorldEffect Effect;
#if CLIENT
	public readonly Texture2D Icon;
	#endif
	public WorldAction(string name, List<DeliveryMethod> deliveryMethods, WorldEffect effect)
	{
		this.name = name;
		DeliveryMethods = deliveryMethods;
		Effect = effect;
		#if CLIENT
		if (name != "")
		{
			Icon = TextureManager.GetTexture("UI/GameHud/" + name);
		}
#endif
	}

	public void Execute(Controllable actor, Vector2Int target)
	{
		foreach (var method in DeliveryMethods)
		{
			target = method.ExectuteAndProcessLocation(actor, target);
			Effect.Apply(target,actor);
			
		}
		
		
	}




	public Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		return DeliveryMethods[0].CanPerform(actor, target);
	}
#if CLIENT
	public void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		foreach (var method in DeliveryMethods)
		{
			var result = method.Preview(actor, target, spriteBatch);
			if(result == null) return;
			Effect.Preview(result, spriteBatch,actor);
		}
	
	}

	public void Animate(Controllable actor, Vector2Int target)
	{
		foreach (var method in DeliveryMethods)
		{
			method.Animate(actor, target);
			Effect.Animate(target);//TODO: this is not correct
			Thread.Sleep(300);
		}
		
		
	}
	public void InitPreview()
	{
		foreach (var method in DeliveryMethods)
		{
			method.InitPreview();
		}
	}

#endif


	
}
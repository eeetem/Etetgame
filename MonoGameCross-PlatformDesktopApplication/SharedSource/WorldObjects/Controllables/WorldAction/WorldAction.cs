using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;

#if CLIENT
using MultiplayerXeno.UILayouts;
#endif


namespace MultiplayerXeno.Items;

public class WorldAction
{
	public readonly string Name;
	public readonly string Description;
	public readonly List<DeliveryMethod> DeliveryMethods = new List<DeliveryMethod>();
	public readonly WorldEffect Effect;
#if CLIENT
	public readonly Texture2D Icon;
	#endif
	public WorldAction(string name,  string description, List<DeliveryMethod> deliveryMethods, WorldEffect effect)
	{
		this.Name = name;
		DeliveryMethods = deliveryMethods;
		Effect = effect;
		Description = description;
#if CLIENT
		if (name != "")
		{
			Icon = TextureManager.GetTexture("UI/GameHud/" + name);
		}
#endif
	}

	

	public void Execute(Controllable actor, Vector2Int target)
	{
		Console.WriteLine("Executing Action "+Name+" on "+target+" by "+actor.worldObject.Id);
		foreach (var method in DeliveryMethods)
		{
			target = method.ExectuteAndProcessLocation(actor, target);
			Effect.Apply(target,actor);
			
		}
#if CLIENT
		GameLayout.ReMakeMovePreview();
#endif
		
		
	}




	public Tuple<bool, string> CanPerform(Controllable actor, Vector2Int target)
	{
		return DeliveryMethods[0].CanPerform(actor, target);
	}
#if CLIENT
	public void Preview(Controllable actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (DeliveryMethods[0] is Shootable)
		{
			if (!Shootable.freeFire)
			{
				var tile = WorldManager.Instance.GetTileAtGrid(target);
				if (tile.ControllableAtLocation == null || !tile.ControllableAtLocation.IsVisible() || tile.ControllableAtLocation.ControllableComponent.IsMyTeam())
				{
					return;
				}
			}
		}

		foreach (var method in DeliveryMethods)
		{
			var result = method.Preview(actor, target, spriteBatch);
			if(result == null) continue;
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
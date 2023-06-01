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
	public readonly Texture2D? Icon;
	public TargetAid targetAid;
#endif

	public enum TargetAid
	{
		None,
		Unit,
		Enemy
		
	}
	
	public WorldAction(string name,  string description, List<DeliveryMethod> deliveryMethods, WorldEffect effect)
	{
		Name = name;
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

	

	public void Execute(Unit actor, Vector2Int target)
	{
		Console.WriteLine("Executing Action "+Name+" on "+target+" by "+actor.worldObject.Id);
		foreach (var method in DeliveryMethods)
		{
			var t = method.ExectuteAndProcessLocation(actor, target);
			if (t != null)
			{
				Effect.Apply(t, actor.worldObject);
			}
		}
#if CLIENT
		GameLayout.ReMakeMovePreview();
#endif
		
		
	}




	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{
#if CLIENT
		if (!FreeFire && targetAid != TargetAid.None)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(target);
			if (tile.ControllableAtLocation == null || !tile.ControllableAtLocation.IsVisible() || tile.ControllableAtLocation.ControllableComponent.IsMyTeam())
			{
				return new Tuple<bool, string>(false, "Invalid target, hold ctrl for free fire");
			}
		}	
#endif
		return DeliveryMethods[0].CanPerform(actor,ref target);
	}
#if CLIENT
	public static bool FreeFire = false;
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
	
		if (!FreeFire && targetAid != TargetAid.None)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(target);
			if (tile.ControllableAtLocation == null || !tile.ControllableAtLocation.IsVisible())
			{
				return;
			}

			if (targetAid == TargetAid.Enemy && tile.ControllableAtLocation.ControllableComponent.IsMyTeam())
			{
				return;
			}
		}
		

		foreach (var method in DeliveryMethods)
		{
			var result = method.Preview(actor, target, spriteBatch);
			if(result == null) continue;
			Effect.Preview(result, spriteBatch,actor.worldObject);
		}
	
	}

	public void Animate(Unit actor, Vector2Int target)
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
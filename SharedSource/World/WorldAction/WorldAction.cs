using System;
using System.Collections.Generic;
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
			Icon = TextureManager.GetTextureFromPNG("Icons/" + name);
		}
#endif
	}

	

	public void Execute(Unit actor, Vector2Int target)
	{
		Console.WriteLine("Executing Action "+Name+" on "+target+" by "+actor.WorldObject.ID);
		foreach (var method in DeliveryMethods)
		{
			var t = method.ExectuteAndProcessLocation(actor, target);
			if (t.HasValue)
			{
				Effect.Apply(t.Value, actor.WorldObject);
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
			if (tile.UnitAtLocation == null || !tile.UnitAtLocation.WorldObject.IsVisible())
			{
				return new Tuple<bool, string>(false, "Invalid target, hold ctrl for free fire");
			}
			if (targetAid == TargetAid.Enemy && tile.UnitAtLocation.IsMyTeam())
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
			if (tile.UnitAtLocation == null || !tile.UnitAtLocation.WorldObject.IsVisible())
			{
				foreach (var m in DeliveryMethods)
				{
					m.InitPreview();
				}
				return;
			}

			if (targetAid == TargetAid.Enemy && tile.UnitAtLocation.IsMyTeam())
			{
				foreach (var m in DeliveryMethods)
				{
					m.InitPreview();//reset selection memory
				}
				return;
			}
		}
		

		foreach (var method in DeliveryMethods)
		{
			var result = method.Preview(actor, target, spriteBatch);
			if(result == null) continue;
			Effect.Preview(result.Value, spriteBatch,actor.WorldObject);
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
using System;
using System.Collections.Generic;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.World.WorldActions;

public class ExtraAction : IExtraAction
{
	public readonly string name = null!;
	public readonly string tooltip = null!;
	public readonly int DetCost;
	public readonly int MoveCost;
	public readonly int ActCost;
	public readonly WorldAction act = null!;
	public WorldAction WorldAction => act;
	public readonly bool immideaateActivation;



	public Tuple<int,int,int> GetCost(Unit c)
	{
		return new Tuple<int, int, int>(DetCost,ActCost,MoveCost);
	}

	public bool ImmideateActivation => immideaateActivation;

	public string Tooltip => tooltip;

	public ExtraAction(string name, string tooltip, int determinationCost, int movePointCost, int actionPointCost, WorldAction action, bool immideaateActivation)
	{
		this.name = name;
		this.tooltip = tooltip;
		DetCost = determinationCost;
		MoveCost = movePointCost;
		ActCost  = actionPointCost;
		act = action;
		this.immideaateActivation = immideaateActivation;
#if CLIENT
		
		Icon = act.Icon;
#endif
	}

	public ExtraAction()
	{
		
	}

	public Tuple<bool, string> CanPerform(Unit actor, ref Vector2Int target)
	{

		var res = HasEnoughPointsToPerform(actor);
		if (!res.Item1)
		{
			return res;
		}

		return WorldAction.CanPerform(actor, ref target);
	}

	public Tuple<bool, string> HasEnoughPointsToPerform(Unit actor)
	{
		if (actor.Determination - DetCost < 0)
		{
			return new Tuple<bool, string>(false, "Not enough determination");
		}
		
		if (actor.MovePoints - MoveCost < 0)
		{
			return new Tuple<bool, string>(false, "Not enough move points");
		}
		

		if (actor.ActionPoints - MoveCost < 0)
		{
			return new Tuple<bool, string>(false, "Not enough action points");
		}
		

		return new Tuple<bool, string>(true, "");
	}

	public List<string> MakePacketArgs()
	{
		if(WorldAction.DeliveryMethods.Find(x => x is Shootable)!= null)
		{
			return new List<string>() {Shootable.targeting.ToString()};
		}

		return new List<string>();
	}

	public void Execute(Unit actor, Vector2Int target)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}
		
		actor.Suppress(-DetCost,true);
		actor.MovePoints -= MoveCost;
		actor.ActionPoints -= ActCost;
		WorldAction.Execute( actor,  target);
	}
#if CLIENT
	
	public void InitPreview()
	{
		WorldAction.InitPreview();
	}
	public void Preview(Unit actor, Vector2Int target, SpriteBatch spriteBatch)
	{
		if (immideaateActivation)
		{
			target = actor.WorldObject.TileLocation.Position;
		}

		WorldAction.Preview(actor, target, spriteBatch);
	}


	public Texture2D Icon { get; set; }
	

#endif

	public object Clone()
	{
		return new ExtraAction(name, tooltip, DetCost, MoveCost, ActCost, act, immideaateActivation);	
	}


}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class HudActionButton
{
	public static HudActionButton? SelectedButton;
	public readonly ImageButton UIButton;
	public readonly Unit Owner;
	private readonly Action<Unit,Vector2Int> ExecuteTask;
	private readonly Func<Unit,Vector2Int,Tuple<bool,string>> PerformCheckTask;
	private readonly Action<Unit,Vector2Int,SpriteBatch>? PreviewTask;
	public readonly AbilityCost Cost;
	readonly TextureRegion Icon;
	public string Tooltip;
	public readonly bool CanOverwatch;
	public HudActionButton(ImageButton imageButton,Texture2D icon ,Unit owner ,Action<Unit,Vector2Int> executeTask, Func<Unit,Vector2Int,Tuple<bool,string>> performCheckTask, AbilityCost cost, string tooltip, Action<Unit,Vector2Int,SpriteBatch>? previewTask = null)
	{
		UIButton = imageButton;
		Cost = cost;
		Tooltip = tooltip;
		Owner = owner;
		Icon = new TextureRegion(icon);
		ExecuteTask = executeTask;
		PreviewTask = previewTask;
		PerformCheckTask = performCheckTask;
		UIButton.Click += (o, a) =>
		{
			SelectedButton = this;
		};
		
		UIButton.Click += (o, a) =>
		{
			GameLayout.SelectHudAction(this);
		};
		



	
	}



	public List<Unit> GetSuggestedTargets(Unit user, List<Vector2Int> targetsToCheck)
	{
		List<Unit> SuggestedTargets = new();

		foreach (var target in targetsToCheck)
		{
			if (PerformCheckTask(Owner,target).Item1)
			{
				SuggestedTargets.Add(WorldManager.Instance.GetTileAtGrid(target).UnitAtLocation);
			}
		}
	
		return SuggestedTargets;
	}

	public void Preview(Unit selectedUnit, Vector2Int actionTarget, SpriteBatch batch)
	{
		PreviewTask?.Invoke(selectedUnit,actionTarget,batch);
	}

	public bool HasPoints()
	{
		if (Cost.Determination > 0)
		{
			if (Owner.Determination.Current - Cost.Determination < 0)
			{
				return false;
			}
		}

		if(Cost.MovePoints>0)
		{
			if (Owner.MovePoints.Current - Cost.MovePoints < 0)
			{
				return false;
			}
		}

		if (Cost.ActionPoints > 0) 
		{
			if (Owner.ActionPoints.Current - Cost.ActionPoints < 0)
			{
				return false;
			}
		}

		return true;
	}

	public void UpdateIcon()
	{
		if (this ==SelectedButton)
		{
			UIButton.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), new Color(255, 140, 140));
			UIButton.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), new Color(255, 140, 140));
			UIButton.Image = new ColoredRegion(Icon, Color.Red);
		}
		else if(HasPoints())
		{
			UIButton.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button"));
			UIButton.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button"));
			UIButton.Image = Icon;
		}
		else
		{
			UIButton.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), Color.Gray);
			UIButton.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")),  Color.Gray);
			UIButton.Image = new ColoredRegion(Icon, Color.Gray);
		}

	
	}

	public void PerformAction(Vector2Int target)
	{
		ExecuteTask(Owner,target);
	}
	public Tuple<bool,string> CanPerformAction(Vector2Int target)
	{
		return PerformCheckTask(Owner,target);
	}

	public void OverwatchAction(Vector2Int actionTarget)
	{
		throw new NotImplementedException();
	}
}
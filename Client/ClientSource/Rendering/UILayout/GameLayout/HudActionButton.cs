
using System;
using System.Collections.Generic;
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class HudActionButton
{
	public static HudActionButton? SelectedButton;
	public readonly ImageButton UIButton;
	public readonly Unit Owner;
	private readonly Action<Unit,Vector2Int> _executeTask;
	private readonly Action<Unit,Vector2Int>? _executeOverWatchTask;
	private readonly Func<Unit,Vector2Int,Tuple<bool,string>> _performCheckTask;
	private readonly Action<Unit,Vector2Int,SpriteBatch>? _previewTask;
	private readonly Action<Unit,Vector2Int,SpriteBatch>? _previewOverwatchTask;
	public readonly AbilityCost Cost;
	readonly TextureRegion _icon;
	public string Tooltip;
	public readonly bool CanOverwatch;
	public HudActionButton(ImageButton imageButton,Texture2D icon ,Unit owner ,Action<Unit,Vector2Int> executeTask, Func<Unit,Vector2Int,Tuple<bool,string>> performCheckTask, AbilityCost cost, string tooltip)
	{
		UIButton = imageButton;
		Cost = cost;
		Tooltip = tooltip;
		CanOverwatch = false;
		Owner = owner;
		_icon = new TextureRegion(icon);
		_executeTask = executeTask;
		_previewTask = null;
		_performCheckTask = performCheckTask;
		UIButton.Click += (o, a) =>
		{
			SelectedButton = this;
			GameLayout.SelectHudAction(this);
		};
		

		
	}

	public HudActionButton(ImageButton imageButton, UnitAbility abl, Unit owner)
	{
		UIButton = imageButton;
		Cost = abl.GetCost();
		Tooltip = abl.Tooltip;
		CanOverwatch = abl.CanOverWatch;
		Owner = owner;
		_icon = new TextureRegion(abl.Icon);
		_executeTask = (unit, vector2Int) =>
		{
			if(abl.ImmideateActivation)
				vector2Int = unit.WorldObject.TileLocation.Position;
			unit.DoAction(Action.ActionType.UseAbility, vector2Int,new  List<string> { abl.Index.ToString()});
		};
		_previewTask = (unit, target, batch) =>
		{
			Action.Actions[Action.ActionType.UseAbility].Preview(unit, target, batch, new List<string> {abl.Index.ToString()});
		};
		_performCheckTask = (unit, vector2Int) => abl.CanPerform(unit, vector2Int, true, false);
		if (CanOverwatch)
		{
			_previewOverwatchTask = (unit, target, batch) =>
			{
				Action.Actions[Action.ActionType.OverWatch].Preview(unit, target, batch, new List<string> {abl.Index.ToString()});
			};
			_executeOverWatchTask = (unit, vector2Int) =>
			{
				unit.DoAction(Action.ActionType.OverWatch, vector2Int, new List<string> {abl.Index.ToString()});
			};
		}
		UIButton.Click += (o, a) =>
		{
			SelectedButton = this;
			GameLayout.SelectHudAction(this);
		};

	}



	public List<Unit> GetSuggestedTargets(Unit user, List<Vector2Int> targetsToCheck)
	{
		List<Unit> suggestedTargets = new();

		foreach (var target in targetsToCheck)
		{
			if (_performCheckTask(Owner,target).Item1 && WorldManager.Instance.GetTileAtGrid(target).UnitAtLocation != null && WorldManager.Instance.CanTeamSee(target, Owner.IsPlayer1Team) >= WorldManager.Instance.GetTileAtGrid(target).UnitAtLocation!.WorldObject.GetMinimumVisibility())
			{
				suggestedTargets.Add(WorldManager.Instance.GetTileAtGrid(target).UnitAtLocation!);
			}
		}
	
		return suggestedTargets;
	}

	public void Preview(Vector2Int actionTarget, SpriteBatch batch)
	{
		_previewTask?.Invoke(Owner,actionTarget,batch);
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
			UIButton.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/button")), new Color(255, 140, 140));
			UIButton.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/button")), new Color(255, 140, 140));
			UIButton.Image = new ColoredRegion(_icon, Color.Red);
		}
		else if(HasPoints())
		{
			UIButton.Background = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/button"));
			UIButton.OverBackground = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/button"));
			UIButton.Image = _icon;
		}
		else
		{
			UIButton.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/button")), Color.Gray);
			UIButton.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/button")),  Color.Gray);
			UIButton.Image = new ColoredRegion(_icon, Color.Gray);
		}

	
	}

	public void PerformAction(Vector2Int target)
	{
		_executeTask(Owner,target);
	}
	public Tuple<bool,string> CanPerformAction(Vector2Int target)
	{
		return _performCheckTask(Owner,target);
	}

	public void OverwatchAction(Vector2Int actionTarget)
	{
		_executeOverWatchTask?.Invoke(Owner,actionTarget);
	}

	public void PreviewOverwatch(Vector2Int actionTarget, SpriteBatch batch)
	{
		_previewOverwatchTask?.Invoke(Owner,actionTarget,batch);
	}
}
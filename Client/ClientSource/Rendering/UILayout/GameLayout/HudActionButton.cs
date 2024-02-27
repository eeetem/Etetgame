
using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldActions;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Action = DefconNull.WorldObjects.Units.Actions.Action;
using Unit = DefconNull.WorldObjects.Unit;
using WorldObject = DefconNull.WorldObjects.WorldObject;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class HudActionButton
{
	public static HudActionButton? SelectedButton;
	public readonly ImageButton UIButton;
	public readonly int OwnerID;
	public Unit? Owner => WorldObjectManager.GetObject(OwnerID)?.UnitComponent;
	private readonly Action<Unit,WorldObject> _executeTask;
	private readonly Action<Unit,Vector2Int>? _executeOverWatchTask;
	private readonly Func<Unit,WorldObject,Tuple<bool,string>> _performCheckTask;
	private readonly Func<Unit,WorldObject,SpriteBatch,List<SequenceAction>>? _previewTask;
	private readonly Action<Unit,Vector2Int,SpriteBatch>? _previewOverwatchTask;
	public readonly AbilityCost Cost;
	readonly TextureRegion _icon;
	public string Tooltip;
	public readonly bool CanOverwatch;
	private bool selfOnly = false;
	public HudActionButton(ImageButton imageButton,Texture2D icon ,Unit owner ,Action<Unit,WorldObject> executeTask, Func<Unit,WorldObject,Tuple<bool,string>> performCheckTask, AbilityCost cost, string tooltip, bool selfOnly)
	{
		UIButton = imageButton;
		Cost = cost;
		Tooltip = tooltip;
		CanOverwatch = false;
		this.selfOnly = selfOnly;
		OwnerID = owner.WorldObject.ID;
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
		selfOnly = abl.ImmideateActivation;
		CanOverwatch = abl.CanOverWatch;
		OwnerID = owner.WorldObject.ID;
		_icon = new TextureRegion(abl.Icon);
		_executeTask = (unit, target) =>
		{
			if(abl.ImmideateActivation)
				target = unit.WorldObject;
			unit.DoAbility(target, abl.Index);
		};
		_previewTask = (unit, target, batch) =>
		{
			Action.ActionExecutionParamters args = new Action.ActionExecutionParamters();
			args.TargetObj = target;
			args.AbilityIndex = abl.Index;
			return Action.Actions[Action.ActionType.UseAbility].Preview(unit, args,batch);
		};
		_performCheckTask = (unit, vector2Int) => abl.CanPerform(unit, vector2Int, true, false);
		if (CanOverwatch)
		{
			_previewOverwatchTask = (unit, target, batch) =>
			{
				Action.ActionExecutionParamters args = new Action.ActionExecutionParamters(target);
				args.AbilityIndex = abl.Index;
				Action.Actions[Action.ActionType.OverWatch].Preview(unit, args,batch);
			};
			_executeOverWatchTask = (unit, target) =>
			{
				unit.DoOverwatch(target, abl.Index);
			};
		}
		UIButton.Click += (o, a) =>
		{
			SelectedButton = this;
			GameLayout.SelectHudAction(this);
		};

	}



	public List<WorldObject> GetSuggestedTargets(List<Unit> targetsToCheck)
	{
		List<WorldObject> suggestedTargets = new();
	
		if(selfOnly)
		{
			suggestedTargets.Add(Owner.WorldObject);
			return suggestedTargets;
		}
		foreach (var target in targetsToCheck)
		{
			if (_performCheckTask(Owner,target.WorldObject).Item1
			    && WorldManager.Instance.CanTeamSee(target.WorldObject.TileLocation.Position, Owner.IsPlayer1Team) >= target.WorldObject.GetMinimumVisibility())
			{
				suggestedTargets.Add(target.WorldObject);
			}
		}
	
		return suggestedTargets;
	}

	public List<SequenceAction> Preview(WorldObject actionTarget, SpriteBatch batch)
	{
		return _previewTask?.Invoke(Owner,actionTarget,batch) ?? new List<SequenceAction>();
	}

	public bool HasPoints()
	{
		if(Owner == null) return false;
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

	public void PerformAction(WorldObject target)
	{
		_executeTask(Owner,target);
	}
	public Tuple<bool,string> CanPerformAction(WorldObject target)
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
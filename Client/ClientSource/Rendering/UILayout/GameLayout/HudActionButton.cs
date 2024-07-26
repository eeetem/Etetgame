
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
	private readonly Func<Unit,WorldObject,Tuple<bool,string>> _recommendedToPerformTask;
	private readonly Func<Unit,WorldObject,Tuple<bool,string>> _canPerformTask;
	private readonly Action<Unit,WorldObject,SpriteBatch>? _previewTask;
	private readonly Action<Unit,Vector2Int,SpriteBatch>? _previewOverwatchTask;
	public readonly AbilityCost Cost;
	readonly TextureRegion _icon;
	public string Tooltip;
	public readonly bool CanOverwatch;
	public readonly bool SelfOnly = false;
	private readonly Func<Unit,WorldObject,List<SequenceAction>> _getConsequencesTask;
	private bool _suggestTargets = false;

	public HudActionButton(ImageButton imageButton,Texture2D icon ,Unit owner ,Action<Unit,WorldObject> executeTask, Func<Unit,WorldObject,Tuple<bool,string>> recommendedToPerformTask, AbilityCost cost, string tooltip, bool selfOnly)
	{
		UIButton = imageButton;
		Cost = cost;
		Tooltip = tooltip;
		CanOverwatch = false;
		SelfOnly = selfOnly;
		OwnerID = owner.WorldObject.ID;
		_icon = new TextureRegion(icon);
		_executeTask = executeTask;
		_previewTask = null;
		_getConsequencesTask = (unit, target) => new List<SequenceAction>();
		_recommendedToPerformTask = recommendedToPerformTask;
		_canPerformTask = recommendedToPerformTask;
		UIButton.Click += (o, a) =>
		{
			SelectedButton = this;
			GameLayout.SelectHudAction(this);
		};
		

		
	}
	bool quickPreview = false;
	public HudActionButton(ImageButton imageButton, UnitAbility abl, Unit owner)
	{
		UIButton = imageButton;
		_suggestTargets = abl.TargetAids.Count > 0;//if we dont have any target aids dont suggest any targets, this is hacky and doesnt cover all possible edge cases, but rihgt now it's only relavent to the smoke and it works

		Cost = abl.GetCost();
		Tooltip = abl.Tooltip;
		SelfOnly = abl.ImmideateActivation;
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
			Action.ActionExecutionParamters args = new Action.ActionExecutionParamters(target);
			args.AbilityIndex = abl.Index;
			Action.Actions[Action.ActionType.UseAbility].Preview(unit, args,batch);
		};
		_getConsequencesTask = (unit, target) =>
		{
			Action.ActionExecutionParamters args = new Action.ActionExecutionParamters(target);
			args.AbilityIndex = abl.Index;
			List<SequenceAction> consequences = new();
			var arr =  Action.Actions[Action.ActionType.UseAbility].GetConsequenes(unit, args);
			foreach (var queue in arr)
			{
				foreach (var action in queue)
				{
					consequences.Add(action);
				}
			}

			return consequences;
		};
		_recommendedToPerformTask = (unit, vector2Int) =>
		{
		
			return abl.CanPerform(unit, vector2Int, true, false);
		};
		_canPerformTask = (unit, vector2Int) => abl.CanPerform(unit, vector2Int, false, false);
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
			GameLayout.SelectHudAction(this);
			quickPreview = false;
		};
		UIButton.MouseEntered += (o, a) =>
		{
			if (SelectedButton == null)
			{
				GameLayout.SelectHudAction(this);
				quickPreview = true;
			}

		};
		UIButton.MouseLeft += (o, a) =>
		{
			if (quickPreview)
			{
				GameLayout.SelectHudAction(null);
				quickPreview = false;
			}

			
		};

	}



	public List<WorldObject> GetSuggestedTargets(List<Unit> targetsToCheck)
	{
		List<WorldObject> suggestedTargets = new();
		if(_suggestTargets == false)
			return suggestedTargets;
	
		if(SelfOnly)
		{
			suggestedTargets.Add(Owner.WorldObject);
			return suggestedTargets;
		}
		foreach (var target in targetsToCheck)
		{
			if (_recommendedToPerformTask(Owner,target.WorldObject).Item1
			    && WorldManager.Instance.CanTeamSee(target.WorldObject.TileLocation.Position, Owner.IsPlayer1Team) >= target.WorldObject.GetMinimumVisibility())
			{
				suggestedTargets.Add(target.WorldObject);
			}
		}
	
		return suggestedTargets;
	}

	public void Preview(WorldObject actionTarget, SpriteBatch batch)
	{
		if (Owner==null) return;
		_previewTask?.Invoke(Owner, actionTarget, batch);
	}

	public Tuple<bool,string> IsAbleToPerform(WorldObject target)
	{
		if (Owner==null) return new Tuple<bool, string>(false, "Owner is null");
		return _canPerformTask(Owner,target);
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
	public void UpdateIcon( )
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
	public Tuple<bool,string> ShouldBeAbleToPerform(WorldObject target)
	{
		if (!_suggestTargets) return new Tuple<bool, string>(true, "");
		return _recommendedToPerformTask(Owner,target);
	}

	public void OverwatchAction(Vector2Int actionTarget)
	{
		_executeOverWatchTask?.Invoke(Owner,actionTarget);
	}

	public void PreviewOverwatch(Vector2Int actionTarget, SpriteBatch batch)
	{
		_previewOverwatchTask?.Invoke(Owner,actionTarget,batch);
	}

	public IEnumerable<SequenceAction> GetConsequences(WorldObject actionTarget)
	{
		return _getConsequencesTask?.Invoke(Owner,actionTarget);
	}
}
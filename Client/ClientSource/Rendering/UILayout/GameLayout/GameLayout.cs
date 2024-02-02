using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldActions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Action = DefconNull.WorldObjects.Units.Actions.Action;
using Move = DefconNull.AI.Move;
using Thickness = Myra.Graphics2D.Thickness;
using Unit = DefconNull.WorldObjects.Unit;
using WorldObject = DefconNull.WorldObjects.WorldObject;

namespace DefconNull.Rendering.UILayout.GameLayout;

public class GameLayout : MenuLayout
{

	public static List<Vector2Int>[] PreviewMoves = Array.Empty<List<Vector2Int>>();
	
	public static void SetScore(int score)
	{
		if (ScoreIndicator != null)
		{
			ScoreIndicator.Text = "score: " + score;
		}
	}

	public static Unit? SelectedUnit { get; private set;} = null!;
	public static Unit? SelectedEnemyUnit { get; private set;} = null!;

	public static void SelectUnit(Unit? unit)
	{
		if (unit == null)
		{
			unit = GameManager.GetTeamUnits(GameManager.IsPlayer1).FirstOrDefault();
		}
		if(unit is null)return;

		if (!unit.IsMyTeam())
		{
			SelectedEnemyUnit = unit;
			return;
		}
		SelectHudAction(null);

		SelectedUnit = unit;
		ReMakeMovePreview();
	
			UI.SetUI( new GameLayout());
		
		Camera.SetPos(unit.WorldObject.TileLocation.Position);
	}

	public static readonly int[,,] AIMoveCache = new int[100,100,2];
	public static void ReMakeMovePreview()
	{
		if(UI.currentUi is not GameLayout) return;
		
		if(SelectedUnit == null) return;
		var ret =  SelectedUnit.GetPossibleMoveLocations();
		if(PreviewMoves.Length <= ret.Length)
		{
			Array.Resize(ref PreviewMoves, ret.Length);
		}

		foreach (var p in PreviewMoves)
		{
			if(p != null)p.Clear();
		}

		for (int j = 0; j < ret.Length; j++)
		{
			if(PreviewMoves[j] == null)
			{
				PreviewMoves[j] = new List<Vector2Int>();
			}
			else
			{
				PreviewMoves[j].Clear();
			}
		}
		
		int i = 0;
		foreach (var g in ret)
		{

			foreach (var item in g)
			{
				PreviewMoves[i].Add(item.Item1);
			}

			i++;
		}
		UpdateHudButtons();
	}

	private static RenderTarget2D hoverHudRenderTarget;
	private static RenderTarget2D consequenceListRenderTarget;

	private static RenderTarget2D?timerRenderTarget;
	//private static RenderTarget2D? chatRenderTarget;
	//private static RenderTarget2D? chatScreenRenderTarget;
	private static Dictionary<int, RenderTarget2D> unitBarRenderTargets;
	private static Dictionary<int, RenderTarget2D>?targetBarRenderTargets;
		

	public static void Init()
	{
		hoverHudRenderTarget = new RenderTarget2D(graphicsDevice,250,100);
		consequenceListRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("HoverHud/consequenceFrame").Width,200);
	
		timerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("GameHud/UnitBar/unitframe").Width,TextureManager.GetTexture("GameHud/UnitBar/unitframe").Height);
		unitBarRenderTargets = new Dictionary<int, RenderTarget2D>();
		targetBarRenderTargets = new Dictionary<int, RenderTarget2D>();
	}


	public static void MakeUnitBarRenders(SpriteBatch batch)
	{
		if(_unitBar == null ||  _unitBar.Widgets.Count == 0) return;
		if( GameManager.GetMyTeamUnits().Count == 0) return;
		int columCounter = 0;
		//sort by id
		GameManager.GetMyTeamUnits().Sort((a, b) => a.WorldObject.ID.CompareTo(b.WorldObject.ID));
		foreach (var unit in new List<Unit>(GameManager.GetMyTeamUnits()))
		{
			if (!unitBarRenderTargets.ContainsKey(unit.WorldObject.ID))
			{
				unitBarRenderTargets.Add(unit.WorldObject.ID, new RenderTarget2D(graphicsDevice, TextureManager.GetTexture("GameHud/UnitBar/Background").Width, TextureManager.GetTexture("GameHud/UnitBar/Background").Height));

			}

			graphicsDevice.SetRenderTarget(unitBarRenderTargets[unit.WorldObject.ID]);
			graphicsDevice.Clear(Color.Transparent);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/Background"), Vector2.Zero, Color.White);
			int i;
			if (unit.ActionPoints > 0 || unit.MovePoints > 0)
			{
				batch.End();
				PostProcessing.PostProcessing.ApplyUIEffect( new Vector2(TextureManager.GetTexture("GameHud/UnitBar/screen").Width, TextureManager.GetTexture("GameHud/UnitBar/screen").Height));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();

				PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(5,5));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);

				int notchpos = 0;
				for (i = 0; i < unit.MovePoints; i++)
				{
					
					batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/moveNotch"), new Vector2(6 * notchpos, 0), Color.White);
					notchpos++;
				}

				for (i = 0; i < unit.ActionPoints; i++)
				{
					batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/fireNotch"), new Vector2(6 * notchpos, 0), Color.White);
					
					notchpos++;
				}
				batch.End();
			}
			else
			{
				batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();
			}




			var healthTexture = TextureManager.GetTexture("GameHud/UnitBar/red");
			var healthTextureoff = TextureManager.GetTexture("GameHud/UnitBar/redoff");
			float healthWidth = healthTexture.Width;
			float healthHeight = healthTexture.Height;
			int baseWidth = TextureManager.GetTexture("GameHud/UnitBar/Background").Width;
			float healthBarWidth = (healthWidth + 1) * unit.WorldObject.Type.MaxHealth;
			float emtpySpace = baseWidth - healthBarWidth;
			Vector2 healthBarPos = new Vector2(emtpySpace / 2f, 22);


			for (int y = 0; y < unit.WorldObject.Type.MaxHealth; y++)
			{
				var indicator = healthTexture;
				bool health = !(y >= unit.WorldObject.Health);
				if (health)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(healthWidth, healthHeight));
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					indicator = healthTexture;
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					indicator = healthTextureoff;
				}

				batch.Draw(indicator, healthBarPos + new Vector2((healthWidth + 1) * y, 0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				batch.End();
			}

			healthTexture = TextureManager.GetTexture("GameHud/UnitBar/green");
			healthTextureoff = TextureManager.GetTexture("GameHud/UnitBar/greenoff");
			healthWidth = healthTexture.Width;
			healthHeight = healthTexture.Height;
			healthBarWidth = (healthWidth + 1) * unit.Type.Maxdetermination;
			emtpySpace = baseWidth - healthBarWidth;
			healthBarPos = new Vector2(emtpySpace / 2f, 25);


			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				var indicator = healthTexture;
				bool health = !(y >= unit.Determination);
				if (health)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(healthWidth, healthHeight));
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					indicator = healthTexture;
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					indicator = healthTextureoff;
				}

				batch.Draw(indicator, healthBarPos + new Vector2((healthWidth + 1) * y, 0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				batch.End();
			}

			columCounter++;
		}

		Debug.Assert(unitBarRenderTargets != null, nameof(unitBarRenderTargets) + " != null");
		int realWidth = unitBarRenderTargets[GameManager.GetMyTeamUnits()[0].WorldObject.ID].Width;
		int realHeight = unitBarRenderTargets[GameManager.GetMyTeamUnits()[0].WorldObject.ID].Height;
		//float scale = (float) (_unitBar.MaxWidth! / _unitBar.Widgets.Count/realWidth);

		bool twoLayer = false;

		float scale = (float) (_unitBar.MaxHeight! / 2 /realHeight);
		if (realWidth * scale * _unitBar.Widgets.Count > _unitBar.MaxWidth!)//only go for two layer if just downscaling to two layer size is not enough
		{
			twoLayer = true;
		}
		else
		{
			scale = Math.Min((float) (_unitBar.MaxWidth! / _unitBar.Widgets.Count/realWidth),(float)(_unitBar.MaxHeight! /realHeight));
		}
		
		int w = (int) (realWidth * scale);
		if (w % realWidth > 0) { w+= realWidth - w % realWidth; }
		float actualScale =  (float) w/realWidth;
		actualScale *= 0.95f;
		int h = (int) (realHeight * actualScale);
		columCounter = 0;
		if (!twoLayer)
		{
			_unitBar.Top = (int) ( (_unitBar.MaxHeight! - h)/2f);
		}

		foreach (var unit in GameManager.GetMyTeamUnits())
		{
			
			if(_unitBar.Widgets.Count > columCounter ){
				ImageButton elem = (ImageButton)_unitBar.Widgets[columCounter];
				int colum = columCounter;
				if(twoLayer&& columCounter >= _unitBar.Widgets.Count/2){
					colum = columCounter - _unitBar.Widgets.Count/2;
					elem.GridRow = 1;
				}
				else
				{
					elem.GridRow = 0;
				}
				elem.GridColumn = colum;
				elem.Width = w;
				elem.ImageWidth = w;
				elem.Height = h;
				elem.ImageHeight = h;
				elem.Background = new SolidBrush(Color.Transparent);
				elem.FocusedBackground = new SolidBrush(Color.Transparent);
				elem.OverBackground = new SolidBrush(Color.Transparent);
				elem.PressedBackground = new SolidBrush(Color.Transparent);
				elem.PressedImage = new TextureRegion(unitBarRenderTargets[unit.WorldObject.ID]);
				elem.Image = new TextureRegion(unitBarRenderTargets[unit.WorldObject.ID]);
				if (unit.Equals(SelectedUnit)){
					elem.Top = 10;
				}
				else
				{
					elem.Top = 0;
				}
			}
			columCounter++;
		}

		if(targetBar == null ||  targetBar.Widgets.Count < suggestedTargets.Count) return;
		if( suggestedTargets.Count == 0) return;
		foreach (var wo in suggestedTargets)
		{
			var unit = wo.UnitComponent;
			if (!targetBarRenderTargets.ContainsKey(wo.ID))
			{
				targetBarRenderTargets.Add(wo.ID, new RenderTarget2D(graphicsDevice, TextureManager.GetTextureFromPNG("Units/" + wo.Type.Name+"/Icon").Width,TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon").Height));

			}
			graphicsDevice.SetRenderTarget(targetBarRenderTargets[wo.ID]);
			graphicsDevice.Clear(Color.Transparent);
			if (unit != null && unit.IsMyTeam())
			{
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Green);	
			}
			else
			{
				PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Red);
			}

			batch.Begin(samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.OutLineEffect);
			batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
			batch.End();
		}
		
		
		columCounter = 0;
		foreach (var unit in suggestedTargets)
		{
			ImageButton elem = (ImageButton)targetBar.Widgets[columCounter];
			elem.GridRow = 0;
			elem.GridColumn = columCounter;
			elem.Width = w;
			elem.ImageWidth = w;
			elem.Height = h;
			elem.ImageHeight = h;
			elem.Background = new SolidBrush(Color.Transparent);
			elem.FocusedBackground = new SolidBrush(Color.Transparent);
			elem.OverBackground = new SolidBrush(Color.Transparent);
			elem.PressedBackground = new SolidBrush(Color.Transparent);
			elem.PressedImage = new TextureRegion(targetBarRenderTargets[unit.ID]);
			elem.Image = new TextureRegion(targetBarRenderTargets[unit.ID]);
			if (unit.Equals(ActionTarget)){
				elem.Top = 10;
			}
			else
			{
				elem.Top = 0;
			}
			
			columCounter++;
			
			
		}
		

	}

	private static Grid? _unitBar;
	private static Grid? targetBar;
	private static HorizontalStackPanel? targetBarStack;
	private static ImageButton? ConfirmButton;
	private static ImageButton? OverWatchToggle;
	public static Label? ScoreIndicator;
	private static ImageButton? endBtn;

	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{

		WorldManager.Instance.MakeFovDirty();
		var panel = new Panel ();
		if (GameManager.spectating || GameManager.PreGameData.SinglePLayerFeatures)
		{
			var swapTeam = new TextButton
			{
				Top = (int) (100f * globalScale.Y),
				Left = (int) (-10f * globalScale.X),
				Width = (int) (80 * globalScale.X),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Text = "Change POV",
				//Scale = globalScale
			};
			swapTeam.Click += (o, a) =>
			{
				GameManager.IsPlayer1 = !GameManager.IsPlayer1;
				WorldManager.Instance.MakeFovDirty();
				UI.SetUI(null);
			};
			panel.Widgets.Add(swapTeam);
		}
		var doAI = new TextButton
		{
			Top = (int) (200f * globalScale.Y),
			Left = (int) (-10f * globalScale.X),
			Width = (int) (80 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Text = "FinishTurnWithAI",
			//Scale = globalScale
		};
		doAI.Click += (o, a) =>
		{
			NetworkingManager.SendAITurn();
		};
		panel.Widgets.Add(doAI);
		
		if (!GameManager.spectating)
		{
			endBtn = new ImageButton()
			{
				Top = (int) (25f * globalScale.X),
				Left = (int) (-10.4f * globalScale.X),
				Width = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Width * globalScale.X * 0.9f),
				Height = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Height * globalScale.X * 0.9f),
				ImageWidth = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Width * globalScale.X * 0.9f),
				ImageHeight = (int) (TextureManager.GetTexture("GameHud/UnitBar/end button").Height * globalScale.X * 0.9f),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Background = new SolidBrush(Color.Transparent),
				OverBackground = new SolidBrush(Color.Transparent),
				PressedBackground = new SolidBrush(Color.Transparent),
				Image = new TextureRegion(TextureManager.GetTexture("GameHud/UnitBar/end button")),
			};
			endBtn.Click += (o, a) => GameManager.EndTurn();


			panel.Widgets.Add(endBtn);
		}

		if (inputBox == null)
		{
			inputBox = new TextBox();
			inputBox.Visible = false;
		}

	
		
		inputBox.HorizontalAlignment = HorizontalAlignment.Left;
		inputBox.Background = new SolidBrush(Color.Transparent);
		inputBox.Border = new SolidBrush(Color.Black);
		inputBox.BorderThickness = new Thickness(3);
		inputBox.Background = new SolidBrush(Color.Black*0.5f);
		inputBox.Left = (int) (0f * globalScale.X);
		inputBox.Width = (int) (200 * globalScale.Y);
		inputBox.Top = (int) (80 * globalScale.Y);
		inputBox.Font = DefaultFont.GetFont(FontSize / 3f);
		inputBox.TextColor = Color.White;
		inputBox.VerticalAlignment = VerticalAlignment.Center;
		inputBox.KeyDown += (sender, args) =>
		{
			if (args.Data == Keys.Enter)
			{
				if (inputBox.Text!= null && inputBox.Text.Length > 0)
				{
					Chat.SendMessage(inputBox.Text);
					inputBox.Text = "";
					inputBox.Visible = false;
					UI.Desktop.FocusedKeyboardWidget = null;
				}
			}

		};
		panel.Widgets.Add(inputBox);
		

		
		if (ScoreIndicator == null)
		{
			ScoreIndicator = new Label()
			{
				Top=150,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			SetScore(0);
		}

		panel.Widgets.Add(ScoreIndicator);

		_unitBar = new Grid()
		{
			GridColumnSpan = 4,
			GridRowSpan = 1,
			RowSpacing = 2,
			ColumnSpacing = 2,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Top,
			MaxWidth = (int)(365f*globalScale.X),
			//Width = (int)(365f*globalScale.X),
			MaxHeight = (int)(26f*globalScale.X),
			//Height = (int)(38f*globalScale.X),
			Top = (int)(0f*globalScale.Y),
			Left = (int)(-5f*globalScale.X),
			//ShowGridLines = true,
		};
		panel.Widgets.Add(_unitBar);
		targetBarStack = new HorizontalStackPanel();
		targetBarStack.HorizontalAlignment = HorizontalAlignment.Center;
		targetBarStack.VerticalAlignment = VerticalAlignment.Bottom;
		targetBarStack.Top = (int) (-230 * globalScale.Y);
		targetBarStack.MaxWidth = (int) (365f * globalScale.X);
		//	targetBarStack.Width = (int) (365f * globalScale.X);
	
		
		targetBar = new Grid()
		{
			GridColumnSpan = 4,
			GridRowSpan = 1,
			RowSpacing = 2,
			ColumnSpacing = 2,
			Padding = new Thickness(0),
			Margin = new Thickness(0),
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			//MaxWidth = (int)(365f*globalScale.X),
			//Width = (int)(50f*globalScale.X),
			//MaxHeight = (int)(26f*globalScale.X),
			//Height = (int)(38f*globalScale.X),
			Top = (int)(0f*globalScale.Y),
			Left = (int)(-5f*globalScale.X),
			ShowGridLines = true,
		};
	
		var left = new Image();
		left.Height = (int) (34f * globalScale.X);
		left.Width = (int) (16f * globalScale.X);
		left.Renderable = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/leftq"));
		//left.
		var right = new Image();
		right.Height = (int) (34f * globalScale.X);
		right.Width = (int) (16f * globalScale.X);
		right.Renderable = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/righte"));
		
		targetBarStack.Widgets.Add(left);
		targetBarStack.Widgets.Add(targetBar);
		targetBarStack.Widgets.Add(right);
		
		panel.Widgets.Add(targetBarStack);
		
		targetBarStack.Visible = false;
		

		ConfirmButton = new ImageButton();
		ConfirmButton.HorizontalAlignment = HorizontalAlignment.Center;
		ConfirmButton.VerticalAlignment = VerticalAlignment.Bottom;
		ConfirmButton.Top = (int) (-120 * globalScale.X);
		ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/confirm"));
		ConfirmButton.ImageWidth = (int) (80 * globalScale.X);
		ConfirmButton.Width = (int) (80 * globalScale.X);
		ConfirmButton.ImageHeight = (int) (20 * globalScale.X);
		ConfirmButton.Height = (int) (20 * globalScale.X);
		ConfirmButton.Click += (sender, args) =>
		{
			DoActiveAction();
		};
		ConfirmButton.Visible = (activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch);

		panel.Widgets.Add(ConfirmButton);
		
		OverWatchToggle = new ImageButton();
		OverWatchToggle.HorizontalAlignment = HorizontalAlignment.Center;
		OverWatchToggle.VerticalAlignment = VerticalAlignment.Bottom;
		OverWatchToggle.Top = (int) (-115 * globalScale.X);
		OverWatchToggle.Left = (int) (40 * globalScale.X);
	

		OverWatchToggle.ImageWidth = (int) (17 * globalScale.X);
		OverWatchToggle.Width = (int) (17 * globalScale.X);
		OverWatchToggle.ImageHeight = (int) (17 * globalScale.X);
		OverWatchToggle.Height = (int) (17 * globalScale.X);
		OverWatchToggle.Click += (sender, args) =>
		{
			ToggleOverWatch();
		};
		OverWatchToggle.Visible = false;
		

		panel.Widgets.Add(OverWatchToggle);
		
		foreach (var unit in new List<Unit>(GameManager.GetMyTeamUnits()))
		{
			var unitPanel = new ImageButton();

			Unit u = unit;
			unitPanel.Click += (sender, args) =>
			{
				SelectUnit(u);
			};
			_unitBar.Widgets.Add(unitPanel);
		}
		
		if (SelectedUnit == null) return panel;


		ActionButtons.Clear();
		var crouchbtn = new ImageButton();
		HudActionButton crouchHudBtn = new HudActionButton(crouchbtn, TextureManager.GetTexture("GameHud/BottomBar/crouch"),SelectedUnit,delegate(Unit unit, WorldObject target)
			{
				unit.DoAction(Action.ActionType.Crouch,new Action.ActionExecutionParamters());
			}, 
			delegate(Unit unit, WorldObject vector2Int)
			{
				if(unit.WorldObject.TileLocation.Position != vector2Int.TileLocation.Position) return new Tuple<bool, string>(false,"Target isnt self");
				if(unit.MovePoints<0) return new Tuple<bool, string>(false,"Not Enough Move Points");
				return new Tuple<bool, string>(true,"");
			}
			
			,new AbilityCost(0,0,1),"Crouching increases the benefit of cover and allows hiding behind tall cover.");

		panel.Widgets.Add(crouchbtn);

		ActionButtons.Add(crouchHudBtn);

		
		int i = 0;
		foreach (var action in SelectedUnit.Abilities)
		{
			var btn = new ImageButton();
			var hudBtn = new HudActionButton(btn,action,SelectedUnit);
			ActionButtons.Add(hudBtn);
			
			panel.Widgets.Add(btn);
			i++;
		}


		int top = (int) (-2*globalScale.X) ;
		float scale = globalScale.X * 0.9f;
		int totalBtns = ActionButtons.Count;
		int btnWidth = (int) (32 * scale);
		int totalWidth = totalBtns * btnWidth;
		int startOffest = Game1.resolution.X / 2 - totalWidth / 2;

		i = 0;
		int index = 0;
		foreach (var actionBtn in ActionButtons)
		{
			actionBtn.UpdateIcon();
			var UIButton = actionBtn.UIButton;
			UIButton.HorizontalAlignment = HorizontalAlignment.Left;
			UIButton.VerticalAlignment = VerticalAlignment.Bottom;
			UIButton.Width = (int) (32 * scale);
			UIButton.Height = (int) (32 * scale);
			UIButton.ImageHeight = (int) (32 * scale);
			UIButton.ImageWidth = (int) (32 * scale);
			UIButton.Top = top;
			UIButton.Left = startOffest + btnWidth*(index);
			index++;
		}


	
		
		return panel;
	}




	private static readonly List<HudActionButton> ActionButtons = new();
	
	private static void DoActiveAction(bool force = false)
	{
		if(ActionTarget == null) return;
		if (activeAction != ActiveActionType.Action && activeAction != ActiveActionType.Overwatch) return;

		if (force||activeAction == ActiveActionType.Overwatch)
		{
			if (!HudActionButton.SelectedButton.HasPoints()) return;//only check for point is we're forcing
		}
		else
		{

			var res = HudActionButton.SelectedButton.CanPerformAction(ActionTarget);
			if (!res.Item1)
			{
				new PopUpText(res.Item2, ActionTarget.TileLocation.Position, Color.Red);
				new PopUpText(res.Item2, SelectedUnit.WorldObject.TileLocation.Position, Color.Red);
				return;
			}

		}

	

		if (activeAction == ActiveActionType.Action)
		{
			HudActionButton.SelectedButton!.PerformAction(ActionTarget);
		}
		else
		{
			HudActionButton.SelectedButton!.OverwatchAction(ActionTarget.TileLocation.Position);
		}


		SelectHudAction(null);
	}
	


	public static WorldObject? ActionTarget;
	
	private static List<WorldObject> suggestedTargets = new();
	private int GetSelectedTargetIndex() {
		for (int i = 0; i < suggestedTargets.Count; i++)
		{
			if (suggestedTargets[i].Equals(ActionTarget))
			{
				return i;
			}
		}

		return -1;
	}

	private static readonly Dictionary<WorldObject,List<SequenceAction>> SortedConsequences = new();
	List<SequenceAction> previewConsequences = new List<SequenceAction>();
	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		if(SelectedUnit==null) return;
		
		base.RenderBehindHud(batch, deltatime);
		MakeUnitBarRenders(batch);
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Immediate, samplerState: SamplerState.PointClamp);
		
		
		var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
	
		var mousepos = Utility.GridToWorldPos(TileCoordinate+new Vector2(-1.5f,-0.5f));
		for (int i = 0; i < 8; i++)
		{
		
			var indicator = TextureManager.GetSpriteSheet("coverIndicator",3,3)[i];
			Color c = Color.White;
			switch (WorldManager.Instance.GetCover(TileCoordinate,(Direction) i))
			{
				case Cover.Full:
					c = Color.Red;
					break;
				case Cover.High:
					c = Color.Yellow;
					break;
				case Cover.Low:
					c = Color.Green;
					break;
			}


			batch.Draw(indicator, mousepos, c);
		}

		var args = new Action.ActionExecutionParamters(TileCoordinate);
		
		previewConsequences.Clear();
		switch (activeAction)
		{
			case ActiveActionType.Move:
				Action.Actions[Action.ActionType.Move].Preview(SelectedUnit, args, batch);
				break;
			case ActiveActionType.Face:
				Action.Actions[Action.ActionType.Face].Preview(SelectedUnit, args, batch);
				break;
			case ActiveActionType.Action:
				if (HudActionButton.SelectedButton == null)
				{
					throw new Exception("Action as active action without selected action button");
				}

				if (ActionTarget != null) previewConsequences.AddRange( HudActionButton.SelectedButton.Preview(ActionTarget, batch));
				break;
			case ActiveActionType.Overwatch:
				if (ActionTarget != null) HudActionButton.SelectedButton.PreviewOverwatch(ActionTarget.TileLocation.Position, batch);
				break;
		}
		SortedConsequences.Clear();
		foreach (var act in previewConsequences)
		{
			if(act.GetType() == typeof(FaceUnit)) continue;
			if (act.GetType() == typeof(WorldObjectManager.TakeDamage))
			{
				var tkDmg = (WorldObjectManager.TakeDamage) act;
				WorldObject? obj = tkDmg.GetTargetObject();
				if(obj == null) continue;
				if (!SortedConsequences.ContainsKey(obj))
				{
					SortedConsequences.Add(obj,new List<SequenceAction>());
				}
				SortedConsequences[obj].Add(act);
			
			}if (act.GetType() == typeof(Shoot))
			{
				var shoot = (Shoot) act;
				var obj = WorldObjectManager.GetObject(shoot.Projectile.Result.HitObjId);
				if(obj == null) continue;
				if (!SortedConsequences.ContainsKey(obj))
				{
					SortedConsequences.Add(obj,new List<SequenceAction>());
				}
				SortedConsequences[obj].Add(act);
			
			}else if (act.IsUnitAction)
			{
				var uact = (UnitSequenceAction) act;
				var actor = uact.GetAffectedActor(-1);
				if (actor != null)
				{
					var obj = actor.WorldObject;
					if (!SortedConsequences.ContainsKey(obj))
					{
						SortedConsequences.Add(obj,new List<SequenceAction>());
					}
					SortedConsequences[obj].Add(act);
				}
				
			}
		}
		
		

		batch.End();

		if (activeAction == ActiveActionType.Action)
		{
			foreach (var target in suggestedTargets)
			{

				if (!target.IsVisible()) continue;
				if (target.UnitComponent!=null && target.UnitComponent.IsMyTeam())
				{
					PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Green);
				}
				else
				{
					PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Red);
				}

				batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.OutLineEffect);
				Texture2D sprite = target.GetTexture();
				batch.Draw(sprite, target.GetDrawTransform().Position + Utility.GridToWorldPos(new Vector2(1.5f, 0.5f)), null, Color.White, 0, sprite.Bounds.Center.ToVector2(), Math.Max(1, 2f / Camera.GetZoom()), SpriteEffects.None, 0);
				batch.End();
			}
		}
		
		
		
		
		panicAnimCounter += deltatime/100f;
		if (panicAnimCounter >= 4)
		{
			panicAnimCounter = 0;
		}

		counter += deltatime/1000f;
		if (counter > 2)
		{
			counter= 0;
		}
		animopacity = counter;
		if (counter > 1)
		{
			animopacity = 2- counter;
		}

		var tile = WorldManager.Instance.GetTileAtGrid(TileCoordinate);
		if (drawExtra)
		{
			foreach (var obj in tile.ObjectsAtLocation)
			{
				if (obj.LifeTime > 0&& obj.IsVisible())
				{
					batch.Begin(transformMatrix: Camera.GetViewMatrix(),samplerState: SamplerState.AnisotropicClamp);
					batch.DrawText(""+obj.LifeTime, Utility.GridToWorldPos(tile.Position + new Vector2(0f,0f)),  5,5, Color.White);
					//obj.Type.DesturctionEffect?.Preview(obj.TileLocation.Position,batch,null);
					batch.End();
				}

			}

			foreach (var edge in tile.GetAllEdges())
			{
				DrawHoverHud(batch, edge);
			}


		}
		var tiles = WorldManager.Instance.GetTilesAround(TileCoordinate, 10);
		foreach (var t in tiles)
		{
			foreach (var edge in t.GetAllEdges())
			{ 
				if (!Equals(edge.PreviewData, new PreviewData()))
				{ 
					DrawHoverHud(batch, edge);
				}
			}
		}
		foreach (var controllable in GameManager.GetAllUnits())
		{
			if (controllable.WorldObject.TileLocation != null)// && controllable.WorldObject.IsVisible()
			{
				DrawHoverHud(batch, controllable.WorldObject);
			}
		}
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		if (SelectedEnemyUnit != null)
		{
			batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		
			foreach (var t in SelectedEnemyUnit?.VisibleTiles)
			{
				Color c = Color.White;
				if (t.Value == Visibility.Partial)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("eye"), Utility.GridToWorldPos(t.Key+new Vector2(0.5f,0.5f)), null, c, 0, Vector2.Zero, 1f, SpriteEffects.None, 0);

			}
			batch.End();
		}

		
		
		
		graphicsDevice.SetRenderTarget(timerRenderTarget);
	
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		var frame = TextureManager.GetTexture("GameHud/UnitBar/unitframe");
		batch.Draw(frame, new Vector2(0,0), null, Color.White, 0, Vector2.Zero, 1,SpriteEffects.None, 0);
		
		var totalLenght = 259 + 30;
		var fraction = GameManager.TimeTillNextTurn / (GameManager.PreGameData.TurnTime * 1000);
		var displayLenght = totalLenght - totalLenght * fraction;
		
		batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/Timer"), Vector2.Zero, null, Color.Gray, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.Draw(TextureManager.GetTexture("GameHud/UnitBar/Timer"), Vector2.Zero, new Rectangle(0,0,190+(int)displayLenght,80), Color.White, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.End();
		PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(TextureManager.GetTexture("GameHud/UnitBar/enemyTurn").Width,TextureManager.GetTexture("GameHud/UnitBar/enemyTurn").Height),false);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostProcessing.PostProcessing.UIGlowEffect);
		var turn = TextureManager.GetTexture("GameHud/UnitBar/enemyTurn");
		if (GameManager.IsMyTurn())
		{		
			turn = TextureManager.GetTexture("GameHud/UnitBar/yourTurn");
		}

		batch.Draw(turn, new Vector2(0, 0), null,Color.White, 0, Vector2.Zero, new Vector2(1f,1f),SpriteEffects.None, 0);
		batch.End();

		//final Draw
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		//batch.Draw(rightCornerRenderTarget, new Vector2(Game1.resolution.X - (rightCornerRenderTarget.Width-104)*globalScale.Y*1.3f, Game1.resolution.Y - rightCornerRenderTarget.Height*globalScale.Y*1.3f), null, Color.White, 0, Vector2.Zero, globalScale.Y*1.3f ,SpriteEffects.None, 0);

	
		batch.Draw(timerRenderTarget, new Vector2(Game1.resolution.X-timerRenderTarget.Width*globalScale.X*0.9f, 0), null, Color.White, 0, Vector2.Zero, globalScale.X*0.9f ,SpriteEffects.None, 0);
		
		Texture2D bar = TextureManager.GetTexture("GameHud/BottomBar/mainbuttonbox");
		var box = TextureManager.GetTexture("GameHud/BottomBar/Infobox");
		if (activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch)
		{
			tooltipPos = new Vector2((Game1.resolution.X - box.Width * globalScale.X) / 2f, Game1.resolution.Y - (box.Height+2) * globalScale.X);
		}
		else
		{
			tooltipPos = new Vector2((Game1.resolution.X - box.Width * globalScale.X) / 2f, Game1.resolution.Y - (bar.Height+10) * globalScale.X);
		}

		batch.Draw(box, tooltipPos, null, Color.White, 0, Vector2.Zero, globalScale.X,SpriteEffects.None, 0);
		batch.End();
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		//centered
		batch.Draw(bar, new Vector2((Game1.resolution.X - bar.Width*globalScale.X)/2f, Game1.resolution.Y - bar.Height*globalScale.X), null, Color.White, 0, Vector2.Zero, globalScale.X ,SpriteEffects.None, 0);
		string chatmsg = "";
		int extraLines = 0;
		int width = 40;
		foreach (var msg in Chat.Messages)
		{
			chatmsg +=msg+"\n";
			if (msg.Length > width)
			{
				extraLines++;
			}

			extraLines++;
		}
		batch.DrawText(chatmsg,new Vector2(15,-7*extraLines+240*globalScale.Y),1.5f,width,Color.White);
		batch.End();
		
		
	}
	private static Vector2 tooltipPos = new(0,0);

	public override void RenderFrontHud(SpriteBatch batch, float deltatime)
	{
		if(SelectedUnit==null) return;
		base.RenderFrontHud(batch, deltatime);
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);

		char[] characters = { 'Z','X','C','V', 'B', 'N' };
		for (int i = 0; i < ActionButtons.Count && i < characters.Length; i++)
		{
			batch.DrawText(characters[i].ToString(), new Vector2(ActionButtons[i].UIButton.Left + 12 * globalScale.Y + 3 * globalScale.Y, ActionButtons[i].UIButton.Top + Game1.resolution.Y - 20 * globalScale.Y), globalScale.Y * 1.6f, 1, Color.White);
		}


		if (activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch)
		{

			string toolTipText = HudActionButton.SelectedButton!.Tooltip;

			batch.DrawText(toolTipText, tooltipPos + new Vector2(15, 10) * globalScale.X, globalScale.X * 0.6f, 40, Color.White);
			Vector2 startpos = tooltipPos + new Vector2(5, 40f) * globalScale.X;


			AbilityCost cost = HudActionButton.SelectedButton.Cost;
			Vector2 offset = new Vector2(5, 0);
			Color c = Color.Green;


			if (cost.MovePoints > 0)
			{
				if (cost.MovePoints > SelectedUnit.MovePoints)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("GameHud/BottomBar/tinyBox"), startpos + offset * globalScale.X + new Vector2(0, 0) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
				batch.Draw(TextureManager.GetTexture("HoverHud/movepoint"), startpos + offset * globalScale.X + new Vector2(5, 3) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
				batch.DrawText(cost.MovePoints + "", startpos + offset * globalScale.X + new Vector2(30, 2) * globalScale.X, globalScale.X * 1.5f, 24, c);
			}

			if (cost.ActionPoints > 0)
			{
				offset += new Vector2(40, 0);
				c = Color.Green;
				if (cost.ActionPoints > SelectedUnit.ActionPoints)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("GameHud/BottomBar/tinyBox"), startpos + offset * globalScale.X + new Vector2(0, 0) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
				batch.Draw(TextureManager.GetTexture("HoverHud/actionpoint"), startpos + offset * globalScale.X + new Vector2(5, 3) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
				batch.DrawText(cost.ActionPoints + "", startpos + offset * globalScale.X + new Vector2(30, 2) * globalScale.X, globalScale.X * 1.5f, 24, c);
			}

			if (cost.Determination > 0)
			{
				offset += new Vector2(40, 0);
				c = Color.Green;
				if (cost.Determination > SelectedUnit.Determination)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("GameHud/BottomBar/tinyBox"), startpos + offset * globalScale.X + new Vector2(0, 0) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
				batch.Draw(TextureManager.GetTexture("HoverHud/detgreen"), startpos + offset * globalScale.X + new Vector2(3, 5) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.1f, SpriteEffects.None, 0);
				batch.DrawText(cost.Determination + "", startpos + offset * globalScale.X + new Vector2(30, 2) * globalScale.X, globalScale.X * 1.5f, 24, c);
			}

			if (ActionTarget != null)
			{
				if (activeAction == ActiveActionType.Action)
				{
					var res = HudActionButton.SelectedButton.CanPerformAction(ActionTarget);
					if (!res.Item1)
					{
						batch.DrawText(res.Item2, startpos + new Vector2(12, 25) * globalScale.X, globalScale.X, 25, Color.DarkRed);

					}
				}
				else
				{
					batch.DrawText("Using overwatch will prevent this unit from  doing any other actions this turn", startpos + new Vector2(12, 25) * globalScale.X, globalScale.X / 2f, 50, Color.Yellow);
				}

			}
			
		}
		batch.End();
	

		if (drawExtra)
		{
			return;/*
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
			Move.MoveCalcualtion details;
			Move.MoveCalcualtion details2;
			var path = PathFinding.GetPath(SelectedUnit.WorldObject.TileLocation.Position, TileCoordinate);
			int moveUse = 1;
			while (path.Cost > SelectedUnit.GetMoveRange()*moveUse)
			{
				moveUse++;
			}
			//int res = AI.Move.GetTileMovementScore(TileCoordinate,moveUse,false,SelectedUnit, out details);
			int res2 = AI.Move.GetTileMovementScore(TileCoordinate,moveUse,true, SelectedUnit, out details2);
			//details = details2;
			//var res = res2;
			string text = $" Total: {res}\n Closest Distance: {details.ClosestDistance}\n Distance Reward: {details.DistanceReward}\n ProtectionPenalty: {details.ProtectionPentalty}\n";
			

			text += $" Clumping Penalty: {details.ClumpingPenalty}\n  Damage Potential: {details.DamagePotential}\n Cover Bonus: {details.CoverBonus}\n";
			batch.Begin(samplerState: SamplerState.AnisotropicClamp);
			batch.DrawText(text,Vector2.One,  3,100, Color.Green);
			batch.End();
			
			
			
			string text2 = $" Total: {res2}\n Closest Distance: {details2.ClosestDistance}\n Distance Reward: {details2.DistanceReward}\n ProtectionPenalty: {details2.ProtectionPentalty}\n";
			

			text2 += $" Clumping Penalty: {details2.ClumpingPenalty}\n Damage Potential: {details2.DamagePotential}\n Cover Bonus: {details2.CoverBonus}\n ";
			batch.Begin(samplerState: SamplerState.AnisotropicClamp);
			batch.DrawText(text2,new Vector2(700,0),  3,100, Color.Red);
			batch.End();
			AIMoveCache[(int) TileCoordinate.X, (int) TileCoordinate.Y, 1] = res2;
			AIMoveCache[(int) TileCoordinate.X, (int) TileCoordinate.Y, 0] = res;*/
		}

	

	}

	private Vector2Int _mouseTileCoordinate = new(0, 0);
	private Vector2Int _lastMouseTileCoordinate = new(0, 0);

	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		var count = 0;
	

		//moves selected contorlable to the top
		_mouseTileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		_mouseTileCoordinate = Vector2.Clamp(_mouseTileCoordinate, Vector2.Zero, new Vector2(99, 99));

		if (_mouseTileCoordinate != _lastMouseTileCoordinate)
		{
			switch (activeAction)
			{
				case ActiveActionType.Move:
				case ActiveActionType.Face:
					activeAction = ActiveActionType.None;
					break;
			}
		}

		int targetIndex = GameManager.GetAllUnits().IndexOf(SelectedUnit);
		if (targetIndex != -1)
		{
			for (int i = targetIndex; i < GameManager.GetAllUnits().Count - 1; i++)
			{
				GameManager.GetAllUnits()[i] = GameManager.GetAllUnits()[i + 1];
				GameManager.GetAllUnits()[i + 1] = SelectedUnit;
			}
		}

		var tile = WorldManager.Instance.GetTileAtGrid(_mouseTileCoordinate);
		
		if (tile.UnitAtLocation != null)
		{
			targetIndex = GameManager.GetAllUnits().IndexOf(tile.UnitAtLocation);
			if (targetIndex != -1)
			{
				Unit target = GameManager.GetAllUnits()[targetIndex];
				for (int i = targetIndex; i < GameManager.GetAllUnits().Count - 1; i++)
				{
						
					GameManager.GetAllUnits()[i] = GameManager.GetAllUnits()[i + 1];
					GameManager.GetAllUnits()[i + 1] = target;
				}
			}
			
		}

		ProcessKeyboard();
		
		//bad
		if (!GameManager.IsMyTurn())
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/UnitBar/end button")), Color.Gray);
		}
		else
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("GameHud/UnitBar/end button")), Color.White);
		}
		_lastMouseTileCoordinate = _mouseTileCoordinate;
	}

	private bool drawExtra;
	
	public void ProcessKeyboard()
	{
		//Console.WriteLine(UI.Desktop.FocusedKeyboardWidget);
		if (lastKeyboardState.IsKeyDown(Keys.Tab))
		{
			UI.Desktop.FocusedKeyboardWidget = null;//override myra focus switch functionality
		}
		if(inputBox != null && inputBox.IsKeyboardFocused) return;
		
		if(UI.Desktop.FocusedKeyboardWidget != null) return;

		drawExtra = false;
		if (currentKeyboardState.IsKeyDown(Keys.LeftAlt))
		{
			drawExtra = true;
		}

		if (JustPressed(Keys.Enter))
		{
			inputBox.Visible = true;
			inputBox.SetKeyboardFocus();
		}


		currentKeyboardState.IsKeyDown(Keys.LeftControl);


		if (suggestedTargets.Count > 0)
		{
			if (JustPressed(Keys.Q))
			{
				int idx = GetSelectedTargetIndex();
				if (idx <= 0)
				{
					ActionTarget = suggestedTargets[suggestedTargets.Count - 1];
				}
				else
				{
					ActionTarget = suggestedTargets[idx - 1];
				}

				Camera.SetPos(ActionTarget.TileLocation.Position);
			}
			else if (JustPressed(Keys.E))
			{
				int idx = GetSelectedTargetIndex();
				if (idx >= suggestedTargets.Count - 1)
				{
					ActionTarget = suggestedTargets[0];
				}
				else
				{
					ActionTarget = suggestedTargets[idx + 1];
				}

				Camera.SetPos(ActionTarget.TileLocation.Position);
			}
		}

		if (JustPressed(Keys.Z))
		{
			if (ActionButtons.Count > 0)
			{
				ActionButtons[0].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.X))
		{
			if (ActionButtons.Count >1)
			{
				ActionButtons[1].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.C))
		{
			if (ActionButtons.Count > 2)
			{
				ActionButtons[2].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.V))
		{
			if (ActionButtons.Count > 3)
			{
				ActionButtons[3].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.B))
		{
			if (ActionButtons.Count > 4)
			{
				ActionButtons[4].UIButton.DoClick();
			}
		}else if (JustPressed(Keys.N))
		{
			if (ActionButtons.Count >5)
			{
				ActionButtons[5].UIButton.DoClick();
			}
		}

		if (JustPressed(Keys.D1) && GameManager.GetMyTeamUnits().Count > 0)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[0]);
		}else if (JustPressed(Keys.D2) && GameManager.GetMyTeamUnits().Count > 1)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[1]);
		}else if (JustPressed(Keys.D3) && GameManager.GetMyTeamUnits().Count > 2)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[2]);
		}else if (JustPressed(Keys.D4) && GameManager.GetMyTeamUnits().Count > 3)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[3]);
		}else if (JustPressed(Keys.D5) && GameManager.GetMyTeamUnits().Count > 4)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[4]);
		}else if (JustPressed(Keys.D6) && GameManager.GetMyTeamUnits().Count > 5)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[5]);
		}else if (JustPressed(Keys.D7) && GameManager.GetMyTeamUnits().Count > 6)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[6]);
		}else if (JustPressed(Keys.D8) && GameManager.GetMyTeamUnits().Count > 7)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[7]);
		}else if (JustPressed(Keys.D9) && GameManager.GetMyTeamUnits().Count > 8)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[8]);
		}else if (JustPressed(Keys.D0) && GameManager.GetMyTeamUnits().Count > 9)
		{
			SelectUnit(GameManager.GetMyTeamUnits()[9]);
		}

		if (JustPressed(Keys.Space))
		{
			if (currentKeyboardState.IsKeyDown(Keys.LeftControl))
			{
				ToggleOverWatch();
			}
			else if(currentKeyboardState.IsKeyDown(Keys.LeftShift))
			{
				DoActiveAction(true);
			}
			else
			{
				DoActiveAction();
			}

			
		}

	}

	private enum ActiveActionType
	{
		Move,
		Face,
		Action,
		None,
		Overwatch,
		EnemyPreview
	}

	private static ActiveActionType activeAction = ActiveActionType.None;

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);
		

		position = Vector2.Clamp(position, Vector2.Zero, new Vector2(99, 99));
		
		var tile =WorldManager.Instance.GetTileAtGrid( position);

		if (tile.UnitAtLocation != null&& tile.UnitAtLocation.WorldObject.GetMinimumVisibility() <= tile.GetVisibility() && (activeAction == ActiveActionType.None || activeAction == ActiveActionType.Move)) { 
			SelectUnit(tile.UnitAtLocation);
			return;
		}
		
		if(SelectedUnit == null) return;
		SelectedEnemyUnit = null;
		if (rightclick)
		{
			switch (activeAction)
			{
				case ActiveActionType.None:
					activeAction = ActiveActionType.Face;
					ActionTarget = tile.Surface;
					break;
				case ActiveActionType.Face:
					SelectedUnit.DoAction(Action.ActionType.Face, new Action.ActionExecutionParamters(position));
					break;
				case ActiveActionType.Action:
				case ActiveActionType.Overwatch:
					SelectHudAction(null);
					break;
					

			}
		}
		else
		{
			switch (activeAction)
			{

				case ActiveActionType.None:
					activeAction = ActiveActionType.Move;
					ActionTarget = tile.Surface;
					break;
				case ActiveActionType.Face:
					activeAction = ActiveActionType.None;
					break;
				case ActiveActionType.Move:
					SelectedUnit.DoAction(Action.ActionType.Move, new Action.ActionExecutionParamters(position));
					break;
				case ActiveActionType.Action:
					var edgeList = tile.GetAllEdges();
					if (drawExtra && edgeList.Count>0)
					{

						int currentlySelected = -1;
						if(ActionTarget != null)
						{
							currentlySelected = edgeList.IndexOf(ActionTarget);
						}
						if (currentlySelected == -1)
						{
							ActionTarget = edgeList[0];
						}
						else
						{
							if (currentlySelected == edgeList.Count - 1)
							{
								ActionTarget = edgeList[0];
							}
							else
							{
								ActionTarget = edgeList[currentlySelected + 1];
							}
						}
					}
					else
					{
						if(tile.UnitAtLocation!= null)
						{
							ActionTarget = tile.UnitAtLocation.WorldObject;
						}
						else
						{
							ActionTarget = tile.Surface;
						}
					}

				

					break;
				case ActiveActionType.Overwatch:
					ActionTarget = WorldManager.Instance.GetTileAtGrid(position).Surface;
					break;

			}

		}
	}


	private static float counter;
	private static float animopacity;
	private static TextBox? inputBox;
	private static float panicAnimCounter;

	
	
	public static void DrawHoverHud(SpriteBatch batch, WorldObject obj)
	{

		Unit? unit = obj.UnitComponent;
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		graphicsDevice.SetRenderTarget(consequenceListRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		if (SortedConsequences.TryGetValue(obj, out var list))
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
			Vector2 pos = new Vector2(0, 0);
			foreach (var cons in list)
			{	
				batch.Draw(TextureManager.GetTexture("HoverHud/consequenceFrame"),pos,null,Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
				cons.DrawDesc(pos+new Vector2(10,4),batch);
				pos+= new Vector2(0, TextureManager.GetTexture("HoverHud/consequenceFrame").Height);
			}
			batch.End();
		}

		
		graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		float opacity = 0.9f;
		

		if (Equals(SelectedUnit, unit) || MousePos == obj.TileLocation.Position || (obj.Type.Edge && Utility.IsClose(obj,MousePos)))
		{
			opacity = 1;
		}

		var healthTexture = TextureManager.GetTexture("HoverHud/health");
		var nohealthTexture = TextureManager.GetTexture("HoverHud/nohealth");

		if (unit == null)
		{
			healthTexture = TextureManager.GetTexture("HoverHud/healthenv");
			nohealthTexture = TextureManager.GetTexture("HoverHud/nohealthenv");
		}
		float healthWidth = healthTexture.Width;
		float baseWidth = hoverHudRenderTarget.Width;
		float healthBarWidth = healthWidth*obj.Type.MaxHealth;
		float emtpySpace = baseWidth - healthBarWidth;
		//Vector2 healthBarPos = new Vector2(emtpySpace/2f,36);
		Vector2 healthBarPos = new Vector2(5f,25);


		Vector2 offset = new Vector2(0,0);
	

		for (int y = 0; y < obj.Type.MaxHealth; y++)
		{
			if (y >= obj.Health)
			{

				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(nohealthTexture, healthBarPos + new Vector2(healthWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
				batch.End();
			}
			else
			{
				PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(hoverHudRenderTarget.Width,hoverHudRenderTarget.Height), false);
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect:PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(healthTexture, healthBarPos + new Vector2(healthWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
				batch.End();
				if (obj.PreviewData.finalDmg >= obj.Health - y)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(TextureManager.GetTexture("HoverHud/detGreen").Width,TextureManager.GetTexture("HoverHud/detGreen").Height), true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(healthTexture, healthBarPos + new Vector2(healthWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				
				}

			}
		}

		if (unit != null)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);

			if (unit.Overwatch.Item1)
			{
				batch.Draw(unit.Abilities[unit.Overwatch.Item2].Icon,new Vector2(0,0),null,Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
			}

			int i = 0;
			
			foreach (var effect in unit.StatusEffects)
			{
				batch.Draw(TextureManager.GetTextureFromPNG("Icons/"+effect.type.name),new Vector2(23*i,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.DrawText(effect.duration+"", new Vector2(23*i+10,0), 1f, 100, Color.White);
				i++;
			}
			batch.End();

			float detWidth = 0;
			float detHeight = 0;
			float detbarWidht;
			float detEmtpySpace;
			Vector2 DetPos = default;
		

			detWidth = TextureManager.GetTexture("HoverHud/detGreen").Width;
			detHeight = TextureManager.GetTexture("HoverHud/detGreen").Height;
			detbarWidht = detWidth * unit.Type.Maxdetermination;
			detEmtpySpace = baseWidth - detbarWidht;
			//DetPos = new Vector2(detEmtpySpace / 2f, 28);
			DetPos = new Vector2(5f, 20);

			
			var animSheet = TextureManager.GetSpriteSheet("HoverHud/panicSheet", 1, 4);
			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				if (unit.Paniced)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(animSheet[(int) panicAnimCounter], DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					continue;
				}

				if (y == unit.Determination)
				{
					batch.Begin(sortMode: SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.Draw(TextureManager.GetTexture("HoverHud/detYellow"), DetPos + new Vector2(detWidth * y, 0), null, Color.White * (animopacity + 0.1f), 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else if (y >= unit.Determination)
				{

					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(hoverHudRenderTarget.Width, hoverHudRenderTarget.Height), false);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("HoverHud/detGreen"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					if (unit.WorldObject.PreviewData.detDmg >= unit.Determination - y)
					{
						PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(TextureManager.GetTexture("HoverHud/detGreen").Width, TextureManager.GetTexture("HoverHud/detGreen").Height), true);
						batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
						batch.Draw(TextureManager.GetTexture("HoverHud/detGreen"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
						batch.End();

					}

				}



			}
		
			Vector2Int pointPos = new Vector2Int((int)healthBarPos.X, 33);
			int o = 5;
			i = 0;
			var nextT = unit.GetPointsNextTurn();
			for (int j = 0; j < Math.Max(nextT.Item1,unit.MovePoints.Current); j++)
			{
				Texture2D t;
				if (j < unit.MovePoints)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(hoverHudRenderTarget.Width,hoverHudRenderTarget.Height), false);
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead,effect:PostProcessing.PostProcessing.UIGlowEffect);
					t = TextureManager.GetTexture("HoverHud/movepoint");
				
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					t = TextureManager.GetTexture("HoverHud/nomovepoint");
				}

				batch.Draw(t, pointPos + new Vector2(o * i, 0), Color.White);
				batch.End();
				i++;
			}

			i++;
			
			for (int j = 0; j < Math.Max(nextT.Item2,unit.ActionPoints.Current); j++)
			{	
				Texture2D t;
				if (j < unit.ActionPoints)
				{
					PostProcessing.PostProcessing.ApplyUIEffect(new Vector2(hoverHudRenderTarget.Width,hoverHudRenderTarget.Height), false);
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead,effect:PostProcessing.PostProcessing.UIGlowEffect);
					t = TextureManager.GetTexture("HoverHud/actionpoint");
				
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					t = TextureManager.GetTexture("HoverHud/noactionpoint");
				}
				batch.Draw(t, pointPos + new Vector2(o * i, 0), Color.White);
				batch.End();
				i++;
			}
		}
		else
		{
			switch (obj.Facing)
			{
				case Direction.North:
					offset = new Vector2(1, 1);
					break;
				case Direction.West:
					offset = new Vector2(0.5f, 1);
					break;
			}
		}

		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);

		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2) obj.TileLocation.Position + offset) + new Vector2(-10, -100),null,Color.White*opacity,0,Vector2.Zero,2f,SpriteEffects.None,0);
		batch.Draw(consequenceListRenderTarget,Utility.GridToWorldPos((Vector2) obj.TileLocation.Position + offset) + new Vector2(-5, -60)+new Vector2(-78,0),null,Color.White*opacity,0,Vector2.Zero,0.7f,SpriteEffects.None,0);
		batch.End();
	}

	private static void ToggleOverWatch()
	{
		if (activeAction == ActiveActionType.Action)
		{
			if (HudActionButton.SelectedButton!.CanOverwatch)
			{
				activeAction = ActiveActionType.Overwatch;
			}
			else
			{
				activeAction = ActiveActionType.Action;
			}


		}
		else if(activeAction == ActiveActionType.Overwatch)
		{
			activeAction = ActiveActionType.Action;
		}

		UpdateHudButtons();
	}

	private static void UpdateHudButtons()
	{
		if(ConfirmButton is null) return;
		foreach (var act in ActionButtons)
		{
			act.UpdateIcon();
		}

		switch (activeAction)
		{
			case ActiveActionType.None:
				ConfirmButton.Visible = false;
				targetBarStack!.Visible = false;
				OverWatchToggle.Visible = false;
				return;
				break;
			case ActiveActionType.Overwatch:
				OverWatchToggle.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/overwatchON"));
				ConfirmButton!.Visible = true;
				targetBarStack!.Visible = false;
				OverWatchToggle.Visible = true;
				suggestedTargets.Clear();
				break;
			case ActiveActionType.Action:
				ConfirmButton!.Visible = true;
				targetBarStack!.Visible = true;
				OverWatchToggle.Visible = false;
				if (HudActionButton.SelectedButton.CanOverwatch)
				{
					OverWatchToggle.Image = new TextureRegion(TextureManager.GetTexture("GameHud/BottomBar/overwatchOFF"));
					OverWatchToggle.Visible = true;
				}

				List<Unit> potentialTargets = new();
				GameManager.GetMyTeamUnits().ForEach(x => potentialTargets.Add(x));
				GameManager.GetEnemyTeamUnits().ForEach(x => potentialTargets.Add(x));
				suggestedTargets = HudActionButton.SelectedButton.GetSuggestedTargets(potentialTargets);

				if (suggestedTargets.Count > 0)
				{
					ActionTarget = suggestedTargets[0];

				}

				break;



		}



		if (suggestedTargets.Count < 2) targetBarStack.Visible = false;

		foreach (var unit in suggestedTargets)
		{
			var unitPanel = new ImageButton();

			Unit u = unit.UnitComponent;
			unitPanel.Click += (sender, args) =>
			{
				ActionTarget = unit;
				Console.WriteLine("Target set to " + ActionTarget);
			};
			targetBar!.Widgets.Add(unitPanel);
		}

		targetBarStack!.Proportions!.Clear();
		targetBarStack!.Proportions!.Add(new Proportion(ProportionType.Pixels, 50));
		targetBarStack!.Proportions!.Add(new Proportion(ProportionType.Pixels, 200 * suggestedTargets.Count));
		targetBarStack!.Proportions!.Add(new Proportion(ProportionType.Pixels, 50));
	}

	public static void SelectHudAction(HudActionButton? hudActionButton)
	{
		HudActionButton.SelectedButton = hudActionButton;
		activeAction = ActiveActionType.Action;
		if (hudActionButton == null)
		{
			activeAction = ActiveActionType.None;
		}

		UpdateHudButtons();
	}

	public static void CleanUp()
	{
		ActionButtons.Clear();
	}
}
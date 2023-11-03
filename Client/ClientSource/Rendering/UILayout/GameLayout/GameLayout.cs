﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DefconNull.Networking;
using DefconNull.World;
using DefconNull.World.WorldActions;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;
using Move = DefconNull.AI.Move;
using Thickness = Myra.Graphics2D.Thickness;

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

	public static void SelectUnit(Unit? controllable)
	{
		if (controllable == null)
		{
			controllable = MyUnits.FirstOrDefault();
		}
		if(controllable is null)return;

		if (!controllable.IsMyTeam())
		{
			return;
		}
		SelectHudAction(null);

		SelectedUnit = controllable;
		ReMakeMovePreview();
		UI.SetUI( new GameLayout());
		Camera.SetPos(controllable.WorldObject.TileLocation.Position);
	}

	public static readonly int[,,] AIMoveCache = new int[100,100,2];
	public static void ReMakeMovePreview()
	{
		if(SelectedUnit == null) return;
		PreviewMoves = SelectedUnit.GetPossibleMoveLocations();

	}

	private static RenderTarget2D? hoverHudRenderTarget;
	//private static RenderTarget2D? rightCornerRenderTarget;
	//private static RenderTarget2D? leftCornerRenderTarget;
	private static RenderTarget2D? dmgScreenRenderTarget;
	private static RenderTarget2D? infoScreenRenderTarget;
	private static RenderTarget2D? timerRenderTarget;
	//private static RenderTarget2D? chatRenderTarget;
	//private static RenderTarget2D? chatScreenRenderTarget;
	private static Dictionary<Unit, RenderTarget2D>? unitBarRenderTargets;
	private static Dictionary<Unit, RenderTarget2D>? targetBarRenderTargets;
		
	private static bool inited = false;

	public static void Init()
	{
		if (inited) return;
		inited = true;
		//rightCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/frames").Width,TextureManager.GetTexture("UI/GameHud/frames").Height);
		//leftCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/LeftPanel/base").Width,TextureManager.GetTexture("UI/GameHud/LeftPanel/base").Height);
		hoverHudRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/HoverHud/base").Width,TextureManager.GetTexture("UI/HoverHud/base").Height);
		//statScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/statScreen").Width,TextureManager.GetTexture("UI/GameHud/statScreen").Height);
		dmgScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/BottomBar/screen").Width,TextureManager.GetTexture("UI/GameHud/BottomBar/screen").Height);
		infoScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/BottomBar/screen").Width,TextureManager.GetTexture("UI/GameHud/BottomBar/screen").Height);
		//chatRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/chatframe").Width,TextureManager.GetTexture("UI/GameHud/chatframe").Height);
		//	chatScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/chatscreen").Width,TextureManager.GetTexture("UI/GameHud/chatscreen").Height);
		timerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/UnitBar/unitframe").Width,TextureManager.GetTexture("UI/GameHud/UnitBar/unitframe").Height);
		unitBarRenderTargets = new Dictionary<Unit, RenderTarget2D>();
		targetBarRenderTargets = new Dictionary<Unit, RenderTarget2D>();
	}


	public static void MakeUnitBarRenders(SpriteBatch batch)
	{
		if(_unitBar == null ||  _unitBar.Widgets.Count == 0) return;
		if( MyUnits.Count == 0) return;
		int columCounter = 0;
		//sort by id
		MyUnits.Sort((a, b) => a.WorldObject.ID.CompareTo(b.WorldObject.ID));
		foreach (var unit in MyUnits)
		{
			if (!unitBarRenderTargets.ContainsKey(unit))
			{
				unitBarRenderTargets.Add(unit, new RenderTarget2D(graphicsDevice, TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Width, TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Height));

			}

			graphicsDevice.SetRenderTarget(unitBarRenderTargets[unit]);
			graphicsDevice.Clear(Color.Transparent);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/Background"), Vector2.Zero, Color.White);
			int i;
			if (unit.ActionPoints > 0 || unit.MovePoints > 0)
			{
				batch.End();
				PostProcessing.PostProcessing.ShuffleUIeffect(columCounter + 123, new Vector2(unitBarRenderTargets[unit].Width, unitBarRenderTargets[unit].Height));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();

				int notchpos = 0;
				for (i = 0; i < unit.MovePoints; i++)
				{
					PostProcessing.PostProcessing.ShuffleUIeffect(columCounter + 123, new Vector2(TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch").Width, TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch").Height));
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch"), new Vector2(6 * notchpos, 0), Color.White);
					batch.End();
					notchpos++;
				}

				for (i = 0; i < unit.ActionPoints; i++)
				{
					PostProcessing.PostProcessing.ShuffleUIeffect(columCounter + 123, new Vector2(TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch").Width, TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch").Height));
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch"), new Vector2(6 * notchpos, 0), Color.White);
					batch.End();
					notchpos++;
				}

			}
			else
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();
			}




			var healthTexture = TextureManager.GetTexture("UI/GameHud/UnitBar/red");
			var healthTextureoff = TextureManager.GetTexture("UI/GameHud/UnitBar/redoff");
			float healthWidth = healthTexture.Width;
			float healthHeight = healthTexture.Height;
			int baseWidth = TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Width;
			_ = TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Height;
			float healthBarWidth = (healthWidth + 1) * unit.WorldObject.Type.MaxHealth;
			float emtpySpace = baseWidth - healthBarWidth;
			Vector2 healthBarPos = new Vector2(emtpySpace / 2f, 22);


			for (int y = 0; y < unit.WorldObject.Type.MaxHealth; y++)
			{
				var indicator = healthTexture;
				bool health = !(y >= unit.WorldObject.Health);
				if (health)
				{
					PostProcessing.PostProcessing.ShuffleUIeffect(y + unit.WorldObject.ID, new Vector2(healthWidth, healthHeight));
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

			healthTexture = TextureManager.GetTexture("UI/GameHud/UnitBar/green");
			healthTextureoff = TextureManager.GetTexture("UI/GameHud/UnitBar/greenoff");
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
					PostProcessing.PostProcessing.ShuffleUIeffect(y + unit.WorldObject.ID, new Vector2(healthWidth, healthHeight));
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
		int realWidth = unitBarRenderTargets[MyUnits[0]].Width;
		int realHeight = unitBarRenderTargets[MyUnits[0]].Height;
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

		foreach (var unit in MyUnits)
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
				elem.PressedImage = new TextureRegion(unitBarRenderTargets[unit]);
				elem.Image = new TextureRegion(unitBarRenderTargets[unit]);
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

		if(targetBar == null ||  targetBar.Widgets.Count < SuggestedTargets.Count) return;
		if( SuggestedTargets.Count == 0) return;
		foreach (var unit in SuggestedTargets)
		{
			if (!targetBarRenderTargets.ContainsKey(unit))
			{
				targetBarRenderTargets.Add(unit, new RenderTarget2D(graphicsDevice, TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon").Width,TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon").Height));

			}
			graphicsDevice.SetRenderTarget(targetBarRenderTargets[unit]);
			graphicsDevice.Clear(Color.Transparent);
			if (unit.IsMyTeam())
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
		foreach (var unit in SuggestedTargets)
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
			elem.PressedImage = new TextureRegion(targetBarRenderTargets[unit]);
			elem.Image = new TextureRegion(targetBarRenderTargets[unit]);
			if (unit.WorldObject.TileLocation.Position.Equals(ActionTarget)){
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
		Init();
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
				(MyUnits, EnemyUnits) = (EnemyUnits, MyUnits);
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
				Width = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Width * globalScale.X * 0.9f),
				Height = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Height * globalScale.X * 0.9f),
				ImageWidth = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Width * globalScale.X * 0.9f),
				ImageHeight = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Height * globalScale.X * 0.9f),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Background = new SolidBrush(Color.Transparent),
				OverBackground = new SolidBrush(Color.Transparent),
				PressedBackground = new SolidBrush(Color.Transparent),
				Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/UnitBar/end button")),
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
		left.Renderable = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/leftq"));
		//left.
		var right = new Image();
		right.Height = (int) (34f * globalScale.X);
		right.Width = (int) (16f * globalScale.X);
		right.Renderable = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/righte"));
		
		targetBarStack.Widgets.Add(left);
		targetBarStack.Widgets.Add(targetBar);
		targetBarStack.Widgets.Add(right);
		
		panel.Widgets.Add(targetBarStack);
		
		targetBarStack.Visible = false;
		

		ConfirmButton = new ImageButton();
		ConfirmButton.HorizontalAlignment = HorizontalAlignment.Center;
		ConfirmButton.VerticalAlignment = VerticalAlignment.Bottom;
		ConfirmButton.Top = (int) (-120 * globalScale.X);
		ConfirmButton.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/confirm"));
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
		
		foreach (var unit in MyUnits)
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
		HudActionButton crouchHudBtn = new HudActionButton(crouchbtn, TextureManager.GetTexture("UI/GameHud/BottomBar/crouch"),SelectedUnit,delegate(Unit unit, Vector2Int target)
			{
				unit.DoAction(Action.ActionType.Crouch, unit.WorldObject.TileLocation.Position,null);
			}, 
			delegate(Unit unit, Vector2Int vector2Int)
			{
				if(!unit.WorldObject.TileLocation.Position.Equals(vector2Int)) return new Tuple<bool, string>(false,"");
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


		int top = (int) (-4*globalScale.X) ;
		float scale = globalScale.X * 1.05f;
		int totalBtns = 3 + SelectedUnit.Abilities.Count;
		int btnWidth = (int) (24 * scale);
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
			UIButton.Width = (int) (24 * scale);
			UIButton.Height = (int) (29 * scale);
			UIButton.ImageHeight = (int) (29 * scale);
			UIButton.ImageWidth = (int) (24 * scale);
			UIButton.Top = top;
			UIButton.Left = startOffest + btnWidth*(index);
			index++;
		}


	
		
		return panel;
	}




	private static readonly List<HudActionButton> ActionButtons = new();


	private static readonly List<Unit> Controllables = new();
	public static List<Unit> MyUnits = new();
	public static List<Unit> EnemyUnits = new();

	private static void DoActiveAction(bool force = false)
	{
		if(activeAction != ActiveActionType.Action && activeAction != ActiveActionType.Overwatch) return;

		if (force)
		{
			if (!HudActionButton.SelectedButton.HasPoints()) return;//only check for point is we're forcing
		}
		else
		{

			var res = HudActionButton.SelectedButton.CanPerformAction(ActionTarget);
			if (!res.Item1)
			{
				new PopUpText(res.Item2, ActionTarget, Color.Red);
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
			HudActionButton.SelectedButton!.OverwatchAction(ActionTarget);
		}


		SelectHudAction(null);
	}

	public static void RegisterUnit(Unit c)
	{
		
		Controllables.Add(c);
		if (c.IsMyTeam())
		{
			if (SelectedUnit == null)
			{
				SelectUnit(c);
			}

			
			MyUnits.Add(c);
		}
		else
		{
			EnemyUnits.Add(c);
		}
	}
	public static void UnRegisterUnit(Unit c)
	{
		Controllables.Remove(c);
		if (c.IsMyTeam())
		{
			MyUnits.Remove(c);
		}		
		else
		{
			EnemyUnits.Remove(c);
		}
		
	}


	public static PreviewData? ScreenData;
	public static Vector2Int ActionTarget;
	
	private static List<Unit> SuggestedTargets = new();
	private int GetSelectedTargetIndex() {
		for (int i = 0; i < SuggestedTargets.Count; i++)
		{
			if (SuggestedTargets[i].WorldObject.TileLocation.Position.Equals(ActionTarget))
			{
				return i;
			}
		}

		return -1;
	}

	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
		if(SelectedUnit==null) return;
		
		base.RenderBehindHud(batch, deltatime);
		MakeUnitBarRenders(batch);
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		
		
		var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
		var mousepos = Utility.GridToWorldPos(TileCoordinate+new Vector2(-1.5f,-0.5f));
		for (int i = 0; i < 8; i++)
		{
		
			var indicator = TextureManager.GetSpriteSheet("UI/coverIndicator",3,3)[i];
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

		switch (activeAction)
		{
			case ActiveActionType.Move:
				Action.Actions[Action.ActionType.Move].Preview(SelectedUnit, TileCoordinate, batch);
				break;
			case ActiveActionType.Face:
				Action.Actions[Action.ActionType.Face].Preview(SelectedUnit, TileCoordinate, batch);
				break;
			case ActiveActionType.Action:
				if (HudActionButton.SelectedButton == null)
				{
					throw new Exception("Action as active action without selected action button");
				}

				HudActionButton.SelectedButton.Preview(ActionTarget, batch);
				break;
			case ActiveActionType.Overwatch:
				HudActionButton.SelectedButton.PreviewOverwatch(ActionTarget, batch);
				break;
		}
		
		


		batch.End();

		if (activeAction == ActiveActionType.Action)
		{
			foreach (var target in SuggestedTargets)
			{

				if (!target.WorldObject.IsVisible()) continue;
				if (target.IsMyTeam())
				{
					PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Green);
				}
				else
				{
					PostProcessing.PostProcessing.SetOutlineEffectColor(Color.Red);
				}

				batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.OutLineEffect);
				Texture2D sprite = target.WorldObject.GetTexture();
				batch.Draw(sprite, target.WorldObject.GetDrawTransform().Position + Utility.GridToWorldPos(new Vector2(1.5f, 0.5f)), null, Color.White, 0, sprite.Bounds.Center.ToVector2(), Math.Max(1, 2f / Camera.GetZoom()), SpriteEffects.None, 0);
				batch.End();
			}
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
				DrawHoverHud(batch, edge, deltatime);
			}


		}
		foreach (var controllable in Controllables)
		{
			if (controllable.WorldObject.IsVisible())
			{
				DrawHoverHud(batch, controllable.WorldObject, deltatime);
			}
		}
		var tiles = WorldManager.Instance.GetTilesAround(TileCoordinate, 10);
		foreach (var t in tiles)
		{
			foreach (var edge in t.GetAllEdges())
			{ 
				if (!Equals(edge.PreviewData, new PreviewData()))
				{ 
					DrawHoverHud(batch, edge, deltatime);
				}
			}
		}
		
		
		graphicsDevice.SetRenderTarget(dmgScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/BottomBar/screen"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		bool drawPreviewDmg = false;
		
		if (ScreenData is not null)
		{
			drawPreviewDmg = true;
			batch.DrawText("Damage", new Vector2(12, 8), 1, 5, Color.White);
			for (int i = 0; i < ScreenData.Value.totalDmg; i++)
			{
				Color c = Color.Green;
				if ( ScreenData.Value.totalDmg - ScreenData.Value.finalDmg> i)
				{
					c = Color.Red;
				}
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 7) + i * new Vector2(5, 0), null, c, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
			batch.DrawLine(0,22,200,22,Color.White,2);
			batch.DrawText("Deter", new Vector2(12, 29), 1, 5, Color.White);
			for (int i = 0; i < ScreenData.Value.determinationBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 28) + i * new Vector2(5, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
			batch.DrawText("Cover", new Vector2(12, 44), 1, 5, Color.White);
			for (int i = 0; i < ScreenData.Value.coverBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 43) + i * new Vector2(5, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
			batch.DrawText("Range", new Vector2(12, 59), 1, 5, Color.White);
			for (int i = 0; i < ScreenData.Value.distanceBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 58) + i * new Vector2(5, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}

		
		}

		batch.End();
		
		graphicsDevice.SetRenderTarget(infoScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/BottomBar/screen"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		
		Vector2Int pointPos = new Vector2Int(5, 60);
		int o = 20;
		int g = 0;
		for (int j = 0; j < SelectedUnit.MovePoints; j++)
		{	
			
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"),pointPos+new Vector2(o*g,0),null, Color.White,0,Vector2.Zero,3.2f,SpriteEffects.None,0);
			g++;
		}

		g++;
			
		for (int j = 0; j < SelectedUnit.ActionPoints; j++)
		{
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/actionpoint"),pointPos+new Vector2(o*g,0),null, Color.White,0,Vector2.Zero,3.2f,SpriteEffects.None,0);
			g++;
		}
		
		batch.DrawText("Health", new Vector2(15, 5), 1, 5, Color.White);
		

		var nohealthTexture = TextureManager.GetTexture("UI/HoverHud/nohealth");
		var healthTexture = TextureManager.GetTexture("UI/HoverHud/health");
		var healthWidth = healthTexture.Width;
		Vector2 healthBarPos = new Vector2(8, 14);

		for (int y = 0; y < SelectedUnit.WorldObject.Type.MaxHealth; y++)
		{
			bool health = !(y>= SelectedUnit.WorldObject.Health);

			Texture2D indicator;
			if (health) {
				indicator = healthTexture;
			}
			else {
				indicator= nohealthTexture;
			}
			
			batch.Draw(indicator,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			
		}
		batch.DrawText("Determination", new Vector2(15, 27), 1, 100, Color.White);
		Vector2 DetPos = new Vector2(8, 40);
		int detWidth = TextureManager.GetTexture("UI/HoverHud/detGreen").Width;
		int detHeight = TextureManager.GetTexture("UI/HoverHud/detGreen").Height;
		
		for (int y = 0; y < SelectedUnit.Type.Maxdetermination; y++)
		{
			Texture2D indicator;
			bool pulse = false;

			if (y == SelectedUnit.Determination && !SelectedUnit.Paniced)
			{
				indicator = TextureManager.GetTexture("UI/HoverHud/detYellow");
				pulse = true;
			}
			else if (y >= SelectedUnit.Determination)
			{
				indicator = TextureManager.GetTexture("UI/HoverHud/detBlank");
			}
			else
			{
				indicator = TextureManager.GetTexture("UI/HoverHud/detGreen");
			}

			float op = 1;
			if (pulse)
			{
				op = animopacity+0.1f;
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);

			}

			batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White*op, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			

		}
		if (SelectedUnit.Paniced)
		{
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/panic"), new Vector2(8,35), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
		}
		
		batch.End();
		graphicsDevice.SetRenderTarget(timerRenderTarget);
	
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		var frame = TextureManager.GetTexture("UI/GameHud/UnitBar/unitframe");
		batch.Draw(frame, new Vector2(0,0), null, Color.White, 0, Vector2.Zero, 1,SpriteEffects.None, 0);
		
		var totalLenght = 259 + 30;
		var fraction = GameManager.TimeTillNextTurn / (GameManager.PreGameData.TurnTime * 1000);
		var displayLenght = totalLenght - totalLenght * fraction;
		
		batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/Timer"), Vector2.Zero, null, Color.Gray, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/Timer"), Vector2.Zero, new Rectangle(0,0,190+(int)displayLenght,80), Color.White, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.End();
		PostProcessing.PostProcessing.ShuffleUIeffect(595,new Vector2(10,10),true);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostProcessing.PostProcessing.UIGlowEffect);
		var turn = TextureManager.GetTexture("UI/GameHud/UnitBar/enemyTurn");
		if (GameManager.IsMyTurn())
		{		
			turn = TextureManager.GetTexture("UI/GameHud/UnitBar/yourTurn");
		}

		batch.Draw(turn, new Vector2(0, 0), null,Color.White, 0, Vector2.Zero, new Vector2(1f,1f),SpriteEffects.None, 0);
		batch.End();

		//final Draw
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		//batch.Draw(rightCornerRenderTarget, new Vector2(Game1.resolution.X - (rightCornerRenderTarget.Width-104)*globalScale.Y*1.3f, Game1.resolution.Y - rightCornerRenderTarget.Height*globalScale.Y*1.3f), null, Color.White, 0, Vector2.Zero, globalScale.Y*1.3f ,SpriteEffects.None, 0);

	
		batch.Draw(timerRenderTarget, new Vector2(Game1.resolution.X-timerRenderTarget.Width*globalScale.X*0.9f, 0), null, Color.White, 0, Vector2.Zero, globalScale.X*0.9f ,SpriteEffects.None, 0);
		
		Texture2D bar = TextureManager.GetTexture("UI/GameHud/BottomBar/mainbuttonbox");
		if (activeAction == ActiveActionType.Action || activeAction == ActiveActionType.Overwatch)
		{
			var box = TextureManager.GetTexture("UI/GameHud/BottomBar/Infobox");
			tooltipPos = new Vector2((Game1.resolution.X - box.Width * globalScale.X) / 2f, Game1.resolution.Y - box.Height * globalScale.X - bar.Height * globalScale.X);
			batch.Draw(box, tooltipPos, null, Color.White, 0, Vector2.Zero, globalScale.X,SpriteEffects.None, 0);
		}
	
		if (drawPreviewDmg)
		{
			batch.End();
			PostProcessing.PostProcessing.ShuffleUIeffect(100, new Vector2(dmgScreenRenderTarget.Width, dmgScreenRenderTarget.Height), false);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
			batch.Draw(dmgScreenRenderTarget, new Vector2((Game1.resolution.X - bar.Width * globalScale.X) / 2f + bar.Width * globalScale.X, Game1.resolution.Y - dmgScreenRenderTarget.Height * globalScale.X / 2f), null, Color.White, 0, Vector2.Zero, globalScale.X / 2f, SpriteEffects.None, 0);
			batch.End();
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		}
		else
		{
			batch.Draw(dmgScreenRenderTarget, new Vector2((Game1.resolution.X - bar.Width * globalScale.X) / 2f + bar.Width * globalScale.X, Game1.resolution.Y - dmgScreenRenderTarget.Height * globalScale.X / 2f), null, Color.White, 0, Vector2.Zero, globalScale.X / 2f, SpriteEffects.None, 0);
		}
		batch.End();
		PostProcessing.PostProcessing.ShuffleUIeffect(100, new Vector2(dmgScreenRenderTarget.Width, dmgScreenRenderTarget.Height), false);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostProcessing.PostProcessing.UIGlowEffect);
		batch.Draw(infoScreenRenderTarget, new Vector2((Game1.resolution.X - bar.Width*globalScale.X)/2f - infoScreenRenderTarget.Width*globalScale.X/2f, Game1.resolution.Y - dmgScreenRenderTarget.Height * globalScale.X / 2f), null, Color.White, 0, Vector2.Zero, globalScale.X / 2f, SpriteEffects.None, 0);
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

			batch.DrawText(toolTipText,tooltipPos + new Vector2(15,10)*globalScale.X, globalScale.X*0.6f,40,Color.White);
			Vector2 startpos = tooltipPos+ new Vector2(5,40f)*globalScale.X;
			
			
			AbilityCost cost = HudActionButton.SelectedButton.Cost;
			Vector2 offset = new Vector2(5, 0);
			Color c = Color.Green;


			if (cost.MovePoints > 0)
			{
				if (cost.MovePoints > SelectedUnit.MovePoints)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("UI/GameHud/BottomBar/tinyBox"), startpos + offset * globalScale.X + new Vector2(0, 0) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"), startpos + offset * globalScale.X + new Vector2(5, 3) * globalScale.X, null, Color.White, 0, Vector2.Zero, globalScale.X * 1.5f, SpriteEffects.None, 0);
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

				batch.Draw(TextureManager.GetTexture("UI/GameHud/BottomBar/tinyBox"),startpos + offset*globalScale.X+new Vector2(0,0)*globalScale.X,null,Color.White,0,Vector2.Zero,globalScale.X*1.5f,SpriteEffects.None,0);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/actionpoint"),startpos +offset*globalScale.X+ new Vector2(5,3)*globalScale.X,null,Color.White,0,Vector2.Zero,globalScale.X*1.5f,SpriteEffects.None,0);
				batch.DrawText(cost.ActionPoints+"", startpos + offset*globalScale.X+new Vector2(30,2)*globalScale.X, globalScale.X*1.5f, 24, c);
			}

			if (cost.Determination > 0)
			{
				offset += new Vector2(40, 0);
				c = Color.Green;
				if (cost.Determination > SelectedUnit.Determination)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("UI/GameHud/BottomBar/tinyBox"),startpos + offset*globalScale.X+new Vector2(0,0)*globalScale.X,null,Color.White,0,Vector2.Zero,globalScale.X*1.5f,SpriteEffects.None,0);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/detgreen"),startpos +offset*globalScale.X+ new Vector2(3,5)*globalScale.X,null,Color.White,0,Vector2.Zero,globalScale.X*1.1f,SpriteEffects.None,0);
				batch.DrawText(cost.Determination+"", startpos + offset*globalScale.X+new Vector2(30,2)*globalScale.X, globalScale.X*1.5f, 24, c);
			}

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
				batch.DrawText("Using overwatch will prevent this unit from  doing any other actions this turn", startpos + new Vector2(12, 25) * globalScale.X, globalScale.X/2f, 50, Color.Yellow);
			}

		}
		batch.End();


		return;
		if (drawExtra)
		{
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
			int res = AI.Move.GetTileMovementScore(TileCoordinate,moveUse,false,SelectedUnit, out details);
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
			AIMoveCache[(int) TileCoordinate.X, (int) TileCoordinate.Y, 0] = res;
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

		int targetIndex = Controllables.IndexOf(SelectedUnit);
		if (targetIndex != -1)
		{
			for (int i = targetIndex; i < Controllables.Count - 1; i++)
			{
				Controllables[i] = Controllables[i + 1];
				Controllables[i + 1] = SelectedUnit;
			}
		}

		var tile = WorldManager.Instance.GetTileAtGrid(_mouseTileCoordinate);
		
		if (tile.UnitAtLocation != null)
		{
			targetIndex = Controllables.IndexOf(tile.UnitAtLocation);
			if (targetIndex != -1)
			{
				Unit target = Controllables[targetIndex];
				for (int i = targetIndex; i < Controllables.Count - 1; i++)
				{
						
					Controllables[i] = Controllables[i + 1];
					Controllables[i + 1] = target;
				}
			}
			
		}

		ProcessKeyboard();
		
		//bad
		if (!GameManager.IsMyTurn())
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/UnitBar/end button")), Color.Gray);
		}
		else
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/UnitBar/end button")), Color.White);
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


		WorldEffect.FreeFire = currentKeyboardState.IsKeyDown(Keys.LeftControl);


		if (SuggestedTargets.Count > 0)
		{
			if (JustPressed(Keys.Q))
			{
				int idx = GetSelectedTargetIndex();
				if (idx <= 0)
				{
					ActionTarget = SuggestedTargets[SuggestedTargets.Count - 1].WorldObject.TileLocation.Position;
				}
				else
				{
					ActionTarget = SuggestedTargets[idx - 1].WorldObject.TileLocation.Position;
				}

				Camera.SetPos(ActionTarget);
			}
			else if (JustPressed(Keys.E))
			{
				int idx = GetSelectedTargetIndex();
				if (idx >= SuggestedTargets.Count - 1)
				{
					ActionTarget = SuggestedTargets[0].WorldObject.TileLocation.Position;
				}
				else
				{
					ActionTarget = SuggestedTargets[idx + 1].WorldObject.TileLocation.Position;
				}

				Camera.SetPos(ActionTarget);
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

		if (JustPressed(Keys.D1) && MyUnits.Count > 0)
		{
			SelectUnit(MyUnits[0]);
		}else if (JustPressed(Keys.D2) && MyUnits.Count > 1)
		{
			SelectUnit(MyUnits[1]);
		}else if (JustPressed(Keys.D3) && MyUnits.Count > 2)
		{
			SelectUnit(MyUnits[2]);
		}else if (JustPressed(Keys.D4) && MyUnits.Count > 3)
		{
			SelectUnit(MyUnits[3]);
		}else if (JustPressed(Keys.D5) && MyUnits.Count > 4)
		{
			SelectUnit(MyUnits[4]);
		}else if (JustPressed(Keys.D6) && MyUnits.Count > 5)
		{
			SelectUnit(MyUnits[5]);
		}else if (JustPressed(Keys.D7) && MyUnits.Count > 6)
		{
			SelectUnit(MyUnits[6]);
		}else if (JustPressed(Keys.D8) && MyUnits.Count > 7)
		{
			SelectUnit(MyUnits[7]);
		}else if (JustPressed(Keys.D9) && MyUnits.Count > 8)
		{
			SelectUnit(MyUnits[8]);
		}else if (JustPressed(Keys.D0) && MyUnits.Count > 9)
		{
			SelectUnit(MyUnits[9]);
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
		Overwatch
	}

	private static ActiveActionType activeAction = ActiveActionType.None;

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);
		if(SelectedUnit == null) return;

		position = Vector2.Clamp(position, Vector2.Zero, new Vector2(99, 99));
		
		var tile =WorldManager.Instance.GetTileAtGrid( position);

		if (tile.UnitAtLocation != null&& tile.UnitAtLocation.WorldObject.GetMinimumVisibility() <= tile.GetVisibility() && (activeAction == ActiveActionType.None || activeAction == ActiveActionType.Move)) { 
			SelectUnit(tile.UnitAtLocation);
			return;
		}

		if (rightclick)
		{
			switch (activeAction)
			{
				case ActiveActionType.None:
					activeAction = ActiveActionType.Face;
					ActionTarget = position;
					break;
				case ActiveActionType.Face:
					SelectedUnit.DoAction(Action.ActionType.Face, position);
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
					ActionTarget = position;
					break;
				case ActiveActionType.Face:
					activeAction = ActiveActionType.None;
					break;
				case ActiveActionType.Move:
					SelectedUnit.DoAction(Action.ActionType.Move, position);
					break;
				case ActiveActionType.Action:
				case ActiveActionType.Overwatch:
					ActionTarget = position;
					break;

			}

		}
	}


	private static float counter;
	private static float animopacity;
	private static TextBox? inputBox;

	public static void DrawHoverHud(SpriteBatch batch, WorldObject target,float deltaTime)
	{
		counter += deltaTime/3000f;
		if (counter > 2)
		{
			counter= 0;
		}
		animopacity = counter;
		if (counter > 1)
		{
			animopacity = 2- counter;
		}
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		float opacity = 1f;
		bool highlighted = false;

		
		if (Equals(SelectedUnit, target.UnitComponent) || MousePos == target.TileLocation.Position || (target.Type.Edge && Utility.IsClose(target,MousePos)))
		{
			opacity = 1;
			highlighted = true;
		}
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		float healthWidth = TextureManager.GetTexture("UI/HoverHud/nohealth").Width;
		float healthHeight = TextureManager.GetTexture("UI/HoverHud/nohealth").Height;
		float baseWidth = TextureManager.GetTexture("UI/HoverHud/base").Width;
		float healthBarWidth = healthWidth*target.Type.MaxHealth;
		float emtpySpace = baseWidth - healthBarWidth;
		Vector2 healthBarPos = new Vector2(emtpySpace/2f,36);

		float detWidth = 0;
		float detHeight = 0;
		float detbarWidht;
		float detEmtpySpace;
		Vector2 DetPos = default;
		
		
		if (target.UnitComponent == null)
		{
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/baseCompact"), new Vector2(healthBarPos.X,0), new Rectangle((int) healthBarPos.X,0,(int) healthBarWidth,TextureManager.GetTexture("UI/HoverHud/baseCompact").Height), Color.White);
		}
		else
		{
			detWidth = TextureManager.GetTexture("UI/HoverHud/detGreen").Width;
			detHeight = TextureManager.GetTexture("UI/HoverHud/detGreen").Height;
			detbarWidht = detWidth * target.UnitComponent.Type.Maxdetermination;
			detEmtpySpace = baseWidth - detbarWidht;
			DetPos = new Vector2(detEmtpySpace / 2f, 28);
			if (detbarWidht > healthBarWidth)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/base"), new Vector2(DetPos.X-5, 0), new Rectangle((int) DetPos.X-5, 0, (int) detbarWidht+10, TextureManager.GetTexture("UI/HoverHud/baseCompact").Height), Color.White);

			}
			else
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/base"), new Vector2(healthBarPos.X-5, 0), new Rectangle((int) healthBarPos.X-5, 0, (int) healthBarWidth+10, TextureManager.GetTexture("UI/HoverHud/baseCompact").Height), Color.White);
			}
		}

		batch.End();

		int dmgDone = 0; 
		int i = 0;
		var healthTexture = TextureManager.GetTexture("UI/HoverHud/health");
		var nohealthTexture = TextureManager.GetTexture("UI/HoverHud/nohealth");
		Vector2 offset = new Vector2(0,0);
		if (target.UnitComponent == null)
		{
			healthTexture = TextureManager.GetTexture("UI/HoverHud/healthenv");
			nohealthTexture = TextureManager.GetTexture("UI/HoverHud/nohealthenv");
			if (target.Type.Edge && MousePos == target.TileLocation.Position)
			{
				if (Equals(target.TileLocation.NorthEdge, target)){
					offset = new Vector2(0, -1);
				}
				else if (Equals(target.TileLocation.WestEdge, target)){
					offset = new Vector2(-1, 0);
				}
			}
			offset = new Vector2(1, 1);
	
		}
		else
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
			i = 0;
			foreach (var effect in target.UnitComponent.StatusEffects)
			{
				batch.Draw(TextureManager.GetTextureFromPNG("Icons/"+effect.type.name),new Vector2(23*i,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.DrawText(effect.duration+"", new Vector2(23*i+10,0), 1, 100, Color.White);
				i++;
			}
			batch.End();
		}

		for (int y = 0; y < target.Type.MaxHealth; y++)
		{
			bool health = true;
				
			if (y>= target.Health)
			{
				health = false;
			}
			else if ( target.PreviewData.finalDmg >= target.Health-y)
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(nohealthTexture,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
				i = y;
				PostProcessing.PostProcessing.ShuffleUIeffect(y + target.ID,new Vector2(healthWidth,healthHeight),highlighted,true);
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(healthTexture,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				//batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdone"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				batch.End();
				dmgDone++;
				continue;

			}
			Texture2D indicator;
			if (health)
			{
				PostProcessing.PostProcessing.ShuffleUIeffect(y + target.ID,new Vector2(healthWidth,healthHeight),highlighted);
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
				indicator = healthTexture;
			}
			else
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				indicator= nohealthTexture;
			}

				
				
			batch.Draw(indicator,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}

		if (target.UnitComponent != null)
		{
			Unit unit = target.UnitComponent;

			i = 0;



			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				Texture2D indicator;
				bool litup = true;
				bool dissapate = false;
				bool pulse = false;

				if (y == unit.Determination && !unit.Paniced)
				{
					indicator = TextureManager.GetTexture("UI/HoverHud/detYellow");
					pulse = true;
					litup = false;
				}
				else if (y >= unit.Determination)
				{
					indicator = TextureManager.GetTexture("UI/HoverHud/detBlank");
					litup = false;
				}
				else
				{
					indicator = TextureManager.GetTexture("UI/HoverHud/detGreen");
					if (target.PreviewData.detDmg >= unit.Determination - y)
					{
						dissapate = true;
					}

				}
				
				
				if (dissapate)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("UI/HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					PostProcessing.PostProcessing.ShuffleUIeffect(y + unit.WorldObject.ID + 10, new Vector2(detWidth, detHeight), highlighted, true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else if (litup)
				{

					PostProcessing.PostProcessing.ShuffleUIeffect(y + unit.WorldObject.ID + 10, new Vector2(detWidth, detHeight), highlighted);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, new Color(0,0,0), 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else
				{
					float op = 1;
					if (pulse)
					{
						op = animopacity+0.1f;
						batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
						batch.Draw(TextureManager.GetTexture("UI/HoverHud/detBlank"), DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
						batch.End();
					}

					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White*op, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}

			}
			if (unit.Paniced)
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/panic"), new Vector2(healthBarPos.X,18), new Rectangle((int) healthBarPos.X,0,(int) healthBarWidth,TextureManager.GetTexture("UI/HoverHud/baseCompact").Height), Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
				batch.End();
			}

			Vector2Int pointPos = new Vector2Int((int)healthBarPos.X, 51);
			int o = 8;
			i = 0;
			for (int j = 0; j < target.UnitComponent.MovePoints; j++)
			{	
				PostProcessing.PostProcessing.ShuffleUIeffect(i + unit.WorldObject.ID + 6, new Vector2(detWidth, detHeight), highlighted);
				batch.Begin(sortMode: SpriteSortMode.Texture, samplerState: SamplerState.PointClamp, effect:PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"),pointPos+new Vector2(o*i,0),Color.White);
				batch.End();
				i++;
			}

			i++;
			
			for (int j = 0; j < target.UnitComponent.ActionPoints; j++)
			{	
				PostProcessing.PostProcessing.ShuffleUIeffect(i + unit.WorldObject.ID + 6, new Vector2(detWidth, detHeight), highlighted);
				batch.Begin(sortMode: SpriteSortMode.Texture, samplerState:  SamplerState.PointClamp, effect:PostProcessing.PostProcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/actionpoint"),pointPos+new Vector2(o*i,0),null, Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
				batch.End();
				i++;
			}
		}
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2)target.TileLocation.Position+offset)+new Vector2(-70,-120),null,Color.White*opacity,0,Vector2.Zero,1.2f,SpriteEffects.None,0);
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
		foreach (var act in ActionButtons)
		{
			act.UpdateIcon();
		}
		if (HudActionButton.SelectedButton != null)
		{
			
		}


		switch (activeAction)
		{
			case ActiveActionType.None:
				ConfirmButton!.Visible = false;
				targetBarStack!.Visible = false;
				OverWatchToggle.Visible = false;
				return;
				break;
			case ActiveActionType.Overwatch:
				OverWatchToggle.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchON"));
				ConfirmButton!.Visible = true;
				targetBarStack!.Visible = false;
				OverWatchToggle.Visible = true;
				SuggestedTargets.Clear();
				break;
			case ActiveActionType.Action:
				ConfirmButton!.Visible = true;
				targetBarStack!.Visible = true;
				OverWatchToggle.Visible = false;
				if (HudActionButton.SelectedButton.CanOverwatch)
				{
					OverWatchToggle.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchOFF"));
					OverWatchToggle.Visible = true;
				}
				List<Vector2Int> potentialTargets = new();
				MyUnits.ForEach(x => potentialTargets.Add(x.WorldObject.TileLocation.Position));
				EnemyUnits.ForEach(x => potentialTargets.Add(x.WorldObject.TileLocation.Position));
				SuggestedTargets = HudActionButton.SelectedButton.GetSuggestedTargets(SelectedUnit, potentialTargets);
				
				if (!HudActionButton.SelectedButton.CanPerformAction(ActionTarget).Item1 && SuggestedTargets.Count > 0)
				{
					ActionTarget = SuggestedTargets[0].WorldObject.TileLocation.Position;

				}
				break;

					
				
		}
	
		
		
		if(SuggestedTargets.Count<2) targetBarStack.Visible = false;
		
		foreach (var unit in SuggestedTargets)
		{
			var unitPanel = new ImageButton();

			Unit u = unit;
			unitPanel.Click += (sender, args) =>
			{
				ActionTarget = u.WorldObject.TileLocation.Position;
				Console.WriteLine("Target set to "+ActionTarget);
			};
			targetBar.Widgets.Add(unitPanel);
		}

		targetBarStack.Proportions.Clear();
		targetBarStack.Proportions.Add(new Proportion(ProportionType.Pixels, 50));
		targetBarStack.Proportions.Add(new Proportion(ProportionType.Pixels, 200*SuggestedTargets.Count) );
		targetBarStack.Proportions.Add(new Proportion(ProportionType.Pixels, 50));

	}
	public static void SelectHudAction(HudActionButton? hudActionButton)
	{
		if (!inited) return;
		HudActionButton.SelectedButton = hudActionButton;
		activeAction = ActiveActionType.Action;
		if (hudActionButton == null)
		{
			activeAction = ActiveActionType.None;
		}

		UpdateHudButtons();
	}
}
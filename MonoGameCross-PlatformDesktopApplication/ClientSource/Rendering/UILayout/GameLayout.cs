using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Shapes;
using MultiplayerXeno.Items;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace MultiplayerXeno.UILayouts;

public class GameLayout : MenuLayout
{
	public static Label? ScoreIndicator;
	private static ImageButton? endBtn;

	public static List<Vector2Int>[] previewMoves = Array.Empty<List<Vector2Int>>();

	


	public static void SetScore(int score)
	{
		if (ScoreIndicator != null)
		{
			ScoreIndicator.Text = "score: " + score;
		}
	}

	public static Unit SelectedUnit { get; private set;} = null!;

	public static void SelectUnit(Unit? controllable)
	{
		if (controllable == null)
		{
			controllable = MyUnits.FirstOrDefault();
		}

		if (!controllable.IsMyTeam())
		{
			return;
		}

		SelectedUnit = controllable;
		ReMakeMovePreview();
		UI.SetUI( new GameLayout());
		Camera.SetPos(controllable.WorldObject.TileLocation.Position);
	}
	public static void ReMakeMovePreview()
	{
		previewMoves = SelectedUnit.GetPossibleMoveLocations();

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
		
	private static bool inited = false;

	public static void Init()
	{
		if (inited) return;
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
	}


	public static void MakeUnitBarRenders(SpriteBatch batch)
	{
		if(_unitBar == null ||  _unitBar.Widgets.Count == 0) return;
		int columCounter = 0;
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
				PostPorcessing.ShuffleUIeffect(columCounter + 123, new Vector2(unitBarRenderTargets[unit].Width, unitBarRenderTargets[unit].Height));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostPorcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTextureFromPNG("Units/" + unit.Type.Name+"/Icon"), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();

				int notchpos = 0;
				for (i = 0; i < unit.MovePoints; i++)
				{
					PostPorcessing.ShuffleUIeffect(columCounter + 123, new Vector2(TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch").Width, TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch").Height));
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostPorcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch"), new Vector2(6 * notchpos, 0), Color.White);
					batch.End();
					notchpos++;
				}

				for (i = 0; i < unit.ActionPoints; i++)
				{
					PostPorcessing.ShuffleUIeffect(columCounter + 123, new Vector2(TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch").Width, TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch").Height));
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.AnisotropicClamp, effect: PostPorcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch"), new Vector2(6 * notchpos, 0), Color.White);
					batch.End();
					notchpos++;
				}

			}
			else
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/" + unit.Type.Name), Vector2.Zero, Color.White);
				batch.DrawText(columCounter + 1 + "", new Vector2(10, 5), Color.White);
				batch.End();
			}




			var healthTexture = TextureManager.GetTexture("UI/GameHud/UnitBar/red");
			float healthWidth = healthTexture.Width;
			float healthHeight = healthTexture.Height;
			int baseWidth = TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Width;
			_ = TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Height;
			float healthBarWidth = (healthWidth + 1) * unit.WorldObject.Type.MaxHealth;
			float emtpySpace = baseWidth - healthBarWidth;
			Vector2 healthBarPos = new Vector2(emtpySpace / 2f, 22);


			for (int y = 0; y < unit.WorldObject.Type.MaxHealth; y++)
			{
				bool health = !(y >= unit.WorldObject.Health);
				if (health)
				{
					PostPorcessing.ShuffleUIeffect(y + unit.WorldObject.ID, new Vector2(healthWidth, healthHeight), true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				}

				batch.Draw(healthTexture, healthBarPos + new Vector2((healthWidth + 1) * y, 0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
				batch.End();
			}

			healthTexture = TextureManager.GetTexture("UI/GameHud/UnitBar/green");
			healthWidth = healthTexture.Width;
			healthHeight = healthTexture.Height;
			healthBarWidth = (healthWidth + 1) * unit.Type.Maxdetermination;
			emtpySpace = baseWidth - healthBarWidth;
			healthBarPos = new Vector2(emtpySpace / 2f, 25);


			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				bool health = !(y >= unit.Determination);
				if (health)
				{
					PostPorcessing.ShuffleUIeffect(y + unit.WorldObject.ID, new Vector2(healthWidth, healthHeight), true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				}

				batch.Draw(healthTexture, healthBarPos + new Vector2((healthWidth + 1) * y, 0), null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
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
		

	}

	private static Grid? _unitBar;
	private static bool invOpen;
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		Init();
		WorldManager.Instance.MakeFovDirty(false);
		var panel = new Panel ();
		if (!GameManager.spectating)
		{
			endBtn = new ImageButton()
			{
				Top = (int) (32.5f * globalScale.X),
				Left = (int) (-14f * globalScale.X),
				Width = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Width * globalScale.X*1.15f),
				Height = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Height * globalScale.X*1.15f),
				ImageWidth = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Width * globalScale.X*1.15f),
				ImageHeight = (int) (TextureManager.GetTexture("UI/GameHud/UnitBar/end button").Height * globalScale.X*1.15f),
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
		else
		{
			var swapTeam = new TextButton
			{
				Top = (int) (0f * globalScale.Y),
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


		inputBox = new TextBox();
		inputBox.Visible = false;
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
				Top=90,
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
			MaxWidth = (int)(420f*globalScale.X),
			//	Width = (int)(420f*globalScale.X),
			MaxHeight = (int)(37f*globalScale.X),
			//Height = (int)(38f*globalScale.X),
			Top = (int)(0f*globalScale.Y),
			Left = (int)(-45f*globalScale.X),
			//ShowGridLines = true,
		};
		panel.Widgets.Add(_unitBar);

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
		
			
		overwatchBtn = new ImageButton();
		overwatchBtn.Click += (o, a) => Action.SetActiveAction(ActionType.OverWatch);
		overwatchBtn.MouseEntered += (o, a) => Tooltip("Watches over an area and attacks the first enemy that enters it",0,-1,-1);
		overwatchBtn.MouseLeft += (o, a) =>  HideTooltip();
		panel.Widgets.Add(overwatchBtn);
		crouchbtn = new ImageButton();
		crouchbtn.Click += (o, a) => SelectedUnit.DoAction(Action.Actions[ActionType.Crouch], new Vector2Int(0,0));
		crouchbtn.MouseEntered += (o, a) => Tooltip("Crouching improves benefits of cover and allows hiding under tall cover",0,0,0);
		crouchbtn.MouseLeft += (o, a) => HideTooltip();
		panel.Widgets.Add(crouchbtn);
		itemBtn = new ImageButton();
		itemBtn.Click += (o, a) => Action.SetActiveAction(ActionType.UseItem);
		if (SelectedUnit.SelectedItem != null)
		{
			itemBtn.MouseEntered += (o, a) => Tooltip("Activates selected item:\n" + SelectedUnit.SelectedItem.Name + " - " + SelectedUnit.SelectedItem.Description, 0, -1, 0);
			itemBtn.MouseLeft += (o, a) => HideTooltip();
		}
		else
		{
			itemBtn.MouseEntered += (o, a) => Tooltip("Activates selected item", 0, -1, 0);
			itemBtn.MouseLeft += (o, a) => HideTooltip();
		}

		panel.Widgets.Add(itemBtn);
		
		
		collpaseBtn = new ImageButton();
		collpaseBtn.Click += (o, a) =>
		{
			invOpen = !invOpen; 
			UpdateActionButtons();
		};
		panel.Widgets.Add(collpaseBtn);
		
		ActionButtons.Clear();
		InvButtons.Clear();
		int i = 0;

		for (int j = 0; j < SelectedUnit.Type.InventorySize; j++)
		{
			int index = j;
			var btn = new ImageButton();
			InvButtons.Add(btn);
			btn.Click+= (o, a) =>
			{
				if (SelectedUnit.Inventory[index] != null)
				{
					SelectedUnit.DoAction(Action.Actions[ActionType.SelectItem], new Vector2Int(index, 0));
					UpdateActionButtons();
				}
			};
			panel.Widgets.Add(btn);
		}

		i = 0;
		foreach (var action in SelectedUnit.extraActions)
		{
			int index = i;
			var btn = new ImageButton();
			ActionButtons.Add(btn);
			btn.MouseEntered += (o, a) => Tooltip(action.Tooltip,action.GetConsiquences(SelectedUnit)[0],action.GetConsiquences(SelectedUnit)[1],action.GetConsiquences(SelectedUnit)[2]);
			btn.MouseLeft += (o, a) => HideTooltip();
			if (action.ImmideateActivation)
			{
				btn.MouseEntered += (o, a) =>
				{
					if (Action.ActiveAction == null)
					{
						UseExtraAbility.AbilityIndex = index;
						Action.SetActiveAction(ActionType.UseAbility);
					}
				};
				btn.MouseLeft += (o, a) =>
				{
					if (UseExtraAbility.AbilityIndex == index)
					{
						Action.SetActiveAction(null);
						UseExtraAbility.AbilityIndex = -1;
					}
				};
				btn.Click += (o, a) =>
				{
					Console.WriteLine("click");
					Action.SetActiveAction(ActionType.UseAbility);
					UseExtraAbility.AbilityIndex = index;
					SelectedUnit.DoAction(Action.ActiveAction, SelectedUnit.WorldObject.TileLocation.Position);
				};
			}
			else
			{
				btn.Click += (o, a) =>
				{
					UseExtraAbility.AbilityIndex = index;
					Action.SetActiveAction(ActionType.UseAbility);
				};
			}
			panel.Widgets.Add(btn);
			i++;
		}

		UpdateActionButtons();
		
		return panel;
	}

	private static bool toolTip;
	private static string toolTipText = "";
	private static int detChange;
	private static int actChange;
	private static int moveChange;
	private static void Tooltip(string text, int det = 0, int act = 0, int move = 0)
	{
		toolTip = true;
		toolTipText = text;
		detChange = det;
		actChange = act;
		moveChange = move;
	}

	private static void HideTooltip()
	{
		toolTip = false;
		detChange = 0;
		actChange = 0;
		moveChange = 0;
	}

	private static ImageButton overwatchBtn = null!;
	private static ImageButton crouchbtn = null!;
	private static ImageButton itemBtn = null!;
	private static ImageButton collpaseBtn = null!;
	private static readonly List<ImageButton> ActionButtons = new();
	private static readonly List<ImageButton> InvButtons = new();

	public static void UpdateActionButtons()
	{

		int top = (int) (-4*globalScale.X) ;
		float scale = globalScale.X * 1.05f;
		int totalBtns = 3 + SelectedUnit.extraActions.Count;
		int btnWidth = (int) (24 * scale);
		int totalWidth = totalBtns * btnWidth;
		int startOffest = Game1.resolution.X / 2 - totalWidth / 2;
		overwatchBtn.HorizontalAlignment = HorizontalAlignment.Left;
		overwatchBtn.VerticalAlignment = VerticalAlignment.Bottom;
		overwatchBtn.Width = (int) (24 * scale);
		overwatchBtn.Height = (int) (29 * scale);
		if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.OverWatch)
		{
			overwatchBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchbtn")), new Color(255, 140, 140));
			overwatchBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchbtn")), new Color(255, 140, 140));
		}
		else if (SelectedUnit.MovePoints>0 && SelectedUnit.ActionPoints>0)
		{
			overwatchBtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchbtn"));
		}
		else
		{
			overwatchBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchbtn")), Color.Gray);
			overwatchBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchbtn")),  Color.Gray);
			overwatchBtn.Image =  new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/overwatchbtn")),Color.Gray);
		}


		overwatchBtn.ImageHeight = (int) (29 * scale);
		overwatchBtn.ImageWidth = (int) (24 * scale);
		overwatchBtn.Top = top;
		overwatchBtn.Left = startOffest+btnWidth*2;

		crouchbtn.HorizontalAlignment = HorizontalAlignment.Left;
		crouchbtn.VerticalAlignment = VerticalAlignment.Bottom;
		crouchbtn.Width = (int) (24 * scale);
		crouchbtn.Height = (int) (29 * scale);
		var img  = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/crouchOff"));
		if (SelectedUnit.Crouching)
		{
			img = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/crouchOn"));
		}

		if (SelectedUnit.MovePoints > 0)
		{
			crouchbtn.Image = img;
		}
		else
		{
			crouchbtn.Image = new ColoredRegion(img, Color.Gray);
		}

		crouchbtn.ImageHeight = (int) (29 * scale);
		crouchbtn.ImageWidth = (int) (24 * scale);
		crouchbtn.Top = top;
		crouchbtn.Left = startOffest + btnWidth;

		itemBtn.HorizontalAlignment = HorizontalAlignment.Left;
		itemBtn.VerticalAlignment = VerticalAlignment.Bottom;
		itemBtn.Width = (int) (24 * scale);
		itemBtn.Height = (int) (29 * scale);
		if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.UseItem)
		{
			itemBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), new Color(255, 140, 140));
			itemBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), new Color(255, 140, 140));
			if (SelectedUnit.SelectedItem != null)
			{
				itemBtn.Image = new ColoredRegion(new TextureRegion(SelectedUnit.SelectedItem?.Icon), new Color(255, 140, 140));
			}
		}
		else if(SelectedUnit.SelectedItem != null && SelectedUnit.ActionPoints > 0)
		{
			itemBtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button"));
			itemBtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button"));
			itemBtn.Image = new TextureRegion(SelectedUnit.SelectedItem?.Icon);

				
			
		}
		else
		{
			itemBtn.Background =  new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")),Color.Gray);
			itemBtn.OverBackground =  new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")),Color.Gray);
			if (SelectedUnit.SelectedItem != null)
			{
				itemBtn.Image = new ColoredRegion(new TextureRegion(SelectedUnit.SelectedItem?.Icon),Color.Gray);
			}else
			{
				itemBtn.Image = null;
			}
		}

		itemBtn.ImageHeight = (int) (29 * scale);
		itemBtn.ImageWidth = (int) (24 * scale);
		itemBtn.Top = top;
		itemBtn.Left = startOffest + btnWidth*0;

		int i = 0;
		if (invOpen)
		{
			for (int j = 0; j < SelectedUnit.Type.InventorySize; j++)
			{
				WorldAction? inv = SelectedUnit.Inventory[j];
				if (j == SelectedUnit.SelectedItemIndex)
				{
					var btn = InvButtons[j];
					btn.Visible = false;
					continue;
				}

				var invbtn = InvButtons[j];
				invbtn.Visible = true;
				invbtn.HorizontalAlignment = HorizontalAlignment.Left;
				invbtn.VerticalAlignment = VerticalAlignment.Bottom;
				invbtn.Width = (int) (24 * scale);
				invbtn.Height = (int) (29 * scale);
				invbtn.ImageHeight = (int) (29 * scale);
				invbtn.ImageWidth = (int) (24 * scale);
				invbtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/empty"));
				if (inv != null)
				{
					invbtn.Image = new TextureRegion(inv.Icon);
				}
				else
				{
					invbtn.Image = null;
				}

				invbtn.Top = (int) (top - (i+1)*invbtn.Height);
				invbtn.Left = itemBtn.Left;
				i++;
			}
		}
		else
		{
			foreach (var btn in InvButtons)
			{
				btn.Visible = false;
			}
		}

		if (SelectedUnit.Inventory.Length < 2)
		{
			collpaseBtn.Visible = false;
		}
		else
		{
			collpaseBtn.Visible = true;
		}

		collpaseBtn.HorizontalAlignment = HorizontalAlignment.Left;
		collpaseBtn.VerticalAlignment = VerticalAlignment.Bottom;
		collpaseBtn.Width = (int) (24 * scale);
		collpaseBtn.Height = (int) (7 * scale);
		collpaseBtn.ImageWidth = (int) (23 * scale);
		collpaseBtn.ImageHeight = (int) (7 * scale);
		collpaseBtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/invbtn"));
		collpaseBtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/invbtn"));
		collpaseBtn.Top = (int) (top - itemBtn.Height);
		collpaseBtn.Left = itemBtn.Left;
		if (invOpen)
		{
			collpaseBtn.Top = (int)(top - (i+1)*itemBtn.Height);
			
		}


		i = 0;
		foreach (var action in SelectedUnit.extraActions)
		{


			var index = i;
			var actbtn = ActionButtons[i];
			actbtn.HorizontalAlignment = HorizontalAlignment.Left;
			actbtn.VerticalAlignment = VerticalAlignment.Bottom;
			actbtn.Width = (int) (24 * scale);
			actbtn.Height = (int) (29 * scale);
			if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.UseAbility && UseExtraAbility.AbilityIndex == index)
			{
				actbtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), new Color(255, 140, 140));
				actbtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), new Color(255, 140, 140));
				actbtn.Image = new ColoredRegion(new TextureRegion(action.Icon), Color.Red);
			}
			else if(SelectedUnit.extraActions[index].HasEnoughPointsToPerform(SelectedUnit).Item1)
			{
				actbtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button"));
				actbtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button"));
				actbtn.Image = new TextureRegion(action.Icon);
			}
			else
			{
				actbtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")), Color.Gray);
				actbtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/BottomBar/button")),  Color.Gray);
				actbtn.Image = new ColoredRegion(new TextureRegion(action.Icon), Color.Gray);
			}

			actbtn.ImageHeight = (int) (29 * scale);
			actbtn.ImageWidth = (int) (24 * scale);
			actbtn.Top = top;
			actbtn.Left = startOffest + btnWidth*(3+i);
				

			i++;
		}
	}

	private static readonly List<Unit> Controllables = new();
	public static List<Unit> MyUnits = new();
	private static List<Unit> EnemyUnits = new();

	
	public static void RegisterUnit(Unit c)
	{
		
		Controllables.Add(c);
		if (c.IsMyTeam())
		{
			SelectUnit(c);
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

	public override void RenderBehindHud(SpriteBatch batch, float deltatime)
	{
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
			switch (WorldManager.Instance.GetTileAtGrid(TileCoordinate).GetCover((Direction) i))
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

			//spriteBatch.DrawCircle(Mousepos, 5, 10, Color.Red, 200f);
			batch.Draw(indicator, mousepos, c);
		}


	

		if (Action.GetActiveActionType() != null)
		{
			Debug.Assert(Action.ActiveAction != null, "Action.ActiveAction != null");
			Action.ActiveAction.Preview(SelectedUnit, TileCoordinate,batch);
		}
		
		
		batch.End();

		var tile = WorldManager.Instance.GetTileAtGrid(TileCoordinate);
		if (drawExtra)
		{
			foreach (var obj in tile.ObjectsAtLocation)
			{
				if (obj.LifeTime > 0&& obj.IsVisible())
				{
					batch.Begin(transformMatrix: Camera.GetViewMatrix(),samplerState: SamplerState.AnisotropicClamp);
					batch.DrawText(""+obj.LifeTime, Utility.GridToWorldPos(tile.Position + new Vector2(0f,0f)),  5,5, Color.White);
					obj.Type.DesturctionEffect?.Preview(obj.TileLocation.Position,batch,null);
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
		
		

/*
		//bottom right corner
		graphicsDevice.SetRenderTarget(statScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/statScreen"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		*/

		graphicsDevice.SetRenderTarget(dmgScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/BottomBar/screen"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		bool drawPreviewDmg = false;
		if (tile.UnitAtLocation != null && tile.UnitAtLocation.WorldObject.PreviewData.totalDmg != 0)
		{
			drawPreviewDmg = true;
			batch.DrawText("Damage", new Vector2(12, 8), 1, 5, Color.White);
			for (int i = 0; i < tile.UnitAtLocation.WorldObject.PreviewData.totalDmg; i++)
			{
				Color c = Color.Green;
				if ( tile.UnitAtLocation.WorldObject.PreviewData.totalDmg - tile.UnitAtLocation.WorldObject.PreviewData.finalDmg> i)
				{
					c = Color.Red;
				}
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 7) + i * new Vector2(5, 0), null, c, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
			batch.DrawLine(0,22,200,22,Color.White,2);
			batch.DrawText("Deter", new Vector2(12, 29), 1, 5, Color.White);
			for (int i = 0; i < tile.UnitAtLocation.WorldObject.PreviewData.determinationBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 28) + i * new Vector2(-5, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
			batch.DrawText("Cover", new Vector2(12, 44), 1, 5, Color.White);
			for (int i = 0; i < tile.UnitAtLocation.WorldObject.PreviewData.coverBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/healthenv"), new Vector2(55, 43) + i * new Vector2(5, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
			batch.DrawText("Range", new Vector2(12, 59), 1, 5, Color.White);
			for (int i = 0; i < tile.UnitAtLocation.WorldObject.PreviewData.distanceBlock; i++)
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
		batch.End();
		
		graphicsDevice.SetRenderTarget(timerRenderTarget);
	
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/unitframe"), new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, 1,SpriteEffects.None, 0);
		
		var totalLenght = 259 + 30;
		var fraction = GameManager.TimeTillNextTurn / (GameManager.PreGameData.TurnTime * 1000);
		var displayLenght = totalLenght - totalLenght * fraction;
		
		batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/Timer"), Vector2.Zero, null, Color.Gray, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/Timer"), Vector2.Zero, new Rectangle(0,0,190+(int)displayLenght,80), Color.White, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.End();
		PostPorcessing.ShuffleUIeffect(595,new Vector2(10,10),true);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.UIGlowEffect);
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

	
		batch.Draw(timerRenderTarget, new Vector2(Game1.resolution.X-timerRenderTarget.Width*globalScale.X*1.15f, 0), null, Color.White, 0, Vector2.Zero, globalScale.X*1.15f ,SpriteEffects.None, 0);
		
		Texture2D bar = TextureManager.GetTexture("UI/GameHud/BottomBar/mainbuttonbox");
		if (toolTip)
		{
			var box = TextureManager.GetTexture("UI/GameHud/BottomBar/Infobox");
			tooltipPos = new Vector2((Game1.resolution.X - box.Width * globalScale.X) / 2f, Game1.resolution.Y - box.Height * globalScale.X - bar.Height * globalScale.X + 5);
			batch.Draw(box, tooltipPos, null, Color.White, 0, Vector2.Zero, globalScale.X,SpriteEffects.None, 0);
		}
		
		
	
		if (drawPreviewDmg)
		{	batch.End();
			PostPorcessing.ShuffleUIeffect(100, new Vector2(dmgScreenRenderTarget.Width, dmgScreenRenderTarget.Height), false);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostPorcessing.UIGlowEffect);
			batch.Draw(dmgScreenRenderTarget, new Vector2((Game1.resolution.X - bar.Width * globalScale.X) / 2f + bar.Width * globalScale.X, Game1.resolution.Y - dmgScreenRenderTarget.Height * globalScale.X / 2f), null, Color.White, 0, Vector2.Zero, globalScale.X / 2f, SpriteEffects.None, 0);
			batch.End();
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		}
		else
		{
			batch.Draw(dmgScreenRenderTarget, new Vector2((Game1.resolution.X - bar.Width * globalScale.X) / 2f + bar.Width * globalScale.X, Game1.resolution.Y - dmgScreenRenderTarget.Height * globalScale.X / 2f), null, Color.White, 0, Vector2.Zero, globalScale.X / 2f, SpriteEffects.None, 0);
		}
		batch.End();
		PostPorcessing.ShuffleUIeffect(100, new Vector2(dmgScreenRenderTarget.Width, dmgScreenRenderTarget.Height), false);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp, effect: PostPorcessing.UIGlowEffect);
		batch.Draw(infoScreenRenderTarget, new Vector2((Game1.resolution.X - bar.Width * globalScale.X) - bar.Width * globalScale.X, Game1.resolution.Y - dmgScreenRenderTarget.Height * globalScale.X / 2f), null, Color.White, 0, Vector2.Zero, globalScale.X / 2f, SpriteEffects.None, 0);
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
		base.RenderFrontHud(batch, deltatime);
		/*
				//bottom left corner
		graphicsDevice.SetRenderTarget(leftCornerRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/base"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		
		float bulletWidth = TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletOn").Width;
		float baseWidth = 81;
		float bulletBarWidth = bulletWidth*SelectedUnit.Type.MaxActionPoints;
		float emtpySpace = baseWidth - bulletBarWidth;
		Vector2 bulletPos = new Vector2(5+emtpySpace/2f,106);
		for (int i = 0; i < SelectedUnit.Type.MaxActionPoints; i++)
		{
			Texture2D tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletOn");
			if(i >= SelectedUnit.ActionPoints)
				tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletOff");
			
			batch.Draw(tex,bulletPos + i*new Vector2(40,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
		}
		
		
		batch.End();

		Texture2D indi;
		if (SelectedUnit.canTurn)
		{
			indi = TextureManager.GetTexture("UI/GameHud/LeftPanel/turnon");
			PostPorcessing.ShuffleUIeffect( SelectedUnit.WorldObject.ID,new Vector2(indi.Width,indi.Height),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp, effect:PostPorcessing.UIGlowEffect);
		}
		else
		{
			indi = TextureManager.GetTexture("UI/GameHud/LeftPanel/turnoff");
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		}
		batch.Draw(indi,new Vector2(73,64),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
		batch.End();

		baseWidth = 84;
		float arrowWidth = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn").Width;
		float arrowHeight = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn").Height;
		float moveBarWidth = arrowWidth*SelectedUnit.Type.MaxMovePoints;
		emtpySpace = baseWidth - moveBarWidth;
		Vector2 arrowPos = new Vector2(12+emtpySpace/2f,70);
		for (int i = 0; i < SelectedUnit.Type.MaxMovePoints; i++)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowBackround"),arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
			Texture2D tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOff");
			if (i < SelectedUnit.MovePoints)
			{
				tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn");
			}
			PostPorcessing.ShuffleUIeffect(i + SelectedUnit.WorldObject.ID,new Vector2(arrowWidth,arrowHeight),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp,effect:PostPorcessing.UIGlowEffect);
			batch.Draw(tex,arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}

		for (int i = 0; i <  SelectedUnit.MovePoints - SelectedUnit.Type.MaxMovePoints; i++)
		{
			var tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOverflow");
			PostPorcessing.ShuffleUIeffect(i + SelectedUnit.WorldObject.ID,new Vector2(arrowWidth,arrowHeight),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp,effect:PostPorcessing.UIGlowEffect);
			batch.Draw(tex,arrowPos + i*new Vector2(25,0),null,Color.Black,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}

*/


		
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);

		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.DrawText("Z",new Vector2(itemBtn.Left+12*globalScale.Y+3*globalScale.Y,crouchbtn.Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		batch.DrawText("X",new Vector2(crouchbtn.Left+12*globalScale.Y+3*globalScale.Y,itemBtn.Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		batch.DrawText("C",new Vector2(overwatchBtn.Left+12*globalScale.Y+3*globalScale.Y,overwatchBtn.Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		//	batch.Draw(leftCornerRenderTarget, new Vector2(0, Game1.resolution.Y - leftCornerRenderTarget.Height*globalScale.Y*2f), null, Color.White, 0, Vector2.Zero, globalScale.Y*2f ,SpriteEffects.None, 0);

		if (ActionButtons.Count>0)
		{
			batch.DrawText("V",new Vector2(ActionButtons[0].Left+12*globalScale.Y+3*globalScale.Y,ActionButtons[0].Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		}
		if (ActionButtons.Count > 1)
		{
			batch.DrawText("B",new Vector2(ActionButtons[1].Left+12*globalScale.Y+3*globalScale.Y,ActionButtons[1].Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		}
		if (ActionButtons.Count > 2)
		{
			batch.DrawText("N",new Vector2(ActionButtons[2].Left+12*globalScale.Y+3*globalScale.Y,ActionButtons[2].Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		}
		if (toolTip)
		{			
			batch.DrawText(toolTipText,tooltipPos + new Vector2(5,2.5f)*globalScale.X, globalScale.X*0.6f,31,Color.White);
			int offset = 0;
			Vector2 startpos = tooltipPos+ new Vector2(12,55f)*globalScale.X;
			int movesToDraw = 0;
			if (moveChange > 0)
			{
				batch.DrawText("+", startpos + new Vector2(1,0)*offset*globalScale.X, globalScale.X, 24, Color.White);
				movesToDraw = moveChange;
				offset+= 1;

			}else if (moveChange < 0)
			{
				batch.DrawText("-", startpos + new Vector2(1,0)*offset*globalScale.X, globalScale.X , 24, Color.White);
				movesToDraw = -moveChange;
				offset+= 1;
			}

			for (int i = 0; i < movesToDraw; i++)
			{
					batch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"),startpos + new Vector2(1,0)*offset*globalScale.X,null,Color.White,0,Vector2.Zero,globalScale.X,SpriteEffects.None,0);
				offset+= 6;
			}
			offset+=10;
			int actsToDraw = 0;
			if (actChange > 0)
			{
				batch.DrawText("+", startpos + new Vector2(1,0)*offset*globalScale.X, globalScale.X, 24, Color.White);
				actsToDraw = actChange;
				offset+= 1;

			}else if (actChange < 0)
			{
				batch.DrawText("-", startpos + new Vector2(1,0)*offset*globalScale.X, globalScale.X , 24, Color.White);
				actsToDraw = -actChange;
				offset+= 1;
			}
			
			for (int i = 0; i < actsToDraw; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/actionpoint"),startpos + new Vector2(1,0)*offset*globalScale.X,null,Color.White,0,Vector2.Zero,globalScale.X,SpriteEffects.None,0);
				offset+= 7;
			}
			offset+=10;
			int detsToDraw = 0;
			if (detChange > 0)
			{
				batch.DrawText("+", startpos + new Vector2(1,0)*offset*globalScale.X, globalScale.X, 24, Color.White);
				detsToDraw = detChange;
				offset+= 1;

			}else if (detChange < 0)
			{
				batch.DrawText("-", startpos + new Vector2(1,0)*offset*globalScale.X, globalScale.X , 24, Color.White);
				detsToDraw = -detChange;
				offset+= 1;
			}
			
			for (int i = 0; i < detsToDraw; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/detGreen"),startpos + new Vector2(1,0)*offset*globalScale.X+ new Vector2(0,12),null,Color.White,0,Vector2.Zero,globalScale.X,SpriteEffects.None,0);
				offset+= 15;
			}
		}
		

		
	
		batch.End();

	}

	private Vector2Int MouseTileCoordinate = new(0, 0);
	private Vector2Int LastMouseTileCoordinate = new(0, 0);
	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		var count = 0;
	

		//moves selected contorlable to the top
		MouseTileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		MouseTileCoordinate = Vector2.Clamp(MouseTileCoordinate, Vector2.Zero, new Vector2(99, 99));
		int targetIndex = Controllables.IndexOf(SelectedUnit);
		if (targetIndex != -1)
		{
			for (int i = targetIndex; i < Controllables.Count - 1; i++)
			{
				Controllables[i] = Controllables[i + 1];
				Controllables[i + 1] = SelectedUnit;
			}
		}

		var tile = WorldManager.Instance.GetTileAtGrid(MouseTileCoordinate);
		
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

	


		if (WorldAction.FreeFire||( tile.UnitAtLocation != null && tile.UnitAtLocation.WorldObject.IsVisible() &&!tile.UnitAtLocation.IsMyTeam()))
		{
			//we should attack
			if(Action.ActiveAction == null){
				Action.SetActiveAction(ActionType.Attack);
			}
			
		}else if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.Attack)
		{
				
			Action.SetActiveAction(null);
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
		
		LastMouseTileCoordinate = MouseTileCoordinate;
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


		WorldAction.FreeFire = currentKeyboardState.IsKeyDown(Keys.LeftControl);
		if(Action.ActiveAction != null && currentKeyboardState.IsKeyUp(Keys.LeftControl) && lastKeyboardState.IsKeyDown(Keys.LeftControl) && Action.ActiveAction.ActionType == ActionType.Attack)
		{
			Action.SetActiveAction(null);
		}

		if(WorldAction.FreeFire){
			if (currentKeyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
			{
				if (Shootable.targeting == TargetingType.Auto)
				{
					Shootable.targeting = TargetingType.High;
				}
				else if (Shootable.targeting == TargetingType.High)
				{
					Shootable.targeting = TargetingType.Low;
				}
				else if (Shootable.targeting == TargetingType.Low)
				{
					Shootable.targeting = TargetingType.Auto;
				}
			}
		}
		else
		{
			Shootable.targeting = TargetingType.Auto;
		}

		if (JustPressed(Keys.Z))
		{
			itemBtn.DoClick();
		}else if (JustPressed(Keys.X))
		{
			crouchbtn.DoClick();
		}else if (JustPressed(Keys.C))
		{
			overwatchBtn.DoClick();
		}else if (JustPressed(Keys.V))
		{
			if (ActionButtons.Count > 0)
			{
				ActionButtons[0]?.DoClick();
			}
		}else if (JustPressed(Keys.B))
		{
			if (ActionButtons.Count > 1)
			{
				ActionButtons[1].DoClick();
			}
		}else if (JustPressed(Keys.N))
		{
			if (ActionButtons.Count > 2)
			{
				ActionButtons[2]?.DoClick();
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
		
	}

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);

		var Tile =WorldManager.Instance.GetTileAtGrid( Vector2.Clamp(position, Vector2.Zero, new Vector2(99, 99)));

		if (Tile.UnitAtLocation != null&& Tile.UnitAtLocation.WorldObject.GetMinimumVisibility() <= Tile.Visible && (Action.GetActiveActionType() == null||Action.GetActiveActionType() ==ActionType.Move)) { 
			SelectUnit(Tile.UnitAtLocation);
			return;
		}

		if (rightclick)
		{
			switch (Action.GetActiveActionType())
			{

				case null:
					Action.SetActiveAction(ActionType.Face);
					break;
				case ActionType.Face:
					SelectedUnit.DoAction(Action.ActiveAction,position);
					break;
				default:
					Action.SetActiveAction(null);
					break;
					

			}
		}
		else
		{
			switch (Action.GetActiveActionType())
			{

				case null:
					Action.SetActiveAction(ActionType.Move);
					break;
				default:
					SelectedUnit.DoAction(Action.ActiveAction, position);
					break;


			}

		}
	}


	private static float counter;
	private TextBox? inputBox;

	public static void DrawHoverHud(SpriteBatch batch, WorldObject target,float deltaTime)
	{
		counter += deltaTime/3000f;
		if (counter > 2)
		{
			counter= 0;
		}
		float animopacity = counter;
		if (counter > 1)
		{
			animopacity = 2- counter;
		}
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		float opacity = 1f;
		bool highlighted = false;

		
		if (Equals(SelectedUnit, target.UnitComponent) || MousePos == (Vector2) target.TileLocation.Position || (target.Type.Edge && Utility.IsClose(target,MousePos)))
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
			if (target.Type.Edge && MousePos == (Vector2)target.TileLocation.Position)
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
				PostPorcessing.ShuffleUIeffect(y + target.ID,new Vector2(healthWidth,healthHeight),highlighted,true);
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
				batch.Draw(healthTexture,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				//batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdone"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				batch.End();
				dmgDone++;
				continue;

			}
			Texture2D indicator;
			if (health)
			{
				PostPorcessing.ShuffleUIeffect(y + target.ID,new Vector2(healthWidth,healthHeight),highlighted);
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
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

				if (y == unit.Determination && !unit.paniced)
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
					PostPorcessing.ShuffleUIeffect(y + unit.WorldObject.ID + 10, new Vector2(detWidth, detHeight), highlighted, true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else if (litup)
				{

					PostPorcessing.ShuffleUIeffect(y + unit.WorldObject.ID + 10, new Vector2(detWidth, detHeight), highlighted);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
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
			if (unit.paniced)
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/panic"), new Vector2(healthBarPos.X,17), new Rectangle((int) healthBarPos.X,0,(int) healthBarWidth,TextureManager.GetTexture("UI/HoverHud/baseCompact").Height), Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
				batch.End();
			}

			Vector2Int pointPos = new Vector2Int((int)healthBarPos.X, 51);
			int o = 8;
			i = 0;
			for (int j = 0; j < target.UnitComponent.MovePoints; j++)
			{	
				PostPorcessing.ShuffleUIeffect(i + unit.WorldObject.ID + 6, new Vector2(detWidth, detHeight), highlighted);
				batch.Begin(sortMode: SpriteSortMode.Texture, samplerState: SamplerState.PointClamp, effect:PostPorcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/movepoint"),pointPos+new Vector2(o*i,0),Color.White);
				batch.End();
				i++;
			}

			i++;
			
			for (int j = 0; j < target.UnitComponent.ActionPoints; j++)
			{	
				PostPorcessing.ShuffleUIeffect(i + unit.WorldObject.ID + 6, new Vector2(detWidth, detHeight), highlighted);
				batch.Begin(sortMode: SpriteSortMode.Texture, samplerState:  SamplerState.PointClamp, effect:PostPorcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/actionpoint"),pointPos+new Vector2(o*i,0),null, Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
				batch.End();
				i++;
			}

		}


		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2)target.TileLocation.Position+offset)+new Vector2(-140,-180),null,Color.White*opacity,0,Vector2.Zero,2f,SpriteEffects.None,0);
		batch.End();

		
		

	}


}
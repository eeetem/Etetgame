using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
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

	private static List<Vector2Int>[] previewMoves = Array.Empty<List<Vector2Int>>();

	


	public static void SetScore(int score)
	{
		if (ScoreIndicator != null)
		{
			ScoreIndicator.Text = "score: " + score;
		}
	}

	public static Unit SelectedUnit { get; private set;} = null!;

	public static void SelectControllable(Unit? controllable)
	{
		if (controllable == null)
		{
			controllable = MyUnits.FirstOrDefault();
		}

		if (!controllable!.IsMyTeam())
		{
			return;
		}

		SelectedUnit = controllable;
		ReMakeMovePreview();
		UI.SetUI( new GameLayout());
		Camera.SetPos(controllable.worldObject.TileLocation.Position);
	}
	public static void ReMakeMovePreview()
	{
		previewMoves = SelectedUnit.GetPossibleMoveLocations();

	}

	private static RenderTarget2D? hoverHudRenderTarget;
	private static RenderTarget2D? rightCornerRenderTarget;
	private static RenderTarget2D? leftCornerRenderTarget;
	private static RenderTarget2D? statScreenRenderTarget;
	private static RenderTarget2D? dmgScreenRenderTarget;
	private static RenderTarget2D? endBarRenderTarget;
	private static RenderTarget2D? chatRenderTarget;
	private static RenderTarget2D? chatScreenRenderTarget;
	private static Dictionary<Unit, RenderTarget2D>? unitBarRenderTargets;
		
	private static bool inited = false;

	public static void Init()
	{
		if (inited) return;
		rightCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/frames").Width,TextureManager.GetTexture("UI/GameHud/frames").Height);
		leftCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/LeftPanel/base").Width,TextureManager.GetTexture("UI/GameHud/LeftPanel/base").Height);
		hoverHudRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/HoverHud/base").Width,TextureManager.GetTexture("UI/HoverHud/base").Height);
		statScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/statScreen").Width,TextureManager.GetTexture("UI/GameHud/statScreen").Height);
		dmgScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/dmgScreen").Width,TextureManager.GetTexture("UI/GameHud/dmgScreen").Height);
		chatRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/chatframe").Width,TextureManager.GetTexture("UI/GameHud/chatframe").Height);
		chatScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/chatscreen").Width,TextureManager.GetTexture("UI/GameHud/chatscreen").Height);
		endBarRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/endframe1").Width,TextureManager.GetTexture("UI/GameHud/endframe1").Height);
		unitBarRenderTargets = new Dictionary<Unit, RenderTarget2D>();
	}


	public static void MakeUnitBarRenders(SpriteBatch batch)
	{
		if(_unitBar == null) return;
		int column = 0;
		foreach (var unit in MyUnits)
		{
			if (!unitBarRenderTargets.ContainsKey(unit))
			{
				unitBarRenderTargets.Add(unit, new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Width,TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Height));
				
			}
			graphicsDevice.SetRenderTarget(unitBarRenderTargets[unit]);
			graphicsDevice.Clear(Color.Transparent);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/Background"), Vector2.Zero, Color.White);
			int i;
			if (unit.ActionPoints > 0 || unit.MovePoints > 0)
			{
				batch.End();
				PostPorcessing.ShuffleUIeffect(column+123,new Vector2(unitBarRenderTargets[unit].Width,unitBarRenderTargets[unit].Height));
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp, effect: PostPorcessing.UIGlowEffect);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/"+unit.Type.Name), Vector2.Zero, Color.White);
				batch.DrawText(column+1+"",new Vector2(2, 5), Color.White);
				batch.End();

				int notchpos = 0;
				for (i = 0; i <  unit.MovePoints; i++)
				{
					PostPorcessing.ShuffleUIeffect(column+123,new Vector2(TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch").Width,TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch").Height));
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp, effect: PostPorcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/moveNotch"), new Vector2(4*notchpos,0), Color.White);
					batch.End();
					notchpos++;
				}
				for (i = 0; i <  unit.ActionPoints; i++)
				{
					PostPorcessing.ShuffleUIeffect(column+123,new Vector2(TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch").Width,TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch").Height));
					batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp, effect: PostPorcessing.UIGlowEffect);
					batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/fireNotch"), new Vector2(4*notchpos,0), Color.White);
					batch.End();
					notchpos++;
				}

			}
			else
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/screen"), Vector2.Zero, Color.White);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/UnitBar/"+unit.Type.Name), Vector2.Zero, Color.White);
				batch.DrawText(column+1+"",new Vector2(2, 5), Color.White);
				batch.End();
			}
			

			

			var healthTexture = TextureManager.GetTexture("UI/GameHud/UnitBar/red");
			float healthWidth =  healthTexture.Width;
			float healthHeight = healthTexture.Height;
			int baseWidth = TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Width;
			int baseHeight = TextureManager.GetTexture("UI/GameHud/UnitBar/Background").Height;
			float healthBarWidth = (healthWidth+1)*unit.worldObject.Type.MaxHealth;
			float emtpySpace = baseWidth - healthBarWidth;
			Vector2 healthBarPos = new Vector2(emtpySpace/2f,22);


			for (int y = 0; y < unit.worldObject.Type.MaxHealth; y++)
			{
				bool health = !(y >= unit.worldObject.Health);
				if (health)
				{
					PostPorcessing.ShuffleUIeffect(y + unit.worldObject.Id, new Vector2(healthWidth, healthHeight), true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				}
				batch.Draw(healthTexture,healthBarPos+new Vector2((healthWidth+1)*y,0),null,Color.White,0,Vector2.Zero,1,SpriteEffects.None,0);
				batch.End();
			}
			
			healthTexture = TextureManager.GetTexture("UI/GameHud/UnitBar/green");
			healthWidth =  healthTexture.Width;
			healthHeight = healthTexture.Height;
			healthBarWidth = (healthWidth+1)*unit.Type.Maxdetermination;
			emtpySpace = baseWidth - healthBarWidth;
			healthBarPos = new Vector2(emtpySpace/2f,25);
	

			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{
				bool health = !(y >= unit.Determination);
				if (health)
				{
					PostPorcessing.ShuffleUIeffect(y + unit.worldObject.Id, new Vector2(healthWidth, healthHeight), true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				}
				batch.Draw(healthTexture,healthBarPos+new Vector2((healthWidth+1)*y,0),null,Color.White,0,Vector2.Zero,1,SpriteEffects.None,0);
				batch.End();
			}

			if(_unitBar.Widgets.Count > column){
				ImageButton elem = (ImageButton)_unitBar.Widgets[column];
				elem.GridColumn = column;
				var w = (int)(unitBarRenderTargets[unit].Width * 0.8f * globalScale.X);
				if (w % baseWidth > 0)
				{
					w+= baseWidth - w % baseWidth;
				}
				var h = (int)(unitBarRenderTargets[unit].Height * 0.8f * globalScale.X);
				if (h % baseHeight > 0)
				{
					h+= baseHeight - h % baseHeight;
				}
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
			column++;
		}
		

	}

	private static Grid? _unitBar;
	private static bool invOpen;
	private static bool showChat = true;
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		Init();
		WorldManager.Instance.MakeFovDirty(false);
		var panel = new Panel ();
		if (!GameManager.spectating)
		{
			endBtn = new ImageButton()
			{
				Top = (int) (0 * globalScale.X),
				Left = (int) (-16f * globalScale.X),
				Width = (int) (TextureManager.GetTexture("UI/GameHud/end button").Width * globalScale.X*1.3f),
				Height = (int) (TextureManager.GetTexture("UI/GameHud/end button").Height * globalScale.X*1.3f),
				ImageWidth = (int) (TextureManager.GetTexture("UI/GameHud/end button").Width * globalScale.X*1.3f),
				ImageHeight = (int) (TextureManager.GetTexture("UI/GameHud/end button").Height * globalScale.X*1.3f),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Background = new SolidBrush(Color.Transparent),
				OverBackground = new SolidBrush(Color.Transparent),
				PressedBackground = new SolidBrush(Color.Transparent),
				Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/end button")),
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

		var chatbtn = new ImageButton();
		chatbtn.HorizontalAlignment = HorizontalAlignment.Left;
		chatbtn.VerticalAlignment = VerticalAlignment.Center;
		chatbtn.Top = (int)(-100 * globalScale.Y);
		chatbtn.ImageHeight = (int)(TextureManager.GetTexture("UI/GameHud/chatboxbtn1").Height*4f);	
		chatbtn.ImageWidth = (int)(TextureManager.GetTexture("UI/GameHud/chatboxbtn1").Width*4f);	
		chatbtn.Width = (int)(TextureManager.GetTexture("UI/GameHud/chatboxbtn1").Width*4f);	
		chatbtn.Height = (int)(TextureManager.GetTexture("UI/GameHud/chatboxbtn1").Height*4f);	
		chatbtn.Background = new SolidBrush(Color.Transparent);
		chatbtn.PressedBackground = new SolidBrush(Color.Transparent);
		chatbtn.OverBackground = new SolidBrush(Color.Transparent);
	
		if (showChat)
		{
			chatbtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/chatboxbtn1"));
			chatbtn.Left = (int)(130 * globalScale.Y);
		}
		else
		{
			chatbtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/chatboxbtn2"));
			chatbtn.Left = (int)(0 * globalScale.Y);
		}
		chatbtn.Click += (sender, args) =>
		{
			showChat = !showChat;
			UI.SetUI(null);
		};
		panel.Widgets.Add(chatbtn);
		
		if (showChat)
		{
			inputBox = new TextBox();
			inputBox.HorizontalAlignment = HorizontalAlignment.Left;
			inputBox.Background = new SolidBrush(Color.Transparent);
			inputBox.Border = new SolidBrush(Color.Black);
			inputBox.BorderThickness = new Thickness(5);
			inputBox.Left = (int) (5f * globalScale.X);
			inputBox.Width = (int) (115 * globalScale.Y);
			inputBox.Top = (int) (55 * globalScale.Y);
			inputBox.Font = DefaultFont.GetFont(FontSize / 3f);
			inputBox.TextColor = Color.White;
			inputBox.VerticalAlignment = VerticalAlignment.Center;
			inputBox.KeyDown += (sender, args) =>
			{
				if (args.Data == Keys.Enter)
				{
					Chat.SendMessage(inputBox.Text);
					inputBox.Text = "";
				}

			};
			panel.Widgets.Add(inputBox);
		}

		
		if (ScoreIndicator == null)
		{
			ScoreIndicator = new Label()
			{
				Top=0,
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
			MaxWidth = (int)(500f*globalScale.X),
			Top = (int)(50f*globalScale.Y),
			//ShowGridLines = true,
		};
		panel.Widgets.Add(_unitBar);

		foreach (var unit in MyUnits)
		{
			var unitPanel = new ImageButton();

			Unit u = unit;
			unitPanel.Click += (sender, args) =>
			{
				SelectControllable(u);
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
					SelectedUnit.DoAction(Action.ActiveAction, SelectedUnit.worldObject.TileLocation.Position);
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
	private static ImageButton? collpaseBtn;
	private static readonly List<ImageButton> ActionButtons = new List<ImageButton>();
	private static readonly List<ImageButton> InvButtons = new List<ImageButton>();

	public static void UpdateActionButtons()
	{

		overwatchBtn.HorizontalAlignment = HorizontalAlignment.Left;
		overwatchBtn.VerticalAlignment = VerticalAlignment.Bottom;
		overwatchBtn.Width = (int) (24 * globalScale.Y * 2f);
		overwatchBtn.Height = (int) (29 * globalScale.Y * 2f);
		if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.OverWatch)
		{
			overwatchBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn")), new Color(255, 140, 140));
			overwatchBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn")), new Color(255, 140, 140));
		}
		else if (SelectedUnit.MovePoints>0 && SelectedUnit.ActionPoints>0)
		{
			overwatchBtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn"));
		}
		else
		{
			overwatchBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn")), Color.Gray);
			overwatchBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn")),  Color.Gray);
			overwatchBtn.Image =  new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn")),Color.Gray);
		}


		overwatchBtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
		overwatchBtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
		overwatchBtn.Top = (int) -10;
		overwatchBtn.Left = (int) (88 * globalScale.Y * 2f);

		crouchbtn.HorizontalAlignment = HorizontalAlignment.Left;
		crouchbtn.VerticalAlignment = VerticalAlignment.Bottom;
		crouchbtn.Width = (int) (24 * globalScale.Y * 2f);
		crouchbtn.Height = (int) (29 * globalScale.Y * 2f);
		var img  = new TextureRegion(TextureManager.GetTexture("UI/GameHud/crouchOff"));
		if (SelectedUnit.Crouching)
		{
			img = new TextureRegion(TextureManager.GetTexture("UI/GameHud/crouchOn"));
		}

		if (SelectedUnit.MovePoints > 0)
		{
			crouchbtn.Image = img;
		}
		else
		{
			crouchbtn.Image = new ColoredRegion(img, Color.Gray);
		}

		crouchbtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
		crouchbtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
		crouchbtn.Top = (int) (-10 - overwatchBtn.Height);
		crouchbtn.Left = (int) (88 * globalScale.Y * 2f);

		itemBtn.HorizontalAlignment = HorizontalAlignment.Left;
		itemBtn.VerticalAlignment = VerticalAlignment.Bottom;
		itemBtn.Width = (int) (24 * globalScale.Y * 2f);
		itemBtn.Height = (int) (29 * globalScale.Y * 2f);
		if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.UseItem)
		{
			itemBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
			itemBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
			if (SelectedUnit.SelectedItem != null)
			{
				itemBtn.Image = new ColoredRegion(new TextureRegion(SelectedUnit.SelectedItem?.Icon), new Color(255, 140, 140));
			}
		}
		else if(SelectedUnit.SelectedItem != null && SelectedUnit.ActionPoints > 0)
		{
			itemBtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
			itemBtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
			itemBtn.Image = new TextureRegion(SelectedUnit.SelectedItem?.Icon);

				
			
		}
		else
		{
			itemBtn.Background =  new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")),Color.Gray);
			itemBtn.OverBackground =  new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")),Color.Gray);
			if (SelectedUnit.SelectedItem != null)
			{
				itemBtn.Image = new ColoredRegion(new TextureRegion(SelectedUnit.SelectedItem?.Icon),Color.Gray);
			}else
			{
				itemBtn.Image = null;
			}
		}

		itemBtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
		itemBtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
		itemBtn.Top = (int) (-10 - overwatchBtn.Height);
		itemBtn.Left = (int) (88 * globalScale.Y * 2f + crouchbtn.Width);

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
				invbtn.Width = (int) (24 * globalScale.Y * 2f);
				invbtn.Height = (int) (29 * globalScale.Y * 2f);
				invbtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
				invbtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
				invbtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/empty"));
				if (inv != null)
				{
					invbtn.Image = new TextureRegion(inv.Icon);
				}
				else
				{
					invbtn.Image = null;
				}

				invbtn.Top = (int) (-10 - overwatchBtn.Height);
				invbtn.Left = (int) (88 * globalScale.Y * 2f + crouchbtn.Width + itemBtn.Width + itemBtn.Width * i);
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
		collpaseBtn.Width = (int) (7 * globalScale.Y * 2f);
		collpaseBtn.Height = (int) (29 * globalScale.Y * 2f);
		collpaseBtn.ImageWidth = (int) (7 * globalScale.Y * 2f);
		collpaseBtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
		collpaseBtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/invbtn"));
		collpaseBtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/invbtn"));
		collpaseBtn.Top = (int) (-10 - overwatchBtn.Height);
		collpaseBtn.Left = (int) (88 * globalScale.Y * 2f + crouchbtn.Width + itemBtn.Width + itemBtn.Width * i);
		

		i = 0;
		foreach (var action in SelectedUnit.extraActions)
		{


			var index = i;
			var actbtn = ActionButtons[i];
			actbtn.HorizontalAlignment = HorizontalAlignment.Left;
			actbtn.VerticalAlignment = VerticalAlignment.Bottom;
			actbtn.Width = (int) (24 * globalScale.Y * 2f);
			actbtn.Height = (int) (29 * globalScale.Y * 2f);
			if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.UseAbility && UseExtraAbility.AbilityIndex == index)
			{
				actbtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
				actbtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
				actbtn.Image = new ColoredRegion(new TextureRegion(action.Icon), Color.Red);
			}
			else if(SelectedUnit.extraActions[index].HasEnoughPointsToPerform(SelectedUnit).Item1)
			{
				actbtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
				actbtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
				actbtn.Image = new TextureRegion(action.Icon);
			}
			else
			{
				actbtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), Color.Gray);
				actbtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")),  Color.Gray);
				actbtn.Image = new ColoredRegion(new TextureRegion(action.Icon), Color.Gray);
			}

			actbtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
			actbtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
			actbtn.Top = -10;
			actbtn.Left = (int) (88* globalScale.Y * 2f + crouchbtn.Width * (i + 1));
				

			i++;
		}
	}

	private static readonly List<Unit> Controllables = new List<Unit>();
	private static List<Unit> MyUnits = new List<Unit>();
	private static List<Unit> EnemyUnits = new List<Unit>();

	
	public static void RegisterContollable(Unit c)
	{
		
		Controllables.Add(c);
		if (c.IsMyTeam())
		{
			SelectControllable(c);
			MyUnits.Add(c);
		}
		else
		{
			EnemyUnits.Add(c);
		}
	}
	public static void UnRegisterContollable(Unit c)
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
		var mousepos = Utility.GridToWorldPos((Vector2)TileCoordinate+new Vector2(-1.5f,-0.5f));
		for (int i = 0; i < 8; i++)
		{
		
			var indicator = TextureManager.GetSpriteSheet("UI/coverIndicator",3,3)[i];
			Color c = Color.White;
			switch ((Cover) WorldManager.Instance.GetTileAtGrid(TileCoordinate).GetCover((Direction) i))
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
			if (controllable.worldObject.IsVisible())
			{
				DrawHoverHud(batch, controllable.worldObject, deltatime);
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
		
		


		//bottom right corner
		graphicsDevice.SetRenderTarget(statScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/statScreen"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		
		graphicsDevice.SetRenderTarget(dmgScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/dmgScreen"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		if (tile.ControllableAtLocation != null && tile.ControllableAtLocation.PreviewData.totalDmg != 0)
		{
			for (int i = 0; i < tile.ControllableAtLocation?.PreviewData.totalDmg; i++)
			{
				Color c = Color.Green;
				if (tile.ControllableAtLocation?.PreviewData.finalDmg <= i)
				{
					c = Color.Red;
				}

				batch.Draw(TextureManager.GetTexture("UI/GameHud/pip"), new Vector2(80, 2) + i * new Vector2(-9, 0), null, c, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}

			for (int i = 0; i < tile.ControllableAtLocation?.PreviewData.distanceBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/pip"), new Vector2(66, 37) + i * new Vector2(-9, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}

			for (int i = 0; i < tile.ControllableAtLocation?.PreviewData.determinationBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/pip"), new Vector2(66, 52) + i * new Vector2(-9, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}

			for (int i = 0; i < tile.ControllableAtLocation?.PreviewData.coverBlock; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/pip"), new Vector2(66, 22) + i * new Vector2(-9, 0), null, Color.Green, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
			}
		}

		batch.End();
		
		graphicsDevice.SetRenderTarget(rightCornerRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/frames"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		PostPorcessing.ApplyScreenUICrt(new Vector2(statScreenRenderTarget.Width,statScreenRenderTarget.Height));
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.crtEffect);
		batch.Draw(statScreenRenderTarget,new Vector2(146,12),null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		PostPorcessing.ApplyScreenUICrt(new Vector2(dmgScreenRenderTarget.Width,dmgScreenRenderTarget.Height));
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.crtEffect);
		batch.Draw(dmgScreenRenderTarget,new Vector2(12,78),null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		
		

		graphicsDevice.SetRenderTarget(chatScreenRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/chatscreen"),new Vector2(0,0),null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		string chatmsg = "";
		foreach (var msg in Chat.Messages)
		{
			chatmsg +=msg+"\n";
		}
		batch.DrawText(chatmsg,new Vector2(22,25),1f,23,Color.White);
		batch.End();
		graphicsDevice.SetRenderTarget(chatRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		PostPorcessing.ApplyScreenUICrt(new Vector2(TextureManager.GetTexture("UI/GameHud/chatscreen").Width,TextureManager.GetTexture("UI/GameHud/chatscreen").Height));
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.crtEffect);
		batch.Draw(chatScreenRenderTarget,new Vector2(0,0),null,Color.White,0,Vector2.Zero,1f,SpriteEffects.None,0);
		batch.End();
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/chatframe"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		
		graphicsDevice.SetRenderTarget(endBarRenderTarget);
	
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/endframe1"), new Vector2(0, 0), null, Color.White, 0, Vector2.Zero, 1,SpriteEffects.None, 0);
		
		var totalLenght = TextureManager.GetTexture("UI/GameHud/endbar").Width-70;
		var fraction = GameManager.TimeTillNextTurn / (GameManager.PreGameData.TurnTime * 1000);
		var displayLenght = totalLenght - totalLenght * fraction;
		
		batch.Draw(TextureManager.GetTexture("UI/GameHud/endbar"), Vector2.Zero, null, Color.Gray, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/endbar"), Vector2.Zero, new Rectangle(0,0,(int)displayLenght,30), Color.White, 0, Vector2.Zero,1 ,SpriteEffects.None, 0);
		batch.End();
		//	PostPorcessing.ShuffleUIeffect(59,new Vector2(10,10),true);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		Color lightCol = Color.Red;
		if (GameManager.IsMyTurn())
		{
			lightCol = Color.Green;
		}
		batch.Draw(TextureManager.GetTexture(""), new Vector2(251, 29), null,lightCol, 0, Vector2.Zero, new Vector2(4.71f,1.62f),SpriteEffects.None, 0);
		batch.End();

		//final Draw
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		batch.Draw(rightCornerRenderTarget, new Vector2(Game1.resolution.X - (rightCornerRenderTarget.Width-104)*globalScale.Y*1.3f, Game1.resolution.Y - rightCornerRenderTarget.Height*globalScale.Y*1.3f), null, Color.White, 0, Vector2.Zero, globalScale.Y*1.3f ,SpriteEffects.None, 0);

	
		batch.Draw(endBarRenderTarget, new Vector2(Game1.resolution.X-endBarRenderTarget.Width*globalScale.X*1.3f, 0), null, Color.White, 0, Vector2.Zero, globalScale.X*1.3f ,SpriteEffects.None, 0);

		
		if (showChat)
		{
			batch.Draw(chatRenderTarget, new Vector2(0, 100 * globalScale.Y), null, Color.White, 0, Vector2.Zero, globalScale.Y * 0.6f, SpriteEffects.None, 0);
		}
		

		batch.End();

		
		
		
	}

	public override void RenderFrontHud(SpriteBatch batch, float deltatime)
	{
		base.RenderFrontHud(batch, deltatime);
		
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
			PostPorcessing.ShuffleUIeffect( SelectedUnit.worldObject.Id,new Vector2(indi.Width,indi.Height),true);
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
			PostPorcessing.ShuffleUIeffect(i + SelectedUnit.worldObject.Id,new Vector2(arrowWidth,arrowHeight),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp,effect:PostPorcessing.UIGlowEffect);
			batch.Draw(tex,arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}

		for (int i = 0; i <  SelectedUnit.MovePoints - SelectedUnit.Type.MaxMovePoints; i++)
		{
			var tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOverflow");
			PostPorcessing.ShuffleUIeffect(i + SelectedUnit.worldObject.Id,new Vector2(arrowWidth,arrowHeight),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp,effect:PostPorcessing.UIGlowEffect);
			batch.Draw(tex,arrowPos + i*new Vector2(25,0),null,Color.Black,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}



		if (toolTip)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/infobox"), new Vector2(0,0), null, Color.White, 0, Vector2.Zero, 1 ,SpriteEffects.None, 0);
			batch.End();
		}
		
		
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.DrawText("Q",new Vector2(crouchbtn.Left+3*globalScale.Y,crouchbtn.Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		batch.DrawText("E",new Vector2(itemBtn.Left+3*globalScale.Y,itemBtn.Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		batch.DrawText("Z",new Vector2(overwatchBtn.Left+3*globalScale.Y,overwatchBtn.Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		batch.Draw(leftCornerRenderTarget, new Vector2(0, Game1.resolution.Y - leftCornerRenderTarget.Height*globalScale.Y*2f), null, Color.White, 0, Vector2.Zero, globalScale.Y*2f ,SpriteEffects.None, 0);

		if (ActionButtons.Count>0)
		{
			batch.DrawText("X",new Vector2(ActionButtons[0].Left+3*globalScale.Y,ActionButtons[0].Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		}
		if (ActionButtons.Count > 1)
		{
			batch.DrawText("C",new Vector2(ActionButtons[1].Left+3*globalScale.Y,ActionButtons[1].Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		}
		if (ActionButtons.Count > 2)
		{
			batch.DrawText("V",new Vector2(ActionButtons[2].Left+3*globalScale.Y,ActionButtons[2].Top+Game1.resolution.Y-20*globalScale.Y),globalScale.Y*1.6f, 1,Color.White);
		}
		if (toolTip)
		{
			batch.DrawText(toolTipText,new Vector2(0, Game1.resolution.Y - leftCornerRenderTarget.Height*globalScale.Y*2f+5),globalScale.Y*1.25f, 25,Color.White);
			int offset = 0;
			Vector2 startpos = new Vector2(0, Game1.resolution.Y - (leftCornerRenderTarget.Height-42)*globalScale.Y*2f);
			//Vector2 offset = new Vector2(20f*globalScale.Y,00);
			int movesToDraw = 0;
			if (moveChange > 0)
			{
				batch.DrawText("+", startpos + new Vector2(1,0)*offset*globalScale.Y, globalScale.Y *2f, 24, Color.White);
				movesToDraw = moveChange;
				offset+= 10;

			}else if (moveChange < 0)
			{
				batch.DrawText("-", startpos + new Vector2(1,0)*offset*globalScale.Y, globalScale.Y * 2f, 24, Color.White);
				movesToDraw = -moveChange;
				offset+= 10;
			}

			for (int i = 0; i < movesToDraw; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn"),startpos + new Vector2(1,0)*offset*globalScale.Y,null,Color.White,0,Vector2.Zero,globalScale.Y,SpriteEffects.None,0);
				offset+= 25;
			}
			offset+=10;
			int actsToDraw = 0;
			if (actChange > 0)
			{
				batch.DrawText("+", startpos + new Vector2(1,0)*offset*globalScale.Y, globalScale.Y *2f, 24, Color.White);
				actsToDraw = actChange;
				offset+= 15;

			}else if (actChange < 0)
			{
				batch.DrawText("-", startpos + new Vector2(1,0)*offset*globalScale.Y, globalScale.Y * 2f, 24, Color.White);
				actsToDraw = -actChange;
				offset+= 15;
			}
			
			for (int i = 0; i < actsToDraw; i++)
			{
				batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletON"),startpos + new Vector2(1,0)*offset*globalScale.Y + new Vector2(0,18),null,Color.White,0,Vector2.Zero,globalScale.Y,SpriteEffects.None,0);
				offset+= 50;
			}
			
			int detsToDraw = 0;
			if (detChange > 0)
			{
				batch.DrawText("+", startpos + new Vector2(1,0)*offset*globalScale.Y, globalScale.Y *2f, 24, Color.White);
				detsToDraw = detChange;
				offset+= 12;

			}else if (detChange < 0)
			{
				batch.DrawText("-", startpos + new Vector2(1,0)*offset*globalScale.Y, globalScale.Y * 2f, 24, Color.White);
				detsToDraw = -detChange;
				offset+= 12;
			}
			
			for (int i = 0; i < detsToDraw; i++)
			{
				batch.Draw(TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1)[1],startpos + new Vector2(1,0)*offset*globalScale.Y+ new Vector2(0,12),null,Color.White,0,Vector2.Zero,globalScale.Y*1.5f,SpriteEffects.None,0);
				offset+= 25;
			}
		}
		

		
	
		batch.End();

	}

	private Vector2Int MouseTileCoordinate = new Vector2Int(0, 0);
	private Vector2Int LastMouseTileCoordinate = new Vector2Int(0, 0);
	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		var count = 0;
		if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.Move)
		{

			foreach (var moves in previewMoves.Reverse())
			{
				foreach (var path in moves)
				{


					if (path.X < 0 || path.Y < 0) break;
					var pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

					Color c = Color.White;
					switch (count)
					{
						case 0:
							c = Color.Red;
							break;
						case 1:
							c = Color.Orange;
							break;
						case 2:
							c = Color.Yellow;
							break;
						case 3:
							c = Color.GreenYellow;
							break;
						case 4:
							c = Color.Green;
							break;
						case 5:
							c = Color.LightGreen;
							break;
						default:
							c = Color.White;
							break;

					}

					WorldManager.Instance.GetTileAtGrid(path).Surface.OverRideColor = c;
				}

				count++;
			}
		}

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
		
		if (tile.ControllableAtLocation != null)
		{
			targetIndex = Controllables.IndexOf(tile.ControllableAtLocation.ControllableComponent);
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

	


		if (WorldAction.FreeFire||( tile.ControllableAtLocation != null && tile.ControllableAtLocation.IsVisible() &&!tile.ControllableAtLocation.ControllableComponent.IsMyTeam()))
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
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/end button")), Color.Gray);
		}
		else
		{
			if (endBtn != null) endBtn.Image = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/end button")), Color.White);
		}
		
		LastMouseTileCoordinate = MouseTileCoordinate;
	}

	private bool drawExtra;
	
	public void ProcessKeyboard()
	{
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

		if (JustPressed(Keys.Q))
		{
			crouchbtn.DoClick();
		}else if (JustPressed(Keys.E))
		{
			itemBtn.DoClick();
		}else if (JustPressed(Keys.Z))
		{
			overwatchBtn.DoClick();
		}else if (JustPressed(Keys.X))
		{
			if (ActionButtons.Count > 0)
			{
				ActionButtons[0]?.DoClick();
			}
		}else if (JustPressed(Keys.C))
		{
			if (ActionButtons.Count > 1)
			{
				ActionButtons[1].DoClick();
			}
		}else if (JustPressed(Keys.V))
		{
			if (ActionButtons.Count > 2)
			{
				ActionButtons[2]?.DoClick();
			}
		}

		if (JustPressed(Keys.D1) && MyUnits.Count > 0)
		{
			SelectControllable(MyUnits[0]);
		}else if (JustPressed(Keys.D2) && MyUnits.Count > 1)
		{
			SelectControllable(MyUnits[1]);
		}else if (JustPressed(Keys.D3) && MyUnits.Count > 2)
		{
			SelectControllable(MyUnits[2]);
		}else if (JustPressed(Keys.D4) && MyUnits.Count > 3)
		{
			SelectControllable(MyUnits[3]);
		}else if (JustPressed(Keys.D5) && MyUnits.Count > 4)
		{
			SelectControllable(MyUnits[4]);
		}else if (JustPressed(Keys.D6) && MyUnits.Count > 5)
		{
			SelectControllable(MyUnits[5]);
		}else if (JustPressed(Keys.D7) && MyUnits.Count > 6)
		{
			SelectControllable(MyUnits[6]);
		}else if (JustPressed(Keys.D8) && MyUnits.Count > 7)
		{
			SelectControllable(MyUnits[7]);
		}else if (JustPressed(Keys.D9) && MyUnits.Count > 8)
		{
			SelectControllable(MyUnits[8]);
		}else if (JustPressed(Keys.D0) && MyUnits.Count > 9)
		{
			SelectControllable(MyUnits[9]);
		}
		
	}

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);

		var Tile =WorldManager.Instance.GetTileAtGrid( Vector2.Clamp(position, Vector2.Zero, new Vector2(99, 99)));

		WorldObject obj = Tile.ControllableAtLocation;
		if (obj!=null&&obj.ControllableComponent != null&& obj.GetMinimumVisibility() <= obj.TileLocation.Visible && (Action.GetActiveActionType() == null||Action.GetActiveActionType() ==ActionType.Move)) { 
			SelectControllable(obj.ControllableComponent);
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

		
		if (Equals(SelectedUnit, target.ControllableComponent) || MousePos == (Vector2) target.TileLocation.Position || (target.Type.Edge && Utility.IsClose(target,MousePos)))
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
		
		if (target.ControllableComponent == null)
		{
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/baseCompact"), new Vector2(healthBarPos.X,0), new Rectangle((int) healthBarPos.X,0,(int) healthBarWidth,TextureManager.GetTexture("UI/HoverHud/baseCompact").Height), Color.White);
		}
		else
		{
			detWidth = TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1)[0].Width;
			detHeight = TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1)[0].Height;
			detbarWidht = detWidth * target.ControllableComponent.Type.Maxdetermination;
			detEmtpySpace = baseWidth - detbarWidht;
			DetPos = new Vector2(detEmtpySpace / 2f, 18);
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
		if (target.ControllableComponent == null)
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
			foreach (var effect in target.ControllableComponent.StatusEffects)
			{
				batch.Draw(TextureManager.GetTexture("UI/StatusEffects/"+effect.type.name),new Vector2(23*i,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
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
				PostPorcessing.ShuffleUIeffect(y + target.Id,new Vector2(healthWidth,healthHeight),highlighted,true);
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
				PostPorcessing.ShuffleUIeffect(y + target.Id,new Vector2(healthWidth,healthHeight),highlighted);
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

		if (target.ControllableComponent != null)
		{
			Unit unit = target.ControllableComponent;

			i = 0;
			var lights = TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1);
			
			for (int y = 0; y < unit.Type.Maxdetermination; y++)
			{

				Texture2D indicator;
				bool litup = true;
				bool dissapate = false;
				bool pulse = false;
				if (unit.paniced)
				{
					indicator = lights[0];
				}
				else if (y == unit.Determination)
				{
					indicator = lights[2];
					pulse = true;
					litup = false;
					
				}
				else if (y >= unit.Determination)
				{
					indicator = lights[3];
					litup = false;
				}
				else
				{
					indicator = lights[1];
					if (target.PreviewData.detDmg >= unit.Determination - y)
					{
						dissapate = true;
					}

				}
				
				
				if (dissapate)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(lights[3], DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					PostPorcessing.ShuffleUIeffect(y + unit.worldObject.Id + 10, new Vector2(detWidth, detHeight), highlighted, true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIGlowEffect);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else if (litup)
				{

					PostPorcessing.ShuffleUIeffect(y + unit.worldObject.Id + 10, new Vector2(detWidth, detHeight), highlighted);
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
						batch.Draw(lights[3], DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
						batch.End();
					}

					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White*op, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}

			}
		}

		batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);




		batch.End();
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2)target.TileLocation.Position+offset)+new Vector2(-140,-180),null,Color.White*opacity,0,Vector2.Zero,2f,SpriteEffects.None,0);
		batch.End();

		
		

	}


}
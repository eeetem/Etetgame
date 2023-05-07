using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MultiplayerXeno.Items;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameLayout : MenuLayout
{
	private static Panel turnIndicator;
	private static Label scoreIndicator;

	private static List<Vector2Int>[] previewMoves = Array.Empty<List<Vector2Int>>();

	
	public static void SetMyTurn(bool myTurn)
	{
		if (myTurn)
		{
			turnIndicator.Background = new SolidBrush(Color.Green);

		}
		else
		{
			turnIndicator.Background = new SolidBrush(Color.Red);
		}
	}
	public static void SetScore(int score)
	{
		scoreIndicator.Text = "score: " + score;
	}

	public static Controllable? SelectedControllable { get; private set;}

	public static void SelectControllable(Controllable controllable)
	{
			
		if (!controllable.IsMyTeam())
		{
			return;
		}

		SelectedControllable = controllable;
		ReMakeMovePreview();
		UI.SetUI( new GameLayout());
		Camera.SetPos(controllable.worldObject.TileLocation.Position);
			
	}
	public static void ReMakeMovePreview()
	{
		if(SelectedControllable!=null){
			previewMoves = SelectedControllable.GetPossibleMoveLocations();
		}

	}

	private static RenderTarget2D? hoverHudRenderTarget;
	private static RenderTarget2D? rightCornerRenderTarget;
	private static RenderTarget2D? leftCornerRenderTarget;
	private static RenderTarget2D? statScreenRenderTarget;
	private static RenderTarget2D? dmgScreenRenderTarget;
		
	private static bool inited = false;

	public static void Init()
	{
		if (inited) return;
		rightCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/frames").Width,TextureManager.GetTexture("UI/GameHud/frames").Height);
		leftCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/LeftPanel/base").Width,TextureManager.GetTexture("UI/GameHud/LeftPanel/base").Height);
		hoverHudRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/HoverHud/base").Width,TextureManager.GetTexture("UI/HoverHud/base").Height);
		statScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/statScreen").Width,TextureManager.GetTexture("UI/GameHud/statScreen").Height);
		dmgScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/RightScreen/dmgScreen").Width,TextureManager.GetTexture("UI/GameHud/dmgScreen").Height);
	}


	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		Init();
		
		var panel = new Panel ();
		if (!GameManager.spectating)
		{
			var end = new TextButton
			{
				Top = (int) (0f * globalScale.Y),
				Left = (int) (-10f * globalScale.X),
				Width = (int) (80 * globalScale.X),
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Top,
				Text = "End Turn",
				Font = DefaultFont.GetFont(FontSize/2)
				//Scale = globalScale
			};
			end.Click += (o, a) => GameManager.EndTurn(); 
			panel.Widgets.Add(end);
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


		turnIndicator = new Panel()
		{
			Top= (int)(-1f*globalScale.Y),
			Left =(int)(-150f*globalScale.X),
			Height =50,
			Width = (int)(80 * globalScale.X),
			HorizontalAlignment = HorizontalAlignment.Right,
			VerticalAlignment = VerticalAlignment.Top,
			Background = new SolidBrush(Color.Red),
			//Scale = globalScale
		};
		panel.Widgets.Add(turnIndicator);
		SetMyTurn(GameManager.IsMyTurn());
		if (scoreIndicator == null)
		{


			scoreIndicator = new Label()
			{
				Top=0,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Left
			};
			SetScore(0);
		}

		panel.Widgets.Add(scoreIndicator);

		AttachSideChatBox(panel);
		
		var UnitContainer = new Grid()
		{
			GridColumnSpan = 4,
			GridRowSpan = 1,
			RowSpacing = 10,
			ColumnSpacing = 10,
			HorizontalAlignment = HorizontalAlignment.Center,
			VerticalAlignment = VerticalAlignment.Top,
			MaxWidth = (int)(700f*globalScale.X),
			//ShowGridLines = true,
		};
		panel.Widgets.Add(UnitContainer);

		var column = 0;

		foreach (var unit in MyUnits)
		{

			var unitPanel = new Panel()
			{
				Width = Math.Clamp((int) (100 * globalScale.X), 0, 110),
				Height = Math.Clamp((int) (200 * globalScale.Y), 0, 210),
				GridColumn = column,
				Background = new SolidBrush(Color.Black),

			};
			if (unit.Equals(SelectedControllable))
			{
				unitPanel.Background = new SolidBrush(Color.DimGray);
				unitPanel.Top = 25;
			}

			unitPanel.TouchDown += (sender, args) =>
			{
				if (unit.worldObject.Health > 0)
				{
					SelectControllable(unit);
				}
			};

			UnitContainer.Widgets.Add(unitPanel);
			var unitName = new Label()
			{
				Text = unit.Type.Name,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Center,
				Font = DefaultFont.GetFont(FontSize/2)
			};
			unitPanel.Widgets.Add(unitName);
			var unitImage = new Image()
			{
				Width = 80,
				Height = 80,
				Top = 20,
				VerticalAlignment = VerticalAlignment.Top,
				HorizontalAlignment = HorizontalAlignment.Center,
				Renderable = new TextureRegion(TextureManager.GetTexture("UI/PortraitAlive"))
			};
			if (unit.worldObject.Health <= 0)
			{
				unitImage.Renderable = new TextureRegion(TextureManager.GetTexture("UI/PortraitDead"));
				unitPanel.Top = -10;
				unitPanel.Background = new SolidBrush(Color.DarkRed);
			}unitPanel.Widgets.Add(unitImage);

			column++;
		}

		
			
		overwatchBtn = new ImageButton();
		overwatchBtn.Click += (o, a) => Action.SetActiveAction(ActionType.OverWatch);
		overwatchBtn.MouseEntered += (o, a) => Tooltip("Watches over an area and attacks the first enemy that enters it",0,-1,-1);
		overwatchBtn.MouseLeft += (o, a) =>  HideTooltip();
		panel.Widgets.Add(overwatchBtn);
		courchBtn = new ImageButton();
		courchBtn.Click += (o, a) => SelectedControllable.DoAction(Action.Actions[ActionType.Crouch], null);
		courchBtn.MouseEntered += (o, a) => Tooltip("Crouching improves benefits of cover and allows hiding under tall cover",0,0,-1);
		courchBtn.MouseLeft += (o, a) => HideTooltip();
		panel.Widgets.Add(courchBtn);
		itemBtn = new ImageButton();
		itemBtn.Click += (o, a) => Action.SetActiveAction(ActionType.UseItem);
		itemBtn.MouseEntered += (o, a) => Tooltip("Activates selected item",0,-1,0);
		itemBtn.MouseLeft += (o, a) => HideTooltip();
		panel.Widgets.Add(itemBtn);
		
		actionButtons.Clear();
		int i = 0;
		foreach (var action in SelectedControllable.extraActions)
		{
			int index = i;
			var btn = new ImageButton();
			actionButtons.Add(btn);
			btn.MouseEntered += (o, a) => Tooltip(action.Tooltip,action.GetConsiquences(SelectedControllable)[0],action.GetConsiquences(SelectedControllable)[1],action.GetConsiquences(SelectedControllable)[2]);
			btn.MouseLeft += (o, a) => HideTooltip();
			if (action.ImmideateActivation)
			{
				btn.MouseEntered += (o, a) =>
				{
					if (Action.ActiveAction == null)
					{
						UseExtraAbility.abilityIndex = index;
						Action.SetActiveAction(ActionType.UseAbility);
					}
				};
				btn.MouseLeft += (o, a) =>
				{
					if (UseExtraAbility.abilityIndex == index)
					{
						Action.SetActiveAction(null);
						UseExtraAbility.abilityIndex = -1;
					}
				};
				btn.Click += (o, a) =>
				{
					Console.WriteLine("click");
					Action.SetActiveAction(ActionType.UseAbility);
					UseExtraAbility.abilityIndex = index;
					SelectedControllable.DoAction(Action.ActiveAction, SelectedControllable.worldObject.TileLocation.Position);
				};
			}
			else
			{
				btn.Click += (o, a) =>
				{
					UseExtraAbility.abilityIndex = index;
					Action.SetActiveAction(ActionType.UseAbility);
				};
			}
			panel.Widgets.Add(btn);
			i++;
		}

		UpdateActionButtons();
		
		return panel;
	}

	private static bool toolTip = false;
	private static string toolTipText = "";
	private static int detChange = 0;
	private static int actChange = 0;
	private static int moveChange = 0;
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

	private static ImageButton overwatchBtn;
	private static ImageButton courchBtn;
	private static ImageButton itemBtn;
	private static List<ImageButton> actionButtons = new List<ImageButton>();

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
			else
			{
				overwatchBtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn"));
			}


			overwatchBtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
			overwatchBtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
			overwatchBtn.Top = (int) (-10);
			overwatchBtn.Left = (int) (TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f);

			courchBtn.HorizontalAlignment = HorizontalAlignment.Left;
			courchBtn.VerticalAlignment = VerticalAlignment.Bottom;
			courchBtn.Width = (int) (24 * globalScale.Y * 2f);
			courchBtn.Height = (int) (29 * globalScale.Y * 2f);
			if (SelectedControllable.Crouching)
			{
				courchBtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/crouchOn"));
			}
			else
			{
				courchBtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/crouchOff"));
			}

			courchBtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
			courchBtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
			courchBtn.Top = (int) (-10 - overwatchBtn.Height);
			courchBtn.Left = (int) (TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f);

			itemBtn.HorizontalAlignment = HorizontalAlignment.Left;
			itemBtn.VerticalAlignment = VerticalAlignment.Bottom;
			itemBtn.Width = (int) (24 * globalScale.Y * 2f);
			itemBtn.Height = (int) (29 * globalScale.Y * 2f);
			if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.UseItem)
			{
				itemBtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
				itemBtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
				if (SelectedControllable.SelectedItem != null)
				{
					itemBtn.Image = new ColoredRegion(new TextureRegion(SelectedControllable.SelectedItem?.Icon), new Color(255, 140, 140));
				}
			}
			else
			{
				itemBtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
				itemBtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
				if (SelectedControllable.SelectedItem != null)
				{
					itemBtn.Image = new TextureRegion(SelectedControllable.SelectedItem?.Icon);
				}
			}

			itemBtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
			itemBtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
			itemBtn.Top = (int) (-10 - overwatchBtn.Height);
			itemBtn.Left = (int) (TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f + courchBtn.Width);

			int i = 0;
			foreach (var action in SelectedControllable.extraActions)
			{


				var index = i;
				var actbtn = actionButtons[i];
				actbtn.HorizontalAlignment = HorizontalAlignment.Left;
				actbtn.VerticalAlignment = VerticalAlignment.Bottom;
				actbtn.Width = (int) (24 * globalScale.Y * 2f);
				actbtn.Height = (int) (29 * globalScale.Y * 2f);
				if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.UseAbility && UseExtraAbility.abilityIndex == index)
				{
					actbtn.Background = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
					actbtn.OverBackground = new ColoredRegion(new TextureRegion(TextureManager.GetTexture("UI/GameHud/button")), new Color(255, 140, 140));
					actbtn.Image = new ColoredRegion(new TextureRegion(action.Icon), Color.Red);
				}
				else
				{
					actbtn.Background = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
					actbtn.OverBackground = new TextureRegion(TextureManager.GetTexture("UI/GameHud/button"));
					actbtn.Image = new TextureRegion(action.Icon);
				}

				actbtn.ImageHeight = (int) (29 * globalScale.Y * 2f);
				actbtn.ImageWidth = (int) (24 * globalScale.Y * 2f);
				actbtn.Top = (-10);
				actbtn.Left = (int) (TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f + (courchBtn.Width * (i + 1)));
				

				i++;
			}
	}

	private static readonly List<Controllable> Controllables = new List<Controllable>();
	private static List<Controllable> MyUnits = new List<Controllable>();
	private static List<Controllable> EnemyUnits = new List<Controllable>();

	
	public static void RegisterContollable(Controllable c)
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
	public static void UnRegisterContollable(Controllable c)
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


		var count = 0;
		foreach (var moves in previewMoves.Reverse())
		{
			foreach (var path in moves)
			{


				if (path.X < 0 || path.Y < 0) break;
				var  pos = Utility.GridToWorldPos((Vector2) path + new Vector2(0.5f, 0.5f));

				Color c = Color.White;
				switch (count)
				{
					case 0:
						c = Color.Purple;
						break;
					case 1:
						c = Color.Red;
						break;
					case 2:
						c = Color.Orange;
						break;
					case 3:
						c = Color.Yellow;
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

				batch.DrawRectangle(pos, new Size2(20, 20), c, 5);


			}

			count++;
		}

		if (Action.GetActiveActionType() != null)
		{
			Debug.Assert(Action.ActiveAction != null, "Action.ActiveAction != null");
			Action.ActiveAction.Preview(SelectedControllable, TileCoordinate,batch);
		}
		
		
		batch.End();
		foreach (var controllable in Controllables)
		{
			if (controllable.worldObject.IsVisible())
			{
				DrawHoverHud(batch, controllable.worldObject, deltatime);
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
					batch.DrawText(""+obj.LifeTime, Utility.GridToWorldPos(tile.Position + new Vector2(-0.4f,-0.4f)),  1,5, Color.White);
					batch.End();
				}

			}

			foreach (var edge in tile.GetAllEdges())
			{
				DrawHoverHud(batch, edge, deltatime);
			}
		}
		else
		{
			
			foreach (var edge in tile.GetAllEdges())
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
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
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
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.UIcrtEffect);
		batch.Draw(statScreenRenderTarget,new Vector2(146,12),null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		PostPorcessing.ApplyScreenUICrt(new Vector2(dmgScreenRenderTarget.Width,dmgScreenRenderTarget.Height));
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.UIcrtEffect);
		batch.Draw(dmgScreenRenderTarget,new Vector2(12,78),null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		batch.End();
		
		
		//bottom left corner
		graphicsDevice.SetRenderTarget(leftCornerRenderTarget);
		graphicsDevice.Clear(Color.Transparent);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/base"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		
		float bulletWidth = TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletOn").Width;
		float baseWidth = 81;
		float bulletBarWidth = (bulletWidth*SelectedControllable.Type.MaxActionPoints);
		float emtpySpace = baseWidth - bulletBarWidth;
		Vector2 bulletPos = new Vector2(95+emtpySpace/2f,106);
		for (int i = 0; i < SelectedControllable.Type.MaxActionPoints; i++)
		{
			Texture2D tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletOn");
			if(i >= SelectedControllable.ActionPoints)
				tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/bulletOff");
			
			batch.Draw(tex,bulletPos + i*new Vector2(40,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
		}
		
		
		batch.End();

		baseWidth = 84;
		float arrowWidth = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn").Width;
		float arrowHeight = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn").Height;
		float moveBarWidth = (arrowWidth*SelectedControllable.Type.MaxMovePoints);
		emtpySpace = baseWidth - moveBarWidth;
		Vector2 arrowPos = new Vector2(104+(emtpySpace/2f),70);
		for (int i = 0; i < SelectedControllable.Type.MaxMovePoints; i++)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowBackround"),arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
			Texture2D tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOff");
			if (i < SelectedControllable.MovePoints)
			{
				tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOn");
			}
			PostPorcessing.ShuffleUICRTeffect(i + SelectedControllable.worldObject.Id,new Vector2(arrowWidth,arrowHeight),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp,effect:PostPorcessing.UIcrtEffect);
			batch.Draw(tex,arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}

		for (int i = 0; i <  SelectedControllable.MovePoints - SelectedControllable.Type.MaxMovePoints; i++)
		{
			var tex = TextureManager.GetTexture("UI/GameHud/LeftPanel/arrowOverflow");
			PostPorcessing.ShuffleUICRTeffect(i+10 + SelectedControllable.worldObject.Id,new Vector2(arrowWidth,arrowHeight),true);
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.AnisotropicClamp,effect:PostPorcessing.UIcrtEffect);
			batch.Draw(tex,arrowPos + i*new Vector2(25,0),null,Color.Black,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}



		if (toolTip)
		{
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/LeftPanel/infobox"), new Vector2(0,0), null, Color.White, 0, Vector2.Zero, 1 ,SpriteEffects.None, 0);
			batch.End();
		}




		//final Draw
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		
		batch.Draw(rightCornerRenderTarget, new Vector2(Game1.resolution.X - rightCornerRenderTarget.Width*globalScale.Y*1.1f, Game1.resolution.Y - rightCornerRenderTarget.Height*globalScale.Y*1.1f), null, Color.White, 0, Vector2.Zero, globalScale.Y*1.1f ,SpriteEffects.None, 0);

		batch.Draw(leftCornerRenderTarget, new Vector2(0, Game1.resolution.Y - leftCornerRenderTarget.Height*globalScale.Y*2f), null, Color.White, 0, Vector2.Zero, globalScale.Y*2f ,SpriteEffects.None, 0);

		if (toolTip)
		{
			batch.DrawText(toolTipText,new Vector2(190*globalScale.Y, Game1.resolution.Y - leftCornerRenderTarget.Height*globalScale.Y*2f+5),globalScale.Y*1.25f, 27,Color.White);
			int offset = 0;
			Vector2 startpos = new Vector2(190*globalScale.Y, Game1.resolution.Y - (leftCornerRenderTarget.Height-42)*globalScale.Y*2f);
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

	public override void RenderFrontHud(SpriteBatch batch, float deltatime)
	{
		base.RenderFrontHud(batch, deltatime);

	}

	private Vector2Int MouseTileCoordinate = new Vector2Int(0, 0);
	private Vector2Int LastMouseTileCoordinate = new Vector2Int(0, 0);
	public override void Update(float deltatime)
	{
		base.Update(deltatime);
		
		//moves selected contorlable to the top
		MouseTileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		MouseTileCoordinate = Vector2.Clamp(MouseTileCoordinate, Vector2.Zero, new Vector2(99, 99));
		int targetIndex = Controllables.IndexOf(SelectedControllable);
		if (targetIndex != -1)
		{
			for (int i = targetIndex; i < Controllables.Count - 1; i++)
			{
				Controllables[i] = Controllables[i + 1];
				Controllables[i + 1] = SelectedControllable;
			}
		}

		var tile = WorldManager.Instance.GetTileAtGrid(MouseTileCoordinate);
		
		if (tile.ControllableAtLocation != null)
		{
			targetIndex = Controllables.IndexOf(tile.ControllableAtLocation.ControllableComponent);
			if (targetIndex != -1)
			{
				Controllable target = Controllables[targetIndex];
				for (int i = targetIndex; i < Controllables.Count - 1; i++)
				{
						
					Controllables[i] = Controllables[i + 1];
					Controllables[i + 1] = target;
				}
			}
			
		}

	


			if ((Shootable.freeFire||( tile.ControllableAtLocation != null && tile.ControllableAtLocation.IsVisible() &&!tile.ControllableAtLocation.ControllableComponent.IsMyTeam())))
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
		LastMouseTileCoordinate = MouseTileCoordinate;
	}

	private bool drawExtra = false;
	
	public void ProcessKeyboard()
	{
		if (lastKeyboardState.IsKeyDown(Keys.Tab))
		{
			UI.Desktop.FocusedKeyboardWidget = null;//override myra focus switch functionality
		}
		if(UI.Desktop.FocusedKeyboardWidget != null) return;

		drawExtra = false;
		if (currentKeyboardState.IsKeyDown(Keys.LeftAlt))
		{
			drawExtra = true;
		}
		
		
		Shootable.freeFire = currentKeyboardState.IsKeyDown(Keys.LeftControl);
		if(Action.ActiveAction != null && currentKeyboardState.IsKeyUp(Keys.LeftControl) && lastKeyboardState.IsKeyDown(Keys.LeftControl) && Action.ActiveAction.ActionType == ActionType.Attack)
		{
			Action.SetActiveAction(null);
		}

		if(Shootable.freeFire){
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
		
	}

	public override void MouseDown(Vector2Int position, bool rightclick)
	{
		base.MouseDown(position, rightclick);
		Console.WriteLine("mouseodnw");

		
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
					SelectedControllable.DoAction(Action.ActiveAction,position);
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
					SelectedControllable.DoAction(Action.ActiveAction, position);
					break;


			}

		}
	}


	private static float counter = 0;

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

		
		if (Equals(SelectedControllable, target.ControllableComponent) || MousePos == (Vector2) target.TileLocation.Position || (target.Type.Edge && Utility.IsClose(target,MousePos)))
		{
			opacity = 1;
			highlighted = true;
		}
		
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(TextureManager.GetTexture("UI/HoverHud/base"), Vector2.One, null,Color.White);
		batch.End();
		float healthWidth = TextureManager.GetTexture("UI/HoverHud/nohealth").Width;
		float healthHeight = TextureManager.GetTexture("UI/HoverHud/nohealth").Height;
		float baseWidth = TextureManager.GetTexture("UI/HoverHud/base").Width;
		float healthBarWidth = (healthWidth*target.Type.MaxHealth);
		float emtpySpace = baseWidth - healthBarWidth;
		Vector2 healthBarPos = new Vector2(emtpySpace/2f,36);
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
				PostPorcessing.ShuffleUICRTeffect(y + target.Id,new Vector2(healthWidth,healthHeight),highlighted,true);
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
				batch.Draw(healthTexture,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				//batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdone"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				batch.End();
				dmgDone++;
				continue;

			}
			Texture2D indicator;
			if (health)
			{
				PostPorcessing.ShuffleUICRTeffect(y + target.Id,new Vector2(healthWidth,healthHeight),highlighted);
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
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
			Controllable controllable = target.ControllableComponent;
			float detWidth = TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1)[0].Width;
			float detHeight = TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1)[0].Height;
			float detbarWidht = (detWidth * controllable.Type.Maxdetermination);
			float detEmtpySpace = baseWidth - detbarWidht;
			Vector2 DetPos = new Vector2(detEmtpySpace / 2f, 18);
			i = 0;
			var lights = TextureManager.GetSpriteSheet("UI/HoverHud/detlights", 4, 1);
			for (int y = 0; y < controllable.Type.Maxdetermination; y++)
			{

				Texture2D indicator;
				bool litup = true;
				bool dissapate = false;
				if (controllable.paniced)
				{
					indicator = lights[0];
				}
				else if (y == controllable.Determination)
				{
					indicator = lights[2];
					if (target.PreviewData.detDmg > 0)
					{
						dissapate = true;
					}
				}
				else if (y >= controllable.Determination)
				{
					indicator = lights[3];
					litup = false;
				}
				else
				{
					indicator = lights[1];
					if (target.PreviewData.detDmg >= controllable.Determination - y)
					{
						dissapate = true;
					}

				}

				if (dissapate)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(lights[3], DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
					PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id + 10, new Vector2(detWidth, detHeight), highlighted, true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else if (litup)
				{
					PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id + 10, new Vector2(detWidth, detHeight), highlighted);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(indicator, DetPos + new Vector2(detWidth * y, 0), null, Color.White, 0, Vector2.Zero, new Vector2(1, 1), SpriteEffects.None, 0);
					batch.End();
				}

			}
		}

		batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);




		batch.End();
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2)target.TileLocation.Position+offset)+new Vector2(-150,-150),null,Color.White*opacity,0,Vector2.Zero,2.5f,SpriteEffects.None,0);
	
		
		batch.End();

		
		

	}


}
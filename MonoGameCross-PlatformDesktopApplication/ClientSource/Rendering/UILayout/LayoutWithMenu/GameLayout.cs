using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MultiplayerXeno;
using FontStashSharp.RichText;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
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
		rightCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/frames").Width,TextureManager.GetTexture("UI/GameHud/frames").Height);
		leftCornerRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/base").Width,TextureManager.GetTexture("UI/GameHud/base").Height);
		hoverHudRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/HoverHud/base").Width,TextureManager.GetTexture("UI/HoverHud/base").Height);
		statScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/statScreen").Width,TextureManager.GetTexture("UI/GameHud/statScreen").Height);
		dmgScreenRenderTarget = new RenderTarget2D(graphicsDevice,TextureManager.GetTexture("UI/GameHud/dmgScreen").Width,TextureManager.GetTexture("UI/GameHud/dmgScreen").Height);
	}


	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		Init();
		ReMakeMovePreview();
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


			var watch = new ImageButton();

			watch.HorizontalAlignment = HorizontalAlignment.Left;
			watch.VerticalAlignment = VerticalAlignment.Bottom;
			watch.Width = (int) (24 * globalScale.Y*2f);
			watch.Height = (int) (29 * globalScale.Y*2f);
			watch.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/overwatchbtn"));
			watch.ImageHeight = (int) (29 * globalScale.Y*2f);
			watch.ImageWidth = (int) (24 * globalScale.Y*2f);
			watch.Top = (int) (-10);
			watch.Click += (o, a) => Action.SetActiveAction(ActionType.OverWatch);
			watch.Left = (int)(TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f);
			panel.Widgets.Add(watch);

			var crouch = new ImageButton();
			crouch.HorizontalAlignment = HorizontalAlignment.Left;
			crouch.VerticalAlignment = VerticalAlignment.Bottom;
			crouch.Width = (int) (24 * globalScale.Y*2f);
			crouch.Height = (int) (29 * globalScale.Y*2f);
			crouch.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/crouchbtn"));
			crouch.ImageHeight = (int) (29 * globalScale.Y*2f);
			crouch.ImageWidth = (int) (24 * globalScale.Y*2f);
			crouch.Top = (int) (-10 - watch.Height);
			crouch.Left = (int)(TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f);
			crouch.Click += (o, a) => SelectedControllable.DoAction(Action.Actions[ActionType.Crouch], null);
			panel.Widgets.Add(crouch);
			
			var itembtn = new ImageButton();
			itembtn.HorizontalAlignment = HorizontalAlignment.Left;
			itembtn.VerticalAlignment = VerticalAlignment.Bottom;
			itembtn.Width = (int) (24 * globalScale.Y*2f);
			itembtn.Height = (int) (29 * globalScale.Y*2f);
			itembtn.Image = new TextureRegion(TextureManager.GetTexture("UI/GameHud/invbtn"));
			itembtn.ImageHeight = (int) (29 * globalScale.Y*2f);
			itembtn.ImageWidth = (int) (24 * globalScale.Y*2f);
			itembtn.Top = (int) (-10 - watch.Height);
			itembtn.Left = (int) (TextureManager.GetTexture("UI/GameHud/base").Width * globalScale.Y * 2f + crouch.Width);
			itembtn.Click += (o, a) => Action.SetActiveAction(ActionType.UseItem);
			itembtn.Background = new SolidBrush(Color.Transparent);
			panel.Widgets.Add(itembtn);
			
		}
		
		
		
		return panel;
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
	
	public override void Render(SpriteBatch batch, float deltatime)
	{
		base.Render(batch, deltatime);
		
		
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
						c = Color.Red;
						break;
					case 1:
						c = Color.Yellow;
						break;
					case 2:
						c = Color.Green;
						break;
					default:
						c = Color.LightGreen;
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
					batch.DrawString(Game1.SpriteFont, ""+obj.LifeTime, Utility.GridToWorldPos(tile.Position + new Vector2(-0.4f,-0.4f)), Color.Red, 0, Vector2.Zero, 5f, SpriteEffects.None, 0);
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
			for (int i = 0; i < SelectedControllable.Type.WeaponDmg; i++)
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

			for (int i = 0; i < tile.ControllableAtLocation?.PreviewData.detDmg; i++)
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
		batch.Draw(TextureManager.GetTexture("UI/GameHud/base"),Vector2.Zero,null,Color.White,0,Vector2.Zero,new Vector2(1f,1f),SpriteEffects.None,0);
		
		float bulletWidth = TextureManager.GetTexture("UI/GameHud/bulletOn").Width;
		float baseWidth = 81;
		float bulletBarWidth = (bulletWidth*SelectedControllable.Type.MaxFirePoints);
		float emtpySpace = baseWidth - bulletBarWidth;
		Vector2 bulletPos = new Vector2(95+emtpySpace/2f,49);
		for (int i = 0; i < SelectedControllable.Type.MaxFirePoints; i++)
		{
			Texture2D tex = TextureManager.GetTexture("UI/GameHud/bulletOn");
			if(i >= SelectedControllable.FirePoints)
				tex = TextureManager.GetTexture("UI/GameHud/bulletOff");
			
			batch.Draw(tex,bulletPos + i*new Vector2(40,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
		}
		
		
		batch.End();

		baseWidth = 84;
		float arrowWidth = TextureManager.GetTexture("UI/GameHud/arrowOn").Width;
		float arrowHeight = TextureManager.GetTexture("UI/GameHud/arrowOn").Height;
		float moveBarWidth = (arrowWidth*SelectedControllable.Type.MaxMovePoints);
		emtpySpace = baseWidth - moveBarWidth;
		Vector2 arrowPos = new Vector2(104+(emtpySpace/2f),13);
		for (int i = 0; i < SelectedControllable.Type.MaxMovePoints; i++)
		{
			
		
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			batch.Draw(TextureManager.GetTexture("UI/GameHud/arrowOff"),arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
			if (i < SelectedControllable.MovePoints)
			{
				PostPorcessing.ShuffleUICRTeffect(i + SelectedControllable.worldObject.Id,new Vector2(arrowWidth,arrowHeight),true);
				batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp,effect:PostPorcessing.UIcrtEffect);
				batch.Draw(TextureManager.GetTexture("UI/GameHud/arrowOn"),arrowPos + i*new Vector2(25,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
			}
			
		
		}


		
		
		
		
		
		
		//final Draw
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(rightCornerRenderTarget, new Vector2(Game1.resolution.X - rightCornerRenderTarget.Width*globalScale.Y*1.1f, Game1.resolution.Y - rightCornerRenderTarget.Height*globalScale.Y*1.1f), null, Color.White, 0, Vector2.Zero, globalScale.Y*1.1f ,SpriteEffects.None, 0);
		batch.End();
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
		batch.Draw(leftCornerRenderTarget, new Vector2(0, Game1.resolution.Y - leftCornerRenderTarget.Height*globalScale.Y*2f), null, Color.White, 0, Vector2.Zero, globalScale.Y*2f ,SpriteEffects.None, 0);
		batch.End();

		batch.Begin();
		
		
		
	}
	
	bool freeFire = false;
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

		if (LastMouseTileCoordinate != MouseTileCoordinate)
		{
			if (Action.ActiveAction != null && Action.ActiveAction.ActionType == ActionType.Attack)
			{
				Action.SetActiveAction(null);
			}


			if (Action.ActiveAction == null && (freeFire||( tile.ControllableAtLocation != null && tile.ControllableAtLocation.IsVisible() &&!tile.ControllableAtLocation.ControllableComponent.IsMyTeam())))
			{
				Action.SetActiveAction(ActionType.Attack);
			}
		
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
		
		
		freeFire = currentKeyboardState.IsKeyDown(Keys.LeftControl);
		if (MyUnits.Count != 0)
		{
			if (currentKeyboardState.IsKeyDown(Keys.E) && lastKeyboardState.IsKeyUp(Keys.E))
			{
				
				int fails = 0;
				do
				{
					var index = MyUnits.FindIndex(i => i == SelectedControllable) + 1;
					if (index >= MyUnits.Count)
					{
						index = 0;
					}

					SelectControllable(MyUnits[index]);
					if(fails>MyUnits.Count)
						break;
					fails++;
				} while (SelectedControllable.worldObject.Health <= 0);


			}
			if (currentKeyboardState.IsKeyDown(Keys.Q) && lastKeyboardState.IsKeyUp(Keys.Q))
			{
				int fails = 0;
				do
				{
					var index = MyUnits.FindIndex(i => i == SelectedControllable)-1;
					if (index < 0)
					{
						index = MyUnits.Count-1;
					}
			
					SelectControllable(MyUnits[index]);
					if(fails>MyUnits.Count)
						break;
					fails++;
				} while (SelectedControllable.worldObject.Health <= 0);

			}
		}if(freeFire){
			if (currentKeyboardState.IsKeyDown(Keys.Tab) && lastKeyboardState.IsKeyUp(Keys.Tab))
			{
				if (Attack.targeting == TargetingType.Auto)
				{
					Attack.targeting = TargetingType.High;
				}
				else if (Attack.targeting == TargetingType.High)
				{
					Attack.targeting = TargetingType.Low;
				}
				else if (Attack.targeting == TargetingType.Low)
				{
					Attack.targeting = TargetingType.Auto;
				}
			}
		}
		else
		{
			Attack.targeting = TargetingType.Auto;
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
		Vector2 healthBarPos = new Vector2(emtpySpace/2f,22);
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
			Vector2 DetPos = new Vector2(detEmtpySpace / 2f, 4);
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
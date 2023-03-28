using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommonData;
using HeartSignal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MultiplayerXeno;
using MultiplayerXeno.UILayouts.LayoutWithMenu;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace MultiplayerXeno.UILayouts;

public class GameLayout : MenuLayout
{
	private static Panel turnIndicator;
	private static Label scoreIndicator;

	private static List<Vector2Int>[] previewMoves = new List<Vector2Int>[2];


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

	private static Label? descBox;
	public static string? PreviewDesc { get; set; }
	private static void SetPreviewDesc(string? desc)
	{
		PreviewDesc = desc;
		if(descBox!= null){
			descBox.Text = desc;
		}
	}
	public static Controllable SelectedControllable { get; private set;}

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
		previewMoves = SelectedControllable.GetPossibleMoveLocations();
	}



	
	public override Widget Generate(Desktop desktop, UiLayout? lastLayout)
	{
		if (hoverHudRenderTarget == null)
		{
			hoverHudRenderTarget = new RenderTarget2D(Game1.instance.GraphicsDevice,TextureManager.GetTexture("UI/HoverHud/base").Width,TextureManager.GetTexture("UI/HoverHud/base").Height);
		}
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
				if (unit.Health > 0)
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
			if (unit.Health <= 0)
			{
				unitImage.Renderable = new TextureRegion(TextureManager.GetTexture("UI/PortraitDead"));
				unitPanel.Top = -10;
				unitPanel.Background = new SolidBrush(Color.DarkRed);
			}unitPanel.Widgets.Add(unitImage);
			

		

			column++;
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
				DrawControllableHoverHud(batch, controllable, deltatime);
			}
		}
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
		
		if (tile.ObjectAtLocation?.ControllableComponent != null)
		{
			targetIndex = Controllables.IndexOf(tile.ObjectAtLocation.ControllableComponent);
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
			Action.SetActiveAction(null);

			if (freeFire||(tile.ObjectAtLocation?.ControllableComponent != null && !tile.ObjectAtLocation.ControllableComponent.IsMyTeam()))
			{
				Action.SetActiveAction(ActionType.Attack);
			}
		}


		ProcessKeyboard();
		LastMouseTileCoordinate = MouseTileCoordinate;
	}
	
	public void ProcessKeyboard()
	{
		if (lastKeyboardState.IsKeyDown(Keys.Tab))
		{
			UI.Desktop.FocusedKeyboardWidget = null;//override myra focus switch functionality
		}
		if(UI.Desktop.FocusedKeyboardWidget != null) return;
		
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
				} while (SelectedControllable.Health <= 0);


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
				} while (SelectedControllable.Health <= 0);

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

	public override void MouseDown(Vector2Int position, bool righclick)
	{
		base.MouseDown(position, righclick);
		
		var Tile = WorldManager.Instance.GetTileAtGrid(position);

		WorldObject obj = Tile.ObjectAtLocation;
		if (obj!=null&&obj.ControllableComponent != null&& obj.GetMinimumVisibility() <= obj.TileLocation.Visible && Action.GetActiveActionType() == null) { 
			SelectControllable(obj.ControllableComponent);
			return;
		}

		if (!GameManager.IsMyTurn()) return;
		if (righclick)
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
	private static RenderTarget2D? hoverHudRenderTarget;


	private static float counter = 0;

	public static void DrawControllableHoverHud(SpriteBatch batch, Controllable controllable,float deltaTime)
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

		Debug.Assert(controllable != null, nameof(controllable) + " != null");
		var MousePos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
		graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
		graphicsDevice.Clear(Color.White*0);
		float opacity = 1f;
		bool highlighted = false;

		if (SelectedControllable == controllable || MousePos == (Vector2) controllable.worldObject.TileLocation.Position)
		{
			opacity = 1;
			highlighted = true;
		}

			
		batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			

		int i;

		/*
			float detHeigth = 415f / controllable.Type.Maxdetermination;
		float detYscale = detHeigth/106;
		int detDMG = controllable.PreviewData.detDmg;
		for (i = 0; i < controllable.Type.Maxdetermination; i++)
		{
			
			var indicator = TextureManager.GetTexture("UI/HoverHud/deton");
			if (controllable.Type.Maxdetermination  - i  == controllable.determination+1 && !controllable.paniced)
			{
				indicator=TextureManager.GetTexture("UI/HoverHud/dethalf");
			}
			else if (controllable.Type.Maxdetermination - i > controllable.determination)
			{
				indicator=TextureManager.GetTexture("UI/HoverHud/detoff");
			}else if (detDMG>0)
			{
				batch.Draw(indicator,new Vector2(0,67)+new Vector2(0,detHeigth*i),null,Color.White*animopacity,0,Vector2.Zero,new Vector2(1,detYscale),SpriteEffects.None,0);
				detDMG--;
				continue;
			}
			if(indicator == TextureManager.GetTexture("UI/HoverHud/deton"))
			{
				//graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
				//batch.Begin(sortMode: SpriteSortMode.Immediate,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.colorEffect);

			}
			else
			{
			//	graphicsDevice.SetRenderTarget(hoverHudRenderTarget);
			//	/batch.Begin(sortMode: SpriteSortMode.Deferred);
			}
			batch.Draw(indicator, new Vector2(0, 67) + new Vector2(0, detHeigth * i), null, Color.White, 0, Vector2.Zero, new Vector2(1, detYscale), SpriteEffects.None, 0);
			//batch.End();
		}
		//batch.Begin(sortMode: SpriteSortMode.Deferred);
		*/
		batch.Draw(TextureManager.GetTexture("UI/HoverHud/base"), Vector2.One, null,Color.White);
		



		batch.End();
		float healthWidth = TextureManager.GetTexture("UI/HoverHud/nohealth").Width;
		float healthHeight = TextureManager.GetTexture("UI/HoverHud/nohealth").Height;
		float baseWidth = TextureManager.GetTexture("UI/HoverHud/base").Width;
		float healthBarWidth = (healthWidth*controllable.Type.MaxHealth);
		float emtpySpace = baseWidth - healthBarWidth;
		Vector2 healthBarPos = new Vector2(emtpySpace/2f,22);
		int dmgDone = 0; 
		i = 0;
		for (int y = 0; y < controllable.Type.MaxHealth; y++)
		{
			bool health = true;
				
			if (y>= controllable.Health)
			{
				health = false;
			}
			else if ( controllable.PreviewData.finalDmg >= controllable.Health-y)
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/nohealth"),healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
				i = y;
				PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id,new Vector2(healthWidth,healthHeight),highlighted,true);
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/health"),healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				//batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdone"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				batch.End();
				dmgDone++;
				continue;

			}
			Texture2D indicator;
			if (health)
			{
				PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id,new Vector2(healthWidth,healthHeight),highlighted);
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
				indicator = TextureManager.GetTexture("UI/HoverHud/health");
			}
			else
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				indicator= TextureManager.GetTexture("UI/HoverHud/nohealth");
			}

				
				
			batch.Draw(indicator,healthBarPos+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			batch.End();
		}
		float detWidth = TextureManager.GetSpriteSheet("UI/HoverHud/detlights",4,1)[0].Width;
		float detHeight = TextureManager.GetSpriteSheet("UI/HoverHud/detlights",4,1)[0].Height;
		float detbarWidht = (detWidth*controllable.Type.Maxdetermination);
		float detEmtpySpace = baseWidth - detbarWidht;
		Vector2 DetPos = new Vector2(detEmtpySpace/2f,4);
		i = 0;
		var lights = TextureManager.GetSpriteSheet("UI/HoverHud/detlights",4,1);
		for (int y = 0; y < controllable.Type.Maxdetermination; y++)
		{

			Texture2D indicator;
			bool litup = true;
			bool dissapate = false;
			if (controllable.paniced)
			{
				indicator = lights[0];
			}
			else if (y == controllable.determination)
			{
				indicator = lights[2];
				if (controllable.PreviewData.detDmg >0)
				{
					dissapate = true;
				}
			}
			else if (y >= controllable.determination)
			{
				indicator = lights[3];
				litup = false;
			}
			else
			{
				indicator = lights[1];
				if (controllable.PreviewData.detDmg >= controllable.determination-y)
				{
					dissapate = true;
				}
					
			}

			if (dissapate)
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(lights[3],DetPos+new Vector2(detWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
				PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id+10,new Vector2(detWidth,detHeight),highlighted,true);
				batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
				batch.Draw(indicator,DetPos+new Vector2(detWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
			}
			else if (litup)
			{
				PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id+10,new Vector2(detWidth,detHeight),highlighted);
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.AnisotropicClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
				batch.Draw(indicator,DetPos+new Vector2(detWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
			}
			else
			{
				batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
				batch.Draw(indicator,DetPos+new Vector2(detWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
				batch.End();
			}






		}
		batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
/*
			i++;
			for (int j = dmgDone; j < controllable.PreviewData.finalDmg; j++)
			{
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdone"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				i++;
			}

			for (int j = 0; j < controllable.PreviewData.coverBlock; j++)
			{
				//if(j>controllable.PreviewData.totalDmg) break;
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgcov"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				i++;
			}
			for (int j = 0; j < controllable.PreviewData.distanceBlock; j++)
			{
				//	if(j>controllable.PreviewData.totalDmg) break;
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgrange"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				i++;
			}
			for (int j = 0; j < controllable.PreviewData.determinationBlock; j++)
			{
				//	if(j>controllable.PreviewData.totalDmg) break;
				batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdet"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				i++;
			}
		*/
			
			
			
			
		batch.End();
		graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
		batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
		batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2)controllable.worldObject.TileLocation.Position)+new Vector2(-150,-150),null,Color.White*opacity,0,Vector2.Zero,2.5f,SpriteEffects.None,0);
		batch.End();


	}
}
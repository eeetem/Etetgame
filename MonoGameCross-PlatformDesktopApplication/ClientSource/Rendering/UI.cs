using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CommonData;
using FontStashSharp;
using HeartSignal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;
using MultiplayerXeno.UILayouts;
using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using Thickness = Myra.Graphics2D.Thickness;

namespace MultiplayerXeno
{
	public static class UI
	{
		public static Desktop Desktop { get; private set; }
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		private static Texture2D[] coverIndicator = new Texture2D[8];
		private static Texture2D targetingCUrsor;
		
		public static readonly List<Controllable> Controllables = new List<Controllable>();

		public static Texture2D[] infoIndicator; 
		public static void Init(ContentManager content, GraphicsDevice graphicsdevice)
		{
			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			
			Texture2D indicatorSpriteSheet = TextureManager.GetTexture("UI/indicators");
			infoIndicator = Utility.SplitTexture(indicatorSpriteSheet, indicatorSpriteSheet.Width / 6, indicatorSpriteSheet.Height);//this is inneficient but it'll be gone once new unit bar gets made
			MyraEnvironment.Game = Game1.instance;
			//	MyraEnvironment.DrawWidgetsFrames = true; MyraEnvironment.DrawTextGlyphsFrames = true;
	

			Desktop = new Desktop();
			Desktop.TouchDown += MouseDown;
			Desktop.TouchUp += MouseUp;


			Texture2D coverIndicatorSpriteSheet = content.Load<Texture2D>("textures/UI/coverIndicator");
			coverIndicator = Utility.SplitTexture(coverIndicatorSpriteSheet, coverIndicatorSpriteSheet.Width / 3, coverIndicatorSpriteSheet.Width / 3);
			targetingCUrsor = TextureManager.GetTexture("UI/targetingCursor");
			
			previewMoves[0] = new List<Vector2Int>();
			previewMoves[1] = new List<Vector2Int>();
			hoverHudRenderTarget = new RenderTarget2D(graphicsdevice,144,60);
		}



		public delegate void UIGen();
		

		private static UiLayout CurrentUI;
		private static Widget root;
		public static void SetUI(UiLayout? newUI)
		{

			UiLayout.SetScale(new Vector2((Game1.resolution.X / 500f) * 1f, (Game1.resolution.Y / 500f) * 1f));
			
		
			if (newUI != null)
			{
				root = newUI.Generate(Desktop, CurrentUI);
				CurrentUI = newUI;
			}
			else
			{
				root = CurrentUI.Generate(Desktop, CurrentUI);
			}

			Console.WriteLine("Changing UI to: "+CurrentUI);
		}

		public delegate void MouseClick(Vector2Int gridPos);

		public static event MouseClick RightClick;
		public static event MouseClick LeftClick;
		public static event MouseClick RightClickUp;
		public static event MouseClick LeftClickUp;

		private static MouseState lastMouseState;

		public static void MouseDown(object? sender, EventArgs e)
		{
			if (!Game1.instance.IsActive) return;
			if (Desktop.IsMouseOverGUI)
			{
				return; //let myra do it's thing
			}

			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (mouseState.LeftButton == ButtonState.Pressed)
			{
				CurrentUI.MouseDown(gridClick, false);
				
			}

			if (mouseState.RightButton == ButtonState.Pressed)
			{
				CurrentUI.MouseDown(gridClick, true); 
			}

			lastMouseState = mouseState;
		}

		public static void MouseUp(object? sender, EventArgs e)
		{
			if (UI.Desktop.IsMouseOverGUI)
			{
				return; //let myra do it's thing
			}

			var mouseState = Mouse.GetState();
			Vector2Int gridClick = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (lastMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
			{
				 CurrentUI.MouseUp(gridClick, false);

			}

			if (lastMouseState.RightButton == ButtonState.Pressed && mouseState.RightButton == ButtonState.Released)
			{
				CurrentUI.MouseUp(gridClick, true);
			}

		}


		public static Controllable SelectedControllable { get; private set;}

		public static void SelectControllable(Controllable controllable)
		{
			
			if (controllable!= null&&!controllable.IsMyTeam())
			{
				return;
			}

			SelectedControllable = controllable;
			if(controllable==null) return;
			ReMakeMovePreview();
			SetUI( new UnitGameLayout());
			Camera.SetPos(controllable.worldObject.TileLocation.Position);
			
		}
		public static void ReMakeMovePreview()
		{
			previewMoves = SelectedControllable.GetPossibleMoveLocations();
		}
		
	


	
		public static Dialog OptionMessage(string title, string content, string option1text, EventHandler option1,string option2text, EventHandler option2)
		{
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ButtonCancel.Text = option1text;
			messageBox.ButtonCancel.Click += option1;
			messageBox.ButtonOk.Text = option2text;
			messageBox.ButtonOk.Click += option2;
			messageBox.ShowModal(Desktop);
			return messageBox;
		}

		public static void ShowMessage(string title, string content)
		{
			var messageBox = Dialog.CreateMessageBox(title,content);
			messageBox.ShowModal(Desktop);

		}
		private static RenderTarget2D hoverHudRenderTarget;
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
			float opacity = 0.25f;
			bool highlighted = false;

			if (SelectedControllable == controllable || MousePos == (Vector2) controllable.worldObject.TileLocation.Position)
			{
				opacity = 1;
				highlighted = true;
			}

			
			batch.Begin(sortMode: SpriteSortMode.Deferred, samplerState:SamplerState.PointClamp);
			

			int i;
			for (i = 0; i < controllable.Type.MaxFirePoints; i++)
			{
				var indicator = TextureManager.GetTexture("UI/HoverHud/bullet");
				if (i>= controllable.FirePoints)
				{
					indicator= TextureManager.GetTexture("UI/HoverHud/nobullet");
				}

				batch.Draw(indicator,new Vector2(950,0)+new Vector2((50)*i,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);
			}
			
			
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/detbase"), new Vector2(0,50), null,Color.White);

			
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
			
			batch.Draw(TextureManager.GetTexture("UI/HoverHud/base"), Vector2.One, null,Color.White);
		

			var turnIndicator = TextureManager.GetTexture("UI/HoverHud/turnon");
			if(!controllable.canTurn)
				turnIndicator = TextureManager.GetTexture("UI/HoverHud/turnoff");
			batch.Draw(turnIndicator,new Vector2(80,310),Color.White);
			var panicIndicator = TextureManager.GetTexture("UI/HoverHud/panicon");
			if(!controllable.paniced)
				panicIndicator = TextureManager.GetTexture("UI/HoverHud/panicoff");
			batch.Draw(panicIndicator,new Vector2(80,112),Color.White);


			var u = 0;
			if (controllable.Type.MaxMovePoints == 2)
			{
				u = 10;
			}

			batch.End();
			for (i = 0; i < controllable.Type.MaxMovePoints; i++)
			{

				Texture2D indicator;
				if (i>= controllable.MovePoints)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					indicator= TextureManager.GetTexture("UI/HoverHud/actionoff");
				}
				else
				{
					PostPorcessing.ShuffleUICRTeffect(i + controllable.worldObject.Id+10,highlighted);
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
					indicator = TextureManager.GetTexture("UI/HoverHud/actionon");

				}


				batch.Draw(indicator,new Vector2(35,10)+new Vector2((u+35)*i,0),null,Color.White,0,Vector2.Zero,new Vector2(1,1),SpriteEffects.None,0);	
				batch.End();
			}

			float healthWidth = 109f / controllable.Type.MaxHealth;
			float healthXScale = healthWidth/(TextureManager.GetTexture("UI/HoverHud/health").Width);
			int dmgDone = 0; 
			i = 0;
			for (int y = 0; y < controllable.Type.MaxHealth; y++)
			{
				bool health = true;
				
				if (y>= controllable.Health)
				{
					health = false;
				}
				else if (controllable.PreviewData.finalDmg >= controllable.Health-y)
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					batch.Draw(TextureManager.GetTexture("UI/HoverHud/nohealth"),new Vector2(28,42)+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
					batch.End();
					i = y;
					PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id,highlighted,true);
					batch.Begin(sortMode: SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
					batch.Draw(TextureManager.GetTexture("UI/HoverHud/health"),new Vector2(28,42)+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
					//batch.Draw(TextureManager.GetTexture("UI/HoverHud/dmgdone"),new Vector2(184,400)+new Vector2(healthWidth*i,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
					batch.End();
					dmgDone++;
					continue;

				}
				Texture2D indicator;
				if (health)
				{
					PostPorcessing.ShuffleUICRTeffect(y + controllable.worldObject.Id,highlighted);
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, effect: PostPorcessing.UIcrtEffect);
					indicator = TextureManager.GetTexture("UI/HoverHud/health");
				}
				else
				{
					batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);
					indicator= TextureManager.GetTexture("UI/HoverHud/nohealth");
				}

				
				
				batch.Draw(indicator,new Vector2(28,42)+new Vector2(healthWidth*y,0),null,Color.White,0,Vector2.Zero,new Vector2(healthXScale,1),SpriteEffects.None,0);
				batch.End();
			}
			batch.Begin(sortMode: SpriteSortMode.Deferred,  BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead);

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
		
			
			
			
			
			batch.End();
			graphicsDevice.SetRenderTarget(Game1.GlobalRenderTarget);
			batch.Begin(transformMatrix: Camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			batch.Draw(hoverHudRenderTarget,Utility.GridToWorldPos((Vector2)controllable.worldObject.TileLocation.Position)+new Vector2(-200,-200),null,Color.White*opacity,0,Vector2.Zero,2.5f,SpriteEffects.None,0);
			batch.End();


		}

		public static void Update(float deltatime)
		{
			CurrentUI.Update(deltatime);
		}

		private static bool raycastDebug;
		private static List<Vector2Int>[] previewMoves = new List<Vector2Int>[2];

		public static void Render(float deltaTime)
		{
			
			var TileCoordinate = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			//TileCoordinate = new Vector2(34, 33);
			TileCoordinate = Vector2.Clamp(TileCoordinate, Vector2.Zero, new Vector2(99, 99));
			var Mousepos = Utility.GridToWorldPos((Vector2)TileCoordinate+new Vector2(-1.5f,-0.5f));


			spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Deferred, samplerState: SamplerState.PointClamp);
			
			for (int i = 0; i < 8; i++)
			{
				var indicator = coverIndicator[i];
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
				spriteBatch.Draw(indicator, Mousepos, c);
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

					spriteBatch.DrawRectangle(pos, new Size2(20, 20), c, 5);


				}

				count++;
			}
			



/* griddebug
			spriteBatch.Begin(transformMatrix: Camera.Cam.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
			for (int x = 0; x < 10; x++)
			{
				for (int y = 0; y < 10; y++)
				{
				
					spriteBatch.DrawCircle(Utility.GridToWorldPos(new Vector2(x,y)), 5, 10, Color.Black, 5f);
				}
			}
			spriteBatch.End();
			
			*/
			var tile = WorldManager.Instance.GetTileAtGrid(TileCoordinate);
			if (SelectedControllable != null && Action.GetActiveActionType() != null)
			{
				Action.ActiveAction.Preview(SelectedControllable, TileCoordinate,spriteBatch);
			}

			
			/*else if (tile.ObjectAtLocation?.ControllableComponent != null && tile.ObjectAtLocation.IsVisible() && !tile.ObjectAtLocation.ControllableComponent.IsMyTeam())
			{
				if (tile.ObjectAtLocation.ControllableComponent != enemySelected)
				{
					EnemyVisiblityCache = WorldManager.Instance.GetVisibleTiles(tile.ObjectAtLocation.TileLocation.Position, tile.ObjectAtLocation.Facing,tile.ObjectAtLocation.ControllableComponent.GetSightRange(),tile.ObjectAtLocation.ControllableComponent.Crouching );
					enemySelected = tile.ObjectAtLocation.ControllableComponent;
				}

				foreach (var unit in GameManager.MyUnits)
				{
					if (unit.Health <= 0) continue;
					if (Camera.IsOnScreen(unit.worldObject.TileLocation.Position))
					{
						Action.Actions[ActionType.Attack].Preview(tile.ObjectAtLocation.ControllableComponent,unit.worldObject.TileLocation.Position,spriteBatch);
						if(EnemyVisiblityCache.ContainsKey(unit.worldObject.TileLocation.Position) && EnemyVisiblityCache[unit.worldObject.TileLocation.Position] >= unit.worldObject.GetMinimumVisibility())
						{
							spriteBatch.Draw(vissionIndicator[0],Utility.GridToWorldPos(unit.worldObject.TileLocation.Position),  Color.White);
						}
						else
						{
							spriteBatch.Draw(vissionIndicator[1],Utility.GridToWorldPos(unit.worldObject.TileLocation.Position),  Color.White);
						}
					}

				}
			}*/

			CurrentUI.Render(spriteBatch,deltaTime);

			spriteBatch.End();


			int targetIndex = Controllables.IndexOf(SelectedControllable);
			if (targetIndex != -1)
			{
				for (int i = targetIndex; i < Controllables.Count - 1; i++)
				{
					Controllables[i] = Controllables[i + 1];
					Controllables[i + 1] = SelectedControllable;
				}
			}

			if (tile.ObjectAtLocation != null)
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

			foreach (var obj in Controllables)
			{
				if (obj.worldObject.IsVisible())
				{
					DrawControllableHoverHud(spriteBatch, obj,deltaTime);
				}
			}

			Desktop.Root = root;
			if (Game1.instance.IsActive)
			{
				Desktop.Render();
			}
			else
			{
				Desktop.RenderVisual();
			}


		}

		
	
	}
}
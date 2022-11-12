using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using Network;

namespace MultiplayerXeno
{
	public partial class WorldManager
	{
		
		private static SpriteBatch spriteBatch;
		private static GraphicsDevice graphicsDevice;
		public void Init(GraphicsDevice graphicsdevice)
		{

			graphicsDevice = graphicsdevice;
			spriteBatch = new SpriteBatch(graphicsDevice);
			UI.LeftClick += LeftClickAtPosition;
			UI.RightClick += RightClickAtPosition;
		
		}
		private void LeftClickAtPosition(Vector2Int position)
		{
			ClickAtPosition(position,false);
		}
		private void RightClickAtPosition(Vector2Int position)
		{
			ClickAtPosition(position,true);
		}

		private void ClickAtPosition(Vector2Int position,bool righclick)
		{
			var Tile = GetTileAtGrid(position);

			WorldObject obj = Tile.ObjectAtLocation;
			if (obj!=null&&obj.ControllableComponent != null && !Controllable.Targeting) { 
				obj.ControllableComponent.Select(); 
				return;
			}
			
			
			//if nothing was selected then it's a click on an empty tile
			
			Controllable.StartOrder(position,righclick);

		}



		public List<RayCastOutcome> RecentFOVRaycasts = new List<RayCastOutcome>();
		public void CalculateFov()
		{
			if (!GameManager.intated)
			{
				return;//dont update fov untll atleast 1 server update has been recived
			}

			RecentFOVRaycasts = new List<RayCastOutcome>();
			foreach (var tile in _gridData)
			{
				tile.IsVisible = false;
			}


	

			foreach (var obj in WorldObjects.Values)
			{
			
				if (obj.ControllableComponent is not null && obj.ControllableComponent.IsMyTeam())
				{
					Vector2Int pos = obj.TileLocation.Position;

					int itteration = 0;

					List<Vector2Int> positionsToCheck = new List<Vector2Int>();
					while (itteration < obj.ControllableComponent.GetSightRange())
					{
						
						positionsToCheck.Add(pos);
						Vector2Int offset;
						Vector2Int invoffset;
						if (Utility.DirToVec2(obj.Facing).Magnitude() > 1)//diagonal
						{
							offset = Utility.DirToVec2(obj.Facing+3);
							invoffset = Utility.DirToVec2(obj.Facing-3);
						}
						else
						{
							offset = Utility.DirToVec2(obj.Facing+2);
							invoffset = Utility.DirToVec2(obj.Facing-2);
						}

					
						for (int x = 0; x < itteration; x++)
						{
							positionsToCheck.Add(pos + invoffset*(x+1));
							positionsToCheck.Add(pos + offset*(x+1));
						}

						pos += Utility.DirToVec2(obj.Facing);
						itteration++;
					}
		
					

					foreach (var tile in positionsToCheck)
					{
						if(!IsPositionValid(tile)) continue;
						RayCastOutcome cast = Raycast(obj.TileLocation.Position, tile,Cover.Full,true);
						RecentFOVRaycasts.Add(cast);
						if (cast.hit) continue;
						GetTileAtGrid(tile).IsVisible = true;
					}
				}
			}
		}

		


		public void Draw(GameTime gameTime)
		{
			List<WorldTile> allTiles = new List<WorldTile>();

			foreach (var tile  in _gridData)
			{
					allTiles.Add(tile);
			}

			allTiles.Sort(new WorldTileDrawOrderCompare());
				
				spriteBatch.Begin(transformMatrix: Camera.GetViewMatrix(),sortMode: SpriteSortMode.Immediate);
				List<WorldObject> tileObjs = new List<WorldObject>();
				foreach (var tile in allTiles)
				{
					tileObjs.Clear();
					tileObjs.Add(tile.Surface);
					tileObjs.Add(tile.NorthEdge);
					tileObjs.Add(tile.WestEdge);
					tileObjs.Add(tile.ObjectAtLocation);
					//tileObjs.Sort(new DrawLayerSort());


					foreach (var worldObject in tileObjs)
					{
						if(worldObject == null)continue;
							var sprite = worldObject.GetSprite();
						var transform = worldObject.Type.Transform;
						Color c = Color.White;
						if (!worldObject.TileLocation.IsVisible)
						{
							c = Color.DarkGray;
							if (worldObject.TileLocation.ObjectAtLocation == worldObject)
							{
								continue;
							}
						}


						sprite.Color = c;

						spriteBatch.Draw(sprite, transform.Position + Utility.GridToWorldPos(worldObject.TileLocation.Position), transform.Rotation, transform.Scale);
						if (worldObject.ControllableComponent != null)
						{
							UI.DrawControllableHoverHud(spriteBatch, worldObject);
						}
					}
				}
				spriteBatch.End();


			
			

			
		}
		public static Vector2 RadianToVector2(float radian)
		{
			return new Vector2((float) Math.Cos(radian), (float) Math.Sin(radian));
		}
		public static Vector2 RadianToVector2(float radian, float length)
		{
			return RadianToVector2(radian) * length;
		}
		public static Vector2 DegreeToVector2(float degree)
		{
			return RadianToVector2(degree * (MathF.PI/180));
		}
		public static Vector2 DegreeToVector2(float degree, float length)
		{
			return RadianToVector2(degree * (MathF.PI/180)) * length;
		}
		
			
		
		public class WorldTileDrawOrderCompare : Comparer<WorldTile>
		{
//draws "top" ones first

			public override int Compare(WorldTile x, WorldTile y)
			{

				int xpos = x.Position.X + x.Position.Y;
				int ypos = y.Position.X + y.Position.Y;
				return xpos.CompareTo(ypos);
			}
		}
		public class DrawLayerSort : Comparer<WorldObject>
		{
//draws "top" ones first

			public override int Compare(WorldObject x, WorldObject y)
			{

				int xpos = x.GetDrawLayer();
				int ypos = y.GetDrawLayer();
				return xpos.CompareTo(ypos);
			}
		}
	}
}
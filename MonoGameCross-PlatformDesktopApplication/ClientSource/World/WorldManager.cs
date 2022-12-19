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

		public void Init()
		{
		}



		public List<RayCastOutcome> RecentFOVRaycasts = new List<RayCastOutcome>();

		private bool fovDirty = false;
		public void MakeFovDirty()
		{
			fovDirty = true;
		}

		
		private void CalculateFov()
		{
			fovDirty = false;
				if (!GameManager.intated)
				{
					return; //dont update fov untll atleast 1 server update has been recived
				}

				RecentFOVRaycasts = new List<RayCastOutcome>();
				foreach (var tile in _gridData)
				{
					tile.Visible = Visibility.None;
				}


				foreach (var obj in _worldObjects.Values)
				{

					if (obj.ControllableComponent is not null && obj.ControllableComponent.IsMyTeam())
					{
						Vector2Int pos = obj.TileLocation.Position;

						int itteration = 0;

						List<Vector2Int> positionsToCheck = new List<Vector2Int>();
						while (itteration < obj.ControllableComponent.GetSightRange()+2)
						{

							positionsToCheck.Add(pos);
							Vector2Int offset;
							Vector2Int invoffset;
							if (Utility.DirToVec2(obj.Facing).Magnitude() > 1) //diagonal
							{
								offset = Utility.DirToVec2(obj.Facing + 3);
								invoffset = Utility.DirToVec2(obj.Facing - 3);
							}
							else
							{
								offset = Utility.DirToVec2(obj.Facing + 2);
								invoffset = Utility.DirToVec2(obj.Facing - 2);
							}


							for (int x = 0; x < itteration; x++)
							{
								positionsToCheck.Add(pos + invoffset * (x + 1));
								positionsToCheck.Add(pos + offset * (x + 1));
							}

							pos += Utility.DirToVec2(obj.Facing);
							itteration++;
						}


					

						foreach (var tile in positionsToCheck)
						{
							if (!IsPositionValid(tile)) continue;
							var visibility = CanSee(obj.ControllableComponent, tile);
							if(GetTileAtGrid(tile).Visible < visibility)
							{
								GetTileAtGrid(tile).Visible = visibility;
							}
							
							
							
						}
					}
				}
			
		}



	}
}
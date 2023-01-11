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


				foreach (var obj in WorldObjects.Values)
				{

					if (obj.ControllableComponent is not null && obj.ControllableComponent.IsMyTeam())
					{
						foreach (var visTuple in GetVisibleTiles(obj.TileLocation.Position,obj.Facing,obj.ControllableComponent.GetSightRange(),obj.ControllableComponent.Crouching))
						{
							
					
							if(GetTileAtGrid(visTuple.Item1).Visible < visTuple.Item2)
							{
								GetTileAtGrid(visTuple.Item1).Visible = visTuple.Item2;
								GetTileAtGrid(visTuple.Item1)?.ObjectAtLocation?.ControllableComponent.Spoted();
							}
							
							
							
						}
					}
				}
			
		}
		
		public List<Tuple<Vector2Int,Visibility>> GetVisibleTiles(Vector2Int pos, Direction dir, int range,bool crouched)
		{

			int itteration = 0;

			List<Tuple<Vector2Int,Visibility>> positionsToCheck = new List<Tuple<Vector2Int, Visibility>>();
			Vector2Int initialpos = pos;
			while (itteration < range+2)
			{
				
				
				if (IsPositionValid(pos))
				{
					var visibility = CanSee(initialpos,pos,range,crouched);
					if (visibility > Visibility.None)
					{
						positionsToCheck.Add(new Tuple<Vector2Int,Visibility>(pos,visibility));
					}
				}
				
				Vector2Int offset;
				Vector2Int invoffset;
				if (Utility.DirToVec2(dir).Magnitude() > 1) //diagonal
				{
					offset = Utility.DirToVec2(dir + 3);
					invoffset = Utility.DirToVec2(dir - 3);
				}
				else
				{
					offset = Utility.DirToVec2(dir+ 2);
					invoffset = Utility.DirToVec2(dir - 2);
				}


				for (int x = 0; x < itteration; x++)
				{
					
					if (IsPositionValid(pos + invoffset * (x + 1)))
					{
						var visibility = CanSee(initialpos,pos + invoffset * (x + 1),range,crouched);
						if (visibility > Visibility.None)
						{
							positionsToCheck.Add(new Tuple<Vector2Int,Visibility>(pos + invoffset * (x + 1),visibility));
						}
					}

					if (IsPositionValid(pos + offset * (x + 1)))
					{
						var visibility = CanSee(initialpos,pos + offset * (x + 1),range,crouched);
						if (visibility > Visibility.None)
						{
							positionsToCheck.Add(new Tuple<Vector2Int,Visibility>(pos + offset * (x + 1),visibility));
						}
					}
				}

				pos += Utility.DirToVec2(dir);
				itteration++;
			}


			return positionsToCheck;
		}



	}
}
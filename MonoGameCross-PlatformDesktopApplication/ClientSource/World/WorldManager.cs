using System;
using System.Collections.Generic;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;

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

		private bool fovDirty;
		private bool FullVis;
		public void MakeFovDirty(bool fullvis = false)
		{
			fovDirty = true;
			FullVis = fullvis;
		}

		
		private void CalculateFov()
		{
			fovDirty = false;
	

				
				if (FullVis)
				{
					foreach (var tile in _gridData)
					{
						tile.Visible = Visibility.Full;
					}
					return;
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
							
					
							if(GetTileAtGrid(visTuple.Key).Visible < visTuple.Value)
							{
								GetTileAtGrid(visTuple.Key).Visible = visTuple.Value;
								GetTileAtGrid(visTuple.Key)?.ControllableAtLocation?.ControllableComponent.Spoted();
							}
							
							
							
						}
					}
				}
			
		}
		
		public Dictionary<Vector2Int,Visibility> GetVisibleTiles(Vector2Int pos, Direction dir, int range,bool crouched)
		{

			int itteration = 0;

			Dictionary<Vector2Int, Visibility> positionsToCheck = new Dictionary<Vector2Int, Visibility>();
			Vector2Int initialpos = pos;
			while (itteration < range+2)
			{
				
				
				if (IsPositionValid(pos))
				{
					var visibility = CanSee(initialpos,pos,range,crouched);
					if (visibility > Visibility.None)
					{
						positionsToCheck.Add(pos, visibility);
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
							positionsToCheck.Add(pos + invoffset * (x + 1),visibility);
						}
					}

					if (IsPositionValid(pos + offset * (x + 1)))
					{
						var visibility = CanSee(initialpos,pos + offset * (x + 1),range,crouched);
						if (visibility > Visibility.None)
						{
							positionsToCheck.Add(pos + offset * (x + 1),visibility);
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
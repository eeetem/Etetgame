using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MultiplayerXeno;

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

		Parallel.ForEach(WorldObjects.Values, obj =>
		{
			if (obj.UnitComponent is not null && obj.UnitComponent.IsMyTeam())
			{
				foreach (var visTuple in GetVisibleTiles(obj.TileLocation.Position,obj.Facing,obj.UnitComponent.GetSightRange(),obj.UnitComponent.Crouching))
				{
					if(GetTileAtGrid(visTuple.Key).Visible < visTuple.Value)
					{
						GetTileAtGrid(visTuple.Key).Visible = visTuple.Value;
						GetTileAtGrid(visTuple.Key)?.UnitAtLocation?.Spoted();
					}

				}
			}
		});

	}
		
	public ConcurrentDictionary<Vector2Int,Visibility> GetVisibleTiles(Vector2Int pos, Direction dir, int range,bool crouched)
	{

		int itteration = 0;

		List<Vector2Int> positionsToCheck = new List<Vector2Int>();
		ConcurrentDictionary<Vector2Int, Visibility> resullt = new ConcurrentDictionary<Vector2Int, Visibility>();
		Vector2Int initialpos = pos;
		while (itteration < range+2)
		{
			if (IsPositionValid(pos))
			{
				positionsToCheck.Add(pos);
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

					positionsToCheck.Add(pos + invoffset * (x + 1));
					
				}

				if (IsPositionValid(pos + offset * (x + 1)))
				{

					positionsToCheck.Add(pos + offset * (x + 1));
					
				}
			}

			pos += Utility.DirToVec2(dir);
			itteration++;
		}

		Parallel.ForEach(positionsToCheck, position =>
		{
			var vis = CanSee(initialpos, position, range, crouched);
			if(vis != Visibility.None)
				resullt.AddOrUpdate(position,vis,(key,old) => vis);
		});
		
		return resullt;
	}



}
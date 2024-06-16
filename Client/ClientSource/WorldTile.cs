using System;
using System.Collections.Generic;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace DefconNull;

public partial class WorldTile : IWorldTile
{
	private bool _enemyWatching;
	private bool _friendlyWatching;


	public Color GetTileColor()
	{
		Color color = Color.White;
			
		if (TileVisibility == Visibility.None)
		{
			color = Color.DimGray;
					
		}else if (TileVisibility == Visibility.Partial)
		{
			color = Color.MediumPurple;
		}


		if (_watchers.Count > 0)
		{
			if (_friendlyWatching)
			{
				var vec = color.ToVector3();
				color = new Color(vec.X/2f,vec.Y, vec.Z/2f, 1);
			}
			if (_enemyWatching)
			{
				var vec = color.ToVector3();
				color = new Color(vec.X*1.5f,vec.Y/2f, vec.Z*1.2f, 1);
			}
			
		}
		


		return color;
	}
	public void CalcWatchLevel()
	{
		if(Surface == null) return;
		_friendlyWatching = false;
		_enemyWatching = false;
		foreach (var id in new List<int>(_watchers))
		{
			var watcher = WorldObjectManager.GetObject(id)?.UnitComponent;
			if (watcher == null || !watcher!.Overwatch.Item1)
			{
				continue;
			}

			var res = watcher.Abilities[watcher.Overwatch.Item2].IsPlausibleToPerform(watcher, Surface!, -1);
			if (res.Item1 && watcher.VisibleTiles.ContainsKey(Position))
			{

				if (watcher.IsMyTeam())
				{
					_friendlyWatching = true;
				}
				else
				{
					_enemyWatching = true;
				} 
			}
			else
			{
				Console.WriteLine("overwatch canceled because: "+res.Item2);
			}

		}
	}

	public Rectangle GetDrawBounds()
	{
		var corner = Utility.GridToWorldPos(Position);
		return new Rectangle((int)corner.X, (int)corner.Y, 64, 64);
	}
}
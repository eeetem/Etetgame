using System;
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
		foreach (var watcher in _watchers)
		{
			if (!watcher.WorldObject.IsVisible())
			{
				continue;
			}

			if (!watcher.Overwatch.Item1)
			{
				Console.WriteLine("Watcher not overwatching");	
				continue;
			}

			var res = watcher.Abilities[watcher.Overwatch.Item2].CanPerform(watcher, Surface!, false, true);
			if (res.Item1)
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
    
}
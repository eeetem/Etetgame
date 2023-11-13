using System;
using DefconNull.World.WorldObjects;
using Color = Microsoft.Xna.Framework.Color;

namespace DefconNull.World;

public partial class WorldTile : IWorldTile
{
	private bool enemyWatching;
	private bool friendlyWatching;
	public Color GetTileColor()
	{
		Color color = Color.White;
			
		if (TileVisibility == Visibility.None)
		{
			color = Color.DimGray;
					
		}else if (TileVisibility == Visibility.Partial)
		{
			color = Color.LightPink;
		}


		if (Watchers.Count > 0)
		{
	
			if (friendlyWatching)
			{
				var vec = color.ToVector3();
				color = new Color(vec.X/2f,vec.Y, vec.Z/2f, 1);
			}
			if (enemyWatching)
			{
				var vec = color.ToVector3();
				color = new Color(vec.X*1.2f,vec.Y/2f, vec.Z*1.2f, 1);
			}
			

		}






		return color;
	}
	public void CalcWatchLevel()
	{
		friendlyWatching = false;
		enemyWatching = false;
		foreach (var watcher in Watchers)
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

			var res = watcher.Abilities[watcher.Overwatch.Item2].CanPerform(watcher, Position, false, true);
			if (res.Item1)
			{
				if (watcher.IsMyTeam())
				{
					friendlyWatching = true;
				}
				else
				{
					enemyWatching = true;
				} 
			}
			else
			{
				Console.WriteLine("overwatch canceled because: "+res.Item2);
			}

		}
	}
    
}
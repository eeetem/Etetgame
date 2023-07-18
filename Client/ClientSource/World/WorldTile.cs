﻿using DefconNull.World.WorldObjects;
using Color = Microsoft.Xna.Framework.Color;

namespace DefconNull.World;

public partial class WorldTile
{

	public Visibility TileVisibility;

	public bool IsVisible(Visibility minimum = Visibility.Partial)
	{
		if (TileVisibility >= minimum)
		{
			return true;
		}

		return false;
	}

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


		if (HighestWatchLevel == 2)
		{
			var vec = color.ToVector3();
			return new Color(vec.X/1.5f,vec.Y+0.2f, vec.Z/1.5f, 1);
		}
		if (HighestWatchLevel == 1)
		{
			var vec = color.ToVector3();
			return new Color(vec.X+0.1f,vec.Y, vec.Z/1.5f, 1);
		}






		return color;
	}
	public void CalcWatchLevel()
	{
		HighestWatchLevel = 0;
		foreach (var watcher in Watchers)
		{
			if (!this.IsVisible())
			{
				return;
			}

			HighestWatchLevel = 0;
			if (watcher.CanHit(Position,true))
			{
				HighestWatchLevel = 2;
				return;
			}

			if (watcher.CanHit(Position,false))
			{
				HighestWatchLevel = 1;
				return;
			}

		}
	}


	
}
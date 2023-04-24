using System.Numerics;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;

namespace MultiplayerXeno
{
	public partial class WorldTile
	{

		public Visibility Visible;

		public bool IsVisible(Visibility minimum = Visibility.Partial)
		{
			if (this.Visible >= minimum)
			{
				return true;
			}

			return false;
		}

		public Color GetTileColor()
		{
			Color color = Color.White;
			
			if (Visible == Visibility.None)
			{
				color = Color.DimGray;
					
			}else if (Visible == Visibility.Partial)
			{
				color = Color.LightPink;
			}


			if (HighestWatchLevel == 2)
			{
				var vec = color.ToVector3();
				return new Color(vec.X/1.5f,vec.Y+0.2f, vec.Z/1.5f, 1);
			}else if (HighestWatchLevel == 1)
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
				if (!watcher.worldObject.IsVisible())
				{
					return;
				}

				HighestWatchLevel = 1;
				if (watcher.CanHit(Position,true))
				{
					HighestWatchLevel = 2;
					return;
				}
				
			}
		}


	
	}


}
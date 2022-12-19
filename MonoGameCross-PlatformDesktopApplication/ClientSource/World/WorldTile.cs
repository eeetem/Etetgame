using System;
using System.Drawing;
using CommonData;
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

			
			foreach (var watcher in Watchers)
			{
				if (watcher.worldObject.IsVisible())
				{
					var vec = color.ToVector3();
					return new Color(vec.X+0.1f,vec.Y, vec.Z/1.5f, 1);
				}
			}
			
			return color;
		}
	}
}
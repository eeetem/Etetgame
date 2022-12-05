using System;
using CommonData;

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
	}
}
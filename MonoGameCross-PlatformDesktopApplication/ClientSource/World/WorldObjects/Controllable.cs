using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Pathfinding;

namespace MultiplayerXeno
{
	public partial class Controllable
	{

		public bool IsMyTeam()
		{
			return GameManager.IsPlayer1 == this.IsPlayerOneTeam;
		}
		public void Spoted()
		{
			foreach (var tile in overWatchedTiles)
			{
				WorldManager.Instance.GetTileAtGrid(tile).CalcWatchLevel();
			}
			
		}


	}
}
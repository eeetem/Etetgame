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

	

	
		public void DoAction(Action a,Vector2Int target)
		{
			
			
			if (!a.CanPerform(this, target))
			{
				return;
			}

			a.ToPacket(this, target);

	
	
		}

	
	}
}
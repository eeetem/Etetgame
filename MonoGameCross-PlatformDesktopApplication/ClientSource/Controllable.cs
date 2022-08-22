using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Pathfinding;

namespace MultiplayerXeno
{
	public partial class Controllable
	{
		public static Controllable Selected { get; set; }

		public bool IsMyTeam()
		{
			return GameManager.IsPlayer1 == this.IsPlayerOneTeam;
		}

		public void Select()
		{

			if (IsMyTeam())
			{
				//ui.fullUI
				
			}
			else
			{
				//ui.infoui
				
			}

			Selected = this;


		}

		public static void StartOrder(Vector2Int Position)
		{
			if (GameManager.IsPlayer1 != GameManager.IsPlayer1Turn) return;

			if(Selected == null) return;
			
			if (Selected.IsPlayerOneTeam != GameManager.IsPlayer1) return;

			Selected.MoveAction(Position);


		}

	
	}
}
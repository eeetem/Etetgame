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
				UI.FullUnitUI(this.worldObject);
				
			}
			else
			{
				//ui.infoui
				
			}

			Selected = this;


		}

		public static bool Targeting { get; private set; } = false;

		public static void ToggleTarget()
		{
			Targeting = !Targeting;
		}

		public static void StartOrder(Vector2Int Position, bool rightclicked)
		{
			if (!GameManager.IsMyTurn()) return;

			if(Selected == null) return;
			
			if (Selected.IsPlayerOneTeam != GameManager.IsPlayer1) return;

			if (Targeting)
			{
				if (rightclicked)
				{
					Targeting = false;
				}
				else
				{
					Selected.FireAction(Position);
				}
			}
			else
			{
				if (rightclicked)
				{
					Selected.FaceAction(Position);
				}
				else
				{
					Selected.MoveAction(Position);
				}
			}
			


		}

	
	}
}
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

				UI.UnitUI(this.worldObject);
				
		

			Selected = this;


		}

		public static bool Targeting { get; set; } = false;

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
					Selected.DoAction(Action.Actions[ActionType.Attack],Position);
				}
			}
			else
			{
				if (rightclicked)
				{
					Selected.DoAction(Action.Actions[ActionType.Face],Position);
				}
				else
				{
					if (UI.showPath)
					{
						UI.showPath = false;
						Selected.DoAction(Action.Actions[ActionType.Move],Position);
					}
					else
					{
						UI.showPath = true;
					}
				}
			}
			


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
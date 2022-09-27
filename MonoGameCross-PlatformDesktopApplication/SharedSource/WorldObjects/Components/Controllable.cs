using System.Collections.Generic;
using System.Linq;
using CommonData;
using Microsoft.Xna.Framework;
using MultiplayerXeno.Pathfinding;

#nullable enable


namespace MultiplayerXeno
{
	public partial class Controllable
	{
		public WorldObject Parent { get; private set; }
		private ControllableType Type;
		public Controllable(bool isPlayerOneTeam, WorldObject parent, ControllableType type)
		{
			Parent = parent;
			Type = type;
			IsPlayerOneTeam = isPlayerOneTeam;
#if CLIENT
			WorldManager.CalculateFov();
#endif
			
		}

		public bool IsPlayerOneTeam { get; private set;}

		private bool hasMoved = false;

		public int GetSightRange()
		{
			//apply effects and offests
			return Type.sightRange;
		}

		public void StartTurn()
		{
			hasMoved = false;

		}


		public void MoveAction(Vector2Int position)
		{
			if(hasMoved) return;
			
			List<Vector2Int> path = PathFinding.GetPath(this.Parent.TileLocation.Position, position);
			if (path.Count == 0)
			{
				#if CLIENT
				Selected = null;
				#endif
				return;//no path
			}

			if (path.Count > this.Type.moveRange)
			{
#if CLIENT
				Selected = null;
#endif
				return; // too far
			}

			
			//todo move animation
			
			var packet = new MovementPacket(Parent.Id,path);

			Networking.DoAction(packet);
			//we only move as server - as client we send the action, let server verify it and then the server will force us to do the action
			#if SERVER
			Move(path);
			#endif


		}

		private List<Vector2Int> CurrentPath;
		private bool moving;
		private float MoveCounter;
		public void Move(List<Vector2Int> path)
		{
			if(hasMoved) return;
			hasMoved = true;
			moving = true;
			CurrentPath = path;
		}

		public void EndTurn()
		{
			
			
		}
		public void Update(float gameTime)
		{
			if (moving)
			{
				MoveCounter += gameTime;
				if (MoveCounter > 500)
				{
					MoveCounter = 0;

					Parent.Face(WorldManager.Vec2ToDir(CurrentPath[0] - Parent.TileLocation.Position ));
					Parent.Move(CurrentPath[0]);
					CurrentPath.RemoveAt(0);
					if (CurrentPath.Count == 0)
					{
						moving = false;
					}
#if CLIENT
					WorldManager.CalculateFov();
#endif
				}
			}

		}
	}
}
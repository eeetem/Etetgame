using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Riptide;

namespace MultiplayerXeno.ReplaySequence;

public class Move : SequenceAction
{
	private List<Vector2Int> Path;
	public Move(int actorID,List<Vector2Int> path) : base(actorID,SequenceType.Move)
	{
		Path = path;
	}
	public Move(int actorID,Message args) : base(actorID,SequenceType.Move)
	{
		Path = args.GetSerializables<Vector2Int>().ToList();
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			while (Path.Count >0)
			{
				
				try
				{
					Actor.WorldObject.Face(Utility.Vec2ToDir(Path[0] - Actor.WorldObject.TileLocation.Position));
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception when facing, the values are: "+Path[0]+" and " + Actor.WorldObject.TileLocation.Position + " exception: "+e);
				}
				
				Thread.Sleep((int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*250));
				Actor.WorldObject.Move(Path[0]);
				Path.RemoveAt(0);



				
#if CLIENT
					WorldManager.Instance.MakeFovDirty();
					if (Actor.WorldObject.IsVisible())
					{
						Audio.PlaySound("footstep", Utility.GridToWorldPos(Actor.WorldObject.TileLocation.Position));
					}
#endif
			}
#if SERVER
			

			//not sure if this needs to be kept
			foreach (var u in GameManager.T1Units)
			{
				WorldManager.Instance.GetObject(u).UnitComponent.overwatchShotThisMove = false;
			}
			foreach (var u in GameManager.T2Units)
			{
				WorldManager.Instance.GetObject(u).UnitComponent.overwatchShotThisMove = false;
			}
#endif
		
			Actor.canTurn = true;
		});

		return t;
	}
	

	protected override void SerializeArgs(Message message)
	{
		message.AddSerializables(Path.ToArray());
	}


}
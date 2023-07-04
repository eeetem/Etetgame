using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
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

	public override bool ShouldDo()
	{
		return !Actor.Paniced;
	}

	protected override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			
			while (Path.Count >0)
			{
				Console.Write("Current Path:");
				foreach (var p in Path)
				{
					Console.Write(p + " ");
				}
				Console.WriteLine("donepath");
				var frametask1 = new Task(delegate
				{
				//	Console.WriteLine("running move task1: "+Path.Count);
					try
					{
						if(Path[0] != Actor.WorldObject.TileLocation.Position)
							Actor.WorldObject.Face(Utility.Vec2ToDir(Path[0] - Actor.WorldObject.TileLocation.Position));
					}
					catch (Exception e)
					{
						Console.WriteLine("Exception when facing, the values are: " + Path[0] + " and " + Actor.WorldObject.TileLocation.Position + " exception: " + e);
					}
				});
				WorldManager.Instance.RunNextFrame(frametask1);
				Thread.Sleep((int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*200));
				var frametask2 = new Task(delegate
				{
					//Console.WriteLine("running move task2: "+Path.Count);
					Actor.WorldObject.Move(Path[0]);
					Path.RemoveAt(0);

#if CLIENT
					WorldManager.Instance.MakeFovDirty();
					if (Actor.WorldObject.IsVisible())
					{
						Audio.PlaySound("footstep", Utility.GridToWorldPos(Actor.WorldObject.TileLocation.Position));
					}
#endif
					
				});
				WorldManager.Instance.RunNextFrame(frametask2);

				while (frametask2.Status != TaskStatus.RanToCompletion)
				{
					Thread.Sleep(10);
				}
			
			}

			Actor.canTurn = true;
		});

		return t;
	}
	

	protected override void SerializeArgs(Message message)
	{
		message.AddSerializables(Path.ToArray());
	}


}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
#endif
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class UnitMove : UnitSequenceAction
{
	private List<Vector2Int> Path;
	public UnitMove(int actorID,List<Vector2Int> path) : base(new TargetingRequirements(actorID),SequenceType.Move)
	{
		Path = path;
	}
	public UnitMove(TargetingRequirements actorID, Message args) : base(actorID,SequenceType.Move)
	{
		Path = args.GetSerializables<Vector2Int>().ToList();
	}

	public override bool ShouldDo()
	{
		return !Actor.Paniced;
	}

	public override Task GenerateTask()
	{
#if CLIENT
		//	debugPaths.Add(new Tuple<Color, List<Vector2Int>>(new Color(Random.Shared.NextSingle(),Random.Shared.NextSingle(),Random.Shared.NextSingle()), new List<Vector2Int>(Path)));
#endif
		var t = new Task(delegate
		{
			//	int tick = 0;
			
			while (Path.Count >0)
			{
				//Console.WriteLine("movement task is running: "+tick);
				//tick++;

				var frametask1 = new Task(delegate
				{

					if(Path[0] != Actor.WorldObject.TileLocation.Position)
						Actor.WorldObject.Face(Utility.Vec2ToDir(Path[0] - Actor.WorldObject.TileLocation.Position));

				});
				WorldManager.Instance.RunNextAfterFrames(frametask1);
				Thread.Sleep((int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*200));
				var frametask2 = new Task(delegate
				{
					//	Console.WriteLine("moving to: "+Path[0]+" path size left: "+Path.Count);
					Actor.WorldObject.Move(Path[0]);
					Path.RemoveAt(0);


					WorldManager.Instance.MakeFovDirty();

#if CLIENT
					if (Actor.WorldObject.IsVisible())
					{
						Audio.PlaySound("footstep", Utility.GridToWorldPos(Actor.WorldObject.TileLocation.Position));
					}
#endif
					
				});
				//	Console.WriteLine("queued movement task to: "+Path[0]+" path size left: "+Path.Count);
				WorldManager.Instance.RunNextAfterFrames(frametask2);

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
		base.SerializeArgs(message);
		message.AddSerializables(Path.ToArray());
	}

#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}
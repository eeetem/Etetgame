using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
#endif

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class UnitMove : UnitSequenceAction
{
	private List<Vector2Int> Path;

	public static UnitMove Make(int actorID,List<Vector2Int> path)
	{
		UnitMove t = (GetAction(SequenceType.Move) as UnitMove)!;
		t.Path = path;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}

	public override SequenceType GetSequenceType()
	{
		return SequenceType.Move;
	}

	public override bool ShouldDo()
	{
		return Actor != null && !Actor.Paniced;
	}

	protected override void RunSequenceAction()
	{
#if CLIENT
		//	debugPaths.Add(new Tuple<Color, List<Vector2Int>>(new Color(Random.Shared.NextSingle(),Random.Shared.NextSingle(),Random.Shared.NextSingle()), new List<Vector2Int>(Path)));
#endif
		
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
				#if CLIENT
				Thread.Sleep((int) (WorldManager.Instance.GetTileAtGrid(Path[0]).TraverseCostFrom(Actor.WorldObject.TileLocation.Position)*200));
			#else
					Thread.Sleep(10);

			#endif
				var frametask2 = new Task(delegate
				{
					//	Console.WriteLine("moving to: "+Path[0]+" path size left: "+Path.Count);
					
					Actor.WorldObject.TileLocation.UnitAtLocation = null;
					var newTile = WorldManager.Instance.GetTileAtGrid(Path[0]);
					Actor.WorldObject.TileLocation = newTile;
					newTile.UnitAtLocation = Actor;
			
#if CLIENT
		Actor.WorldObject.GenerateDrawOrder();
#endif
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


	}
	

	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.AddSerializables(Path.ToArray());
	}

	protected override void DeserializeArgs(Message message)
	{
		base.DeserializeArgs(message);
		Path = message.GetSerializables<Vector2Int>().ToList();
	}

#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}
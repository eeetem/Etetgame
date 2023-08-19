using System.Collections.Concurrent;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.AI;

public class AI
{
	public static readonly Dictionary<AIAction.AIActionType, AIAction> AiActions = new();

	public static void Init()
	{
		new Move();

	}
	public static void DoAITurn(List<Unit> squad)
	{
		var t = new Task(delegate
		{
			Task.Run(() =>
			{
				foreach (var unit in squad)
				{
					Console.WriteLine("---------AI acting with unit: "+unit!.WorldObject.TileLocation.Position);
                    
					while (unit!.MovePoints.Current > unit!.ActionPoints.Current)
					{

						do
						{
							Thread.Sleep(1000);
						} while (WorldManager.Instance.SequenceRunning);

					}
					Console.WriteLine("---------AI DONE ---- acting with unit: "+unit!.WorldObject.TileLocation.Position);
				}
				Console.WriteLine("AI turn over, ending turn"); 
				GameManager.NextTurn();
			});
		});
		WorldManager.Instance.RunNextAfterFrames(t,2);

	}


	
	


}
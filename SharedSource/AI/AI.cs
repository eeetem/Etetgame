using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

	public static Task AItask;
	public static void DoAITurn(List<Unit> squad)
	{
		var t = new Task(delegate
		{
            AItask = Task.Run(() =>
			{
				foreach (var unit in squad)
				{	
					if(unit.IsPlayer1Team != GameManager.IsPlayer1Turn) break;
					Console.WriteLine("---------AI acting with unit: "+unit!.WorldObject.TileLocation.Position);
                    
					while (0>unit!.MovePoints.Current)
					{
						AiActions[AIAction.AIActionType.Move].Execute(unit);
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
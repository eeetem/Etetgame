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

					while (true)
					{
						var actions = CalculateActionList(unit);
						AIAction actionToDo = null;
						int totalScore = 0;
						foreach (var action in actions)
						{
							totalScore += action.Item2;
						}

						if (totalScore == 0) break;
						int roll = Random.Shared.Next(1000) % totalScore;
						for (int i = 0; i < actions.Count; i++)
						{
							if (roll < actions[i].Item2)
							{
								actionToDo = actions[i].Item1;
								break;
							}

							roll -= actions[i].Item2;

						}

						if (actionToDo == null)
						{
							throw new Exception("AI failed to pick an actionq");
						}

						actionToDo.Execute(unit);
						do
						{
							Thread.Sleep(1000);
						} while (WorldManager.Instance.SequenceRunning);
					}

					/*while (0>unit!.MovePoints.Current)
					{
						AiActions[AIAction.AIActionType.Move].Execute(unit);
	

					}*/
					Console.WriteLine("---------AI DONE ---- acting with unit: "+unit!.WorldObject.TileLocation.Position);
				}
				Console.WriteLine("AI turn over, ending turn"); 
				GameManager.NextTurn();
			});
		});
		WorldManager.Instance.RunNextAfterFrames(t,2);

	}


	public static List<Tuple<AIAction, int>>  CalculateActionList(Unit u)
	{
		List<Tuple<AIAction, int>> actions = new();
		AIAction a = new Attack();
		actions.Add(new Tuple<AIAction, int>(a, a.GetScore(u)));
		a = new Move();
		actions.Add(new Tuple<AIAction, int>(a, a.GetScore(u)));
		actions.RemoveAll((x) => x.Item2 <= 0);
		return actions;
	}



}
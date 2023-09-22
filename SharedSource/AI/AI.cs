using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.AI;

public class AI
{
	public static void DoAITurn(List<Unit> squad)
	{
		var t = new Task(delegate
		{
            Task.Run(() =>
            {
	            try
	            {
		            while (true)
		            {
			            List<Tuple<AIAction, Unit, int>> actions = new();
			            foreach (var unit in squad.Shuffle(Random.Shared))
			            {
				            AIAction a = new Attack();
				            Console.WriteLine("Calculating Attack Action..."); 
				            actions.Add(new Tuple<AIAction,Unit, int>(a,unit, a.GetScore(unit)));
				            a = new Move();
				            Console.WriteLine("Calculating Move Action..."); 
				            actions.Add(new Tuple<AIAction,Unit, int>(a,unit, a.GetScore(unit)));
				            actions.RemoveAll((x) => x.Item3 <= 0);
				            if(actions.Count > 0) break;
			            }
			            if(actions.Count == 0) break;
						
			            Tuple<AIAction, Unit, int>? actionToDo = null;
						
			            foreach (var action in actions)
			            {
				            if (actionToDo == null || action.Item3 > actionToDo.Item3)
				            {
					            actionToDo = action;
				            }
			            }


			            actionToDo.Item1.Execute(actionToDo.Item2);
			            Console.WriteLine("Doing AI action");
			            do
			            {
				            Thread.Sleep(100);
			            } while (WorldManager.Instance.SequenceRunning);
		            }

		
		            Console.WriteLine("AI turn over, ending turn");
		            do
		            {
			            Thread.Sleep(1000);
		            } while (WorldManager.Instance.SequenceRunning);

		            GameManager.NextTurn();
	            }
	            catch (Exception e)
	            {
		            Console.WriteLine(e);
		            throw;
	            }
            });
		});
		WorldManager.Instance.RunNextAfterFrames(t,2);

	}


}
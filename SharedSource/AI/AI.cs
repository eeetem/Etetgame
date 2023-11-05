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
				int currentUnitIndex =0;
	            try
	            {
		            while (true)
		            {
			            if(GameManager.IsPlayer1Turn) break;
			            List<Tuple<AIAction, Unit, int>> actions = new();
			            
			            //keep going with current  unit untill depleted
			            var currentUnit = squad[currentUnitIndex];
			            AIAction a = new Attack();
			            Console.WriteLine("Calculating Attack Action..."); 
			            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));
			            a = new Overwatch();
			            Console.WriteLine("Calculating Overwatch Action...");
			            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));
			            
			            a = new Move();
			            Console.WriteLine("Calculating Move Action..."); 
			            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));
			            actions.RemoveAll((x) => x.Item3 <= 0);
			            int totalScore = 0;
			            actions.ForEach((x) => totalScore += x.Item3);
			            if (actions.Count == 0 || Random.Shared.Next(0, 100) > totalScore)
			            {
				            foreach (var unit in squad.Shuffle(Random.Shared))
				            {
					            currentUnitIndex = squad.IndexOf(unit);
					            a = new Attack();
					            Console.WriteLine("Calculating Attack Action...");
					            actions.Add(new Tuple<AIAction, Unit, int>(a, unit, a.GetScore(unit)));
					            a = new Overwatch();
					            Console.WriteLine("Calculating Overwatch Action...");
					            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));

					            a = new Move();
					            Console.WriteLine("Calculating Move Action...");
					            actions.Add(new Tuple<AIAction, Unit, int>(a, unit, a.GetScore(unit)));
					            actions.RemoveAll((x) => x.Item3 <= 0);
					            if (actions.Count > 0) break;
				            }

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

		            if (!GameManager.IsPlayer1Turn)
		            {
			            GameManager.NextTurn();
		            }
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
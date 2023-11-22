using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.ReplaySequence;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Collections;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;

namespace DefconNull.AI;

public class AI
{


	public static bool passiveMode = false;
	protected static bool CanSeeAnyEnemy(bool team1)
	{
		var enemyUnits = GameManager.GetTeamUnits(!team1);
		foreach (var enemy in enemyUnits)
		{
			if (WorldManager.Instance.CanTeamSee(enemy.WorldObject.TileLocation.Position, team1) >= enemy.WorldObject.GetMinimumVisibility())
			{
				return true;
			}
		}

		return false;
	}
	public static void DoAITurn(List<Unit> squad)
	{
		var t = new Task(delegate
		{
       
				int currentUnitIndex =0;
	            try
	            {
		            while (true)
		            {
			            do
			            {
				            Thread.Sleep(100);
			            } while (SequenceManager.SequenceRunning);
			            
			          //  if(GameManager.IsPlayer1Turn) break;
			            List<Tuple<AIAction, Unit, int>> actions = new();
			            
			            //keep going with current  unit untill depleted
			            var currentUnit = squad[currentUnitIndex];
			            if(currentUnit.IsPlayer1Team != GameManager.IsPlayer1Turn) return;
			            passiveMode =  !CanSeeAnyEnemy(currentUnit.IsPlayer1Team); 
			            AIAction a = new Attack();
			            Console.WriteLine("Calculating Attack Action..."); 
			            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));
			            a = new Overwatch();
			            Console.WriteLine("Calculating Overwatch Action...");
			            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));
			            
			            a = new Move();
			            Console.WriteLine("Calculating Move Action..."); 
			            actions.Add(new Tuple<AIAction,Unit, int>(a,currentUnit, a.GetScore(currentUnit)));
			            actions.RemoveAll((x) => x.Item3 <= 1);
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
					            actions.RemoveAll((x) => x.Item3 <= 1);
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
			            Console.WriteLine("Doing AI action with score: "+actionToDo.Item3);
			          
		            }

		
		            Console.WriteLine("AI turn over, ending turn");
		            do
		            {
			            Thread.Sleep(1000);
		            } while (SequenceManager.SequenceRunning);

		          //  if (!GameManager.IsPlayer1Turn)
		           // {
			            GameManager.NextTurn();
		           // }
	            }
	            catch (Exception e)
	            {
		            Console.WriteLine(e);
		            throw;
	            }
          
		});
		
		t.Start();

	}


}
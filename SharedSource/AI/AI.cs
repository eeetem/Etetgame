using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DefconNull.Networking;
using DefconNull.ReplaySequence;
using DefconNull.WorldObjects;
using MonoGame.Extended.Collections;
using Riptide;

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

	public static void PopulateActionsForUnit(Unit u, List<ValueTuple<AIAction, int>> actions)
	{
		AIAction a = new Attack(u);
		Log.Message("AI","Calculating Attack Action...");
		actions.Add(new ValueTuple<AIAction,int>(a, a.GetScore()));
		Move mv = new Move(u);
		Log.Message("AI","Calculating Move Action...");
		actions.Add(new ValueTuple<AIAction, int>(mv, mv.GetScore()));
		a = new Overwatch(u,mv);
		Log.Message("AI","Calculating Overwatch Action...");
		actions.Add(new ValueTuple<AIAction,int>(a, a.GetScore()));
					
		int i = 0;
		actions.ForEach((x) =>
		{
			Log.Message("AI",i + " ability scored: " + x.Item2);
			i++;
		});
		actions.RemoveAll((x) => x.Item2 <= 0);
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
						Log.Message("AI","Waiting for sequence to end");
					} while (SequenceManager.SequenceRunning);
					Log.Message("AI","Waiting for seqeunce ended");
					//  if(GameManager.IsPlayer1Turn) break;
					List<ValueTuple<AIAction, int>> actions = new();

					//keep going with current  unit untill depleted
					var currentUnit = squad[currentUnitIndex];
					if (currentUnit.IsPlayer1Team != GameManager.IsPlayer1Turn) return;
					passiveMode = !CanSeeAnyEnemy(currentUnit.IsPlayer1Team);
					
					Log.Message("AI","PASSIVE MODE: "+passiveMode);
					PopulateActionsForUnit(currentUnit, actions);
	
					int totalScore = 0;
					actions.ForEach((x) => totalScore += x.Item2);
					if (actions.Count == 0 || Random.Shared.Next(0, 100) > totalScore)
					{
						foreach (var unit in squad.Shuffle(Random.Shared))
						{
							currentUnitIndex = squad.IndexOf(unit);
							PopulateActionsForUnit(unit,actions);
							if (actions.Count > 0)
							{
								break;
							}
					            
							if(unit.MovePoints.Current > 0 && !unit.Overwatch.Item1)
							{
								Log.Message("AI","no actions for unit despite having points");
							}
						}

					}

					if(actions.Count == 0) break;
						
					ValueTuple<AIAction, int>? actionToDo = null;
						
					foreach (var action in actions)
					{
						if (!actionToDo.HasValue || action.Item2 > actionToDo.Value.Item2)
						{
							actionToDo = action;
						}
					}

#if SERVER
					while (NetworkingManager.HasPendingMessages)
					{
						Thread.Sleep(1000);
					};		
#endif
					Log.Message("AI","Doing AI action"+actionToDo.Value.Item1+" with score: "+actionToDo.Value.Item2 + " by " + actionToDo.Value.Item1.Unit.WorldObject.ID);

					actionToDo.Value.Item1.Execute();

				}

		
				Log.Message("AI","AI turn over, ending turn");
				do
				{
					Thread.Sleep(1000);
				} while (SequenceManager.SequenceRunning);
#if SERVER
			
				while (NetworkingManager.HasPendingMessages)
				{
					Thread.Sleep(1000);
				} 
#endif
				
				//  if (!GameManager.IsPlayer1Turn)
				// {
				GameManager.NextTurn();
				// }
			}
			catch (Exception e)
			{
				Log.Message("AI",e.ToString());
				throw;
			}
          
		});
		
		t.Start();

	}


}
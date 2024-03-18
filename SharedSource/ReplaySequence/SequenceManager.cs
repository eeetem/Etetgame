using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.Networking;

#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
using DefconNull.WorldObjects;
#endif

namespace DefconNull.ReplaySequence;

public class SequenceManager
{
	private static readonly Queue<SequenceAction> SequenceQueue = new Queue<SequenceAction>();
	public static bool SequenceRunning => SequenceQueue.Count > 0 || CurrentSequenceTasks.Count > 0 || nextFrameTasks.Count > 0 || SequenceRunningRightNow;
	private static readonly List<Task> CurrentSequenceTasks = new List<Task>();
	public static bool SequenceRunningRightNow;
	
	private static object lockObj = new object();
    
    
	private static List<Tuple<Task,int>> nextFrameTasks = new List<Tuple<Task,int>>();
	public static readonly object TaskSync = new object();
	public static void RunNextAfterFrames(Task t, int frames = 1)
	{
		lock (TaskSync)
		{
			nextFrameTasks.Add(new Tuple<Task, int>(t, frames));
		}
	}
	public static void Update()
	{
		List<Task> tasksToRun = new List<Task>();
		lock (TaskSync)
		{
			List<Tuple<Task, int>> updatedList = new List<Tuple<Task, int>>();	
			foreach (var task in nextFrameTasks)
			{
				if (task.Item2 > 1)
				{
					updatedList.Add(new Tuple<Task, int>(task.Item1, task.Item2 - 1));
				}
				else
				{
					tasksToRun.Add(task.Item1);
				}
			}

			nextFrameTasks = updatedList;
		}

		if (tasksToRun.Count > 0)
		{
			Log.Message("WORLD MANAGER","running next frame tasks: "+tasksToRun.Count);
		}
		foreach (var task in tasksToRun)
		{
			task.RunTaskSynchronously();
		}
		
		SequenceRunningRightNow = true;
		lock (lockObj)
		{
			if (CurrentSequenceTasks.Count == 0 && GameManager.NoPendingUpdates())
			{
				if (SequenceQueue.Count > 0)
				{
					var act = SequenceQueue.Dequeue();
					while (!act.ShouldDo())
					{
						act.Return();
						if (SequenceQueue.Count == 0)
						{
							SequenceRunningRightNow = false;
							return;
						}

						act = SequenceQueue.Dequeue();
					}

					Log.Message("SEQUENCE MANAGER","runnin sequnce task: " + act.GetSequenceType());
					CurrentSequenceTasks.Add(act.GenerateTask());
					CurrentSequenceTasks.Last().Start();

					int i = 0;
					//do sequential tasks in queue
					Stopwatch sw = new Stopwatch();
					sw.Start();
					while (true)
					{
						
						if (SequenceQueue.Count == 0)
						{
							break;
						}
						var peeked = SequenceQueue.Peek();
						if (peeked.Batching != SequenceAction.BatchingMode.Sequential || !peeked.ShouldDo())
						{
							break;
						}

						CurrentSequenceTasks.Last().Wait(100);
						if(CurrentSequenceTasks.Last().Status != TaskStatus.RanToCompletion)
						{
							break;//most likely cause by the task waiting for UI Thread
						}
						if(sw.ElapsedMilliseconds >600)//dont freeze the game
						{
							break;
						}
						
						Log.Message("SEQUENCE MANAGER",$"sequnceing a  task: {i}{peeked.GetSequenceType()}");
						i++;
						StartNextTask();
						
					}
					sw.Stop();
					//then do parrallel tasks in queue
					//batch tile updates and other things
					
					while (true)
					{
						if (SequenceQueue.Count == 0)
						{
							break;
						}

						var peeked = SequenceQueue.Peek();
						bool shouldBatch = false;
						switch (peeked.Batching)
						{
							case SequenceAction.BatchingMode.Always:
								shouldBatch = true;
								break;
							case SequenceAction.BatchingMode.OnlySameType:
								shouldBatch = peeked.GetSequenceType() == act.GetSequenceType();
								break;
							case SequenceAction.BatchingMode.Never:
							case SequenceAction.BatchingMode.Sequential:
								shouldBatch = false;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						if(!shouldBatch || !peeked.ShouldDo()) break;
						
						Log.Message("SEQUENCE MANAGER",$"batching sequnce task: {i}{peeked.GetSequenceType()}");
						i++;
						StartNextTask();

					}

				}
			}
			else if (CurrentSequenceTasks.TrueForAll((t) => t.Status != TaskStatus.Running && t.Status != TaskStatus.WaitingToRun))
			{
				foreach (var t in CurrentSequenceTasks)
				{
					if (t.Status == TaskStatus.RanToCompletion)
					{
						//	Log.Message("SEQUENCE MANAGER","sequence task finished");
					}
					else if (t.Status == TaskStatus.Faulted)
					{
						Log.Message("SEQUENCE MANAGER","Sequence task failed");
						Log.Message("SEQUENCE MANAGER",t.Status.ToString());
						throw t.Exception!;
					}
					else
					{
						Log.Message("SEQUENCE MANAGER","undefined sequence task state: "+t.Status);
						throw new Exception("undefined sequence task state: "+t.Status);
					}

				}

				CurrentSequenceTasks.Clear();
				SequenceRunningRightNow = false;

				if(SequenceQueue.Count == 0)
				{
					
#if CLIENT
					GameLayout.ReMakeMovePreview();
					NetworkingManager.SendSequenceExecuted();
#endif
				}

			}
		}

		SequenceRunningRightNow = false;
	}

	private static void StartNextTask()
	{
		var act = SequenceQueue.Dequeue();
		CurrentSequenceTasks.Add(act.GenerateTask());
		CurrentSequenceTasks.Last().Start();
	}

	
	public static void AddSequence(SequenceAction action)
	{
		if(action==null) throw new ArgumentNullException(nameof(action));

		lock (lockObj)
		{
			Log.Message("SEQUENCE MANAGER","adding sequnce task: " + action);
			SequenceQueue.Enqueue(action);
		}
	
	}

	public static void AddSequence(IEnumerable<SequenceAction> actions)
	{
		foreach (var a in actions)
		{
			AddSequence(a);
		}
	}

}
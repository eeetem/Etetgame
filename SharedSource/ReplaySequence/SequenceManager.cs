using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif

namespace DefconNull.ReplaySequence;

public class SequenceManager
{
	private static readonly Queue<SequenceAction> SequenceQueue = new Queue<SequenceAction>();
	public static bool SequenceRunning => SequenceQueue.Count > 0 || CurrentSequenceTasks.Count > 0;
	private static readonly List<Task> CurrentSequenceTasks = new List<Task>();
	public static bool SequenceRunningRightNow;
	
	private static object lockObj = new object();
	public static void Update()
	{
				
		SequenceRunningRightNow = true;
		lock (lockObj)
		{
			if (CurrentSequenceTasks.Count == 0)
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

					Console.WriteLine("runnin sequnce task: " + act.GetSequenceType());
					CurrentSequenceTasks.Add(act.GenerateTask());
					CurrentSequenceTasks.Last().Start();




					//batch tile updates and other things
					int i = 0;
					while (true)
					{
						if (SequenceQueue.Count == 0 || CurrentSequenceTasks.Count >= 45)
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
								shouldBatch = false;
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						if(!shouldBatch) break;
						act = SequenceQueue.Dequeue();
						Console.WriteLine($"batching sequnce task: {i}{act.GetSequenceType()}");
						i++;
						CurrentSequenceTasks.Add(act.GenerateTask());
						CurrentSequenceTasks.Last().Start();
					}

				}
			}
			else if (CurrentSequenceTasks.TrueForAll((t) => t.Status != TaskStatus.Running))
			{
				foreach (var t in CurrentSequenceTasks)
				{
					if (t.Status == TaskStatus.RanToCompletion)
					{
						//	Console.WriteLine("sequence task finished");
					}
					else if (t.Status == TaskStatus.Faulted)
					{
						Console.WriteLine("Sequence task failed");
						Console.WriteLine(t.Status);
						throw t.Exception!;
					}
					else
					{
						Console.WriteLine("undefined sequence task state");
						throw new Exception("undefined sequence task state");
					}

				}

				CurrentSequenceTasks.Clear();
				SequenceRunningRightNow = false;
#if CLIENT
			if(SequenceQueue.Count == 0)
			{
				GameLayout.ReMakeMovePreview();
			}
#endif
			}
		}

		SequenceRunningRightNow = false;
	}


	public static void AddSequence(SequenceAction action)
	{
		if(action==null) throw new ArgumentNullException(nameof(action));

		lock (lockObj)
		{
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
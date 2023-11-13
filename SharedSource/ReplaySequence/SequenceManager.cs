using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefconNull.World.WorldObjects.Units.ReplaySequence;

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
	
	public static void Update()
	{
				
		SequenceRunningRightNow = true;
		
		if (CurrentSequenceTasks.Count == 0)
		{
			if (SequenceQueue.Count > 0)
			{
				var act = SequenceQueue.Dequeue();
				while (!act.ShouldDo())
				{
					act.Return();
					if(SequenceQueue.Count == 0){
						SequenceRunningRightNow = false;
						return;
					}
					act = SequenceQueue.Dequeue();
				}
				//Console.WriteLine("runnin sequnce task: "+task.SqcType);
				CurrentSequenceTasks.Add(act.GenerateTask());
				CurrentSequenceTasks.Last().Start();
		
					

					
				//batch tile updates and other things
				while (true)
				{
					if (SequenceQueue.Count == 0)
					{
						break;
					}
				
					if(!SequenceQueue.Peek().CanBatch || !SequenceQueue.Peek().ShouldDo()) break;
						
                        
					CurrentSequenceTasks.Add(SequenceQueue.Dequeue().GenerateTask());
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
				}else{
					Console.WriteLine("undefined sequence task state");
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
		SequenceRunningRightNow = false;
	}


	public static void AddSequence(SequenceAction action)
	{
		if(action==null) throw new ArgumentNullException(nameof(action));
		SequenceQueue.Enqueue(action);
		Console.WriteLine("adding action "+action.GetSequenceType()+" to sequence");
	}

	public static void AddSequence(IEnumerable<SequenceAction> actions)
	{
		foreach (var a in actions)
		{
			AddSequence(a);
		}
	}

}
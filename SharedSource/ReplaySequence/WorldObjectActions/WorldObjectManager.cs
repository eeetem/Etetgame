using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DefconNull.WorldObjects;
using DefconNull.Networking;

#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif

namespace DefconNull.ReplaySequence.WorldObjectActions;

public static partial class WorldObjectManager
{
	
	
	public static void Init()
	{
		throw new NotImplementedException();
	}
	
	/**
	 * Destructs the world object with the given ID properly with all destruction effects rather than just deleting it
	 * 
	 */
	public static void Destroy(WorldObject obj)
	{
		if(obj.destroyed)return;
		obj.destroyed = true;

		DeleteWorldObject.Make(obj.ID).GenerateTask().RunTaskSynchronously();
#if SERVER
		if(obj.Type.DestructionConseqences != null)
		{
			var cons = obj.Type.DestructionConseqences.GetApplyConsiqunces(obj);
			NetworkingManager.SendSequence(cons);
			SequenceManager.AddSequence(cons);
		}

#endif
		Console.WriteLine("Destroyed "+obj.ID +" "+obj.Type.Name);

	}
	
	public static WorldObject? GetObject(int id)
	{
		if(WorldObjects.TryGetValue(id, out var obj2))
			return obj2;
		return null;
	}
	
	
	private static readonly object IdAquireLock = new object();
	private static int NextId;
	private static readonly object WoLock = new object();
	private static readonly Dictionary<int, WorldObject> WorldObjects = new Dictionary<int, WorldObject>(){};
	private static int GetNextId()
	{
		lock (IdAquireLock)
		{
			NextId++;
			while (WorldObjects.ContainsKey(NextId)) //skip all the server-side force assinged IDs
			{
				NextId++;
			}
		}

		return NextId;
	}


}
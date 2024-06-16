using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DefconNull.WorldObjects;
using DefconNull.Networking;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

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
		if(obj.UnitComponent!=null)
		{
			obj.UnitComponent.ClearOverWatch();
		}
#if CLIENT
		obj.StartAnimation("end");
#endif
		SequenceManager.AddSequence(DeleteWorldObject.Make(obj.ID));
#if SERVER
		if(obj.Type.DestructionConseqences != null)
		{
			var cons = obj.Type.DestructionConseqences.GetApplyConsequnces(obj,obj);
			NetworkingManager.AddSequenceToSendQueue(cons);
		}

#endif
		Log.Message("WORLD OBJECT MANAGER","Destroyed "+obj.ID +" "+obj.Type.Name);

	}
	
	public static WorldObject? GetObject(int id)
	{
		if(WorldObjects.TryGetValue(id, out var obj2))
			return obj2;
		return null;
	}
	

	private static int NextId;
	public static readonly object WoLock = new object();
	public static int WoReadLock = 0;
	private static readonly Dictionary<int, WorldObject> WorldObjects = new Dictionary<int, WorldObject>(){};
	private static int GetNextId()
	{
		lock (WoLock)
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
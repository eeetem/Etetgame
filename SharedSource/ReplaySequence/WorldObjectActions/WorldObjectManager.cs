using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using DefconNull.WorldObjects;

#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif

namespace DefconNull.ReplaySequence.WorldObjectActions;

public static partial class WorldObjectManager
{
	/**
	 * Destructs the world object with the given ID properly with all destruction effects rather than just deleting it
	 * 
	 */
	public static void Destroy(WorldObject obj)
	{
		if(obj.destroyed)return;
		obj.destroyed = true;
		
		
		if(obj.Type.DestructionConseqences != null)
		{
			SequenceManager.AddSequence(obj.Type.DestructionConseqences.GetApplyConsiqunces(obj));
		}

		
		DeleteWorldObject(obj.ID);


		Console.WriteLine("Destroyed "+obj.ID +" "+obj.Type.Name);

	}
	
	public static WorldObject? GetObject(int id)
	{
		if(WorldObjects.TryGetValue(id, out var obj2))
			return obj2;
		return null;
	}

	public static void DeleteWorldObject(WorldObject obj)
	{
		DeleteWorldObject(obj.ID);
	}

	public static void Update(float gameTime)
	{
		foreach (var obj in WorldObjects.Values)
		{
			obj.Update(gameTime);
		}
	}

	public static void DeleteWorldObject(int id)
	{
		if (!WorldObjects.ContainsKey(id)) return;

		if (id < NextId)
		{
			NextId = id; //reuse IDs
		}

		WorldObject Obj = WorldObjects[id];
			
		GameManager.Forget(Obj);

#if  CLIENT
		if (Obj.UnitComponent != null)
		{
			GameLayout.UnRegisterUnit(Obj.UnitComponent);
		}
#endif		
		(Obj.TileLocation as WorldTile)?.Remove(id);
	
		WorldObjects.Remove(id);
	}
	
	private static readonly object IdAquireLock = new object();
	private static int NextId;

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
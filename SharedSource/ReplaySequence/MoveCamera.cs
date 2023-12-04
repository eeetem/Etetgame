﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Riptide;

namespace DefconNull.ReplaySequence;

public class MoveCamera : SequenceAction
{
	
	public Vector2Int location;
	public bool doAlways;
	public int scatter;

	public static MoveCamera Make(Vector2Int location,bool doAlways, int scatter) 
	{
		MoveCamera t = (GetAction(SequenceType.MoveCamera) as MoveCamera)!;
		t.location = location;
		t.doAlways = doAlways;
		t.scatter = scatter;
		return t;
	}
#if SERVER
	public override bool ShouldDoServerCheck(bool player1)
	{
		return true;
	}
#endif


	public override BatchingMode Batching => BatchingMode.Always;

	public override SequenceType GetSequenceType()
	{
		return SequenceType.MoveCamera;
	}

	
	//todo serverside scattering
	protected override void RunSequenceAction()
	{

#if CLIENT
			if (WorldManager.Instance.GetTileAtGrid(location).GetVisibility()==Visibility.None)
			{
				if (doAlways)
				{
					Camera.SetPos(location + new Vector2Int(Random.Shared.Next(-scatter, scatter), Random.Shared.Next(-scatter, scatter)),true);
				}
			}
			else
			{
				Camera.SetPos(location,true);
			}

			while (Camera.ForceMoving)
			{
				Thread.Sleep(50);
			}
#endif
			

	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(location);
		message.Add(doAlways);
		message.Add(scatter);
	}

	protected override void DeserializeArgs(Message message)
	{
		location = message.GetSerializable<Vector2Int>();
		doAlways = message.GetBool();
		scatter = message.GetInt();
	}
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using DefconNull.ReplaySequence.WorldObjectActions;
using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.ReplaySequence;

public class SpotUnit : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.SpotUnit;
	}

	public override BatchingMode Batching => BatchingMode.AsyncBatchSameType;

	
	private (Vector2Int, WorldObject.WorldObjectData)? spotedUnit = new();
	private int unitId;
	private bool _player1;
	
	//not netowrked
	Vector2Int forcePos;
	
	public static SpotUnit Make(int id, bool player1)
	{
		SpotUnit t = (GetAction(SequenceType.SpotUnit) as SpotUnit)!;
		t.unitId = id;
		t._player1 = player1;
		t.forcePos = new Vector2Int(-1,-1);
		t.spotedUnit = null;
		return t;
	}
	public static SpotUnit Make(int id, Vector2Int forcePos, bool player1)
	{
		SpotUnit t = (GetAction(SequenceType.SpotUnit) as SpotUnit)!;
		t.unitId = id;
		t.forcePos = forcePos;
		t._player1 = player1;
		t.spotedUnit = null;
		return t;
	}
	protected override void RunSequenceAction()
	{
#if SERVER
		// this is sent to clients so make sure we dont override with it a regular unit update
		GameManager.GetPlayer(_player1)!.KnownUnitPositions[unitId] = spotedUnit!.Value;
#else
		Dictionary<int,(Vector2Int,WorldObject.WorldObjectData)> dict = new();
		dict[unitId] = spotedUnit!.Value;
		GameManager.UpdateUnitPositions(_player1, dict, false);
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_player1);

		message.Add(unitId);
		message.Add(spotedUnit!.Value.Item1);
		message.Add(spotedUnit.Value.Item2);
		
	}

	protected override void DeserializeArgs(Message message)
	{
		_player1 = message.GetBool();
		
		unitId = message.GetInt();
		Vector2Int pos = message.GetSerializable<Vector2Int>();
		WorldObject.WorldObjectData data = message.GetSerializable<WorldObject.WorldObjectData>();
		spotedUnit = (pos, data);
		
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return player1 == _player1;
	}

	
	//we get data here instead of creation for the sake of getting most recent state
	public override void FilterForPlayer(bool player1)
	{
		var unit = WorldObjectManager.GetObject(unitId);

		Debug.Assert(unit != null, nameof(unit) + " != null");
		Vector2Int pos = unit.TileLocation.Position;
		if(forcePos != new Vector2Int(-1,-1))
			pos = forcePos;
		spotedUnit = (pos, unit.GetData());

	}
#endif
	public override string ToString()
	{
		return $"{nameof(spotedUnit)}: {spotedUnit}, {nameof(unitId)}: {unitId}";
	}
}
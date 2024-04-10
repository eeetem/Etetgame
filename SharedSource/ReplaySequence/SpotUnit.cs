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
		return SequenceType.UnitUpdate;
	}

	public override BatchingMode Batching => BatchingMode.Sequential;

	
	private (Vector2Int, WorldObject.WorldObjectData)? spotedUnit = new();
	private int unitId;
	private bool _player1;
	

	public static SpotUnit Make(int id, bool player1)
	{
		SpotUnit t = (GetAction(SequenceType.SpotUnit) as SpotUnit)!;
		t.unitId = id;
		t._player1 = player1;
		t.spotedUnit = null;
		return t;
	}
	protected override void RunSequenceAction()
	{
#if SERVER
		if (_player1)
		{
			GameManager.Player1UnitPositions[unitId] = spotedUnit!.Value;
		}
		else
		{
			GameManager.Player2UnitPositions[unitId] = spotedUnit!.Value;
		}
		
#else

		GameManager.UpdateUnitPositions(_player1, , _fullUpdate);
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_player1);

		message.Add(unitId);
		message.Add(spotedUnit.Value.Item1);
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
		spotedUnit = (unit.TileLocation.Position, unit.GetData());

	}
#endif
	
}
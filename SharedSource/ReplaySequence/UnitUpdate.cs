using System;
using System.Collections.Generic;
using DefconNull.WorldObjects;
using Riptide;

namespace DefconNull.ReplaySequence;

public class UnitUpdate : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.UnitUpdate;
	}

	public override BatchingMode Batching => BatchingMode.OnlySameType;

	private Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)> unitPositions = new Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)>();
	private bool _fullUpdate = false;
	private bool _player1;
	
	public static UnitUpdate Make(int id, Vector2Int location,WorldObject.WorldObjectData data, bool player1) 
	{
		UnitUpdate t = (GetAction(SequenceType.UnitUpdate) as UnitUpdate)!;
		t.unitPositions.Clear();
		t.unitPositions.Add(id, (location, data));
		t._fullUpdate = false;
		t._player1 = player1;
		return t;
	}
	public static UnitUpdate Make(Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)> player1UnitPositions, bool player1)
	{
		UnitUpdate t = (GetAction(SequenceType.UnitUpdate) as UnitUpdate)!;
		t.unitPositions = new Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)>(player1UnitPositions);
		t._fullUpdate = true;
		t._player1 = player1;
		return t;
	}
	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		if (_player1 != GameManager.IsPlayer1) throw new Exception("unitUpdate for wrong team recived");
		GameManager.UpdateUnitPositions(unitPositions, _fullUpdate);
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_player1);
		message.Add(_fullUpdate);
		message.Add(unitPositions.Count);
		foreach (var u in unitPositions)
		{
			message.Add(u.Key);
			message.Add(u.Value.Item1);
			message.Add(u.Value.Item2);
		}
	}

	protected override void DeserializeArgs(Message message)
	{
		_player1 = message.GetBool();
		_fullUpdate = message.GetBool();
		unitPositions.Clear();
		int count = message.GetInt();
		for (int i = 0; i < count; i++)
		{
			int id = message.GetInt();
			Vector2Int pos = message.GetSerializable<Vector2Int>();
			WorldObject.WorldObjectData data = message.GetSerializable<WorldObject.WorldObjectData>();
			unitPositions.Add(id, (pos, data));
		}
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return player1 == _player1;
	}
#endif
	
}
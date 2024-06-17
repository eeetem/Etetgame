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

	public override BatchingMode Batching => BatchingMode.BlockingAlone;

	private Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)> _unitPositions = new Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)>();
	private bool _player1;
	
	
	public static UnitUpdate Make(Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)> player1UnitPositions, bool player1)
	{
		UnitUpdate t = (GetAction(SequenceType.UnitUpdate) as UnitUpdate)!;
		t._unitPositions = new Dictionary<int, (Vector2Int, WorldObject.WorldObjectData)>(player1UnitPositions);
		t._player1 = player1;
		return t;
	}
	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		GameManager.UpdateUnitPositions(_player1, _unitPositions, true);
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_player1);
		message.Add(_unitPositions.Count);
		foreach (var u in _unitPositions)
		{
			message.Add(u.Key);
			message.Add(u.Value.Item1);
			message.Add(u.Value.Item2);
		}
	}

	protected override void DeserializeArgs(Message message)
	{
		_player1 = message.GetBool();
		_unitPositions.Clear();
		int count = message.GetInt();
		for (int i = 0; i < count; i++)
		{
			int id = message.GetInt();
			Vector2Int pos = message.GetSerializable<Vector2Int>();
			WorldObject.WorldObjectData data = message.GetSerializable<WorldObject.WorldObjectData>();
			_unitPositions.Add(id, (pos, data));
		}
	}
#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return player1 == _player1;
	}
#endif
	
}
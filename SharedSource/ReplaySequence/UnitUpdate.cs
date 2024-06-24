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
	private bool? _player1;
	
	
	public static UnitUpdate Make(bool? player1 = null)
	{
		UnitUpdate t = (GetAction(SequenceType.UnitUpdate) as UnitUpdate)!;
		t._player1 = player1;
		return t;
	}
	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		GameManager.UpdateUnitPositions(_player1.Value, _unitPositions, true);
#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.AddNullableBool(_player1);
		
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
		_player1 = message.GetNullableBool();
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
	public override void FilterForPlayer(bool player1)
	{
		base.FilterForPlayer(player1);
		if(_player1 != null)// if we have a hardcoded value for player we dont filter
			if(player1 != _player1)
				return;
		_player1 = player1;
		_unitPositions = GameManager.GetPlayer(player1)!.KnownUnitPositions.ToDictionary(x => x.Key, x => x.Value);
	}
#endif
	
}
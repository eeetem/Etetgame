using System;
using System.Collections.Generic;
using DefconNull.WorldObjects;
using DefconNull.WorldObjects.Units.Actions;
using Riptide;

namespace DefconNull.ReplaySequence;

public class TileUpdate : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.TileUpdate;
	}

	public override BatchingMode Batching => BatchingMode.Sequential;
	private WorldTile.WorldTileData _data = new WorldTile.WorldTileData();
	
	public static TileUpdate Make(Vector2Int location) 
	{
		TileUpdate t = (GetAction(SequenceType.TileUpdate) as TileUpdate)!;
		t._data = WorldManager.Instance.GetTileAtGrid(location).GetData();
		return t;
	}
	public static TileUpdate Make(WorldTile.WorldTileData data) 
	{
		TileUpdate t = (GetAction(SequenceType.TileUpdate) as TileUpdate)!;
		t._data = data;
		return t;
	}

	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		WorldManager.Instance.LoadWorldTile(_data);
		if (GameManager.GameState == GameState.Lobby)
		{
		//	Camera.SetPos(_data.Position,false);
		}

#endif
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(_data);

	}

	protected override void DeserializeArgs(Message message)
	{
		_data = message.GetSerializable<WorldTile.WorldTileData>();
	}
#if SERVER
	public override void FilterForPlayer(bool player1)
	{
		var p = GameManager.GetPlayer(player1);
		if (p.WorldState.TryGetValue(_data.Position, out var value))
		{
			_data = value;
		}
	}

	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return true;
	}
#endif
	
}
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
	private bool force = false;
	
	public static TileUpdate Make(Vector2Int location, bool force) 
	{
		TileUpdate t = (GetAction(SequenceType.TileUpdate) as TileUpdate)!;
		t._data = WorldManager.Instance.GetTileAtGrid(location).GetData();
		t.force = force;
		return t;
	}
	public static TileUpdate Make(WorldTile.WorldTileData data,bool force) 
	{
		TileUpdate t = (GetAction(SequenceType.TileUpdate) as TileUpdate)!;
		t._data = data;
		t.force = force;
		return t;
	}
	public override bool ShouldDo()
	{
#if SERVER
		return false;
#else
		var t = WorldManager.Instance.GetTileAtGrid(_data.Position);
		return !t.GetData().Equals(_data);
#endif
	}

	
	protected override void RunSequenceAction()
	{
#if SERVER
		return;
#else
		WorldManager.Instance.LoadWorldTile(_data);
		if (GameManager.GameState == GameState.Lobby)
		{
			Camera.SetPos(_data.Position,false);
		}
#endif
	}
	
	protected override void SerializeArgs(Message message)
	{
		message.Add(_data);
		message.Add(force);
	}

	protected override void DeserializeArgs(Message message)
	{
		_data = message.GetSerializable<WorldTile.WorldTileData>();
		force = message.GetBool();
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
		if(force)
			return true;
		if(GameManager.GetPlayer(player1)!.WorldState.TryGetValue(_data.Position, out var value))
		{
			return !value.Equals(_data);
		}
		return true;
	}
#endif
	
}
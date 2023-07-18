

namespace DefconNull.World.WorldObjects;

public partial class Unit
{
	
	public bool IsMyTeam()
	{
		return GameManager.IsPlayer1 == IsPlayerOneTeam;
	}
	public void Spoted()
	{
		foreach (var tile in overWatchedTiles)
		{
			WorldManager.Instance.GetTileAtGrid(tile).CalcWatchLevel();
		}
			
	}

	
}


namespace DefconNull.WorldObjects;

public partial class Unit
{
	
	public bool IsMyTeam()
	{
		return GameManager.IsPlayer1 == IsPlayer1Team;
	}
	public void Spoted()
	{
		foreach (var tile in overWatchedTiles)
		{
			WorldManager.Instance.GetTileAtGrid(tile).CalcWatchLevel();
		}
			
	}



}
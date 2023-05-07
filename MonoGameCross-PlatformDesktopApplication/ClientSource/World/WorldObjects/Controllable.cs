using MultiplayerXeno;

namespace MultiplayerXeno;

public partial class Controllable
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
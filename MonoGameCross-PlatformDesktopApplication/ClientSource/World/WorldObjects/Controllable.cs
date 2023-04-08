using CommonData;

namespace MultiplayerXeno;

public partial class Controllable
{
	public PreviewData PreviewData;
	public bool IsMyTeam()
	{
		return GameManager.IsPlayer1 == this.IsPlayerOneTeam;
	}
	public void Spoted()
	{
		foreach (var tile in overWatchedTiles)
		{
			WorldManager.Instance.GetTileAtGrid(tile).CalcWatchLevel();
		}
			
	}



}
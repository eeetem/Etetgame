﻿using System;
using MultiplayerXeno;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

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
using System;
using MultiplayerXeno;
using MultiplayerXeno.Items;

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

	private int _selectedItemIndex = -1;
	public int SelectedItemIndex
	{
		get => _selectedItemIndex;
		set
		{
			Console.WriteLine("Selected Item: "+value);
			UseItem.ItemIndex = value;
			_selectedItemIndex = value;
		}
	}
	public WorldAction? SelectedItem
	{
		get
		{
			if(SelectedItemIndex == -1) return null;
			return Inventory[SelectedItemIndex];
		}
	}

	public void SelectAnyItem()
	{
		for (int i = 0; i < Inventory.Length; i++)
		{
			if (Inventory[i] != null)
			{
				SelectedItemIndex = i;
				return;
			}
		}
	}



}
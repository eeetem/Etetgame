using System.Collections.Generic;

namespace MultiplayerXeno;

public class SquadMember
{
	public string? Prefab { get; set; }
	public Vector2Int Position { get; set; } = new Vector2Int(0, 0);
	public List<string?> Inventory { get; set; } = new List<string?>();
}
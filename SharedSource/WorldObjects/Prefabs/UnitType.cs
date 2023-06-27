using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno;

public class UnitType
{

	public UnitType(string? name)
	{
		Name = name;
	}

	public readonly string? Name;
	public int MoveRange = 4;
	public int SightRange = 16;
	public int OverWatchSize = 2;
		
	public int MaxMovePoints = 2;
	public int MaxActionPoints = 2;

	public int Maxdetermination = 2;

	public int InventorySize = 1;


	public Texture2D[] CrouchSpriteSheet = null!;
	public ExtraAction DefaultAttack = null!;
	public readonly List<IExtraAction> ExtraActions = new List<IExtraAction>();
	public WorldEffect? SpawnEffect { get; set; }

	public Unit Instantiate(WorldObject parent,Unit.UnitData data)
	{
		Unit obj = new Unit(data.Team1,parent,this,data);

		return obj;
	}
}
using System.Collections.Generic;
using DefconNull.WorldActions;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif


namespace DefconNull.WorldObjects;

public partial class UnitType : WorldObjectType
{

	public UnitType(string name, List<UnitAbility> actions) : base(name)
	{
		this.actions = actions;
	}

	public int MoveRange = 4;
	public int SightRange = 16;

	public int MaxMovePoints = 2;
	public int MaxActionPoints = 2;

	public int Maxdetermination = 2;


	public readonly List<UnitAbility> actions;

	public WorldConseqences? SpawnEffect { get; set; }


	public override void Place(WorldObject wo, WorldTile tile, WorldObject.WorldObjectData data)
	{ 
		tile.UnitAtLocation = wo.UnitComponent;
	}

	
}
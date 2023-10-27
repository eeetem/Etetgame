using System.Collections.Generic;
using DefconNull.World.WorldActions;

using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif


namespace DefconNull.World.WorldObjects;

public class UnitType : WorldObjectType
{

	public UnitType(string name, List<UnitAbility> actions) : base(name)
	{
		Actions = actions;
	}

	public int MoveRange = 4;
	public int SightRange = 16;

	public int MaxMovePoints = 2;
	public int MaxActionPoints = 2;

	public int Maxdetermination = 2;




	private readonly List<UnitAbility> Actions = new List<UnitAbility>();

	public WorldConseqences? SpawnEffect { get; set; }


	public override void Place(WorldObject wo, WorldTile tile, WorldObject.WorldObjectData data)
	{
		wo.Face(data.Facing,false);
		Unit component = new Unit(wo,this,data.UnitData!.Value);
		Actions.ForEach(extraAction => { component.Abilities.Add((UnitAbility) extraAction.Clone()); });
#if CLIENT
		GameLayout.RegisterUnit(component);
#endif
		tile.UnitAtLocation = component;


	}

	
}
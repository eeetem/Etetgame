using System.Collections.Generic;
using DefconNull.WorldActions;
using DefconNull.WorldActions.UnitAbility;
#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif


namespace DefconNull.WorldObjects;

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
		Unit component = new Unit(wo,this,data.UnitData!.Value,data.JustSpawned);
		Actions.ForEach(extraAction => { component.Abilities.Add((UnitAbility) extraAction.Clone()); });
#if CLIENT
		GameLayout.RegisterUnit(component);
#endif
		tile.UnitAtLocation = component;


	}

	
}
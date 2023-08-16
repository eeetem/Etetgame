using System.Collections.Generic;
using DefconNull.World.WorldActions;
using Microsoft.Xna.Framework.Graphics;
#if CLIENT
using DefconNull.Rendering.UILayout;
#endif


namespace DefconNull.World.WorldObjects;

public class UnitType : WorldObjectType
{

	public UnitType(string name,UnitAbility defAttack, List<IUnitAbility> actions) : base(name)
	{
		DefaultAttack = defAttack;
		Actions = actions;
	}

	public int MoveRange = 4;
	public int SightRange = 16;
	public int OverWatchSize = 2;
		
	public int MaxMovePoints = 2;
	public int MaxActionPoints = 2;

	public int Maxdetermination = 2;

	public int InventorySize = 1;


	public Texture2D[] CrouchSpriteSheet = null!;

	private UnitAbility DefaultAttack = null!;
	private readonly List<IUnitAbility> Actions = new List<IUnitAbility>();

	public WorldConseqences? SpawnEffect { get; set; }


	public override void Place(WorldObject wo, WorldTile tile, WorldObject.WorldObjectData data)
	{
		wo.Face(data.Facing,false);
		Unit component = new Unit(wo,this,data.UnitData!.Value);
		Actions.ForEach(extraAction => { component.Abilities.Add((IUnitAbility) extraAction.Clone()); });
		component.DefaultAttack = (UnitAbility) DefaultAttack.Clone();

#if CLIENT
		GameLayout.RegisterUnit(component);
#endif
		tile.UnitAtLocation = component;


	}
#if CLIENT
	public override Texture2D GetSprite(int spriteVariation, int spriteIndex, WorldObject worldObject)
	{
		if (worldObject.UnitComponent!.Crouching)
		{
			return CrouchSpriteSheet[(int) Utility.NormaliseDir(spriteIndex)];
			
		}

		return base.GetSprite(spriteVariation, spriteIndex, worldObject);
	}
#endif
	
}
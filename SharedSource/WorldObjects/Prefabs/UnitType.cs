using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

#if CLIENT
using MultiplayerXeno.UILayouts;
#endif


namespace MultiplayerXeno;

public class UnitType : WorldObjectType
{

	public UnitType(string name) : base(name)
	{

	}

	public int MoveRange = 4;
	public int SightRange = 16;
	public int OverWatchSize = 2;
		
	public int MaxMovePoints = 2;
	public int MaxActionPoints = 2;

	public int Maxdetermination = 2;

	public int InventorySize = 1;


	public Texture2D[] CrouchSpriteSheet = null!;

	public ExtraAction DefaultAttack = null!;
	public readonly List<IExtraAction> Actions = new List<IExtraAction>();

	public WorldEffect? SpawnEffect { get; set; }


	public override void Place(WorldObject wo, WorldTile tile, WorldObject.WorldObjectData data)
	{
		wo.Face(data.Facing,false);
		Unit component = new Unit(wo,this,data.UnitData!.Value);

#if CLIENT
		GameLayout.RegisterUnit(component);
#endif
		tile.UnitAtLocation = component;

		if (data.UnitData.Value.JustSpawned)//bad spot for this but whatever for now
		{
			this.SpawnEffect?.Apply(tile.Position);
		}
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
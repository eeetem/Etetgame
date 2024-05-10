using System;

using DefconNull.WorldActions;
using DefconNull.WorldActions.UnitAbility;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout;
using Myra.Graphics2D;
#endif
namespace DefconNull.WorldObjects;

public partial class WorldObjectType
{

	public readonly string Name;
	public int MaxHealth;
	public bool lifetimeTick;
	public WorldObjectType(string name)
	{
		Name = name;

#if CLIENT
		Transform = new Transform2();
		Transform.Position = Utility.GridToWorldPos(new Vector2(-1.5f, -0.5f));
#endif
			
	}

	public void SpecialBehaviour(WorldObject objOfType)
	{
		switch (Name)
		{
			case "capturePoint":
				GameManager.CapturePoints.Add(objOfType);
#if CLIENT
				if(GameManager.GameState == GameState.Lobby){
					Camera.SetPos(objOfType.TileLocation.Position);
				}
#endif
				
				break;
			case "spawnPointT1":
				Log.Message("WORLD OBJECT MANAGER","adding spawn point FOR T1");
				GameManager.T1SpawnPoints.Add(objOfType.TileLocation.Position);
#if CLIENT
				if(UI.currentUi is SquadCompBuilderLayout)
				{
					UI.SetUI(null);
				}
#endif	
				break;
			case "spawnPointT2":
				Log.Message("WORLD OBJECT MANAGER","adding spawn point FOR T2");
				GameManager.T2SpawnPoints.Add(objOfType.TileLocation.Position);
#if CLIENT
				if(UI.currentUi is SquadCompBuilderLayout)
				{
					UI.SetUI(null);
				}
               
#endif
				break;
		}
	}


	public Cover SolidCover = Cover.None;
	public Cover VisibilityCover = Cover.None;
	
	public WorldConseqences? DestructionConseqences;

	//should probably be an enum
	public bool Faceable { get; set; }
	public bool Edge { get; set; }
	public bool Surface { get; set; }
	public bool Impassible { get; set; }
	public int VisibilityObstructFactor { get; set; }

	public virtual void Place(WorldObject wo, WorldTile? tile, WorldObject.WorldObjectData data)
	{
			
		wo.Face(data.Facing,false);
		if(tile is null)
			return;
			
		WorldTile newTile;
		if (Surface)
		{
			tile.Surface = wo;
		}
		else if (Edge)
		{
			switch (data.Facing)
			{
				case Direction.North:
					Log.Message("WORLD OBJECT MANAGER","placing edge on north");
					tile.NorthEdge = wo;
					break;
				
				case Direction.West:
					Log.Message("WORLD OBJECT MANAGER","placing edge on west");
					tile.WestEdge = wo;
					break;
				
				case Direction.East:
					Log.Message("WORLD OBJECT MANAGER","placing edge on west");
					newTile = (WorldTile)WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.East));
					newTile.WestEdge = wo;
					data.Facing = Direction.West;
					wo.Face(Direction.West, false);
					wo.Fliped = true;
					wo.TileLocation = newTile;
					break;
					
				case Direction.South:
					Log.Message("WORLD OBJECT MANAGER","placing edge on south");
					newTile = (WorldTile)WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.South));
					newTile.NorthEdge = wo;
					data.Facing = Direction.North;
					wo.Face(Direction.North,false);
					wo.Fliped = true;
					wo.TileLocation = newTile;
					break;
					
				default:
					throw new Exception("edge cannot be cornerfacing");
					
			}
		}
		else
		{
			tile.PlaceObject(wo);
		}
		
	}
}
using System;
using MonoGame.Extended;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno;

public partial class WorldObjectType
{

	public readonly string Name;
	public int MaxHealth;
	public int lifetime = -100;
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

				GameManager.T1SpawnPoints.Add(objOfType.TileLocation.Position);
		
				break;
			case "spawnPointT2":

				GameManager.T2SpawnPoints.Add(objOfType.TileLocation.Position);

				break;
		}
	}


	public Cover SolidCover = Cover.None;
	public Cover VisibilityCover = Cover.None;
	
	public WorldEffect? DesturctionEffect;

	//should probably be an enum
	public bool Faceable { get; set; }
	public bool Edge { get; set; }
	public bool Surface { get; set; }
	public bool Impassible { get; set; }
	public int VisibilityObstructFactor { get; set; }

	public virtual void Place(WorldObject wo, WorldTile tile, WorldObject.WorldObjectData data)
	{
			WorldTile newTile;
			wo.Face(data.Facing,false);
			if (Surface)
			{
				tile.Surface = wo;
			}
			else if (Edge)
			{
				switch (data.Facing)
				{
					case Direction.North:
						tile.NorthEdge = wo;
						break;
				
					case Direction.West:
						tile.WestEdge = wo;
						break;
				
					case Direction.East:
						newTile = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.East));
						newTile.WestEdge = wo;
						wo.Face(Direction.West, false);
						wo.fliped = true;
						wo.TileLocation = newTile;
						break;
					
					case Direction.South:
						newTile = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.South));
						newTile.NorthEdge = wo;
						wo.Face(Direction.North,false);
						wo.fliped = true;
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
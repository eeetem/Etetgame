using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using CommonData;

namespace MultiplayerXeno
{
	public static class WorldEditSystem 
	{

		
		private static bool enabled = false;
		public static void Init()
		{
			enabled = true;

		}


		public static string ActivePrefab = "basicFloor";
		public static Direction ActiveDir = Direction.North;
		public static void GenerateUI()
		{
			if(!enabled) return;
			UI.EditorMenu();
			MouseManager.LeftClick += PlacePrefab;
			MouseManager.RightClick += DeletePrefab;



		}

		private static void DeletePrefab(Vector2Int Pos)
		{
			WorldTile tile = WorldManager.GetTileAtGrid(Pos);

			if (tile.ObjectAtLocation != null)
			{
				WorldManager.DeleteWorldObject(tile.ObjectAtLocation);
				return;
			}
			if (tile.NorthEdge != null)
			{
				WorldManager.DeleteWorldObject(tile.NorthEdge);
				return;
			}
			if (tile.WestEdge != null)
			{
				WorldManager.DeleteWorldObject(tile.WestEdge);
				return;
			}
			
			WorldTile southTile = WorldManager.GetTileAtGrid(Pos+WorldManager.DirToVec2(Direction.South));
			WorldTile eastTile = WorldManager.GetTileAtGrid(Pos+WorldManager.DirToVec2(Direction.East));
			if (eastTile.WestEdge != null)
			{
				WorldManager.DeleteWorldObject(eastTile.WestEdge);
				return;
			}
			if (southTile.NorthEdge != null)
			{
				WorldManager.DeleteWorldObject(southTile.NorthEdge);
				return;
			}

			
			
			
		}

		private static bool isValidPlacement(Vector2Int pos)
		{
			var tile = WorldManager.GetTileAtGrid(pos);
			WorldTile tile2;
			WorldObjectType type = PrefabManager.Prefabs[ActivePrefab];
			if (type.Surface)
			{
				return (tile.Surface == null);
			}
			else if (type.Edge)
			{
				switch (ActiveDir)
				{
					case Direction.North:
						return (tile.NorthEdge == null);
						break;
					case Direction.West:
						return (tile.WestEdge == null);
						break;
					case Direction.East:
						tile2 = WorldManager.GetTileAtGrid(tile.Position + WorldManager.DirToVec2(Direction.East));
						return (tile2.WestEdge == null);
						break;
					case Direction.South:
						tile2 = WorldManager.GetTileAtGrid(tile.Position + WorldManager.DirToVec2(Direction.South));
						return (tile2.NorthEdge == null);
						break;
					default:
						return false;
				}
			}
			else
			{
				return (tile.ObjectAtLocation == null);
			}
		}

		private static void PlacePrefab(Vector2Int Pos)
		{
			if(!enabled) return;
			if (!isValidPlacement(Pos)) return;
			WorldManager.MakeWorldObjectPublically(ActivePrefab,Pos,ActiveDir);
		}

		private static KeyboardState lastState;
		public static void Update(GameTime gameTime)
		{
			if(!enabled) return;
			
			var keyboardState = Keyboard.GetState();
			if (keyboardState.IsKeyDown(Keys.E) && lastState.IsKeyUp(Keys.E))
			{
				ActiveDir += 1;
			} else if (keyboardState.IsKeyDown(Keys.Q)&& lastState.IsKeyUp(Keys.Q))
			{
				ActiveDir -= 1;
			}

			lastState = keyboardState;


		}
	}
}
using System.Diagnostics;
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
			UI.LeftClick += PlacePrefab;
			UI.LeftClickUp += FinishPlacePrefab;
			UI.RightClick += DeletePrefab;

			for (int x = 0; x < 100; x++)
			{
				for (int y = 0; y < 100; y++)
				{
					WorldManager.Instance.MakeWorldObject(ActivePrefab,new Vector2Int(x,y),ActiveDir);
				}	
			}



		}

		private static void DeletePrefab(Vector2Int Pos)
		{
			WorldTile tile = WorldManager.Instance.GetTileAtGrid(Pos);

			if (tile.ObjectAtLocation != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.ObjectAtLocation);
				return;
			}
			if (tile.NorthEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.NorthEdge);
				return;
			}
			if (tile.WestEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.WestEdge);
				return;
			}
			
			WorldTile southTile = WorldManager.Instance.GetTileAtGrid(Pos+Utility.DirToVec2(Direction.South));
			WorldTile eastTile = WorldManager.Instance.GetTileAtGrid(Pos+Utility.DirToVec2(Direction.East));
			if (eastTile.WestEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(eastTile.WestEdge);
				return;
			}
			if (southTile.NorthEdge != null)
			{
				WorldManager.Instance.DeleteWorldObject(southTile.NorthEdge);
				return;
			}

			if (tile.Surface != null)
			{
				WorldManager.Instance.DeleteWorldObject(tile.Surface);
				return;
			}




		}

		private static bool IsValidPlacement(Vector2Int pos)
		{
			var tile = WorldManager.Instance.GetTileAtGrid(pos);
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
						tile2 = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.East));
						return (tile2.WestEdge == null);
						break;
					case Direction.South:
						tile2 = WorldManager.Instance.GetTileAtGrid(tile.Position + Utility.DirToVec2(Direction.South));
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

		private static bool placing = false;
		private static void PlacePrefab(Vector2Int Pos)
		{
			placing = true;
		}
		
		private static void FinishPlacePrefab(Vector2Int Pos)
		{
			placing = false;
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

			ActiveDir = Utility.NormaliseDir(ActiveDir);

			lastState = keyboardState;

			var Pos = Utility.WorldPostoGrid(Camera.GetMouseWorldPos());
			if (placing && IsValidPlacement(Pos))
			{
				WorldManager.Instance.MakeWorldObject(ActivePrefab,Pos,ActiveDir);
			}

		}
	}
}
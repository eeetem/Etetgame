using Microsoft.Xna.Framework;

namespace MultiplayerXeno
{
	public partial class WorldObject
	{
		
		
		
		public WorldObject(Vector2Int position, string prefabName)
		{
			Position = position;
			this.PrefabName = prefabName;
			DesiredPosition = position;
		}

		public readonly string PrefabName = "";
		public Vector2Int Position { get; private set; }

		public Vector2Int DesiredPosition { get; private set; }
		
		public void MoveTo(Vector2Int pos)
		{
			DesiredPosition = pos;

		}
		public void UpdatePosition()
		{
			Position = DesiredPosition;
		}

		
		public Cover SouthCover = Cover.None;
		public Cover NorthCover = Cover.None; 
		public Cover EastCover = Cover.None;
		public Cover WestCover = Cover.None;
		

		public enum Cover
		{
			None=0,//empty terain tiles
			Low=1,//small fences and such, visible when crouch
			High=2,//small walls and such, hidden when crouched
			Full=3,//full impassible walls
		}
	}
	public struct Vector2Int
	{
		public int X;
		public int Y;

		public Vector2Int(int x, int y)
		{
			X = x;
			Y = y;
		}
		public static implicit operator Vector2(Vector2Int vec) => new Vector2(vec.X,vec.Y);
	
		public static Vector2Int operator*(Vector2Int a, int b)
			=> new Vector2Int(a.X*b, a.Y*b);
		public static Vector2 operator+(Vector2 a, Vector2Int b)
			=> new Vector2(a.X+b.X, a.Y+b.Y);
		public static bool operator ==(Vector2Int lhs, Vector2Int rhs) => (lhs.X == rhs.X)&&(lhs.Y == rhs.Y);

		public static bool operator !=(Vector2Int lhs, Vector2Int rhs) => !(lhs == rhs);

		public override string ToString()
		{
			return "{X: " + X + " Y:" + Y + "}";
		}
	}
}
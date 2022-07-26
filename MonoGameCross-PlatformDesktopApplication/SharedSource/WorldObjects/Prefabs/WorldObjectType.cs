using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using MultiplayerXeno.Structs;

namespace MultiplayerXeno
{
	public partial class WorldObjectType
	{

		public readonly string TypeName;
		public WorldObjectType(string name,ControllableType? controllableType)
		{
			TypeName = name;
			if (controllableType != null)
			{
				controllable = controllableType;
			}
			
		}

		public Dictionary<Direction, Cover> Covers  = new Dictionary<Direction, Cover>();

		public readonly ControllableType? controllable;


		public bool Faceable { get; set; }
	}
}
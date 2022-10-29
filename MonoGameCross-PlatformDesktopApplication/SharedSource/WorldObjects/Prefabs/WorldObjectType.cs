using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;
using CommonData;


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
				Controllable = controllableType;
			}
			
		}

		
		public Cover Cover;

		public readonly ControllableType? Controllable;


		public bool Faceable { get; set; }
		public bool Edge { get; set; }
		public bool Surface { get; set; }
	}
}
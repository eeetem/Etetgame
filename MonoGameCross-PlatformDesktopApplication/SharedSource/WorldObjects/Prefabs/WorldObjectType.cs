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

		public void SpecialBehaviour(WorldObject objOfType)
		{
			switch (TypeName)
			{
				case "capturePoint":
					GameManager.CapturePoints.Add(objOfType);
					break;
				case "spawnPointT1":
#if SERVER
					GameManager.T1SpawnPoints.Add(objOfType);
#endif
				
					break;
				case "spawnPointT2":
#if SERVER
					GameManager.T2SpawnPoints.Add(objOfType);
#endif
					break;
			}
		}


		public Cover Cover = Cover.None;

		public readonly ControllableType? Controllable;

		//should probably be an enum
		public bool Faceable { get; set; }
		public bool Edge { get; set; }
		public bool Surface { get; set; }
	}
}
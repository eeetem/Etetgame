#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MultiplayerXeno.Structs;


namespace MultiplayerXeno
{
	public partial class WorldObject
	{

		public WorldObject(Vector2Int position, WorldObjectType type, int id)
		{

			this.Id = id;
			Position = position;
			this.Type = type;
#if CLIENT
			Transform = new Transform2();
#endif
		}

		public Controllable? ControllableComponent { get;  set; }

		public readonly int Id;


		public void Face(Direction dir)
		{
			if (!Type.Faceable)
			{
				return;
			}

			while (dir < 0)
			{
				dir += 8;
			}

			while (dir > (Direction) 7)
			{
				dir -= 8;
			}

			Facing = dir;
		}

		public void Update()
		{

			if (ControllableComponent != null)
			{
				ControllableComponent.Update();
			}
		}

		public readonly WorldObjectType Type;	
	
		public Direction Facing { get; private set;}

	

		public Vector2Int Position { get;  set; }
		

		public Cover GetCover(Direction dir)
		{
			Direction resulting = dir - (int) Facing;
			if (resulting < 0)
			{
				resulting += 8;
			}

			return Type.Covers[resulting];

		}



	}
	
}
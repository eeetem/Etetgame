#nullable enable
using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using MonoGame.Extended;



namespace MultiplayerXeno
{
	public partial class WorldObject
	{

		public WorldObject(WorldObjectType type, int id, WorldTile tileLocation)
		{

			this.Id = id;
			TileLocation = tileLocation;
			this.Type = type;
	

		}

		public WorldTile TileLocation;
		public Controllable? ControllableComponent { get;  set; }

		public readonly int Id;


		public void Move(Vector2Int position)
		{
			if (Type.Edge || Type.Surface)
			{
				throw new Exception("attempted to  move and  edge or surface");
			}

			TileLocation.ObjectAtLocation = null;
			var newTile = WorldManager.GetTileAtGrid(position);
			newTile.ObjectAtLocation = this;
			TileLocation = newTile;


		}

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

		public void Update(float gametime)
		{

			if (ControllableComponent != null)
			{
				ControllableComponent.Update(gametime);
			}
		}

		public readonly WorldObjectType Type;	
	
		public Direction Facing { get; private set;}


		public Cover GetCover()
		{

			return Type.Cover;

		}


		public WorldObjectData GetData()
		{
			WorldObjectData data = new WorldObjectData(Type.TypeName);
			data.Facing = this.Facing;
			data.Id = this.Id;
			if (ControllableComponent != null)
			{
				ControllableData cdata = new ControllableData(ControllableComponent.IsPlayerOneTeam);
				data.ControllableData = cdata;
			}

			return data;

		}



	}
	
}
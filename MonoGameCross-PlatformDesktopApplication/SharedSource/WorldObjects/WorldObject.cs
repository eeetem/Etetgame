#nullable enable
using System;
using CommonData;
using MonoGame.Extended;



namespace MultiplayerXeno
{
	public partial class WorldObject
	{

		public WorldObject(WorldObjectType? type, int id, WorldTile tileLocation)
		{
			this.Id = id;
			if (type == null)
			{
				type = new WorldObjectType("nullType",null);
				this.Type = type;
				return;
			}
			this.Type = type;


			TileLocation = tileLocation;
			Type.SpecialBehaviour(this);
#if CLIENT
			DrawTransform = new Transform2(type.Transform.Position, type.Transform.Rotation, type.Transform.Scale);
			this.spriteVariation = Random.Shared.Next(type.variations);
		
#endif
			this.Id = id;
			TileLocation = tileLocation;


	

		}

		private WorldTile _tileLocation;
		public WorldTile TileLocation
		{
			get => _tileLocation;

			set
			{
				_tileLocation = value;
#if CLIENT
				

				if (_tileLocation != null)
				{
					GenerateDrawOrder();
				}
#endif
			}
		}
		public Controllable? ControllableComponent { get;  set; }

		public readonly int Id;

		public void Move(Vector2Int position)
		{
			if (Type.Edge || Type.Surface)
			{
				throw new Exception("attempted to  move and  edge or surface");
			}
			TileLocation.ObjectAtLocation = null;
			var newTile = WorldManager.Instance.GetTileAtGrid(position);
			TileLocation = newTile;
			newTile.ObjectAtLocation = this;
			
#if CLIENT
			GenerateDrawOrder();
#endif
		}
		
		

		public bool fliped = false;//for display back texture


		

		public void Face(Direction dir,bool updateFOV  =true)
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
#if CLIENT
			if (updateFOV)
			{
				WorldManager.Instance.MakeFovDirty();
			}
#endif
			
		}
		public Visibility GetMinimumVisibility()
		{
			if (Type.Surface || Type.Edge)
			{
				return Visibility.None;
			}

			if (ControllableComponent != null && ControllableComponent.Crouching)
			{
				return Visibility.Full;
			}

			return Visibility.Partial;
		}

		public void TakeDamage(int dmg, int detResist)
		{
			
			Console.WriteLine(this + " got hit " + TileLocation.Position);
			if (dmg < 0)
			{
				return;
			}

			if (ControllableComponent != null)
			{//let controlable handle it
				ControllableComponent.TakeDamage(dmg, detResist);
				
			}
			else
			{
				//enviroment destruction
				if (TileLocation.ObjectAtLocation == null) return;

				var obj = TileLocation.ObjectAtLocation;

			
			}
		}

		public void TakeDamage(Projectile proj)
		{
			TakeDamage(proj.dmg,proj.determinationResistanceCoefficient);
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

			if (ControllableComponent != null && ControllableComponent.Crouching)
			{
				return Type.Cover - 1;
			}

			return Type.Cover;



		}


		public WorldObjectData GetData()
		{
			WorldObjectData data = new WorldObjectData(Type.TypeName);
			data.Facing = this.Facing;
			data.Id = this.Id;
			data.fliped = this.fliped;

			if (ControllableComponent != null)
			{
				ControllableData cdata = ControllableComponent.GetData();
				data.ControllableData = cdata;
			}

			return data;

		}
		protected bool Equals(WorldObject other)
		{
			return Id == other.Id;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((WorldObject) obj);
		}

		public override int GetHashCode()
		{
			return Id;
		}
		

	}
	
}
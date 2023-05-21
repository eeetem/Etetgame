#nullable enable
using System;
using System.Threading;
using MultiplayerXeno;
using MonoGame.Extended;



namespace MultiplayerXeno;

public partial class WorldObject
{

	public int LifeTime = -100;
	public WorldObject(WorldObjectType? type, WorldTile tile, WorldObjectData data)
	{
		Id = data.Id;
		
		if (type == null)
		{
			type = new WorldObjectType("nullType",null);
			Type = type;
			return;
		}
		Type = type;

		TileLocation = tile;
		if (data.Health == 0 || data.Health == -100)
		{
			Health = type.MaxHealth;
		}
		else
		{
			Health = data.Health;
		}
		if (data.Lifetime == -100 || data.Lifetime == 0)//this will cause issues
		{
			LifeTime = type.lifetime;
		}
		else
		{
			LifeTime = data.Lifetime;
		}



		Type.SpecialBehaviour(this);
#if CLIENT
		DrawTransform = new Transform2(type.Transform.Position, type.Transform.Rotation, type.Transform.Scale);
		spriteVariation = Random.Shared.Next(type.variations);
		
#endif
			

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

	public int Health;

	public void Move(Vector2Int position)
	{
		if (Type.Edge || Type.Surface)
		{
			throw new Exception("attempted to  move and  edge or surface");
		}

		if (ControllableComponent != null)
		{
			TileLocation.ControllableAtLocation = null;
			var newTile = WorldManager.Instance.GetTileAtGrid(position);
			TileLocation = newTile;
			newTile.ControllableAtLocation = this;
		}
		else
		{
			TileLocation.RemoveObject(this);
			var newTile = WorldManager.Instance.GetTileAtGrid(position);
			TileLocation = newTile;
			newTile.PlaceObject(this);
		}


			
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
		
	public void NextTurn()
	{
		if (LifeTime != -100)
		{
			LifeTime--;
			if (LifeTime <= 0)
			{
				Destroy();
					
			}
		}
	}



	bool destroyed = false;
	public void Destroy()
	{
		if(destroyed)return;
		destroyed = true;
		if(Type.DesturctionEffect != null)
		{
#if CLIENT
			Type.DesturctionEffect?.Animate(TileLocation.Position);
#endif
			Type.DesturctionEffect?.Apply(TileLocation.Position);
			Thread.Sleep(300);
		}

		WorldManager.Instance.DeleteWorldObject(this);
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
		if (LifeTime != -100)
		{
			LifeTime -= dmg;
			if (LifeTime <= 0)
			{
				Destroy();
			}
		}
		

		if (dmg < 0)
		{
			return;
		}
		Console.WriteLine(this + " got hit " + TileLocation.Position);
		if (ControllableComponent != null)
		{//let controlable handle it
			ControllableComponent.TakeDamage(dmg, detResist);
		}
		else
		{
			Health-= (dmg-detResist);
			if (Health <= 0)
			{
				WorldManager.Instance.DeleteWorldObject(this);
			}
		}
	}

	public void TakeDamage(Projectile proj)
	{
		TakeDamage(proj.dmg,proj.determinationResistanceCoefficient);
	}

	public void Update(float gametime)
	{
#if CLIENT
		PreviewData = new PreviewData();//probably very bad memory wise
#endif 
		if (ControllableComponent != null)
		{
			ControllableComponent.Update(gametime);
		}
	}

	public readonly WorldObjectType Type;	
	
	public Direction Facing { get; private set;}


	public Cover GetCover(bool visibileCover = false)
	{

		Cover cover = Type.SolidCover;
		if (visibileCover)
		{
			cover = Type.VisibilityCover;
		}
			
		if (ControllableComponent != null && ControllableComponent.Crouching)
		{
			return cover - 1;
		}

		return cover;
	}



	public WorldObjectData GetData()
	{
		WorldObjectData data = new WorldObjectData(Type.TypeName);
		data.Facing = Facing;
		data.Id = Id;
		data.fliped = fliped;
		data.Health = Health;

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
		if (obj.GetType() != GetType()) return false;
		return Equals((WorldObject) obj);
	}

	public override int GetHashCode()
	{
		return Id;
	}
		
	public bool IsVisible()
	{
#if SERVER
		return true;	
#else
		if (TileLocation == null)
		{
			return true;
		}

		return GetMinimumVisibility() <= TileLocation.Visible;
#endif
	}

		

}
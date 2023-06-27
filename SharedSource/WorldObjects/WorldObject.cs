#nullable enable
using System;
using System.Threading.Tasks;
using MonoGame.Extended;
using Riptide;

#if CLIENT
using MultiplayerXeno.UILayouts;

#endif


namespace MultiplayerXeno;

public partial class WorldObject
{

	
	
	public int LifeTime = -100;
	public WorldObject(WorldObjectType? type, WorldTile tile, WorldObjectData data)
	{
		ID = data.ID;
		
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
		spriteVariation = (tile.Position.X + tile.Position.Y + ID )% type.variations;
		
#endif
			
		Health = Math.Clamp(Health, 0, type.MaxHealth);
	}


	private WorldTile _tileLocation = null!;
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
	public Unit? UnitComponent { get;  set; }

	public readonly int ID;

	public int Health;

	public void Move(Vector2Int position)
	{
		if (Type.Edge || Type.Surface)
		{
			throw new Exception("attempted to  move and  edge or surface");
		}

		if (UnitComponent != null)
		{
			TileLocation.UnitAtLocation = null;
			var newTile = WorldManager.Instance.GetTileAtGrid(position);
			TileLocation = newTile;
			newTile.UnitAtLocation = UnitComponent;
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

		dir = Utility.ClampFacing(dir);

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



	public bool destroyed { get; private set; } = false;

	public void Destroy()
	{
		if(destroyed)return;
		destroyed = true;
		
		
		if(Type.DesturctionEffect != null)
		{
			//wrap this in a task

			Task t = new Task(delegate
			{
#if CLIENT
			Type.DesturctionEffect?.Animate(TileLocation.Position);
#endif
				Type.DesturctionEffect?.Apply(TileLocation.Position,this);
			});
			WorldManager.Instance.RunNextFrame(t);

		}
		
#if CLIENT
			if(Equals(GameLayout.SelectedUnit, UnitComponent)){
				GameLayout.SelectUnit(null);
			}
		
		Console.WriteLine("Destroyed "+ID +" "+Type.TypeName);
#else
		

#endif
		
#if SERVER
WorldManager.Instance.DeleteWorldObject(this);
#endif

		Console.WriteLine("Destroyed "+ID +" "+Type.TypeName);

	}
	public Visibility GetMinimumVisibility()
	{
		if (Type.Surface || Type.Edge)
		{
			return Visibility.None;
		}

		if (UnitComponent != null && UnitComponent.Crouching)
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
		if (UnitComponent != null)
		{//let controlable handle it
			UnitComponent.TakeDamage(dmg, detResist);
		}
		else
		{
			Health-= dmg-detResist;
			if (Health <= 0)
			{
				Destroy();
			}
		}
	}

	public void TakeDamage(Projectile proj)
	{
		TakeDamage(proj.Dmg,proj.DeterminationResistanceCoefficient);
	}

	public void Update(float gametime)
	{
#if CLIENT
		OverRideColor = null;
		PreviewData = new PreviewData();//probably very bad memory wise
#endif 
		if (UnitComponent != null)
		{
			UnitComponent.Update(gametime);
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
			
		if (UnitComponent != null && UnitComponent.Crouching)
		{
			return cover - 1;
		}

		return cover;
	}

	public struct WorldObjectData : IMessageSerializable
	{
		public Direction Facing;
		public int ID;

		public bool Fliped;
		//health
		public string Prefab;
		public Unit.UnitData? UnitData;
		public int Health;
		public int Lifetime;
		public WorldObjectData(string prefab)
		{
			Prefab = prefab;
			ID = -1;
			Facing = Direction.North;
			UnitData = null;
			Fliped = false;
			Health = -100;
			Lifetime = -100;
		}

		public void Serialize(Message message)
		{
			message.AddString(Prefab);
			message.AddInt(ID);
			message.AddInt((int)Facing);
			message.AddBool(Fliped);
			message.Add(Health);
			message.Add(Lifetime);
			message.AddBool(UnitData != null);
			if (UnitData != null)
			{
				message.AddSerializable(UnitData.Value);
			}
		}

		public void Deserialize(Message message)
		{
			Prefab = message.GetString();
			ID = message.GetInt();
			Facing = (Direction)message.GetInt();
			Fliped = message.GetBool();
			Health = message.GetInt();
			Lifetime = message.GetInt();
			bool hasUnit = message.GetBool();
			if (hasUnit)
			{
				UnitData = message.GetSerializable<Unit.UnitData>();
			}
			else
			{
				UnitData = null;
			}
		}
	}

	public WorldObjectData GetData()
	{
		WorldObjectData data = new WorldObjectData(Type.TypeName);
		data.Facing = Facing;
		data.ID = ID;
		data.Fliped = fliped;
		data.Health = Health;

		if (UnitComponent != null)
		{
			Unit.UnitData cdata = UnitComponent.GetData();
			data.UnitData = cdata;
		}

		return data;

	}
	protected bool Equals(WorldObject other)
	{
		return ID == other.ID;
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
		return ID;
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
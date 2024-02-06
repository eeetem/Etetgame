#nullable enable
using System;
using DefconNull.ReplaySequence.WorldObjectActions;
using MonoGame.Extended;
using Riptide;
#if CLIENT
using DefconNull.Rendering;
using DefconNull.Rendering.UILayout.GameLayout;
#endif


namespace DefconNull.WorldObjects;

public partial class WorldObject
{

	public int LifeTime = -100;
	public WorldObject(WorldObjectType? type, IWorldTile? tile, WorldObjectData data)
	{
		ID = data.ID;
		if (type == null)
		{
			type = new WorldObjectType("nullType");
			Type = type;
			return;
		}
		Type = type;
		TileLocation = tile;
		SetData(data);
		Type.SpecialBehaviour(this);
#if CLIENT
		DrawTransform = new Transform2(type.Transform.Position, type.Transform.Rotation, type.Transform.Scale);
		int seed = ID;
		if (tile != null)
		{
			seed = (int)(tile.Position.X + tile.Position.Y + ID);
		}
		
		spriteVariation = Type.GetRandomVariationIndex(seed);
#endif
			
		Health = Math.Clamp(Health, 0, type.MaxHealth);
	}


	private IWorldTile _tileLocation = null!;
	public IWorldTile TileLocation
	{
		get => _tileLocation;
		set
		{
			if(value == null)
				throw new Exception("Tile location cannot be null");
			_tileLocation = value;
		}
	}
	public Unit? UnitComponent { get;  set; }

	public readonly int ID;

	public int Health;

	

	public bool Fliped = false;//for display back texture


		

	public void Face(Direction dir,bool updateFov  =true)
	{
		if (!Type.Faceable)
		{
			return;
		}

		dir = Utility.ClampFacing(dir);

		Facing = dir;

		if (updateFov)
		{
			WorldManager.Instance.MakeFovDirty();
		}

			
	}
		
	public void NextTurn()
	{
		if (LifeTime != -100)
		{
			LifeTime--;
			if (LifeTime <= 0)
			{
				WorldObjectManager.Destroy(this);
					
			}
		}
	}



	public bool destroyed { get; set; } = false;

	
	public Visibility GetMinimumVisibility(bool dontautoRevealEdges = false)
	{
		if (Type.Surface)
		{
			return Visibility.None;
		}

		if (Type.Edge)
		{
			if (!dontautoRevealEdges)
			{
				return Visibility.None;
			}
			return Visibility.Partial;
		}

		if (UnitComponent != null && UnitComponent.Crouching)
		{
			return Visibility.Full;
		}

		return Visibility.Partial;
	}

	


	public void Update(float msDelta)
	{
#if CLIENT
		PreviewData = new PreviewData();//probably very bad memory wise
		AnimationUpdate(msDelta);
#endif 
		if (UnitComponent != null)
		{
			UnitComponent.Update(msDelta);
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
		public string Prefab ="";
		
        public bool Fliped;
        
		public Unit.UnitData? UnitData;
		public int Health;
		public int Lifetime;
		public bool JustSpawned;
		public WorldObjectData(string prefab)
		{
			Prefab = prefab;
			ID = -1;
			Facing = Direction.North;
			UnitData = null;
			Fliped = false;
			Health = -100;
			Lifetime = -100;
			JustSpawned = true;
		}

		public void Serialize(Message message)
		{
			message.AddString(Prefab);
			message.AddInt(ID);
			message.AddInt((int)Facing);
			message.AddBool(Fliped);
			message.Add(Health);
			message.Add(Lifetime);
			message.AddBool(JustSpawned);
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
			JustSpawned = message.GetBool();
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

		public string GetHash()
		{
			return Prefab + ID + Health;
		}

		public override string ToString()
		{
			return $"{nameof(Facing)}: {Facing}, {nameof(ID)}: {ID}, {nameof(Prefab)}: {Prefab}, {nameof(Fliped)}: {Fliped}, {nameof(UnitData)}: {UnitData}, {nameof(Health)}: {Health}, {nameof(Lifetime)}: {Lifetime}, {nameof(JustSpawned)}: {JustSpawned}";
		}
	}

	public WorldObjectData GetData(bool forceJustSpawned = false)
	{
		WorldObjectData data = new WorldObjectData(Type.Name);
		data.Facing = Facing;
		data.ID = ID;
		data.Fliped = Fliped;
		data.Health = Health;
		data.Lifetime = LifeTime;
		data.JustSpawned = forceJustSpawned;
		if (UnitComponent != null)
		{
			Unit.UnitData cdata = UnitComponent.GetData();
			data.UnitData = cdata;
		}

		return data;

	}

	protected bool Equals(WorldObject other)
	{
		return LifeTime == other.LifeTime && _tileLocation.Equals(other._tileLocation) && ID == other.ID && Health == other.Health && Fliped == other.Fliped && Type.Equals(other.Type) && Equals(UnitComponent, other.UnitComponent) && destroyed == other.destroyed && Facing == other.Facing;
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
		unchecked
		{
			int hashCode = LifeTime;
			hashCode = (hashCode * 397) ^ _tileLocation.GetHashCode();
			hashCode = (hashCode * 397) ^ ID;
			hashCode = (hashCode * 397) ^ Health;
			hashCode = (hashCode * 397) ^ Fliped.GetHashCode();
			hashCode = (hashCode * 397) ^ Type.GetHashCode();
			hashCode = (hashCode * 397) ^ (UnitComponent != null ? UnitComponent.GetHashCode() : 0);
			hashCode = (hashCode * 397) ^ destroyed.GetHashCode();
			hashCode = (hashCode * 397) ^ (int) Facing;
			return hashCode;
		}
	}

	public bool IsVisible()
	{
		return GetMinimumVisibility() <=  ((WorldTile)TileLocation).GetVisibility();
	}
	public bool ShouldBeVisibilityUpdated(bool team1)
	{
		var vis =  ((WorldTile)TileLocation).GetVisibility(team1);
		
		if(Type.Edge)
		{
			Visibility vis2;
			WorldTile t;
			switch (Facing)
			{
				case Direction.North:
					t = WorldManager.Instance.GetTileAtGrid(TileLocation.Position + new Vector2Int(0, -1));
					vis2 = t.GetVisibility(team1);
					if(vis2>vis) vis = vis2;
					break;
					
				case Direction.West:
					t = WorldManager.Instance.GetTileAtGrid(TileLocation.Position + new Vector2Int(-1, 0));
					vis2 = t.GetVisibility(team1);
					if(vis2>vis) vis = vis2;
					break;
			}
		}

		return GetMinimumVisibility(true) <= vis;
	}
	


	public void SetData(WorldObjectData data)
	{
		if(data.ID != -1 && data.ID != ID)
			throw new Exception("Data set ID mismatch");
		Face(data.Facing,false);
		Fliped = data.Fliped;
		if (data.JustSpawned)
		{
			Health = Type.MaxHealth;
#if CLIENT
			StartAnimation("start");
#endif
		}
		else
		{
			Health = data.Health;
		}
		if (data.JustSpawned)
		{
			LifeTime = Type.lifetime;
		}
		else
		{
			LifeTime = data.Lifetime;
		}

		if (data.UnitData != null)
		{
			if (UnitComponent != null)
			{
				UnitComponent.SetData(data.UnitData.Value, data.JustSpawned);
			}
			else
			{
				UnitComponent = new Unit(this, (UnitType)Type, data.UnitData.Value, data.JustSpawned);
			}}
		else
		{
			UnitComponent = null;
		}
		
	}
}
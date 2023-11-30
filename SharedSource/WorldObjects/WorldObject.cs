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
	public WorldObject(WorldObjectType? type, IWorldTile tile, WorldObjectData data)
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
		if (data.JustSpawned)
		{
			Health = type.MaxHealth;
		}
		else
		{
			Health = data.Health;
		}
		if (data.JustSpawned)//this will cause issues
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
		
		var r = new Random(tile.Position.X + tile.Position.Y +ID);
		int roll = (r.Next(1000)) % type.TotalVariationsWeight;
		for (int i = 0; i < type.Variations.Count; i++)
		{
			if (roll < type.Variations[i].Item2)
			{
				spriteVariation = i;
				break;
			}
			roll -= type.Variations[i].Item2;
		
		}

		if (spriteVariation == null)
		{
			throw new Exception("failed to generate sprite variation");
		}

#endif
			
		Health = Math.Clamp(Health, 0, type.MaxHealth);
	}


	private IWorldTile _tileLocation = null!;
	public IWorldTile TileLocation
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
			var newTile = WorldManager.Instance.GetTileAtGrid(position);
			TileLocation = newTile;
			newTile.UnitAtLocation = UnitComponent;
		}
		else
		{
			TileLocation.RemoveObject(this);
			IWorldTile newTile = WorldManager.Instance.GetTileAtGrid(position);
			TileLocation = newTile;
			newTile.PlaceObject(this);
		}


			
#if CLIENT
		GenerateDrawOrder();
#endif
	}
		
		

	public bool fliped = false;//for display back texture


		

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

	


	public void Update(float gametime)
	{
#if CLIENT
		PreviewData = new PreviewData();//probably very bad memory wise
		AnimationUpdate(gametime);
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
	}

	public WorldObjectData GetData(bool forceJustSpawned = false)
	{
		WorldObjectData data = new WorldObjectData(Type.Name);
		data.Facing = Facing;
		data.ID = ID;
		data.Fliped = fliped;
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
		return GetMinimumVisibility() <=  ((WorldTile)TileLocation).GetVisibility();
	}
	public bool IsVisible(bool team1)
	{
		return GetMinimumVisibility() <=  ((WorldTile)TileLocation).GetVisibility(team1);
	}


	public string GetHash()
	{
		return Type.Name + ID;
	}
}
using CommonData;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno;

public class Fire : Action
{
	public Fire() :base(ActionType.Attack)
	{
	}

	
	public override bool CanPerform(Controllable actor, Vector2Int position)
	{
		
		if (position == actor.worldObject.TileLocation.Position)
		{
			return false;
		}

		if (actor.ActionPoints <= 0)
		{
			return false;
		}
		if (actor.MovePoints <= 0)
		{
			return false;
		}

		return true;

	}

	protected override void Execute(Controllable actor,Vector2Int target)
	{

			actor.ActionPoints--;
			actor.Awareness--;
			actor.MovePoints--;

			
		//client shouldnt be allowed to judge what got hit
		//fire packet just makes the unit "shoot"
		//actual damage and projectile is handled elsewhere
#if SERVER
			bool lowShot = false;
			if (actor.Crouching)
			{
				lowShot = true;
			}else
			{
				WorldTile tile = WorldManager.Instance.GetTileAtGrid(target);
				if (tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent != null && tile.ObjectAtLocation != null && tile.ObjectAtLocation.ControllableComponent.Crouching)
				{
					lowShot = true;
				}
			}
			Vector2 shotDir = Vector2.Normalize(target - actor.worldObject.TileLocation.Position);
			Projectile p = new Projectile(actor.worldObject.TileLocation.Position+new Vector2(0.5f,0.5f)+(shotDir/new Vector2(2.5f,2.5f)),target+new Vector2(0.5f,0.5f),actor.Type.WeaponDmg,actor.Type.WeaponRange,lowShot);
			p.Fire();
			Networking.DoAction(new ProjectilePacket(p.result,p.covercast,actor.Type.WeaponDmg,p.dropoffRange));

#endif
#if CLIENT
		Camera.SetPos(target);
		Controllable.Targeting = false;
		if (actor.Type.WeaponRange > 6)
		{
			ObjectSpawner.Burst(actor.worldObject.TileLocation.Position, target);
		}
		else
		{
			ObjectSpawner.ShotGun(actor.worldObject.TileLocation.Position,target);	
		}
#endif
		actor.worldObject.Face(Utility.ToClampedDirection( actor.worldObject.TileLocation.Position-target));

	}




}


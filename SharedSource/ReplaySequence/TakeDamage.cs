using System.Collections.Generic;
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class TakeDamage : SequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return	SequenceType.TakeDamage;
	}

	public override bool CanBatch => true;
	public int Dmg;
	public int DetResistance;
	public int ObjID = -1;
	public Vector2Int Position = new Vector2Int(-1,-1);
	private List<string> Ignores = new List<string>();

	public static TakeDamage Make(int dmg, int detResistance,Vector2Int position, List<string> ignores) 
	{
		var t = GetAction(SequenceType.TakeDamage) as TakeDamage;
		t.Dmg = dmg;
		t.DetResistance = detResistance;
		t.Position = position;
		t.Ignores = ignores;
		return t;
	}
	


	public static TakeDamage Make(int dmg, int detResistance, int objID)
	{
		var t = GetAction(SequenceType.TakeDamage) as TakeDamage;
		t.Dmg = dmg;
		t.DetResistance = detResistance;
		t.ObjID = objID;
		return t;
	}

	public override bool ShouldDo()
	{

		if (ObjID != -1) return true;
		if (Position == new Vector2Int(-1, -1)) return false;
		var tile = WorldManager.Instance.GetTileAtGrid(Position);
		if(tile.UnitAtLocation == null) return false;
		var obj = tile.UnitAtLocation;
		if(Ignores.Contains(obj!.Type.Name )) return false;

		return true;
	}

	public WorldObject? GetTargetObject()
	{
		if (ObjID != -1)
		{
			return WorldManager.Instance.GetObject(ObjID);
		}
		else
		{
			var u = WorldManager.Instance.GetTileAtGrid(Position).UnitAtLocation;
			if (u is null) return null;
			if (Ignores.Contains(u.Type.Name)) return null;
			return u.WorldObject;
		}

		return null;
	}

	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
	
			GetTargetObject()?.TakeDamage(Dmg, DetResistance);
			
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(Dmg);
		message.Add(DetResistance);
		message.Add(ObjID);
	}

	protected override void DeserializeArgs(Message args)
	{
		Dmg = args.GetInt();
		DetResistance = args.GetInt();
		ObjID = args.GetInt();
	}

#if CLIENT
	public override void DrawDesc(Vector2 pos, SpriteBatch batch)
	{
		if (GetTargetObject().UnitComponent == null || GetTargetObject().UnitComponent.Determination > 0)
		{
			batch.DrawText("DMG :" + Dmg+"("+(Dmg-DetResistance)+")", pos, Color.White);
			return;
		}
		batch.DrawText("DMG : " + Dmg, pos, Color.White);
	}

	protected override void Preview(SpriteBatch spriteBatch)
	{
		if(Dmg == 0 || ObjID == -1)
			return;

		
		var obj = WorldManager.Instance.GetObject(ObjID);

		if (obj !=null && obj!.IsVisible())
		{
			obj!.PreviewData.totalDmg += Dmg;


			Texture2D sprite = obj.GetTexture();
			spriteBatch.Draw(sprite, obj.GetDrawTransform().Position, Color.Red * 0.8f);


			//this is scuffed
			if (obj.UnitComponent == null || obj.UnitComponent.Determination > 0)
			{
				WorldManager.Instance.GetObject(ObjID)!.PreviewData.finalDmg += Dmg - DetResistance;
				WorldManager.Instance.GetObject(ObjID)!.PreviewData.determinationBlock += DetResistance;
			}
			else
			{
				WorldManager.Instance.GetObject(ObjID)!.PreviewData.finalDmg += Dmg;
			}
		}

	}
#endif



}
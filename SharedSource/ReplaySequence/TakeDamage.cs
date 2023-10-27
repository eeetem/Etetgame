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
	public override bool CanBatch => true;
	public readonly int Dmg;
	public readonly int DetResistance;
	public readonly int ObjID = -1;
	public Vector2Int Position = new Vector2Int(-1,-1);
	private readonly List<string> Ignores = new List<string>();
	public TakeDamage(int dmg, int detResistance, int objID) : base(SequenceType.TakeDamage)
	{
		this.Dmg = dmg;
		this.DetResistance = detResistance;
		this.ObjID = objID;
	}
	public TakeDamage(int dmg, int detResistance,Vector2Int position, List<string> ignores) : base(SequenceType.TakeDamage)
	{
		this.Dmg = dmg;
		this.DetResistance = detResistance;
		this.Position = position;
		this.Ignores = ignores;
	}
	
	public TakeDamage(Message args) : base(SequenceType.TakeDamage)
	{
		Dmg = args.GetInt();
		DetResistance = args.GetInt();
		ObjID = args.GetInt();
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

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			if (ObjID != -1)
			{
				WorldManager.Instance.GetObject(ObjID)!.TakeDamage(Dmg, DetResistance);
			}
			else
			{
				var u =WorldManager.Instance.GetTileAtGrid(Position).UnitAtLocation!;
                if(Ignores.Contains(u.Type.Name)) return ;
				u.TakeDamage(Dmg, DetResistance);
			}
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(Dmg);
		message.Add(DetResistance);
		message.Add(ObjID);
	}
	
#if CLIENT
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
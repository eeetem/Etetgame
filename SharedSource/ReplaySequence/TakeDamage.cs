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
	public readonly int dmg;
	public readonly int detResistance;
	public int objID = -1;
	public Vector2Int position = new Vector2Int(-1,-1);
	private List<string> Ignores = new List<string>();
	public TakeDamage(int dmg, int detResistance, int objID) : base(SequenceType.TakeDamage)
	{
		this.dmg = dmg;
		this.detResistance = detResistance;
		this.objID = objID;
	}
	public TakeDamage(int dmg, int detResistance,Vector2Int position, List<string> ignores) : base(SequenceType.TakeDamage)
	{
		this.dmg = dmg;
		this.detResistance = detResistance;
		this.position = position;
		this.Ignores = ignores;
	}
	
	public TakeDamage(Message args) : base(SequenceType.TakeDamage)
	{
		dmg = args.GetInt();
		detResistance = args.GetInt();
		objID = args.GetInt();
	}

	public override bool ShouldDo()
	{

		if (objID != -1) return true;
		if (position == new Vector2Int(-1, -1)) return false;
		var tile = WorldManager.Instance.GetTileAtGrid(position);
		if(tile.UnitAtLocation == null) return false;
		var obj = tile.UnitAtLocation;
		if(Ignores.Contains(obj!.Type.Name )) return false;

		return true;
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			if (objID != -1)
			{
				WorldManager.Instance.GetObject(objID)!.TakeDamage(dmg, detResistance);
			}
			else
			{
				var u =WorldManager.Instance.GetTileAtGrid(position).UnitAtLocation!;
                if(Ignores.Contains(u.Type.Name)) return ;
				u.TakeDamage(dmg, detResistance);
			}


		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(dmg);
		message.Add(detResistance);
		message.Add(objID);
	}
	
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		if(dmg == 0 || objID == -1)
			return;

		
		var obj = WorldManager.Instance.GetObject(objID);

		if (obj !=null && obj!.IsVisible())
		{
			obj!.PreviewData.totalDmg += dmg;


			Texture2D sprite = obj.GetTexture();
			spriteBatch.Draw(sprite, obj.GetDrawTransform().Position, Color.Red * 0.8f);


			//this is scuffed
			if (obj.UnitComponent == null || obj.UnitComponent.Determination > 0)
			{
				WorldManager.Instance.GetObject(objID)!.PreviewData.finalDmg += dmg - detResistance;
				WorldManager.Instance.GetObject(objID)!.PreviewData.determinationBlock += detResistance;
			}
			else
			{
				WorldManager.Instance.GetObject(objID)!.PreviewData.finalDmg += dmg;
			}
		}

	}
#endif



}
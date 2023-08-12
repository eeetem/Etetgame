using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.WorldObjects.Units.ReplaySequence;

public class TakeDamage : SequenceAction
{
	public override bool CanBatch => true;
	private int dmg;
	private int detResistance;
	private int objID;
	
	public TakeDamage(int dmg, int detResistance, int objID) : base(SequenceType.TakeDamage)
	{
		this.dmg = dmg;
		this.detResistance = detResistance;
		this.objID = objID;
	}
	
	public TakeDamage(Message args) : base(SequenceType.TakeDamage)
	{
		dmg = args.GetInt();
		detResistance = args.GetInt();
		objID = args.GetInt();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			WorldManager.Instance.GetObject(objID)!.TakeDamage(dmg, detResistance);
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
	public override void Preview(SpriteBatch spriteBatch)
	{
		if(dmg == 0)
			return;
		
		var obj = WorldManager.Instance.GetObject(objID);
		obj!.PreviewData.totalDmg += dmg;
		
		
		Texture2D sprite = obj.GetTexture();
		spriteBatch.Draw(sprite, obj.GetDrawTransform().Position, Color.Red * 0.8f);
		
		
		//this is scuffed
		if (obj.UnitComponent == null || obj.UnitComponent.Determination > 0)
		{
			WorldManager.Instance.GetObject(objID)!.PreviewData.finalDmg += dmg-detResistance;
			WorldManager.Instance.GetObject(objID)!.PreviewData.determinationBlock += detResistance;
		}
		else
		{
			WorldManager.Instance.GetObject(objID)!.PreviewData.finalDmg += dmg;
		}


	}
#endif
}
using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
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

	protected override Task GenerateTask()
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
}
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public abstract class SequenceAction : IMessageSerializable
{
	public enum SequenceType
	{
		
		PlaySound =100,
		PostProcessingEffect =101,
		TakeDamage = 102,
		UpdateTile =103,
		MakeWorldObject =104,
		
		ChangeUnitValues =0,
		GiveItem =1,
		Move=2,
		Face=3,
		Crouch=4,
		UseItem = 5,
		SelectItem = 6,
		Suppress = 7,
		PlayAnimation =8,
		Overwatch = 9,
		UnitStatusEffect =10,
		
	}
	public readonly SequenceType SqcType;
	public bool IsUnitAction => (int) SqcType < 100;
	public virtual bool CanBatch => false;
	public SequenceAction(SequenceType tp)
	{
		SqcType = tp;
	}

	public virtual bool ShouldDo()
	{
		return true;
	}


	public abstract Task GenerateTask();


	protected abstract void SerializeArgs(Message message);	

	public void Serialize(Message message)
	{
		SerializeArgs(message);
	}

	public void Deserialize(Message message)
	{
		throw new Exception("cannot deserialize abstract SequenceAction");
	}
#if CLIENT
	public virtual void Preview(SpriteBatch spriteBatch){}
#endif
}
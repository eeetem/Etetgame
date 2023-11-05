using System.Threading.Tasks;
using DefconNull.World;
using DefconNull.World.WorldObjects;
using DefconNull.World.WorldObjects.Units.ReplaySequence;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.SharedSource.Units.ReplaySequence;

public class Suppress : WorldObjects.Units.ReplaySequence.UnitSequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.Suppress;
	}

	public override bool CanBatch => true;
	public int detDmg;
	


	public static Suppress Make(int detDmg, int actorID) 
	{
		Suppress t = GetAction(SequenceType.Suppress) as Suppress;
		t.detDmg = detDmg;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	public static Suppress Make(int detDmg, Vector2Int actorID)
	{
		Suppress t = GetAction(SequenceType.Suppress) as Suppress;
		t.detDmg = detDmg;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	public static Suppress Make(int detDmg, TargetingRequirements req) 
	{
		Suppress t = GetAction(SequenceType.Suppress) as Suppress;
		t.detDmg = detDmg;
		t.Requirements = req;
		return t;
	}


	protected override void SerializeArgs(Message message)
	{
		
		base.SerializeArgs(message);
		message.Add(detDmg);
	}

	protected override void DeserializeArgs(Message msg)
	{
		base.DeserializeArgs(msg);
		detDmg = msg.GetInt();
	}


	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			Actor.Suppress(detDmg);
		});
		return t;
	}
	
#if CLIENT
	protected override void Preview(SpriteBatch spriteBatch)
	{
		if(!ShouldDo())return;
		if (Actor.WorldObject.IsVisible())
		{
			Texture2D sprite = Actor.WorldObject.GetTexture();
			spriteBatch.Draw(sprite, Actor.WorldObject.GetDrawTransform().Position, Color.Blue * 0.8f);
			Actor.WorldObject.PreviewData.detDmg += detDmg;
		}
	}
#endif


	
}
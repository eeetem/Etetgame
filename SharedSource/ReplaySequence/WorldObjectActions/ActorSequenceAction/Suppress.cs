using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class Suppress : UnitSequenceAction
{
	public override SequenceType GetSequenceType()
	{
		return SequenceType.Suppress;
	}

	public override BatchingMode Batching => BatchingMode.OnlySameType;
	public int DetDmg;
	


	public static Suppress Make(int detDmg, int actorID) 
	{
		Suppress t = (GetAction(SequenceType.Suppress) as Suppress)!;
		t.DetDmg = detDmg;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	public static Suppress Make(int detDmg, Vector2Int actorID)
	{
		Suppress t = (GetAction(SequenceType.Suppress) as Suppress)!;
		t.DetDmg = detDmg;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	public static Suppress Make(int detDmg, TargetingRequirements req) 
	{
		Suppress t = (GetAction(SequenceType.Suppress) as Suppress)!;
		t.DetDmg = detDmg;
		t.Requirements = req;
		return t;
	}


	protected override void SerializeArgs(Message message)
	{
		
		base.SerializeArgs(message);
		message.Add(DetDmg);
	}

	protected override void DeserializeArgs(Message msg)
	{
		base.DeserializeArgs(msg);
		DetDmg = msg.GetInt();
	}


	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
			Actor.Suppress(DetDmg);
		});
		return t;
	}
	
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		if (Requirements.Position != new Vector2Int(-1, -1))
		{
			var t = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
			Texture2D sprite = t.Surface.GetTexture();
			spriteBatch.Draw(sprite, t.Surface.GetDrawTransform().Position, Color.Blue * 0.1f);
			spriteBatch.DrawOutline(new List<WorldTile>(){t}, Color.Blue, 0.5f);
		}
		if(!ShouldDo())return;
		if (Actor.WorldObject.IsVisible())
		{
			Texture2D sprite = Actor.WorldObject.GetTexture();
			spriteBatch.Draw(sprite, Actor.WorldObject.GetDrawTransform().Position, Color.Blue * 0.8f);
			Actor.WorldObject.PreviewData.detDmg += DetDmg;
		}

		

		
	}
#endif


	
}
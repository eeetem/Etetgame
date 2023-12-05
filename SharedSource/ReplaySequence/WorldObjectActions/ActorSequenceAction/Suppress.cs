using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DefconNull.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

#if CLIENT
using DefconNull.Rendering;
#endif

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


	protected override void RunSequenceAction()
	{
		
		if(DetDmg==0) return;
		Console.WriteLine("Suppressing: "+Actor.WorldObject.ID + " by "+DetDmg);
#if CLIENT
		
		new PopUpText("\nSupression: " + DetDmg, Actor.WorldObject.TileLocation.Position, Color.Blue, 0.8f);
#endif
		Actor.Determination-= DetDmg;
		if (Actor.Determination <= 0)
		{
			Actor.Panic();
		}

		if (Actor.Paniced && Actor.Determination > 0)
		{
			Actor.Paniced = false;
		}
		if(Actor.Determination>Actor.Type.Maxdetermination) Actor.Determination.Current = Actor.Type.Maxdetermination;
		if(Actor.Determination<0) Actor.Determination.Current = 0;
		
		Console.WriteLine("Suppression result: "+Actor.WorldObject.ID + " has "+Actor.Determination.Current);

	}
	
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		if (Requirements.Position != new Vector2Int(-1, -1))
		{
			var t = WorldManager.Instance.GetTileAtGrid(Requirements.Position);
			if(t.Surface is null)return;
			Texture2D sprite = t.Surface.GetTexture();
			spriteBatch.Draw(sprite, t.Surface.GetDrawTransform().Position, Color.Blue * 0.2f);
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
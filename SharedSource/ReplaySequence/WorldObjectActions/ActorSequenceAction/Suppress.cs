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
	public ushort DetDmg;

	protected bool Equals(Suppress other)
	{
		return base.Equals(other) && DetDmg == other.DetDmg;
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((Suppress) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (base.GetHashCode() * 397) ^ DetDmg;
		}
	}


	public static Suppress Make(ushort detDmg, int actorID) 
	{
		Suppress t = (GetAction(SequenceType.Suppress) as Suppress)!;
		t.DetDmg = detDmg;
		t.Requirements = new TargetingRequirements(actorID);
		return t;
	}
	public static Suppress Make(ushort detDmg, Vector2Int actorID)
	{
		Suppress t = (GetAction(SequenceType.Suppress) as Suppress)!;
		t.DetDmg = detDmg;
		t.Requirements = new TargetingRequirements(actorID);

		return t;
	}
	public static Suppress Make(ushort detDmg, TargetingRequirements req) 
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
		DetDmg = msg.GetUShort();
	}


	protected override void RunSequenceAction()
	{
		
		if(DetDmg==0) return;
		Log.Message("UNITS","Suppressing: "+Actor.WorldObject.ID + " by "+DetDmg);
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

	public override void DrawConsequence(Vector2 pos, SpriteBatch batch)
	{
		
		var arrowSprite = TextureManager.GetTexture("HoverHud/Consequences/downArrow");

		Vector2 offset = new Vector2(arrowSprite.Width-2,0);
		batch.Draw(arrowSprite, pos+offset*3, Color.White);
		batch.DrawNumberedIcon(DetDmg.ToString(),TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"),pos+offset*3+new Vector2(10,0),Color.White);
		if (Actor.WorldObject.PreviewData.detDmg >= Actor.Determination)
		{
			batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/panic"),pos+offset*2,Color.White);
		}
	}

	public override void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
	{
		string str = "           Suppression:\n" +
		             "  [Green]Amount[-] to be subtracted from determination\n";
		if (Actor.WorldObject.PreviewData.detDmg >= Actor.Determination)
		{
			str += "  Unit will [Red]panic[-] causing it to crouch and loose a movement point next turn";
		}

		batch.DrawText(str, pos, scale, 45, Color.White);
		batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/determinationFlame"), pos + new Vector2(0, 5)*scale,scale/2f,Color.White);
		if (Actor.WorldObject.PreviewData.detDmg >= Actor.Determination)
		{
			batch.Draw(TextureManager.GetTexture("HoverHud/Consequences/panic"), pos + new Vector2(0, 18)*scale,scale/2f,Color.White);
		}


	}

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
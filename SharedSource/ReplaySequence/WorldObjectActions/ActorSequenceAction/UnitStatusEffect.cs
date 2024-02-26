using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

#if CLIENT
using DefconNull.Rendering;
using DefconNull.WorldObjects;
#endif

namespace DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;

public class UnitStatusEffect  : UnitSequenceAction
{
	public bool addNotRemove;
	public string effectName = "";
	public int duration;
	public override SequenceType GetSequenceType()
	{
		return SequenceType.UnitStatusEffect;
	}

	public override BatchingMode Batching => BatchingMode.Always;

	public static UnitStatusEffect Make(TargetingRequirements actorID, bool addNotRemove, string effectName, int duration = 0)
	{
		UnitStatusEffect t = GetAction(SequenceType.UnitStatusEffect) as UnitStatusEffect;
		t.addNotRemove = addNotRemove;
		t.effectName = effectName;
		t.duration = duration;
		t.Requirements = actorID;
		return t;
	}

	protected override void RunSequenceAction()
	{
		
			if (addNotRemove)
			{
				Actor.ApplyStatus(effectName, duration);
			}
			else
			{
				Actor.RemoveStatus(effectName);
			}

	}
	
	protected override void SerializeArgs(Message message)
	{
		base.SerializeArgs(message);
		message.Add(addNotRemove);
		message.Add(effectName);
		message.Add(duration);
	}

	protected override void DeserializeArgs(Message msg)
	{
		base.DeserializeArgs(msg);
		addNotRemove = msg.GetBool();
		effectName = msg.GetString();
		duration = msg.GetInt();
	}

#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		if(Actor == null) return;
		Texture2D sprite = Actor.WorldObject.GetTexture();
		spriteBatch.Draw(sprite, Actor.WorldObject.GetDrawTransform().Position, Color.Yellow * 0.8f);

		
		//todo UI rework
	}

	public override void DrawDesc(Vector2 pos, SpriteBatch batch)
	{
		Texture2D plusMinus = TextureManager.GetTexture("HoverHud/Consequences/minus");
		if(addNotRemove) plusMinus = TextureManager.GetTexture("HoverHud/Consequences/plus");
		Texture2D effectIcon = TextureManager.GetTextureFromPNG("Icons/" + effectName);
		pos += new Vector2(70, 0);
		batch.Draw(plusMinus, pos, Color.White);
		batch.DrawNumberedIcon(duration.ToString(),effectIcon, pos + new Vector2(30, 0), Color.White);

		
		
	}

	public override void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
	{

		string str = "                 Status Effect:\n";
		Texture2D rmicn = TextureManager.GetTexture("HoverHud/Consequences/plus");
		if (!addNotRemove)
		{
			str += "  Effect wil be [Red]removed[-]";
			rmicn = TextureManager.GetTexture("HoverHud/Consequences/minus");
		}
		else
		{
			str += "  Effect wil be [Green]added[-]";
			str += "\n  [Green]Duration[-]";
		}
		str += "\n"+PrefabManager.StatusEffects[effectName].Tip;
		batch.DrawText(str, pos, scale, 50,Color.White);
		batch.Draw(rmicn, pos + new Vector2(0, 5),scale/2f,Color.White);
		if (addNotRemove)
		{
			Texture2D effectIcon = TextureManager.GetTextureFromPNG("Icons/" + effectName);
			batch.Draw(effectIcon, pos + new Vector2(0, 16),scale/2f,Color.White);
		}


	}
#endif
}
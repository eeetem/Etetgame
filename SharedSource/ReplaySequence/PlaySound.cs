using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence;

public class PlaySound : SequenceAction
{
	
	public override BatchingMode Batching => BatchingMode.Always;
	public override SequenceType GetSequenceType()
	{
		return SequenceType.PlaySound;
	}

	protected bool Equals(PlaySound other)
	{
		return SFX == other.SFX && Location.Equals(other.Location);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((PlaySound) obj);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			return (SFX.GetHashCode() * 397) ^ Location.GetHashCode();
		}
	}

	public string SFX ="";
	public Vector2Int Location;
	
	//TODO always play some sounds and hide others
	public static PlaySound Make(string sfx, Vector2Int target)
	{
		PlaySound t = (GetAction(SequenceType.PlaySound) as PlaySound)!;
		t.SFX = sfx;
		t.Location = target;
		return t;
	
	}

#if SERVER
	public override bool ShouldSendToPlayerServerCheck(bool player1)
	{
		return true;
	}
#endif




	protected override void RunSequenceAction()
	{
		
#if CLIENT
			Audio.PlaySound(SFX, Location);
#endif

	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(SFX);
		message.Add(Location);
	}

	protected override void DeserializeArgs(Message msg)
	{
		SFX = msg.GetString();
		Location = msg.GetSerializable<Vector2Int>();
	}


#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif


}
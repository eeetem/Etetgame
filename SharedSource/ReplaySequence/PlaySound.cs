using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.ReplaySequence;

public class PlaySound : SequenceAction
{
	
	public override bool CanBatch => true;
	public override SequenceType GetSequenceType()
	{
		return SequenceType.PlaySound;
	}

	public string SFX;
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
	public override bool ShouldDoServerCheck(bool player1)
	{
		return true;
	}
#endif




	protected override Task GenerateSpecificTask()
	{
		var t = new Task(delegate
		{
#if CLIENT
			Audio.PlaySound(SFX, Location);
#endif
		});
		return t;
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
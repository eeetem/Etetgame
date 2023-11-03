using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class PlaySound : SequenceAction
{
	
	public override bool CanBatch => true;
	public override SequenceType GetSequenceType()
	{
		return SequenceType.PlaySound;
	}

	public string SFX;
	public Vector2Int Location;
	/*
	public PlaySound(string sfx,Vector2Int location)
	{
		this.sfx = sfx;
		this.location = location;
	}
	*/


	public override Task GenerateTask()
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
	protected override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif

}
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.World.WorldObjects.Units.ReplaySequence;

public class PlaySound : SequenceAction
{
	public override bool CanBatch => true;
	private string sfx;
	private Vector2Int location;
	public PlaySound(string sfx,Vector2Int location) : base(SequenceType.PlaySound)
	{
		this.sfx = sfx;
		this.location = location;
	}
	public PlaySound(Message msg) : base(SequenceType.PlaySound)
	{
		sfx = msg.GetString();
		location = msg.GetSerializable<Vector2Int>();
	}

	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
#if CLIENT
			Audio.PlaySound(sfx, location);
#endif
		});
		return t;
	}

	protected override void SerializeArgs(Message message)
	{
		message.Add(sfx);
		message.Add(location);
	}
	
	
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{
		//no need to preview
	}
#endif
}
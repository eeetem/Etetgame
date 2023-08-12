﻿using System.Threading.Tasks;
using DefconNull.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Riptide;

namespace DefconNull.SharedSource.Units.ReplaySequence;

public class Suppress : WorldObjects.Units.ReplaySequence.UnitSequenceAction
{
	public override bool CanBatch => true;
	private int detDmg;

	public Suppress(int detDmg, int actorID) : base(actorID, SequenceType.Suppress)
	{
		this.detDmg = detDmg;
	}
	
	public Suppress(int actorID, Message msg) : base(actorID, SequenceType.Suppress)
	{
		detDmg = msg.GetInt();
	}

	protected override void SerializeArgs(Message message)
	{
		
		base.SerializeArgs(message);
		message.Add(detDmg);
	}


	public override Task GenerateTask()
	{
		var t = new Task(delegate
		{
			Actor.Suppress(detDmg);
		});
		return t;
	}
	
#if CLIENT
	public override void Preview(SpriteBatch spriteBatch)
	{

		Texture2D sprite = Actor.WorldObject.GetTexture();
		spriteBatch.Draw(sprite, Actor.WorldObject.GetDrawTransform().Position, Color.Blue * 0.8f);
		
		Actor.WorldObject.PreviewData.detDmg = detDmg;
	}
#endif

}
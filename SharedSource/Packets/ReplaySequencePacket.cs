using System;
using System.Collections.Generic;
using MultiplayerXeno.ReplaySequence;
using Network.Packets;
using Newtonsoft.Json;
namespace MultiplayerXeno;

public class ReplaySequencePacket : Packet
{
	public Queue<SequenceAction> SequenceActions { get; set; }
	public string SequenceActionsJSON { get; set; } = null!;

	public override void BeforeReceive()
	{
		SequenceActions = JsonConvert.DeserializeObject<Queue<SequenceAction>>(SequenceActionsJSON) ?? throw new InvalidOperationException();
		base.BeforeReceive();
	}

	public override void BeforeSend()
	{
		SequenceActionsJSON = JsonConvert.SerializeObject(SequenceActions);
		base.BeforeSend();
	}
	

	public ReplaySequencePacket(Queue<SequenceAction> actions)
	{
		SequenceActions = actions;
	}

}

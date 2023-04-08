﻿using Network.Packets;

namespace CommonData;

public class SquadCompPacket : Packet
{
	public List<SquadMember>? Composition {get; set;} = new List<SquadMember>();
	public string CompositionJson {get; set;}


	public override void BeforeSend()
	{
		base.BeforeSend();
		CompositionJson = Newtonsoft.Json.JsonConvert.SerializeObject(Composition);
	}

	public override void BeforeReceive()
	{
		base.BeforeReceive();
		Composition = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SquadMember>>(CompositionJson);
	}
}
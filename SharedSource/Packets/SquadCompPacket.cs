using System.Collections.Generic;
using Newtonsoft.Json;
namespace MultiplayerXeno;

public class SquadCompPacket : Packet
{
	public List<SquadMember>? Composition {get; set;} = new List<SquadMember>();
	public string CompositionJson {get; set;} = null!;


	public override void BeforeSend()
	{
		base.BeforeSend();
		CompositionJson = JsonConvert.SerializeObject(Composition);
	}

	public override void BeforeReceive()
	{
		base.BeforeReceive();
		Composition = JsonConvert.DeserializeObject<List<SquadMember>>(CompositionJson);
	}
}
using System;
using System.Collections.Generic;
using Network.Packets;

namespace MultiplayerXeno;


public class MapDataPacket : Packet
{
	public MapData MapData { get; set; }
	public string MapDataJson { get; set; } = null!;

	public override void BeforeReceive()
	{
		MapData = MapData.Deserialse(MapDataJson);
		base.BeforeReceive();
	}

	public override void BeforeSend()
	{
		MapDataJson = MapData.Serialise();
		base.BeforeSend();
	}

	public MapDataPacket(MapData data)
	{
		MapData = data;
	}
}

public class MapData
{

	public string Name = "New Map";
	public string Author = "Unknown";
	public int unitCount;
	public List<WorldTileData> Data = new List<WorldTileData>();
	

	public static MapData Deserialse(string json)
	{
		return Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(json) ?? throw new InvalidOperationException();
	}

	public string Serialise()
	{
		return Newtonsoft.Json.JsonConvert.SerializeObject(this);
	}

	
}
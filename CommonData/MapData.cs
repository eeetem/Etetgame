using Network.Packets;

namespace CommonData;


public class MapDataPacket : Packet
{
	public MapData MapData { get; set; }
	public string MapDataJson { get; set; }

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
		return Newtonsoft.Json.JsonConvert.DeserializeObject<MapData>(json);
	}

	public string Serialise()
	{
		return Newtonsoft.Json.JsonConvert.SerializeObject(this);
	}

	
}
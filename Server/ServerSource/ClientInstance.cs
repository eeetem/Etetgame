using Riptide;

namespace DefconNull.Networking;

public class ClientInstance
{
	public Connection? Connection;
	public string Name;
	public bool IsAI;
	public List<SquadMember>?  SquadComp { get; private set; }
	public bool IsPracticeOpponent { get; set; }

	public ClientInstance(string name,Connection? con)
	{
		Name = name;
		Connection = con;
	}

	public void SetSquadComp(List<SquadMember> squad)
	{
		SquadComp = squad;
	}


	//public static List<Client> Clients = new List<Client>();
}
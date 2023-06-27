using Riptide;

namespace MultiplayerXeno;

public class Client
{
	public Connection? Connection;
	public string Name;
	public List<Networking.SquadMember>?  SquadComp { get; private set; }

	public Client(string name,Connection? con)
	{
		Name = name;
		Connection = con;
	}

	public void SetSquadComp(List<Networking.SquadMember> squad)
	{
		SquadComp = squad;
	}


	//public static List<Client> Clients = new List<Client>();
}
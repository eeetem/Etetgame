using Riptide;

namespace DefconNull;

public class ClientInstance
{
	public Connection? Connection;
	public string Name;
	public bool IsAI;
	public List<SquadMember>?  SquadComp { get; private set; }
	public bool IsPracticeOpponent { get; set; }
	
	public bool HasDeliveredAllMessages => MessagesToBeDelivered.Count == 0;
	
	private List<ushort> MessagesToBeDelivered = new List<ushort>();
	public ClientInstance(string name,Connection con)
	{
		Name = name;
		Connection = con;
		Connection.ReliableDelivered += ProcessDelivery;
	}
	
	public void RegisterMessageToBeDelivered(ushort id)
	{
		MessagesToBeDelivered.Add(id);
	}
	private void ProcessDelivery(ushort id)
	{
		MessagesToBeDelivered.Remove(id);
	}


	public void SetSquadComp(List<SquadMember> squad)
	{
		SquadComp = squad;
	}

}
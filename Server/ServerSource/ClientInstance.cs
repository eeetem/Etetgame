using System.Collections.Concurrent;
using DefconNull.ReplaySequence;
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
	
	public ConcurrentQueue<List<SequenceAction>> SequenceQueue = new ConcurrentQueue<List<SequenceAction>>();
	
	private List<ushort> MessagesToBeDelivered = new List<ushort>();
	public ClientInstance(string name,Connection? con)
	{
		Name = name;
		Connection = con;
		if (Connection != null) Connection.ReliableDelivered += ProcessDelivery;
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
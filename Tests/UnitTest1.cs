using DefconNull;
using DefconNull.ReplaySequence;
using DefconNull.ReplaySequence.WorldObjectActions.ActorSequenceAction;
using DefconNull.WorldObjects;
using Riptide;

namespace Tests;

public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	[Test]
	public void GlobalSerialisabiltyTest()
	{
		return;
		var commandTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t => typeof(IMessageSerializable).IsAssignableFrom(t));
		
		foreach (var type in commandTypes)
		{
			IMessageSerializable? serializable = Activator.CreateInstance(type) as IMessageSerializable;
			Assert.NotNull(serializable);

			Message message = Message.Create();
			serializable!.Serialize(message);
			
			IMessageSerializable? deserialized = Activator.CreateInstance(type) as IMessageSerializable;
			Assert.NotNull(deserialized);
			deserialized!.Deserialize(message);
			
			Assert.AreEqual(serializable, deserialized);
		}
	}
	[Test]
	public void SequenceActionSeriasabilityTest()
	{
		return;
		SequenceAction.InitialisePools();
		Random r = new Random(12345);
		Message.MaxPayloadSize = 2048*2048;

		
		foreach (SequenceAction.SequenceType sqc in Enum.GetValues(typeof(SequenceAction.SequenceType)))
		{
			if(sqc == SequenceAction.SequenceType.Undefined) continue;
			if(SequenceAction.GetAction(sqc).IsUnitAction )continue;
			//Assert.Warn("running test for "+sqc+" action");
			Console.WriteLine("running test for "+sqc+" action");

			Message newSqcMessage;

			var type = SequenceAction.EnumToType(sqc);
			
			var msg = type.GetMethod("MakeTestingMessage")?.Invoke(SequenceAction.GetAction(sqc), null);
			if (msg == null)
			{
				newSqcMessage = Message.Create();	
				for (int i = 0; i < 15; i++)
				{
					ulong randLong = (ulong) r.NextInt64();
					//newSqcMessage.AddBits(randLong,64);
				}
			}
			else
			{
				newSqcMessage = (Message) msg;
			}

			var action3 = SequenceAction.GetAction(sqc, newSqcMessage);//deserialise from garbage
			newSqcMessage.Release();
			Assert.NotNull(action3);
			if (action3.IsUnitAction)
			{
				UnitSequenceAction u = (UnitSequenceAction) action3;
				if(u.Requirements.TypesToIgnore != null)
					u.Requirements.TypesToIgnore!.RemoveAll(x => x == "");//remove garbage from lists
			}

			Message message = Message.Create();
			
			action3!.Serialize(message);
			var action4 =SequenceAction.GetAction(sqc, message);
			Assert.NotNull(action4);	
			Assert.AreEqual(action3, action4);
			
			message.Release();
		
		}

	}
	[Test]
	public void WorldDataSerialisabilityTest()
	{
		for (int i = 0; i < 100; i++)
		{
			Console.WriteLine(i);
			WorldObject.WorldObjectData d = new WorldObject.WorldObjectData();
			d.Prefab = Random.Shared.Next().ToString();
			d.ID = Random.Shared.Next();
			d.Facing = (Direction) Random.Shared.Next(0, 8);
			d.Fliped = Random.Shared.Next(2) == 1;
			
			d.JustSpawned = Random.Shared.Next(2) == 1;
			d.Health = Random.Shared.Next();
			
			if (Random.Shared.Next(2) == 1)
			{
				
				var unitData = new Unit.UnitData();
				unitData.Determination = Random.Shared.Next();
				unitData.ActionPoints = Random.Shared.Next();
				unitData.MoveRangeEffect = Random.Shared.Next();
				unitData.MovePoints = Random.Shared.Next();
				unitData.CanTurn = Random.Shared.Next(2) == 1;
				unitData.Overwatch = new Tuple<bool, int>( Random.Shared.Next(2) == 1, Random.Shared.Next());
				unitData.OverWatchedTiles = new List<Vector2Int>();
				unitData.StatusEffects = new List<Tuple<string, int>>();
				d.UnitData = unitData;
			}
			Message message = Message.Create();
			message.AddSerializable(d);
			var d2 = message.GetSerializable<WorldObject.WorldObjectData>();
			Assert.AreEqual(d, d2);
			message.Release();
		}
		
	}
}
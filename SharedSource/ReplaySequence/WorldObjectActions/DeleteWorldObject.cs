using System.Threading.Tasks;

using DefconNull.WorldObjects;
using Riptide;

#if CLIENT
using DefconNull.Rendering.UILayout.GameLayout;
#endif
namespace DefconNull.ReplaySequence.WorldObjectActions;

public static partial class WorldObjectManager
{
	public class DeleteWorldObject : SequenceAction
	{
		public override SequenceType GetSequenceType()
		{
			return SequenceType.DeleteWorldObject;
		}
#if SERVER
		public override bool ShouldDoServerCheck(bool player1)
		{
			var obj = GetObject(id);
			if (obj is null) return false;
			return obj.IsVisible(team1: player1);
		}
#endif

		public override BatchingMode Batching => BatchingMode.OnlySameType;

		private int id;
		public static DeleteWorldObject Make(int id)
		{
			var t = GetAction(SequenceType.DeleteWorldObject) as DeleteWorldObject;
			t!.id = id;
			return t;
		}

		public static DeleteWorldObject Make(WorldObject obj)
		{
			var t = GetAction(SequenceType.DeleteWorldObject) as DeleteWorldObject;
			t!.id = obj.ID;
			return t;
		}

		protected override void RunSequenceAction()
		{

				if (!WorldObjects.ContainsKey(id)) return;

				if (id < NextId)
				{
					NextId = id; //reuse IDs
				}

				WorldObject Obj = WorldObjects[id];
			
				GameManager.Forget(Obj);

#if  CLIENT
		if (Obj.UnitComponent != null)
		{
			GameLayout.UnRegisterUnit(Obj.UnitComponent);
		}
#endif		
				(Obj.TileLocation as WorldTile)?.Remove(id);
				lock (WoLock)
				{
					WorldObjects.Remove(id);
				}
		}



		protected override void SerializeArgs(Message message)
		{
			message.Add(id);
		}

		protected override void DeserializeArgs(Message msg)
		{
			id = msg.GetInt();
		}

	
	}

}
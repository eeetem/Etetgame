using System;
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

		protected bool Equals(DeleteWorldObject other)
		{
			return id == other.id;
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DeleteWorldObject) obj);
		}

		public override int GetHashCode()
		{
			return id;
		}
#if SERVER
		public override bool ShouldSendToPlayerServerCheck(bool player1)
		{
			var obj = GetObject(id);
			if (obj is null) return false;
			return obj.ShouldBeVisibilityUpdated(team1: player1);
		}
#endif

		public override BatchingMode Batching => BatchingMode.Sequential;

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
			Console.WriteLine("Deleting world object: " + id);
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
				WorldTile tile = WorldManager.Instance.GetTileAtGrid(Obj.TileLocation.Position);//get real tile
				tile.Remove(id);
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
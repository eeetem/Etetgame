using System;
using System.Threading.Tasks;

using DefconNull.WorldObjects;
using Riptide;

#if CLIENT
using System.Threading;
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
			if (obj.GetType() != GetType()) return false;
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

		public override BatchingMode Batching => BatchingMode.AsyncBatchSameType;

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

		public override bool ShouldDo()
		{
			return WorldObjects.ContainsKey(id);
		}

		protected override void RunSequenceAction()
		{

			Log.Message("WORLD OBJECT MANAGER","Deleting world object: " + id);
			WorldObject obj = WorldObjects[id];
#if CLIENT
			while (obj.IsAnimating)
			{
				Thread.Sleep(50);//wait for animation finish this is quite bad tho
			}
#endif



			while (WoReadLock>0)
			{
				Thread.Sleep(100);
			}
			lock (WoLock)
			{
				GameManager.Forget(obj);
				WorldTile tile = WorldManager.Instance.GetTileAtGrid(obj.TileLocation.Position);//get real tile
				tile.Remove(id);
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

		public override string ToString()
		{
			var obj = GetObject(id);
			if (obj is null) return $"Deleteing non existant Object: {nameof(id)}: {id}";
			var tileLocationPosition = obj.TileLocation.Position;
			return $"Delete Object: {nameof(id)}: {id} {obj.Type.Name} "+tileLocationPosition;
		}
	}

}
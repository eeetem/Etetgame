using System;
using System.Collections.Generic;
using MultiplayerXeno;
using Microsoft.Xna.Framework.Graphics;
using MultiplayerXeno.Items;

namespace MultiplayerXeno
{
	public class ControllableType
	{

		public ControllableType(string name)
		{
			Name = name;
		}

		public readonly string Name;
		public int MoveRange = 4;
		public int SightRange = 16;
		public int OverWatchSize = 2;
		
		public int MaxMovePoints = 2;
		public int MaxActionPoints = 2;

		public int Maxdetermination = 2;

		public int InventorySize = 1;


		public Texture2D[] CrouchSpriteSheet;
		public ExtraAction DefaultAttack;
		public List<IExtraAction> extraActions = new List<IExtraAction>();
		public WorldEffect? SpawnEffect { get; set; }

		public Controllable Instantiate(WorldObject parent,ControllableData data)
		{
			Controllable obj = new Controllable(data.Team1,parent,this,data);

			return obj;
		}
	}
}
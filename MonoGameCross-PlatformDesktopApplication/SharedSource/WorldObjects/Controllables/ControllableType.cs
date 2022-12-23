using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;

namespace MultiplayerXeno
{
	public class ControllableType
	{
		public int MoveRange = 4;
		public int SightRange = 16;
		public int SupressionRange = 2;
		public int OverWatchSize = 2;
		
		public int MaxMovePoints = 2;
		public int MaxTurnPoints = 2;
		public int MaxActionPoints = 2;
		

		public int WeaponDmg = 4;
		public int WeaponRange = 10;
		
		public int MaxHealth = 10;
		public int MaxAwareness = 2;


		public Texture2D[] CrouchSpriteSheet;

		public List<Tuple<string, ActionType>> extraActions = new List<Tuple<string, ActionType>>();
		public Controllable Instantiate(WorldObject parent,ControllableData data)
		{
			
			Controllable obj = new Controllable(data.Team1,parent,this,data);


			return obj;
		}
	}
}
﻿using System;
using System.Threading;
using CommonData;
using Microsoft.Xna.Framework;

namespace MultiplayerXeno;

public static class ObjectSpawner
{
	public static void Burst(Vector2Int from, Vector2Int to)
	{
		float dist = Vector2.Distance(from, to)/2f;
		from = Utility.GridToWorldPos(from);
		to = Utility.GridToWorldPos(to);
		

		for (int i = 0; i < 3; i++)
		{
			Vector2 dir = Vector2.Normalize(to - from);
			dir += new Vector2( Random.Shared.Next(-10,10)/100f,Random.Shared.Next(-10,10)/100f);
			Audio.PlaySound("rifle", from);
			var obj = new LocalObject(Game1.Textures["bullet"], from,dir*2, dist*200);
			Thread.Sleep(150);
		}


	}
	public static void ShotGun(Vector2Int from, Vector2Int to)
	{
		float dist = Vector2.Distance(from, to)/2f;
		from = Utility.GridToWorldPos(from);
		to = Utility.GridToWorldPos(to);
		
		Audio.PlaySound("shotgun", from);
		for (int i = 0; i < 6; i++)
		{
			Vector2 dir = Vector2.Normalize(to - from);
			dir += new Vector2( Random.Shared.Next(-50,50)/100f,Random.Shared.Next(-50,50)/100f);
			var obj = new LocalObject(Game1.Textures["bullet"], from,dir*2, dist*200);
		}


	}
}
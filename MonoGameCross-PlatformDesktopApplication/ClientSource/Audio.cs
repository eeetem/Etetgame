﻿using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using MonoGame.Extended.Content;


namespace MultiplayerXeno;

public static class Audio
{
	private static Dictionary<string, SoundEffect> soundEffects = new Dictionary<string, SoundEffect>();
	private static List<SoundEffectInstance> activeSounds = new List<SoundEffectInstance>();
	public static readonly object syncobj = new object();

	public static void Init(ContentManager content)
	{
		
		soundEffects.Add("death",content.Load<SoundEffect>("audio/damage/death"));
		soundEffects.Add("grunt",content.Load<SoundEffect>("audio/damage/grunt"));
		soundEffects.Add("wilhelm",content.Load<SoundEffect>("audio/damage/wilhelm"));
		soundEffects.Add("rifle",content.Load<SoundEffect>("audio/rifle"));
		soundEffects.Add("mg",content.Load<SoundEffect>("audio/mg"));
		soundEffects.Add("shotgun",content.Load<SoundEffect>("audio/shotgun"));


		for (int i = 1; i < 10; i++)
		{
			soundEffects.Add("footstep"+i,content.Load<SoundEffect>("audio/footsteps/footstep "+i));
		}

	}

	public static void PlaySound(string name, Vector2Int location)
	{
		string sfxID = name;
		switch (name)
		{
			case "footstep":
				sfxID = "footstep" + Random.Shared.Next(1, 10);
				break;
			case "death":
				if (Random.Shared.Next(100) == 1)
				{
					sfxID = "wilhelm";
				}
				else
				{
					sfxID = "death";
				}

				break;
			
			
		}


		
		SoundEffectInstance instance = soundEffects[sfxID].CreateInstance();
		instance.Pitch += (float)(Random.Shared.NextDouble() - 0.5f) / 1f;
		AudioEmitter emitter = new AudioEmitter();
		emitter.Position = new Vector3((Vector2)location/80f, 0);
		instance.Play();
		instance.Apply3D(Camera.AudioListener,emitter);
		lock (syncobj)
		{
			activeSounds.Add(instance);
		}
	}

	public static void Update(float gametime)
	{
		lock (syncobj)
		{
			foreach (var sound in activeSounds)
			{
				//sound.Apply3D(s);
			}
		}
	}
}
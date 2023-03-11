using System;
using System.Collections.Generic;
using CommonData;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using MonoGame.Extended.Content;


namespace MultiplayerXeno;

public static class Audio
{
	private static List<SoundEffectInstance> activeSounds = new List<SoundEffectInstance>();
	public static readonly object syncobj = new object();
	private static ContentManager content;
	
	private static float musicVolume = 0.5f;
	public static float MusicVolume
	{
		get
		{
			return musicVolume;
		}
		set
		{
			musicVolume = value;
			MediaPlayer.Volume = MusicVolume;
		}
	}
	public static float SoundVolume = 0.5f;
		
	public static Dictionary<string, SoundEffect> SFX = new Dictionary<string, SoundEffect>();
	
	public static void PlayMenu()
	{
		
		MediaPlayer.Play(content.Load<Song>("audio/music/menu"));
		MediaPlayer.IsRepeating = true;
	}
	public static void PlayCombat()
	{

		MediaPlayer.Play(content.Load<Song>("audio/music/tension"));
		MediaPlayer.IsRepeating = true;
	}

	private static SoundEffect GetSound(string name)
	{
		if (SFX.ContainsKey(name))
		{
			return SFX[name];
		}

		SFX.Add(name,content.Load<SoundEffect>("audio/"+name));
		
		return SFX[name];
	}
	public static void Init(ContentManager content)
	{
		Audio.content = content;
	}


	public static void PlaySound(string name, Vector2Int? location = null , float pitchVariationScale = 1)
	{
		if (location == null)
		{
			location = Camera.GetPos();
		}

		string sfxID = name;
		switch (name)
		{
			case "footstep":
				sfxID = "footsteps/Footstep " + Random.Shared.Next(1, 10);
				break;
			case "death":
				if (Random.Shared.Next(100) == 1)
				{
					sfxID = "damage/wilhelm";
				}
				else
				{
					sfxID = "damage/death"+Random.Shared.Next(1,4);
				}

				break;
			case "grunt":
				sfxID = "damage/grunt"+Random.Shared.Next(1,2);//todo standartised audio system with variations
				break;
			
			
		}


		
		SoundEffectInstance instance = GetSound(sfxID).CreateInstance();
		instance.Pitch += (float)((Random.Shared.NextDouble() - 0.5f) / 2f )*pitchVariationScale;
		instance.Volume = SoundVolume;
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
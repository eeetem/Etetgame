using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace DefconNull;

public static class Audio
{
	private static List<Tuple<SoundEffectInstance, AudioEmitter>> activeSounds = new List<Tuple<SoundEffectInstance, AudioEmitter>>();
	public static readonly object syncobj = new object();
	private static ContentManager content = null!;

	private static Tuple<string,SoundEffectInstance>? CurrentMusic = null;
	
	private static float musicVolume = 0.5f;

	public static float MusicVolume
	{
		get => musicVolume;
		set
		{
			musicVolume = value;
			if (CurrentMusic != null)
			{
				CurrentMusic.Item2.Volume = value;
			}
		}
	}
	
	public static float SoundVolume = 0.5f;
		
	public static Dictionary<string, SoundEffect> SFX = new Dictionary<string, SoundEffect>();
	public static void PlayMusic(GameState g)
	{
		string nextSong = "";
		switch (g)
		{
			case GameState.Lobby:
				nextSong =  "tresspassing";
				break;
			default:
				nextSong = "";
				break;
		}

		if (nextSong != "")
		{
			if (CurrentMusic == null || CurrentMusic.Item2.State == SoundState.Stopped || CurrentMusic.Item1 != nextSong)
			{
				var instance = content.Load<SoundEffect>("CompressedContent/audio/music/" + nextSong).CreateInstance();
				instance.Volume = MusicVolume;
				instance.IsLooped = true;
				CurrentMusic = new Tuple<string, SoundEffectInstance>(nextSong, instance);
				instance.Play();
			}
		}

	}

	private static SoundEffect GetSound(string name)
	{
		if (SFX.ContainsKey(name))
		{
			return SFX[name];
		}

		SFX.Add(name,content.Load<SoundEffect>("CompressedContent/audio/"+name));
		
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
				sfxID = "damage/grunt" + Random.Shared.Next(1, 2);
				break;
		}


		
			SoundEffectInstance instance = GetSound(sfxID).CreateInstance();
			instance.Pitch += (float) ((Random.Shared.NextDouble() - 0.5f) / 2f) * pitchVariationScale;
			instance.Volume = SoundVolume;
			
			AudioEmitter emitter = new AudioEmitter();
			emitter.Position = new Vector3((Vector2) location /150f, 0);
			instance.Apply3D(Camera.AudioListener, emitter);
			instance.Play();
			
			lock (syncobj)
			{
				activeSounds.Add(new Tuple<SoundEffectInstance, AudioEmitter>(instance,emitter));
			}
	
	}
	
	
	

	public static void Update(float gametime)
	{
		lock (syncobj)
		{
			foreach (var sound in  new List<Tuple<SoundEffectInstance, AudioEmitter>>(activeSounds))
			{
				sound.Item1.Apply3D(Camera.AudioListener, sound.Item2);
				if (sound.Item1.State == SoundState.Stopped)
				{
					activeSounds.Remove(sound);
				}

			}
		}
	}
}
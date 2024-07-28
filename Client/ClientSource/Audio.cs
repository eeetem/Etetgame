using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace DefconNull;

public static class Audio
{
	private static List<Tuple<SoundEffectInstance, float,float, Vector2?>> activeSounds = new List<Tuple<SoundEffectInstance, float,float,Vector2?>>();
	public static readonly object syncobj = new object();
	public static readonly object audioLoadSync = new object();
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
	public static void OnGameStateChange(GameState g)
	{
		List<string> songList = new List<string>();
		switch (g)
		{
			case GameState.Lobby:
				songList.Add("Tresspassing");
				break;
			case GameState.Playing:
				if (GameManager.IsMyTurn())//intense songs reserved for player's turn
				{
					songList.Add("Universal");
					songList.Add("Planning");
				}
				songList.Add("Seclusion");
				songList.Add("Warframes");
				songList.Add("Cavern Dwelling");
				break;
			case GameState.Setup:
				songList.Add("Seclusion");
				songList.Add("Warframes");
				songList.Add("Cavern Dwelling");
				break;
			
		}
		if(songList.Count == 0)
		{
			return;
		}
		var nextSong = songList[Random.Shared.Next(songList.Count)];
		PlayMusicTrack(nextSong);

	}

	private static void PlayMusicTrack(string nextSong)
	{
		if (CurrentMusic == null || CurrentMusic.Item2.State == SoundState.Stopped && CurrentMusic.Item1 != nextSong)
		{
			var instance = content.Load<SoundEffect>("CompressedContent/audio/music/" + nextSong).CreateInstance();
			instance.Volume = MusicVolume;
			instance.IsLooped = false;
			CurrentMusic = new Tuple<string, SoundEffectInstance>(nextSong, instance);
			instance.Play();
		}
	}
	public static void ForcePlayMusic(string name)
	{
		CurrentMusic?.Item2.Stop();
		PlayMusicTrack(name);
	}


	private static SoundEffect GetSound(string name)
	{
		lock (audioLoadSync)
		{
			if (SFX.ContainsKey(name))
			{
				return SFX[name];
			}

			SFX.Add(name,content.Load<SoundEffect>("CompressedContent/audio/"+name));
		}
		return SFX[name];
	}
	public static void Init(ContentManager content)
	{
		Audio.content = content;
	}


	public static void PlaySound(string name, Vector2? location = null , float pitchVariationScale = 1, float volume = 1, float rangeMod = 1)
	{
		
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
			case "panic":
					sfxID = "damage/panic"+Random.Shared.Next(1,5);
					pitchVariationScale = 0.5f;
					volume = 0.25f;
					rangeMod = 0.1f;
				break;
				

		}


		
		SoundEffectInstance instance = GetSound(sfxID).CreateInstance();
		instance.Pitch += (float) ((Random.Shared.NextDouble() - 0.5f) / 2f) * pitchVariationScale;
		instance.Volume = SoundVolume*volume;
		ApplySoundPan(instance, volume,rangeMod, location);
		instance.Play();
			
		lock (syncobj)
		{
			activeSounds.Add(new Tuple<SoundEffectInstance, float,float, Vector2?>(instance, volume,rangeMod, location));
		}
	
	}


	static void ApplySoundPan(SoundEffectInstance effectInstance, float volumeMod, float rangeMod, Vector2? source)
	{
		
		float MaxRadius = 2000 *rangeMod;
		int MinRadius = 50;
		Vector2 camPos = Camera.GetPos();
		if(source == null)
		{
			source = Camera.GetPos();
		}
		float distance = Vector2.Distance(source.Value, camPos);
		float pan = 0f;
		float volume = 1f;

		if (distance <= MaxRadius)
		{
			if (distance > MinRadius)
			{
				float ratio = (distance / MaxRadius);

				volume = 1 - ratio;

				var differenceX = (source - camPos).Value.X;

				if (differenceX != 0)
				{
					var differenceAbs = MathF.Abs(differenceX);

					if (differenceAbs >= MinRadius / 2f)
						pan = ratio * (differenceX / differenceAbs);
				}

			}
		}
		else volume = 0f;

		effectInstance.Volume = volume*volumeMod;
		effectInstance.Pan = pan;
		
	}

	public static void Update(float gametime)
	{
		
		lock (syncobj)
		{
			foreach (var sound in  new List<Tuple<SoundEffectInstance, float,float, Vector2?>>(activeSounds))
			{
				ApplySoundPan(sound.Item1, sound.Item2, sound.Item3,sound.Item4);
				if (sound.Item1.State == SoundState.Stopped)
				{
					activeSounds.Remove(sound);
				}

			}
		}
	}
}
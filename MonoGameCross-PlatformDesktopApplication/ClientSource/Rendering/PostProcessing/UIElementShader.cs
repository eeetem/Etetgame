using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.PostProcessing;

public class UIElementShader : ShaderPreset
{
	public UIElementShader()
	{
	}

	private float counter = 0;
	public bool flicker = false;
	public bool dissapation = false;
	public override void Update(float deltatime)
	{
		if (!dissapation)
		{
			counter += (deltatime/2000f) * (counter*3f+Random.Shared.NextSingle())/2f;
		}
		else
		{
			counter += (deltatime/100f);
		}

		float range = 2.5f;
		float min = 1.5f;
		if (flicker)
		{
			range = 8;
			min = 0;
		}

		if (dissapation)
		{
			range = 100;
			min = 0f;
		}


		while (counter > range)
		{
			counter -= range;
		}
		float uiAnim = counter;
		if (counter > range/2f)
		{
			uiAnim = range - counter;
		}

		uiAnim += min;
		EffectParams["blurAmmount"] = (0.04f*(uiAnim/4)+0.01f);
		EffectParams["glowAmmount"] = (0.5f*uiAnim+0.1f);
		if (dissapation)
		{
		//	EffectParams["shape"] = (55*uiAnim);
		}
		else
		{
		//	EffectParams["shape"] = (30f*uiAnim);
		}



	}

	public override void Apply(Effect effect,Vector2 size)
	{
		
		
		
		base.Apply(effect,size);

		//effect.Parameters["textureSize"].SetValue(size*3);
	//	effect.Parameters["videoSize"].SetValue(size*3);
	//	effect.Parameters["outputSize"].SetValue(size*3);
	}
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.PostProcessing;

public class CrtLIghtNormalShader : ShaderPreset
{
	public CrtLIghtNormalShader()
	{
		EffectParams["hardScan"] = 0f;
		EffectParams["hardPix"] = 0f;
		EffectParams["warpX"] = 0.1f;
		EffectParams["warpY"] = 0.1f;
		EffectParams["maskDark"] = 1f;
		EffectParams["maskLight"] = 1.5f;
		EffectParams["scaleInLinearGamma"] = 0f;
		EffectParams["shadowMask"] = 0f;
	}

	private float counter = 0;
	public bool flicker = false;
	public bool dissapation = false;
	public override void Update(float deltatime)
	{
		if (!dissapation)
		{
			counter += (deltatime/2000f) * (counter*2f+Random.Shared.NextSingle())/2f;
		}
		else
		{
			counter += (deltatime / 2000f);
		}

		float range = 4;
		float min = 1.5f;
		if (flicker)
		{
			range = 8;
			min = 0;
		}

		if (dissapation)
		{
			range = 2;
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
		EffectParams["brightboost"] = (0.05f+(uiAnim/3f));
		EffectParams["hardBloomScan"] = (-1f*uiAnim);
		EffectParams["hardBloomPix"]= (-2f*uiAnim);
		EffectParams["bloomAmount"] = (0.1f*uiAnim);
		if (dissapation)
		{
			EffectParams["shape"] = (55*uiAnim);
		}
		else
		{
			EffectParams["shape"] = (4f*uiAnim);
		}



	}

	public override void Apply(Effect effect,Vector2 size)
	{
		
		base.Apply(effect,size);

		effect.Parameters["textureSize"].SetValue(size*3);
		effect.Parameters["videoSize"].SetValue(size*3);
		effect.Parameters["outputSize"].SetValue(size*3);
	}
}
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.PostProcessing;

public class CrtScreenShaderPreset : ShaderPreset
{
	public CrtScreenShaderPreset()
	{

	}

	private float counter = 0;

	public override void Update(float deltatime)
	{
		counter += (deltatime / 3000f);


		float range = 1;
		float min = 1f;


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
		
		EffectParams["bloomAmount"] = (60+5f*uiAnim);
		EffectParams["warpX"] = (0.04f*uiAnim);
		EffectParams["warpY"] = (0.04f*uiAnim);
		EffectParams["shape"] = (0.02f*uiAnim);
		EffectParams["maskLight"] = (8f*uiAnim);
		EffectParams["maskDark"] = (1f*uiAnim);


	}

	public override void Apply(Effect effect,Vector2 size)
	{
		EffectParams["hardScan"] = -0;
		EffectParams["hardPix"] = -0;
		
		EffectParams["hardBloomScan"] = -5f;
		EffectParams["hardBloomPix"] = -0f;
		
		EffectParams["scaleInLinearGamma"] = 2f;
		EffectParams["shadowMask"] = 2f;
		EffectParams["brightboost"] = (0.15f);
		
		base.Apply(effect,size);

		effect.Parameters["textureSize"].SetValue(size*3);
		effect.Parameters["videoSize"].SetValue(size*3);
		effect.Parameters["outputSize"].SetValue(size*3);
		
		

	}
}
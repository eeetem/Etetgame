using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.Rendering.PostProcessing;

public class CrtScreenShaderPreset : ShaderPreset
{
	public CrtScreenShaderPreset()
	{
		
	}

	private float counter;

	public override void Update(float deltatime)
	{
		counter += deltatime / 3000f* (counter*2f+Random.Shared.NextSingle())/2f;


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
		
		EffectParams["bloomAmount"] = 40+50f*uiAnim;
		EffectParams["warpX"] = 0.004f * uiAnim - 0.002f;
		EffectParams["warpY"] = 0.004f*uiAnim-0.002f;
		EffectParams["shape"] = 1f*uiAnim;
		EffectParams["maskLight"] = 3f*uiAnim;
		EffectParams["maskDark"] = -1f*uiAnim;


	}

	public override void Apply(Effect effect,Vector2 size)
	{
		EffectParams["hardScan"] = 1;
		EffectParams["hardPix"] = 1;
		EffectParams["hardBloomScan"] = -5f;
		EffectParams["hardBloomPix"] = -1f;
		EffectParams["shadowMask"] = 2f;
		EffectParams["brightboost"] = 0.01f;

		base.Apply(effect,size);

		effect.Parameters["textureSize"].SetValue(size*3);
		effect.Parameters["videoSize"].SetValue(size*3);
		effect.Parameters["outputSize"].SetValue(size*3);
		
		

	}
}
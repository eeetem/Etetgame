using System.Collections.Generic;
using System.Data;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering;

public abstract class ShaderPreset
{
	public readonly Dictionary<string, float> EffectParams = new Dictionary<string, float>();
	public abstract void Update(float deltaTime);
	public virtual void Apply(Effect effect)
	{
		foreach (var param in EffectParams)
		{
			effect.Parameters[param.Key].SetValue(param.Value);
		}
	}
}
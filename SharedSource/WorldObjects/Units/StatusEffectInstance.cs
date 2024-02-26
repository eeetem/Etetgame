

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DefconNull.WorldObjects.Units;

public class StatusEffectInstance
{
	public StatusEffectInstance(StatusEffectType type, int duration, Unit owner)
	{
		this.Type = type;
		this.Duration = duration;
		this.Owner = owner;
	}

	public readonly StatusEffectType Type;
	public int Duration;
	public Unit Owner;

	public void Apply()
	{
		Type.Apply(Owner);
		Log.Message("UNITS", "Applying status effect: " + Type.Name + " to " + Owner.WorldObject.ID);
		Duration--;
	}
#if CLIENT
	public void DrawTooltip(Vector2 pos, float scale, SpriteBatch batch)
	{
		batch.DrawText("         Status Effect:\n[Green]"+Type.Name+"[-]\n"+Type.Tip, pos, scale, 50, Color.White);
	}
#endif

}
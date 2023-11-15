
using Myra.Graphics2D.UI;

namespace DefconNull.Rendering.CustomUIElements;

public class SoundButton : TextButton
{
	public override void OnMouseEntered()
	{
		Audio.PlaySound("select",null,0.5f);
		base.OnMouseEntered();
	}

	public override void OnTouchDown()
	{
		Audio.PlaySound("press",null,0.5f);
		base.OnTouchDown();
	}
}
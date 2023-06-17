using MultiplayerXeno;
using Myra.Graphics2D.UI;

namespace MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;

public class SoundButton : TextButton
{
	public override void OnMouseEntered()
	{
		Audio.PlaySound("UI/select",null,0.5f);
		base.OnMouseEntered();
	}

	public override void OnTouchDown()
	{
		Audio.PlaySound("UI/press",null,0.5f);
		base.OnTouchDown();
	}
}
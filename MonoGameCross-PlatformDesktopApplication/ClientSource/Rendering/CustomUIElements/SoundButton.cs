using MultiplayerXeno;
using Myra.Assets;
using Myra.Graphics2D.UI;

namespace MonoGameCrossPlatformDesktopApplication.ClientSource.Rendering.CustomUIElements;

public class SoundButton : TextButton
{
	public override void OnMouseEntered()
	{
		ResourceManager.GetSound("UI/select").Play();
		base.OnMouseEntered();
	}

	public override void OnTouchDown()
	{
		ResourceManager.GetSound("UI/press").Play();
		base.OnTouchDown();
	}
}
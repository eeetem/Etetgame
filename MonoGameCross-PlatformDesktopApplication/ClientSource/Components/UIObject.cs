namespace MultiplayerXeno
{
	public class UiObject
	{
		//percentages up to 100%
		public float width;
		public float height;
		public float xPos;
		public float yPos;
		
		public delegate void UIevent();

		public event UIevent Click;
		public event UIevent Hover;
		//event UIevent OnClick();

		public UiObject(float width, float height, float xPos, float yPos)
		{
			this.width = width;
			this.height = height;
			this.xPos = xPos;
			this.yPos = yPos;
		}


		public virtual void OnClick()
		{
			Click?.Invoke();
		}

		public virtual void OnHover()
		{
			Hover?.Invoke();
		}
	}
}
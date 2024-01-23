namespace DefconNull.WorldObjects;

public class Animation
{
	private int _currentFrame = 0;
	private readonly string _name;
	private readonly float _msPerFrame = 0;
	float _elapsedMsSinceLastFrame = 0;
	private readonly int _frameCount;

	public Animation(string name, int frameCount, int FPS=5)
	{
		this._name = name;
		this._frameCount = frameCount;
		this._msPerFrame = 1000 / (float)FPS;
	}

	public void Process(float msDelta)
	{
		_elapsedMsSinceLastFrame += msDelta;
		if(_elapsedMsSinceLastFrame>_msPerFrame)
		{
			_elapsedMsSinceLastFrame -= _msPerFrame;
			_currentFrame++;
		}
	}

	public bool IsOver => _currentFrame >= _frameCount;

	public string GetState(string spriteVariation)
	{
		if (IsOver) return "";
		return "/"+_name+spriteVariation+"/"+_currentFrame.ToString();
	}
}
namespace DefconNull.WorldObjects;

public class Animation
{
	private int _currentFrame = 0;
	private readonly int _frameCount = 0;
	private readonly string _name;
	private readonly float _msPerFrame = 0;
	float _elapsedMsSinceLastFrame = 0;

	public Animation(string name, int frameCount, int FPS)
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

	public string GetState()
	{
		return _name+_currentFrame.ToString();
	}
}
namespace DefconNull.WorldObjects;

public class Animation
{
	private int _currentFrame = 0;
	public readonly string Name;
	private readonly float _msPerFrame = 0;
	float _elapsedMsSinceLastFrame = 0;
	private readonly int _lastFrame;
	private readonly string _baseState = "";

	public Animation(string name, string baseState, int frameCount, int FPS=5)
	{
		_baseState = baseState;
		Name = name;
		_lastFrame = frameCount-1;
		_msPerFrame = 1000 / (float)FPS;
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

	public bool IsOver => _currentFrame >= _lastFrame;
	public bool ShouldStop;

	public string GetState(string spriteVariation)
	{
		var frame = _currentFrame;
		if(frame>_lastFrame)
		{
			frame = _lastFrame;
			ShouldStop = true;
		}

		return _baseState+"/" + Name + spriteVariation + "/" + frame;
	}
}
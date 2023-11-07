
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Action = DefconNull.World.WorldObjects.Units.Actions.Action;
using Color = Microsoft.Xna.Framework.Color;


namespace DefconNull.Rendering.PostProcessing;

public static class PostProcessing
{
	private static ContentManager content;
	private static GraphicsDevice graphicsDevice;
		
	public static UIElementShader CrtLightShaderPreset;
	public static UIElementShader Crtdissapation;
	public static CrtScreenShaderPreset CrtScreenPreset;

	public static void Init(ContentManager c, GraphicsDevice g)
	{
		content = c;
		graphicsDevice = g;


		CrtEffect = content.Load<Effect>("CompressedContent/shaders/CRT");
		UIGlowEffect = content.Load<Effect>("CompressedContent/shaders/Glow");
		ConnectionEffect = content.Load<Effect>("CompressedContent/shaders/lc");
		ColorEffect = content.Load<Effect>("CompressedContent/shaders/colorshader");
		DistortEffect = content.Load<Effect>("CompressedContent/shaders/distort");
		OutLineEffect = content.Load<Effect>("CompressedContent/shaders/outline");
		
		
		Game1.instance.IsMouseVisible = false;
		int countx;
		int county;
		cursorTextures = Utility.SplitTexture(content.Load<Texture2D>("CompressedContent/textures/mouse"), 40, 40, out countx, out county);


		combinedSpriteBatch = new SpriteBatch(g);
		emptyTexture = new Texture2D(g, 1, 1);
		CrtLightShaderPreset = new UIElementShader(false);
		Crtdissapation = new UIElementShader(true);
		CrtScreenPreset = new CrtScreenShaderPreset();
		Crtdissapation.dissapation = true;
			

				
				
				
				
		//default effect parameters

		EffectParams["hardScan"] = 0f;
		EffectParams["hardPix"] = 0f;
		EffectParams["warpX"] = 10;
		EffectParams["warpY"] = 10;
		EffectParams["maskDark"] = 0.5f;
		EffectParams["maskLight"] = 1.5f;
		EffectParams["scaleInLinearGamma"] = 1f;
		EffectParams["shadowMask"] = 0f;
		EffectParams["brightboost"] = 1f;
		EffectParams["hardBloomScan"] = -1.5f;
		EffectParams["hardBloomPix"] = -2.0f;
		EffectParams["bloomAmount"] = 100f;
		EffectParams["shape"] = 10;

		EffectParams["noise"] = 0f;

			
			
		//loose conection
		EffectParams["clmagnitude"] = 0.01f;
		EffectParams["clalpha"] = 1;
		EffectParams["clspeed"] = 1f;
		EffectParams["overlayalpha"] = 0f;
			
			
			
		//colorshader
		EffectParams["minR"] = 0f;
		EffectParams["minG"] = 0f;
		EffectParams["minB"] = 0f;
		EffectParams["maxR"] = 1f;
		EffectParams["maxG"] = 1f;
		EffectParams["maxB"] = 1f;
		EffectParams["tintR"] = 1f;
		EffectParams["tintG"] = 1f;
		EffectParams["tintB"] = 1f;
			
			
		//distort
		EffectParams["dxspeed"] = 50;
		EffectParams["dxamplitude"] = 80f;
		EffectParams["dxfrequency"] = 80f;
			
		EffectParams["dyspeed"] = 50;
		EffectParams["dyamplitude"] = 80f;
		EffectParams["dyfrequency"] = 80f;

		SetOutlineEffectColor(Color.Black);


		foreach (var p in EffectParams)
		{
			if (!DefaultParams.ContainsKey(p.Key))
			{
				DefaultParams.Add(p.Key, p.Value);
			}
		}
	
		Task.Factory.StartNew(StartingTweens);
	}

	private static void StartingTweens()
	{
		AddTween("noise", 1f, 100f, false);
		AddTween("clmagnitude", 100, 100, false);
		AddTween("dxspeed", 5, 1, false);
		AddTween("dyspeed", 5, 1, false);
		AddTween("dxfrequency", 0, 7f, false);
		AddTween("dyfrequency", 0, 7f, false);
		AddTween("dyamplitude", 0, 7f, false);
		AddTween("dxamplitude", 0, 7f, false);
			
		AddTween("clmagnitude", 50, 3, false);
		AddTween("bloomAmount", 0.15f, 5, false);
		AddTween("warpX", 0.01f, 5, false);
		AddTween("warpY", 0.01f, 5, false);
		AddTween("clspeed", 7f, 2f, false);
		AddTween("clalpha", 0.07f, 1f, false);
		AddTween("shape", 0.1f, 5f, false);
		AddTween("noise", 0.1f, 10f, false);



		AddTween("noise", 0.002f, 5f, false);
		AddTween("clmagnitude", 0.01f, 2f, false);
			

			
		Console.Write("added tweens");

	}

	private static Texture2D[] cursorTextures;
	private static Texture2D emptyTexture;
	private static Texture2D overlayTexture;

	private static SpriteBatch combinedSpriteBatch;

	public static Effect CrtEffect = null!;
	public static Effect UIGlowEffect = null!;
	public static Effect ConnectionEffect = null!;
	public static Effect ColorEffect = null!;
	public static Effect DistortEffect = null!;
	public static Effect OutLineEffect = null!;

	private static readonly Dictionary<string, float> EffectParams = new Dictionary<string, float>();
	private static readonly Dictionary<string, float> DefaultParams = new Dictionary<string, float>();

	private static float clcounter;
	private static float dxcounter;
	private static float dycounter;

	private static RenderTarget2D combinedRender;
	private static RenderTarget2D combinedRender2;
	//When we need to draw to the screen, it's done here.


	public static void SetOutlineEffectColor(Color c)
	{
		var vec = new Vector4(c.R, c.G, c.B, c.A);
		OutLineEffect.Parameters["outlineColor"].SetValue(vec);
	}


	static float GetNoise()
	{
		float noiseAmount = ((float) Random.Shared.NextDouble() - (float) Random.Shared.NextDouble()) * EffectParams["noise"];
		return noiseAmount;
	}

	public static void RemakeRenderTarget()
	{
			
		combinedRender?.Dispose();
		combinedRender = new RenderTarget2D(graphicsDevice, Game1.resolution.X,Game1.resolution.Y);
		combinedRender2?.Dispose();
		combinedRender2 = new RenderTarget2D(graphicsDevice,Game1.resolution.X,Game1.resolution.Y);
			
	}

	public static void ApplyScreenUICrt(Vector2 textureVector2)
	{
		CrtScreenPreset.Apply(CrtEffect,textureVector2);
	}

	public static void ApplyUIEffect(Vector2 textureVector2,bool disapate = false)
	{
		if (disapate)
		{
			Crtdissapation.Apply(UIGlowEffect,textureVector2);
			return;
		}
		
		CrtLightShaderPreset.Apply(UIGlowEffect,textureVector2);
	}


	private static float timeToFlicker = 10000;
	private static int flickerIndex;
	public static void Apply(float deltaTime)
	{
		

		clcounter += deltaTime/1000 * EffectParams["clspeed"];
		dxcounter += deltaTime/1000 * EffectParams["dxspeed"];
		dycounter += deltaTime/1000 * EffectParams["dyspeed"];

		CrtLightShaderPreset.Update(deltaTime);

		
		Crtdissapation.Update(deltaTime*4f);
		CrtScreenPreset.Update(deltaTime);


			

		if (combinedRender == null || combinedRender2 == null)//shitcode
		{ 
			RemakeRenderTarget(); 
			return;
		}

		//init effects
		ProcessTweens(deltaTime);

			

		CrtEffect.Parameters["hardScan"]?.SetValue(EffectParams["hardScan"] + GetNoise());
		CrtEffect.Parameters["hardPix"]?.SetValue(EffectParams["hardPix"] + GetNoise());
		CrtEffect.Parameters["warpX"]?.SetValue(0.05f);
		CrtEffect.Parameters["warpY"]?.SetValue(0.05f);
		CrtEffect.Parameters["maskDark"]?.SetValue(EffectParams["maskDark"] + GetNoise() * 0.1f);
		CrtEffect.Parameters["maskLight"]?.SetValue(EffectParams["maskLight"] + GetNoise() * 0.1f);
		CrtEffect.Parameters["scaleInLinearGamma"]?.SetValue(2);
		CrtEffect.Parameters["shadowMask"]?.SetValue(EffectParams["shadowMask"] + GetNoise() * 1f);
		CrtEffect.Parameters["brightboost"]?.SetValue(EffectParams["brightboost"] + GetNoise() * 0.05f);
		CrtEffect.Parameters["hardBloomScan"]?.SetValue(EffectParams["hardBloomScan"] + GetNoise() * 0.01f);
		CrtEffect.Parameters["hardBloomPix"]?.SetValue(EffectParams["hardBloomPix"] + GetNoise() * 0.01f);
		CrtEffect.Parameters["bloomAmount"]?.SetValue(5);
		CrtEffect.Parameters["shape"]?.SetValue(0f);
		
		CrtEffect.Parameters["textureSize"]
			.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
		CrtEffect.Parameters["videoSize"]
			.SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
		CrtEffect.Parameters["outputSize"].SetValue(new Vector2(graphicsDevice.Viewport.Width,
			graphicsDevice.Viewport.Height));
			

				
			
			
		ConnectionEffect.Parameters["textureSize"].SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
		ConnectionEffect.Parameters["videoSize"].SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
		ConnectionEffect.Parameters["fps"].SetValue(clcounter);
		ConnectionEffect.Parameters["staticAlpha"].SetValue(EffectParams["clalpha"] + GetNoise() * 0.01f);
		ConnectionEffect.Parameters["magnitude"].SetValue(EffectParams["clmagnitude"] + GetNoise() * 1f);
		ConnectionEffect.Parameters["overlayalpha"].SetValue(EffectParams["overlayalpha"] + GetNoise() * 0.05f);
		if (overlayTexture != null)
		{
			ConnectionEffect.Parameters["overlay"].SetValue(overlayTexture);
		}
		else
		{
			ConnectionEffect.Parameters["overlay"].SetValue(emptyTexture);
		}

		ColorEffect.Parameters["max"].SetValue(new Vector4(EffectParams["maxR"]+ GetNoise() * 0.5f,EffectParams["maxG"]+ GetNoise() *0.5f,EffectParams["maxB"]+ GetNoise() *0.5f,1));
		ColorEffect.Parameters["min"].SetValue(new Vector4(EffectParams["minR"]+ GetNoise() * 0.5f,EffectParams["minG"]+ GetNoise() *0.5f,EffectParams["minB"]+ GetNoise() * 0.5f,1));
		ColorEffect.Parameters["tint"].SetValue(new Vector4(EffectParams["tintR"]+ GetNoise() * 1f ,EffectParams["tintG"]+ GetNoise() *1f,EffectParams["tintB"]+ GetNoise() * 1f,1));
			

		DistortEffect.Parameters["xfps"].SetValue(dxcounter);
		DistortEffect.Parameters["yfps"].SetValue(dycounter);
		DistortEffect.Parameters["xamplitude"].SetValue(EffectParams["dxamplitude"]+ GetNoise() * 0.1f);
		DistortEffect.Parameters["yamplitude"].SetValue(EffectParams["dyamplitude"]+ GetNoise() * 0.1f);
		DistortEffect.Parameters["xfrequency"].SetValue(EffectParams["dxfrequency"]+ GetNoise() * 0.1f);
		DistortEffect.Parameters["yfrequency"].SetValue(EffectParams["dyfrequency"]+ GetNoise() * 0.1f);
		
		

		
		
			

	
		Texture2D cursorTexture2D  = cursorTextures[0];


		combinedSpriteBatch.GraphicsDevice.SetRenderTarget(combinedRender);
		combinedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);

		combinedSpriteBatch.Draw(Game1.GlobalRenderTarget, Game1.GlobalRenderTarget.Bounds, Color.White);

		combinedSpriteBatch.Draw(cursorTexture2D, new Vector2(Mouse.GetState().X - cursorTexture2D.Width / 2, Mouse.GetState().Y - cursorTexture2D.Height / 2),Color.White);
		
		combinedSpriteBatch.End();
		

	





		combinedSpriteBatch.GraphicsDevice.SetRenderTarget(combinedRender2);
		combinedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone,ColorEffect);
		combinedSpriteBatch.Draw(combinedRender, combinedRender.Bounds, Color.White);
		combinedSpriteBatch.End();
	
		combinedSpriteBatch.GraphicsDevice.SetRenderTarget(combinedRender);
		combinedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone,DistortEffect);
		combinedSpriteBatch.Draw(combinedRender2, combinedRender2.Bounds, Color.White);
		combinedSpriteBatch.End();
		
		combinedSpriteBatch.GraphicsDevice.SetRenderTarget(combinedRender2);
		combinedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone,ConnectionEffect);
		combinedSpriteBatch.Draw(combinedRender, combinedRender.Bounds, Color.White);
		combinedSpriteBatch.End();	
		
	
		combinedSpriteBatch.GraphicsDevice.SetRenderTarget(combinedRender);
		combinedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone,null);
		combinedSpriteBatch.Draw(combinedRender2, combinedRender2.Bounds, Color.White);
		combinedSpriteBatch.End();
		



		combinedRender.GraphicsDevice.SetRenderTarget(null);

		
		combinedSpriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
		combinedSpriteBatch.Draw(combinedRender, combinedRender.GraphicsDevice.Viewport.Bounds, Color.White);
		combinedSpriteBatch.End();
			
	

	

	}


		
	private static List<EventWaitHandle> threadQueue = new List<EventWaitHandle>();
	private static Dictionary<string, List<EventWaitHandle>> awaitingthreadQueue = new();
		
	private static readonly object syncObj = new object();
	public static void AddTweenReturnTask(string parameter,float target, float speed, bool wipeQueue = false, float returnSpeed = 10f)
	{
		Task.Factory.StartNew(() =>
		{
			
			var startValue = DefaultParams[parameter];
				

			AddTween(parameter, target, speed, wipeQueue);
			Thread.Sleep((int) (1000f *  (10f/speed)));
			AddTween(parameter, startValue, returnSpeed, true);
		});
	}
		
	public static void AddTweenTask(string parameter,float target, float speed, bool wipeQueue = false)
	{
		Task.Factory.StartNew(() =>
		{
			AddTween(parameter, target, speed, wipeQueue);
		});
	}
	public static void AddTween(string parameter,float target, float speed, bool wipeQueue = false)
	{

		lock (syncObj)
		{
			if (!awaitingthreadQueue.ContainsKey(parameter))
			{
				awaitingthreadQueue[parameter] = new List<EventWaitHandle>(); //create queue for each parameter
			}

			if (wipeQueue)
			{
				awaitingthreadQueue[parameter] = new List<EventWaitHandle>();
				int index = tweens.FindIndex(x => x.parameter == parameter);
				if (index > -1)
				{
					tweens.RemoveAt(index);
				}

			}
		}

		if (tweens.FindIndex(x => x.parameter == parameter) != -1) //queue up if parameter is being currently tweened
		{

			var eventWaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
			awaitingthreadQueue[parameter].Add(eventWaitHandle);
			//System.Console.WriteLine("stoped by awaiting for: "+parameter);
			eventWaitHandle.WaitOne();
			eventWaitHandle.Close();
		}
		lock (syncObj)
		{
			//	System.Console.WriteLine("passed and set: "+parameter);
			Tween t = new Tween(parameter,EffectParams[parameter],speed,target);
			tweens.Add(t);

		}


	}

	//might be better to make this a dict and only allow 1 tween per parameter
	private static List<Tween> tweens = new List<Tween>();
	private static void ProcessTweens(float deltaTime)
	{
		lock (syncObj)
		{
			foreach (Tween t in tweens.ToList())
			{
				if (t.counter > 1)
				{
					tweens.Remove(t);
					if (awaitingthreadQueue[t.parameter].Count > 0)
					{
						EventWaitHandle nextThread = awaitingthreadQueue[t.parameter][^1];

						nextThread.Set();
						awaitingthreadQueue[t.parameter].RemoveAt(awaitingthreadQueue[t.parameter].Count - 1);

					}

					continue;
				}

				t.Lerp(deltaTime);
			}
		}
	}

	public class Tween
	{
		public Tween(string parameter, float start, float speed, float endValue)
		{
			this.parameter = parameter;
			this.speed = speed;
			this.endValue = endValue;
			startValue = start;
				
		}

		public float counter;
		public string parameter;
		public float startValue;
		public float endValue;
		public float speed;
	

		public void Lerp(float deltaTime)
		{
				
			counter += deltaTime / 10000 * speed;
			EffectParams[parameter] = Utility.Lerp(startValue, endValue, counter);

		}

	}

}
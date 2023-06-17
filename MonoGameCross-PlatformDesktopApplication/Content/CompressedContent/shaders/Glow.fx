#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float blurAmmount =1;
float glowAmmount =1;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 Cords : TEXCOORD0;
};

float4 Blur1(float2 vTex : TEXCOORD0, float4 theResult : COLOR0) 
{ 
	theResult=0; 
	for (float aU=-3;aU<=2;aU++) 
	{ 
		theResult+=tex2D(SpriteTextureSampler, float2(vTex.x+blurAmmount*aU,vTex.y)); 
       	theResult+=tex2D(SpriteTextureSampler, float2(vTex.x-blurAmmount*aU,vTex.y)); 
       	theResult+=tex2D(SpriteTextureSampler, float2(vTex.x,vTex.y-blurAmmount*aU)); 
       	theResult+=tex2D(SpriteTextureSampler, float2(vTex.x,vTex.y+blurAmmount*aU)); 
	} 
	theResult/=20; 
	return theResult;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 color = tex2D(SpriteTextureSampler, input.Cords);
    float4 blur = Blur1(input.Cords, color);
    color += blur*glowAmmount;
  //  vec3 col = vec3(step(0., -d));

   // col += clamp(vec3(0.001/d), 0., 1.) * 12. * glowAmmount; // add glow

  //  col *= vec3(1, 1, 1);

//

	return color;

}

technique BasicColorDrawing
{
	pass P0
	{
		//VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
}; 
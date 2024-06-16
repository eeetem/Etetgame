#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float lifeTime;

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

float2 random2(float2 st){
    st = float2( dot(st,float2(127.1,311.7)),
              dot(st,float2(269.5,183.3)) );
    return -1.0 + 2.0*frac(sin(st)*43758.5453123);
}
float noise(float2 st) {
    float2 i = floor(st);
    float2 f = frac(st);

    float2 u = f*f*(3.0-2.0*f);

    return lerp( lerp( dot( random2(i + float2(0.0,0.0) ), f - float2(0.0,0.0) ),
                     dot( random2(i + float2(1.0,0.0) ), f - float2(1.0,0.0) ), u.x),
                lerp( dot( random2(i + float2(0.0,1.0) ), f - float2(0.0,1.0) ),
                     dot( random2(i + float2(1.0,1.0) ), f - float2(1.0,1.0) ), u.x), u.y);
}


float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.Cords);
	float lf = lifeTime*100;
    float inverse = 1.0 - lifeTime;
	float ammount = abs(noise(input.Cords*0.5*lf))/inverse;
    
    color =  color*float4(ammount,ammount,0,ammount);
	
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
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
float4 tint;
float4 max;
float4 min;
float grayscaleMag;

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

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(SpriteTextureSampler, input.Cords);
	color = color * tint;
	color = clamp(color,min,max);
	
	
	    // Calculate the grayscale value by averaging the RGB channels
    float grayscale = dot(color.rgb, float3(0.3333, 0.3333, 0.3333));
    
        // Interpolate between full color and grayscale based on the "transition" parameter
    float3 finalColor = lerp(color.rgb, grayscale, grayscaleMag);
    
    color = float4(finalColor, 1.0f);
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
#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;


sampler2D Sampler = sampler_state{
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 Cords : TEXCOORD0;
};

float4 outlineColor;

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 color = tex2D(Sampler, input.Cords);

	
    float4 neighbourColor[4];
    neighbourColor[0] = tex2D(Sampler, input.Cords + float2(0, 1));
    neighbourColor[1] = tex2D(Sampler, input.Cords + float2(0, -1));
    neighbourColor[2] = tex2D(Sampler, input.Cords + float2(1, 0));
    neighbourColor[3] = tex2D(Sampler, input.Cords + float2(-1, 0));
  
  
    for(int i = 0; i < 4; i++)
    {
       if(neighbourColor[i].a < 1)
       {
       		if(color.a > 0 || color.r < 0 || color.g < 0 || color.b < 0)
	   			color = outlineColor;
            break;
        }
    }

	
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
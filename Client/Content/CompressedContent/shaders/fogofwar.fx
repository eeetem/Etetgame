#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};


float DistanceFactorBlend = 20.5f;

float Halo = 0.5f;


float TextureWidth;
float TextureHeight;




float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
        float2 texCoord = input.TextureCoordinates;
      // Inside PixelShaderFunction, calculate texSize using the global variables
      float2 texSize = float2(1.0 / TextureWidth, 1.0 / TextureHeight);
        float4 halo = float4(0, 0, 0, 0);
    
        for (int i = -20; i <= 20; i++)
        {
            
            for (int j = -20; j <= 20; j++)
            {
               
                if (i == 0 && j == 0) continue; // Skip the current pixel
                float2 neighborCoord = texCoord + float2(i, j) * texSize;
                float4 neighbor = tex2D(SpriteTextureSampler, neighborCoord);
                float distance = length(float2(i, j));
                float distanceFactor = 1.0 - (distance / DistanceFactorBlend); // Adjusted for the new range
                if (neighbor.a > 0 && distanceFactor > 0) // Example condition, adjust based on actual use case
                {
                    neighbor.rgb *= distanceFactor; // Modify this logic to blend halo effect smoothly
                    halo += neighbor;
                }
            }
        }
    

    // Combine the two images with adjusted halo effect
    float4 result = halo * Halo; // Adjust Halo value for blending

    return result;
}

technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
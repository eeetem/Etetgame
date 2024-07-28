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


float BloomIntensity = 2;
float BaseIntensity = 1;

float BloomSaturation = 1;
float BaseSaturation = 1;

float BloomThreshold = 1000;

float Halo = 0.5f;

float TextureWidth;
float TextureHeight;


// Helper for modifying the saturation of a color.
float4 AdjustSaturation(float4 color, float saturation)
{
    float a = color.a;
    // The constants 0.3, 0.59, and 0.11 are chosen because the
    // human eye is more sensitive to green light, and less to blue.
    float grey = dot(color, float3(0.3, 0.59, 0.11));

    float4 res = lerp(grey, color, saturation);
    res.a = a;

    return res;
}




float4 GetBloomTexture(float4 c)
{
     // Above threshold, proceed with original bloom calculation.
     return saturate((c - BloomThreshold) / (1 - BloomThreshold));
}
float4 GetBloomForPixel(float2 texCoord,float4 color)
{    
         // Look up the bloom and original base image colors.
         float4 bloom = GetBloomTexture(tex2D(SpriteTextureSampler, texCoord))*color;
         float4 base = tex2D(SpriteTextureSampler, texCoord)*color;
         
         // Adjust color saturation and intensity.
         float a = base.a;
         bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
         base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
         base.a = a;
         return  base + bloom;
}
float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

    float2 texCoord = input.TextureCoordinates;
    
    float4 bloom = GetBloomForPixel(texCoord,input.Color);

   // Inside PixelShaderFunction, calculate texSize using the global variables
      float2 texSize = float2(1.0 / TextureWidth, 1.0 / TextureHeight);
       float4 halo = float4(0, 0, 0, 0);
    
        for (int i = -3; i <= 3; i++)
        {
            for (int j = -3; j <= 3; j++)
            {
                if (i == 0 && j == 0) continue; // Skip the current pixel
                float2 neighborCoord = texCoord + float2(i, j) * texSize;
                float4 neighbor = GetBloomForPixel(texCoord,input.Color);
                float distanceFactor = 1.0 - length(float2(i, j)) * 0.5; // Adjust for smoother blending
                // Apply a condition to ensure halo is only added around intended pixels
                if (distanceFactor > 0) // Example condition, adjust based on actual use case
                {
                    neighbor.rgb *= distanceFactor; // Modify this logic to blend halo effect smoothly
                    halo += neighbor;
                }
            }
        }
    // Combine the two images with adjusted halo effect
    float4 result =  bloom + halo * Halo; // Adjust Halo value for blending

    // Darken down the base image in areas where there is a lot of bloom,
    // to prevent things looking excessively burned-out.
    //base *= (1 - saturate(bloom));
    
    // Combine the two images.
    return result;
}


technique BloomCombine
{
    pass Pass1
    {
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}
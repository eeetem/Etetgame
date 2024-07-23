// Pixel shader combines the bloom image with the original
// scene, using tweakable intensity levels and saturation.
// This is the final step in applying a bloom postprocess.

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
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
float4 PixelShaderFunction(float2 texCoord : TEXCOORD0) : COLOR0
{
    // Look up the bloom and original base image colors.
    float4 bloom = GetBloomTexture(tex2D(SpriteTextureSampler, texCoord));
    float4 base = tex2D(SpriteTextureSampler, texCoord);
    
    // Adjust color saturation and intensity.
    float a = base.a;
    bloom = AdjustSaturation(bloom, BloomSaturation) * BloomIntensity;
    base = AdjustSaturation(base, BaseSaturation) * BaseIntensity;
    base.a = a;
    
    
      // Inside PixelShaderFunction, calculate texSize using the global variables
      float2 texSize = float2(1.0 / TextureWidth, 1.0 / TextureHeight);
        float4 halo = float4(0, 0, 0, 0);
    
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue; // Skip the current pixel
                float2 neighborCoord = texCoord + float2(i, j) * texSize;
                float4 neighbor = tex2D(SpriteTextureSampler, neighborCoord);
                float distanceFactor = 1.0 - length(float2(i, j)) * 0.5; // Adjust for smoother blending
                // Apply a condition to ensure halo is only added around intended pixels
                if (neighbor.a > 0 && distanceFactor > 0) // Example condition, adjust based on actual use case
                {
                    neighbor.rgb *= distanceFactor; // Modify this logic to blend halo effect smoothly
                    halo += neighbor;
                }
            }
        }
    

    // Combine the two images with adjusted halo effect
    float4 result = base + bloom + halo * Halo; // Adjust Halo value for blending

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
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
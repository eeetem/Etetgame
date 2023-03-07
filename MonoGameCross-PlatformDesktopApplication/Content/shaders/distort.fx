#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
	#define HLSL_4
#endif



struct out_vertex
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 texCoord : TEXCOORD0;
};


float yfps;
float xfps;
float xamplitude = 1;
float xfrequency = 1;
float yamplitude = 1;
float yfrequency = 1;


sampler2D Sampler = sampler_state{
    AddressU = Wrap;
    AddressV = Wrap;
};


float4 main_fragment(out_vertex VOUT) : COLOR0
{
    float yt = 0.01*(-yfps/1000*130.0);
    float xt = 0.02*(-xfps/1000*130.0);



    
    
    float y = sin(VOUT.texCoord.x * yfrequency);
    
    y += sin(VOUT.texCoord.x*yfrequency*2.1 + yt)*4.5;
    y += sin(VOUT.texCoord.x*yfrequency*1.72 + yt*1.121)*4.0;
    y += sin(VOUT.texCoord.x*yfrequency*2.221 + yt*0.437)*5.0;
    y += sin(VOUT.texCoord.x*yfrequency*3.1122+ yt*4.269)*2.5;
    y *= yamplitude*0.06;
    
    float x = sin(VOUT.texCoord.y * xfrequency);
    x += sin(VOUT.texCoord.y*xfrequency*2.1 + xt)*4.5;
    x += sin(VOUT.texCoord.y*xfrequency*1.72 + xt*1.121)*4.0;
    x += sin(VOUT.texCoord.y*xfrequency*2.221 + xt*0.437)*5.0;
    x += sin(VOUT.texCoord.y*xfrequency*3.1122+ xt*4.269)*2.5;
    x *= xamplitude*0.06;
    
    
    float4 final = tex2D(Sampler,VOUT.texCoord+ float2(x,y));

	return final;
}

technique
{
    pass P0
	{
	    PixelShader = compile PS_SHADERMODEL main_fragment();
	}
}


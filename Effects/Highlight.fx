sampler tex : register(s0);

float3 RGB2HSV(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsb2rgb(float3 c)
{
    float3 rgb = clamp(abs(fmod(c.x * 6.0 + float3(0.0, 4.0, 2.0), 6) - 3.0) - 1.0, 0, 1);
    rgb = rgb * rgb * (3.0 - 2.0 * rgb);
    return c.z * lerp(float3(1, 1, 1), rgb, c.y);
}

float4 PixelShaderFunction(float2 coords : TEXCOORD0, float4 inC : COLOR0) : COLOR0
{
    float4 c = tex2D(tex, coords);
    
    if (!any(c))
        return float4(0, 0, 0, 0);
    
    float3 hsv = RGB2HSV(c.rgb);
    
    c.rgb *= hsv.y;
    
    return c * inC;
}

technique Technique1
{
    pass HighlightPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
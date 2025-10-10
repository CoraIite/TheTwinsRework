sampler _MainTex : register(s0);
float blur;

float4 PixelShaderFunction(float2 uv : TEXCOORD0, float4 inC : COLOR0) : COLOR0
{
    float offset = blur * 0.0625f;

    // 左上
    float4 color = tex2D(_MainTex, float2(uv.x - offset, uv.y - offset)) * 0.0947416f;
    // 上
    color += tex2D(_MainTex, float2(uv.x, uv.y - offset)) * 0.118318f;
    // 右上
    color += tex2D(_MainTex, float2(uv.x + offset, uv.y + offset)) * 0.0947416f;
    // 左
    color += tex2D(_MainTex, float2(uv.x - offset, uv.y)) * 0.118318f;
    // 中
    color += tex2D(_MainTex, float2(uv.x, uv.y)) * 0.147761f;
    // 右
    color += tex2D(_MainTex, float2(uv.x + offset, uv.y)) * 0.118318f;
    // 左下
    color += tex2D(_MainTex, float2(uv.x - offset, uv.y + offset)) * 0.0947416f;
    // 下
    color += tex2D(_MainTex, float2(uv.x, uv.y + offset)) * 0.118318f;
    // 右下
    color += tex2D(_MainTex, float2(uv.x + offset, uv.y - offset)) * 0.0947416f;

    return color * inC;
}

technique Technique1
{
    pass CosmosBlurPass
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
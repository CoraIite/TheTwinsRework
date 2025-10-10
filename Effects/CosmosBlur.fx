sampler _MainTex : register(s0);
float blur;

float4 PixelShaderFunction(float2 uv : TEXCOORD0, float4 inC : COLOR0) : COLOR0
{
    float offset = blur * 0.0625f;

    // ����
    float4 color = tex2D(_MainTex, float2(uv.x - offset, uv.y - offset)) * 0.0947416f;
    // ��
    color += tex2D(_MainTex, float2(uv.x, uv.y - offset)) * 0.118318f;
    // ����
    color += tex2D(_MainTex, float2(uv.x + offset, uv.y + offset)) * 0.0947416f;
    // ��
    color += tex2D(_MainTex, float2(uv.x - offset, uv.y)) * 0.118318f;
    // ��
    color += tex2D(_MainTex, float2(uv.x, uv.y)) * 0.147761f;
    // ��
    color += tex2D(_MainTex, float2(uv.x + offset, uv.y)) * 0.118318f;
    // ����
    color += tex2D(_MainTex, float2(uv.x - offset, uv.y + offset)) * 0.0947416f;
    // ��
    color += tex2D(_MainTex, float2(uv.x, uv.y + offset)) * 0.118318f;
    // ����
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
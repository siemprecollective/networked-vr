Texture2D ScreenTexture;
SamplerState ScreenTextureSampler {
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VOut
{
    float4 position : SV_POSITION;
    float2 texCoord : TEXCOORD; 
};

VOut VShader(float4 position : POSITION, float2 texCoord : TEXCOORD)
{
    VOut output;

    output.position = position;
    output.texCoord = texCoord;

    return output;
}

float4 PShader(float4 position : SV_POSITION, float4 color : COLOR, float2 texCoord : TEXCOORD) : SV_TARGET
{
    float4 texColor = ScreenTexture.Sample(ScreenTextureSampler, texCoord);
    texColor.a = 1;
    return texColor;
}

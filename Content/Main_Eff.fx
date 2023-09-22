float4x4 World;
float4x4 View;
float4x4 Projection;
sampler2D Texture : register(s0);

struct VertexShaderInput {
    // float4 TexCoord : TEXCOORD0;
    float4 Position : POSITION0;
    // float4 Normal : NORMAL;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput {
    float4 Position : POSITION0;
  	float4 Color : COLOR0;
  	float2 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord / 128;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
    return tex2D(Texture, input.TexCoord);
}

technique Ambient {
    pass Pass1 {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

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

float Floor(float val) {
    return val - (val % 1);
}

float2 FloorCoord(float2 val) {
    return float2(Floor(val.x), Floor(val.y));
}

VertexShaderOutput VertexShaderFunction(VertexShaderInput input) {
    VertexShaderOutput output;
    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0 {
    float4 texcolor = tex2D(Texture, FloorCoord(input.TexCoord) / 128);
    return float4((texcolor * input.Color).rgb, texcolor.a);
}

technique Ambient {
    pass Pass1 {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}

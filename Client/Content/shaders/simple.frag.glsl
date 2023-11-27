#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 1) in vec4 fsin_Color;
layout(location = 2) in float fsin_Distance;
layout(location = 3) in vec2 fsin_UvMin;
layout(location = 4) in vec2 fsin_UvMax;

layout(location = 0) out vec4 fsout_Color;

layout (set = 1, binding = 0) uniform sampler TextureSampler;
layout (set = 1, binding = 1) uniform texture2D Texture;

void main() {
    vec2 inverseTexSize = textureSize(sampler2D(Texture, TextureSampler), 0);
    vec2 texSize = 1 / inverseTexSize;
    
    vec2 oldUv = fsin_texCoords;
    
    vec2 boxSize = clamp((abs(dFdx(oldUv)) + abs(dFdy(oldUv))) * inverseTexSize, 0.0001, 0.9999);
    
    vec2 tx = oldUv * inverseTexSize - 0.5 * boxSize;
    vec2 tfract = fract(tx);
    vec2 txOffset = smoothstep(1 - boxSize, vec2(1), tfract);
    
    vec2 newUv = clamp((tx - tfract + 0.5 + txOffset) * texSize, fsin_UvMin + texSize * 0.5, fsin_UvMax - texSize * 0.5);
    
    vec4 sampledColor = textureGrad(sampler2D(Texture, TextureSampler), newUv, dFdx(newUv), dFdy(newUv));
    fsout_Color = sampledColor * fsin_Color;
}

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
    
    vec2 boxSize = (abs(dFdx(oldUv)) + abs(dFdy(oldUv))) * inverseTexSize * 0.8;
    
    vec2 tx = oldUv * inverseTexSize - 0.5 * boxSize;
    vec2 tfract = fract(tx);
    vec2 txOffset = smoothstep(1 - boxSize, vec2(1), tfract);
    
    vec2 tmin = fsin_UvMin + texSize;
    vec2 tminhalf = fsin_UvMin + 0.5 * texSize;
    vec2 tmax = fsin_UvMax - texSize;
    vec2 tmaxhalf = fsin_UvMax - 0.5 * texSize;
    
    vec2 newUvMin = clamp((tx - tfract + 0.5) * texSize, tminhalf, tmaxhalf);
    vec2 newUvMax = clamp((tx - tfract + 0.5 + txOffset) * texSize, tminhalf, tmaxhalf);
    
    vec4 sampledColor = (
        texture(sampler2D(Texture, TextureSampler), newUvMin) +
        texture(sampler2D(Texture, TextureSampler), newUvMax) +
        texture(sampler2D(Texture, TextureSampler), vec2(newUvMin.x, newUvMax.y)) +
        texture(sampler2D(Texture, TextureSampler), vec2(newUvMax.x, newUvMin.y))
    ) / 4;
    fsout_Color = sampledColor * fsin_Color;
}

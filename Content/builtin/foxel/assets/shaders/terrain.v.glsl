#version 440

#include "common/camera_and_model.glsl"
#include "common/filtering.glsl"

layout (set = 2, binding = 0) uniform sampler TextureSampler;
layout (set = 2, binding = 1) uniform texture2D Texture;

void UnpackUv(uint packedUv, out vec2 uv) {
    const float _1_65535 = 1 / 65535.0f;
    uv = vec2(
        (packedUv & 0xFFFFu) * _1_65535,
        ((packedUv >> 16u) & 0xFFFFu) * _1_65535
    );
}

void Unpack(uint packedColorAndAo, uint packedUv, uint packedUvMin, uint packedUvMax, out vec4 colorAndAo, out vec2 uv, out vec2 uvMin, out vec2 uvMax){
    const float _1_255 = 1.0f / 255.0f;
    colorAndAo = vec4(
        (packedColorAndAo & 0xFFu) * _1_255,
        ((packedColorAndAo >> 8u) & 0xFFu) * _1_255,
        ((packedColorAndAo >> 16u) & 0xFFu) * _1_255,
        ((packedColorAndAo >> 24u) & 0xFFu) * _1_255
    );

    UnpackUv(packedUv, uv);
    UnpackUv(packedUvMin, uvMin);
    UnpackUv(packedUvMax, uvMax);
}

vert_param(0, vec3 vs_Position)
vert_param(1, uint vs_PackedColorAndAo)
vert_param(2, uint vs_PackedUv)
vert_param(3, uint vs_PackedUvMin)
vert_param(4, uint vs_PackedUvMax)
frag_param(0, vec3 fs_Color)
frag_param(1, vec2 fs_Uv)
frag_param(2, vec2 fs_UvMin)
frag_param(3, vec2 fs_UvMax)
out_param(0, vec4 o_Color)

#ifdef VERTEX

void vert(){
    vec4 colorAndAo;
    Unpack(vs_PackedColorAndAo, vs_PackedUv, vs_PackedUvMin, vs_PackedUvMax, colorAndAo, fs_Uv, fs_UvMin, fs_UvMax);
    fs_Color = colorBlendUniform(colorAndAo.rgb, vec3(0), colorAndAo.a);

    vec4 pos = ModelVertex(vs_Position);
    gl_Position = pos;
}

#endif
#ifdef FRAGMENT

void frag(){
    vec4 sampledColor = colorBlendAverage(interpolatePixels(fs_Uv, fs_UvMin, fs_UvMax, Texture, TextureSampler));
    o_Color = vec4(colorBlendUniform(sampledColor.rgb, sampledColor.rgb * fs_Color, 0.15), sampledColor.a);
}

#endif

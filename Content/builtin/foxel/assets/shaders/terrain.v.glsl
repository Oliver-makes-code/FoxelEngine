#version 440

#include "common/camera_and_model.glsl"

layout (set = 2, binding = 0) uniform sampler TextureSampler;
layout (set = 2, binding = 1) uniform texture2D Texture;

void UnpackUv(uint packedUv, out vec2 uv) {
    const float _1_65535 = 1 / 65535.0f;
    uv = vec2(
        (packedUv & 0xFFFFu) * _1_65535,
        ((packedUv >> 16u) & 0xFFFFu) * _1_65535
    );
}

void Unpack(uint packedColor, uint packedUv, uint packedUvMin, uint packedUvMax, out vec3 color, out vec2 uv, out vec2 uvMin, out vec2 uvMax){
    const float _1_255 = 1.0f / 255.0f;
    color = vec3(
        (packedColor & 0xFFu) * _1_255,
        ((packedColor >> 8u) & 0xFFu) * _1_255,
        ((packedColor >> 16u) & 0xFFu) * _1_255
    );

    UnpackUv(packedUv, uv);
    UnpackUv(packedUvMin, uvMin);
    UnpackUv(packedUvMax, uvMax);
}

vert_param(0, vec3 vs_Position)
vert_param(1, uint vs_PackedColor)
vert_param(2, uint vs_PackedUv)
vert_param(3, uint vs_PackedUvMin)
vert_param(4, uint vs_PackedUvMax)
vert_param(5, vec3 vs_Normal)
frag_param(0, vec3 fs_Color)
frag_param(1, vec2 fs_Uv)
frag_param(2, vec2 fs_UvMin)
frag_param(3, vec2 fs_UvMax)
frag_param(4, vec3 fs_Normal)
frag_param(5, vec3 fs_Position)
out_param(0, vec4 o_Color)
out_param(1, vec4 o_Normal)
out_param(2, vec4 o_Position)

#ifdef VERTEX

void vert(){
    Unpack(vs_PackedColor, vs_PackedUv, vs_PackedUvMin, vs_PackedUvMax, fs_Color, fs_Uv, fs_UvMin, fs_UvMax);
    fs_Normal = ModelNormal(vs_Normal);

    vec4 pos = ModelVertex(vs_Position);
    gl_Position = pos;
    fs_Position = vec3(inverse(ViewMatrix) * transpose(ModelMatrix) * vec4(vs_Position, 1));
}

#endif
#ifdef FRAGMENT

void frag(){
    vec2 uv = clamp(fs_Uv, fs_UvMin, fs_UvMax);
    vec4 sampledColor = texture(sampler2D(Texture, TextureSampler), uv);
    o_Color = vec4(sampledColor.rgb * (fs_Color * 0.25 + 0.75), sampledColor.a);
    o_Normal = vec4(fs_Normal, 1);
    o_Position = vec4(fs_Position, 1);
}

#endif

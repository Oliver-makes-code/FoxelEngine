#version 440

#include "foxel:common/camera.glsl"
#include "foxel:common/math.glsl"

layout (set = 1, binding = 0) uniform sampler TextureSampler;
layout (set = 1, binding = 1) uniform texture2D Texture;

layout (set = 2, binding = 0) uniform ModelTransform {
    vec4 Rotation;
};

vert_param(0, vec3 vs_Position)
vert_param(1, vec3 vs_Color)
vert_param(2, vec2 vs_Uv)
vert_param(3, vec2 vs_UvMin)
vert_param(4, vec2 vs_UvMax)
frag_param(0, vec3 fs_Color)
frag_param(1, vec2 fs_Uv)
frag_param(2, vec2 fs_UvMin)
frag_param(3, vec2 fs_UvMax)
out_param(0, vec4 o_Color)
out_param(1, vec4 o_Normal)

#ifdef VERTEX

void vert() {
    vec3 centeredPos = vs_Position - 0.5;
    centeredPos *= 2;

    vec4 pos = GetVP() * vec4(math_MulQuat(Rotation, centeredPos), 1);
    pos.z = (pos.z + 500) / 1000;
    gl_Position = pos;
    fs_Color = vs_Color;
    fs_Uv = vs_Uv;
    fs_UvMin = vs_UvMin;
    fs_UvMax = vs_UvMax;
}

#endif
#ifdef FRAGMENT

void frag() {
    vec2 uv = clamp(fs_Uv, fs_UvMin, fs_UvMax);
    vec4 sampledColor = texture(sampler2D(Texture, TextureSampler), uv);
    o_Color = vec4(sampledColor.rgb * (fs_Color * 0.25 + 0.75), sampledColor.a);
    o_Normal = vec4(1, 1, 1, 1);
}

#endif

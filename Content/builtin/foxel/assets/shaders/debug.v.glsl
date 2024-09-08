#version 440

#include "common/camera.glsl"

vert_param(0, vec3 vs_Position)
vert_param(1, vec4 vs_Color)
vert_param(2, vec2 vs_Uv)
frag_param(0, vec4 fs_Color)
out_param(0, vec4 o_Color)

#ifdef VERTEX

void vert(){
    vec4 pos = vec4(vs_Position, 1) * VPMatrix;
    gl_Position = pos;

    fs_Color = vs_Color;
}

#endif
#ifdef FRAGMENT

void frag(){
    o_Color = fs_Color;
}

#endif

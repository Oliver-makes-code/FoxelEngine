#version 440

#include "foxel:common/camera_and_model.glsl"

vert_param(0, vec3 vs_Model_Position)
vert_param(1, vec3 vs_Model_Normal)
vert_param(2, vec2 vs_Model_Uv)
vert_param(3, vec2 vs_Model_UvMin)
vert_param(4, vec2 vs_Model_UvMax)

vert_param(5, mat4 vs_Instance_Transform)

frag_param(0, vec2 fs_Uv)
frag_param(1, vec2 fs_UvMin)
frag_param(2, vec2 fs_UvMax)
frag_param(3, vec3 fs_Normal)
frag_param(4, vec4 fs_Position)

out_param(0, vec4 o_Color)
out_param(1, vec4 o_Normal)
out_param(2, vec4 o_Position)

#ifdef VERTEX

void vert() {
    vec4 pos = vec4(vs_Model_Position, 0);
    pos *= vs_Instance_Transform;

}

#endif
#ifdef FRAGMENT

void frag() {

}

#endif
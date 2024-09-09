#version 440

#include "deferred/common.glsl"

#ifdef FRAGMENT

void frag() {
    o_Color = vec4(1, 1, 1, 1);
}

#endif

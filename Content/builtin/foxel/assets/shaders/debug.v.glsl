#include "common/camera.glsl"

void vert(vec3 position, vec4 color, vec2 uv, out vec4 o_color){
    vec4 pos = vec4(position, 1) * VPMatrix;
    gl_Position = pos;

    o_color = color;
}

void frag(vec4 color, out vec4 o_color){
    o_color = color;
}

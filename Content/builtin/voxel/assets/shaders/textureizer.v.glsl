#include "voxel:common/camera.glsl"
#include "voxel:common/math.glsl"

void vert(vec3 position, vec3 color, vec2 uv, vec2 ao, vec2 uvMin, vec2 uvMax) {
    vec4 pos = VPMatrix * vec4(position, 1);
    pos.z = (pos.z + 4.5) / 9;
    gl_Position = pos;
}

void frag(out vec4 o_color) {
    o_color = vec4(1, 0, 0, 1);
}

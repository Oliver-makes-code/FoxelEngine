#include "voxel:common/camera.glsl"
#include "voxel:common/math.glsl"

void vert(vec3 vertexPos, vec3 quadPos, vec4 quadRotation, out float z) {
    gl_Position = VPMatrix * vec4(math_MulQuat(quadRotation, vertexPos + quadPos) + vec3(0, 0, -10), 1);
}

void frag(float z, out vec4 o_color) {
    o_color = vec4(1, 0, 0, 1);
}

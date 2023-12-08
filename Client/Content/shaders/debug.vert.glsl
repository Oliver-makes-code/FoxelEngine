#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in vec4 Color;
layout(location = 2) in vec2 UV;


layout(location = 1) out vec4 fsin_Color;

layout (set = 0, binding = 0) uniform CameraData {
    mat4 VPMatrix;
};

void main() {
    vec4 pos = vec4(Position, 1) * VPMatrix;
    gl_Position = pos;

    fsin_Color = Color;
}

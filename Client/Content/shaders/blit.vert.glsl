#version 450

layout(location = 0) in vec3 Position;
layout(location = 0) out vec2 fsin_texCoords;

layout (set = 1, binding = 0) uniform TextureDrawParams {
    bool flip;
};

void main() {
    gl_Position = vec4((Position.xy * 2) - 1, 0, 1);

    //Sample from source texture.

    vec2 uv = Position.xy;
    if (flip)
        uv = vec2(uv.x, 1 - uv.y);
    fsin_texCoords = uv;
}

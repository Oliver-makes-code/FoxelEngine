#version 450

layout(location = 0) in vec2 fsin_texCoords;
layout(location = 1) in vec4 fsin_Color;

layout(location = 0) out vec4 fsout_Color;

layout (set = 1, binding = 0) uniform sampler TextureSampler;
layout (set = 1, binding = 1) uniform texture2D Texture;

void main() {
    fsout_Color = texture(sampler2D(Texture, TextureSampler), fsin_texCoords) * fsin_Color;
}

layout (set = 0, binding = 0) uniform sampler TextureSampler;
layout (set = 0, binding = 1) uniform texture2D Texture;

void vert(vec2 position, vec2 uv, out vec2 o_uv) {
    gl_Position = vec4(position, 0, 1);
    o_uv = uv;
}
void frag(vec2 uv, out vec4 o_color) {
    o_color = texture(sampler2D(Texture, TextureSampler), uv);
}

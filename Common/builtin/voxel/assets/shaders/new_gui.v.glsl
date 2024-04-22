layout (set = 0, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
    vec2 InverseScreenSize;
};

layout (set = 0, binding = 1) uniform GuiData {
    int GuiScale;
};

layout (set = 1, binding = 0) uniform sampler TextureSampler;
layout (set = 1, binding = 1) uniform texture2D Texture;

void vert(vec2 i_position, vec2 anchor, ivec2 position, ivec2 size, vec4 color, vec2 uvMin, vec2 uvMax, out vec4 o_color, out vec2 o_uv) {
    vec2 pos = i_position;
    pos -= anchor;
    pos *= size * GuiScale;
    pos += anchor * ScreenSize;
    pos += position * GuiScale * 2;
    gl_Position = vec4(pos * InverseScreenSize, 0, 1);
    o_color = color;
    int yIdx = gl_VertexIndex >> 1;
    int xIdx = (gl_VertexIndex & 1) ^ yIdx;
    float[] x = { uvMin.x, uvMax.x };
    float[] y = { uvMin.y, uvMax.y };
    o_uv = vec2(x[xIdx], y[-yIdx+1]);
}

void frag(vec4 color, vec2 uv, out vec4 o_color) {
    o_color = color * texture(sampler2D(Texture, TextureSampler), uv);
}

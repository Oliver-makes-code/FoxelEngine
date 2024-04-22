layout (set = 0, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
    vec2 InverseScreenSize;
};

layout (set = 0, binding = 1) uniform GuiData {
    int GuiScale;
};

void vert(vec2 i_position, vec2 anchor, ivec2 position, ivec2 size, vec4 color, out vec4 o_color) {
    vec2 pos = i_position;
    pos -= anchor;
    pos *= size * GuiScale;
    pos += anchor * ScreenSize;
    pos += position * GuiScale * 2;
    gl_Position = vec4(pos * InverseScreenSize, 0, 1);
    o_color = color;
}

void frag(vec4 color, out vec4 o_color) {
    o_color = color;
}

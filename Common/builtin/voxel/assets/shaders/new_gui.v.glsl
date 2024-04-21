layout (set = 0, binding = 0) uniform ScreenData {
    vec4 ScreenSize;
};

void vert(vec2 i_position, vec2 anchor, ivec2 position, ivec2 size, vec4 color, out vec4 o_color) {
    vec2 pos = i_position;
    pos -= anchor;
    pos *= size;
    pos += anchor * ScreenSize.xy;
    pos += position;
    gl_Position = vec4(pos * ScreenSize.zw, 0, 1);
    o_color = color;
}

void frag(vec4 color, out vec4 o_color) {
    o_color = color;
}

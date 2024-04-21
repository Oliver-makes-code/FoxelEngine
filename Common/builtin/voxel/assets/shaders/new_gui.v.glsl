layout (set = 0, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
};

void vert(vec3 i_position, vec2 anchor, ivec2 position, ivec2 size, vec4 color) {
    vec2 pos = vec2(i_position.x, -i_position.y + ScreenSize.y);
    gl_Position = vec4((pos / ScreenSize * 2) - 1, 0, 1);
}

void frag(out vec4 o_color) {
    o_color = vec4(1, 0, 0, 1);
}

layout (set = 0, binding = 0) uniform sampler TextureSampler;
layout (set = 0, binding = 1) uniform texture2D Texture[4];

#define TEXTURE_COLOR 0
#define TEXTURE_NORMAL 1
#define TEXTURE_POSITION 2
#define TEXTURE_DEPTH 3

layout (set = 1, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
    vec2 InverseScreenSize;
};

vert_param(0, vec2 vs_Position)
frag_param(0, vec2 fs_Uv)
out_param(0, vec4 o_Color)

#define USER_LAYOUT(idx, bind) layout ( set = idx + 2, binding = bind )

#ifndef VERTEX_HANDLED
#ifdef VERTEX

void vert() {
    gl_Position = vec4((vs_Position.xy * 2) - 1, 0, 1);

    //Sample from source texture.

    fs_Uv = vs_Position.xy;
    fs_Uv.y *= -1;
    fs_Uv.y += 1;
}

#endif
#endif

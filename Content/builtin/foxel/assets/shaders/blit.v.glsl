#version 440

layout (set = 0, binding = 0) uniform sampler ColorTextureSampler;
layout (set = 0, binding = 1) uniform texture2D ColorTexture;
layout (set = 1, binding = 0) uniform sampler NormalTextureSampler;
layout (set = 1, binding = 1) uniform texture2D NormalTexture;
layout (set = 2, binding = 0) uniform sampler DepthTextureSampler;
layout (set = 2, binding = 1) uniform texture2D DepthTexture;
layout (set = 3, binding = 0) uniform TextureDrawParams {
    bool flip;
};

vert_param(0, vec2 vs_Position)
frag_param(0, vec2 fs_Uv)
out_param(0, vec4 o_Color)

#ifdef VERTEX

void vert() {
    gl_Position = vec4((vs_Position.xy * 2) - 1, 0, 1);

    //Sample from source texture.

    fs_Uv = vs_Position.xy;
    if (flip)
        fs_Uv = vec2(fs_Uv.x, 1 - fs_Uv.y);
}

#endif
#ifdef FRAGMENT

void frag() {
    o_Color = texture(sampler2D(ColorTexture, ColorTextureSampler), fs_Uv);
}

#endif

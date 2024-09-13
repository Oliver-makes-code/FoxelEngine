#version 440

layout (set = 0, binding = 0) uniform sampler TextureSampler;
layout (set = 0, binding = 1) uniform texture2D Texture;

layout (set = 1, binding = 0) uniform TextureDrawParams {
    vec2 SrcMin;
    vec2 SrcMax;
    vec2 DstMin;
    vec2 DstMax;
    vec2 SrcSize;
    vec2 DstSize;
};

vert_param(0, vec3 vs_Position)

frag_param(0, vec2 fs_Uv)

out_param(0, vec4 o_Color)

#ifdef VERTEX

void vert() {
    vec2 scaledDstMin = DstMin / DstSize;
    vec2 scaledDstMax = DstMax / DstSize;

    //Move vertex to correct place on texture.
    vec2 uv = (scaledDstMin + (scaledDstMax - scaledDstMin) * vs_Position.xy);
    uv.y = 1-uv.y;

    gl_Position = vec4((uv * 2) - 1, 0, 1);

    vec2 scaledSrcMin = SrcMin / SrcSize;
    vec2 scaledSrcMax = SrcMax / SrcSize;

    //Sample from source texture.
    fs_Uv = scaledSrcMin + (scaledSrcMax - scaledSrcMin) * vs_Position.xy;
}

#endif
#ifdef FRAGMENT

void frag() {
    o_Color = texture(sampler2D(Texture, TextureSampler), fs_Uv);
}

#endif

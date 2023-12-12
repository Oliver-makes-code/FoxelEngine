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


void vert(vec3 Position, out vec2 o_uv){
    vec2 scaledDstMin = DstMin / DstSize;
    vec2 scaledDstMax = DstMax / DstSize;

    //Move vertex to correct place on texture.
    vec2 uv = (scaledDstMin + (scaledDstMax - scaledDstMin) * Position.xy);
    uv.y = 1-uv.y;

    gl_Position = vec4((uv * 2) - 1, 0, 1);

    vec2 scaledSrcMin = SrcMin / SrcSize;
    vec2 scaledSrcMax = SrcMax / SrcSize;

    //Sample from source texture.
    o_uv = scaledSrcMin + (scaledSrcMax - scaledSrcMin) * Position.xy;
}

void frag(vec2 uv, out vec4 o_color){
    o_color = texture(sampler2D(Texture, TextureSampler), uv);
}

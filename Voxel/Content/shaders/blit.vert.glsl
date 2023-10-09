#version 450

layout(location = 0) in vec3 Position;
layout(location = 0) out vec2 fsin_texCoords;

layout (set = 1, binding = 0) uniform TextureDrawParams {
    vec2 SrcMin;
    vec2 SrcMax;
    vec2 DstMin;
    vec2 DstMax;
    vec2 SrcSize;
    vec2 DstSize;
};

void main()
{
    vec2 scaledDstMin = DstMin / DstSize;
    vec2 scaledDstMax = DstMax / DstSize;

    //Move vertex to correct place on texture.
    gl_Position = vec4((((scaledDstMin + (scaledDstMax - scaledDstMin) * Position.xy)) * 2) - 1, 0, 1);

    vec2 scaledSrcMin = SrcMin / SrcSize;
    vec2 scaledSrcMax = SrcMax / SrcSize;

    //Sample from source texture.
    fsin_texCoords = scaledSrcMin + (scaledSrcMax - scaledSrcMin) * Position.xy;
}

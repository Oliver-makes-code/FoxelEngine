layout (set = 0, binding = 0) uniform sampler ColorTextureSampler;
layout (set = 0, binding = 1) uniform texture2D ColorTexture;

layout (set = 1, binding = 0) uniform sampler NormalTextureSampler;
layout (set = 1, binding = 1) uniform texture2D NormalTexture;

layout (set = 2, binding = 0) uniform sampler PositionTextureSampler;
layout (set = 2, binding = 1) uniform texture2D PositionTexture;

layout (set = 3, binding = 0) uniform sampler DepthTextureSampler;
layout (set = 3, binding = 1) uniform texture2D DepthTexture;

layout (set = 4, binding = 0) uniform ScreenData {
    vec2 ScreenSize;
    vec2 InverseScreenSize;
};

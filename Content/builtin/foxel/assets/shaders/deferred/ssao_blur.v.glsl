#version 440

#include "deferred/common.glsl"

USER_LAYOUT(0, 0) uniform sampler SsaoTextureSampler;
USER_LAYOUT(0, 1) uniform texture2D SsaoTexture;

#ifdef FRAGMENT

void frag() {
    ivec2 ssaoSize = textureSize(sampler2D(SsaoTexture, SsaoTextureSampler), 0);
    vec2 inverseSsaoSize = 1 / vec2(ssaoSize);

    float value = 0;

    vec2 scaled = fs_Uv * ssaoSize;
    for (int x = -2; x < 3; x++) {
        for (int y = -2; y < 3; y++) {
            vec2 uv = (scaled + vec2(x, y)) * inverseSsaoSize;
            value += texture(sampler2D(SsaoTexture, SsaoTextureSampler), uv).r;
        }
    }

    o_Color.r = value / 25;
}

#endif

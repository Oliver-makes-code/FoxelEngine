#version 440

#include "deferred/common.glsl"

#define SAMPLE_COUNT 64
#define SAMPLE_RADIUS 0.5
#define SAMPLE_BIAS 0.025

USER_LAYOUT(0, 0) uniform sampler OffsetTextureSampler;
USER_LAYOUT(0, 1) uniform texture2D OffsetTexture;

USER_LAYOUT(1, 0) uniform SsaoSamples {
    vec4 Samples[SAMPLE_COUNT];
};

#ifdef FRAGMENT

void frag() {
    // Get position and normal
    vec3 pos = gSample(TEXTURE_POSITION, fs_Uv).xyz;
    vec3 normal = gSample(TEXTURE_NORMAL, fs_Uv).xyz;
    vec4 color = gSample(TEXTURE_COLOR, fs_Uv);
    if (color.a <= 0) {
        o_Color = vec4(1, 1, 1, 1);
        return;
    }

    // Get texture sizes
    ivec2 posSize = gSize(TEXTURE_POSITION);
    ivec2 noiseSize = textureSize(sampler2D(OffsetTexture, OffsetTextureSampler), 0);
    const vec2 offsetPos = vec2(float(posSize.x)/float(noiseSize.x), float(posSize.y)/(noiseSize.y)) * fs_Uv;

    // Get random offset
    vec3 offset = texture(sampler2D(OffsetTexture, OffsetTextureSampler), offsetPos).rgb;
    // vec3 offset = vec3(0, 1, 0);

    // Get TBN
    vec3 tangent = normalize(offset - normal * dot(offset, normal));
	vec3 bitangent = cross(tangent, normal);
	mat3 tbn = mat3(tangent, bitangent, normal);

    float occlusion = 0;
    for (int i = 0; i < SAMPLE_COUNT; i++) {
        vec3 samplePos = tbn * Samples[i].xyz;
        samplePos = pos + samplePos * SAMPLE_RADIUS;

        vec4 sampled = vec4(samplePos, 1);
        sampled = ProjectMatrix * sampled;
        sampled.xyz /= sampled.w;
		sampled.xyz = sampled.xyz * 0.5 + 0.5;

        vec2 clamped = clamp(offset.xy, vec2(0, 0), vec2(1, 1));

        float sampleDepth = gSample(TEXTURE_POSITION, clamped).z;
        float sampleAlpha = gSample(TEXTURE_COLOR, clamped).a;
        if (sampleAlpha <= 0) {
            sampleDepth = 0;
        }
        float rangeCheck = smoothstep(0.0, 1.0, SAMPLE_RADIUS / abs(sampled.z - samplePos.z - SAMPLE_BIAS));
        occlusion += (sampleDepth >= pos.z + SAMPLE_BIAS ? 1 : 0) * 1;
    }

    o_Color.r = (1 - (occlusion / float(SAMPLE_COUNT))) * 0.25 + 0.75;
}

#endif

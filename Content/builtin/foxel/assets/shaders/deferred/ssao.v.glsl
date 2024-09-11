#version 440

#include "deferred/common.glsl"

#define SAMPLE_COUNT 64
#define SAMPLE_RADIUS 0.5
#define SAMPLE_BIAS 0.025
#define FALLOFF_DISTANCE 64

USER_LAYOUT(0, 0) uniform sampler OffsetTextureSampler;
USER_LAYOUT(0, 1) uniform texture2D OffsetTexture2D;

#define OffsetTexture sampler2D(OffsetTexture2D, OffsetTextureSampler)

USER_LAYOUT(1, 0) uniform SsaoSamples {
    vec4 Samples[SAMPLE_COUNT];
};

#ifdef FRAGMENT

void frag() {
    vec2 fragUv = clamp(fs_Uv, 0, 1);
    vec3 fragPos = gSample(TEXTURE_POSITION, fragUv).xyz;
    float fallOff = clamp((fragPos.z + FALLOFF_DISTANCE) / FALLOFF_DISTANCE, 0, 1);
    vec3 normal = normalize(gSample(TEXTURE_NORMAL, fragUv).xyz);

    ivec2 texDim = gSize(TEXTURE_DEPTH); 
    ivec2 noiseDim = textureSize(OffsetTexture, 0);
    const vec2 noiseUv = vec2(float(texDim.x)/float(noiseDim.x), float(texDim.y)/(noiseDim.y)) * fragUv;  
    vec3 randomVec = texture(OffsetTexture, noiseUv).xyz;

    vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN       = mat3(tangent, bitangent, normal);

    float occlusion = 0;
    
    for (int i = 0; i < SAMPLE_COUNT; i++) {
        vec3 samplePos = TBN * Samples[i].xyz;
        samplePos = fragPos + samplePos * SAMPLE_RADIUS;

        vec4 offset = vec4(samplePos, 1.0) * ProjectMatrix;
        offset.xyz /= offset.w;
        offset.xy  = offset.xy * 0.5 + 0.5;
        offset.y *= -1;
        offset.y += 1;

        if (offset.x > 1 || offset.x < 0 || offset.y > 1 || offset.x < 0)
            continue;

        float sampleDepth = gSample(TEXTURE_POSITION, offset.xy).z;

        if (abs(samplePos.z - sampleDepth - SAMPLE_BIAS) > SAMPLE_RADIUS)
            continue;

        // float rangeCheck = smoothstep(0.0, 1.0, SAMPLE_RADIUS / abs(samplePos.z - sampleDepth - SAMPLE_BIAS));
        occlusion += (sampleDepth >= samplePos.z + SAMPLE_BIAS ? 1.0 : 0.0) * 1;
    }

    occlusion = 1.0 - (occlusion / SAMPLE_COUNT * fallOff);

    o_Color.r = occlusion;
}

#endif

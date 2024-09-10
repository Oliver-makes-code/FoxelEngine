#version 440

#include "deferred/common.glsl"

#define SIGMA 10.0
#define BSIGMA 1
#define MSIZE 8

USER_LAYOUT(0, 0) uniform sampler SsaoTextureSampler;
USER_LAYOUT(0, 1) uniform texture2D SsaoTexture2D;

#define SsaoTexture sampler2D(SsaoTexture2D, SsaoTextureSampler)

#ifdef FRAGMENT

float normpdf(in float x, in float sigma) {
    return 0.39894*exp(-0.5*x*x/(sigma*sigma))/sigma;
}

void frag() {
    vec2 size = textureSize(SsaoTexture, 0);
    vec2 invSize = 1 / size;
    vec2 uv = fs_Uv * size;
    float c = texture(SsaoTexture, fs_Uv).r;
    //declare stuff
    const int kSize = (MSIZE-1)/2;
    float kernel[MSIZE];
    float final_colour = 0;
    
    //create the 1-D kernel
    float Z = 0.0;
    for (int j = 0; j <= kSize; ++j) {
        kernel[kSize+j] = kernel[kSize-j] = normpdf(j, SIGMA);
    }
    
    float cc;
    float factor;
    float bZ = 1.0/normpdf(0.0, BSIGMA);
    //read out the texels
    for (int i = -kSize; i <= kSize; ++i) {
        for (int j = -kSize; j <= kSize; ++j) {
            cc = texture(SsaoTexture, (uv + vec2(i, j)) * invSize).r;
            factor = normpdf(cc-c, BSIGMA)*bZ*kernel[kSize+j]*kernel[kSize+i];
            Z += factor;
            final_colour += factor*cc;

        }
    }
    
    o_Color.r = final_colour / Z;
}

#endif

// Modified version of https://www.shadertoy.com/view/ttcyRS
vec3 colorBlendUniform(vec3 colA, vec3 colB, float h) {
    // https://bottosson.github.io/posts/oklab
    const mat3 kCONEtoLMS = mat3(                
        0.4121656120, 0.2118591070, 0.0883097947,
        0.5362752080, 0.6807189584, 0.2818474174,
        0.0514575653, 0.1074065790, 0.6302613616
    );
    const mat3 kLMStoCONE = mat3(
        4.0767245293, -1.2681437731, -0.0041119885,
        -3.3072168827, 2.6093323231, -0.7034763098,
        0.2307590544, -0.3411344290, 1.7068625689
    );
                    
    // rgb to cone (arg of pow can't be negative)
    const vec3 _1_3 = vec3(1.0f / 3.0f);
    vec3 lmsA = pow(kCONEtoLMS*colA, _1_3);
    vec3 lmsB = pow(kCONEtoLMS*colB, _1_3);
    // lerp
    vec3 lms = mix(lmsA, lmsB, h);
    // gain in the middle (no oaklab anymore, but looks better?)
    lms *= 1.0 + 0.2 * h * (1.0 - h);
    // cone to rgb
    return kLMStoCONE * (lms * lms * lms);
}

float weightedRatio(float n, float m) {
    return 0.5 + 0.5 * (1 - min(n,m) / max(max(n,m), 1.175494e-38)) * sign(m-n);
}

vec4 colorBlendWeightedAverage(vec4[4] colorPoints) {
    float alphaAvg = (
        colorPoints[0].a + colorPoints[1].a +
        colorPoints[2].a + colorPoints[3].a
    );
    return vec4((
        colorPoints[0].rgb * colorPoints[0].a +
        colorPoints[1].rgb * colorPoints[1].a +
        colorPoints[2].rgb * colorPoints[2].a +
        colorPoints[3].rgb * colorPoints[3].a
    ) / alphaAvg, alphaAvg);
}

vec4 colorBlendWeightedUniform(vec4[4] colorPoints) {
    float alphaAvg = (
        colorPoints[0].a + colorPoints[1].a +
        colorPoints[2].a + colorPoints[3].a
    ) * 0.25;

    float aMix = weightedRatio(colorPoints[0].a, colorPoints[1].a);
    vec3 a = colorBlendUniform(colorPoints[0].rgb, colorPoints[1].rgb, aMix);
    float bMix = weightedRatio(colorPoints[2].a, colorPoints[3].a);
    vec3 b = colorBlendUniform(colorPoints[2].rgb, colorPoints[3].rgb, bMix);

    float cMix = weightedRatio(aMix, bMix);
    // Multiply the output by 0.75 because it ends up a bit overly bright.
    return vec4(colorBlendUniform(a, b, cMix) * 0.75, alphaAvg);
}

vec4 colorBlendAverage(vec4[4] colorPoints) {
    return (
        colorPoints[0] +
        colorPoints[1] +
        colorPoints[2] +
        colorPoints[3]
    ) * 0.25;
}

vec4 colorBlendUniform(vec4[4] colorPoints) {
    vec3 a = colorBlendUniform(colorPoints[0].rgb, colorPoints[1].rgb, 0.5);
    vec3 b = colorBlendUniform(colorPoints[2].rgb, colorPoints[3].rgb, 0.5);

    // Multiply the output by 0.75 because it ends up a bit overly bright.
    return vec4(colorBlendUniform(a, b, 0.5) * 0.75, 1);
}

#ifdef FRAGMENT

#extension GL_ARB_derivative_control : require

vec4[4] interpolatePixels(vec2 uv, vec2 uvMin, vec2 uvMax, texture2D tex, sampler sam) {
    // Get the size of the texture
    vec2 inverseTexSize = textureSize(sampler2D(tex, sam), 0);
    vec2 texSize = 1 / inverseTexSize;

    vec2 oldUv = uv;

    // Get the size of the current pixel on the texture
    vec2 boxSize = (abs(dFdxCoarse(oldUv)) + abs(dFdyCoarse(oldUv))) * inverseTexSize * 0.8;

    // Get the functional center for interpolation
    vec2 tx = oldUv * inverseTexSize - 0.5 * boxSize;
    vec2 tfract = fract(tx);

    // Get the offset for the interpolation
    vec2 txOffset = smoothstep(1 - boxSize, vec2(1), tfract);

    // Get the minimum and maximum coordinates for sampling
    vec2 tmin = uvMin + 0.5 * texSize;
    vec2 tmax = uvMax - 0.5 * texSize;

    // Clamp the texture coordinates and rescale it to the texture
    vec2 newUvMin = clamp((tx - tfract + 0.5) * texSize, tmin, tmax);
    vec2 newUvMax = clamp((tx - tfract + 0.5 + txOffset) * texSize, tmin, tmax);

    // Sample the colors
    vec4[4] sampledColors = {
        texture(sampler2D(tex, sam), newUvMin),
        texture(sampler2D(tex, sam), newUvMax),
        texture(sampler2D(tex, sam), vec2(newUvMin.x, newUvMax.y)),
        texture(sampler2D(tex, sam), vec2(newUvMax.x, newUvMin.y))
    };
    return sampledColors;
}

#endif

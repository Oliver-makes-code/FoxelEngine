#AREA FRAGMENT

vec4 colorWeightedAverage(vec4[4] colorPoints) {
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

vec4 colorNormalAverage(vec4[4] colorPoints) {
    return (
        colorPoints[0] +
        colorPoints[1] +
        colorPoints[2] +
        colorPoints[3]
    ) / 4;
}

vec4[4] interpolatePixels(vec2 uv, vec2 uvMin, vec2 uvMax, texture2D tex, sampler sam) {
    vec2 inverseTexSize = textureSize(sampler2D(tex, sam), 0);
    vec2 texSize = 1 / inverseTexSize;

    vec2 oldUv = uv;

    vec2 boxSize = (abs(dFdx(oldUv)) + abs(dFdy(oldUv))) * inverseTexSize * 0.8;

    vec2 tx = oldUv * inverseTexSize - 0.5 * boxSize;
    vec2 tfract = fract(tx);
    vec2 txOffset = smoothstep(1 - boxSize, vec2(1), tfract);

    vec2 tmin = uvMin + 0.5 * texSize;
    vec2 tmax = uvMax - 0.5 * texSize;

    vec2 newUvMin = clamp((tx - tfract + 0.5) * texSize, tmin, tmax);
    vec2 newUvMax = clamp((tx - tfract + 0.5 + txOffset) * texSize, tmin, tmax);

    vec4[4] sampledColors = {
        texture(sampler2D(tex, sam), newUvMin),
        texture(sampler2D(tex, sam), newUvMax),
        texture(sampler2D(tex, sam), vec2(newUvMin.x, newUvMax.y)),
        texture(sampler2D(tex, sam), vec2(newUvMax.x, newUvMin.y))
    };
    return sampledColors;
}

#END

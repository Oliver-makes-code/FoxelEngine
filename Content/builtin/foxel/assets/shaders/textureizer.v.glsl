#include "foxel:common/camera.glsl"
#include "foxel:common/filtering.glsl"
#include "foxel:common/math.glsl"

layout (set = 1, binding = 0) uniform sampler TextureSampler;
layout (set = 1, binding = 1) uniform texture2D Texture;

layout (set = 2, binding = 0) uniform ModelTransform {
    vec4 Rotation;
};

void vert(vec3 position, vec3 color, vec2 uv, vec2 ao, vec2 uvMin, vec2 uvMax, out vec3 o_color, out vec2 o_uv, out vec2 o_uvMin, out vec2 o_uvMax) {
    vec3 centeredPos = position - 0.5;
    centeredPos *= 2;

    vec4 pos = VPMatrix * vec4(math_MulQuat(Rotation, centeredPos), 1);
    pos.z = (pos.z + 500) / 1000;
    gl_Position = pos;
    o_color = color;
    o_uv = uv;
    o_uvMin = uvMin;
    o_uvMax = uvMax;
}

void frag(vec3 color, vec2 uv, vec2 uvMin, vec2 uvMax, out vec4 o_color) {
    vec4 sampledColor = colorBlendAverage(interpolatePixels(uv, uvMin, uvMax, Texture, TextureSampler));
    o_color = vec4(colorBlendUniform(sampledColor.rgb, sampledColor.rgb * color, 0.25), sampledColor.a);
}

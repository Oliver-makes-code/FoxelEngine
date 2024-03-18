#include "common/camera_and_model.glsl"
#include "common/filtering.glsl"

layout (set = 2, binding = 0) uniform sampler TextureSampler;
layout (set = 2, binding = 1) uniform texture2D Texture;

void UnpackUv(int packedUv, out vec2 uv) {
    uv = vec2(
        (packedUv & 65535) / 65535.0f,
        ((packedUv >> 16) & 65535) / 65535.0f
    );
}

void Unpack(int packedColor, int packedUv, int packedUvMin, int packedUvMax, out vec4 color, out vec2 uv, out vec2 uvMin, out vec2 uvMax){
    color = vec4(
        (packedColor & 255) / 255.0f,
        ((packedColor >> 8) & 255) / 255.0f,
        ((packedColor >> 16) & 255) / 255.0f,
        ((packedColor >> 24) & 255) / 255.0f
    );

    UnpackUv(packedUv, uv);
    UnpackUv(packedUvMin, uvMin);
    UnpackUv(packedUvMax, uvMax);
}

float singleInterp(float strength, float baseColor) {
    float min = 0.95 * baseColor * baseColor;
    return min + (baseColor - min) * strength;
}

vec3 getColorMultiplier(float strength, vec3 baseColor) {
    return vec3(
        singleInterp(strength, baseColor.x),
        singleInterp(strength, baseColor.y),
        singleInterp(strength, baseColor.z)
    );
}

void vert(vec3 position, int packedColor, int packedUv, int packedUvMin, int packedUvMax, out vec3 o_color, out vec2 o_uv, out float o_distance, out vec2 o_uvMin, out vec2 o_uvMax){
    vec4 color;
    Unpack(packedColor, packedUv, packedUvMin, packedUvMax, color, o_uv, o_uvMin, o_uvMax);
    o_color = colorBlendUniform(color.rgb, vec3(0), color.a);

    vec4 pos = ModelVertex(position);
    gl_Position = pos;
    o_distance = pos.z;
}

void frag(vec3 color, vec2 uv, float distance, vec2 uvMin, vec2 uvMax, out vec4 o_color){
    vec4 sampledColor = colorBlendAverage(interpolatePixels(uv, uvMin, uvMax, Texture, TextureSampler));
    o_color = vec4(colorBlendUniform(sampledColor.rgb, sampledColor.rgb * color, 0.15), sampledColor.a);
    //o_gbuffer = vec4(1, 1, 0, 1);
}

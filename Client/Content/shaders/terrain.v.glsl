#include "common/camera_and_model.glsl"
#include "common/filtering.glsl"

layout (set = 2, binding = 0) uniform sampler TextureSampler;
layout (set = 2, binding = 1) uniform texture2D Texture;

void Unpack(int packedColor, int packedUV, out vec4 color, out vec2 uv){
    color = vec4(
    (packedColor & 255) / 255.0f,
    ((packedColor >> 8) & 255) / 255.0f,
    ((packedColor >> 16) & 255) / 255.0f,
    ((packedColor >> 24) & 255) / 255.0f
    );

    uv = vec2(
    (packedUV & 65535) / 65535.0f,
    ((packedUV >> 16) & 65535) / 65535.0f
    );
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

void vert(vec3 position, int packedColor, int packedUV, float ao, vec2 uvMin, vec2 uvMax, out vec4 o_color, out vec2 o_uv, out float o_distance, out vec2 o_uvMin, out vec2 o_uvMax){
    Unpack(packedColor, packedUV, o_color, o_uv);
    o_color = o_color * vec4(getColorMultiplier(1-ao, vec3(0.95, 0.95, 0.975)), 1);

    vec4 pos = ModelVertex(position);
    gl_Position = pos;
    o_distance = pos.z;

    o_uvMin = uvMin;
    o_uvMax = uvMax;
}

void frag(vec4 color, vec2 uv, float distance, vec2 uvMin, vec2 uvMax, out vec4 o_color){
    o_color = colorNormalAverage(interpolatePixels(uv, uvMin, uvMax, Texture, TextureSampler)) * color;
    //o_gbuffer = vec4(1, 1, 0, 1);
}

#include "common/camera_and_model.glsl"
#include "common/filtering.glsl"

layout (set = 2, binding = 0) uniform sampler TextureSampler;
layout (set = 2, binding = 1) uniform texture2D Texture;

void UnpackUv(uint packedUv, out vec2 uv) {
    const float _1_65535 = 1 / 65535.0f;
    uv = vec2(
        (packedUv & 0xFFFFu) * _1_65535,
        ((packedUv >> 16u) & 0xFFFFu) * _1_65535
    );
}

void Unpack(uint packedColorAndAo, uint packedUv, uint packedUvMin, uint packedUvMax, out vec4 colorAndAo, out vec2 uv, out vec2 uvMin, out vec2 uvMax){
    const float _1_255 = 1.0f / 255.0f;
    colorAndAo = vec4(
        (packedColorAndAo & 0xFFu) * _1_255,
        ((packedColorAndAo >> 8u) & 0xFFu) * _1_255,
        ((packedColorAndAo >> 16u) & 0xFFu) * _1_255,
        ((packedColorAndAo >> 24u) & 0xFFu) * _1_255
    );

    UnpackUv(packedUv, uv);
    UnpackUv(packedUvMin, uvMin);
    UnpackUv(packedUvMax, uvMax);
}

void vert(vec3 position, uint packedColorAndAo, uint packedUv, uint packedUvMin, uint packedUvMax, out vec3 o_color, out vec2 o_uv, out vec2 o_uvMin, out vec2 o_uvMax){
    vec4 colorAndAo;
    Unpack(packedColorAndAo, packedUv, packedUvMin, packedUvMax, colorAndAo, o_uv, o_uvMin, o_uvMax);
    o_color = colorBlendUniform(colorAndAo.rgb, vec3(0), colorAndAo.a);

    vec4 pos = ModelVertex(position);
    gl_Position = pos;
}

void frag(vec3 color, vec2 uv, vec2 uvMin, vec2 uvMax, out vec4 o_color){
    vec4 sampledColor = colorBlendAverage(interpolatePixels(uv, uvMin, uvMax, Texture, TextureSampler));
    o_color = vec4(colorBlendUniform(sampledColor.rgb, sampledColor.rgb * color, 0.15), sampledColor.a);
}

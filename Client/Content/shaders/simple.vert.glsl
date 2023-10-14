#version 450

layout(location = 0) in vec3 Position;
layout(location = 1) in int PackedColor;
layout(location = 2) in int PackedUV;
layout(location = 3) in float AmbientOcclusion;

layout(location = 0) out vec2 fsin_texCoords;
layout(location = 1) out vec4 fsin_Color;

struct UnpackedVertex{
    vec4 color;
    vec2 uv;
};

layout (set = 0, binding = 0) uniform CameraData {
    mat4 VPMatrix;
};

layout (set = 2, binding = 0) uniform ModelData {
    mat4 ModelMatrix;
};

UnpackedVertex unpack(int packedColor, int packedUV) {
    UnpackedVertex ret;

    ret.color = vec4(
    (packedColor & 255) / 255.0f,
    ((packedColor >> 8) & 255) / 255.0f,
    ((packedColor >> 16) & 255) / 255.0f,
    ((packedColor >> 24) & 255) / 255.0f
    );

    ret.uv = vec2(
    (packedUV & 65535) / 65535.0f,
    ((packedUV >> 16) & 65535) / 65535.0f
    );

    return ret;
}

void main() {
    mat4 mvp = ModelMatrix * VPMatrix;
    gl_Position = vec4(Position, 1) * mvp;

    UnpackedVertex up = unpack(PackedColor, PackedUV);
    fsin_texCoords = up.uv;
    float ao = 1 - ((AmbientOcclusion / 3.0) * 0.5f);
    fsin_Color = up.color * ao;
}
